using System;
#if !BESTHTTP_DISABLE_CACHING
using BestHTTP.Caching;
#endif

using BestHTTP.Core;

using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP
{
    public enum ShutdownTypes
    {
        Running,
        Gentle,
        Immediate
    }

    /// <summary>
    ///
    /// </summary>
    public static partial class HTTPManager
    {
        // Static constructor. Setup default values
        static HTTPManager()
        {
            MaxConnectionPerServer = 6;
            KeepAliveDefaultValue = true;
            MaxPathLength = 255;
            MaxConnectionIdleTime = TimeSpan.FromSeconds(20);

#if !BESTHTTP_DISABLE_COOKIES
#if UNITY_WEBGL
            // Under webgl when IsCookiesEnabled is true, it will set the withCredentials flag for the XmlHTTPRequest
            //  and that's different from the default behavior.
            // https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/withCredentials
            IsCookiesEnabled = false;
#else
            IsCookiesEnabled = true;
#endif
#endif

            CookieJarSize = 10 * 1024 * 1024;
            EnablePrivateBrowsing = false;
            ConnectTimeout = TimeSpan.FromSeconds(20);
            RequestTimeout = TimeSpan.FromSeconds(60);

            // Set the default logger mechanism
            logger = new BestHTTP.Logger.DefaultLogger();

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
            DefaultCertificateVerifyer = null;
            UseAlternateSSLDefaultValue = true;
#endif

#if NETFX_CORE
            IOService = new PlatformSupport.FileSystem.NETFXCOREIOService();
#elif UNITY_WEBGL && !UNITY_EDITOR
            IOService = new PlatformSupport.FileSystem.WebGLIOService();
#else
            IOService = new PlatformSupport.FileSystem.DefaultIOService();
#endif
        }

#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2
        /// <summary>
        /// HTTP/2 settings
        /// </summary>
        public static Connections.HTTP2.HTTP2PluginSettings HTTP2Settings = new Connections.HTTP2.HTTP2PluginSettings();
#endif

#region Global Options

        /// <summary>
        /// The maximum active TCP connections that the client will maintain to a server. Default value is 4. Minimum value is 1.
        /// </summary>
        public static byte MaxConnectionPerServer
        {
            get{ return maxConnectionPerServer; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("MaxConnectionPerServer must be greater than 0!");

                bool isGrowing = value > maxConnectionPerServer;
                maxConnectionPerServer = value;

                // If the allowed connections per server is growing, go through all hosts and try to send out queueud requests.
                if (isGrowing)
                    HostManager.TryToSendQueuedRequests();
            }
        }
        private static byte maxConnectionPerServer;

        /// <summary>
        /// Default value of a HTTP request's IsKeepAlive value. Default value is true. If you make rare request to the server it should be changed to false.
        /// </summary>
        public static bool KeepAliveDefaultValue { get; set; }

#if !BESTHTTP_DISABLE_CACHING
        /// <summary>
        /// Set to true, if caching is prohibited.
        /// </summary>
        public static bool IsCachingDisabled { get; set; }
#endif

        /// <summary>
        /// How many time must be passed to destroy that connection after a connection finished its last request. Its default value is 20 seconds.
        /// </summary>
        public static TimeSpan MaxConnectionIdleTime { get; set; }

#if !BESTHTTP_DISABLE_COOKIES
        /// <summary>
        /// Set to false to disable all Cookie. It's default value is true.
        /// </summary>
        public static bool IsCookiesEnabled { get; set; }
#endif

        /// <summary>
        /// Size of the Cookie Jar in bytes. It's default value is 10485760 (10 MB).
        /// </summary>
        public static uint CookieJarSize { get; set; }

        /// <summary>
        /// If this property is set to true, then new cookies treated as session cookies and these cookies are not saved to disk. Its default value is false;
        /// </summary>
        public static bool EnablePrivateBrowsing { get; set; }

        /// <summary>
        /// Global, default value of the HTTPRequest's ConnectTimeout property. If set to TimeSpan.Zero or lower, no connect timeout logic is executed. Default value is 20 seconds.
        /// </summary>
        public static TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// Global, default value of the HTTPRequest's Timeout property. Default value is 60 seconds.
        /// </summary>
        public static TimeSpan RequestTimeout { get; set; }

#if !(BESTHTTP_DISABLE_CACHING && BESTHTTP_DISABLE_COOKIES)
        /// <summary>
        /// By default the plugin will save all cache and cookie data under the path returned by Application.persistentDataPath.
        /// You can assign a function to this delegate to return a custom root path to define a new path.
        /// <remarks>This delegate will be called on a non Unity thread!</remarks>
        /// </summary>
        public static System.Func<string> RootCacheFolderProvider { get; set; }
#endif

#if !BESTHTTP_DISABLE_PROXY
        /// <summary>
        /// The global, default proxy for all HTTPRequests. The HTTPRequest's Proxy still can be changed per-request. Default value is null.
        /// </summary>
        public static Proxy Proxy { get; set; }
#endif

        /// <summary>
        /// Heartbeat manager to use less threads in the plugin. The heartbeat updates are called from the OnUpdate function.
        /// </summary>
        public static HeartbeatManager Heartbeats
        {
            get
            {
                if (heartbeats == null)
                    heartbeats = new HeartbeatManager();
                return heartbeats;
            }
        }
        private static HeartbeatManager heartbeats;

        /// <summary>
        /// A basic BestHTTP.Logger.ILogger implementation to be able to log intelligently additional informations about the plugin's internal mechanism.
        /// </summary>
        public static BestHTTP.Logger.ILogger Logger
        {
            get
            {
                // Make sure that it has a valid logger instance.
                if (logger == null)
                {
                    logger = new DefaultLogger();
                    logger.Level = Loglevels.None;
                }

                return logger;
            }

            set { logger = value; }
        }
        private static BestHTTP.Logger.ILogger logger;

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
        /// <summary>
        /// The default ICertificateVerifyer implementation that the plugin will use when the request's UseAlternateSSL property is set to true.
        /// </summary>
        public static SecureProtocol.Org.BouncyCastle.Crypto.Tls.ICertificateVerifyer DefaultCertificateVerifyer { get; set; }

        /// <summary>
        /// The default IClientCredentialsProvider implementation that the plugin will use when the request's UseAlternateSSL property is set to true.
        /// </summary>
        public static SecureProtocol.Org.BouncyCastle.Crypto.Tls.IClientCredentialsProvider DefaultClientCredentialsProvider { get; set; }

        /// <summary>
        /// The default value for the HTTPRequest's UseAlternateSSL property.
        /// </summary>
        public static bool UseAlternateSSLDefaultValue { get; set; }
#endif

#if !NETFX_CORE
        public static Func<HTTPRequest, System.Security.Cryptography.X509Certificates.X509Certificate, System.Security.Cryptography.X509Certificates.X509Chain, bool> DefaultCertificationValidator { get; set; }
#endif

        /// <summary>
        /// TCP Client's send buffer size.
        /// </summary>
        public static int SendBufferSize = 65 * 1024;

        /// <summary>
        /// TCP Client's receive buffer size.
        /// </summary>
        public static int ReceiveBufferSize = 65 * 1024;

        /// <summary>
        /// An IIOService implementation to handle filesystem operations.
        /// </summary>
        public static PlatformSupport.FileSystem.IIOService IOService;

        /// <summary>
        /// On most systems the maximum length of a path is around 255 character. If a cache entity's path is longer than this value it doesn't get cached. There no platform independent API to query the exact value on the current system, but it's
        /// exposed here and can be overridden. It's default value is 255.
        /// </summary>
        internal static int MaxPathLength { get; set; }

        /// <summary>
        /// User-agent string that will be sent with each requests.
        /// </summary>
        public static string UserAgent = "BestHTTP 2.0.0";

#endregion

#region Manager variables

        internal static bool IsQuitting { get; private set; }

        private static bool IsSetupCalled;

#endregion

#region Public Interface

        public static void Setup()
        {
            if (IsSetupCalled)
                return;
            IsSetupCalled = true;

            HTTPManager.Logger.Information("HTTPManager", "Setup called!");

            IsQuitting = false;
            HTTPUpdateDelegator.CheckInstance();

#if !BESTHTTP_DISABLE_CACHING
            HTTPCacheService.CheckSetup();
#endif

#if !BESTHTTP_DISABLE_COOKIES
            Cookies.CookieJar.SetupFolder();
            Cookies.CookieJar.Load();
#endif

            HostManager.Load();
        }

        public static HTTPRequest SendRequest(string url, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), HTTPMethods.Get, callback));
        }

        public static HTTPRequest SendRequest(string url, HTTPMethods methodType, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), methodType, callback));
        }

        public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, callback));
        }

        public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, disableCache, callback));
        }

        public static HTTPRequest SendRequest(HTTPRequest request)
        {
            if (request.IsCancellationRequested || IsQuitting)
                return request;

            if (!IsSetupCalled)
                Setup();

            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(request, RequestEvents.Resend));

            return request;
        }

#endregion

#region Internal Helper Functions

#if !(BESTHTTP_DISABLE_CACHING && BESTHTTP_DISABLE_COOKIES)
        /// <summary>
        /// Will return where the various caches should be saved.
        /// </summary>
        internal static string GetRootCacheFolder()
        {
            try
            {
                if (RootCacheFolderProvider != null)
                    return RootCacheFolderProvider();
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("HTTPManager", "GetRootCacheFolder", ex);
            }

#if NETFX_CORE
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
            return UnityEngine.Application.persistentDataPath;
#endif
        }
#endif

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Reset()
        {
            IsSetupCalled = false;
            HTTPManager.Logger.Information("HTTPManager", "Reset called!");
        }

        #endregion

        #region MonoBehaviour Events (Called from HTTPUpdateDelegator)

        /// <summary>
        /// Update function that should be called regularly from a Unity event(Update, LateUpdate). Callbacks are dispatched from this function.
        /// </summary>
        public static void OnUpdate()
        {
            RequestEventHelper.ProcessQueue();
            ConnectionEventHelper.ProcessQueue();
            ProtocolEventHelper.ProcessQueue();
            PluginEventHelper.ProcessQueue();

            BestHTTP.Extensions.Timer.Process();

            if (heartbeats != null)
                heartbeats.Update();

            BufferPool.Maintain();
        }

        public static void OnQuit()
        {
            HTTPManager.Logger.Information("HTTPManager", "OnQuit called!");

            IsQuitting = true;

            AbortAll();

#if !BESTHTTP_DISABLE_CACHING
            PluginEventHelper.EnqueuePluginEvent(new PluginEventInfo(PluginEvents.SaveCacheLibrary));
#endif

#if !BESTHTTP_DISABLE_COOKIES
            PluginEventHelper.EnqueuePluginEvent(new PluginEventInfo(PluginEvents.SaveCookieLibrary));
#endif

            OnUpdate();

            HostManager.Clear();
        }

        public static void AbortAll()
        {
            HTTPManager.Logger.Information("HTTPManager", "AbortAll called!");

            // This is an immediate shutdown request!

            RequestEventHelper.Clear();
            ConnectionEventHelper.Clear();
            PluginEventHelper.Clear();
            ProtocolEventHelper.Clear();

            HostManager.Shutdown();

            ProtocolEventHelper.CancelActiveProtocols();
        }

#endregion
    }
}