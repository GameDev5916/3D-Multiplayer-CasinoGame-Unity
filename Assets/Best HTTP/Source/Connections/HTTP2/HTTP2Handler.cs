#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using System;
using System.Collections.Generic;
using System.Threading;

using System.Collections.Concurrent;

using BestHTTP.Extensions;
using BestHTTP.Core;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.Connections.HTTP2
{
    public sealed class HTTP2Handler : IHTTPRequestHandler
    {
        public bool HasCustomRequestProcessor { get { return true; } }

        public KeepAliveHeader KeepAlive { get { return null; } }

        public bool CanProcessMultiple { get { return this.goAwaySentAt == DateTime.MaxValue; } }

        // Connection preface starts with the string PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n).
        private static readonly byte[] MAGIC = new byte[24] { 0x50, 0x52, 0x49, 0x20, 0x2a, 0x20, 0x48, 0x54, 0x54, 0x50, 0x2f, 0x32, 0x2e, 0x30, 0x0d, 0x0a, 0x0d, 0x0a, 0x53, 0x4d, 0x0d, 0x0a, 0x0d, 0x0a };
        public const UInt32 MaxValueFor31Bits = 0xFFFFFFFF >> 1;

        public double Latency { get; private set; }

        private DateTime lastPingSent = DateTime.MinValue;
        private TimeSpan pingFrequency = TimeSpan.FromSeconds(5);
        public static int RTTBufferCapacity = 5;
        private CircularBuffer<double> rtts = new CircularBuffer<double>(RTTBufferCapacity);

        private volatile bool isThreadsStarted;
        private volatile bool isRunning;

        private AutoResetEvent newFrameSignal = new AutoResetEvent(false);

        private ConcurrentQueue<HTTPRequest> requestQueue = new ConcurrentQueue<HTTPRequest>();

        private List<HTTP2Stream> clientInitiatedStreams = new List<HTTP2Stream>();
        private HPACKEncoder HPACKEncoder;

        private ConcurrentQueue<HTTP2FrameHeaderAndPayload> newFrames = new ConcurrentQueue<HTTP2FrameHeaderAndPayload>();

        private List<HTTP2FrameHeaderAndPayload> outgoingFrames = new List<HTTP2FrameHeaderAndPayload>();

        private HTTP2SettingsManager settings = new HTTP2SettingsManager();
        private UInt32 remoteWindow;
        private DateTime lastInteraction;
        private DateTime goAwaySentAt = DateTime.MaxValue;

        private HTTPConnection conn;
        private int threadExitCount;

        public HTTP2Handler(HTTPConnection conn)
        {
            this.conn = conn;
            this.isRunning = true;

            // Put the first request to the queue
            this.requestQueue.Enqueue(conn.CurrentRequest);
        }

        public void Process(HTTPRequest request)
        {
            HTTPManager.Logger.Information("HTTP2Handler", "Process request called");

            this.lastInteraction = DateTime.UtcNow;

            this.requestQueue.Enqueue(request);

            this.newFrameSignal.Set();
        }

        public void RunHandler()
        {
            HTTPManager.Logger.Information("HTTP2Handler", "Processing thread up and running!");

            Thread.CurrentThread.Name = "HTTP2 Process";

            PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ReadThread);

            try
            {
                bool atLeastOneStreamHasAFrameToSend = true;

                this.HPACKEncoder = new HPACKEncoder(this.settings);

                // https://httpwg.org/specs/rfc7540.html#InitialWindowSize
                // The connection flow-control window is also 65,535 octets.
                this.remoteWindow = this.settings.RemoteSettings[HTTP2Settings.INITIAL_WINDOW_SIZE];

                // we want to pack as many data as we can in one tcp segment, but setting the buffer's size too high
                //  we might keep data too long and send them in bursts instead of in a steady stream.
                // Keeping it too low might result in a full tcp segment and one with very low payload
                // Is it possible that one full tcp segment sized buffer would be the best, or multiple of it.
                // It would keep the network busy without any fragments. The ethernet layer has a maximum of 1500 bytes,
                // but there's two layers of 20 byte headers each, so as a theoretical maximum it's 1500-20-20 bytes.
                // On the other hand, if the buffer is small (1-2), that means that for larger data, we have to do a lot
                // of system calls, in that case a larger buffer might be better. Still, if we are not cpu bound,
                // a well saturated network might serve us better.
                using (WriteOnlyBufferedStream bufferedStream = new WriteOnlyBufferedStream(this.conn.connector.Stream, 1024 * 1024 /*1500 - 20 - 20*/))
                {
                    // The client connection preface starts with a sequence of 24 octets
                    bufferedStream.Write(MAGIC, 0, MAGIC.Length);

                    // This sequence MUST be followed by a SETTINGS frame (Section 6.5), which MAY be empty.
                    // The client sends the client connection preface immediately upon receipt of a
                    // 101 (Switching Protocols) response (indicating a successful upgrade)
                    // or as the first application data octets of a TLS connection

                    // Set streams' initial window size to its maximum.
                    this.settings.InitiatedMySettings[HTTP2Settings.INITIAL_WINDOW_SIZE] = HTTPManager.HTTP2Settings.InitialStreamWindowSize;
                    this.settings.InitiatedMySettings[HTTP2Settings.MAX_CONCURRENT_STREAMS] = HTTPManager.HTTP2Settings.MaxConcurrentStreams;
                    this.settings.SendChanges(this.outgoingFrames);

                    // The default window size for the whole connection is 65535 bytes,
                    // but we want to set it to the maximum possible value.
                    Int64 diff = HTTPManager.HTTP2Settings.InitialConnectionWindowSize - 65535;
                    if (diff > 0)
                        this.outgoingFrames.Add(HTTP2FrameHelper.CreateWindowUpdateFrame(0, (UInt32)diff));

                    while (this.isRunning)
                    {
                        if (!atLeastOneStreamHasAFrameToSend)
                        {
                            // buffered stream will call flush automatically if its internal buffer is full.
                            // But we have to make it sure that we flush remaining data before we go to sleep.
                            bufferedStream.Flush();

                            // Wait until we have to send the next ping, OR a new frame is received on the read thread.
                            //   Sent      Now  Sent+frequency
                            //-----|--------|----|------------
                            int wait = (int)((this.lastPingSent + this.pingFrequency) - DateTime.UtcNow).TotalMilliseconds;

                            if (wait >= 1)
                            {
                                if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                                    HTTPManager.Logger.Information("HTTP2Handler", $"Sleeping for {wait:N0}ms");
                                this.newFrameSignal.WaitOne(wait);
                            }
                        }

                        DateTime now = DateTime.UtcNow;

                        if (now - this.lastPingSent >= this.pingFrequency)
                        {
                            this.lastPingSent = now;

                            var frame = HTTP2FrameHelper.CreatePingFrame(HTTP2PingFlags.None);
                            BufferHelper.SetLong(frame.Payload, 0, now.Ticks);

                            this.outgoingFrames.Add(frame);
                        }

                        // Process received frames
                        HTTP2FrameHeaderAndPayload header;
                        while (this.newFrames.TryDequeue(out header))
                        {
                            if (header.StreamId > 0)
                            {
                                HTTP2Stream http2Stream = FindStreamById(header.StreamId);

                                // Add frame to the stream, so it can process it when its Process function is called
                                if (http2Stream != null)
                                {
                                    http2Stream.AddFrame(header, this.outgoingFrames);
                                }
                                else
                                {
                                    // Error? It's possible that we closed and removed the stream while the server was in the middle of sending frames
                                    //HTTPManager.Logger.Warning("HTTP2Handler", string.Format("No stream found for id: {0}! Can't deliver frame: {1}", header.StreamId, header));
                                }
                            }
                            else
                            {
                                switch (header.Type)
                                {
                                    case HTTP2FrameTypes.SETTINGS:
                                        this.settings.Process(header, this.outgoingFrames);
                                        break;

                                    case HTTP2FrameTypes.PING:
                                        var pingFrame = HTTP2FrameHelper.ReadPingFrame(header);

                                        // if it wasn't an ack for our ping, we have to send one
                                        if ((pingFrame.Flags & HTTP2PingFlags.ACK) == 0)
                                        {
                                            var frame = HTTP2FrameHelper.CreatePingFrame(HTTP2PingFlags.ACK);
                                            this.outgoingFrames.Add(frame);
                                        }
                                        break;

                                    case HTTP2FrameTypes.WINDOW_UPDATE:
                                        var windowUpdateFrame = HTTP2FrameHelper.ReadWindowUpdateFrame(header);
                                        this.remoteWindow += windowUpdateFrame.WindowSizeIncrement;
                                        break;

                                    case HTTP2FrameTypes.GOAWAY:
                                        // parse the frame, so we can print out detailed information
                                        HTTP2GoAwayFrame goAwayFrame = HTTP2FrameHelper.ReadGoAwayFrame(header);

                                        HTTPManager.Logger.Warning("HTTP2Handler", "Received GOAWAY frame: " + goAwayFrame.ToString());

                                        string msg =
                                            $"Server closing the connection! Error code: {goAwayFrame.Error} ({goAwayFrame.ErrorCode})";
                                        for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
                                            this.clientInitiatedStreams[i].Abort(msg);

                                        // set the running flag to false, so the thread can exit
                                        this.isRunning = false;

                                        this.conn.State = HTTPConnectionStates.Closed;
                                        break;

                                    case HTTP2FrameTypes.ALT_SVC:
                                        //HTTP2AltSVCFrame altSvcFrame = HTTP2FrameHelper.ReadAltSvcFrame(header);

                                        // Implement
                                        //HTTPManager.EnqueuePluginEvent(new PluginEventInfo(PluginEvents.AltSvcHeader, new AltSvcEventInfo(altSvcFrame.Origin, ))
                                        break;
                                }
                            }
                        }

                        UInt32 maxConcurrentStreams = Math.Min(HTTPManager.HTTP2Settings.MaxConcurrentStreams, this.settings.RemoteSettings[HTTP2Settings.MAX_CONCURRENT_STREAMS]);

                        // pre-test stream count to lock only when truly needed.
                        if (this.clientInitiatedStreams.Count < maxConcurrentStreams && this.isRunning)
                        {
                            // grab requests from queue
                            HTTPRequest request;
                            while (this.clientInitiatedStreams.Count < maxConcurrentStreams && this.requestQueue.TryDequeue(out request))
                            {
#if !BESTHTTP_DISABLE_CACHING
                                // If possible load the full response from cache.
                                if (Caching.HTTPCacheService.IsCachedEntityExpiresInTheFuture(request))
                                {
                                    PlatformSupport.Threading.ThreadedRunner.RunShortLiving<HTTP2Handler, HTTPRequest>((handler, req) =>
                                        {
                                            if (ConnectionHelper.TryLoadAllFromCache("HTTP2Handler", req))
                                                req.State = HTTPRequestStates.Finished;
                                            else
                                            {
                                                // If for some reason it couldn't load we place back the request to the queue.

                                                handler.requestQueue.Enqueue(req);
                                                handler.newFrameSignal.Set();
                                            }
                                        }, this, request);
                                }
                                else
#endif
                                {
                                    // create a new stream
                                    var newStream = new HTTP2Stream(this.settings, this.HPACKEncoder);

                                    // process the request
                                    newStream.Assign(request);

                                    this.clientInitiatedStreams.Add(newStream);
                                }
                            }
                        }

                        // send any settings changes
                        this.settings.SendChanges(this.outgoingFrames);

                        atLeastOneStreamHasAFrameToSend = false;

                        // process other streams
                        // Room for improvement Streams should be processed by their priority!
                        for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
                        {
                            var stream = this.clientInitiatedStreams[i];
                            stream.Process(this.outgoingFrames);

                            // remove closed, empty streams (not enough to check the closed flag, a closed stream still can contain frames to send)
                            if (stream.State == HTTP2StreamStates.Closed && !stream.HasFrameToSend)
                            {
                                this.clientInitiatedStreams.RemoveAt(i--);
                                stream.Removed();
                            }

                            atLeastOneStreamHasAFrameToSend |= stream.HasFrameToSend;

                            this.lastInteraction = DateTime.UtcNow;
                        }

                        // If we encounter a data frame that too large for the current remote window, we have to stop
                        // sending all data frames as we could send smaller data frames before the large ones.
                        // Room for improvement: An improvement would be here to stop data frame sending per-stream.
                        bool haltDataSending = false;

                        if (this.ShutdownType == ShutdownTypes.Running && now - this.lastInteraction >= HTTPManager.MaxConnectionIdleTime)
                        {
                            this.lastInteraction = DateTime.UtcNow;
                            HTTPManager.Logger.Information("HTTP2Handler", "Reached idle time, sending GoAway frame!");
                            this.outgoingFrames.Add(HTTP2FrameHelper.CreateGoAwayFrame(0, HTTP2ErrorCodes.NO_ERROR));
                            this.goAwaySentAt = DateTime.UtcNow;
                        }

                        // https://httpwg.org/specs/rfc7540.html#GOAWAY
                        // Endpoints SHOULD always send a GOAWAY frame before closing a connection so that the remote peer can know whether a stream has been partially processed or not.
                        if (this.ShutdownType == ShutdownTypes.Gentle)
                        {
                            HTTPManager.Logger.Information("HTTP2Handler", "Connection abort requested, sending GoAway frame!");

                            this.outgoingFrames.Clear();
                            this.outgoingFrames.Add(HTTP2FrameHelper.CreateGoAwayFrame(0, HTTP2ErrorCodes.NO_ERROR));
                            this.goAwaySentAt = DateTime.UtcNow;
                        }

                        if (this.isRunning && now - goAwaySentAt >= TimeSpan.FromMilliseconds(Math.Max(this.Latency * 2.5, 1500)))
                        {
                            HTTPManager.Logger.Information("HTTP2Handler", "No GoAway frame received back. Really quitting now!");
                            this.isRunning = false;
                            conn.State = HTTPConnectionStates.Closed;
                        }

                        uint streamWindowUpdates = 0;

                        // Go through all the collected frames and send them.
                        for (int i = 0; i < this.outgoingFrames.Count; ++i)
                        {
                            var frame = this.outgoingFrames[i];

                            if (HTTPManager.Logger.Level <= Logger.Loglevels.All && frame.Type != HTTP2FrameTypes.DATA /*&& frame.Type != HTTP2FrameTypes.PING*/)
                                HTTPManager.Logger.Information("HTTP2Handler", "Sending frame: " + frame.ToString());

                            // post process frames
                            switch (frame.Type)
                            {
                                case HTTP2FrameTypes.DATA:
                                    if (haltDataSending)
                                        continue;

                                    // if the tracked remoteWindow is smaller than the frame's payload, we stop sending
                                    // data frames until we receive window-update frames
                                    if (frame.PayloadLength > this.remoteWindow)
                                    {
                                        haltDataSending = true;
                                        HTTPManager.Logger.Warning("HTTP2Handler",
                                            $"Data sending halted for this round. Remote Window: {this.remoteWindow:N0}, frame: {frame.ToString()}");
                                        continue;
                                    }

                                    break;

                                case HTTP2FrameTypes.WINDOW_UPDATE:
                                    if (frame.StreamId > 0)
                                        streamWindowUpdates += BufferHelper.ReadUInt31(frame.Payload, 0);
                                    break;
                            }

                            this.outgoingFrames.RemoveAt(i--);

                            using (var buffer = HTTP2FrameHelper.HeaderAsBinary(frame))
                                bufferedStream.Write(buffer.Data, 0, buffer.Length);

                            if (frame.PayloadLength > 0)
                            {
                                bufferedStream.Write(frame.Payload, (int)frame.PayloadOffset, (int)frame.PayloadLength);

                                if (!frame.DontUseMemPool)
                                    BufferPool.Release(frame.Payload);
                            }

                            if (frame.Type == HTTP2FrameTypes.DATA)
                                this.remoteWindow -= frame.PayloadLength;
                        }

                        if (streamWindowUpdates > 0)
                        {
                            var frame = HTTP2FrameHelper.CreateWindowUpdateFrame(0, streamWindowUpdates);

                            if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                                HTTPManager.Logger.Information("HTTP2Handler", "Sending frame: " + frame.ToString());

                            using (var buffer = HTTP2FrameHelper.HeaderAsBinary(frame))
                                bufferedStream.Write(buffer.Data, 0, buffer.Length);

                            bufferedStream.Write(frame.Payload, (int)frame.PayloadOffset, (int)frame.PayloadLength);
                        }
                    } // while (this.isRunning)

                    bufferedStream.Flush();
                }
            }
            catch (Exception ex)
            {
                // Log out the exception if it's a non-expected one.
                if (this.ShutdownType == ShutdownTypes.Running && this.goAwaySentAt == DateTime.MaxValue)
                    HTTPManager.Logger.Exception("HTTP2Handler", "Sender thread", ex);
            }
            finally
            {
                if (this.ShutdownType == ShutdownTypes.Running)
                {
                    // Room for improvement: Do something with the unfinished requests...
                }
                else
                {
                    this.requestQueue.Clear();
                }

                HTTPManager.Logger.Information("HTTP2Handler", "Sender thread closing");
            }

            if (this.conn.connector.Stream != null)
            {
                try
                {
                    this.conn.connector.Stream.Dispose();
                    //this.conn.connector.Stream = null;
                }
                catch
                { }
            }

            if (Interlocked.Increment(ref this.threadExitCount) == 2 /*&& this.ShutdownType != ShutdownTypes.Immediate*/)
                ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this.conn, HTTPConnectionStates.Closed));
        }


        private void ReadThread()
        {
            try
            {
                Thread.CurrentThread.Name = "HTTP2 Read";
                HTTPManager.Logger.Information("HTTP2Handler", "Reader thread up and running!");

                using (ReadOnlyBufferedStream bufferedStream = new ReadOnlyBufferedStream(this.conn.connector.Stream, 32 * 1024))
                {
                    while (this.isRunning)
                    {
                        // TODO:
                        //  1. Set the local window to a reasonable size
                        //  2. stop reading when the local window is about to be 0.
                        //  3.
                        HTTP2FrameHeaderAndPayload header = HTTP2FrameHelper.ReadHeader(bufferedStream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information && header.Type != HTTP2FrameTypes.DATA /*&& header.Type != HTTP2FrameTypes.PING*/)
                            HTTPManager.Logger.Information("HTTP2Handler", "New frame received: " + header.ToString());

                        // Add the new frame to the queue. Processing it on the write thread gives us the advantage that
                        //  we don't have to deal with too much locking.
                        this.newFrames.Enqueue(header);

                        // ping write thread to process the new frame
                        this.newFrameSignal.Set();

                        switch (header.Type)
                        {
                            // Handle pongs on the read thread, so no additional latency is added to the rtt calculation.
                            case HTTP2FrameTypes.PING:
                                var pingFrame = HTTP2FrameHelper.ReadPingFrame(header);

                                if ((pingFrame.Flags & HTTP2PingFlags.ACK) != 0)
                                {
                                    // it was an ack, payload must contain what we sent

                                    var ticks = BufferHelper.ReadLong(pingFrame.OpaqueData, 0);

                                    // the difference between the current time and the time when the ping message is sent
                                    TimeSpan diff = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ticks);

                                    // add it to the buffer
                                    this.rtts.Add(diff.TotalMilliseconds);

                                    // and calculate the new latency
                                    this.Latency = CalculateLatency();

                                    HTTPManager.Logger.Verbose("HTTP2Handler",
                                        $"Latency: {this.Latency:F2}ms, RTT buffer: {this.rtts.ToString()}");
                                }
                                break;

                            case HTTP2FrameTypes.GOAWAY:
                                // Just exit from this thread. The processing thread will handle the frame too.
                                return;
                        }
                    }
                }
            }
            catch //(Exception ex)
            {
                //HTTPManager.Logger.Exception("HTTP2Handler", "", ex);

                this.isRunning = false;
                this.newFrameSignal.Set();
            }
            finally
            {
                HTTPManager.Logger.Information("HTTP2Handler", "Reader thread closing");
            }

            if (Interlocked.Increment(ref this.threadExitCount) == 2 /*&& this.ShutdownType != ShutdownTypes.Immediate*/)
                ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this.conn, HTTPConnectionStates.Closed));
        }

        private double CalculateLatency()
        {
            if (this.rtts.Count == 0)
                return 0;

            double sumLatency = 0;
            for (int i = 0; i < this.rtts.Count; ++i)
                sumLatency += this.rtts[i];

            return sumLatency / this.rtts.Count;
        }

        private HTTP2Stream FindStreamById(UInt32 streamId)
        {
            for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
            {
                var stream = this.clientInitiatedStreams[i];
                if (stream.Id == streamId)
                    return stream;
            }

            return null;
        }

        public ShutdownTypes ShutdownType { get; private set; }

        public void Shutdown(ShutdownTypes type)
        {
            this.ShutdownType = type;

            switch(this.ShutdownType)
            {
                case ShutdownTypes.Gentle:
                    this.newFrameSignal.Set();
                    break;

                case ShutdownTypes.Immediate:
                    this.conn.connector.Stream.Dispose();
                    break;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.newFrameSignal != null)
                this.newFrameSignal.Close();
            this.newFrameSignal = null;
        }

        ~HTTP2Handler()
        {
            Dispose(false);
        }
    }
}

#endif