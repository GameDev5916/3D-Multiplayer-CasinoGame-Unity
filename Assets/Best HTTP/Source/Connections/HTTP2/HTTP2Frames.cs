#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Generic;

namespace BestHTTP.Connections.HTTP2
{
    // https://httpwg.org/specs/rfc7540.html#iana-frames
    internal enum HTTP2FrameTypes : byte
    {
        DATA = 0x00,
        HEADERS = 0x01,
        PRIORITY = 0x02,
        RST_STREAM = 0x03,
        SETTINGS = 0x04,
        PUSH_PROMISE = 0x05,
        PING = 0x06,
        GOAWAY = 0x07,
        WINDOW_UPDATE = 0x08,
        CONTINUATION = 0x09,

        // https://tools.ietf.org/html/rfc7838#section-4
        ALT_SVC = 0x0A
    }

    [Flags]
    internal enum HTTP2DataFlags : byte
    {
        None = 0x00,
        END_STREAM = 0x01,
        PADDED = 0x08,
    }

    [Flags]
    internal enum HTTP2HeadersFlags : byte
    {
        None = 0x00,
        END_STREAM = 0x01,
        END_HEADERS = 0x04,
        PADDED = 0x08,
        PRIORITY = 0x20,
    }

    [Flags]
    internal enum HTTP2SettingsFlags : byte
    {
        None = 0x00,
        ACK = 0x01,
    }

    [Flags]
    internal enum HTTP2PushPromiseFlags : byte
    {
        None = 0x00,
        END_HEADERS = 0x04,
        PADDED = 0x08,
    }

    [Flags]
    internal enum HTTP2PingFlags : byte
    {
        None = 0x00,
        ACK = 0x01,
    }

    [Flags]
    internal enum HTTP2ContinuationFlags : byte
    {
        None = 0x00,
        END_HEADERS = 0x04,
    }

    internal struct HTTP2FrameHeaderAndPayload
    {
        public UInt32 PayloadLength;
        public HTTP2FrameTypes Type;
        public byte Flags;
        public UInt32 StreamId;
        public byte[] Payload;

        public UInt32 PayloadOffset;
        public bool DontUseMemPool;

        public override string ToString()
        {
            return
                $"[HTTP2FrameHeaderAndPayload Length: {this.PayloadLength}, Type: {this.Type}, Flags: {this.Flags.ToBinaryStr()}, StreamId: {this.StreamId}, PayloadOffset: {this.PayloadOffset}, DontUseMemPool: {this.DontUseMemPool}]";
        }
    }

    internal struct HTTP2SettingsFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2SettingsFlags Flags { get { return (HTTP2SettingsFlags)this.Header.Flags; } }
        public List<KeyValuePair<HTTP2Settings, UInt32>> Settings;

        public HTTP2SettingsFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.Settings = null;
        }

        public override string ToString()
        {
            string settings = null;
            if (this.Settings != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("[");
                foreach (var kvp in this.Settings)
                    sb.AppendFormat("[{0}: {1}]", kvp.Key, kvp.Value);
                sb.Append("]");

                settings = sb.ToString();
            }

            return
                $"[HTTP2SettingsFrame Header: {this.Header.ToString()}, Flags: {this.Flags}, Settings: {settings ?? "Empty"}]";
        }
    }

    internal struct HTTP2DataFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2DataFlags Flags { get { return (HTTP2DataFlags)this.Header.Flags; } }

        public byte? PadLength;
        public UInt32 DataIdx;
        public byte[] Data;
        public uint DataLength;

        public HTTP2DataFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.PadLength = null;
            this.DataIdx = 0;
            this.Data = null;
            this.DataLength = 0;
        }

        public override string ToString()
        {
            return
                $"[HTTP2DataFrame Header: {this.Header.ToString()}, Flags: {this.Flags}, PadLength: {(this.PadLength == null ? ":Empty" : this.PadLength.Value.ToString())}, DataLength: {this.DataLength}]";
        }
    }

    internal struct HTTP2HeadersFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2HeadersFlags Flags { get { return (HTTP2HeadersFlags)this.Header.Flags; } }

        public byte? PadLength;
        public byte? IsExclusive;
        public UInt32? StreamDependency;
        public byte? Weight;
        public UInt32 HeaderBlockFragmentIdx;
        public byte[] HeaderBlockFragment;
        public UInt32 HeaderBlockFragmentLength;

        public HTTP2HeadersFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.PadLength = null;
            this.IsExclusive = null;
            this.StreamDependency = null;
            this.Weight = null;
            this.HeaderBlockFragmentIdx = 0;
            this.HeaderBlockFragment = null;
            this.HeaderBlockFragmentLength = 0;
        }

        public override string ToString()
        {
            return
                $"[HTTP2HeadersFrame Header: {this.Header.ToString()}, Flags: {this.Flags}, PadLength: {(this.PadLength == null ? ":Empty" : this.PadLength.Value.ToString())}, IsExclusive: {(this.IsExclusive == null ? "Empty" : this.IsExclusive.Value.ToString())}, StreamDependency: {(this.StreamDependency == null ? "Empty" : this.StreamDependency.Value.ToString())}, Weight: {(this.Weight == null ? "Empty" : this.Weight.Value.ToString())}, HeaderBlockFragmentLength: {this.HeaderBlockFragmentLength}]";
        }
    }

    internal struct HTTP2PriorityFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public byte IsExclusive;
        public UInt32 StreamDependency;
        public byte Weight;

        public HTTP2PriorityFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.IsExclusive = 0;
            this.StreamDependency = 0;
            this.Weight = 0;
        }

        public override string ToString()
        {
            return
                $"[HTTP2PriorityFrame Header: {this.Header.ToString()}, IsExclusive: {this.IsExclusive}, StreamDependency: {this.StreamDependency}, Weight: {this.Weight}]";
        }
    }

    internal struct HTTP2RSTStreamFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public UInt32 ErrorCode;
        public HTTP2ErrorCodes Error { get { return (HTTP2ErrorCodes)this.ErrorCode; } }

        public HTTP2RSTStreamFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.ErrorCode = 0;
        }

        public override string ToString()
        {
            return $"[HTTP2RST_StreamFrame Header: {this.Header.ToString()}, Error: {this.Error}({this.ErrorCode})]";
        }
    }

    internal struct HTTP2PushPromiseFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2PushPromiseFlags Flags { get { return (HTTP2PushPromiseFlags)this.Header.Flags; } }

        public byte? PadLength;
        public byte ReservedBit;
        public UInt32 PromisedStreamId;
        public UInt32 HeaderBlockFragmentIdx;
        public byte[] HeaderBlockFragment;
        public UInt32 HeaderBlockFragmentLength;

        public HTTP2PushPromiseFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.PadLength = null;
            this.ReservedBit = 0;
            this.PromisedStreamId = 0;
            this.HeaderBlockFragmentIdx = 0;
            this.HeaderBlockFragment = null;
            this.HeaderBlockFragmentLength = 0;
        }

        public override string ToString()
        {
            return
                $"[HTTP2Push_PromiseFrame Header: {this.Header.ToString()}, Flags: {this.Flags}, PadLength: {(this.PadLength == null ? "Empty" : this.PadLength.Value.ToString())}, ReservedBit: {this.ReservedBit}, PromisedStreamId: {this.PromisedStreamId}, HeaderBlockFragmentLength: {this.HeaderBlockFragmentLength}]";
        }
    }

    internal struct HTTP2PingFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2PingFlags Flags { get { return (HTTP2PingFlags)this.Header.Flags; } }

        public readonly byte[] OpaqueData;
        public readonly byte OpaqueDataLength;

        public HTTP2PingFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.OpaqueData = BufferPool.Get(8, true);
            this.OpaqueDataLength = 8;
        }

        public override string ToString()
        {
            return
                $"[HTTP2PingFrame Header: {this.Header.ToString()}, Flags: {this.Flags}, OpaqueData: {SecureProtocol.Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(this.OpaqueData, 0, this.OpaqueDataLength)}]";
        }
    }

    internal struct HTTP2GoAwayFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2ErrorCodes Error { get { return (HTTP2ErrorCodes)this.ErrorCode; } }

        public byte ReservedBit;
        public UInt32 LastStreamId;
        public UInt32 ErrorCode;
        public byte[] AdditionalDebugData;
        public UInt32 AdditionalDebugDataLength;

        public HTTP2GoAwayFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.ReservedBit = 0;
            this.LastStreamId = 0;
            this.ErrorCode = 0;
            this.AdditionalDebugData = null;
            this.AdditionalDebugDataLength = 0;
        }

        public override string ToString()
        {
            return
                $"[HTTP2GoAwayFrame Header: {this.Header.ToString()}, ReservedBit: {this.ReservedBit}, LastStreamId: {this.LastStreamId}, Error: {this.Error}({this.ErrorCode}), AdditionalDebugData({this.AdditionalDebugDataLength}): {(this.AdditionalDebugData == null ? "Empty" : SecureProtocol.Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(this.AdditionalDebugData, 0, (int) this.AdditionalDebugDataLength))}]";
        }
    }

    internal struct HTTP2WindowUpdateFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public byte ReservedBit;
        public UInt32 WindowSizeIncrement;

        public HTTP2WindowUpdateFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.ReservedBit = 0;
            this.WindowSizeIncrement = 0;
        }

        public override string ToString()
        {
            return
                $"[HTTP2WindowUpdateFrame Header: {this.Header.ToString()}, ReservedBit: {this.ReservedBit}, WindowSizeIncrement: {this.WindowSizeIncrement}]";
        }
    }

    internal struct HTTP2ContinuationFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2ContinuationFlags Flags { get { return (HTTP2ContinuationFlags)this.Header.Flags; } }

        public byte[] HeaderBlockFragment;
        public UInt32 HeaderBlockFragmentLength { get { return this.Header.PayloadLength; } }

        public HTTP2ContinuationFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.HeaderBlockFragment = null;
        }

        public override string ToString()
        {
            return
                $"[HTTP2ContinuationFrame Header: {this.Header.ToString()}, Flags: {this.Flags}, HeaderBlockFragmentLength: {this.HeaderBlockFragmentLength}]";
        }
    }

    /// <summary>
    /// https://tools.ietf.org/html/rfc7838#section-4
    /// </summary>
    internal struct HTTP2AltSVCFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public string Origin;
        public string AltSvcFieldValue;

        public HTTP2AltSVCFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.Origin = null;
            this.AltSvcFieldValue = null;
        }
    }
}

#endif