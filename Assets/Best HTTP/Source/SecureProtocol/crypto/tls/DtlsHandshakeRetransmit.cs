#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls
{
    internal interface DtlsHandshakeRetransmit
    {
        /// <exception cref="IOException"/>
        void ReceivedHandshakeRecord(int epoch, byte[] buf, int off, int len);
    }
}
#pragma warning restore
#endif
