#if !BESTHTTP_DISABLE_SIGNALR_CORE

using BestHTTP.Examples.Helpers;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
    internal sealed class Person
    {
        public string Name { get; set; }
        public long Age { get; set; }

        public override string ToString()
        {
            return $"[Person Name: '{this.Name}', Age: {this.Age.ToString()}]";
        }
    }

    /// <summary>
    /// This sample demonstrates redirection capabilities. The server will redirect a few times the client before
    /// routing it to the final endpoint.
    /// </summary>
    public sealed class UploadHubSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [SerializeField]
        private string _path = "/uploading";

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

        [SerializeField]
        private float _yieldWaitTime = 0.1f;

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
            {
                hub.StartClose();
            }
        }

        public void OnConnectButton()
        {
            HubOptions options = new HubOptions();
            options.SkipNegotiation = true;

            // Crete the HubConnection
            hub = new HubConnection(new Uri(base.sampleSelector.BaseURL + this._path), new JsonProtocol(new LitJsonEncoder()), options);

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
            if (this.hub != null)
            {
                this.hub.StartClose();

                AddText("StartClose called");
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

            StartCoroutine(UploadWord());

            SetButtons(false, true);
        }

        private IEnumerator UploadWord()
        {
            AddText("<color=green>UploadWord</color>:");

            var controller = hub.GetUpStreamController<string, string>("UploadWord");
            controller.OnSuccess(result =>
                {
                    AddText($"UploadWord completed, result: '<color=yellow>{result}</color>'")
                        .AddLeftPadding(20);
                    AddText("");

                    StartCoroutine(ScoreTracker());
                });

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.UploadParam("Hello ");

            AddText("'<color=green>Hello </color>' uploaded!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.UploadParam("World");

            AddText("'<color=green>World</color>' uploaded!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.UploadParam("!!");

            AddText("'<color=green>!!</color>' uploaded!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);

            controller.Finish();

            AddText("Sent upload finished message.")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
        }

        private IEnumerator ScoreTracker()
        {
            AddText("<color=green>ScoreTracker</color>:");
            var controller = hub.GetUpStreamController<string, int, int>("ScoreTracker");

            controller.OnSuccess(result =>
                {
                    AddText($"ScoreTracker completed, result: '<color=yellow>{result}</color>'")
                        .AddLeftPadding(20);
                    AddText("");

                    StartCoroutine(ScoreTrackerWithParameterChannels());
                });

            const int numScores = 5;
            for (int i = 0; i < numScores; i++)
            {
                yield return new WaitForSeconds(_yieldWaitTime);

                int p1 = UnityEngine.Random.Range(0, 10);
                int p2 = UnityEngine.Random.Range(0, 10);
                controller.UploadParam(p1, p2);

                AddText(
                        $"Score({i + 1}/{numScores}) uploaded! p1's score: <color=green>{p1}</color> p2's score: <color=green>{p2}</color>")
                    .AddLeftPadding(20);
            }

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.Finish();

            AddText("Sent upload finished message.")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
        }

        private IEnumerator ScoreTrackerWithParameterChannels()
        {
            AddText("<color=green>ScoreTracker using upload channels</color>:");

            using (var controller = hub.GetUpStreamController<string, int, int>("ScoreTracker"))
            {
                controller.OnSuccess(result =>
                {
                    AddText($"ScoreTracker completed, result: '<color=yellow>{result}</color>'")
                        .AddLeftPadding(20);
                    AddText("");

                    StartCoroutine(StreamEcho());
                });

                const int numScores = 5;

                // While the server's ScoreTracker has two parameters, we can upload those parameters separately
                // So here we

                using (var player1param = controller.GetUploadChannel<int>(0))
                {
                    for (int i = 0; i < numScores; i++)
                    {
                        yield return new WaitForSeconds(_yieldWaitTime);

                        int score = UnityEngine.Random.Range(0, 10);
                        player1param.Upload(score);

                        AddText($"Player 1's score({i + 1}/{numScores}) uploaded! Score: <color=green>{score}</color>")
                            .AddLeftPadding(20);
                    }
                }

                AddText("");

                using (var player2param = controller.GetUploadChannel<int>(1))
                {
                    for (int i = 0; i < numScores; i++)
                    {
                        yield return new WaitForSeconds(_yieldWaitTime);

                        int score = UnityEngine.Random.Range(0, 10);
                        player2param.Upload(score);

                        AddText($"Player 2's score({i + 1}/{numScores}) uploaded! Score: <color=green>{score}</color>")
                            .AddLeftPadding(20);
                    }
                }

                AddText("All scores uploaded!")
                    .AddLeftPadding(20);
            }
            yield return new WaitForSeconds(_yieldWaitTime);
        }

        private IEnumerator StreamEcho()
        {
            AddText("<color=green>StreamEcho</color>:");
            using (var controller = hub.GetUpAndDownStreamController<string, string>("StreamEcho"))
            {
                controller.OnSuccess(result =>
                {
                    AddText("StreamEcho completed!")
                        .AddLeftPadding(20);
                    AddText("");

                    StartCoroutine(PersonEcho());
                });

                controller.OnItem(item =>
                {
                    AddText($"Received from server: '<color=yellow>{item}</color>'")
                        .AddLeftPadding(20);
                });

                const int numMessages = 5;
                for (int i = 0; i < numMessages; i++)
                {
                    yield return new WaitForSeconds(_yieldWaitTime);

                    string message = $"Message from client {i + 1}/{numMessages}";
                    controller.UploadParam(message);

                    AddText($"Sent message to the server: <color=green>{message}</color>")
                        .AddLeftPadding(20);
                }

                yield return new WaitForSeconds(_yieldWaitTime);
            }

            AddText("Upload finished!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
        }

        /// <summary>
        /// This is basically the same as the previous StreamEcho, but it's streaming a complex object (Person
        /// </summary>
        private IEnumerator PersonEcho()
        {
            AddText("<color=green>PersonEcho</color>:");

            using (var controller = hub.GetUpAndDownStreamController<Person, Person>("PersonEcho"))
            {
                controller.OnSuccess(result =>
                {
                    AddText("PersonEcho completed!")
                        .AddLeftPadding(20);
                    AddText("");
                    AddText("All Done!");
                });

                controller.OnItem(item =>
                {
                    AddText($"Received from server: '<color=yellow>{item}</color>'")
                        .AddLeftPadding(20);
                });

                const int numMessages = 5;
                for (int i = 0; i < numMessages; i++)
                {
                    yield return new WaitForSeconds(_yieldWaitTime);

                    Person person = new Person()
                    {
                        Name = "Mr. Smith",
                        Age = 20 + i * 2
                    };

                    controller.UploadParam(person);

                    AddText($"Sent person to the server: <color=green>{person}</color>")
                        .AddLeftPadding(20);
                }

                yield return new WaitForSeconds(_yieldWaitTime);
            }
            AddText("Upload finished!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
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
}

#endif