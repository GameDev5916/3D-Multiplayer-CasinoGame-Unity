using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;
using System;
using SimpleJSON;

public class MainNetworkManager : MonoBehaviour
{
    public static MainNetworkManager Instance;

    private SocketManager socketManager;
    public Socket MainSocket;

    public static Action<JSONNode> OnEnterCode, OnReciveBonusGift, JoinInvitation, SetInvitePanel, PlayerFriendListAction, SetFriendsPanel, AcceptFriendRequest, SetFriendRequestPanel, AllPlayerListAction, SetAllPlayerPanel, TableItemAction, SetGiftPanel, OnPlayerProfileOpen, SetPlayerProfilePanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
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
            //ReconnectionDelay = TimeSpan.FromSeconds(30)
        };

        socketManager = new SocketManager(new Uri(Constants.SERVER_URL + Constants.SOCKET_URL), options);
        socketManager.Open();

        MainSocket = socketManager.GetSocket("/allinone");
        MainSocket.On(SocketIOEventTypes.Connect, OnConnect);
        MainSocket.On(SocketIOEventTypes.Disconnect, OnDisconnect);
        MainSocket.On(SocketIOEventTypes.Error, OnError);

        MainSocket.On("enterinvitecodeAction", ONCodeEnterResponce);
        MainSocket.On("freeSpinInGift", OnReciveFreeGiftBounuc);
        MainSocket.On("playerfriendList", OnPlayerFriendList);
        MainSocket.On("inviteFriendAction", OnFriendInviteRequest);
        MainSocket.On("allPlayerList", OnAllPlayerList);
        MainSocket.On("friendRequestAction", OnAddFriendRequest);
        MainSocket.On("acceptDeclineAction", AcceptDeclineStatusDisplay);
        MainSocket.On("tableItemsAction", OnGettingTableItems);
        MainSocket.On("playerDetaileAction", OnGettingPlayerDetail);
    }

    #region Network Listners

    private void OnConnect(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("Connect To Main Server Success");

        JSONNode data = new JSONObject
        {
            ["playerId"] = Constants.PLAYER_ID,
        };
        Debug.LogError("StorePlayerInfo~~~~" + data.ToString());
        MainSocket?.Emit("storePlayerInfo", data.ToString());
        //OnSendingFriendList();
    }

    private void OnDisconnect(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("Disconnect Main Form Server");
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

    private void ONCodeEnterResponce(Socket socket, Packet packet, object[] args)
    {
        Debug.LogError("Code Enter Responce : " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        OnEnterCode?.Invoke(jsonNode);
    }

    private void OnReciveFreeGiftBounuc(Socket socket, Packet packet, object[] args)
    {
        Debug.LogError(packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        OnReciveBonusGift?.Invoke(jsonNode);
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~InviteFriendToTable~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void OnPlayerFriendList(Socket socket, Packet packet, object[] args)
    {
        Debug.LogError("OnPlayerFriendList: " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        //Constants.instance.jsonNodeFriendList = jsonNode;
        PlayerFriendListAction?.Invoke(jsonNode);
    }

    private void OnFriendInviteRequest(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("OnFriendInviteRequestHome " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        JoinInvitation?.Invoke(jsonNode);
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~AddPlayerToFriendList~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void OnAllPlayerList(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("OnAllPlayerList " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        AllPlayerListAction?.Invoke(jsonNode);
    }

    private void OnAddFriendRequest(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("OnAddFriendRequest " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        AcceptFriendRequest?.Invoke(jsonNode);
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    }

    private void AcceptDeclineStatusDisplay(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("AcceptDeclineStatusDisplay " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        ShowAcceptDeclineMessage(jsonNode);
    }

    private void OnGettingTableItems(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("OnGettingTableItems " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        TableItemAction?.Invoke(jsonNode);
    }

    private void OnGettingPlayerDetail(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("OnGettingPlayerDetail " + packet);
        JSONNode jsonNode = JSON.Parse(args[0].ToString());
        OnPlayerProfileOpen?.Invoke(jsonNode);
    }

    #endregion Network Listners

    public void OnSendingFriendList()
    {
        Debug.Log("SendingFriendList");

        JSONArray friendListJSONArray = new JSONArray();

        foreach (var item in Constants.instance.fbFriendList)
        {
            Debug.Log("Facebook friends ID to JSON ====" + item);
            friendListJSONArray.Add(item);
        }

        JSONNode data = new JSONObject
        {
            ["playerId"] = Constants.PLAYER_ID,
            ["friendList"] = friendListJSONArray
        };

        Debug.Log(" FriendsJSON : " + data.ToString());
        MainSocket?.Emit("friendList", data.ToString());
    }

    private void ShowAcceptDeclineMessage(JSONNode jsonNode)
    {
        if (jsonNode["ownerPlayerId"] == Constants.PLAYER_ID)
        {
            Constants.ShowWarning(jsonNode["message"]);
        }
    }
}
