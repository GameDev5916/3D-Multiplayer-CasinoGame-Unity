#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using BestHTTP.Core;

#if !BESTHTTP_DISABLE_CACHING
using BestHTTP.Caching;
#endif

namespace BestHTTP.Connections
{
    public sealed class HTTP1Handler : IHTTPRequestHandler
    {
        public bool HasCustomRequestProcessor { get { return false; } }
        public KeepAliveHeader KeepAlive { get { return this._keepAlive; } }
        private KeepAliveHeader _keepAlive;

        public bool CanProcessMultiple { get { return false; } }

        private HTTPConnection conn;

        public HTTP1Handler(HTTPConnection conn)
        {
            this.conn = conn;
        }

        public void Process(HTTPRequest request)
        {
        }

        public void RunHandler()
        {
            HTTPManager.Logger.Information("HTTP1Handler",
                $"[{this}] started processing request '{this.conn.CurrentRequest.CurrentUri.ToString()}'");

            HTTPConnectionStates proposedConnectionState = HTTPConnectionStates.Processing;

            bool resendRequest = false;

            try
            {
                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

#if !BESTHTTP_DISABLE_CACHING
                // Try load the full response from an already saved cache entity.
                // If the response could be loaded completely, we can skip connecting (if not already) and a full round-trip time to the server.
                if (HTTPCacheService.IsCachedEntityExpiresInTheFuture(this.conn.CurrentRequest) && ConnectionHelper.TryLoadAllFromCache(this.ToString(), this.conn.CurrentRequest))
                {
                    HTTPManager.Logger.Information("HTTP1Handler",
                        $"[{this}] Request could be fully loaded from cache! '{this.conn.CurrentRequest.CurrentUri.ToString()}'");
                    return;
                }
#endif

                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

#if !BESTHTTP_DISABLE_CACHING
                // Setup cache control headers before we send out the request
                if (!this.conn.CurrentRequest.DisableCache)
                    HTTPCacheService.SetHeaders(this.conn.CurrentRequest);
#endif

                // Write the request to the stream
                this.conn.CurrentRequest.SendOutTo(this.conn.connector.Stream);

                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

                // Receive response from the server
                bool received = Receive(this.conn.CurrentRequest);

                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

                if (!received && this.conn.CurrentRequest.Retries < this.conn.CurrentRequest.MaxRetries)
                {
                    proposedConnectionState = HTTPConnectionStates.Closed;
                    this.conn.CurrentRequest.Retries++;
                    resendRequest = true;
                    return;
                }

                ConnectionHelper.HandleResponse(this.conn.ToString(), this.conn.CurrentRequest, out resendRequest, out proposedConnectionState, ref this._keepAlive);
            }
            catch (TimeoutException e)
            {
                this.conn.CurrentRequest.Response = null;

                // We will try again only once
                if (this.conn.CurrentRequest.Retries < this.conn.CurrentRequest.MaxRetries)
                {
                    this.conn.CurrentRequest.Retries++;
                    resendRequest = true;
                }
                else
                {
                    this.conn.CurrentRequest.Exception = e;
                    this.conn.CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;
                }

                proposedConnectionState = HTTPConnectionStates.Closed;
            }
            catch (Exception e)
            {
                if (this.ShutdownType == ShutdownTypes.Immediate)
                    return;

#if !BESTHTTP_DISABLE_CACHING
                if (this.conn.CurrentRequest.UseStreaming)
                    HTTPCacheService.DeleteEntity(this.conn.CurrentRequest.CurrentUri);
#endif

                // Something gone bad, Response must be null!
                this.conn.CurrentRequest.Response = null;

                if (!this.conn.CurrentRequest.IsCancellationRequested)
                {
                    this.conn.CurrentRequest.Exception = e;
                    this.conn.CurrentRequest.State = HTTPRequestStates.Error;
                }

                proposedConnectionState = HTTPConnectionStates.Closed;
            }
            finally
            {
                // Exit ASAP
                if (this.ShutdownType != ShutdownTypes.Immediate)
                {
                    if (this.conn.CurrentRequest.IsCancellationRequested)
                    {
                        // we don't know what stage the request is cancelled, we can't safely reuse the tcp channel.
                        proposedConnectionState = HTTPConnectionStates.Closed;

                        this.conn.CurrentRequest.Response = null;

                        this.conn.CurrentRequest.State = this.conn.CurrentRequest.IsTimedOut ? HTTPRequestStates.TimedOut : HTTPRequestStates.Aborted;
                    }
                    else if (resendRequest)
                    {
                        RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.conn.CurrentRequest, RequestEvents.Resend));
                    }
                    else if (this.conn.CurrentRequest.Response != null && this.conn.CurrentRequest.Response.IsUpgraded)
                    {
                        proposedConnectionState = HTTPConnectionStates.WaitForProtocolShutdown;
                    }
                    else if (this.conn.CurrentRequest.State == HTTPRequestStates.Processing)
                    {
                        if (this.conn.CurrentRequest.Response != null)
                            this.conn.CurrentRequest.State = HTTPRequestStates.Finished;
                        else
                        {
                            this.conn.CurrentRequest.Exception = new Exception(
                                $"[{this.ToString()}] Remote server closed the connection before sending response header! Previous request state: {this.conn.CurrentRequest.State.ToString()}. Connection state: {this.conn.State.ToString()}");
                            this.conn.CurrentRequest.State = HTTPRequestStates.Error;

                            proposedConnectionState = HTTPConnectionStates.Closed;
                        }
                    }

                    this.conn.CurrentRequest = null;

                    if (proposedConnectionState == HTTPConnectionStates.Processing)
                        proposedConnectionState = HTTPConnectionStates.Recycle;

                    ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this.conn, proposedConnectionState));
                }
            }
        }

        private bool Receive(HTTPRequest request)
        {
            SupportedProtocols protocol = request.ProtocolHandler == SupportedProtocols.Unknown ? HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) : request.ProtocolHandler;

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTPConnection",
                    $"[{this.ToString()}] - Receive - protocol: {protocol.ToString()}");

            request.Response = HTTPProtocolFactory.Get(protocol, request, this.conn.connector.Stream, request.UseStreaming, false);

            if (!request.Response.Receive())
            {
                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("HTTP1Handler",
                        $"[{this.ToString()}] - Receive - Failed! Response will be null, returning with false.");
                request.Response = null;
                return false;
            }

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTP1Handler", $"[{this.ToString()}] - Receive - Finished Successfully!");

            return true;
        }

        public ShutdownTypes ShutdownType { get; private set; }

        public void Shutdown(ShutdownTypes type)
        {
            this.ShutdownType = type;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
        }

        ~HTTP1Handler()
        {
            Dispose(false);
        }
    }
}

#endif