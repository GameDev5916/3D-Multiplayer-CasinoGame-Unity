using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using MersenneTwister;


///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          SlotGame
///   Description:    All about slot game logic
///   Usage :
///   Author:         BraveElephant inc.
///   Version: 		  v1.0 (2015-08-30)
///-----------------------------------------------------------------------------------------
namespace BE {
	// spin result code
	public enum SlotReturnCode {
		Success 	= 0,	// no problem
		InSpin 		= 1,	// is in spin now
		NoGold 		= 2,	// not enough gold
	};

	// random number generation type
	public enum RNGType {
		UnityRandom 	= 0,
		DotNetRandom 	= 1,
		MersenneTwister	= 2,
	};

	// symbol types
	public enum SymbolType {
		Normal 		= 0,
		Wild 		= 1,
		Wild2X 		= 2,
		Wild3X 		= 3,
		Scatter	 	= 4,
		Bonus 		= 5,
		WildFC 		= 6, // added
	};

	// define a symbol
	[System.Serializable]
	public class Symbol {
		public  GameObject  prfab;
		public 	SymbolType	type;					// symbol types
		public  bool		frequencyPerReel;		// set different frequency to each reel
		public  int []      frequency= new int[10]; // showing probability (permillage)
		public  int []      reward = new int[10];	// reqard gold or freespin count

		public Symbol(string _path, SymbolType _type, string _frequency, string _reward) {
			BaseSet(_path, _type, _reward);

			frequencyPerReel = true;
			string [] frequencySub = _frequency.Split(',');
			for(int i=0 ; i < 10 ; ++i)
				frequency[i] = int.Parse(frequencySub[i]);
		}

		public Symbol(string _path, SymbolType _type, int _frequency, string _reward) {
			BaseSet(_path, _type, _reward);

			frequencyPerReel = false;
			for(int i=0 ; i < 10 ; ++i)
				frequency[i] = _frequency;
		}

		private void BaseSet(string _path, SymbolType _type, string _reward) {
			type = _type;
			SetPrefab((GameObject)Resources.Load (_path, typeof(GameObject)));

			string [] rewardSub = _reward.Split(',');
			for(int i=0 ; i < rewardSub.Length ; ++i)
				reward[i] = int.Parse(rewardSub[i]);
		}

		// is thos symbol is wild card ?
		public bool IsWild() 	{
			return ((type == SymbolType.Wild) || (type == SymbolType.Wild2X) || (type == SymbolType.Wild3X)) ? true : false;
		}

		public void SetPrefab(GameObject go) {
			prfab = go;
			if(go != null)
				BEObjectPool.AddPoolItem(go, 20);
		}
	}

	// define a line
	[System.Serializable]
	public class Line {
		public int [] 	Slots = new int [10];
		public Color 	color;

		public Line(int s0, int s1, int s2, int s3, int s4, string strColor) {
			Slots[0] = s0;
			Slots[1] = s1;
			Slots[2] = s2;
			Slots[3] = s3;
			Slots[4] = s4;
			color = HexToColor(strColor);
		}

		public Color HexToColor(string hex)
		{
			byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
			return new Color32(r,g,b, 255);
		}
	}

	// define marker - indicate line number
	[System.Serializable]
	public class Marker {
		public 	GameObject 	go;
		public 	int			index;
		public 	Image		image;
		public 	Text		text;

		public Marker(int _index, Line ld, GameObject _go) {
			index = _index;
			go = _go;
			image = go.GetComponent<Image>();
			text = go.transform.Find ("Text").GetComponent<Text>();

			image.color = ld.color;
			text.text = (index+1).ToString ();
		}
	}

	// splash window types
	public enum SplashType {
		None		= -1,
		FiveInRow	= 0,	// same symbols are five in row
		BigWin      = 1,
		Line        = 2,
		FreeSpin    = 3,	// free spin start
		FreeSpinEnd = 4,	// free spin end(result)
		Max			= 5,
	};

	// win class - which line is matched
	[System.Serializable]
	public class WinItem {
		public  int		Line;		// line id
		public  int		SymbolIdx;	// symbol id
		public  int		Matches;	// match count
		public  float	WinGold;	// win reward

		public WinItem(int _Line, int _SymbolIdx, int _Matches, float _WinGold) {
			Line 		= _Line;
			SymbolIdx	= _SymbolIdx;
			Matches 	= _Matches;
			WinGold 	= _WinGold;
		}
	}

	public class TestItem {
		public  int		ID;
		public  int		Hit;
		public  float	Bet;
		public  float	WinGold;

		public TestItem(int _ID) {
			ID 		= _ID;
			Hit		= 0;
			Bet 	= 0.0f;
			WinGold = 0.0f;
		}
	}

	// Core of Slot game - divided from SlotGame to simulate return rate
	public class GameResult {
		public 	float 			GameWin = 0.0f;
		public 	float 			SpinWin = 0.0f;
		public	List<WinItem> 	Wins=new List<WinItem>();
		public	int 			NewFreeSpinCount = 0;
		public	int 			FreeSpinCount = 0;
		public	int 			FreeSpinTotalCount = 0;
		public	float 			FreeSpinTotalWins = 0.0f;
		public	int 			FreeSpinAccumCount = 0;
		public	float 			FreeSpinAccumWins = 0.0f;

		public	int				Line = 0;
		public	float			RealBet = 0.0f;
		public	float			TotalBet = 0.0f;

		public	List<TestItem> 	TestSymbols=new List<TestItem>();
		public	List<TestItem> 	TestLines=new List<TestItem>();

		//reset all game related variables
		public void ResetGame() {
			GameWin = 0.0f;
			ResetSpin();
			FreeSpinCount = 0;
			FreeSpinTotalCount = 0;
			FreeSpinTotalWins = 0.0f;
			FreeSpinAccumCount = 0;
			FreeSpinAccumWins = 0.0f;
		}

		//reset single spin related variables
		public void ResetSpin() {
			SpinWin = 0.0f;
			Wins.Clear();
			NewFreeSpinCount = 0;
		}

		public void Spin() {
			// if not freespin, reset freespin related variables
			if(FreeSpinCount > FreeSpinTotalCount) {
				FreeSpinCount = 0;
				FreeSpinTotalCount = 0;
				FreeSpinTotalWins = 0.0f;
			}
		}

		public bool InFreeSpin() {
			return ((FreeSpinTotalCount != 0) && (FreeSpinCount <= FreeSpinTotalCount)) ? true : false;
		}
	}


	[System.Serializable]
	public class IntEvent : UnityEvent<int> {}

	[System.Serializable]
	public class SlotGame : MonoBehaviour {
		public	static SlotGame instance;

		public 	GameObject 		prefMarker;		// prefab of marker
		public 	UILineRenderer 	line;			// line render (show matched line (
		public 	Image 			imgSelection;
		private Transform 		tr;

		public	List<Symbol> 	Symbols=new List<Symbol>();
		public	List<Line> 		Lines=new List<Line>();
		private	List<Marker> 	Markers=new List<Marker>();

		public	int				LineMin;
		public	int				Line;
		public	List<float>		BetTable=new List<float>();
		public	int				Bet;
		public	float			RealBet;
		public	long			TotalBet;
		[HideInInspector]
		public	float 			Gold = 2000.0f;

		public 	int 			SymbolSize = 160;
		public 	int 			MarginSize = 25;
		public	float 			SymbolSizePlusMargin;
		public	float 			DampingHeight;

		public 	RNGType			rngType = RNGType.UnityRandom;
		private	System.Random 	randomDotNet = new System.Random();
		private	MT19937 		randomMersenneTwister = new MT19937();

		public	List<UISGReel>	Reels=new List<UISGReel>();
		public	int				RowCount=3;
		private	bool			InSpin = false;
		private float			SpinAge = 0.0f;
		private int				ActiveReel = -1;
		public 	float 			BoundSpeed = 30.0f;
		public 	float 			Acceleration = 3000.0f;
		public 	float 			SpeedMax = 2000.0f;
		public 	float 			MinimumRotateDistance = 640.0f;
		public 	float 			MinimumRotateDistancePredict = 4000.0f;

		private bool			InResult = false;
		public	int []			SplashCount = new int[(int)SplashType.Max];
		public	int 			SplashActive = -1;
		public	bool 			InSplashShow = false;

		private int 			WinOffset = 0;
		private float 			WinShowAge = 0.0f;
		private int 			WinShowBlinkCount = 3;
		private float 			WinShowPeriod = 2f;

		public	IntEvent		ReelStopCallback = null;
		public	UnityEvent 		SpinEndCallback = null;
		public	IntEvent		SplashShowCallback = null;
		public	UnityEvent 		SplashEndCallback = null;

		private BEChoice [] 	choice = null;
		private List<int> 		Deck = new List<int>();
		public 	GameResult 		gameResult = new GameResult();
		public 	GameResult 		resultTest = null;

		public 	int 			SimulationCount=10;

		public void SetReelCount(int value) {
			// set reel count range 3 ~ 10
			value = Mathf.Clamp(value, 3, 10);
			//Debug.Log ("SetReelCount "+value.ToString ());
			for(int x=0 ; x < Reels.Count ; ++x) {
				if((Reels[x] == null) || (Reels[x].gameObject == null)) continue;

				for(int y=0 ; y < Reels[x].Symbols.Length ; ++y) {
					if(Reels[x].Symbols[y] != null)
						BEObjectPool.Unspawn(Reels[x].Symbols[y]);
				}
				DestroyImmediate(Reels[x].gameObject);
			}
			Reels.Clear();

			for(int x=0 ; x < value ; ++x) {
				GameObject go = new GameObject("Reel"+x.ToString ());
				go.AddComponent<RectTransform>();
				go.transform.SetParent(transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = Vector3.one;

				UISGReel reel = go.AddComponent<UISGReel>();
				reel.Init (this, x);
				Reels.Add (reel);
			}
			RepositionSymbols();
		}

		public void SetRowCount(int value) {
			// set row count range 3 ~ 7
			value = Mathf.Clamp(value, 3, 7);
			//Debug.Log ("SetRowCount "+value.ToString ());
			RowCount = value;

			for(int x=0 ; x < Reels.Count ; ++x) {
				if((Reels[x] == null) || (Reels[x].gameObject == null)) continue;

				for(int y=0 ; y < Reels[x].Symbols.Length ; ++y) {
					if(Reels[x].Symbols[y] != null)
						BEObjectPool.Unspawn(Reels[x].Symbols[y]);
				}

				Reels[x].Init (this, x);
			}
			RepositionSymbols();
		}

		public void SetSymbolSize(int value) { SymbolSize = value; RepositionSymbols(); }
		public void SetMarginSize(int value) { MarginSize = value; RepositionSymbols(); }

		public void RepositionSymbols() {
			//Debug.Log ("RepositionSymbols ");
			float fStart = -(float)(Reels.Count-1)*(float)(SymbolSize+MarginSize)/2.0f;
			for(int x=0 ; x < Reels.Count ; ++x) {
				if((Reels[x] == null) || (Reels[x].gameObject == null)) continue;
				Reels[x].gameObject.transform.localPosition = new Vector3(fStart,0,0);
				fStart += (float)(SymbolSize+MarginSize);

				float fYStart = -(float)(RowCount-1)*(float)(SymbolSize+MarginSize)/2.0f;
				for(int y=0 ; y < Reels[x].Symbols.Length ; ++y) {
					Reels[x].SymbolPos[y] = new Vector3(0,fYStart,0);
					Reels[x].Symbols[y].transform.localPosition = Reels[x].SymbolPos[y];
					fYStart += (float)(SymbolSize+MarginSize)-25;
				}
			}
		}

		public void SymbolInsert(int ID) 	{ if(Application.isPlaying) return; 	Symbols.Insert(ID, new Symbol("Noname", SymbolType.Normal, 0, "0,0,0,0,0")); }
		public int  SymbolRemove(int ID)  	{ if(Application.isPlaying) return 0; 	Symbols.RemoveAt(ID); 	return 1; }
		public void SymbolAdd() 			{ SymbolInsert(Symbols.Count); }
		public void SymbolRemove() 			{ SymbolRemove(Symbols.Count-1); }

		public void LineInsert(int ID) 		{ if(Application.isPlaying) return; 	Lines.Insert(ID, new Line(0,0,0,0,0,"000000")); }
		public int  LineRemove(int ID) 		{ if(Application.isPlaying) return 0; 	Lines.RemoveAt(ID); 	return 1; }
		public void LineAdd() 				{ LineInsert(Lines.Count); }
		public void LineRemove() 			{ LineRemove(Lines.Count-1); }

		public void BetInsert(int ID) 		{ if(Application.isPlaying) return; 	BetTable.Insert(ID, 1.0f); }
		public int  BetRemove(int ID) 		{ if(Application.isPlaying) return 0; 	BetTable.RemoveAt(ID); 	return 1; }
		public void BetAdd() 				{ BetInsert(BetTable.Count); }
		public void BetRemove() 			{ BetRemove(BetTable.Count-1); }


		void Awake () {
			instance = this;
			tr = transform;
			Array.Clear(SplashCount, 0, SplashCount.Length);
		}

		void Start () {
			CreateChoice();

			SymbolSizePlusMargin = (float)(SymbolSize+MarginSize);
			DampingHeight = (float)SymbolSize * 0.4f;

			MarkerCreate();
			LineSet(LineMin);
			BetSet(0);
			SetRandomSymbolToReel();
			MatchLineHide();

			line.Points = new Vector2[Reels.Count+2];

			gameResult.ResetGame();
		}

		void Update () {
			//Display Match Lines
			if (InSplashShow && (SplashActive == (int)SplashType.Line))
			{
				WinItem wi = gameResult.Wins[WinOffset];
				Line ld = GetLine(wi.Line);
				NewSloatManager.Instance.SetLine(wi.Line);
				float fValue = (float)WinShowBlinkCount * 2.0f / WinShowPeriod;
				float fScale = Mathf.PingPong(WinShowAge * fValue, 1.0f) * 0.2f + 1.0f;

				imgSelection.color = ld.color;
				imgSelection.transform.localPosition = Markers[wi.Line].go.transform.localPosition;

				line.color = ld.color;
				for (int x = 0; x < Reels.Count; ++x)
				{
					Vector3 vPos = transform.InverseTransformPoint(Reels[x].Symbols[ld.Slots[x]].transform.position);
					line.Points[x + 1] = new Vector2(vPos.x, vPos.y);

					int SymbolIdx = Deck[RowCount * x + ld.Slots[x]];
					Symbol sd = GetSymbol(Deck[RowCount * x + ld.Slots[x]]);
					if ((SymbolIdx == wi.SymbolIdx) || sd.IsWild())
					{
						if (Reels[x].SymbolFC != null)
						{
							Reels[x].SymbolFC.transform.localScale = new Vector3(fScale, fScale, 1);
						}
						else
						{
							Reels[x].Symbols[ld.Slots[x]].transform.localScale = new Vector3(fScale, fScale, 1);
							if (Reels[x].Symbols[ld.Slots[x]].transform.childCount == 0)
							{
								NewSloatManager.Instance.ShowPartical(Reels[x].Symbols[ld.Slots[x]].transform);
							}
						}
					}
				}

				if (imgSelection.transform.localPosition.x < 0.0f)
				{
					Vector3 vPos = transform.InverseTransformPoint(imgSelection.transform.position);
					//vPos.x += imgSelection.rectTransform.sizeDelta.x*0.5f;
					line.Points[0] = new Vector2(vPos.x, vPos.y);
					line.Points[Reels.Count + 1] = line.Points[Reels.Count];
				}
				else
				{
					Vector3 vPos = transform.InverseTransformPoint(imgSelection.transform.position);
					//vPos.x -= imgSelection.rectTransform.sizeDelta.x*0.5f;
					line.Points[0] = line.Points[1];
					line.Points[Reels.Count + 1] = new Vector2(vPos.x, vPos.y);
				}

				WinShowAge += Time.deltaTime;
				if (WinShowAge > WinShowPeriod)
				{
					WinShowAge -= WinShowPeriod;

					WinOffset++;
					if (WinOffset >= gameResult.Wins.Count)
					{
						WinOffset = 0;

						SplashCount[SplashActive] = 0;
						InSplashShow = false;
					}
				}

				if(InSplashShow)
                {
                    if(NewSloatManager.Instance.SpinButton.interactable == true)
						NewSloatManager.Instance.DisableAllButton();
				}
				else
                {
					NewSloatManager.Instance.EnableAllButton();
					NewSloatManager.Instance.DisableLine();
				}
			}


			// display splash info
			if(!InSpin && InResult && !InSplashShow) {
				//Debug.Log ("SplashActive : "+SplashActive.ToString () + " SplashCount[SplashActive]:"+SplashCount[SplashActive].ToString () + " InSplashShow:"+InSplashShow.ToString ());
				if(SplashActive >= (int)SplashType.Max) {
					InResult = false;

					if(SplashEndCallback != null) {
						SplashEndCallback.Invoke();
						MatchLineHide();
						NewSloatManager.Instance.DisableLine();
					}
				}
				else {
					// if no display info go to next
					if(SplashCount[SplashActive] == 0) {
						SplashActive++;
					}
					else {
						if(SplashShowCallback != null) {
							//Debug.Log ("SplashShow : "+((SplashType)SplashActive).ToString ());
							InSplashShow = true;

							if(SplashActive != (int)SplashType.Line)
								SplashShowCallback.Invoke(SplashActive);
						}
						else {
							SplashCount[SplashActive] = 0;
							SplashActive++;
						}
					}
				}

				return;
			}

			if(!InSpin) return;

			if(ActiveReel == -1) {
				SpinAge += Time.deltaTime;

				//after 0.5 sec latter stop first reel
				if(SpinAge > 0.5f) {
					ActiveReel = 0;

					//before stop. you must set result !!!
					//if you get the result value from server,
					//change if(SpinAge > 0.5f) to if(ServerResponse) to reels keep rolling.
					ApplyResult();
					Reels[ActiveReel].Stop ();
				}
			}
			else {
				//if activeReel is stopped
				if(Reels[ActiveReel].Completed()) {
					if(ReelStopCallback != null) {
						ReelStopCallback.Invoke(ActiveReel);
					}

					//check next reel
					ActiveReel++;

					//if all Reels stopped.
					if(ActiveReel >= Reels.Count) {
						InSpin = false;

						//check result
						CheckWin();

						if(SpinEndCallback != null)
							SpinEndCallback.Invoke();
					}
					else {
						Reels[ActiveReel].Stop ();
					}
				}
			}
		}

		public void CreateChoice() {
			//BEChoice class is a utility class to choose a symbol from many symbols with various frequency.
			choice = new BEChoice[Reels.Count];
			for(int x=0 ; x < Reels.Count ; ++x) {
				choice[x] = new BEChoice();
				choice[x].Clear();

				for(int i=0 ; i < Symbols.Count ; ++i) {
					Symbol sd = GetSymbol(i);

					// if symbol's frequency of this reel is not 0
					// then add frequency to choice object
					if(sd.frequency[x] != 0)
						choice[x].Add (i, sd.frequency[x]);
				}
			}
		}

		public void CheckWin() {
			gameResult = CheckSpinWin(Deck, gameResult);

			Gold += gameResult.SpinWin;
			WinOffset = 0;
			WinShowAge = 0.0f;

			// Check Results
			for(int i=0 ; i < gameResult.Wins.Count ; ++i) {
				if(gameResult.Wins[i].Matches != Reels.Count) continue;

				SplashCount[(int)SplashType.FiveInRow] = 1;
				break;
			}

			if(gameResult.SpinWin > 0.001f) {
				SplashCount[(int)SplashType.Line] = 1;
			}

			// if freespin exist, show splash
			if(gameResult.NewFreeSpinCount > 0)
				SplashCount[(int)SplashType.FreeSpin] = 1;

			// if freespin is ends, show splash
			if((gameResult.FreeSpinTotalCount > 0) && (gameResult.FreeSpinCount > gameResult.FreeSpinTotalCount))
				SplashCount[(int)SplashType.FreeSpinEnd] = 1;

			//
			InResult = true;
			SplashActive = 0;
			GC.Collect();
		}

		public void MarkerCreate() {
			for(int i=0 ; i < Lines.Count ; ++i) {
				GameObject go = (GameObject)Instantiate(prefMarker, Vector3.zero, Quaternion.identity);
				go.name = "Marker "+i.ToString ("00");
				go.transform.SetParent(tr);
				go.transform.localScale = Vector3.one;
				Markers.Add (new Marker(i, GetLine(i), go));
			}

			List<Marker> sortList=new List<Marker>();
			for(int i=0 ; i < Lines.Count ; ++i)
				sortList.Add(Markers[i]);

			sortList.Sort ((x, y) => UnityEngine.Random.value < 0.5f ? -1 : 1);
			int Half = Lines.Count/2;
			int Quad = Lines.Count/4;
			for(int i=0 ; i < Lines.Count ; ++i) {
				if(i < Half) 	sortList[i].go.transform.localPosition = new Vector3(-515.0f+((i%2 == 1) ? 26 : 0), -Quad*22.0f+i*22.0f, 0);
				else  			sortList[i].go.transform.localPosition = new Vector3( 515.0f-((i%2 == 0) ? 26 : 0), -Quad*22.0f+(i-Half)*22.0f, 0);
			}
		}

		public void LineSet(int _Line) {
			Line = _Line;
			if(Line > Lines.Count) 	Line = LineMin;
			if(Line < LineMin) 		Line = Lines.Count;
			TotalBet = Convert.ToInt64(Line * RealBet);

			for(int i=0 ; i < Lines.Count ; ++i) {
				Markers[i].image.color = (i<Line) ? GetLine(i).color : new Color(0.5f,0.5f,0.5f,1.0f);
				Markers[i].text.color = (i<Line) ? Color.white : Color.gray;
			}
			//Debug.LogError("Total Bet1 : " + TotalBet);
		}

		public void BetSet(int _Bet) {
			Bet = _Bet;
			if(Bet >= BetTable.Count) Bet = 0;
			if(Bet < 0) Bet = BetTable.Count-1;
			RealBet = BetTable[Bet];
			TotalBet = Convert.ToInt64(Line * RealBet);
            //Debug.LogError("Total Bet : " + TotalBet);
        }

		public void SetRandomSymbolToReel() {
			for(int x=0 ; x < Reels.Count ; ++x)
				Reels[x].SetSymbolRandom();
		}

		public void MatchLineHide() {
			line.color = new Color(1,1,1,0);
			imgSelection.color = new Color(1,1,1,0);
		}

		public int GetResultSymbolCount(List<int> iDeck, SymbolType eSymbol) {
			int Count = 0;
			for(int x = 0 ; x < Reels.Count ; ++x) {
				for(int y = 0 ; y < RowCount ; ++y) {
					if(GetSymbol(iDeck[RowCount*x+y]).type == eSymbol)
						Count++;
				}
			}

			return Count;
		}

		public bool Spinable() {
			return ((Gold >= TotalBet) && !InSpin && !InResult) ? true : false;
		}

		public SlotReturnCode Spin() {
			GC.Collect();

			if(InSpin) return SlotReturnCode.InSpin;

			//if((gameResult.FreeSpinCount <= gameResult.FreeSpinTotalCount) && (Gold < TotalBet)) {
			//	return SlotReturnCode.NoGold;
			//}

			gameResult.Line = Line;
			gameResult.RealBet = BetTable[Bet];
			gameResult.Spin();

			if(gameResult.FreeSpinCount == 0) {
				gameResult.GameWin = 0.0f;
				Gold -= TotalBet;
			}

			Array.Clear(SplashCount, 0, SplashCount.Length);
			MatchLineHide();

			//Start Reels spin
			for(int x=0 ; x < Reels.Count ; ++x) {
				Reels[x].SymbolScaleReset();
				Reels[x].Spin();
			}

			SpinAge = 0.0f;
			ActiveReel = -1;
			InSpin = true;

			return SlotReturnCode.Success;
		}

		// make result value by frequency of each symbols
		public List<int> GetDeck() {
			List<int> result = new List<int>();

			//BEChoice class is a utility class to choose a symbol from many symbols with various frequency.
			for(int x=0 ; x < Reels.Count ; ++x) {
				for(int y=0 ; y < RowCount ; ++y) {
					//get random value from RNG
					int RandomValue = -1;
					if(rngType == RNGType.UnityRandom)  		RandomValue = UnityEngine.Random.Range (0, choice[x].ValueTotal);
					else if(rngType == RNGType.DotNetRandom)  	RandomValue = randomDotNet.Next(0, choice[x].ValueTotal);
					else if(rngType == RNGType.MersenneTwister) RandomValue = randomMersenneTwister.RandomRange(0, choice[x].ValueTotal-1);
					else {}

					//get selected symbol and add to result list
					int SymbolIdx = choice[x].Choice(RandomValue);
					result.Add (SymbolIdx);
				}
			}

			//Debug.Log ("GetDeck");
			//Debug.Log (GetSymbol(result[2]).prfab.name+","+GetSymbol(result[5]).prfab.name+","+GetSymbol(result[8]).prfab.name+","+GetSymbol(result[11]).prfab.name+","+GetSymbol(result[14]).prfab.name);
			//Debug.Log (GetSymbol(result[1]).prfab.name+","+GetSymbol(result[4]).prfab.name+","+GetSymbol(result[7]).prfab.name+","+GetSymbol(result[10]).prfab.name+","+GetSymbol(result[13]).prfab.name);
			//Debug.Log (GetSymbol(result[0]).prfab.name+","+GetSymbol(result[3]).prfab.name+","+GetSymbol(result[6]).prfab.name+","+GetSymbol(result[ 9]).prfab.name+","+GetSymbol(result[12]).prfab.name);

			return result;
		}

		public void ApplyResult() {
			//Create Result Symbols
			Deck = GetDeck();

			//Start Reels spin
			for(int x=0 ; x < Reels.Count ; ++x) {
				//make array of final value for each reel
				int [] value = new int[RowCount];
				for(int y=0 ; y < RowCount ; ++y)
					value[y] = Deck[RowCount*x+y];

				// set value to each reel
				Reels[x].ApplyResult(value);
			}

			// Bonus, Scatter Spin Effect
			{
				int	 iScatterSum = 0;
				int	 iBonusSum = 0;
				bool StartSet = false;
				for(int x=0 ; x < Reels.Count ; ++x) {
					// if there are scatter or Bonus Symbols more than 2
					// Reels Spin longer than normal before stop
					if(Math.Max (iScatterSum, iBonusSum) >= 2)  {
						Reels[x].MinimumRotateDistance = MinimumRotateDistancePredict;

						if(!StartSet) {
							Reels[x].bSlotIndicateFirst = true;
							StartSet = true;
							//Debug.Log ("Indicator on : "+x.ToString ());
						}
					}

					for(int y=0 ; y < RowCount ; ++y) {
						SymbolType eType = GetSymbol(Deck[RowCount*x+y]).type;
						if(eType == SymbolType.Scatter) iScatterSum++;
						if(eType == SymbolType.Bonus)   iBonusSum++;
					}
				}
			}
		}

		public GameResult CheckSpinWin(List<int> iDeck, GameResult result) {
			for(int x=0 ; x < Reels.Count ; ++x) {
				bool bWildFCExist = false;
				for(int y=0 ; y < RowCount ; ++y) {
					int SymbolIdx = iDeck[RowCount*x+y];
					if(GetSymbol(SymbolIdx).type == SymbolType.WildFC) {
						bWildFCExist = true;
						break;
					}
				}

				if(bWildFCExist) {
					int SymbolWildIdx = GetSymbolIdxByType(SymbolType.Wild);
					for(int y=0 ; y < RowCount ; ++y) {
						iDeck[RowCount*x+y] = SymbolWildIdx;
					}
				}
			}

			//resset spin of result
			result.ResetSpin();
			int [] iResult = new int [Reels.Count];

			for(int i=0 ; i < result.Line ; ++i) {
				Line ld = GetLine(i);

				for(int x=0 ; x < Reels.Count ; ++x) {
					iResult[x] = iDeck[RowCount*x+ld.Slots[x]];
				}

				// check
				int  MatchCount = 0;
				bool bFirstSymbol = false;
				int  SymbolIdx = -1;
				for(int x=0 ; x < Reels.Count ; ++x) {
					if(!bFirstSymbol) {
						if(!GetSymbol(iResult[x]).IsWild()) {
							SymbolIdx = iResult[x];
							bFirstSymbol = true;
						}
						MatchCount++;
					}
					else {
						if((SymbolIdx == iResult[x]) || GetSymbol(iResult[x]).IsWild()) {
							MatchCount++;
						}
						else {
							break;
						}
					}
				}

				if(SymbolIdx == -1) continue;

				Symbol sd = GetSymbol(SymbolIdx);
				if((sd.type == SymbolType.Normal) && (MatchCount != 0) && (sd.reward[MatchCount-1] != 0)) {
					float LineWin = (float)(sd.reward[MatchCount-1]) * result.RealBet;

					//Check whether Wild2X & Wild3X symbol is included
					int Wild2XSymbolCount = 0;
					int Wild3XSymbolCount = 0;
					for(int x=0 ; x < Reels.Count ; ++x) {
						if(GetSymbol(iResult[x]).type == SymbolType.Wild2X) Wild2XSymbolCount++;
						if(GetSymbol(iResult[x]).type == SymbolType.Wild3X) Wild3XSymbolCount++;
					}

					// Change Reward Gold by Wild Symbol Counts
					if(Wild3XSymbolCount > 0)		LineWin *= 3.0f;
					else if(Wild2XSymbolCount >= 2)	LineWin *= 4.0f;
					else if(Wild2XSymbolCount >= 1)	LineWin *= 2.0f;
					else {}

					//Debug.Log ("Win "+result.Wins.Count.ToString ()+" Line:"+i.ToString ()+" Symbol:"+SymbolIdx.ToString ()+" MatchCount:"+MatchCount.ToString ()+" Reward:"+LineWin.ToString ());
					result.Wins.Add (new WinItem(i, SymbolIdx, MatchCount, LineWin));
					result.SpinWin += LineWin;

					//
					if((result.TestSymbols.Count != 0) && (result.TestLines.Count != 0)) {
						result.TestSymbols[SymbolIdx].Hit++;
						result.TestSymbols[SymbolIdx].WinGold += LineWin;
						result.TestLines[i].Hit++;
						result.TestLines[i].WinGold += LineWin;
					}
				}
			}

			result.GameWin += result.SpinWin;

			if((result.FreeSpinTotalCount != 0) && (result.FreeSpinCount <= result.FreeSpinTotalCount)) {
				result.FreeSpinTotalWins += result.SpinWin;
				result.FreeSpinAccumWins += result.SpinWin;
			}

			{
				int ScaterSymbolCount = GetResultSymbolCount(iDeck, SymbolType.Scatter);
				if((3 <= ScaterSymbolCount) && (ScaterSymbolCount <= Reels.Count)){
					Symbol sd = GetSymbolByType(SymbolType.Scatter);
					result.NewFreeSpinCount = sd.reward[ScaterSymbolCount-1];
					result.FreeSpinAccumCount += result.NewFreeSpinCount;
				}

				result.FreeSpinTotalCount += result.NewFreeSpinCount;
				if(result.FreeSpinTotalCount > 0) {
					result.FreeSpinCount++;
				}
			}

			return result;
		}

		public void Simulation() {
			CreateChoice();

			resultTest = new GameResult();
			GameResult rt = resultTest;
			rt.ResetGame();
			rt.Line = Lines.Count;
			rt.RealBet = BetTable[0];

			//
			rt.TestSymbols.Clear();
			for(int i=0 ; i < Symbols.Count ; ++i) {
				rt.TestSymbols.Add (new TestItem(i));
			}
			rt.TestLines.Clear();
			for(int i=0 ; i < Lines.Count ; ++i) {
				rt.TestLines.Add (new TestItem(i));
			}

			for(int c=0 ; c < SimulationCount ; ++c) {
				rt.Spin();

				// get result
				List<int> iDeck = GetDeck();
				rt = CheckSpinWin(iDeck, rt);
				//Debug.Log ("result"+c.ToString ("00000")+" line:"+result.Wins.Count.ToString ()+" GameWin:"+result.GameWin);
			}

/*			float TotalBet = BetTable[0]*(float)SimulationCount;
			float Ratio = rt.GameWin/TotalBet * 100.0f;
			Debug.Log ("SlotGame::Simulation count:"+SimulationCount+" Bet:"+TotalBet+" Win:"+rt.GameWin+" Ratio:"+Ratio.ToString ("000.00"));
			Debug.Log ("FreeSpinAccumCount:"+rt.FreeSpinAccumCount+" FreeSpinAccumWins:"+rt.FreeSpinAccumWins);

			for(int i=0 ; i < Symbols.Count ; ++i) {
				Debug.Log ("Symbol:"+GetSymbol(i).prfab.name+" Hit:"+rt.TestSymbols[i].Hit+" Win:"+rt.TestSymbols[i].WinGold);
			}

			for(int i=0 ; i < Lines.Count ; ++i) {
				Debug.Log ("Line:"+i+" Hit:"+rt.TestLines[i].Hit+" Win:"+rt.TestLines[i].WinGold);
			}
*/		}

		public int GetSymbolIdxByType(SymbolType type) {
			for(int i=0 ; i < Symbols.Count ; ++i) {
				if(Symbols[i].type != type) continue;

				return i;
			}

			return -1;
		}

		public Symbol GetSymbolByType(SymbolType type) {
			int idx = GetSymbolIdxByType(type);
			return (idx == -1) ? null : Symbols[idx];
		}

		public Symbol GetSymbol(int idx) 	{
			return Symbols[idx];
		}

		public Line GetLine(int idx) 	{
			return Lines[idx];
		}
	}
}
