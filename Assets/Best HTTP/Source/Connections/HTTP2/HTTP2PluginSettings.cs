#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2
using System;

namespace BestHTTP.Connections.HTTP2
{
    public sealed class HTTP2PluginSettings
    {
        /// <summary>
        /// Maximum size of the HPACK header table.
        /// </summary>
        public UInt32 HeaderTableSize = 4096; // Spec default: 4096

        /// <summary>
        /// Maximum concurrent http2 stream on http2 connection will allow. Its default value is 128;
        /// </summary>
        public UInt32 MaxConcurrentStreams = 128; // Spec default: not defined

        /// <summary>
        /// Initial window size of a http2 stream. Its default value is 10 Mb (10 * 1024 * 1024).
        /// </summary>
        public UInt32 InitialStreamWindowSize = 10 * 1024 * 1024; // Spec default: 65535

        /// <summary>
        /// Global window size of a http/2 connection. Its default value is the maximum possible value on 31 bits.
        /// </summary>
        public UInt32 InitialConnectionWindowSize = HTTP2Handler.MaxValueFor31Bits; // Spec default: 65535

        /// <summary>
        /// Maximum size of a http2 frame.
        /// </summary>
        public UInt32 MaxFrameSize = 16384; // 16384 spec def.

        /// <summary>
        /// Not used.
        /// </summary>
        public UInt32 MaxHeaderListSize = UInt32.MaxValue; // Spec default: infinite

        /// <summary>
        /// With HTTP/2 only one connection will be open so we can can keep it open longer as we hope it will be resued more.
        /// </summary>
        public TimeSpan MaxIdleTime = TimeSpan.FromSeconds(120);
    }
}
#endif