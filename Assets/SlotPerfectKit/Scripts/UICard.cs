using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BE {
	public enum CardType {
		Clover		= 0,
		Heart		= 1,
		Spade		= 2,
		Diamond		= 3,
	};

	public class UICard : MonoBehaviour {
		private Transform	tr;
		public  Image		Front;
		public  Image		IconCenter;
		public  Image		IconTopLeft;
		public  Text		NumberTopLeft;
		public  Image		IconBottomRight;
		public  Text		NumberBottomRight;
		public  Image		Back;

		public  Sprite[]	Sprites;
		public  CardType	Symbol;
		public  int			Number;

		public  bool		FrontSide = true;
		private bool		InFlipping = false;
		private float		FlippingAge = 0.0f;
		private float		FlippingPeriod = 0.6f;

		void Awake () {
			tr = transform;
			SetSide(true);
		}

		void Start () {
		}

		// Update is called once per frame
		void Update () {
			if(InFlipping) {
				if((FlippingAge < FlippingPeriod*0.5f) && (FlippingAge+Time.deltaTime >= FlippingPeriod*0.5f))
					SetSide(!FrontSide);

				FlippingAge += Time.deltaTime;
				float value = 2.0f/FlippingPeriod;
				float rotY = 90.0f * Mathf.PingPong(FlippingAge*value, 1.0f);
				if(FlippingAge > FlippingPeriod) {
					rotY = 0.0f;
					InFlipping = false;
				}

				tr.localRotation = Quaternion.Euler(0,rotY,0);
			}
		}

		public void SetSide(bool bFront) {
			FrontSide = bFront;
			Front.gameObject.SetActive(FrontSide);
			Back.gameObject.SetActive(!FrontSide);
		}

		public int GetIndexof52() {
			return (int)Symbol * 13 + Number - 1;
		}

		public void SetSymbolNumber(int indexof52) {
			CardType type = (CardType)(indexof52 / 13);
			int number = indexof52 - (int)type * 13 + 1;
			SetSymbolNumber(type, number);
		}

		public void SetSymbolNumber(CardType type, int number) {
			Symbol = type;
			Number = number;

			string strNumber;
			if(number == 11) 		strNumber = "J";
			else if(number == 12) 	strNumber = "Q";
			else if(number == 13) 	strNumber = "K";
			else if(number == 1) 	strNumber = "A";
			else 					strNumber = number.ToString ();

			Sprite sprSymbol = Sprites[(int)type];

			IconCenter.sprite = sprSymbol;
			IconTopLeft.sprite = sprSymbol;
			NumberTopLeft.text = strNumber;
			IconBottomRight.sprite = sprSymbol;
			NumberBottomRight.text = strNumber;

			Color color = isRedColor(type) ? Color.red : Color.black;
			IconCenter.color = color;
			IconTopLeft.color = color;
			NumberTopLeft.color = color;
			IconBottomRight.color = color;
			NumberBottomRight.color = color;
		}

		public static bool isRedColor(CardType type) {
			bool isRed = ((type==CardType.Heart) || (type==CardType.Diamond)) ? true : false;
			return isRed;
		}

		public void Flip() {
			if(InFlipping) return;

			InFlipping = true;
			FlippingAge = 0.0f;
		}
	}
}