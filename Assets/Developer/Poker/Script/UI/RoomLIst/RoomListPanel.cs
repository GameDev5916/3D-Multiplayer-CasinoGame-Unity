using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Casino_Poker
{
    public class RoomListPanel : MonoBehaviour
    {
        [SerializeField] private GameObject RoomDetaPrefab;
        [SerializeField] private GameObject Content;
        [SerializeField] private GameObject NoRoomText;

        private void OnEnable()
        {
            NetworkManager_Poker.Instance.PokerSocket?.Emit(Constants.SEARCH_ROOM);
            NetworkManager_Poker.SerchForRoomList += ShowAllRoomList;
        }

        private void OnDisable()
        {
            NetworkManager_Poker.SerchForRoomList -= ShowAllRoomList;
        }

        void ShowAllRoomList(JSONNode jsonNode)
        {
            DestroyAllObjectINContent();

            if (jsonNode.Count <= 0)
                NoRoomText.SetActive(true);
            else
                NoRoomText.SetActive(false);

            for (int i = 0; i < jsonNode.Count; i++)
            {
                RoomData RoomData = Instantiate(RoomDetaPrefab, Content.transform).GetComponent<RoomData>();

                RoomData.RoomId = jsonNode[i]["roomName"].Value.ToString();
                //RoomData.RoomName.text = "ROOM " + (i + 1);
                RoomData.RoomName.text = jsonNode[i]["roomName"].Value.ToString();
                Debug.LogError(jsonNode[i]["joinedPlayer"].Value);
                RoomData.TotalJoinInRoom.text = jsonNode[i]["joinedPlayer"].Value.ToString();
                //RoomData.TotalJoinInRoom.text = jsonNode[i]["joinedPlayers"].Value + " / " + jsonNode[i]["playersCanJoin"].Value;
                //RoomData.SetImage(jsonNode[i]["roomOwnerProfile"].Value);
            }
        }

        public void BackButtonClick()
        {
            UIManager_Poker.Instance.RoomListPanel.SetActive(false);
            UIManager_Poker.Instance.HomePanel.SetActive(true);
        }

        public void DestroyAllObjectINContent()
        {
            for (int i = 0; i < Content.transform.childCount; i++)
            {
                Destroy(Content.transform.GetChild(i).gameObject);
            }
        }
    }
}
