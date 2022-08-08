using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BalckJack
{
    public class Constance : MonoBehaviour
    {
        public static string PlayerID
        {
            get { return PlayerPrefs.GetString(nameof(PlayerID)); }
            set { PlayerPrefs.SetString(nameof(PlayerID), value); }
        }

        public static int Min_BetAmoutForBlackJack
        {
            get { return PlayerPrefs.GetInt(nameof(Min_BetAmoutForBlackJack)); }
            set { PlayerPrefs.SetInt(nameof(Min_BetAmoutForBlackJack), value); }
        }
        public static int Max_BetAmoutForBlackJack
        {
            get { return PlayerPrefs.GetInt(nameof(Max_BetAmoutForBlackJack)); }
            set { PlayerPrefs.SetInt(nameof(Max_BetAmoutForBlackJack), value); }
        }
        public static int TimerForBlackJack
        {
            get { return PlayerPrefs.GetInt(nameof(TimerForBlackJack)); }
            set { PlayerPrefs.SetInt(nameof(TimerForBlackJack), value); }
        }

        #region Class

        [System.Serializable]
        public class CardThems
        {
            public Sprite CardBackSprite;
            public List<CardSuit> Cards = new List<CardSuit>();
        }

        [System.Serializable]
        public class CardSuit
        {
            public SuitEnum Suit;
            public Sprite[] CardsSprites = new Sprite[13];
        }

        [System.Serializable]
        public enum SuitEnum
        {
            Hearts = 1,
            Clubs = 2,
            Diamonds = 3,
            Spades = 4,
        }

        [System.Serializable]
        public class MiniMax
        {
            public int Min;
            public int Max;
        }
        #endregion

        public static string AmountShow(long Score)
        {
            float Scor = Score;
            string result;
            string[] ScoreNames = new string[] { "", "k", "M", "B", "T" };
            int i;

            for (i = 0; i < ScoreNames.Length; i++)
                if (Scor < 900)
                    break;
                else Scor = Mathf.Floor(Scor / 100f) / 10f;

            if (Scor == Mathf.Floor(Scor))
                result = Scor.ToString() + ScoreNames[i];
            else result = Scor.ToString("F1") + ScoreNames[i];
            return result;
        }

        #region GetCardIndexSwitch
        static int CardIndex;
        public static int GetCardIndex(string CardID)
        {
            switch (CardID)
            {
                case "A":
                    CardIndex = 0;
                    break;
                case "2":
                    CardIndex = 1;
                    break;
                case "3":
                    CardIndex = 2;
                    break;
                case "4":
                    CardIndex = 3;
                    break;
                case "5":
                    CardIndex = 4;
                    break;
                case "6":
                    CardIndex = 5;
                    break;
                case "7":
                    CardIndex = 6;
                    break;
                case "8":
                    CardIndex = 7;
                    break;
                case "9":
                    CardIndex = 8;
                    break;
                case "10":
                    CardIndex = 9;
                    break;
                case "J":
                    CardIndex = 10;
                    break;
                case "Q":
                    CardIndex = 11;
                    break;
                case "K":
                    CardIndex = 12;
                    break;
                default:
                    break;
            }

            return CardIndex;
        }
        #endregion
    }
}