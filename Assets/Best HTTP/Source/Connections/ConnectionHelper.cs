﻿using System;
using System.Collections.Generic;
using BestHTTP.Authentication;
using BestHTTP.Caching;
using BestHTTP.Cookies;
using BestHTTP.Core;
using BestHTTP.Extensions;

namespace BestHTTP.Connections
{
    /// <summary>
    /// https://tools.ietf.org/html/draft-thomson-hybi-http-timeout-03
    /// Test servers: http://tools.ietf.org/ http://nginx.org/
    /// </summary>
    public sealed class KeepAliveHeader
    {
        /// <summary>
        /// A host sets the value of the "timeout" parameter to the time that the host will allow an idle connection to remain open before it is closed. A connection is idle if no data is sent or received by a host.
        /// </summary>
        public TimeSpan TimeOut { get; private set; }

        /// <summary>
        /// The "max" parameter has been used to indicate the maximum number of requests that would be made on the connection.This parameter is deprecated.Any limit on requests can be enforced by sending "Connection: close" and closing the connection.
        /// </summary>
        public int MaxRequests { get; private set; }

        public void Parse(List<string> headerValues)
        {
            HeaderParser parser = new HeaderParser(headerValues[0]);
            HeaderValue value;
            if (parser.TryGet("timeout", out value) && value.HasValue)
            {
                int intValue = 0;
                if (int.TryParse(value.Value, out intValue))
                    this.TimeOut = TimeSpan.FromSeconds(intValue);
                else
                    this.TimeOut = TimeSpan.MaxValue;
            }

            if (parser.TryGet("max", out value) && value.HasValue)
            {
                int intValue = 0;
                if (int.TryParse("max", out intValue))
                    this.MaxRequests = intValue;
                else
                    this.MaxRequests = int.MaxValue;
            }
        }
    }

    public static class ConnectionHelper
    {
        public static void HandleResponse(string context, HTTPRequest request, out bool resendRequest, out HTTPConnectionStates proposedConnectionState, ref KeepAliveHeader keepAlive)
        {
            resendRequest = false;
            proposedConnectionState = HTTPConnectionStates.Processing;

            if (request.Response != null)
            {
#if !BESTHTTP_DISABLE_COOKIES
                // Try to store cookies before we do anything else, as we may remove the response deleting the cookies as well.
                if (request.IsCookiesEnabled && CookieJar.Set(request.Response))
                    PluginEventHelper.EnqueuePluginEvent(new PluginEventInfo(PluginEvents.SaveCookieLibrary));
#endif

                switch (request.Response.StatusCode)
                {
                    // Not authorized
                    // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.2
                    case 401:
                        {
                            string authHeader = DigestStore.FindBest(request.Response.GetHeaderValues("www-authenticate"));
                            if (!string.IsNullOrEmpty(authHeader))
                            {
                                var digest = DigestStore.GetOrCreate(request.CurrentUri);
                                digest.ParseChallange(authHeader);

                                if (request.Credentials != null && digest.IsUriProtected(request.CurrentUri) && (!request.HasHeader("Authorization") || digest.Stale))
                                    resendRequest = true;
                            }

                            goto default;
                        }

                    // Redirected
                    case 301: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.2
                    case 302: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.3
                    case 307: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.8
                    case 308: // http://tools.ietf.org/html/rfc7238
                        {
                            if (request.RedirectCount >= request.MaxRedirects)
                                goto default;
                            request.RedirectCount++;

                            string location = request.Response.GetFirstHeaderValue("location");
                            if (!string.IsNullOrEmpty(location))
                            {
                                Uri redirectUri = ConnectionHelper.GetRedirectUri(request, location);

                                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                                    HTTPManager.Logger.Verbose("HTTPConnection", string.Format("[{0}] - Redirected to Location: '{1}' redirectUri: '{1}'", context, location, redirectUri));

                                // Let the user to take some control over the redirection
                                if (!request.CallOnBeforeRedirection(redirectUri))
                                {
                                    HTTPManager.Logger.Information("HTTPConnection",
                                        $"[{context}] OnBeforeRedirection returned False");
                                    goto default;
                                }

                                // Remove the previously set Host header.
                                request.RemoveHeader("Host");

                                // Set the Referer header to the last Uri.
                                request.SetHeader("Referer", request.CurrentUri.ToString());

                                // Set the new Uri, the CurrentUri will return this while the IsRedirected property is true
                                request.RedirectUri = redirectUri;

                                // Discard the redirect response, we don't need it any more
                                request.Response = null;

                                request.IsRedirected = true;

                                resendRequest = true;
                            }
                            else
                                throw new Exception(
                                    $"[{context}] Got redirect status({request.Response.StatusCode.ToString()}) without 'location' header!");

                            goto default;
                        }

#if !BESTHTTP_DISABLE_CACHING
                    case 304:
                        if (request.DisableCache)
                            break;

                        if (ConnectionHelper.LoadFromCache(context, request))
                        {
                            HTTPManager.Logger.Verbose("HTTPConnection",
                                $"[{context}] - HandleResponse - Loaded from cache successfully!");
                        }
                        else
                        {
                            HTTPManager.Logger.Verbose("HTTPConnection",
                                $"[{context}] - HandleResponse - Loaded from cache failed!");
                            resendRequest = true;
                        }

                        break;
#endif

                    default:
#if !BESTHTTP_DISABLE_CACHING
                        ConnectionHelper.TryStoreInCache(request);
#endif
                        break;
                }

                // Closing the stream is done manually?
                if (!request.Response.IsClosedManually)
                {
                    // If we have a response and the server telling us that it closed the connection after the message sent to us, then
                    //  we will close the connection too.
                    bool closeByServer = request.Response.HasHeaderWithValue("connection", "close");
                    bool closeByClient = !request.IsKeepAlive;

                    if (closeByServer || closeByClient)
                    {
                        proposedConnectionState = HTTPConnectionStates.Closed;
                    }
                    else if (request.Response != null)
                    {
                        var keepAliveheaderValues = request.Response.GetHeaderValues("keep-alive");
                        if (keepAliveheaderValues != null && keepAliveheaderValues.Count > 0)
                        {
                            if (keepAlive == null)
                                keepAlive = new KeepAliveHeader();
                            keepAlive.Parse(keepAliveheaderValues);
                        }
                    }
                }
            }
        }

#if !BESTHTTP_DISABLE_CACHING
        public static bool LoadFromCache(string context, HTTPRequest request)
        {
            if (request.IsRedirected)
            {
                if (LoadFromCache(context, request, request.RedirectUri))
                    return true;
                else
                {
                    Caching.HTTPCacheService.DeleteEntity(request.RedirectUri);
                }
            }

            bool loaded = LoadFromCache(context, request, request.Uri);
            if (!loaded)
                Caching.HTTPCacheService.DeleteEntity(request.Uri);

            return loaded;
        }

        private static bool LoadFromCache(string context, HTTPRequest request, Uri uri)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTPConnection", $"[{context}] - LoadFromCache for Uri: {uri.ToString()}");

            var cacheEntity = HTTPCacheService.GetEntity(uri);
            if (cacheEntity == null)
            {
                HTTPManager.Logger.Warning("HTTPConnection",
                    $"[{context}] - LoadFromCache for Uri: {uri.ToString()} - Cached entity not found!");
                return false;
            }

            request.Response.CacheFileInfo = cacheEntity;

            try
            {
                int bodyLength;
                using (var cacheStream = cacheEntity.GetBodyStream(out bodyLength))
                {
                    if (cacheStream == null)
                        return false;

                    if (!request.Response.HasHeader("content-length"))
                        request.Response.AddHeader("content-length", bodyLength.ToString());
                    request.Response.IsFromCache = true;

                    if (!request.CacheOnly)
                        request.Response.ReadRaw(cacheStream, bodyLength);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool TryLoadAllFromCache(string context, HTTPRequest request)
        {
            if (!HTTPCacheService.IsCachedEntityExpiresInTheFuture(request))
                return false;

            // We will try read the response from the cache, but if something happens we will fallback to the normal way.
            try
            {
                //Unless specifically constrained by a cache-control (section 14.9) directive, a caching system MAY always store a successful response (see section 13.8) as a cache entity,
                //  MAY return it without validation if it is fresh, and MAY    return it after successful validation.
                // MAY return it without validation if it is fresh!
                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("ConnectionHelper",
                        $"[{context}] - TryLoadAllFromCache - whole response loading from cache");

                request.Response = HTTPCacheService.GetFullResponse(request);

                if (request.Response != null)
                    return true;
            }
            catch
            {
                HTTPCacheService.DeleteEntity(request.CurrentUri);
            }

            return false;
        }

        public static void TryStoreInCache(HTTPRequest request)
        {
            // if UseStreaming && !DisableCache then we already wrote the response to the cache
            if (!request.UseStreaming &&
                !request.DisableCache &&
                request.Response != null &&
                HTTPCacheService.IsSupported &&
                HTTPCacheService.IsCacheble(request.CurrentUri, request.MethodType, request.Response))
            {
                if (request.IsRedirected)
                    HTTPCacheService.Store(request.Uri, request.MethodType, request.Response);
                else
                    HTTPCacheService.Store(request.CurrentUri, request.MethodType, request.Response);

                PluginEventHelper.EnqueuePluginEvent(new PluginEventInfo(PluginEvents.SaveCacheLibrary));
            }
        }
#endif

        public static Uri GetRedirectUri(HTTPRequest request, string location)
        {
            Uri result = null;
            try
            {
                result = new Uri(location);

                if (result.IsFile || result.AbsolutePath == location)
                    result = null;
            }
            catch
            {
                // Sometimes the server sends back only the path and query component of the new uri
                result = null;
            }

            if (result == null)
            {
                var uri = request.Uri;
                var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, location);
                result = builder.Uri;
            }

            return result;
        }
    }
}