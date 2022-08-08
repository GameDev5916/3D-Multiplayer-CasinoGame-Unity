#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls
{
    public interface DatagramTransport
        : TlsCloseable
    {
        /// <exception cref="IOException"/>
        int GetReceiveLimit();

        /// <exception cref="IOException"/>
        int GetSendLimit();

        /// <exception cref="IOException"/>
        int Receive(byte[] buf, int off, int len, int waitMillis);

        /// <exception cref="IOException"/>
        void Send(byte[] buf, int off, int len);
    }
}
#pragma warning restore
#endif
