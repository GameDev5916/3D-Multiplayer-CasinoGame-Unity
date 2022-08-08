#if !BESTHTTP_DISABLE_SIGNALR_CORE && !BESTHTTP_DISABLE_WEBSOCKET
namespace BestHTTP.SignalRCore.Messages
{
    public enum MessageTypes : int
    {
        /// <summary>
        /// This is a made up message type, for easier handshake handling.
        /// </summary>
        Handshake  = 0,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#invocation-message-encoding
        /// </summary>
        Invocation = 1,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#streamitem-message-encoding
        /// </summary>
        StreamItem = 2,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#completion-message-encoding
        /// </summary>
        Completion = 3,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#streaminvocation-message-encoding
        /// </summary>
        StreamInvocation = 4,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#cancelinvocation-message-encoding
        /// </summary>
        CancelInvocation = 5,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#ping-message-encoding
        /// </summary>
        Ping = 6,

        /// <summary>
        /// https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#close-message-encoding
        /// </summary>
        Close = 7
    }

    public struct Message
    {
        public MessageTypes type;
        public string invocationId;
        public bool nonblocking;
        public string target;
        public object[] arguments;
        public string[] streamIds;
        public object item;
        public object result;
        public string error;

        public override string ToString()
        {
            switch (this.type)
            {
                case MessageTypes.Invocation:
                    return
                        $"[Invocation Id: {this.invocationId}, Target: '{this.target}', Argument count: {(this.arguments != null ? this.arguments.Length : 0)}, Stream Ids: {(this.streamIds != null ? this.streamIds.Length : 0)}]";
                case MessageTypes.StreamItem:
                    return $"[StreamItem Id: {this.invocationId}, Item: {this.item.ToString()}]";
                case MessageTypes.Completion:
                    return $"[Completion Id: {this.invocationId}, Result: {this.result}, Error: '{this.error}']";
                case MessageTypes.StreamInvocation:
                    return
                        $"[StreamInvocation Id: {this.invocationId}, Target: '{this.target}', Argument count: {(this.arguments != null ? this.arguments.Length : 0)}]";
                case MessageTypes.CancelInvocation:
                    return $"[CancelInvocation Id: {this.invocationId}]";
                case MessageTypes.Ping:
                    return "[Ping]";
                case MessageTypes.Close:
                    return string.IsNullOrEmpty(this.error) ? "[Close]" : $"[Close {this.error}]";
                default:
                    return "Unknown message! Type: " + this.type;
            }
        }
    }
}
#endif