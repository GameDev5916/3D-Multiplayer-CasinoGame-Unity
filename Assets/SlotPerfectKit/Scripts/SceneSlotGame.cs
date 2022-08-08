using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SimpleJSON;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          SceneSlotGame
///   Description:    process user input & display result
///   Usage :
///   Author:         BraveElephant inc.
///   Version: 		  v1.0 (2015-08-30)
///-----------------------------------------------------------------------------------------
namespace BE {
	public class SceneSlotGame : MonoBehaviour {
		public	static SceneSlotGame instance;

		public	static int 		uiState = 0; 	// popup window shows or not
		public  static BENumber	Win;			// total win number

		public 	SlotGame	Slot;			// slot game class

		public	Text		textLine;		// user selected line info text
		public	Text		textBet;		// current betting info text
		public	Text		textTotalBet;	// total betting info
		public	Text		textTotalWin;	// total win info
		public	Text		textGold;		// user gold info
		public	Text		textInfo;		// other info text

		public 	Button		btnBuy;			// buy gold button
		public 	Button		btnMenu;		// call setting dialog
		public 	Button		btnPayTable;	// show pay table info dialog
		public 	Button		btnMaxLine;		// line count to max number
		public 	Button		btnLines;		// change line selected
		public 	Button		btnBet;			// change bet
		public 	Button		btnDouble;		// start double game
		public 	Button		btnSpin;		// start spin
		public 	Toggle		toggleAutoSpin;	// auto spin toggle button

		public 	GameObject	FreeSpinBackground;	// background og game scene

		void Awake () {
			instance=this;
		}

		void Start () {
			// set range of numbers and type
			BESetting.Gold.AddUIText(textGold);
			Win = new BENumber(BENumber.IncType.VALUE, "#,##0.00", 0, 10000000000, 0);
			Win.AddUIText(textTotalWin);

			//set saved user gold count to slotgame
			Slot.Gold = (float)BESetting.Gold.Target();
			//set win value to zero
			Win.ChangeTo(0);

			UpdateUI ();

			//double button show only user win
			btnDouble.gameObject.SetActive (false);
			textInfo.text = "";
		}

		void Update () {
			// if user press 'escape' key, show quit message window
			if ((uiState==0) && Input.GetKeyDown(KeyCode.Escape)) {
				UISGMessage.Show("Quit", "Do you want to quit this program ?", MsgType.OkCancel, MessageQuitResult);
			}

			Win.Update();

			// if auto spin is on or user has free spin to run, then start spin
			if((toggleAutoSpin.isOn || Slot.gameResult.InFreeSpin()) && Slot.Spinable()) {
				//Debug.Log ("Update Spin");
				OnButtonSpin();
			}
		}

		// when user pressed 'ok' button on quit message.
		public void MessageQuitResult(int value) {
			//Debug.Log ("MessageQuitResult value:"+value.ToString ());
			if(value == 0) {
				Application.Quit ();
			}
		}

		//user clicked shop button, then show shop
		public void OnButtonShop() {
			BEAudioManager.SoundPlay(0);
			UISGShop.Show();
		}

		// user clicked option button, then show option
		public void OnButtonOption() {
			BEAudioManager.SoundPlay(0);
			UISGOption.Show();
		}

		// user clicked paytable button, then show paytable
		public void OnButtonPayTable() {
			BEAudioManager.SoundPlay(0);
			UISGPayTable.Show(Slot);
		}

		// user clicked Max line button, then ser slot's line count to max
		public void OnButtonMaxLine() {
			BEAudioManager.SoundPlay(0);
			Slot.LineSet(Slot.Lines.Count);
			UpdateUI();
		}

		//user clicked line button, increase line count
		public void OnButtonLines() {
			BEAudioManager.SoundPlay(0);
			Slot.LineSet(Slot.Line+1);
			UpdateUI();
		}

		// user clicked bet button, then increase bet number
		public void OnButtonBet() {
			BEAudioManager.SoundPlay(0);
			Slot.BetSet(Slot.Bet+1);
			UpdateUI();
		}

		//user clicked double button, then start double game
		public void OnButtonDouble() {
			BEAudioManager.SoundPlay(0);
			UIDoubleGame.Show(Slot.gameResult.GameWin);
		}

		// user clicked spin button
		public void OnButtonSpin()
		{
			NewSloatManager.Instance.WinAmountText.text = "0";
			NewSloatManager.Instance.WinAmount = 0;
			SlotGame.instance.Gold = 10000;
            //Debug.LogError("Bet : " + SlotGame.instance.TotalBet);
            if (NewSloatManager.Instance.TotalBetAMount < 1)
			{
				Debug.LogError("Please Bet First");
				Constants.ShowWarning("Minimum Bet Amount is 10,000");
				return;
			}

			if (NewSloatManager.Instance.TotalBetAMount > Constants.CHIPS)
			{
				Debug.LogError("Not Have Enough Money:");
				Constants.ShowWarning("Not Have Enough Chips For Bet");
				return;
			}

			// start spin
			SlotReturnCode code = Slot.Spin();
			// if spin succeed
			if (SlotReturnCode.Success == code)
			{
				JSONNode data = new JSONObject
				{
					["slot_num"] = Constants.SelectedSlot,
				};
                Debug.LogError(data.ToString());
				StartCoroutine(Constants.ApiCall(Constants.API_Updateslot, data.ToString(), (bool IsSuccess, string result) =>
				{
					if (IsSuccess)
					{
						JSONNode jsonNode = JSON.Parse(result)["slot"];
						Debug.LogError(jsonNode.ToString());
					}
					else
					{
						Debug.LogError("Faild");
					}
				}));

				NewSloatManager.Instance.DisableAllButton();


				BEAudioManager.SoundPlay(0);
				Constants.CHIPS -= NewSloatManager.Instance.TotalBetAMount;
				// disabled inputs
				ButtonEnable(false);
				btnDouble.gameObject.SetActive(false);
				UpdateUI();
				// apply decreased user gold
				//BESetting.Gold.ChangeTo(Slot.Gold);
				BESetting.Save();

				//// set info text
				//if (Slot.gameResult.InFreeSpin()) textInfo.text = "Free Spin " + Slot.gameResult.FreeSpinCount.ToString() + " of " + Slot.gameResult.FreeSpinTotalCount.ToString();
				//else textInfo.text = "";
			}
            //else
            //{
            //    // if spin fails
            //    // show Error Message
            //    if (SlotReturnCode.InSpin == code) { UISGMessage.Show("Error", "Slot is in spin now.", MsgType.Ok, null); }
            //    else if (SlotReturnCode.NoGold == code) { UISGMessage.Show("Error", "Not enough gold.", MsgType.Ok, null); }
            //    else { }
            //}



            // Orignal

            //BEAudioManager.SoundPlay(0);
            //// start spin
            //SlotReturnCode code = Slot.Spin();
            //// if spin succeed
            //if(SlotReturnCode.Success == code) {
            //	// disabled inputs
            //	ButtonEnable(false);
            //	btnDouble.gameObject.SetActive (false);
            //	UpdateUI();
            //	// apply decreased user gold
            //	BESetting.Gold.ChangeTo(Slot.Gold);
            //	BESetting.Save();

            //	// set info text
            //	if(Slot.gameResult.InFreeSpin()) 	textInfo.text = "Free Spin "+Slot.gameResult.FreeSpinCount.ToString ()+" of "+Slot.gameResult.FreeSpinTotalCount.ToString ();
            //	else 								textInfo.text = "";
            //}
            //else {
            //	// if spin fails
            //	// show Error Message
            //	if(SlotReturnCode.InSpin == code) 		{ UISGMessage.Show("Error", "Slot is in spin now.", MsgType.Ok, null); }
            //	else if(SlotReturnCode.NoGold == code) 	{ UISGMessage.Show("Error", "Not enough gold.", MsgType.Ok, null); }
            //	else {}
            //}
        }

		// if user clicked auto spin
		public void AutoSpinToggled(bool value) {
			BEAudioManager.SoundPlay(0);
		}

		// update ui text & win number
		public void UpdateUI() {
			textLine.text = Slot.Line.ToString ();
			textBet.text = Slot.RealBet.ToString ("#0.00");
			textTotalBet.text = Slot.TotalBet.ToString ("#0.00");
			Win.ChangeTo(Slot.gameResult.GameWin);
            //Debug.LogError((long)Slot.gameResult.GameWin);
            NewSloatManager.Instance.UpdateWinAmount((long)Slot.gameResult.GameWin);
		}

		// enable or disable button inputs
		public void ButtonEnable(bool bEnable) {
			btnBuy.interactable = bEnable;
			btnMenu.interactable = bEnable;
			btnPayTable.interactable = bEnable;
			btnMaxLine.interactable = bEnable;
			btnLines.interactable = bEnable;
			btnBet.interactable = bEnable;
			btnSpin.interactable = bEnable;
		}

		//------------------------------------------
		//callback functions
		// when double game ends
		public void OnDoubleGameEnd(float delta) {
			//Debug.Log("OnDoubleGameEnd:"+delta.ToString ());

			//change user gold by delta (change gold value)
			Slot.Gold += delta;
			BESetting.Gold.ChangeTo(Slot.Gold);
			BESetting.Save();
			Slot.gameResult.GameWin += delta;
			Win.ChangeTo(Slot.gameResult.GameWin);
			btnDouble.gameObject.SetActive (false);
		}

		// when reel stoped
		public void OnReelStop(int value) {
			//Debug.Log ("OnReelStop:"+value.ToString());
			BEAudioManager.SoundPlay(2);
        }

		// when spin completed
		public void OnSpinEnd() {
			//Debug.Log("OnSpinEnd");

			// if user has win
			if(Slot.gameResult.Wins.Count != 0)
				textInfo.text = "Win "+Slot.gameResult.Wins.Count.ToString ()+" Lines ";
            //Debug.LogError("Win Count : "+Slot.gameResult.Wins.Count);
            UpdateUI();
			// increase user gold
			BESetting.Gold.ChangeTo(Slot.Gold);
			BESetting.Save();

			NewSloatManager.Instance.EnableAllButton();
		}

		//when splash window shows
		public void OnSplashShow(int value) {
			//Debug.Log ("OnSplashShow:"+value.ToString());
			//BEAudioManager.SoundPlay(3);
			//UISGSplash.Show (value);

			//// change background image if free spin
			//if(value == (int)SplashType.FreeSpin) {
			//	FreeSpinBackground.SetActive(true);
			//}
			//else if(value == (int)SplashType.FreeSpinEnd) {
			//	FreeSpinBackground.SetActive(false);
			//}
			//else {}
		}

		// when splash hide
		public void OnSplashHide(int value) {
			//Debug.Log ("OnSplashHide:"+value.ToString());
			StartCoroutine(SlotSplashHide(value, 0.5f));
		}

		// when all splash works end
		public void OnSplashEnd() {
			//Debug.Log ("OnSplashEnd");
			if(Slot.gameResult.InFreeSpin()) {
				textInfo.text = "Free Spin "+Slot.gameResult.FreeSpinCount.ToString ()+" of "+Slot.gameResult.FreeSpinTotalCount.ToString ();
			}
			else {
				textInfo.text = "";
				btnDouble.gameObject.SetActive ((Slot.gameResult.GameWin > 0.001f) ? true : false);
				ButtonEnable(true);
			}
		}

		// splash idx change
		public IEnumerator SlotSplashHide(int value, float fDelay) {
			if(fDelay > 0.01f)
				yield return new WaitForSeconds(fDelay);

			Slot.SplashCount[value] = 0;
			Slot.SplashActive++;
			Slot.InSplashShow = false;
		}
	}
}
