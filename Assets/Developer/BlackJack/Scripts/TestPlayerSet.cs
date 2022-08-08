using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BalckJack
{
    public class TestPlayerSet : MonoBehaviour
    {
        public void SetPlayerID(string id)
        {
            Constance.PlayerID = id;
        }
    }
}