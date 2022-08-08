using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BE {
	public class UISGReel : MonoBehaviour {
		private Transform	tr;
		public  SlotGame 	game;
		private bool		InSpin = false;
		private bool		InStop = false;

		public	int 			ID;
		public	Vector3 [] 		SymbolPos;
		public 	GameObject []	Symbols;
		public 	GameObject 		SymbolFC=null;
		private bool 			bWildFCExist = false;

		[HideInInspector]
		public 	int []			FinalValues;

		private float 		Speed;
		public 	float 		MinimumRotateDistance;
		public 	bool  		bSlotIndicateFirst;
		public  int			StopOffset = -2;
		private bool		InDamping = false;
		private int			SpinCount = 0;


		public void Init(SlotGame script, int x) {
			tr = transform;
			game = script;//tr.parent.GetComponent<SlotGame>();
			ID = x;

			Symbols = new GameObject[game.RowCount+1];
			SymbolPos = new Vector3[game.RowCount+1];
			FinalValues = new int[game.RowCount];
			for(int y=0 ; y < game.RowCount+1 ; ++y) {
				GameObject go = BEObjectPool.Spawn(game.Symbols[0].prfab);
				go.transform.SetParent(tr);
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = Vector3.one;
				go.name = "Symbol"+y.ToString ();
				Symbols[y] = go;
			}
		}

		void Awake () {
			tr = transform;
		}

		void Start () {
		}

		void Update () {
			if(!InSpin) return;

			float dt = Mathf.Min(Time.deltaTime, 0.1f);
			Vector3 vPos = tr.localPosition;

			if(InStop && (MinimumRotateDistance <= 0.0f)) {
				float fDelta = Speed * dt;

				if(StopOffset < game.RowCount) {
					vPos.y -= fDelta;

					if(vPos.y < -game.SymbolSizePlusMargin) {
						vPos.y += game.SymbolSizePlusMargin;

						if(SymbolFC != null) {
							Vector3 vPosFC = SymbolFC.transform.localPosition;
							vPosFC.y -= game.SymbolSizePlusMargin;
							SymbolFC.transform.localPosition = vPosFC;
						}

						if(StopOffset >= -1) {
							//if(ID == 0)
							//	Debug.Log ("StopOffset:"+StopOffset+" FinalValues.Length:"+FinalValues.Length);
							SymbolShiftDown((StopOffset < game.RowCount-1) ? FinalValues[StopOffset+1] : GetRandom());
						}
						else {
							//if(ID == 0)
							//	Debug.Log ("StopOffset:"+StopOffset);
							SymbolShiftDown(GetRandom());
						}

						StopOffset++;
					}
				}
				else {
					if(!InDamping) {
						vPos.y -= fDelta;
						if(vPos.y < -game.DampingHeight) {
							InDamping = true;
						}
					}
					else
					{
						if((int)vPos.y != 0) {
							vPos.y = Mathf.Lerp(vPos.y, 0.0f, game.BoundSpeed * Time.deltaTime);
						}
						else  {
							vPos.y = 0.0f;
							InStop = false;
							InSpin = false;
						}
					}
				}
			}
			else
			{
				//increase Reel spin speed
				Speed += game.Acceleration * dt;
				if(Speed >= game.SpeedMax)
					Speed = game.SpeedMax;

				float fDelta = Speed * dt;
				vPos.y -= fDelta;

				if(InStop)
					MinimumRotateDistance -= fDelta;

				// if Symbol id lower than SymbolSizePlusMargin, move up
				if(vPos.y < -game.SymbolSizePlusMargin) {
					vPos.y += game.SymbolSizePlusMargin;

					if(SymbolFC != null) {
						Vector3 vPosFC = SymbolFC.transform.localPosition;
						vPosFC.y -= game.SymbolSizePlusMargin;
						SymbolFC.transform.localPosition = vPosFC;
					}

					SymbolShiftDown(GetRandom());
				}
			}

			tr.localPosition = vPos;
		}

		public int GetRandom() {
			return UnityEngine.Random.Range(0,game.Symbols.Count);
		}

		public void SetSymbolRandom() {
			for(int y=0 ; y < game.RowCount+1 ; ++y) {
				SetSymbol(y, GetRandom());
			}
		}

		public void SetSymbol(int y, int Value) {
			if(Symbols[y] != null) {
				BEObjectPool.Unspawn(Symbols[y]);
			}

			//Debug.Log ("SetSymbol y:"+y+" Value:"+Value);
			Symbol newSymbol = game.GetSymbol(Value);
			//Debug.Log ("newSymbol "+newSymbol);
			if(newSymbol.type == SymbolType.WildFC)
				newSymbol = game.GetSymbolByType(SymbolType.Wild);

			GameObject go = BEObjectPool.Spawn(newSymbol.prfab);
			//Debug.Log ("go "+go);
			go.transform.SetParent(tr);
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = SymbolPos[y];
			Symbols[y] = go;
		}

		public void SymbolScaleReset() {
			for(int y=0 ; y < game.RowCount+1 ; ++y) {
				Symbols[y].transform.localScale = Vector3.one;
			}
		}

		public void SymbolShiftDown(int addedValue) {
			SpinCount++;

			if(bWildFCExist && (SymbolFC == null) && (StopOffset == -1)) {
				Symbol symbolFC = game.GetSymbolByType(SymbolType.WildFC);
				if(symbolFC != null) {
					float yOffset = game.SymbolSizePlusMargin * (float)(game.RowCount - 1) * 0.5f;

					GameObject go = BEObjectPool.Spawn(symbolFC.prfab);
					go.transform.SetParent(tr);
					go.transform.localScale = Vector3.one;
					go.transform.localPosition = SymbolPos[3] + new Vector3(0,yOffset,0);
					SymbolFC = go;
					//Debug.Log ("SymbolFC Created Pos:"+SymbolPos[3]);
				}
				else {
					Debug.Log ("No Wild-FullColumn found!!!");
				}
			}

			if((SymbolFC != null) && (SpinCount == game.RowCount)) {
				BEObjectPool.Unspawn(SymbolFC);
				SymbolFC = null;
			}

			for(int y=0 ; y < game.RowCount+1 ; ++y) {
				if(y == 0) {
					BEObjectPool.Unspawn(Symbols[0]);
					//Destroy(Symbols[0]);
				}

				if(y == game.RowCount) {
					Symbols[y] = null;
					SetSymbol(y, addedValue);
				}
				else {
					Symbols[y] = Symbols[y+1];
				}

				Symbols[y].transform.localPosition = SymbolPos[y];
				Symbols[y].name = "Symbol"+y.ToString ();
			}

			if(SymbolFC != null) {
				SymbolFC.transform.SetAsLastSibling();
			}
		}

		public void Spin() {
			Speed = 0.0f;
			MinimumRotateDistance = game.MinimumRotateDistance;
			bSlotIndicateFirst = false;
			StopOffset = -2;
			SpinCount = 0;
			bWildFCExist = false;

			Vector3 vPos = tr.localPosition;
			vPos.y = 0;
			tr.localPosition = vPos;

			SetSymbol(game.RowCount, GetRandom());

			InSpin = true;
			InStop = false;
			InDamping = false;
		}

		public void ApplyResult(int [] values) {
			FinalValues = values;

			// Check if Wild Full Column is exist
			bWildFCExist = false;
			for(int i=0 ; i < game.RowCount ; ++i) {
				if(game.GetSymbol(FinalValues[i]).type == SymbolType.WildFC) {
					bWildFCExist = true;
					break;
				}
			}

			if(bWildFCExist) {
				for(int i=0 ; i < game.RowCount ; ++i) {
					if(game.GetSymbol(FinalValues[i]).type == SymbolType.WildFC) {
						FinalValues[i] = game.GetSymbolIdxByType(SymbolType.Wild);
					}
				}
			}
		}

		public void Stop() {
			InStop = true;
		}

		public bool Completed() {
			return (!InStop && !InSpin) ? true : false;
		}
	}
}
