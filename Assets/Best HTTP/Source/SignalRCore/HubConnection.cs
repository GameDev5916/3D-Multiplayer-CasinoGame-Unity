#if !BESTHTTP_DISABLE_SIGNALR_CORE && !BESTHTTP_DISABLE_WEBSOCKET
using BestHTTP.Futures;
using BestHTTP.SignalRCore.Authentication;
using BestHTTP.SignalRCore.Messages;
using System;
using System.Collections.Generic;

namespace BestHTTP.SignalRCore
{
    public sealed class HubConnection : BestHTTP.Extensions.IHeartbeat
    {
        public static readonly object[] EmptyArgs = new object[0];

        /// <summary>
        /// Uri of the Hub endpoint
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Current state of this connection.
        /// </summary>
        public ConnectionStates State { get; private set; }

        /// <summary>
        /// Current, active ITransport instance.
        /// </summary>
        public ITransport Transport { get; private set; }

        /// <summary>
        /// The IProtocol implementation that will parse, encode and decode messages.
        /// </summary>
        public IProtocol Protocol { get; private set; }

        /// <summary>
        /// This event is called when the connection is redirected to a new uri.
        /// </summary>
        public event Action<HubConnection, Uri, Uri> OnRedirected;

        /// <summary>
        /// This event is called when successfully connected to the hub.
        /// </summary>
        public event Action<HubConnection> OnConnected;

        /// <summary>
        /// This event is called when an unexpected error happen and the connection is closed.
        /// </summary>
        public event Action<HubConnection, string> OnError;

        /// <summary>
        /// This event is called when the connection is gracefully terminated.
        /// </summary>
        public event Action<HubConnection> OnClosed;

        /// <summary>
        /// This event is called for every server-sent message. When returns false, no further processing of the message is done by the plugin.
        /// </summary>
        public event Func<HubConnection, Message, bool> OnMessage;

        /// <summary>
        /// Called when the HubConnection start its reconnection process after loosing its underlying connection.
        /// </summary>
        public event Action<HubConnection, string> OnReconnecting;

        /// <summary>
        /// Called after a succesfull reconnection.
        /// </summary>
        public event Action<HubConnection> OnReconnected;

        /// <summary>
        /// An IAuthenticationProvider implementation that will be used to authenticate the connection.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Negotiation response sent by the server.
        /// </summary>
        public NegotiationResult NegotiationResult { get; private set; }

        /// <summary>
        /// Options that has been used to create the HubConnection.
        /// </summary>
        public HubOptions Options { get; private set; }

        /// <summary>
        /// How many times this connection is redirected.
        /// </summary>
        public int RedirectCount { get; private set; }

        /// <summary>
        /// The reconnect policy that will be used when the underlying connection is lost. Its default value is null.
        /// </summary>
        public IRetryPolicy ReconnectPolicy { get; set; }

        /// <summary>
        /// This will be increment to add a unique id to every message the plugin will send.
        /// </summary>
        private long lastInvocationId = 1;

        /// <summary>
        /// Id of the last streaming parameter.
        /// </summary>
        private int lastStreamId = 1;

        /// <summary>
        ///  Store the callback for all sent message that expect a return value from the server. All sent message has
        ///  a unique invocationId that will be sent back from the server.
        /// </summary>
        private Dictionary<long, Action<Message>> invocations = new Dictionary<long, Action<Message>>();

        /// <summary>
        /// This is where we store the methodname => callback mapping.
        /// </summary>
        private Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// When we sent out the last message to the server.
        /// </summary>
        private DateTime lastMessageSent;

        private RetryContext currentContext;
        private DateTime reconnectStartTime = DateTime.MinValue;
        private DateTime reconnectAt;

        public HubConnection(Uri hubUri, IProtocol protocol)
            : this(hubUri, protocol, new HubOptions())
        {
        }

        public HubConnection(Uri hubUri, IProtocol protocol, HubOptions options)
        {
            this.Uri = hubUri;
            this.State = ConnectionStates.Initial;
            this.Options = options;
            this.Protocol = protocol;
            this.Protocol.Connection = this;
            this.AuthenticationProvider = new DefaultAccessTokenAuthenticator(this);
        }

        public void StartConnect()
        {
            if (this.State != ConnectionStates.Initial && this.State != ConnectionStates.Redirected && this.State != ConnectionStates.Reconnecting)
            {
                HTTPManager.Logger.Warning("HubConnection", "StartConnect - Expected Initial or Redirected state, got " + this.State.ToString());
                return;
            }

            HTTPManager.Logger.Verbose("HubConnection", "StartConnect");

            if (this.AuthenticationProvider != null && this.AuthenticationProvider.IsPreAuthRequired)
            {
                HTTPManager.Logger.Information("HubConnection", "StartConnect - Authenticating");
                SetState(ConnectionStates.Authenticating);

                this.AuthenticationProvider.OnAuthenticationSucceded += OnAuthenticationSucceded;
                this.AuthenticationProvider.OnAuthenticationFailed += OnAuthenticationFailed;

                // Start the authentication process
                this.AuthenticationProvider.StartAuthentication();
            }
            else
                StartNegotiation();
        }

        private void OnAuthenticationSucceded(IAuthenticationProvider provider)
        {
            HTTPManager.Logger.Verbose("HubConnection", "OnAuthenticationSucceded");

            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

            StartNegotiation();
        }

        private void OnAuthenticationFailed(IAuthenticationProvider provider, string reason)
        {
            HTTPManager.Logger.Error("HubConnection", "OnAuthenticationFailed: " + reason);

            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

            SetState(ConnectionStates.Closed, reason);
        }

        private void StartNegotiation()
        {
            HTTPManager.Logger.Verbose("HubConnection", "StartNegotiation");

            if (this.State == ConnectionStates.CloseInitiated)
            {
                SetState(ConnectionStates.Closed);
                return;
            }

            if (this.Options.SkipNegotiation)
            {
                HTTPManager.Logger.Verbose("HubConnection", "Skipping negotiation");
                ConnectImpl();

                return;
            }

            SetState(ConnectionStates.Negotiating);

            // https://github.com/aspnet/SignalR/blob/dev/specs/TransportProtocols.md#post-endpoint-basenegotiate-request
            // Send out a negotiation request. While we could skip it and connect right with the websocket transport
            //  it might return with additional information that could be useful.

            UriBuilder builder = new UriBuilder(this.Uri);
            if (builder.Path.EndsWith("/"))
                builder.Path += "negotiate";
            else
                builder.Path += "/negotiate";

            var request = new HTTPRequest(builder.Uri, HTTPMethods.Post, OnNegotiationRequestFinished);
            if (this.AuthenticationProvider != null)
                this.AuthenticationProvider.PrepareRequest(request);

            request.Send();
        }

        private void ConnectImpl()
        {
            HTTPManager.Logger.Verbose("HubConnection", "ConnectImpl");

            switch (this.Options.PreferedTransport)
            {
                case TransportTypes.WebSocket:
                    if (this.NegotiationResult != null && !IsTransportSupported("WebSockets"))
                    {
                        SetState(ConnectionStates.Closed, "The 'WebSockets' transport isn't supported by the server!");
                        return;
                    }

                    this.Transport = new Transports.WebSocketTransport(this);
                    this.Transport.OnStateChanged += Transport_OnStateChanged;
                    break;

                default:
                    SetState(ConnectionStates.Closed, "Unsupportted transport: " + this.Options.PreferedTransport);
                    break;
            }

            this.Transport.StartConnect();
        }

        private bool IsTransportSupported(string transportName)
        {
            // https://github.com/aspnet/SignalR/blob/release/2.2/specs/TransportProtocols.md#post-endpoint-basenegotiate-request
            // If the negotiation response contains only the url and accessToken, no 'availableTransports' list is sent
            if (this.NegotiationResult.SupportedTransports == null)
                return true;

            for (int i = 0; i < this.NegotiationResult.SupportedTransports.Count; ++i)
                if (this.NegotiationResult.SupportedTransports[i].Name == transportName)
                    return true;

            return false;
        }

        private void OnNegotiationRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            if (this.State == ConnectionStates.CloseInitiated)
            {
                SetState(ConnectionStates.Closed);
                return;
            }

            string errorReason = null;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("HubConnection", "Negotiation Request Finished Successfully! Response: " + resp.DataAsText);

                        // Parse negotiation
                        this.NegotiationResult = NegotiationResult.Parse(resp.DataAsText, out errorReason, this);

                        // Room for improvement: check validity of the negotiation result:
                        //  If url and accessToken is present, the other two must be null.
                        //  https://github.com/aspnet/SignalR/blob/dev/specs/TransportProtocols.md#post-endpoint-basenegotiate-request

                        if (string.IsNullOrEmpty(errorReason))
                        {
                            if (this.NegotiationResult.Url != null)
                            {
                                this.SetState(ConnectionStates.Redirected);

                                if (++this.RedirectCount >= this.Options.MaxRedirects)
                                    errorReason = $"MaxRedirects ({this.Options.MaxRedirects:N0}) reached!";
                                else
                                {
                                    var oldUri = this.Uri;
                                    this.Uri = this.NegotiationResult.Url;

                                    if (this.OnRedirected != null)
                                    {
                                        try
                                        {
                                            this.OnRedirected(this, oldUri, Uri);
                                        }
                                        catch (Exception ex)
                                        {
                                            HTTPManager.Logger.Exception("HubConnection", "OnNegotiationRequestFinished - OnRedirected", ex);
                                        }
                                    }

                                    StartConnect();
                                }
                            }
                            else
                                ConnectImpl();
                        }
                    }
                    else // Internal server error?
                        errorReason =
                            $"Negotiation Request Finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}";
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    errorReason = "Negotiation Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    errorReason = "Negotiation Request Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    errorReason = "Negotiation Request - Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    errorReason = "Negotiation Request - Processing the request Timed Out!";
                    break;
            }

            if (errorReason != null)
                SetState(ConnectionStates.Closed, errorReason);
        }

        public void StartClose()
        {
            HTTPManager.Logger.Verbose("HubConnection", "StartClose");
            if (this.State == ConnectionStates.Reconnecting)
                SetState(ConnectionStates.Closed);
            else
            {
                SetState(ConnectionStates.CloseInitiated);

                if (this.Transport != null)
                    this.Transport.StartClose();
            }
        }

        public IFuture<TResult> Invoke<TResult>(string target, params object[] args)
        {
            Future<TResult> future = new Future<TResult>();

            InvokeImp(target,
                args,
                (message) =>
                    {
                        bool isSuccess = string.IsNullOrEmpty(message.error);
                        if (isSuccess)
                            future.Assign((TResult)this.Protocol.ConvertTo(typeof(TResult), message.result));
                        else
                            future.Fail(new Exception(message.error));
                    });

            return future;
        }

        public IFuture<bool> Send(string target, params object[] args)
        {
            Future<bool> future = new Future<bool>();

            InvokeImp(target,
                args,
                (message) =>
                    {
                        bool isSuccess = string.IsNullOrEmpty(message.error);
                        if (isSuccess)
                            future.Assign(true);
                        else
                            future.Fail(new Exception(message.error));
                    });

            return future;
        }

        private long InvokeImp(string target, object[] args, Action<Message> callback, bool isStreamingInvocation = false)
        {
            if (this.State != ConnectionStates.Connected)
                return -1;

            long invocationId = System.Threading.Interlocked.Increment(ref this.lastInvocationId);
            var message = new Message
            {
                type = isStreamingInvocation ? MessageTypes.StreamInvocation : MessageTypes.Invocation,
                invocationId = invocationId.ToString(),
                target = target,
                arguments = args,
                nonblocking = callback == null,
            };

            SendMessage(message);

            if (callback != null)
                this.invocations.Add(invocationId, callback);

            return invocationId;
        }

        internal void SendMessage(Message message)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HubConnection", "SendMessage: " + message.ToString());

            byte[] encoded = this.Protocol.EncodeMessage(message);
            this.Transport.Send(encoded);

            this.lastMessageSent = DateTime.UtcNow;
        }

        public DownStreamItemController<TDown> GetDownStreamController<TDown>(string target, params object[] args)
        {
            long invocationId = System.Threading.Interlocked.Increment(ref this.lastInvocationId);

            var future = new Future<TDown>();
            future.BeginProcess();

            var controller = new DownStreamItemController<TDown>(this, invocationId, future);

            Action<Message> callback = (Message msg) =>
            {
                switch (msg.type)
                {
                    // StreamItem message contains only one item.
                    case MessageTypes.StreamItem:
                        {
                            if (controller.IsCanceled)
                                break;

                            TDown item = (TDown)this.Protocol.ConvertTo(typeof(TDown), msg.item);

                            future.AssignItem(item);
                            break;
                        }

                    case MessageTypes.Completion:
                        {
                            bool isSuccess = string.IsNullOrEmpty(msg.error);
                            if (isSuccess)
                            {
                                // While completion message must not contain any result, this should be future-proof
                                if (!controller.IsCanceled && msg.result != null)
                                {
                                    TDown result = (TDown)this.Protocol.ConvertTo(typeof(TDown), msg.result);

                                    future.AssignItem(result);
                                }

                                future.Finish();
                            }
                            else
                                future.Fail(new Exception(msg.error));
                            break;
                        }
                }
            };

            var message = new Message
            {
                type = MessageTypes.StreamInvocation,
                invocationId = invocationId.ToString(),
                target = target,
                arguments = args,
                nonblocking = false,
            };

            SendMessage(message);

            if (callback != null)
                this.invocations.Add(invocationId, callback);

            return controller;
        }

        public UpStreamItemController<TResult> GetUpStreamController<TResult>(string target, int paramCount, bool downStream = false)
        {
            Future<TResult> future = new Future<TResult>();
            future.BeginProcess();

            long invocationId = System.Threading.Interlocked.Increment(ref this.lastInvocationId);

            string[] streamIds = new string[paramCount];
            for (int i = 0; i < paramCount; i++)
                streamIds[i] = System.Threading.Interlocked.Increment(ref this.lastStreamId).ToString();

            var controller = new UpStreamItemController<TResult>(this, invocationId, streamIds, future);

            Action<Message> callback = (Message msg) => {
                switch (msg.type)
                {
                    // StreamItem message contains only one item.
                    case MessageTypes.StreamItem:
                        {
                            if (controller.IsCanceled)
                                break;

                            TResult item = (TResult)this.Protocol.ConvertTo(typeof(TResult), msg.item);

                            future.AssignItem(item);
                            break;
                        }

                    case MessageTypes.Completion:
                        {
                            bool isSuccess = string.IsNullOrEmpty(msg.error);
                            if (isSuccess)
                            {
                                // While completion message must not contain any result, this should be future-proof
                                if (!controller.IsCanceled && msg.result != null)
                                {
                                    TResult result = (TResult)this.Protocol.ConvertTo(typeof(TResult), msg.result);

                                    future.AssignItem(result);
                                }

                                future.Finish();
                            }
                            else
                            {
                                var ex = new Exception(msg.error);
                                future.Fail(ex);
                            }
                            break;
                        }
                }
            };

            var messageToSend = new Message
            {
                type = downStream ? MessageTypes.StreamInvocation : MessageTypes.Invocation,
                invocationId = invocationId.ToString(),
                target = target,
                arguments = new object[0],
                streamIds = streamIds,
                nonblocking = false,
            };

            SendMessage(messageToSend);

            this.invocations.Add(invocationId, callback);

            return controller;
        }

        public void On(string methodName, Action callback)
        {
            On(methodName, null, (args) => callback());
        }

        public void On<T1>(string methodName, Action<T1> callback)
        {
            On(methodName, new Type[] { typeof(T1) }, (args) => callback((T1)args[0]));
        }

        public void On<T1, T2>(string methodName, Action<T1, T2> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2) },
                (args) => callback((T1)args[0], (T2)args[1]));
        }

        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2), typeof(T3) },
                (args) => callback((T1)args[0], (T2)args[1], (T3)args[2]));
        }

        public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) },
                (args) => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]));
        }

        public void On(string methodName, Type[] paramTypes, Action<object[]> callback)
        {
            Subscription subscription = null;
            if (!this.subscriptions.TryGetValue(methodName, out subscription))
                this.subscriptions.Add(methodName, subscription = new Subscription());

            subscription.Add(paramTypes, callback);
        }

        internal void OnMessages(List<Message> messages)
        {
            for (int messageIdx = 0; messageIdx < messages.Count; ++messageIdx)
            {
                var message = messages[messageIdx];

                try
                {
                    if (this.OnMessage != null && !this.OnMessage(this, message))
                        return;
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("HubConnection", "Exception in OnMessage user code!", ex);
                }

                switch (message.type)
                {
                    case MessageTypes.Invocation:
                        {
                            Subscription subscribtion = null;
                            if (this.subscriptions.TryGetValue(message.target, out subscribtion))
                            {
                                for (int i = 0; i < subscribtion.callbacks.Count; ++i)
                                {
                                    var callbackDesc = subscribtion.callbacks[i];

                                    object[] realArgs = null;
                                    try
                                    {
                                        realArgs = this.Protocol.GetRealArguments(callbackDesc.ParamTypes, message.arguments);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - Invocation - GetRealArguments", ex);
                                    }

                                    try
                                    {
                                        callbackDesc.Callback.Invoke(realArgs);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - Invocation - Invoke", ex);
                                    }
                                }
                            }

                            break;
                        }

                    case MessageTypes.StreamItem:
                        {
                            long invocationId;
                            if (long.TryParse(message.invocationId, out invocationId))
                            {
                                Action<Message> callback;
                                if (this.invocations.TryGetValue(invocationId, out callback) && callback != null)
                                {
                                    try
                                    {
                                        callback(message);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - StreamItem - callback", ex);
                                    }
                                }
                            }
                            break;
                        }

                    case MessageTypes.Completion:
                        {
                            long invocationId;
                            if (long.TryParse(message.invocationId, out invocationId))
                            {
                                Action<Message> callback;
                                if (this.invocations.TryGetValue(invocationId, out callback) && callback != null)
                                {
                                    try
                                    {
                                        callback(message);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - Completion - callback", ex);
                                    }
                                }
                                this.invocations.Remove(invocationId);
                            }
                            break;
                        }

                    case MessageTypes.Close:
                        SetState(ConnectionStates.Closed, message.error);
                        break;
                }
            }
        }

        private void Transport_OnStateChanged(TransportStates oldState, TransportStates newState)
        {
            HTTPManager.Logger.Verbose("HubConnection",
                $"Transport_OnStateChanged - oldState: {oldState.ToString()} newState: {newState.ToString()}");

            switch (newState)
            {
                case TransportStates.Connected:
                    SetState(ConnectionStates.Connected);
                    break;

                case TransportStates.Failed:
                    SetState(ConnectionStates.Closed, this.Transport.ErrorReason);
                    break;

                case TransportStates.Closed:
                    SetState(ConnectionStates.Closed);
                    break;
            }
        }

        private void SetState(ConnectionStates state, string errorReason = null)
        {
            if (string.IsNullOrEmpty(errorReason))
                HTTPManager.Logger.Information("HubConnection", "SetState - from State: '" + this.State.ToString() + "' to State: '" + state.ToString() + "'");
            else
                HTTPManager.Logger.Information("HubConnection", "SetState - from State: '" + this.State.ToString() + "' to State: '" + state.ToString() + "' errorReason: '" + errorReason + "'");

            if (this.State == state)
                return;

            var previousState = this.State;

            this.State = state;

            switch (state)
            {
                case ConnectionStates.Initial:
                case ConnectionStates.Authenticating:
                case ConnectionStates.Negotiating:
                case ConnectionStates.CloseInitiated:
                    break;

                case ConnectionStates.Reconnecting:
                    HTTPManager.Heartbeats.Subscribe(this);
                    break;

                case ConnectionStates.Connected:
                    // If reconnectStartTime isn't its default value we reconnected
                    if (this.reconnectStartTime != DateTime.MinValue)
                    {
                        try
                        {
                            if (this.OnReconnected != null)
                                this.OnReconnected(this);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "OnReconnected", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (this.OnConnected != null)
                                this.OnConnected(this);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "Exception in OnConnected user code!", ex);
                        }
                    }

                    HTTPManager.Heartbeats.Subscribe(this);
                    this.lastMessageSent = DateTime.UtcNow;

                    // Clean up reconnect related fields
                    this.currentContext = new RetryContext();
                    this.reconnectStartTime = DateTime.MinValue;
                    this.reconnectAt = DateTime.MinValue;

                    break;

                case ConnectionStates.Closed:
                    // No errorReason? It's an expected closure.
                    if (string.IsNullOrEmpty(errorReason))
                    {
                        if (this.OnClosed != null)
                        {
                            try
                            {
                                this.OnClosed(this);
                            }
                            catch(Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "Exception in OnClosed user code!", ex);
                            }
                        }
                    }
                    else
                    {
                        // If possible, try to reconnect
                        if (this.ReconnectPolicy != null && (previousState == ConnectionStates.Connected || this.reconnectStartTime != DateTime.MinValue))
                        {
                            // It's the first attempt after a successful connection
                            if (this.reconnectStartTime == DateTime.MinValue)
                            {
                                this.reconnectStartTime = DateTime.UtcNow;

                                try
                                {
                                    if (this.OnReconnecting != null)
                                        this.OnReconnecting(this, errorReason);
                                }
                                catch (Exception ex)
                                {
                                    HTTPManager.Logger.Exception("HubConnection", "SetState - ConnectionStates.Reconnecting", ex);
                                }
                            }

                            RetryContext context = new RetryContext
                            {
                                ElapsedTime = DateTime.UtcNow - this.reconnectStartTime,
                                PreviousRetryCount = this.currentContext.PreviousRetryCount,
                                RetryReason = errorReason
                            };

                            TimeSpan? nextAttempt = null;
                            try
                            {
                                nextAttempt = this.ReconnectPolicy.GetNextRetryDelay(context);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "ReconnectPolicy.GetNextRetryDelay", ex);
                            }

                            // No more reconnect attempt, we are closing
                            if (nextAttempt == null)
                            {
                                HTTPManager.Logger.Warning("HubConnecction", "No more reconnect attempt!");

                                // Clean up everything
                                this.currentContext = new RetryContext();
                                this.reconnectStartTime = DateTime.MinValue;
                                this.reconnectAt = DateTime.MinValue;
                            }
                            else
                            {
                                HTTPManager.Logger.Information("HubConnecction", "Next reconnect attempt after " + nextAttempt.Value.ToString());

                                this.currentContext = context;
                                this.currentContext.PreviousRetryCount += 1;

                                this.reconnectAt = DateTime.UtcNow + nextAttempt.Value;

                                this.SetState(ConnectionStates.Reconnecting);

                                return;
                            }
                        }

                        if (this.OnError != null)
                        {
                            try
                            {
                                this.OnError(this, errorReason);
                            }
                            catch(Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "Exception in OnError user code!", ex);
                            }
                        }
                    }

                    HTTPManager.Heartbeats.Unsubscribe(this);
                    break;
            }
        }

        void BestHTTP.Extensions.IHeartbeat.OnHeartbeatUpdate(TimeSpan dif)
        {
            switch (this.State)
            {
                case ConnectionStates.Connected:
                    if (this.Options.PingInterval != TimeSpan.Zero && DateTime.UtcNow - this.lastMessageSent >= this.Options.PingInterval)
                        SendMessage(new Message() { type = MessageTypes.Ping });
                    break;

                case ConnectionStates.Reconnecting:
                    if (DateTime.UtcNow >= this.reconnectAt)
                    {
                        HTTPManager.Heartbeats.Unsubscribe(this);
                        this.StartConnect();
                    }
                    break;
            }
        }
    }
}

#endif