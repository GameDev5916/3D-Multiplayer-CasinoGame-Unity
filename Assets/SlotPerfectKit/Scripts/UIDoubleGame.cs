using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BE {
	public class UIDoubleGame : MonoBehaviour {
		private static UIDoubleGame instance;

		public	Image 		Dialog;
		public	Text 		Title;
		public	UICard 		CardDeck;
		public	UICard 		CardCenter;
		public	UICard []	CardsRight;
		public	Button []	Buttons;
		public	Image 		DoubleBackground;
		public	Image 		QuadBackground;
		public	Text 		DoubleText;
		public	Text 		QuadText;
		public  Animator	animator;

		private int			SelectCountMax = 5;
		private int			SelectCount = 0;

		private float		CoinStart;
		private float		CoinWins;
		private List<int>	Deck = new List<int>();
		private int			CardIndexInDeck = 0;

		private bool		InShowResult = false;
		private bool		InputEnabled = true;

		void Awake () {
			instance = this;
			gameObject.SetActive(false);
		}

		void Start () {
		}

		// Update is called once per frame
		void Update () {
			if(InputEnabled) {
				float fRatio = Mathf.PingPong(Time.time*2.0f, 1.0f)*0.25f+0.5f;
				Color color = new Color(fRatio,fRatio,fRatio,1.0f);
				DoubleBackground.color = color;
				QuadBackground.color = color;
			}
		}

		void Shuffle() {
			Deck.Clear();

			for(int i=0 ; i < 52 ; ++i)
				Deck.Add(i);

			Deck.Sort ((x, y) => Random.value < 0.5f ? -1 : 1);
			CardIndexInDeck = 0;
		}

		void InputEnable(bool bEnable) {
			InputEnabled = bEnable;

			for(int i=0 ; i < Buttons.Length ; ++i) {
				Buttons[i].interactable = bEnable;
			}
		}

		void UpdateText() {
			Title.text = "Win <color=yellow>"+CoinWins.ToString ("#,##0.0")+"</color> Credits";
			DoubleText.text = "Double to \n<color=yellow>"+(CoinWins*2.0f).ToString ("#,##0.0")+"</color> credits";
			QuadText.text = "Quadruple to \n<color=yellow>"+(CoinWins*2.0f).ToString ("#,##0.0")+"</color> credits";

			if(SelectCount == SelectCountMax) {
				DoubleText.text = "";
				QuadText.text = "";
			}
		}

		void ShowResult(bool bSuccess, float fMultiply) {
			if(InShowResult) return;

			if(bSuccess)
				BEAudioManager.SoundPlay(3);

			InShowResult = true;
			InputEnable(false);
			SelectCount++;

			CardCenter.Flip ();

			if(bSuccess && (SelectCount < SelectCountMax)) {
				CoinWins *= fMultiply;
				UpdateText ();
				animator.Play ("Move");
			}
			else {
				// in case 5th choice
				if(bSuccess) {
					CoinWins *= fMultiply;
					SceneSlotGame.instance.OnDoubleGameEnd(CoinWins-CoinStart);
				}
				else  {
					CoinWins *= 0.0f;
					SceneSlotGame.instance.OnDoubleGameEnd(-CoinStart);
				}

				UpdateText ();
				StartCoroutine(HideDelay(1.5f));
			}
		}

		public IEnumerator HideDelay(float fDelay) {
			if(fDelay > 0.01f)
				yield return new WaitForSeconds(fDelay);

			animator.Play ("Hide");
		}


		public void CardMoveAnimationEnd() {
			//Debug.Log ("CardMoveAnimationEnd");
			animator.Play ("Normal");

			CardsRight[0].SetSymbolNumber(CardsRight[1].GetIndexof52());
			CardsRight[1].SetSymbolNumber(CardsRight[2].GetIndexof52());
			CardsRight[2].SetSymbolNumber(CardsRight[3].GetIndexof52());
			CardsRight[3].SetSymbolNumber(CardsRight[4].GetIndexof52());
			CardsRight[4].SetSymbolNumber(CardCenter.GetIndexof52());
			CardCenter.SetSymbolNumber(CardDeck.GetIndexof52());
			CardDeck.SetSymbolNumber(Deck[CardIndexInDeck++]);
			CardDeck.SetSide(false);
			CardCenter.SetSide(false);

			InputEnable(true);
			InShowResult = false;
		}

		public void OnButtonDouble(int value) {
			BEAudioManager.SoundPlay(0);
			bool bSuccess = false;
			if(value == 0)  bSuccess =  UICard.isRedColor(CardCenter.Symbol);
			else 			bSuccess = !UICard.isRedColor(CardCenter.Symbol);

			ShowResult(bSuccess, 2.0f);
		}

		public void OnButtonQuad(int value) {
			BEAudioManager.SoundPlay(0);
			bool bSuccess = ((CardType)value == CardCenter.Symbol) ? true : false;

			ShowResult(bSuccess, 4.0f);
		}

		public void OnButtonTake() {
			BEAudioManager.SoundPlay(0);
			SceneSlotGame.instance.OnDoubleGameEnd(CoinWins-CoinStart);
			animator.Play ("Hide");
		}

		public void Hide() {
			//Debug.Log ("Hide");
			gameObject.SetActive(false);
			SceneSlotGame.uiState = 0;
		}

		void _Show (float Coin) {
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.SetActive(true);
			SceneSlotGame.uiState = 1;

			CoinStart = Coin;
			CoinWins = Coin;
			SelectCount = 0;
			InShowResult = false;

			Shuffle();
			for(int i=0 ; i < CardsRight.Length ; ++i) {
				CardsRight[i].SetSymbolNumber(Deck[CardIndexInDeck++]);
			}
			CardCenter.SetSymbolNumber(Deck[CardIndexInDeck++]);
			CardDeck.SetSymbolNumber(Deck[CardIndexInDeck++]);
			CardDeck.SetSide(false);
			CardCenter.SetSide(false);

			UpdateText();
			animator.Play("Show");

			InputEnable(true);
		}

		public static void Show(float Coin) { instance._Show(Coin); }
	}
}