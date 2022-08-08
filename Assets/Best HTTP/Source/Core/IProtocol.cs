using System;

namespace BestHTTP.Core
{
    public struct HostConnectionKey
    {
        public readonly string Host;
        public readonly string Connection;

        public HostConnectionKey(string host, string connection)
        {
            this.Host = host;
            this.Connection = connection;
        }

        public override string ToString()
        {
            return $"[HostConnectionKey Host: '{this.Host}', Connection: '{this.Connection}']";
        }
    }

    public interface IProtocol : IDisposable
    {
        HostConnectionKey ConnectionKey { get; }

        bool IsClosed { get; }

        void HandleEvents();

        void CancellationRequested();
    }
}
