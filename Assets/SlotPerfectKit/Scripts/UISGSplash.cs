using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BE {
	public class UISGSplash : MonoBehaviour {
		private static UISGSplash instance;
		public	Image 	Dialog;
		public	Text 	Title;
		public	Text 	Info;
		public	Text 	Info2;

		private int		Type = -1;
		private float	fAge = 0.0f;
		private float	fLife = 1.6f;

		void Start () {
			instance=this;
			gameObject.SetActive(false);
		}

		void Update () {
			fAge += Time.deltaTime;
			if(fAge > fLife) {
				Hide();
			}
		}

		public void Hide() {
			gameObject.SetActive(false);
			SceneSlotGame.instance.OnSplashHide(Type);
			SceneSlotGame.uiState = 0;
		}

		void _Show (int type) {
			//gameObject.transform.localPosition = Vector3.zero;
			//gameObject.SetActive(true);
			//SceneSlotGame.uiState = 1;
			//Type = type;
			//fAge = 0.0f;

			//SplashType st = (SplashType)Type;

			//if(st == SplashType.FiveInRow) {
			//	Info.text = "<color=yellow>5 in Row !!!</color>";
			//	Info2.text = "";
			//}
			//else if(st == SplashType.BigWin) {
			//	Info.text = "<color=yellow>BigWin !!!</color>";
			//	Info2.text = "";
			//}
			//else if(st == SplashType.FreeSpin) {
			//	Info.text = "<color=yellow>"+SlotGame.instance.gameResult.NewFreeSpinCount.ToString()+"</color>";
			//	Info2.text = "Free Spins Won !";
			//}
			//else if(st == SplashType.FreeSpinEnd) {
			//	Info.text = "<color=yellow>"+SlotGame.instance.gameResult.FreeSpinTotalWins.ToString()+" Won !!! </color>";
			//	Info2.text = SlotGame.instance.gameResult.FreeSpinTotalCount.ToString()+" Free Spins Completed !";
			//}
			//else {}


			//StartCoroutine(BEUtil.instance.ImageScale(Dialog, Dialog.color, 1.0f, 1.1f, 1.0f, 0.1f, 0.0f));
		}

		public static void Show(int type) { instance._Show(type); }
	}
}