#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using System;
using System.Collections.Generic;
using System.Threading;
using BestHTTP.Caching;
using BestHTTP.Core;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.Connections.HTTP2
{
    // https://httpwg.org/specs/rfc7540.html#StreamStates
    //
    //                                       Idle
    //                                        |
    //                                        V
    //                                      Open
    //                Receive END_STREAM  /  |   \  Send END_STREAM
    //                                   v   |R   V
    //                  Half Closed Remote   |S   Half Closed Locale
    //                                   \   |T  /
    //     Send END_STREAM | RST_STREAM   \  |  /    Receive END_STREAM | RST_STREAM
    //     Receive RST_STREAM              \ | /     Send RST_STREAM
    //                                       V
    //                                     Closed
    //
    // IDLE -> send headers -> OPEN -> send data -> HALF CLOSED - LOCAL -> receive headers -> receive Data -> CLOSED
    //               |                                     ^                      |                             ^
    //               +-------------------------------------+                      +-----------------------------+
    //                      END_STREAM flag present?                                   END_STREAM flag present?
    //

    internal enum HTTP2StreamStates
    {
        Idle,
        //ReservedLocale,
        //ReservedRemote,
        Open,
        HalfClosedLocal,
        HalfClosedRemote,
        Closed
    }

    internal sealed class HTTP2Stream
    {
        // https://httpwg.org/specs/rfc7540.html#StreamIdentifiers
        // Streams initiated by a client MUST use odd-numbered stream identifiers
        // With an initial value of -1, the first client initiated stream's id going to be 1.
        private static long LastStreamId = -1;

        public UInt32 Id { get; private set; }

        public HTTP2StreamStates State {
            get { return this._state; }

            private set {
                var oldState = this._state;

                this._state = value;

                if (oldState != this._state)
                    HTTPManager.Logger.Information("HTTP2Stream",
                        $"[{this.Id}] State changed from <color=yellow>{oldState}</color> to <color=green>{this._state}</color>");
            }
        }
        private HTTP2StreamStates _state;

        /// <summary>
        /// This flag is checked by the connection to decide whether to do a new processing-frame sending round before sleeping until new data arrives
        /// </summary>
        public bool HasFrameToSend
        {
            get
            {
                // Don't let the connection sleep until
                return this.outgoing.Count > 0 || // we already booked at least one frame in advance
                       (this.State == HTTP2StreamStates.Open && this.remoteWindow > 0); // we are in the middle of sending request data
            }
        }

        public HTTPRequest AssignedRequest { get; private set; }

        private bool isStreamedDownload;
        private uint downloaded;

        private byte[] bodyToSend;
        private UInt32 bodyToSendOffset;

        private HTTP2SettingsManager settings;
        private HPACKEncoder encoder;

        // Outgoing frames. The stream will send one frame per Process call, but because one step might be able to
        // generate more than one frames, we use a list.
        private Queue<HTTP2FrameHeaderAndPayload> outgoing = new Queue<HTTP2FrameHeaderAndPayload>();

        private Queue<HTTP2FrameHeaderAndPayload> incomingFrames = new Queue<HTTP2FrameHeaderAndPayload>();

        private FramesAsStreamView currentFramesView;

        private UInt32 localWindow;
        private Int64 remoteWindow;

        private uint windowUpdateThreshold;

        private UInt32 sentData;

        private bool isRSTFrameSent;

        private HTTP2Response response;

        /// <summary>
        /// Constructor to create a client stream.
        /// </summary>
        public HTTP2Stream(HTTP2SettingsManager registry, HPACKEncoder hpackEncoder)
            : this(registry, hpackEncoder, (UInt32)Interlocked.Add(ref LastStreamId, 2))
        { }

        public HTTP2Stream(HTTP2SettingsManager registry, HPACKEncoder hpackEncoder, UInt32 id)
        {
            this.settings = registry;
            this.encoder = hpackEncoder;
            this.Id = id;

            this.remoteWindow = this.settings.RemoteSettings[HTTP2Settings.INITIAL_WINDOW_SIZE];
            this.settings.RemoteSettings.OnSettingChangedEvent += OnRemoteSettingChanged;

            // Room for improvement: If INITIAL_WINDOW_SIZE is small (what we can consider a 'small' value?), threshold must be higher
            this.windowUpdateThreshold = (uint)(this.remoteWindow / 2);
        }

        public void Assign(HTTPRequest request)
        {
            HTTPManager.Logger.Information("HTTP2Stream",
                $"[{this.Id}] Request assigned to stream. Remote Window: {this.remoteWindow:N0}. Uri: {request.CurrentUri.ToString()}");
            this.AssignedRequest = request;
            this.isStreamedDownload = request.UseStreaming && request.OnStreamingData != null;
            this.downloaded = 0;
        }

        public void Process(List<HTTP2FrameHeaderAndPayload> outgoingFrames)
        {
            if (this.AssignedRequest.IsCancellationRequested && !this.isRSTFrameSent)
            {
                this.AssignedRequest.Response = null;
                this.AssignedRequest.State = HTTPRequestStates.Aborted;

                this.outgoing.Clear();
                if (this.State != HTTP2StreamStates.Idle)
                    this.outgoing.Enqueue(HTTP2FrameHelper.CreateRSTFrame(this.Id, HTTP2ErrorCodes.CANCEL));

                this.State = HTTP2StreamStates.Closed;

                this.isRSTFrameSent = true;
            }

            // 1.) Go through incoming frames
            ProcessIncomingFrames(outgoingFrames);

            // 2.) Create outgoing frames based on the stream's state and the request processing state.
            ProcessState(outgoingFrames);

            // 3.) Send one frame per Process call
            if (this.outgoing.Count > 0)
            {
                HTTP2FrameHeaderAndPayload frame = this.outgoing.Dequeue();

                outgoingFrames.Add(frame);

                // If END_Stream in header or data frame is present => half closed local
                if ((frame.Type == HTTP2FrameTypes.HEADERS && (frame.Flags & (byte)HTTP2HeadersFlags.END_STREAM) != 0) ||
                    (frame.Type == HTTP2FrameTypes.DATA && (frame.Flags & (byte)HTTP2DataFlags.END_STREAM) != 0))
                {
                    this.State = HTTP2StreamStates.HalfClosedLocal;
                }
            }
        }

        public void AddFrame(HTTP2FrameHeaderAndPayload frame, List<HTTP2FrameHeaderAndPayload> outgoingFrames)
        {
            // Room for improvement: error check for forbidden frames (like settings) and stream state

            this.incomingFrames.Enqueue(frame);

            ProcessIncomingFrames(outgoingFrames);
        }

        public void Abort(string msg)
        {
            this.AssignedRequest.Response = null;
            this.AssignedRequest.Exception = new Exception(msg);
            this.AssignedRequest.State = HTTPRequestStates.Error;

            this.State = HTTP2StreamStates.Closed;

            // After receiving a RST_STREAM on a stream, the receiver MUST NOT send additional frames for that stream, with the exception of PRIORITY.
            this.outgoing.Clear();
        }

        private void ProcessIncomingFrames(List<HTTP2FrameHeaderAndPayload> outgoingFrames)
        {
            UInt32 windowUpdate = 0;

            while (this.incomingFrames.Count > 0)
            {
                HTTP2FrameHeaderAndPayload frame = this.incomingFrames.Dequeue();

                if (this.isRSTFrameSent)
                {
                    BufferPool.Release(frame.Payload);
                    continue;
                }

                if (/*HTTPManager.Logger.Level == Logger.Loglevels.All && */frame.Type != HTTP2FrameTypes.DATA && frame.Type != HTTP2FrameTypes.WINDOW_UPDATE)
                    HTTPManager.Logger.Information("HTTP2Stream",
                        $"[{this.Id}] Process - processing frame: {frame.ToString()}");

                switch (frame.Type)
                {
                    case HTTP2FrameTypes.HEADERS:
                    case HTTP2FrameTypes.CONTINUATION:
                        if (this.State != HTTP2StreamStates.HalfClosedLocal)
                        {
                            // ERROR!
                            continue;
                        }

                        // payload will be released by the view
                        frame.DontUseMemPool = true;

                        if (this.currentFramesView == null)
                            this.currentFramesView = new FramesAsStreamView(new HeaderFrameView());

                        this.currentFramesView.AddFrame(frame);

                        if ((frame.Flags & (byte)HTTP2HeadersFlags.END_HEADERS) != 0)
                        {
                            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();

                            try
                            {
                                this.encoder.Decode(this, this.currentFramesView, headers);
                            }
                            catch(Exception ex)
                            {
                                HTTPManager.Logger.Exception("HTTP2Stream", $"[{this.Id}] ProcessIncomingFrames", ex);
                            }

                            this.response = new HTTP2Response(this.AssignedRequest, false);
                            this.response.AddHeaders(headers);

                            this.AssignedRequest.Response = this.response;

                            this.currentFramesView.Close();
                            this.currentFramesView = null;

                            if (frame.Type == HTTP2FrameTypes.HEADERS && (frame.Flags & (byte)HTTP2HeadersFlags.END_STREAM) != 0)
                            {
                                PlatformSupport.Threading.ThreadedRunner.RunShortLiving<HTTPRequest, HTTP2Response, FramesAsStreamView>(FinishRequest, this.AssignedRequest, this.response, this.currentFramesView);

                                if (this.State == HTTP2StreamStates.HalfClosedLocal)
                                    this.State = HTTP2StreamStates.Closed;
                                else
                                    this.State = HTTP2StreamStates.HalfClosedRemote;
                            }
                        }
                        break;

                    case HTTP2FrameTypes.DATA:
                        if (this.State != HTTP2StreamStates.HalfClosedLocal)
                        {
                            // ERROR!
                            continue;
                        }

                        this.downloaded += frame.PayloadLength;

                        if (this.isStreamedDownload && frame.Payload != null && frame.PayloadLength > 0)
                            this.response.ProcessData(frame.Payload, (int)frame.PayloadLength);

                        // frame's buffer will be released by the frames view
                        frame.DontUseMemPool = true;

                        if (this.currentFramesView == null && !this.isStreamedDownload)
                            this.currentFramesView = new FramesAsStreamView(new DataFrameView());

                        if (!this.isStreamedDownload)
                            this.currentFramesView.AddFrame(frame);

                        // Track received data, and if necessary(local window getting too low), send a window update frame
                        if (this.localWindow < frame.PayloadLength)
                        {
                            HTTPManager.Logger.Error("HTTP2Stream",
                                $"[{this.Id}] Frame's PayloadLength ({frame.PayloadLength:N0}) is larger then local window ({this.localWindow:N0}). Frame: {frame}");
                        }
                        else
                            this.localWindow -= frame.PayloadLength;

                        bool isFinalDataFrame = (frame.Flags & (byte)HTTP2DataFlags.END_STREAM) != 0;

                        // Window update logic.
                        //  1.) We could use a logic to only send window update(s) after a threshold is reached.
                        //      When the initial window size is high enough to contain the whole or most of the result,
                        //      sending back two window updates (connection and stream) after every data frame is pointless.
                        //  2.) On the other hand, window updates are cheap and works even when initial window size is low.
                        //          (
                        if (isFinalDataFrame || this.localWindow <= this.windowUpdateThreshold)
                            windowUpdate += this.settings.MySettings[HTTP2Settings.INITIAL_WINDOW_SIZE] - this.localWindow - windowUpdate;

                        if (isFinalDataFrame)
                        {
                            if (this.isStreamedDownload)
                                this.response.FinishProcessData();

                            HTTPManager.Logger.Information("HTTP2Stream",
                                $"[{this.Id}] All data arrived, data length: {this.downloaded:N0}");

                            // create a short living thread to process the downloaded data:
                            PlatformSupport.Threading.ThreadedRunner.RunShortLiving<HTTPRequest, HTTP2Response, FramesAsStreamView>(FinishRequest, this.AssignedRequest, this.response, this.currentFramesView);

                            this.currentFramesView = null;

                            if (this.State == HTTP2StreamStates.HalfClosedLocal)
                                this.State = HTTP2StreamStates.Closed;
                            else
                                this.State = HTTP2StreamStates.HalfClosedRemote;
                        }
                        else if (this.AssignedRequest.OnDownloadProgress != null)
                            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.AssignedRequest,
                                                                                 RequestEvents.DownloadProgress,
                                                                                 downloaded,
                                                                                 this.response.ExpectedContentLength));

                        break;

                    case HTTP2FrameTypes.WINDOW_UPDATE:
                        HTTP2WindowUpdateFrame windowUpdateFrame = HTTP2FrameHelper.ReadWindowUpdateFrame(frame);

                        if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                            HTTPManager.Logger.Information("HTTP2Stream",
                                $"[{this.Id}] Received Window Update: {windowUpdateFrame.WindowSizeIncrement:N0}, new remoteWindow: {this.remoteWindow + windowUpdateFrame.WindowSizeIncrement:N0}, initial remote window: {this.settings.RemoteSettings[HTTP2Settings.INITIAL_WINDOW_SIZE]:N0}, total data sent: {this.sentData:N0}");

                        this.remoteWindow += windowUpdateFrame.WindowSizeIncrement;
                        break;

                    case HTTP2FrameTypes.RST_STREAM:
                        // https://httpwg.org/specs/rfc7540.html#RST_STREAM

                        var rstStreamFrame = HTTP2FrameHelper.ReadRST_StreamFrame(frame);

                        HTTPManager.Logger.Error("HTTP2Stream",
                            $"[{this.Id}] RST Stream frame ({rstStreamFrame}) received in state {this.State}!");

                        Abort(
                            $"RST_STREAM frame received! Error code: {rstStreamFrame.Error.ToString()}({rstStreamFrame.ErrorCode})");
                        break;

                    default:
                        HTTPManager.Logger.Warning("HTTP2Stream",
                            $"[{this.Id}] Unexpected frame ({frame}) in state {this.State}!");
                        break;
                }

                if (!frame.DontUseMemPool)
                    BufferPool.Release(frame.Payload);
            }

            if (windowUpdate > 0)
            {
                if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                    HTTPManager.Logger.Information("HTTP2Stream",
                        $"[{this.Id}] Sending window update: {windowUpdate:N0}, current window: {this.localWindow:N0}, initial window size: {this.settings.MySettings[HTTP2Settings.INITIAL_WINDOW_SIZE]:N0}");

                this.localWindow += windowUpdate;

                outgoingFrames.Add(HTTP2FrameHelper.CreateWindowUpdateFrame(this.Id, windowUpdate));
            }
        }

        private void ProcessState(List<HTTP2FrameHeaderAndPayload> outgoingFrames)
        {
            switch (this.State)
            {
                case HTTP2StreamStates.Idle:

                    UInt32 initiatedInitialWindowSize = this.settings.InitiatedMySettings[HTTP2Settings.INITIAL_WINDOW_SIZE];
                    this.localWindow = initiatedInitialWindowSize;
                    // window update with a zero increment would be an error (https://httpwg.org/specs/rfc7540.html#WINDOW_UPDATE)
                    //if (HTTP2Connection.MaxValueFor31Bits > initiatedInitialWindowSize)
                    //    this.outgoing.Enqueue(HTTP2FrameHelper.CreateWindowUpdateFrame(this.Id, HTTP2Connection.MaxValueFor31Bits - initiatedInitialWindowSize));
                    //this.localWindow = HTTP2Connection.MaxValueFor31Bits;

#if !BESTHTTP_DISABLE_CACHING
                    // Setup cache control headers before we send out the request
                    if (!this.AssignedRequest.DisableCache)
                        HTTPCacheService.SetHeaders(this.AssignedRequest);
#endif

                    // hpack encode the request's header
                    this.encoder.Encode(this, this.AssignedRequest, this.outgoing, this.Id);

                    // HTTP/2 uses DATA frames to carry message payloads.
                    // The chunked transfer encoding defined in Section 4.1 of [RFC7230] MUST NOT be used in HTTP/2.
                    this.bodyToSend = this.AssignedRequest.GetEntityBody();

                    this.State = HTTP2StreamStates.Open;

                    if (this.bodyToSend == null || this.bodyToSend.Length == 0)
                        this.State = HTTP2StreamStates.HalfClosedLocal;
                    else
                        this.State = HTTP2StreamStates.Open;
                    break;

                case HTTP2StreamStates.Open:
                    // remote Window can be negative! See https://httpwg.org/specs/rfc7540.html#InitialWindowSize
                    if (this.remoteWindow <= 0)
                    {
                        HTTPManager.Logger.Warning("HTTP2Stream",
                            $"[{this.Id}] Skipping data sending as remote Window is {this.remoteWindow}!");
                        return;
                    }

                    // This step will send one frame per OpenState call.

                    Int64 maxFrameSize = Math.Min(this.remoteWindow, this.settings.RemoteSettings[HTTP2Settings.MAX_FRAME_SIZE]);

                    HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
                    frame.Type = HTTP2FrameTypes.DATA;
                    frame.StreamId = this.Id;

                    frame.Payload = this.bodyToSend;
                    frame.PayloadLength = (UInt32)Math.Min(maxFrameSize, this.bodyToSend.Length - this.bodyToSendOffset);
                    frame.PayloadOffset = this.bodyToSendOffset;
                    frame.DontUseMemPool = true;

                    this.bodyToSendOffset += frame.PayloadLength;

                    if (this.bodyToSendOffset >= this.bodyToSend.Length)
                    {
                        frame.Flags = (byte)(HTTP2DataFlags.END_STREAM);

                        this.State = HTTP2StreamStates.HalfClosedLocal;
                    }

                    this.outgoing.Enqueue(frame);

                    this.remoteWindow -= frame.PayloadLength;

                    this.sentData += frame.PayloadLength;

                    //HTTPManager.Logger.Information("HTTP2Stream", string.Format("[{0}] New DATA frame created! remoteWindow: {1:N0}", this.Id, this.remoteWindow));
                    break;

                case HTTP2StreamStates.HalfClosedLocal:
                    break;

                case HTTP2StreamStates.HalfClosedRemote:
                    break;

                case HTTP2StreamStates.Closed:
                    break;
            }
        }

        private void OnRemoteSettingChanged(HTTP2SettingsRegistry registry, HTTP2Settings setting, uint oldValue, uint newValue)
        {
            switch (setting)
            {
                case HTTP2Settings.INITIAL_WINDOW_SIZE:
                    // https://httpwg.org/specs/rfc7540.html#InitialWindowSize
                    // "Prior to receiving a SETTINGS frame that sets a value for SETTINGS_INITIAL_WINDOW_SIZE,
                    // an endpoint can only use the default initial window size when sending flow-controlled frames."
                    // "In addition to changing the flow-control window for streams that are not yet active,
                    // a SETTINGS frame can alter the initial flow-control window size for streams with active flow-control windows
                    // (that is, streams in the "open" or "half-closed (remote)" state). When the value of SETTINGS_INITIAL_WINDOW_SIZE changes,
                    // a receiver MUST adjust the size of all stream flow-control windows that it maintains by the difference between the new value and the old value."

                    // So, if we created a stream before the remote peer's initial settings frame is received, we
                    // will adjust the window size. For example: initial window size by default is 65535, if we later
                    // receive a change to 1048576 (1 MB) we will increase the current remoteWindow by (1 048 576 - 65 535 =) 983 041

                    // But because initial window size in a setting frame can be smaller then the default 65535 bytes,
                    // the difference can be negative:
                    // "A change to SETTINGS_INITIAL_WINDOW_SIZE can cause the available space in a flow-control window to become negative.
                    // A sender MUST track the negative flow-control window and MUST NOT send new flow-controlled frames
                    // until it receives WINDOW_UPDATE frames that cause the flow-control window to become positive.

                    // For example, if the client sends 60 KB immediately on connection establishment
                    // and the server sets the initial window size to be 16 KB, the client will recalculate
                    // the available flow - control window to be - 44 KB on receipt of the SETTINGS frame.
                    // The client retains a negative flow-control window until WINDOW_UPDATE frames restore the
                    // window to being positive, after which the client can resume sending."

                    this.remoteWindow += newValue - oldValue;

                    HTTPManager.Logger.Information("HTTP2Stream",
                        $"[{this.Id}] Remote Setting's Initial Window Updated from {oldValue:N0} to {newValue:N0}, diff: {newValue - oldValue:N0}, new remoteWindow: {this.remoteWindow:N0}, total data sent: {this.sentData:N0}");
                    break;
            }
        }

        private static void FinishRequest(HTTPRequest req, HTTP2Response resp, FramesAsStreamView dataStream)
        {
            if (dataStream != null)
            {
                resp.AddData(dataStream);

                dataStream.Close();
            }

            bool resendRequest;
            HTTPConnectionStates proposedConnectionStates;
            KeepAliveHeader keepAliveHeader = null;

            ConnectionHelper.HandleResponse("HTTP2Stream", req, out resendRequest, out proposedConnectionStates, ref keepAliveHeader);

            if (resendRequest)
                RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(req, RequestEvents.Resend));
            else
                req.State = HTTPRequestStates.Finished;
        }

        public void Removed()
        {

            HTTPManager.Logger.Information("HTTP2Stream", "Stream removed: " + this.Id.ToString());
        }
    }
}

#endif