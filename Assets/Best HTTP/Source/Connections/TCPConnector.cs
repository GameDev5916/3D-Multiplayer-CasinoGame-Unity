#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using BestHTTP.Extensions;

#if !NETFX_CORE || UNITY_EDITOR
using System.Net.Security;
#endif

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls;
#endif

#if NETFX_CORE
    using System.Threading.Tasks;
    using Windows.Networking.Sockets;

    using TcpClient = BestHTTP.PlatformSupport.TcpClient.WinRT.TcpClient;

    //Disable CD4014: Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
#pragma warning disable 4014
#else
using TcpClient = BestHTTP.PlatformSupport.TcpClient.General.TcpClient;
#endif

namespace BestHTTP.Connections
{
    public sealed class TCPConnector : IDisposable
    {
        public bool IsConnected { get { return this.Client != null && this.Client.Connected; } }

        public string NegotiatedProtocol { get; private set; }

        public TcpClient Client { get; private set; }

        public Stream Stream { get; private set; }

        public bool LeaveOpen { get; set; }

        public void Connect(HTTPRequest request)
        {
            string negotiatedProtocol = HTTPProtocolFactory.W3C_HTTP1;

            Uri uri =
#if !BESTHTTP_DISABLE_PROXY
                request.HasProxy ? request.Proxy.Address :
#endif
                request.CurrentUri;

            #region TCP Connection

            if (Client == null)
                Client = new TcpClient();

            if (!Client.Connected)
            {
                Client.ConnectTimeout = request.ConnectTimeout;

#if NETFX_CORE
                Client.UseHTTPSProtocol =
#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
                    !Request.UseAlternateSSL &&
#endif
                    HTTPProtocolFactory.IsSecureProtocol(uri);
#endif

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("TCPConnector",
                        $"'{request.CurrentUri.ToString()}' - Connecting to {uri.Host}:{uri.Port.ToString()}");

#if !NETFX_CORE && (!UNITY_WEBGL || UNITY_EDITOR)
                Client.SendBufferSize = HTTPManager.SendBufferSize;
                Client.ReceiveBufferSize = HTTPManager.ReceiveBufferSize;

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("TCPConnector",
                        $"'{request.CurrentUri.ToString()}' - Buffer sizes - Send: {Client.SendBufferSize.ToString()} Receive: {Client.ReceiveBufferSize.ToString()} Blocking: {Client.Client.Blocking.ToString()}");
#endif

                Client.Connect(uri.Host, uri.Port);

                if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                    HTTPManager.Logger.Information("TCPConnector", "Connected to " + uri.Host + ":" + uri.Port.ToString());
            }
            else if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                HTTPManager.Logger.Information("TCPConnector", "Already connected to " + uri.Host + ":" + uri.Port.ToString());

            #endregion

            if (Stream == null)
            {
                bool isSecure = HTTPProtocolFactory.IsSecureProtocol(request.CurrentUri);

                Stream = Client.GetStream();
                /*if (Stream.CanTimeout)
                    Stream.ReadTimeout = Stream.WriteTimeout = (int)Request.Timeout.TotalMilliseconds;*/


#if !BESTHTTP_DISABLE_PROXY
                if (request.Proxy != null)
                    request.Proxy.Connect(this.Stream, request);
#endif

                // We have to use Request.CurrentUri here, because uri can be a proxy uri with a different protocol
                if (isSecure)
                {
                    #region SSL Upgrade

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
                    if (request.UseAlternateSSL)
                    {
                        var handler = new TlsClientProtocol(Client.GetStream(), new BestHTTP.SecureProtocol.Org.BouncyCastle.Security.SecureRandom());

                        // http://tools.ietf.org/html/rfc3546#section-3.1
                        // -It is RECOMMENDED that clients include an extension of type "server_name" in the client hello whenever they locate a server by a supported name type.
                        // -Literal IPv4 and IPv6 addresses are not permitted in "HostName".

                        // User-defined list has a higher priority
                        List<string> hostNames = request.CustomTLSServerNameList;

                        // If there's no user defined one and the host isn't an IP address, add the default one
                        if ((hostNames == null || hostNames.Count == 0) && !request.CurrentUri.IsHostIsAnIPAddress())
                        {
                            hostNames = new List<string>(1);
                            hostNames.Add(request.CurrentUri.Host);
                        }

                        SupportedProtocols protocol = request.ProtocolHandler == SupportedProtocols.Unknown ? HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) : request.ProtocolHandler;
                        List<string> protocols = new List<string>();
#if !BESTHTTP_DISABLE_HTTP2
                        if (protocol == SupportedProtocols.HTTP)
                        {
                            // http/2 over tls (https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml#alpn-protocol-ids)
                            protocols.Add(HTTPProtocolFactory.W3C_HTTP2);
                        }
#endif

                        protocols.Add(HTTPProtocolFactory.W3C_HTTP1);

                        var tlsClient = new LegacyTlsClient(request.CurrentUri,
                                                            request.CustomCertificateVerifyer == null ? new AlwaysValidVerifyer() : request.CustomCertificateVerifyer,
                                                            request.CustomClientCredentialsProvider,
                                                            hostNames,
                                                            protocols);
                        handler.Connect(tlsClient);

                        if (!string.IsNullOrEmpty(tlsClient.ServerSupportedProtocol))
                            negotiatedProtocol = tlsClient.ServerSupportedProtocol;

                        Stream = handler.Stream;
                    }
                    else
#endif
                    {
#if !NETFX_CORE
                        SslStream sslStream = new SslStream(Client.GetStream(), false, (sender, cert, chain, errors) =>
                        {
                            return request.CallCustomCertificationValidator(cert, chain);
                        });

                        if (!sslStream.IsAuthenticated)
                            sslStream.AuthenticateAsClient(request.CurrentUri.Host);
                        Stream = sslStream;
#else
                        Stream = Client.GetStream();
#endif
                    }

                    #endregion
                }
            }

            this.NegotiatedProtocol = negotiatedProtocol;
        }

        public void Close()
        {
            if (Client != null && !this.LeaveOpen)
            {
                try
                {
                    Client.Close();
                }
                catch
                {
                }
                finally
                {
                    Stream = null;
                    Client = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Close();
        }

        ~TCPConnector()
        {
            Dispose(false);
        }
    }
}
#endif