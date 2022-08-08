using BestHTTP.Extensions;
using BestHTTP.Logger;
using System;
using System.Collections.Concurrent;

namespace BestHTTP.Core
{
    public enum PluginEvents
    {
#if !BESTHTTP_DISABLE_COOKIES
        SaveCookieLibrary,
#endif

        SaveCacheLibrary,

        AltSvcHeader,
    }

    public
#if CSHARP_7_OR_LATER
        readonly
#endif
        struct PluginEventInfo
    {
        public readonly PluginEvents Event;
        public readonly object Payload;

        public PluginEventInfo(PluginEvents @event)
        {
            this.Event = @event;
            this.Payload = null;
        }

        public PluginEventInfo(PluginEvents @event, object payload)
        {
            this.Event = @event;
            this.Payload = payload;
        }

        public override string ToString()
        {
            return $"[PluginEventInfo Event: {this.Event}]";
        }
    }

    internal static class PluginEventHelper
    {
        private static ConcurrentQueue<PluginEventInfo> pluginEvents = new ConcurrentQueue<PluginEventInfo>();

        public static Action<PluginEventInfo> OnEvent;

        public static void EnqueuePluginEvent(PluginEventInfo @event)
        {
            pluginEvents.Enqueue(@event);
        }

        internal static void Clear()
        {
            pluginEvents.Clear();
        }

        internal static void ProcessQueue()
        {
#if !BESTHTTP_DISABLE_COOKIES
            bool saveCookieLibrary = false;
#endif

#if !BESTHTTP_DISABLE_CACHING
            bool saveCacheLibrary = false;
#endif

            PluginEventInfo pluginEvent;
            while (pluginEvents.TryDequeue(out pluginEvent))
            {
                if (HTTPManager.Logger.Level == Loglevels.All)
                    HTTPManager.Logger.Information("PluginEventHelper", "Processing plugin event: " + pluginEvent.ToString());

                if (OnEvent != null)
                {
                    try
                    {
                        OnEvent(pluginEvent);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("PluginEventHelper", "ProcessQueue", ex);
                    }
                }

                switch (pluginEvent.Event)
                {
#if !BESTHTTP_DISABLE_COOKIES
                    case PluginEvents.SaveCookieLibrary:
                        saveCookieLibrary = true;
                        break;
#endif

#if !BESTHTTP_DISABLE_CACHING
                    case PluginEvents.SaveCacheLibrary:
                        saveCacheLibrary = true;
                        break;
#endif

                    case PluginEvents.AltSvcHeader:
                        AltSvcEventInfo altSvcEventInfo = pluginEvent.Payload as AltSvcEventInfo;
                        HostManager.GetHost(altSvcEventInfo.Host)
                                    .HandleAltSvcHeader(altSvcEventInfo.Response);
                        break;
                }
            }

#if !BESTHTTP_DISABLE_COOKIES
            if (saveCookieLibrary)
                PlatformSupport.Threading.ThreadedRunner.RunShortLiving(Cookies.CookieJar.Persist);
#endif

#if !BESTHTTP_DISABLE_CACHING
            if (saveCacheLibrary)
                PlatformSupport.Threading.ThreadedRunner.RunShortLiving(Caching.HTTPCacheService.SaveLibrary);
#endif
        }
    }

    public sealed class AltSvcEventInfo
    {
        public readonly string Host;
        public readonly HTTPResponse Response;

        public AltSvcEventInfo(string host, HTTPResponse resp)
        {
            this.Host = host;
            this.Response = resp;
        }
    }
}
