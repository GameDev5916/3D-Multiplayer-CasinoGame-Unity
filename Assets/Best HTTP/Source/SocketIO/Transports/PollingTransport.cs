﻿#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Text;

namespace BestHTTP.SocketIO.Transports
{
    public sealed class PollingTransport : ITransport
    {
        public static byte[] SquareBrackets = new byte[] { (byte)'[', (byte)']' };
        public static byte[] CurlyBrackets = new byte[] { (byte)'{', (byte)'}' };

        #region Public (ITransport) Properties

        public TransportTypes Type { get { return TransportTypes.Polling; } }
        public TransportStates State { get; private set; }
        public SocketManager Manager { get; private set; }
        public bool IsRequestInProgress { get { return LastRequest != null; } }
        public bool IsPollingInProgress { get { return PollRequest != null; } }

        #endregion

        #region Private Fields

        /// <summary>
        /// The last POST request we sent to the server.
        /// </summary>
        private HTTPRequest LastRequest;

        /// <summary>
        /// Last GET request we sent to the server.
        /// </summary>
        private HTTPRequest PollRequest;

        /// <summary>
        /// The last packet with expected binary attachments
        /// </summary>
        private Packet PacketWithAttachment;

        #endregion

        public enum PayloadTypes : byte
        {
            Text,
            Binary
        }

        public PollingTransport(SocketManager manager)
        {
            Manager = manager;
        }

        public void Open()
        {
            string format = "{0}?EIO={1}&transport=polling&t={2}-{3}{5}";
            if (Manager.Handshake != null)
                format += "&sid={4}";

            bool sendAdditionalQueryParams = !Manager.Options.QueryParamsOnlyForHandshake || (Manager.Options.QueryParamsOnlyForHandshake && Manager.Handshake == null);

            HTTPRequest request = new HTTPRequest(new Uri(string.Format(format,
                                                                        Manager.Uri.ToString(),
                                                                        SocketManager.MinProtocolVersion,
                                                                        Manager.Timestamp.ToString(),
                                                                        Manager.RequestCounter++.ToString(),
                                                                        Manager.Handshake != null ? Manager.Handshake.Sid : string.Empty,
                                                                        sendAdditionalQueryParams ? Manager.Options.BuildQueryParams() : string.Empty)),
                                                OnRequestFinished);

#if !BESTHTTP_DISABLE_CACHING
            // Don't even try to cache it
            request.DisableCache = true;
#endif

            request.MaxRetries = 0;

            request.Send();

            State = TransportStates.Opening;
        }

        /// <summary>
        /// Closes the transport and cleans up resources.
        /// </summary>
        public void Close()
        {
            if (State == TransportStates.Closed)
                return;

            State = TransportStates.Closed;

            /*
            if (LastRequest != null)
                LastRequest.Abort();

            if (PollRequest != null)
                PollRequest.Abort();*/
        }

        #region Packet Sending Implementation

        private System.Collections.Generic.List<Packet> lonelyPacketList = new System.Collections.Generic.List<Packet>(1);
        public void Send(Packet packet)
        {
            try
            {
                lonelyPacketList.Add(packet);
                Send(lonelyPacketList);
            }
            finally
            {
                lonelyPacketList.Clear();
            }
        }

        public void Send(System.Collections.Generic.List<Packet> packets)
        {
            if (State != TransportStates.Opening && State != TransportStates.Open)
                return;

            if (IsRequestInProgress)
                throw new Exception("Sending packets are still in progress!");

            byte[] buffer = null;

            try
            {
                buffer = packets[0].EncodeBinary();

                for (int i = 1; i < packets.Count; ++i)
                {
                    byte[] tmpBuffer = packets[i].EncodeBinary();

                    Array.Resize(ref buffer, buffer.Length + tmpBuffer.Length);

                    Array.Copy(tmpBuffer, 0, buffer, buffer.Length - tmpBuffer.Length, tmpBuffer.Length);
                }

                packets.Clear();
            }
            catch (Exception ex)
            {
                (Manager as IManager).EmitError(SocketIOErrors.Internal, ex.Message + " " + ex.StackTrace);
                return;
            }

            LastRequest = new HTTPRequest(new Uri(
                    $"{Manager.Uri.ToString()}?EIO={SocketManager.MinProtocolVersion}&transport=polling&t={Manager.Timestamp.ToString()}-{Manager.RequestCounter++.ToString()}&sid={Manager.Handshake.Sid}{(!Manager.Options.QueryParamsOnlyForHandshake ? Manager.Options.BuildQueryParams() : string.Empty)}"),
                                          HTTPMethods.Post,
                                          OnRequestFinished);


#if !BESTHTTP_DISABLE_CACHING
            // Don't even try to cache it
            LastRequest.DisableCache = true;
#endif

            LastRequest.SetHeader("Content-Type", "application/octet-stream");
            LastRequest.RawData = buffer;

            LastRequest.Send();
        }

        private void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            // Clear out the LastRequest variable, so we can start sending out new packets
            LastRequest = null;

            if (State == TransportStates.Closed)
                return;

            string errorString = null;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                        HTTPManager.Logger.Verbose("PollingTransport", "OnRequestFinished: " + resp.DataAsText);

                    if (resp.IsSuccess)
                    {
                        // When we are sending data, the response is an 'ok' string
                        if (req.MethodType != HTTPMethods.Post)
                            ParseResponse(resp);
                    }
                    else
                        errorString =
                            $"Polling - Request finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText} Uri: {req.CurrentUri}";
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    errorString = (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    errorString = $"Polling - Request({req.CurrentUri}) Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    errorString = $"Polling - Connection Timed Out! Uri: {req.CurrentUri}";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    errorString = $"Polling - Processing the request({req.CurrentUri}) Timed Out!";
                    break;
            }

            if (!string.IsNullOrEmpty(errorString))
                (Manager as IManager).OnTransportError(this, errorString);
        }

        #endregion

        #region Polling Implementation

        public void Poll()
        {
            if (PollRequest != null || State == TransportStates.Paused)
                return;

            PollRequest = new HTTPRequest(new Uri(
                    $"{Manager.Uri.ToString()}?EIO={SocketManager.MinProtocolVersion}&transport=polling&t={Manager.Timestamp.ToString()}-{Manager.RequestCounter++.ToString()}&sid={Manager.Handshake.Sid}{(!Manager.Options.QueryParamsOnlyForHandshake ? Manager.Options.BuildQueryParams() : string.Empty)}"),
                                        HTTPMethods.Get,
                                        OnPollRequestFinished);

#if !BESTHTTP_DISABLE_CACHING
            // Don't even try to cache it
            PollRequest.DisableCache = true;
#endif

            PollRequest.MaxRetries = 0;

            PollRequest.Send();
        }

        private void OnPollRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            // Clear the PollRequest variable, so we can start a new poll.
            PollRequest = null;

            if (State == TransportStates.Closed)
                return;

            string errorString = null;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:

                    if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                        HTTPManager.Logger.Verbose("PollingTransport", "OnPollRequestFinished: " + resp.DataAsText);

                    if (resp.IsSuccess)
                        ParseResponse(resp);
                    else
                        errorString =
                            $"Polling - Request finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText} Uri: {req.CurrentUri}";
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    errorString = req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception";
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    errorString = $"Polling - Request({req.CurrentUri}) Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    errorString = $"Polling - Connection Timed Out! Uri: {req.CurrentUri}";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    errorString = $"Polling - Processing the request({req.CurrentUri}) Timed Out!";
                    break;
            }

            if (!string.IsNullOrEmpty(errorString))
                (Manager as IManager).OnTransportError(this, errorString);
        }

        #endregion

        #region Packet Parsing and Handling

        /// <summary>
        /// Preprocessing and sending out packets to the manager.
        /// </summary>
        private void OnPacket(Packet packet)
        {
            if (packet.AttachmentCount != 0 && !packet.HasAllAttachment)
            {
                PacketWithAttachment = packet;
                return;
            }

            switch (packet.TransportEvent)
            {
                case TransportEventTypes.Open:
                    if (this.State != TransportStates.Opening)
                        HTTPManager.Logger.Warning("PollingTransport", "Received 'Open' packet while state is '" + State.ToString() + "'");
                    else
                        State = TransportStates.Open;
                    goto default;

                case TransportEventTypes.Message:
                  if (packet.SocketIOEvent == SocketIOEventTypes.Connect) //2:40
                    this.State = TransportStates.Open;
                  goto default;

                default:
                    (Manager as IManager).OnPacket(packet);
                    break;
            }
        }

        /// <summary>
        /// Will parse the response, and send out the parsed packets.
        /// </summary>
        private void ParseResponse(HTTPResponse resp)
        {
            try
            {
                if (resp != null && resp.Data != null && resp.Data.Length >= 1)
                {
// 1.x
//00000000  00 09 07 ff 30 7b 22 73 69 64 22 3a 22 6f 69 48       0{"sid":"oiH
//00000010  34 31 33 73 61 49 4e 52 53 67 37 41 4b 41 41 41   413saINRSg7AKAAA
//00000020  41 22 2c 22 75 70 67 72 61 64 65 73 22 3a 5b 22   A","upgrades":["
//00000030  77 65 62 73 6f 63 6b 65 74 22 5d 2c 22 70 69 6e   websocket"],"pin
//00000040  67 49 6e 74 65 72 76 61 6c 22 3a 32 35 30 30 30   gInterval":25000
//00000050  2c 22 70 69 6e 67 54 69 6d 65 6f 75 74 22 3a 36   ,"pingTimeout":6
//00000060  30 30 30 30 7d                                    0000}

// 2.x
//00000000  39 37 3a 30 7b 22 73 69 64 22 3a 22 73 36 62 5a   97:0{"sid":"s6bZ
//00000010  6c 43 37 66 51 59 6b 4f 46 4f 62 35 41 41 41 41   lC7fQYkOFOb5AAAA
//00000020  22 2c 22 75 70 67 72 61 64 65 73 22 3a 5b 22 77   ","upgrades":["w
//00000030  65 62 73 6f 63 6b 65 74 22 5d 2c 22 70 69 6e 67   ebsocket"],"ping
//00000040  49 6e 74 65 72 76 61 6c 22 3a 32 35 30 30 30 2c   Interval":25000,
//00000050  22 70 69 6e 67 54 69 6d 65 6f 75 74 22 3a 36 30   "pingTimeout":60
//00000060  30 30 30 7d 32 3a 34 30                           000}2:40

//00000000  38 30 3a 34 32 5b 22 50 6f 6c 6c 69 6e 67 20 69   80:42["Polling i
//00000010  73 20 77 6f 72 6b 69 6e 67 20 6e 6f 72 6d 61 6c   s working normal
//00000020  6c 79 20 62 75 74 20 63 72 61 73 68 65 73 20 6f   ly but crashes o
//00000030  6e 20 72 65 63 65 69 76 69 6e 67 20 6d 75 74 61   n receiving muta
//00000040  74 65 64 20 76 6f 77 65 6c 73 20 c3 bc 20 c3 b6   ted vowels
//00000050  20 c3 a4 2e 22 5d                                    ."]


//00000000  33 30 3a 34 32 5b 22 74 79 70 69 6e 67 22 2c 7b   30:42["typing",{
//00000010  22 75 73 65 72 6e 61 6d 65 22 3a 22 *e6 89 8e e5   "username":"
//00000020  bf 83 *22 7d 5d                                      "}]


                    int idx = 0;

                    while (idx < resp.Data.Length)
                    {
                        PayloadTypes type = PayloadTypes.Text;
                        int length = 0;

                        if (resp.Data[idx] < '0') {
                          type = (PayloadTypes)resp.Data[idx++];

                          byte num = resp.Data[idx++];
                          while (num != 0xFF) {
                            length = (length * 10) + num;
                            num = resp.Data[idx++];
                          }
                        }
                        else {
                          byte next = resp.Data[idx++];
                          while (next != ':') {
                            length = (length * 10) + (next - '0');
                            next = resp.Data[idx++];
                          }
                        }

                        Packet packet = null;
                        switch(type)
                        {
                            case PayloadTypes.Text:
                                // While we already received a length for the packet, it contains the character count of the packet not the byte count.
                                // So, when the packet contains UTF8 characters, length will contain less than the actual byte count.
                                // To fix this, we have to find the last square or curly bracket and if the sent and calculated lengths are different
                                //  we will use the larger one.
                                int customLength = 1;
                                byte next = resp.Data[idx];
                                while (next != SquareBrackets[0] && next != CurlyBrackets[0] && idx + customLength < resp.Data.Length)
                                    next = resp.Data[idx + customLength++];

                                if (idx + customLength < resp.Data.Length)
                                {
                                    byte[] brackets = SquareBrackets;
                                    if (next == CurlyBrackets[0])
                                        brackets = CurlyBrackets;

                                    int bracketCount = 1;
                                    while (bracketCount != 0 && idx + customLength < resp.Data.Length)
                                    {
                                        next = resp.Data[idx + customLength++];
                                        if (next == brackets[0])
                                            bracketCount++;
                                        else if (next == brackets[1])
                                            bracketCount--;
                                    }
                                }

                                packet = new Packet(Encoding.UTF8.GetString(resp.Data, idx, Math.Max(length, customLength)));
                                break;
                            case PayloadTypes.Binary:
                                if (PacketWithAttachment != null)
                                {
                                    // First byte is the packet type. We can skip it, so we advance our idx and we also have
                                    // to decrease length
                                    idx++;
                                    length--;

                                    byte[] buffer = new byte[length];
                                    Array.Copy(resp.Data, idx, buffer, 0, length);

                                    PacketWithAttachment.AddAttachmentFromServer(buffer, true);

                                    if (PacketWithAttachment.HasAllAttachment)
                                    {
                                        packet = PacketWithAttachment;
                                        PacketWithAttachment = null;
                                    }
                                }
                                break;
                        } // switch

                        if (packet != null)
                        {
                            try
                            {
                                OnPacket(packet);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("PollingTransport", "ParseResponse - OnPacket", ex);
                                (Manager as IManager).EmitError(SocketIOErrors.Internal, ex.Message + " " + ex.StackTrace);
                            }
                        }

                        idx += length;
                    }// while
                }
            }
            catch (Exception ex)
            {
                (Manager as IManager).EmitError(SocketIOErrors.Internal, ex.Message + " " + ex.StackTrace);

                HTTPManager.Logger.Exception("PollingTransport", "ParseResponse", ex);
            }
        }

        #endregion
    }
}

#endif