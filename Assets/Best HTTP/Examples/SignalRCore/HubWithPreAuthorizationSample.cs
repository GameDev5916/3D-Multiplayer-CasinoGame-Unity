﻿#if !BESTHTTP_DISABLE_SIGNALR_CORE

using BestHTTP.Connections;
using BestHTTP.Examples.Helpers;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
    public sealed class HubWithPreAuthorizationSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [SerializeField]
        private string _hubPath = "/HubWithAuthorization";

        [SerializeField]
        private string _jwtTokenPath = "/generateJwtToken";

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private RectTransform _contentRoot;

        [SerializeField]
        private TextListItem _listItemPrefab;

        [SerializeField]
        private int _maxListItemEntries = 100;

        [SerializeField]
        private Button _connectButton;

        [SerializeField]
        private Button _closeButton;

#pragma warning restore

        // Instance of the HubConnection
        private HubConnection hub;

        protected override void Start()
        {
            base.Start();

            SetButtons(true, false);
        }

        private void OnDestroy()
        {
            if (hub != null)
                hub.StartClose();
        }

        public void OnConnectButton()
        {
            // Server side of this example can be found here:
            // https://github.com/Benedicht/BestHTTP_DemoSite/blob/master/BestHTTP_DemoSite/Hubs/

            // Crete the HubConnection
            hub = new HubConnection(new Uri(base.sampleSelector.BaseURL + this._hubPath), new JsonProtocol(new LitJsonEncoder()));

            hub.AuthenticationProvider = new PreAuthAccessTokenAuthenticator(new Uri(base.sampleSelector.BaseURL + this._jwtTokenPath));

            hub.AuthenticationProvider.OnAuthenticationSucceded += AuthenticationProvider_OnAuthenticationSucceded;
            hub.AuthenticationProvider.OnAuthenticationFailed += AuthenticationProvider_OnAuthenticationFailed;

            // Subscribe to hub events
            hub.OnConnected += Hub_OnConnected;
            hub.OnError += Hub_OnError;
            hub.OnClosed += Hub_OnClosed;

            // And finally start to connect to the server
            hub.StartConnect();

            AddText("StartConnect called");
            SetButtons(false, false);
        }

        public void OnCloseButton()
        {
            if (hub != null)
            {
                hub.StartClose();

                AddText("StartClose called");
                SetButtons(false, false);
            }
        }

        private void AuthenticationProvider_OnAuthenticationSucceded(IAuthenticationProvider provider)
        {
            string str =
                $"Pre-Authentication Succeded! Token: '<color=green>{(hub.AuthenticationProvider as PreAuthAccessTokenAuthenticator).Token}</color>' ";

            AddText(str);
        }

        private void AuthenticationProvider_OnAuthenticationFailed(IAuthenticationProvider provider, string reason)
        {
            AddText($"Authentication Failed! Reason: '{reason}'");
        }

        /// <summary>
        /// This callback is called when the plugin is connected to the server successfully. Messages can be sent to the server after this point.
        /// </summary>
        private void Hub_OnConnected(HubConnection hub)
        {
            AddText("Hub Connected");
            SetButtons(false, true);

            // Call a parameterless function. We expect a string return value.
            hub.Invoke<string>("Echo", "Message from the client")
                .OnSuccess(ret => AddText($"'Echo' returned: '<color=yellow>{ret}</color>'").AddLeftPadding(20));

            AddText("'<color=green>Message from the client</color>' sent!")
                .AddLeftPadding(20);
        }

        /// <summary>
        /// This is called when the hub is closed after a StartClose() call.
        /// </summary>
        private void Hub_OnClosed(HubConnection hub)
        {
            AddText("Hub Closed");
            SetButtons(true, false);
        }

        /// <summary>
        /// Called when an unrecoverable error happen. After this event the hub will not send or receive any messages.
        /// </summary>
        private void Hub_OnError(HubConnection hub, string error)
        {
            AddText($"Hub Error: <color=red>{error}</color>");
            SetButtons(true, false);
        }

        private void SetButtons(bool connect, bool close)
        {
            if (this._connectButton != null)
                this._connectButton.interactable = connect;

            if (this._closeButton != null)
                this._closeButton.interactable = close;
        }

        private TextListItem AddText(string text)
        {
            return GUIHelper.AddText(this._listItemPrefab, this._contentRoot, text, this._maxListItemEntries, this._scrollRect);
        }
    }

    public sealed class PreAuthAccessTokenAuthenticator : IAuthenticationProvider
    {
        /// <summary>
        /// No pre-auth step required for this type of authentication
        /// </summary>
        public bool IsPreAuthRequired { get { return true; } }

#pragma warning disable 0067
        /// <summary>
        /// Not used event as IsPreAuthRequired is false
        /// </summary>
        public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

        /// <summary>
        /// Not used event as IsPreAuthRequired is false
        /// </summary>
        public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

#pragma warning restore 0067

        private Uri authenticationUri;

        public string Token { get; private set; }

        public PreAuthAccessTokenAuthenticator(Uri authUri)
        {
            this.authenticationUri = authUri;
        }

        public void StartAuthentication()
        {
            var request = new HTTPRequest(this.authenticationUri, OnAuthenticationRequestFinished);
            request.Send();
        }

        private void OnAuthenticationRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        this.Token = resp.DataAsText;
                        if (this.OnAuthenticationSucceded != null)
                            this.OnAuthenticationSucceded(this);
                    }
                    else // Internal server error?
                        AuthenticationFailed(
                            $"Request Finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}");
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    AuthenticationFailed("Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "" + req.Exception.StackTrace) : "No Exception"));
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    AuthenticationFailed("Request Aborted!");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    AuthenticationFailed("Connection Timed Out!");
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    AuthenticationFailed("Processing the request Timed Out!");
                    break;
            }
        }

        private void AuthenticationFailed(string reason)
        {
            if (this.OnAuthenticationFailed != null)
                this.OnAuthenticationFailed(this, reason);
        }

        /// <summary>
        /// Prepares the request by adding two headers to it
        /// </summary>
        public void PrepareRequest(BestHTTP.HTTPRequest request)
        {
            if (HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) == SupportedProtocols.HTTP)
                request.Uri = PrepareUri(request.Uri);
        }

        public Uri PrepareUri(Uri uri)
        {
            if (!string.IsNullOrEmpty(this.Token))
            {
                string query = string.IsNullOrEmpty(uri.Query) ? "?" : uri.Query + "&";
                UriBuilder uriBuilder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, query + "access_token=" + this.Token);
                return uriBuilder.Uri;
            }
            else
                return uri;
        }
    }
}

#endif