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

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          BEUtil
///   Description:    Utility class Features are below
///   Usage :
///   Author:         BraveElephant inc.
///   Version: 		  v1.1 (2016-02-29)
///-----------------------------------------------------------------------------------------
namespace BE {
	public class BEUtil : MonoBehaviour {
		public	static BEUtil instance;

		void Awake() {
			instance=this;
		}

		public IEnumerator ImageScale(Image img, Color color, float ScaleStart, float ScaleMax, float ScaleEnd, float ScaleTime, float fDelay) {
			Transform tr = img.transform;
			if(fDelay > 0.01f)
				yield return new WaitForSeconds(fDelay);

			bool  End = false;
			float fAge = 0.0f;
			float fUnit = (ScaleMax-ScaleEnd+ScaleMax-ScaleStart)/ScaleTime;
			float fScale = ScaleStart;
			tr.localScale = new Vector3(fScale,fScale,fScale);
			img.color = color;

			while(true) {
				float fScaleNow = (fScale > ScaleMax) ? ScaleMax-(fScale-ScaleMax) : fScale;
				tr.localScale = new Vector3(fScaleNow, fScaleNow, fScaleNow);
				if(End) {
					break;
				}

				yield return new WaitForSeconds(0.03f);

				fAge += 0.03f;
				fScale += fUnit * 0.03f;
				if(fAge > ScaleTime) {
					End = true;
					fScale = ScaleEnd;
				}
			}
		}

		public IEnumerator ObjectScale(Transform tr, float ScaleStart, float ScaleMax, float ScaleEnd, float ScaleTime, float fDelay) {
			if(fDelay > 0.01f)
				yield return new WaitForSeconds(fDelay);

			bool  End = false;
			float fAge = 0.0f;
			float fUnit = (ScaleMax-ScaleEnd+ScaleMax-ScaleStart)/ScaleTime;
			float fScale = ScaleStart;
			tr.localScale = new Vector3(fScale,fScale,fScale);

			while(true) {
				float fScaleNow = (fScale > ScaleMax) ? ScaleMax-(fScale-ScaleMax) : fScale;
				tr.localScale = new Vector3(fScaleNow, fScaleNow, fScaleNow);
				if(End) {
					break;
				}

				yield return new WaitForSeconds(0.03f);

				fAge += 0.03f;
				fScale += fUnit * 0.03f;
				if(fAge > ScaleTime) {
					End = true;
					fScale = ScaleEnd;
				}
			}
		}

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

		public string DateStringChange(string Regdate) {
			DateTime dtNow = DateTime.Now;
			DateTime dtPost = StringToDateTimeG(Regdate);
			//MyUtil.Instance.Log ("dtPost:"+dtPost.ToString ());
			TimeSpan timeDelta = dtNow.Subtract(dtPost);

			string strRankResetLeft = "";
			if(timeDelta.Days > 0)			strRankResetLeft = timeDelta.Days.ToString ()    + "day"+" "    +timeDelta.Hours.ToString ()+"hoursbefore";
			else if(timeDelta.Hours > 0)	strRankResetLeft = timeDelta.Hours.ToString ()   + "hour"+" "   +timeDelta.Minutes.ToString ()+"minutebefore";
			else if(timeDelta.Minutes > 0)	strRankResetLeft = timeDelta.Minutes.ToString () + "minute"+" " +timeDelta.Seconds.ToString ()+"secondbefore";
			else 							strRankResetLeft = "justbefore";

			return strRankResetLeft;
		}

		public DateTime StringToDateTimeG(string strDate) {
			//Debug.Log ("StringToDateTimeG input:"+strDate);
			string[] 	Sub0 	= strDate.Split(' ');
			string[] 	Sub1 	= Sub0[0].Split('-');
			string[] 	Sub2 	= Sub0[2].Split(':');
			int year 	= int.Parse (Sub1[0]);	//Debug.Log ("0 year:"+year.ToString ());
			int month 	= int.Parse (Sub1[1]);	//Debug.Log ("1 month:"+month.ToString ());
			int day 	= int.Parse (Sub1[2]);	//Debug.Log ("2 day:"+day.ToString ());
			int hour 	= int.Parse (Sub2[0]);	//Debug.Log ("3 hour:"+hour.ToString ());
			int minute 	= int.Parse (Sub2[1]);	//Debug.Log ("4 minute:"+minute.ToString ());
			int second 	= int.Parse (Sub2[2]);	//Debug.Log ("5 second:"+second.ToString ());
			if (hour >= 12) hour -= 12;
			if(string.Equals(Sub0[1],"오후")) hour+=12;
			if(hour >= 24)  hour-=24;

			DateTime dt = new DateTime(year,month,day,hour,minute,second);
			//Debug.Log ("StringToDateTimeG result:"+dt.ToString());
			return dt;
		}

		public DateTime StringToDateTime(string strYYYYMMDD) {
			string sYear 	= strYYYYMMDD.Substring(0,4);
			string sMonth 	= strYYYYMMDD.Substring(4,2);
			string sDay 	= strYYYYMMDD.Substring(6,2);
			DateTime dtStart = Convert.ToDateTime(sDay+"/"+sMonth+"/"+sYear);
			Debug.Log ("StringToDateTime "+strYYYYMMDD+"->"+dtStart.ToString ());

			return dtStart;
		}

		//
		public static string pathForDocumentsFile( string filename )
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer) {
				string path = Application.dataPath.Substring( 0, Application.dataPath.Length - 5 );
				path = path.Substring( 0, path.LastIndexOf( '/' ) );
				return Path.Combine( Path.Combine( path, "Documents" ), filename );
			}
			else if(Application.platform == RuntimePlatform.Android) {
				string path = Application.persistentDataPath;
				path = path.Substring(0, path.LastIndexOf( '/' ) );
				return Path.Combine (path, filename);
			}
			else  {
				string path = Application.dataPath;
				path = path.Substring(0, path.LastIndexOf( '/' ) );
				return Path.Combine (path, filename);
			}
		}

		//
		//
		public static string Encrypt (string toEncrypt)
		{
			byte[] keyArray = UTF8Encoding.UTF8.GetBytes ("12345678901234567890123456789012");
			// 256-AES key
			byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes (toEncrypt);
			RijndaelManaged rDel = new RijndaelManaged ();
			rDel.Key = keyArray;
			rDel.Mode = CipherMode.ECB;
			// http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
			rDel.Padding = PaddingMode.PKCS7;
			// better lang support
			ICryptoTransform cTransform = rDel.CreateEncryptor ();
			byte[] resultArray = cTransform.TransformFinalBlock (toEncryptArray, 0, toEncryptArray.Length);
			return Convert.ToBase64String (resultArray, 0, resultArray.Length);
		}

		public static string Decrypt (string toDecrypt)
		{
			byte[] keyArray = UTF8Encoding.UTF8.GetBytes ("12345678901234567890123456789012");
			// AES-256 key
			byte[] toEncryptArray = Convert.FromBase64String (toDecrypt);
			RijndaelManaged rDel = new RijndaelManaged ();
			rDel.Key = keyArray;
			rDel.Mode = CipherMode.ECB;
			// http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
			rDel.Padding = PaddingMode.PKCS7;
			// better lang support
			ICryptoTransform cTransform = rDel.CreateDecryptor ();
			byte[] resultArray = cTransform.TransformFinalBlock (toEncryptArray, 0, toEncryptArray.Length);
			return UTF8Encoding.UTF8.GetString (resultArray);
		}

		public static string SecToString(int second)	 {
			int iCurrent = second;
			int Day  = iCurrent/86400;	if(Day > 0)  iCurrent -= Day *86400;
			int Hour = iCurrent/3600;	if(Hour > 0) iCurrent -= Hour*3600;
			int Min  = iCurrent/60;		if(Min > 0)  iCurrent -= Min*60;
			int Sec  = iCurrent;

			string strReturn = "";
			if(Day > 0) 		strReturn = Day.ToString()+ "D "+Hour.ToString ()+"H";
			else if(Hour > 0) 	strReturn = Hour.ToString()+"H "+Min.ToString ()+"M";
			else if(Min > 0) 	strReturn = Min.ToString()+ "M "+Sec.ToString ()+"S";
			else 				strReturn = Sec.ToString ()+"S";

			return strReturn;
		}


		/*
		public 	float		deltaTime = 0.01f;
		public 	float		PausedTime = 0.0f;
		private DateTime	pausedTime;
		public	bool		bJustChangeFront = false;

		public void OnApplicationPause(bool paused) {
			if(paused) {
				pausedTime = DateTime.Now;
			}
			else {
				//if(bInitialized) {
					DateTime dtNow = DateTime.Now;
					TimeSpan timeDelta = dtNow.Subtract(pausedTime);
					PausedTime = (float)timeDelta.TotalSeconds;
				//}
			}

			//Start
			PausedTime = 0.0f;

			//Update
			deltaTime = Time.deltaTime;
			if(PausedTime > 0.01f) {
				deltaTime += PausedTime;
				PausedTime = 0.0f;
			}

			DateTime dtNow = DateTime.Now;
			DateTime dtPost = MyUtil.Instance.StringToDateTimeG(newRegdate);
			TimeSpan timeDelta = dtNow.Subtract(dtPost);
			if(timeDelta.TotalDays > NetWeb.Instance.GameTelCycle) {
				GetUserTelList();
			}

			if(DateTime.Now.Subtract(Admin.Instance.TrainingCompleteLastTime).Days == 0) return;
		}
*/
	}
}