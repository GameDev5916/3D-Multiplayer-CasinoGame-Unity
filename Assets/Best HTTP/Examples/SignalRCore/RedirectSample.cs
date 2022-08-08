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
    /// <summary>
    /// This sample demonstrates redirection capabilities. The server will redirect a few times the client before
    /// routing it to the final endpoint.
    /// </summary>
    public sealed class RedirectSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [SerializeField]
        private string _path = "/redirect_sample";

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
        public HubConnection hub;

        protected override void Start()
        {
            base.Start();

            SetButtons(true, false);
        }

        private void OnDestroy()
        {
            if (hub != null)
            {
                hub.StartClose();
            }
        }

        public void OnConnectButton()
        {
            // Server side of this example can be found here:
            // https://github.com/Benedicht/BestHTTP_DemoSite/blob/master/BestHTTP_DemoSite/Hubs/

            // Crete the HubConnection
            hub = new HubConnection(new Uri(base.sampleSelector.BaseURL + this._path), new JsonProtocol(new LitJsonEncoder()));
            hub.AuthenticationProvider = new RedirectLoggerAccessTokenAuthenticator(hub);

            // Subscribe to hub events
            hub.OnConnected += Hub_OnConnected;
            hub.OnError += Hub_OnError;
            hub.OnClosed += Hub_OnClosed;

            hub.OnRedirected += Hub_Redirected;

            // And finally start to connect to the server
            hub.StartConnect();

            AddText("StartConnect called");
            SetButtons(false, false);
        }

        public void OnCloseButton()
        {
            if (hub != null)
            {
                AddText("Calling StartClose");

                hub.StartClose();

                SetButtons(false, false);
            }
        }

        private void Hub_Redirected(HubConnection hub, Uri oldUri, Uri newUri)
        {
            AddText($"Hub connection redirected to '<color=green>{hub.Uri}</color>'!");
        }

        /// <summary>
        /// This callback is called when the plugin is connected to the server successfully. Messages can be sent to the server after this point.
        /// </summary>
        private void Hub_OnConnected(HubConnection hub)
        {
            AddText("Hub Connected");

            // Call a parameterless function. We expect a string return value.
            hub.Invoke<string>("Echo", "Message from the client")
                .OnSuccess(ret => AddText($" 'Echo' returned: '{ret}'"));

            SetButtons(false, true);
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

        private void AddText(string text)
        {
            GUIHelper.AddText(this._listItemPrefab, this._contentRoot, text, this._maxListItemEntries, this._scrollRect);
        }
    }

    public sealed class RedirectLoggerAccessTokenAuthenticator : IAuthenticationProvider
    {
        /// <summary>
        /// No pre-auth step required for this type of authentication
        /// </summary>
        public bool IsPreAuthRequired { get { return false; } }

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

        private HubConnection _connection;

        public RedirectLoggerAccessTokenAuthenticator(HubConnection connection)
        {
            this._connection = connection;
        }

        /// <summary>
        /// Not used as IsPreAuthRequired is false
        /// </summary>
        public void StartAuthentication()
        { }

        /// <summary>
        /// Prepares the request by adding two headers to it
        /// </summary>
        public void PrepareRequest(BestHTTP.HTTPRequest request)
        {
            request.SetHeader("x-redirect-count", _connection.RedirectCount.ToString());

            if (HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) == SupportedProtocols.HTTP)
                request.Uri = PrepareUri(request.Uri);
        }

        public Uri PrepareUri(Uri uri)
        {
            if (this._connection.NegotiationResult != null && !string.IsNullOrEmpty(this._connection.NegotiationResult.AccessToken))
            {
                string query = string.IsNullOrEmpty(uri.Query) ? "?" : uri.Query + "&";
                UriBuilder uriBuilder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, query + "access_token=" + this._connection.NegotiationResult.AccessToken);
                return uriBuilder.Uri;
            }
            else
                return uri;
        }
    }
}

#endif