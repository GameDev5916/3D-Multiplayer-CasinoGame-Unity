#if !BESTHTTP_DISABLE_SIGNALR_CORE && !BESTHTTP_DISABLE_WEBSOCKET
using System;
using System.Collections.Generic;

namespace BestHTTP.SignalRCore
{
    public enum TransportTypes
    {
        WebSocket
    }

    public enum TransferModes
    {
        Binary,
        Text
    }

    public enum TransportStates
    {
        Initial,
        Connecting,
        Connected,
        Closing,
        Failed,
        Closed
    }

    /// <summary>
    /// Possible states of a HubConnection
    /// </summary>
    public enum ConnectionStates
    {
        Initial,
        Authenticating,
        Negotiating,
        Redirected,
        Reconnecting,
        Connected,
        CloseInitiated,
        Closed
    }

    public interface ITransport
    {
        TransferModes TransferMode { get; }
        TransportTypes TransportType { get; }
        TransportStates State { get; }

        string ErrorReason { get; }

        event Action<TransportStates, TransportStates> OnStateChanged;

        void StartConnect();
        void StartClose();

        void Send(byte[] msg);
    }

    public interface IEncoder
    {
        string Name { get; }

        string EncodeAsText<T>(T value);
        T DecodeAs<T>(string text);

        byte[] EncodeAsBinary<T>(T value);
        T DecodeAs<T>(byte[] data);

        object ConvertTo(Type toType, object obj);
    }

    public sealed class StreamItemContainer<T>
    {
        public readonly long id;

        public List<T> Items { get; private set; }
        public T LastAdded { get; private set; }

        public bool IsCanceled;

        public StreamItemContainer(long _id)
        {
            this.id = _id;
            this.Items = new List<T>();
        }

        public void AddItem(T item)
        {
            if (this.Items == null)
                this.Items = new List<T>();

            this.Items.Add(item);
            this.LastAdded = item;
        }
    }

    internal struct CallbackDescriptor
    {
        public readonly Type[] ParamTypes;
        public readonly Action<object[]> Callback;
        public CallbackDescriptor(Type[] paramTypes, Action<object[]> callback)
        {
            this.ParamTypes = paramTypes;
            this.Callback = callback;
        }
    }

    internal sealed class Subscription
    {
        public List<CallbackDescriptor> callbacks = new List<CallbackDescriptor>(1);
        public System.Threading.ReaderWriterLockSlim lockSlim = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

        public void Add(Type[] paramTypes, Action<object[]> callback)
        {
            lockSlim.EnterWriteLock();
            try
            {
                this.callbacks.Add(new CallbackDescriptor(paramTypes, callback));
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        public void Remove(Action<object[]> callback)
        {
            lockSlim.EnterWriteLock();
            try
            {
                int idx = -1;
                for (int i = 0; i < this.callbacks.Count && idx == -1; ++i)
                    if (this.callbacks[i].Callback == callback)
                        idx = i;

                if (idx != -1)
                    this.callbacks.RemoveAt(idx);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }
    }

    public sealed class HubOptions
    {
        /// <summary>
        /// When this is set to true, the plugin will skip the negotiation request if the PreferedTransport is WebSocket. Its default value is false.
        /// </summary>
        public bool SkipNegotiation { get; set; }

        /// <summary>
        /// The preferred transport to choose when more than one available. Its default value is TransportTypes.WebSocket.
        /// </summary>
        public TransportTypes PreferedTransport { get; set; }

        /// <summary>
        /// A ping message is only sent if the interval has elapsed without a message being sent. Its default value is 15 seconds.
        /// </summary>
        public TimeSpan PingInterval { get; set; }

        /// <summary>
        /// The maximum count of redirect negoitiation result that the plugin will follow. Its default value is 100.
        /// </summary>
        public int MaxRedirects { get; set; }

        public HubOptions()
        {
            this.SkipNegotiation = false;
            this.PreferedTransport = TransportTypes.WebSocket;
            this.PingInterval = TimeSpan.FromSeconds(15);
            this.MaxRedirects = 100;
        }
    }

    public interface IRetryPolicy
    {
        /// <summary>
        /// This function must return with a delay time to wait until a new connection attempt, or null to do not do another one.
        /// </summary>
        TimeSpan? GetNextRetryDelay(RetryContext context);
    }

    public struct RetryContext
    {
        /// <summary>
        /// Previous reconnect attempts. A successful connection sets it back to zero.
        /// </summary>
        public uint PreviousRetryCount;

        /// <summary>
        /// Elapsed time since the original connection error.
        /// </summary>
        public TimeSpan ElapsedTime;

        /// <summary>
        /// String representation of the connection error.
        /// </summary>
        public string RetryReason;
    }

    public sealed class DefaultRetryPolicy : IRetryPolicy
    {
        private static TimeSpan?[] DefaultBackoffTimes = new TimeSpan?[]
        {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            null
        };

        private TimeSpan?[] backoffTimes;

        public DefaultRetryPolicy()
        {
            this.backoffTimes = DefaultBackoffTimes;
        }

        public DefaultRetryPolicy(TimeSpan?[] customBackoffTimes)
        {
            this.backoffTimes = customBackoffTimes;
        }

        public TimeSpan? GetNextRetryDelay(RetryContext context)
        {
            if (context.PreviousRetryCount >= this.backoffTimes.Length)
                return null;

            return this.backoffTimes[context.PreviousRetryCount];
        }
    }
}
#endif