using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;
using System;
using SimpleJSON;
using Casino_Poker;

namespace BalckJack
{
    public class BlackJack_NetworkManager : MonoBehaviour
    {
        public static BlackJack_NetworkManager Instance;

        private SocketManager socketManager;
        public Socket BlackJackSocket;

        public static bool isconnected;

        //ACTIONS
        public static Action<JSONNode> PlayerJoinRoom, PlayerLeftRoomAction, BetTimerAction, BetActionShowChips, PlayerTimerStartAction, PlayerOption, WinLoseAction, OnGameRestart, PlayerAmount, RoomPlayerStandUp, GameStatAction, BlackJackSendGiftAction, StartPlayerCardDistribution;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (!isconnected)
            {
                ConnectToServer();
            }
        }

        public void ConnectToServer()
        {
            //For reconnection not in use
            //var observableDictionary = new ObservableDictionary<string, string>
            //{
            //    {"playerId", PlayerPrefs.GetInt(Constants.ID).ToString() },
            //};

            //var options = new SocketOptions
            //{
            //    //AdditionalQueryParams = observableDictionary
            //    //Reconnection = true,
            //    //AutoConnect = true,
            //    //ReconnectionDelay = TimeSpan.FromSeconds(30)
            //};

            socketManager = new SocketManager(new Uri(Constants.SERVER_URL + Constants.SOCKET_URL));
            socketManager.Open();

            BlackJackSocket = socketManager.GetSocket("/BlackJack");

            BlackJackSocket.On(SocketIOEventTypes.Connect, OnConnect);
            BlackJackSocket.On(SocketIOEventTypes.Disconnect, OnDisconnect);
            BlackJackSocket.On(SocketIOEventTypes.Error, OnError);

            BlackJackSocket.On("joinRoomData", PlayerDataOnJoinRoom);
            BlackJackSocket.On("playerLeftAction", RoomPlayerLeft);
            BlackJackSocket.On("betTimerAction", BetTimerStartAction);
            BlackJackSocket.On("betAction", OnBetAction);
            BlackJackSocket.On("gameIsStart", OnGameIsStart);
            BlackJackSocket.On("gameIsReStart", OnGameIsRestart);
            BlackJackSocket.On("playerTimerAction", OnPlayerTimerStart);
            BlackJackSocket.On("gamePlayerData", OnGamePlayerData);
            BlackJackSocket.On("playerOption", OnPlayerOption);
            BlackJackSocket.On("playerSelectAction", OnPlayerSelectAction);
            BlackJackSocket.On("dealerHand", OnDealerHand);
            BlackJackSocket.On("winAction", OnWinAction);
            BlackJackSocket.On("playerAmountAction", OnPlayerAmountAction);
            BlackJackSocket.On("roomPlayerStandUp", OnRoomPlayerStandUp);
            BlackJackSocket.On("gameStasAction", OnGettingGameStatInfo);
            BlackJackSocket.On("giftAction", OnSendingGift);
        }


        #region Network Listners

        private void OnConnect(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("Connect To BlackJack Server Success");
            isconnected = true;
            if (TableSelectionBlacJack.instance)
            {
                TableSelectionBlacJack.instance.PlayButton.interactable = true;
            }


            BlackJackGameManager.Instance.JoinByInvitation();

            //StartCoroutine(JoinRoomInvitation());
        }

        IEnumerator JoinRoomInvitation()
        {
            yield return new WaitForSeconds(2f);
            BlackJackGameManager.Instance.JoinByInvitation();
        }

        private void OnDisconnect(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("Disconnect Form BlackJack Server");
            isconnected = false;
        }

        private void OnError(Socket socket, Packet packet, object[] args)
        {
            Error error = args[0] as Error;
            switch (error.Code)
            {
                case SocketIOErrors.User:
                    Debug.Log("DC ~~~~~~ Exception in an event handler!");
                    break;
                case SocketIOErrors.Internal:
                    Debug.Log("DC ~~~~~~ Internal error! Message: " + error.Message);
                    break;
                default:
                    Debug.Log("DC ~~~~~~ Server error! Message: " + error.Message);
                    break;
            }
        }

        private void PlayerDataOnJoinRoom(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("PlayerDataOnJoinRoom ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerJoinRoom?.Invoke(jsonNode);
        }

        private void RoomPlayerLeft(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("RoomPlayerLeft ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerLeftRoomAction?.Invoke(jsonNode);
        }

        private void BetTimerStartAction(Socket socket, Packet packet, object[] args)
        {
            //Debug.LogError("BetTimerAction ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            BetTimerAction?.Invoke(jsonNode);
        }

        private void OnBetAction(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnBetAction ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            BetActionShowChips?.Invoke(jsonNode);
        }

        private void OnGameIsStart(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnGameIsStart ~~~" + packet);
            //JSONNode jsonNode = JSON.Parse(args[0].ToString());
        }

        private void OnGameIsRestart(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnGameIsRestart ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            OnGameRestart?.Invoke(jsonNode);
            GamePlayManager.instance.BetButtonOnSlider.interactable = true;
        }

        private void OnPlayerTimerStart(Socket socket, Packet packet, object[] args)
        {
            //Debug.LogError("OnPlayerTimerStart ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerTimerStartAction?.Invoke(jsonNode);
        }

        private void OnGamePlayerData(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnGamePlayerData ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            //DelarCards.ShowDealerCards?.Invoke(jsonNode);
            //StartPlayerCardDistribution?.Invoke(jsonNode);
            StartCoroutine(AllCardDistribution(jsonNode));
        }

        IEnumerator AllCardDistribution(JSONNode jsonNode)
        {
            DelarCards.ShowDealerCards?.Invoke(jsonNode);
            yield return new WaitForSeconds(1f);
            StartPlayerCardDistribution?.Invoke(jsonNode);
        }

        private void OnPlayerOption(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnPlayerOption ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerOption?.Invoke(jsonNode);
        }

        private void OnPlayerSelectAction(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnPlayerSelectAction ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GamePlayManager.DisplayPlayerCard?.Invoke(jsonNode);
        }

        private void OnDealerHand(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnDealerHand ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            DelarCards.OnDealerHandDisplay?.Invoke(jsonNode);
        }

        private void OnWinAction(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnWinAction ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            WinLoseAction?.Invoke(jsonNode);
        }

        private void OnPlayerAmountAction(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnPlayerAmountAction ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerAmount?.Invoke(jsonNode);
        }

        private void OnRoomPlayerStandUp(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnRoomPlayerStandUp ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            RoomPlayerStandUp?.Invoke(jsonNode);
        }

        private void OnGettingGameStatInfo(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnGettingGameStatInfo ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameStatAction?.Invoke(jsonNode);
        }

        private void OnSendingGift(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnSendingGiftBJ ~~~" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            BlackJackSendGiftAction?.Invoke(jsonNode);
        }

        #endregion Network Listners

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Disconnection();
            }
        }

        private void OnApplicationQuit()
        {
            Disconnection();
        }

        public void Disconnection()
        {
            JSONNode jsonNode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID
            };

            BlackJackSocket?.Emit("disconnectmanually", jsonNode.ToString());
            //Debug.LogError("Is Pause");
        }
    }
}