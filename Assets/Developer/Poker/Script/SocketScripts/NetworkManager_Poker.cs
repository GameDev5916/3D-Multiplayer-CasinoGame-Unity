using System.Collections.Generic;
using UnityEngine;
using System;
using BestHTTP.SocketIO;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Casino_Poker
{
    public class NetworkManager_Poker : MonoBehaviour
    {
        public static NetworkManager_Poker Instance;

        private SocketManager socketManager;
        public Socket PokerSocket;

        public static Action<JSONNode> RefletPlayerList, SerchForRoomList;

        public static Action<JSONNode> RoomJoin, GameStart, PlayerCardDistribution, PlayerLeft, PlayerTurn, TimerChange, PlayerOption, GameStatus,
                                        GameCardDistriBution, PlayerOptionSelect, Gamewin, GameWinSinglePlayer, TotalBetAmount, ShowAllPlayerCard, NewPlayerJoin, GameRestart, TipDealer, PlayerAmount, RoomPlayerStandUp, PokerSendGiftAction, GameStatAction, OptionSelectAction;

        private GameObject LoadingScreen;

        private void Awake()
        {
            //if (Constants.PokerJoinRoom)
            //    LoadingScreen = Instantiate(Constants.instance.LoadingScreen);

            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            ConnectToServer();
        }

        void ConnectToServer()
        {
            //For reconnection not in use
            //var observableDictionary = new ObservableDictionary<string, string>
            //{
            //    {"playerId", PlayerPrefs.GetInt(Constants.ID).ToString() },
            //};

            var options = new SocketOptions
            {
                //AdditionalQueryParams = observableDictionary
                Reconnection = true,
                AutoConnect = true,
                //ReconnectionDelay = TimeSpan.FromSeconds(2)
            };

            socketManager = new SocketManager(new Uri(Constants.SERVER_URL + Constants.SOCKET_URL), options);
            socketManager.Open();

            PokerSocket = socketManager.GetSocket("/TexasPoker");
            PokerSocket.On(SocketIOEventTypes.Connect, OnConnect);
            PokerSocket.On(SocketIOEventTypes.Disconnect, OnDisconnect);
            PokerSocket.On(SocketIOEventTypes.Error, OnError);

            PokerSocket.On("joinRoomData", OnRoomJoin);
            PokerSocket.On("gameStart", OnGameStart);
            PokerSocket.On("playerOwnCards", OnPlayerCardDistribution);
            PokerSocket.On("roomPlayerLeft", OnPlayerLeft);
            PokerSocket.On("playerTurn", OnPlayerTurn);
            PokerSocket.On("getPlayerTimer", OnPlayerTimer);
            PokerSocket.On("sendPlayerOption", OnPlayerOption);
            PokerSocket.On("gameCardDistribution", OnGameCardDistribution);
            PokerSocket.On("playerSelectOption", OnPlayerSelectionOption);
            PokerSocket.On("tableAmount", OnTotalTableBetAmount);
            PokerSocket.On("allPlayerCardShow", OnShowAllPlayerCard);
            PokerSocket.On("gameWinner", OnGameWin);
            PokerSocket.On("singlePlayerWinner", OnGameWinSinglePlayer);
            PokerSocket.On("joinNewPlayerInRoomData", OnNewPlayerJoinRoom);
            PokerSocket.On("gameRestart", OnGameRestart);
            PokerSocket.On("tipAction", OnTipDealer);
            PokerSocket.On("playerAmountAction", OnPlayerAmountAction);
            PokerSocket.On("roomPlayerStandUp", OnRoomPlayerStandup);
            PokerSocket.On("giftAction", OnSendingGift);
            PokerSocket.On("gameStasAction", OnGettingGameStatInfo);
            PokerSocket.On("playerSelectAction", OnGettingOptionSelection);
            PokerSocket.On("gameStatus", OnGameStatus);
        }

        #region Network Listners

        private void OnConnect(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("Connect To Poker Server Success");
            if (PokerTableSelect.instance)
            {
                PokerTableSelect.instance.playButton.interactable = true;
            }

            StartCoroutine(JoinRoomInvitation());
        }

        IEnumerator JoinRoomInvitation()
        {
            yield return new WaitForSeconds(2f);
            GameManager_Poker.Instance.JoinByInvitation();
        }

        private void OnDisconnect(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("Disconnect Form Server");
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

        public void CreateRoom()
        {
            JSONNode jsonNode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["minAmount"] = 0,
                ["maxAmount"] = 0,
                ["isStandUp"] = true,
            };
            //Debug.LogError(jsonNode.ToString());
            PokerSocket?.Emit("createJoinPublicRoom", jsonNode.ToString());
        }

        private void OnRoomJoin(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnRoomJoin" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            RoomJoin?.Invoke(jsonNode);
            Destroy(LoadingScreen);
        }

        private void OnNewPlayerJoinRoom(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnNewPlayerJoinRoom");
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            NewPlayerJoin?.Invoke(jsonNode);
            Destroy(LoadingScreen);
        }

        private void OnGameStart(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnGameStart");
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameStart?.Invoke(jsonNode);
        }

        private void OnGameStatus(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnGameStatus");
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameStatus?.Invoke(jsonNode);
        }

        private void OnPlayerCardDistribution(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnPlayerCardDistribution");
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerCardDistribution?.Invoke(jsonNode);
            Destroy(LoadingScreen);
        }

        private void OnPlayerLeft(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnPlayerLeft " + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerLeft?.Invoke(jsonNode);
            if(Constants.PLAYER_ID == jsonNode["playerId"].Value) {
                socket.Disconnect();
            }      
        }

        private void OnPlayerAmountAction(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnPlayerAmountAction " + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerAmount?.Invoke(jsonNode);
        }

        private void OnPlayerTurn(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnPlayerTurn : " + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerTurn?.Invoke(jsonNode);
        }

        private void OnPlayerTimer(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnPlayerTimer" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            TimerChange?.Invoke(jsonNode);
        }

        private void OnPlayerOption(Socket socket, Packet packet, object[] args)
        {
            Debug.LogError("OnPlayerOption" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerOption?.Invoke(jsonNode);
        }

        private void OnGameCardDistribution(Socket socket, Packet packet, object[] args)
        {
            //Debug.LogError("OnGameCardDistribution" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameCardDistriBution?.Invoke(jsonNode);
        }

        private void OnPlayerSelectionOption(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnPlayerSelectionOption" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PlayerOptionSelect?.Invoke(jsonNode);
        }

        private void OnTotalTableBetAmount(Socket socket, Packet packet, object[] args)
        {
            //Debug.LogError("OnTotalTableBetAmount" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            TotalBetAmount?.Invoke(jsonNode);
        }

        private void OnShowAllPlayerCard(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnShowAllPlayerCard" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            ShowAllPlayerCard?.Invoke(jsonNode);
        }

        private void OnGameWin(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnGameWin" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            Gamewin?.Invoke(jsonNode);
        }

        private void OnGameWinSinglePlayer(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnGameWinSinglePlayer" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameWinSinglePlayer?.Invoke(jsonNode);
        }

        private void OnGameRestart(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("gameRestart" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameRestart?.Invoke(jsonNode);
        }

        private void OnTipDealer(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnTipDealer" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            TipDealer?.Invoke(jsonNode);
        }

        private void OnRoomPlayerStandup(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnRoomPlayerStandup" + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            RoomPlayerStandUp?.Invoke(jsonNode);
        }

        private void OnSendingGift(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnSendingGift " + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            PokerSendGiftAction?.Invoke(jsonNode);
        }

        private void OnGettingGameStatInfo(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnGettingGameStatInfo " + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            GameStatAction?.Invoke(jsonNode);
        }

        private void OnGettingOptionSelection(Socket socket, Packet packet, object[] args)
        {
            Debug.Log("OnGettingOptionSelection " + packet);
            JSONNode jsonNode = JSON.Parse(args[0].ToString());
            OptionSelectAction?.Invoke(jsonNode);
        }


        #endregion

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

            PokerSocket?.Emit("disconnectmanually", jsonNode.ToString());
            //Debug.LogError("Is Pause");
        }
    }
}