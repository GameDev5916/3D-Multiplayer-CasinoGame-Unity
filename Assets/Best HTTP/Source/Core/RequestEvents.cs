using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Concurrent;

namespace BestHTTP.Core
{
    public enum RequestEvents
    {
        Upgraded,
        DownloadProgress,
        UploadProgress,
        StreamingData,
        StateChange,
        Resend,
        Headers
    }

    public
#if CSHARP_7_OR_LATER
        readonly
#endif
        struct RequestEventInfo
    {
        public readonly HTTPRequest SourceRequest;
        public readonly RequestEvents Event;

        public readonly HTTPRequestStates State;

        public readonly long Progress;
        public readonly long ProgressLength;

        public readonly byte[] Data;
        public readonly int DataLength;

        public RequestEventInfo(HTTPRequest request, RequestEvents @event)
        {
            this.SourceRequest = request;
            this.Event = @event;

            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;

            this.Data = null;
            this.DataLength = 0;
        }

        public RequestEventInfo(HTTPRequest request, HTTPRequestStates newState)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.StateChange;
            this.State = newState;

            this.Progress = this.ProgressLength = 0;
            this.Data = null;
            this.DataLength = 0;
        }

        public RequestEventInfo(HTTPRequest request, RequestEvents @event, long progress, long progressLength)
        {
            this.SourceRequest = request;
            this.Event = @event;
            this.State = HTTPRequestStates.Initial;

            this.Progress = progress;
            this.ProgressLength = progressLength;
            this.Data = null;
            this.DataLength = 0;
        }

        public RequestEventInfo(HTTPRequest request, byte[] data, int dataLength)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.StreamingData;
            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;
            this.Data = data;
            this.DataLength = dataLength;
        }

        public override string ToString()
        {
            return
                $"[RequestEventInfo SourceRequest: {this.SourceRequest.CurrentUri}, Event: {this.Event}, State: {this.State}, Progress: {this.Progress}, ProgressLength: {this.ProgressLength}, Data: {this.DataLength}]";
        }
    }

    internal static class RequestEventHelper
    {
        private static ConcurrentQueue<RequestEventInfo> requestEventQueue = new ConcurrentQueue<RequestEventInfo>();

#pragma warning disable 0649
        public static Action<RequestEventInfo> OnEvent;
#pragma warning restore

        public static void EnqueueRequestEvent(RequestEventInfo @event)
        {
            requestEventQueue.Enqueue(@event);
        }

        internal static void Clear()
        {
            requestEventQueue.Clear();
        }

        internal static void ProcessQueue()
        {
            RequestEventInfo requestEvent;
            while (requestEventQueue.TryDequeue(out requestEvent))
            {
                if (HTTPManager.Logger.Level == Loglevels.All)
                    HTTPManager.Logger.Information("RequestEventHelper", "Processing request event: " + requestEvent.ToString());

                if (OnEvent != null)
                {
                    try
                    {
                        OnEvent(requestEvent);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("RequestEventHelper", "ProcessQueue", ex);
                    }
                }

                HTTPRequest source = requestEvent.SourceRequest;
                switch (requestEvent.Event)
                {
                    case RequestEvents.StreamingData:
                        {
                            var response = source.Response;
                            if (response != null)
                                System.Threading.Interlocked.Decrement(ref response.UnprocessedFragments);

                            bool reuseBuffer = true;
                            try
                            {
                                if (source.OnStreamingData != null)
                                    reuseBuffer = source.OnStreamingData(source, response, requestEvent.Data, requestEvent.DataLength);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.StreamingData", ex);
                            }

                            if (reuseBuffer)
                                BufferPool.Release(requestEvent.Data);
                            break;
                        }

                    case RequestEvents.DownloadProgress:
                        try
                        {
                            if (source.OnDownloadProgress != null)
                                source.OnDownloadProgress(source, requestEvent.Progress, requestEvent.ProgressLength);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.DownloadProgress", ex);
                        }
                        break;

                    case RequestEvents.UploadProgress:
                        try
                        {
                            if (source.OnUploadProgress != null)
                                source.OnUploadProgress(source, requestEvent.Progress, requestEvent.ProgressLength);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.UploadProgress", ex);
                        }
                        break;

                    case RequestEvents.Upgraded:
                        try
                        {
                            if (source.OnUpgraded != null)
                                source.OnUpgraded(source, source.Response);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.Upgraded", ex);
                        }

                        IProtocol protocol = source.Response as IProtocol;
                        if (protocol != null)
                            ProtocolEventHelper.AddProtocol(protocol);
                        break;

                    case RequestEvents.Resend:
                        source.State = HTTPRequestStates.Initial;

                        var host = HostManager.GetHost(source.CurrentUri.Host);

                        host.Send(source);

                        break;

                    case RequestEvents.Headers:
                        {
                            try
                            {
                                var response = source.Response;
                                if (source.OnHeadersReceived != null && response != null)
                                    source.OnHeadersReceived(source, response);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.Headers", ex);
                            }
                            break;
                        }

                    case RequestEvents.StateChange:
                        RequestEventHelper.HandleRequestStateChange(requestEvent);
                        break;
                }
            }
        }

        private static bool AbortRequestWhenTimedOut(object context)
        {
            HTTPRequest request = context as HTTPRequest;

            if (request.State != HTTPRequestStates.Processing)
                return false; // don't repeat

            // Protocols will shut down themself
            if (request.Response is IProtocol)
                return false;

            if (request.IsTimedOut)
            {
                HTTPManager.Logger.Information("RequestEventHelper", "AbortRequestWhenTimedOut - Request timed out. CurrentUri: " + request.CurrentUri.ToString());
                request.Abort();

                return false; // don't repeat
            }

            return true;  // repeat
        }

        internal static void HandleRequestStateChange(RequestEventInfo @event)
        {
            HTTPRequest source = @event.SourceRequest;

            switch (@event.State)
            {
                case HTTPRequestStates.Processing:
                    if ((!source.UseStreaming && source.UploadStream == null) || source.EnableTimoutForStreaming)
                        BestHTTP.Extensions.Timer.Add(new TimerData(TimeSpan.FromSeconds(1), @event.SourceRequest, AbortRequestWhenTimedOut));
                    break;

                case HTTPRequestStates.Aborted:
                case HTTPRequestStates.ConnectionTimedOut:
                case HTTPRequestStates.TimedOut:
                case HTTPRequestStates.Error:
                case HTTPRequestStates.Finished:
                    if (source.Callback != null)
                    {
                        try
                        {
                            source.Callback(source, source.Response);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "HandleRequestStateChange " + @event.State, ex);
                        }

                        //if (source.Response != null && source.Response.Data != null)
                        //    VariableSizedBufferPool.Release(source.Response.Data);
                    }

                    source.Dispose();

                    HostManager.GetHost(source.CurrentUri.Host)
                                .GetHostDefinition(HostDefinition.GetKeyForRequest(source))
                                .TryToSendQueuedRequests();
                    break;
            }
        }
    }
}
