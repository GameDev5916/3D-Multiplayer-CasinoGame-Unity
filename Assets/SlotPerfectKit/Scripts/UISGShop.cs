using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BE {
	[System.Serializable]
	public class ShopItemDef {
		public string 		name;
		public int 			coin;
		public int 			bonus;
		public int 			total;
		public string  		price;

		public ShopItemDef(string _name, int _coin, int _bonus, string _price) {
			name 	= _name;
			coin	= _coin;
			bonus	= _bonus;
			total 	= (int)((float)coin * (float)(100+bonus)/100.0f);
			price	= _price;
		}
	}

	public class UISGShop : MonoBehaviour {
		public	static UISGShop instance;

		public	Toggle [] 	Toggles;
		public	Image 		Dialog;
		private List<ShopItemDef> 	ItemList=new List<ShopItemDef>();
		private int			Selected = -1;

		void Awake () {
			instance=this;
			gameObject.SetActive(false);

			ItemList.Add(new ShopItemDef("shopitem0", 300000, 30, "99.99$"));
			ItemList.Add(new ShopItemDef("shopitem0", 120000, 20, "49.99$"));
			ItemList.Add(new ShopItemDef("shopitem0",  30000, 15, "19.99$"));
			ItemList.Add(new ShopItemDef("shopitem0",  12000, 10, "9.99$"));
			ItemList.Add(new ShopItemDef("shopitem0",   5000,  5, "4.99$"));
			ItemList.Add(new ShopItemDef("shopitem0",   1500,  2, "1.99$"));

			for(int i=0 ; i < Toggles.Length ; ++i) {
				Text textGold = Toggles[i].transform.Find ("LabelGold").GetComponent<Text>();
				textGold.text = ItemList[i].coin.ToString ("##0,0");
				Text textBonus = Toggles[i].transform.Find ("LabelBonus").GetComponent<Text>();
				textBonus.text = ItemList[i].bonus.ToString ()+" % Bonus";
				Text textTotal = Toggles[i].transform.Find ("LabelTotal").GetComponent<Text>();
				textTotal.text = ItemList[i].total.ToString ("##0,0");
				Text textPrice = Toggles[i].transform.Find ("LabelPrice").GetComponent<Text>();
				textPrice.text = ItemList[i].price;
			}
		}

		void Start () {
		}

		void Update () {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				Hide();
			}
		}

		public void ItemSelected(int value) {
			if(SceneSlotGame.uiState == 0)
				return;

			BEAudioManager.SoundPlay(0);
			if(Toggles[value].isOn) {
				Selected = value;
				//Debug.Log ("UISGShop::ItemSelected"+value.ToString ());
			}
		}

		public void OnButtonBuy() {
			//Debug.Log ("UISGShop::OnButtonBuy");
			BEAudioManager.SoundPlay(0);
			if(Selected == -1) return;

			BESetting.Gold.ChangeDelta(ItemList[Selected].total);
			BESetting.Save();
			Hide();
		}

		public void Hide() {
			gameObject.SetActive(false);
			SceneSlotGame.uiState = 0;
		}

		void _Show () {
			Selected = -1;
			for(int i=0 ; i < Toggles.Length ; ++i) {
				Toggles[i].isOn = false;
			}

			gameObject.transform.localPosition = Vector3.zero;
			gameObject.SetActive(true);
			SceneSlotGame.uiState = 1;
			StartCoroutine(BEUtil.instance.ImageScale(Dialog, Dialog.color, 1.0f, 1.1f, 1.0f, 0.1f, 0.0f));
		}

		public static void Show() { instance._Show(); }
	}
}
