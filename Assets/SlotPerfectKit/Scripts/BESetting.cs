using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          BESetting
///   Description:    store variables used in game
///                   save & load values from PlayerPrefs
///   Usage :		  BESetting.Save();
/// 	              BESetting.Load();
///                   BESetting.MusicVolume = 100;
///   Author:         BraveElephant inc.
///   Version: 		  v1.0 (2015-11-15)
///-----------------------------------------------------------------------------------------
namespace BE {
	public class BESetting : MonoBehaviour {
		public	static BESetting instance;

		public	static int		MusicVolume = 100;		// music volume (zero is disable)
		public	static int		SoundVolume = 100;		// sound volume (zero is disable)
		public  static bool		bFirstRun = false;		// id game is first run after installed
		public  static int		StageRunCount = 0;		// how many stage was played after game begin
		public  static int		ShowInstruction = 100;	// instruction dialog show ? (zero is not show)
		public  static BENumber	Gold;					// total gold number

		void Awake() {
			instance=this;
			Gold = new BENumber(BENumber.IncType.VALUE, "#,##0.00", 0, 10000000000, 2000); // default user gold id 2000
			//PlayerPrefs.DeleteAll();
			Load();
		}

		void Update() {
			Gold.Update();
		}

		void OnDestroy() {
			Save();
		}

		public static void Save() {
			//Debug.Log ("BESetting::Save Gold:"+Gold);
			PlayerPrefs.SetInt("MusicVolume", MusicVolume);
			PlayerPrefs.SetInt("SoundVolume", SoundVolume);
			PlayerPrefs.SetInt("StageRunCount", StageRunCount);
			PlayerPrefs.SetInt("ShowInstruction", ShowInstruction);
			PlayerPrefs.SetFloat("Credit", (float)Gold.Target());
		}

		public static void Load() {
			if(PlayerPrefs.HasKey("MusicVolume")) {
				MusicVolume 	= PlayerPrefs.GetInt("MusicVolume");
				SoundVolume 	= PlayerPrefs.GetInt("SoundVolume");
				StageRunCount 	= PlayerPrefs.GetInt("StageRunCount");
				ShowInstruction = PlayerPrefs.GetInt("ShowInstruction");
				float fGold 	= PlayerPrefs.GetFloat("Credit");
				Gold.ChangeTo(fGold);
				//Debug.Log ("BESetting::Load Gold:"+Gold);
			}
			else {
				Save();
				bFirstRun = true;
			}
		}

		// mask ui inside of screen
		public bool MakeUIInsideScreen(Transform tr) {
			Vector3[] objectCorners = new Vector3[4];
			tr.gameObject.GetComponent<RectTransform>().GetWorldCorners(objectCorners);

			bool IsOurSide = false;
			Vector3 vOffset = Vector3.zero;
			foreach (Vector3 corner in objectCorners) {
				if((corner.x < 0.0f) && (vOffset.x < -corner.x)) {
					vOffset.x = -corner.x;
					IsOurSide = true;
				}

				if((corner.x > Screen.width) && (vOffset.x > Screen.width-corner.x)) {
					vOffset.x = Screen.width-corner.x;
					IsOurSide = true;
				}

				if((corner.y < 0.0f) && (vOffset.y < -corner.y)) {
					vOffset.y = -corner.y;
					IsOurSide = true;
				}

				if((corner.y > Screen.height) && (vOffset.y > Screen.height-corner.y)) {
					vOffset.y = Screen.height-corner.y;
					IsOurSide = true;
				}

				if(IsOurSide) {
					Vector3 pos = tr.position;
					tr.position = pos + vOffset;
					return true;
				}
			}

			return false;
		}
	}
}