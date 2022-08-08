using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BE {
	public class UISGPayTable : MonoBehaviour {
		public	static UISGPayTable instance;

		private SlotGame 		game = null;
		public	GameObject 		prefabSymbolItem;
		public	GameObject 		prefabLineItem;
		public	Image 			Dialog;
		public 	RectTransform	rtSymbolScrollContent;
		public 	RectTransform	rtLineScrollContent;

		void Awake () {
			instance=this;
			gameObject.SetActive(false);
		}

		void Start () {
			int iCount = 0;
			for(int i=0 ; i < game.Symbols.Count ; ++i) {
				Symbol sd = game.GetSymbol(i);
				if(sd.type != SymbolType.Normal) continue;

				GameObject go = (GameObject)Instantiate(prefabSymbolItem, Vector3.zero, Quaternion.identity);
				go.transform.SetParent(rtSymbolScrollContent);
				go.transform.localPosition = new Vector3(0,-125*iCount,0);
				go.transform.localScale = Vector3.one;
				go.name = "Symbol"+i.ToString ();

				Image img = go.transform.Find ("Symbol").GetComponent<Image>();
				img.sprite = sd.prfab.GetComponent<Image>().sprite;
				for(int j=1 ; j < 5 ; ++j) {
					Text text = go.transform.Find ("Text"+(j+1).ToString ()).GetComponent<Text>();
					text.text = (sd.reward[j] == 0) ? "" : (j+1).ToString ()+" = "+sd.reward[j].ToString ("#,##0");
				}

				iCount++;
			}
			rtSymbolScrollContent.sizeDelta = new Vector2(450, 125*iCount);

			iCount = 0;
			for(int i=0 ; i < game.Lines.Count ; i+=2) {
				GameObject go = (GameObject)Instantiate(prefabLineItem, Vector3.zero, Quaternion.identity);
				go.transform.SetParent(rtLineScrollContent);
				go.transform.localPosition = new Vector3(0,-80*iCount,0);
				go.transform.localScale = Vector3.one;
				go.name = "Symbol"+i.ToString ();

				Line ld = game.GetLine(i);
				GameObject goLeft = go.transform.Find ("Left").gameObject;
				Text text = goLeft.transform.Find ("ID").GetComponent<Text>();
				text.text = (i+1).ToString ();
				text.color = ld.color;
				for(int j=0 ; j < 5 ; ++j) {
					Image img = goLeft.transform.Find ("Row"+ld.Slots[j].ToString ()).transform.Find ("Image"+j.ToString ()).GetComponent<Image>();
					img.color = ld.color;
				}

				GameObject goRight = go.transform.Find ("Right").gameObject;
				if(i+1 >= SlotGame.instance.Lines.Count) {
					goRight.SetActive(false);
				}
				else {
					ld = game.GetLine(i+1);
					text = goRight.transform.Find ("ID").GetComponent<Text>();
					text.text = (i+2).ToString ();
					text.color = ld.color;
					for(int j=0 ; j < 5 ; ++j) {
						Image img = goRight.transform.Find ("Row"+ld.Slots[j].ToString ()).transform.Find ("Image"+j.ToString ()).GetComponent<Image>();
						img.color = ld.color;
					}
				}

				iCount++;
			}
			rtLineScrollContent.sizeDelta = new Vector2(450, 80*iCount);
		}

		void Update () {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				Hide();
			}
		}

		void ImageShow(int id) {
			//ImageID = id;
		}

		public void OnButtonPrev() {
			BEAudioManager.SoundPlay(0);
		}

		public void OnButtonNext() {
			BEAudioManager.SoundPlay(0);
		}

		public void OnButtonBackToGame() {
			BEAudioManager.SoundPlay(0);
			Hide();
		}

		public void Hide() {
			gameObject.SetActive(false);
			SceneSlotGame.uiState = 0;
		}

		void _Show (SlotGame _game) {
			game = _game;
			ImageShow(0);

			gameObject.transform.localPosition = Vector3.zero;
			gameObject.SetActive(true);
			SceneSlotGame.uiState = 1;
			StartCoroutine(BEUtil.instance.ImageScale(Dialog, Dialog.color, 1.0f, 1.1f, 1.0f, 0.1f, 0.0f));
		}

		public static void Show(SlotGame game) { instance._Show(game); }
	}
}
