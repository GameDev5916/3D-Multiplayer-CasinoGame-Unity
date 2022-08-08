using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using BE;

[CustomEditor(typeof(SlotGame))]
public class SlotGameEditor : Editor {
	private static int tab=0;
	private static SlotGame instance;
	private GUIContent cont;

	GUIStyle styleHelpboxInner;

/*	public enum Rows {
		_1 = 0,
		_2 = 1,
		_3 = 2
	}
*/
	void Awake(){
		instance = (SlotGame)target;
		EditorUtility.SetDirty(instance);

		styleHelpboxInner = new GUIStyle("HelpBox");
		styleHelpboxInner.padding = new RectOffset(4, 4, 4, 4);
	}

	public override void OnInspectorGUI(){
		instance = (SlotGame)target;
		GUI.changed = false;
		int iTemp = 0;

		EditorGUILayout.Space();

		tab = GUILayout.Toolbar (tab, new string[] {"Symbol", "Payline", "Bet", "Basic", "Math", "Default"});
		if(tab == 0) {
			EditorGUILayout.Space();

			for(int i=0; i<instance.Symbols.Count; ++i){
				GUILayout.BeginVertical(styleHelpboxInner);

				GUILayout.BeginHorizontal();
				GUILayout.Label((instance.Symbols[i].prfab != null) ? instance.Symbols[i].prfab.name : "None");
				//if(GUILayout.Button("-", GUILayout.MaxWidth(20)))	i-=instance.SymbolRemove(i);
				GUILayout.EndHorizontal();

				instance.Symbols[i].SetPrefab((GameObject)EditorGUILayout.ObjectField("Prefab", instance.Symbols[i].prfab, typeof(GameObject), false));

				instance.Symbols[i].type=(SymbolType)EditorGUILayout.EnumPopup("Type", (SymbolType)instance.Symbols[i].type);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Frequency ");
				for(int j=0 ; j < instance.Reels.Count ; ++j) {
					if((j != 0) && (!instance.Symbols[i].frequencyPerReel)) {
						instance.Symbols[i].frequency[j] = instance.Symbols[i].frequency[0];
					}
					else {
						instance.Symbols[i].frequency[j]=EditorGUILayout.IntField(instance.Symbols[i].frequency[j], GUILayout.MaxWidth(30));
					}
				}
				GUILayout.EndHorizontal();

				instance.Symbols[i].frequencyPerReel=EditorGUILayout.ToggleLeft("Enable Frequency Per Reel", instance.Symbols[i].frequencyPerReel);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Reward ");
				for(int j=0 ; j < instance.Reels.Count ; ++j) {
					instance.Symbols[i].reward[j]=EditorGUILayout.IntField(instance.Symbols[i].reward[j], GUILayout.MaxWidth(30));
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("+", GUILayout.MaxWidth(30)))	instance.SymbolAdd();
			if(GUILayout.Button("-", GUILayout.MaxWidth(30)))	instance.SymbolRemove();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
		}
		else if(tab == 1)  {
			EditorGUILayout.Space();

			string [] displayedOptions;
			int[] optionValues;

			if(instance.RowCount == 3) {
				displayedOptions = new string [] {"1","2","3"};
				optionValues = new int[] {0,1,2};
			}
			else if(instance.RowCount == 4) {
				displayedOptions = new string [] {"1","2","3","4"};
				optionValues = new int[] {0,1,2,3};
			}
			else if(instance.RowCount == 5) {
				displayedOptions = new string [] {"1","2","3","4","5"};
				optionValues = new int[] {0,1,2,3,4};
			}
			else if(instance.RowCount == 6) {
				displayedOptions = new string [] {"1","2","3","4","5","6"};
				optionValues = new int[] {0,1,2,3,4,5};
			}
			else {
				displayedOptions = new string [] {"1","2","3","4","5","6","7"};
				optionValues = new int[] {0,1,2,3,4,5,6};
			}

			for(int i=0; i<instance.Lines.Count; ++i){
				GUILayout.BeginHorizontal();

				GUILayout.Label("Line "+(i+1).ToString ("00"));

				for(int j=0 ; j < instance.Reels.Count ; ++j) {
					//instance.Lines[i].Slots[j]=(int)(Rows)EditorGUILayout.EnumPopup((Rows)instance.Lines[i].Slots[j],GUILayout.Width(30),GUILayout.MaxWidth(30));
					instance.Lines[i].Slots[j]=EditorGUILayout.IntPopup(instance.Lines[i].Slots[j],displayedOptions,optionValues,GUILayout.Width(30),GUILayout.MaxWidth(30));
				}

				instance.Lines[i].color=EditorGUILayout.ColorField(instance.Lines[i].color);

				if(GUILayout.Button("+", GUILayout.MaxWidth(20))) instance.LineInsert(i);
				if(GUILayout.Button("-", GUILayout.MaxWidth(20))) i-=instance.LineRemove(i);

				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("+", GUILayout.MaxWidth(30))) instance.LineAdd();
			if(GUILayout.Button("-", GUILayout.MaxWidth(30))) instance.LineRemove();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
		}
		else if(tab == 2) {
			EditorGUILayout.Space();

			for(int i=0; i<instance.BetTable.Count; ++i){
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bet "+(i+1).ToString ("00"));
				instance.BetTable[i]=EditorGUILayout.FloatField(instance.BetTable[i]);
				if(GUILayout.Button("+", GUILayout.MaxWidth(20))) instance.BetInsert(i);
				if(GUILayout.Button("-", GUILayout.MaxWidth(20))) i-=instance.BetRemove(i);
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("+", GUILayout.MaxWidth(30))) instance.BetAdd();
			if(GUILayout.Button("-", GUILayout.MaxWidth(30))) instance.BetRemove();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
		}
		else if(tab == 3) {
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Reel", EditorStyles.boldLabel);
			GUILayout.BeginVertical(styleHelpboxInner);

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reel Count", EditorStyles.boldLabel, GUILayout.Width(120));
			if(GUILayout.Button("-", GUILayout.MaxWidth(20))) instance.SetReelCount(instance.Reels.Count-1);
			GUILayout.Label(instance.Reels.Count.ToString (), GUILayout.MaxWidth(100));
			if(GUILayout.Button("+", GUILayout.MaxWidth(20))) instance.SetReelCount(instance.Reels.Count+1);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Row Count", EditorStyles.boldLabel, GUILayout.Width(120));
			if(GUILayout.Button("-", GUILayout.MaxWidth(20))) instance.SetRowCount(instance.RowCount-1);
			GUILayout.Label(instance.RowCount.ToString (), GUILayout.MaxWidth(100));
			if(GUILayout.Button("+", GUILayout.MaxWidth(20))) instance.SetRowCount(instance.RowCount+1);
			GUILayout.EndHorizontal();

			iTemp = EditorGUILayout.IntSlider("Symbol Size", instance.SymbolSize, 0, 256);
			if(iTemp != instance.SymbolSize) instance.SetSymbolSize(iTemp);
			iTemp = EditorGUILayout.IntSlider("Margin Size", instance.MarginSize, -200, 256);
			if(iTemp != instance.MarginSize) instance.SetMarginSize(iTemp);

			GUILayout.EndVertical();

			EditorGUILayout.LabelField("Spin", EditorStyles.boldLabel);
			GUILayout.BeginVertical(styleHelpboxInner);

			iTemp = EditorGUILayout.IntSlider("BoundSpeed", (int)instance.BoundSpeed, 10, 100);
			if(iTemp != (int)instance.BoundSpeed) instance.BoundSpeed = (float)iTemp;
			iTemp = EditorGUILayout.IntSlider("Acceleration", (int)instance.Acceleration, 100, 5000);
			if(iTemp != (int)instance.Acceleration) instance.Acceleration = (float)iTemp;
			iTemp = EditorGUILayout.IntSlider("SpeedMax", (int)instance.SpeedMax, 100, 5000);
			if(iTemp != (int)instance.SpeedMax) instance.SpeedMax = (float)iTemp;
			iTemp = EditorGUILayout.IntSlider("Minimum Rotate", (int)instance.MinimumRotateDistance, 100, 5000);
			if(iTemp != (int)instance.MinimumRotateDistance) instance.MinimumRotateDistance = (float)iTemp;
			iTemp = EditorGUILayout.IntSlider("Minimum Rotate Predict", (int)instance.MinimumRotateDistancePredict, 100, 5000);
			if(iTemp != (int)instance.MinimumRotateDistancePredict) instance.MinimumRotateDistancePredict = (float)iTemp;

			GUILayout.EndVertical();

			EditorGUILayout.LabelField("Others", EditorStyles.boldLabel);
			GUILayout.BeginVertical(styleHelpboxInner);

			instance.rngType=(RNGType)EditorGUILayout.EnumPopup("Random Function", (RNGType)instance.rngType);
			iTemp = EditorGUILayout.IntSlider("Minimum Line", instance.LineMin, 1, 50);
			if(iTemp != instance.LineMin) instance.LineMin = iTemp;

			GUILayout.EndVertical();
		}
		else if(tab == 4) {
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Simulation", EditorStyles.boldLabel);
			GUILayout.BeginVertical(styleHelpboxInner);

			iTemp = EditorGUILayout.IntSlider("Count", (int)instance.SimulationCount, 10, 10000);
			if(iTemp != instance.SimulationCount) {
				instance.SimulationCount = iTemp;
				instance.resultTest = null;
			}
			if(GUILayout.Button("Start", GUILayout.MaxWidth(120)))  {
				instance.Simulation();
				EditorUtility.SetDirty(instance);
			}

			//iTemp = EditorGUILayout.IntSlider("Acceleration", (int)instance.Acceleration, 100, 5000);
			//if(iTemp != (int)instance.Acceleration) instance.Acceleration = (float)iTemp;
			//iTemp = EditorGUILayout.IntSlider("SpeedMax", (int)instance.SpeedMax, 100, 5000);
			//if(iTemp != (int)instance.SpeedMax) instance.SpeedMax = (float)iTemp;
			//iTemp = EditorGUILayout.IntSlider("Minimum Rotate", (int)instance.MinimumRotateDistance, 100, 5000);
			//if(iTemp != (int)instance.MinimumRotateDistance) instance.MinimumRotateDistance = (float)iTemp;
			//iTemp = EditorGUILayout.IntSlider("Minimum Rotate Predict", (int)instance.MinimumRotateDistancePredict, 100, 5000);
			//if(iTemp != (int)instance.MinimumRotateDistancePredict) instance.MinimumRotateDistancePredict = (float)iTemp;

			GUILayout.EndVertical();

			if(instance.resultTest != null) {
				GameResult rt = instance.resultTest;
				float TotalBet = instance.BetTable[0]*(float)instance.Lines.Count*(float)(instance.SimulationCount - rt.FreeSpinAccumCount);
				float Ratio = rt.GameWin/TotalBet * 100.0f;

				EditorGUILayout.LabelField("Return Rate", EditorStyles.boldLabel);
				GUILayout.BeginVertical(styleHelpboxInner);
				{
					//GUILayout.BeginHorizontal();
					GUILayout.Label("SimulationCount : "+instance.SimulationCount.ToString ("#,##0"));
					GUILayout.Label("TotalBets : "+TotalBet.ToString ("#,##0.00"));
					GUILayout.Label("TotalWins : "+rt.GameWin.ToString ("#,##0.00"));
					GUILayout.Label("Return Rate : "+Ratio.ToString ("#,##0.00"));
					//GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				EditorGUILayout.LabelField("Symbol", EditorStyles.boldLabel);
				GUILayout.BeginVertical(styleHelpboxInner);
				for(int i=0 ; i < instance.Symbols.Count+1 ; ++i) {
					if((i > 0) && (instance.Symbols[i-1].type != SymbolType.Normal)) continue;

					GUILayout.BeginHorizontal();
					if(i == 0) {
						GUILayout.Label("Name");
						GUILayout.Label("Hit");
						GUILayout.Label("Wins");
					}
					else {
						GUILayout.Label(instance.Symbols[i-1].prfab.name);
						GUILayout.Label(rt.TestSymbols[i-1].Hit.ToString ());
						GUILayout.Label(rt.TestSymbols[i-1].WinGold.ToString ("#,##0.00"));
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				EditorGUILayout.LabelField("Line", EditorStyles.boldLabel);
				GUILayout.BeginVertical(styleHelpboxInner);
				for(int i=0 ; i < instance.Lines.Count+1 ; ++i) {
					GUILayout.BeginHorizontal();
					if(i == 0) {
						GUILayout.Label("Name");
						GUILayout.Label("Hit");
						GUILayout.Label("Wins");
					}
					else {
						GUILayout.Label(i.ToString ());
						GUILayout.Label(rt.TestLines[i-1].Hit.ToString ());
						GUILayout.Label(rt.TestLines[i-1].WinGold.ToString ("#,##0.00"));
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				EditorGUILayout.LabelField("FreeSpin", EditorStyles.boldLabel);
				GUILayout.BeginVertical(styleHelpboxInner);
				{
					//GUILayout.BeginHorizontal();
					GUILayout.Label("Count : "+rt.FreeSpinAccumCount.ToString ());
					GUILayout.Label("Wins : "+rt.FreeSpinAccumWins.ToString ("#,##0.00"));
					//GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
		}
		else if(tab == 5) {
			DrawDefaultInspector ();
		}
		else {}

		EditorGUILayout.Space();

		if(GUI.changed) {
			EditorUtility.SetDirty(instance);
		}
	}
}