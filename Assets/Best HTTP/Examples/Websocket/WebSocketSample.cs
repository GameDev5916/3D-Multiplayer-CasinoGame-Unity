#if !BESTHTTP_DISABLE_WEBSOCKET

using BestHTTP.Examples.Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.Websockets
{
    public class WebSocketSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [SerializeField]
        [Tooltip("The WebSocket address to connect")]
        private string address = "wss://echo.websocket.org";

        [SerializeField]
        private InputField _input;

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

        /// <summary>
        /// Saved WebSocket instance
        /// </summary>
        private WebSocket.WebSocket webSocket;

        protected override void Start()
        {
            base.Start();

            SetButtons(true, false);
            this._input.interactable = false;
        }

        private void OnDestroy()
        {
            if (this.webSocket != null)
            {
                this.webSocket.Close();
                this.webSocket = null;
            }
        }

        public void OnConnectButton()
        {
            // Create the WebSocket instance
            this.webSocket = new WebSocket.WebSocket(new Uri(address));

#if !UNITY_WEBGL
            this.webSocket.StartPingThread = true;

#if !BESTHTTP_DISABLE_PROXY
            if (HTTPManager.Proxy != null)
                this.webSocket.InternalRequest.Proxy = new HTTPProxy(HTTPManager.Proxy.Address, HTTPManager.Proxy.Credentials, false);
#endif
#endif

            // Subscribe to the WS events
            this.webSocket.OnOpen += OnOpen;
            this.webSocket.OnMessage += OnMessageReceived;
            this.webSocket.OnClosed += OnClosed;
            this.webSocket.OnError += OnError;

            // Start connecting to the server
            this.webSocket.Open();

            AddText("Connecting...");

            SetButtons(false, true);
            this._input.interactable = false;
        }

        public void OnCloseButton()
        {
            AddText("Closing!");
            // Close the connection
            this.webSocket.Close(1000, "Bye!");

            SetButtons(false, false);
            this._input.interactable = false;
        }

        public void OnInputField(string textToSend)
        {
            if ((!Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetKeyDown(KeyCode.Return)) || string.IsNullOrEmpty(textToSend))
                return;

            AddText($"Sending message: <color=green>{textToSend}</color>")
                .AddLeftPadding(20);

            // Send message to the server
            this.webSocket.Send(textToSend);
        }

        #region WebSocket Event Handlers

        /// <summary>
        /// Called when the web socket is open, and we are ready to send and receive data
        /// </summary>
        private void OnOpen(WebSocket.WebSocket ws)
        {
            AddText("WebSocket Open!");

            this._input.interactable = true;
        }

        /// <summary>
        /// Called when we received a text message from the server
        /// </summary>
        private void OnMessageReceived(WebSocket.WebSocket ws, string message)
        {
            AddText($"Message received: <color=yellow>{message}</color>")
                .AddLeftPadding(20);
        }

        /// <summary>
        /// Called when the web socket closed
        /// </summary>
        private void OnClosed(WebSocket.WebSocket ws, UInt16 code, string message)
        {
            AddText($"WebSocket closed! Code: {code} Message: {message}");

            webSocket = null;

            SetButtons(true, false);
        }

        /// <summary>
        /// Called when an error occured on client side
        /// </summary>
        private void OnError(WebSocket.WebSocket ws, string error)
        {
            AddText($"An error occured: <color=red>{error}</color>");

            webSocket = null;

            SetButtons(true, false);
        }

        #endregion

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
}

#endif