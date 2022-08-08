#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using BestHTTP.Connections;

namespace BestHTTP.Core
{
    public interface IHTTPRequestHandler : IDisposable
    {
        bool HasCustomRequestProcessor { get; }

        KeepAliveHeader KeepAlive { get; }

        bool CanProcessMultiple { get; }

        ShutdownTypes ShutdownType { get; }

        void Process(HTTPRequest request);

        void RunHandler();

        /// <summary>
        /// An immediate shutdown request that called only on application closure.
        /// </summary>
        void Shutdown(ShutdownTypes type);
    }
}

#endif