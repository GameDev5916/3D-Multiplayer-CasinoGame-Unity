using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BE {
	//class for random selection
	public class BEChoice {
		private	List<int> 	IDs = new List<int>();
		private	List<int> 	Values = new List<int>();
		public	int			ValueTotal;

		public void Clear() {
			IDs.Clear();
			Values.Clear();
			ValueTotal = 0;
		}

		public void Add(int id, int value) {
			if(value == 0) return;

			IDs.Add (id);
			Values.Add (value);
			ValueTotal += value;
		}

		public int Choice(int choice) {
			//Debug.Log ("Choice value:"+choice.ToString ());

			if((choice < 0) || (ValueTotal <= choice)) return -1;

			for(int i=0 ; i < Values.Count ; ++i) {
				if(choice < Values[i]) 	return IDs[i];
				else  					choice -= Values[i];
			}

			return -1;
		}
	}

	///-----------------------------------------------------------------------------------------
	///   Namespace:      BE
	///   Class:          BENumber
	///   Description:    this is utility calss to manupulate numbers
	///                   when value changes, display values change to target value gradually
	///                   and create string from values by several types.
	///   Usage :
	///   Author:         BraveElephant inc.
	///   Version: 		  v1.0 (2015-08-30)
	///-----------------------------------------------------------------------------------------
	public class BENumber {
		public enum IncType {
			NONE 			= -1,
			VALUE 			= 0,
			VALUEwithMAX 	= 1,
			TIME 			= 2,
		};

		private bool  	bInChange 	= false;
		private float 	fAge 		= 0.0f;
		private float 	fInc 		= 1.0f;
		private double 	fTarget 	= 0.0;

		private double 	fMin 		= 0.0;
		private double 	fMax 		= 1.0;
		private double 	fCurrent 	= 0.0;

		private IncType eType		= IncType.VALUEwithMAX;
		private string  strFormat	= "#,##0";
		private GameObject 		m_EventTarget = null;
		private string 			m_EventFunction;
		private GameObject 		m_EventParameter;

		private Text			UIText = null;

		public BENumber(IncType type, string _strFormat, double min, double max, double current) {
			Init(type, _strFormat, min, max, current);
		}

		public void Init(IncType type, string _strFormat, double min, double max, double current) {
			eType = type;
			strFormat = _strFormat;
			fMin = min;
			fMax = max;
			fCurrent = current;
			fTarget = current;
			bInChange = false;
		}

		public void 	AddUIText(Text ui)		{ UIText = ui; if(UIText != null) UIText.text = ToString(); }

		public void 	TypeSet(IncType type)	{ eType = type; }
		public IncType 	Type()					{ return eType; }

		public bool 	InChange()				{ return bInChange; }
		public float 	Ratio()					{ return (float)((fCurrent-fMin)/(fMax-fMin)); }
		public float 	TargetRatio()			{ return (float)((fTarget-fMin)/(fMax-fMin)); }
		public double 	Current()				{ return fCurrent; }
		public double 	Min()					{ return fMin; }
		public double 	Max()					{ return fMax; }
		public void 	MaxSet(double value)	{ fMax = value; }
		public double 	Target()				{ return fTarget; }
		public override string 	ToString()	 {
			if(eType == IncType.VALUE) 				return fCurrent.ToString (strFormat);
			else if(eType == IncType.VALUEwithMAX) 	return fCurrent.ToString (strFormat)+" / "+fMax.ToString (strFormat);
			else if(eType == IncType.TIME) {
				int iCurrent = (int)fCurrent;
				int Day  = iCurrent/86400;	if(Day > 0)  iCurrent -= Day *86400;
				int Hour = iCurrent/3600;	if(Hour > 0) iCurrent -= Hour*3600;
				int Min  = iCurrent/60;		if(Min > 0)  iCurrent -= Min*60;
				int Sec  = iCurrent;

				if(Day > 0) 		return Day.ToString()+ "D "+Hour.ToString ()+"H";
				else if(Hour > 0) 	return Hour.ToString()+"H "+Min.ToString ()+"M";
				else if(Min > 0) 	return Min.ToString()+ "M "+Sec.ToString ()+"S";
				else 				return Sec.ToString()+"S";
			}
			else {
				return "";
			}
		}

		public void ChangeTo(double target) {
			if(target < fMin) target = fMin;
			if(target > fMax) target = fMax;
			if(!bInChange) {
				bInChange = true;
				fAge = 0.0f;
				fInc = 1.0f;
			}
			fTarget = target;
		}

		public void ChangeDelta(double target) {
			ChangeTo(fTarget+target);
		}

		public void Update() {
			if(!bInChange) return;

			fAge += Time.deltaTime * 6.0f;
			fInc += Mathf.Exp(fAge);

			if(fTarget > fCurrent) 	{ fCurrent += (double)fInc; if(fCurrent >= fTarget) End(); }
			else  					{ fCurrent -= (double)fInc; if(fCurrent <= fTarget) End(); }

			if(UIText != null)
				UIText.text = ToString();
		}

		private void End() {
			bInChange = false;
			fCurrent = fTarget;

			if(m_EventTarget != null)
				m_EventTarget.SendMessage(m_EventFunction, m_EventParameter);
		}

		public void SetReceiver(GameObject target, string functionName, GameObject parameter) {
			m_EventTarget = target;
			m_EventFunction = functionName;
			m_EventParameter = parameter;
		}
	}
}
/*
public void IncInit()
{
	m_IncLevel 		= new UIInc(0,300,0);							m_IncLevel.TypeSet 		(UIInc.IncType.VALUE);
	m_IncExp 		= new UIInc(0,NetAdmin.Instance.ExpMax,0);		m_IncExp.TypeSet 		(UIInc.IncType.VALUE);//VALUEwithMAX);
	m_IncTro 		= new UIInc(0,100000000,0);						m_IncTro.TypeSet 		(UIInc.IncType.VALUE);
	m_IncAdrenaline = new UIInc(0,100000000,0);						m_IncAdrenaline.TypeSet (UIInc.IncType.TIME);
	m_IncShield 	= new UIInc(0,100000000,0);						m_IncShield.TypeSet 	(UIInc.IncType.TIME);
	m_IncEnegyDrink = new UIInc(0,100000000,0);						m_IncEnegyDrink.TypeSet (UIInc.IncType.TIME);
	m_IncGold 		= new UIInc(0,1000000000000,0);					m_IncGold.TypeSet 		(UIInc.IncType.VALUE);
	m_IncTori 		= new UIInc(0,100000000,0);						m_IncTori.TypeSet 		(UIInc.IncType.VALUE);
	m_IncHp 		= new UIInc(0,NetAdmin.Instance.MaxHealth,0);	m_IncHp.TypeSet 		(UIInc.IncType.VALUEwithMAX);

	for(int i=0 ; i < 8 ; ++i)
		StartCoroutine(Inc(0.2f*(float)i,i));
}
public bool IncChangeCheck(UIInc inc, float deltaTime)
{
	double value = inc.Current();
	inc.Update(deltaTime);
	return (value != inc.Current()) ? true : false;
}
public void IncUpdate(bool bForce)
{
	if(m_IncLevel.Target () 	!= NetAdmin.Instance.Level) 		{ m_IncLevel.ChangeTo (NetAdmin.Instance.Level); }
	if(m_IncExp.Target() 		!= NetAdmin.Instance.ExpCurrent)    { m_IncExp.MaxSet(NetAdmin.Instance.ExpMax); m_IncExp.ChangeTo(NetAdmin.Instance.ExpCurrent); }
	if(m_IncTro.Target() 		!= NetAdmin.Instance.LeatherScore) 	{ m_IncTro.ChangeTo(NetAdmin.Instance.LeatherScore); }
	if(m_IncAdrenaline.Target() != NetAdmin.Instance.AdrenalineSec) { m_IncAdrenaline.ChangeTo(NetAdmin.Instance.AdrenalineSec); }
	if(m_IncShield.Target() 	!= NetAdmin.Instance.ShieldSec) 	{ m_IncShield.ChangeTo(NetAdmin.Instance.ShieldSec); }
	if(m_IncEnegyDrink.Target() != NetAdmin.Instance.EnegyDrinkSec) { m_IncEnegyDrink.ChangeTo(NetAdmin.Instance.EnegyDrinkSec); }
	if(m_IncGold.Target() 		!= NetAdmin.Instance.Gold) 			{ m_IncGold.ChangeTo(NetAdmin.Instance.Gold); }
	if(m_IncTori.Target() 		!= NetAdmin.Instance.Tori) 			{ m_IncTori.ChangeTo(NetAdmin.Instance.Tori); }
	if(m_IncHp.Target() 		!= NetAdmin.Instance.Health) 		{ m_IncHp.ChangeTo(NetAdmin.Instance.Health); }

	float deltaTime = Admin.Instance.deltaTime;

	if(bForce || IncChangeCheck(m_IncLevel, deltaTime))				{ labelLevel.text = "Lv"+m_IncLevel.ToString (); }
	if(bForce || IncChangeCheck(m_IncExp, deltaTime))				{ labelExp.text = (NetAdmin.Instance.Level == 200) ? LocalGet("MaxLevel") : m_IncExp.ToString (); sliderExp.sliderValue = m_IncExp.Ratio(); }
	if(bForce || IncChangeCheck(m_IncTro, deltaTime))				{ labelTro.text = m_IncTro.ToString (); }
	if(bForce || IncChangeCheck(m_IncAdrenaline, deltaTime))		{ labelAdrenaline.text = (m_IncAdrenaline.Target () < 0.1f) ? LocalGet("None") : m_IncAdrenaline.ToString (); }
	if(bForce || IncChangeCheck(m_IncShield, deltaTime))			{ labelShield.text = (m_IncShield.Target () < 0.1f) ? LocalGet("None") : m_IncShield.ToString (); }
	if(bForce || IncChangeCheck(m_IncEnegyDrink, deltaTime))		{ labelEnergy.text = (m_IncEnegyDrink.Target () < 0.1f) ? LocalGet("None") : m_IncEnegyDrink.ToString (); }
	if(bForce || IncChangeCheck(m_IncGold, deltaTime))				{ labelGold.text = m_IncGold.ToString (); labelShopGold.text = m_IncGold.ToString (); labelShopSubGold.text = m_IncGold.ToString (); }
	if(bForce || IncChangeCheck(m_IncTori, deltaTime))				{ labelTori.text = m_IncTori.ToString (); labelShopTori.text = m_IncTori.ToString (); labelShopSubTori.text = m_IncTori.ToString (); }
	if(bForce || IncChangeCheck(m_IncHp, deltaTime))		 		{ labelHp.text = m_IncHp.ToString (); labelHpVillage.text = m_IncHp.ToString (); sliderHp.sliderValue = m_IncHp.Ratio(); sliderHpVillage.sliderValue=m_IncHp.Ratio(); }

	labelTitle.text = NetAdmin.Instance.Title;
	SetActive(goButtonHeal, (NetAdmin.Instance.MaxHealth - NetAdmin.Instance.Health < 0.001f) ? false : true);

	MyUtil.Instance.BadgeSet(goPanelCharacterInfoItem0, NetAdmin.Instance.PerfactDefenseCount);
	MyUtil.Instance.BadgeSet(goPanelCharacterInfoItem1, NetAdmin.Instance.DoubleDamageCount);
	MyUtil.Instance.BadgeSet(goPanelCharacterInfoItem2, NetAdmin.Instance.AngelCount);
}
public IEnumerator Inc(float fDelay, int type)
{
	if(fDelay > 0.0001f) yield return new WaitForSeconds(fDelay);

	if(type == 0)  		m_IncLevel.ChangeTo(NetAdmin.Instance.Level);
	else if(type == 1)  m_IncExp.ChangeTo(NetAdmin.Instance.ExpCurrent);
	else if(type == 2)  m_IncTro.ChangeTo(NetAdmin.Instance.LeatherScore);
	else if(type == 3)  m_IncAdrenaline.ChangeTo(NetAdmin.Instance.AdrenalineSec);
	else if(type == 4)  m_IncShield.ChangeTo(NetAdmin.Instance.ShieldSec);
	else if(type == 5)  m_IncEnegyDrink.ChangeTo(NetAdmin.Instance.EnegyDrinkSec);
	else if(type == 6)  m_IncGold.ChangeTo(NetAdmin.Instance.Gold);
	else if(type == 7)  m_IncTori.ChangeTo(NetAdmin.Instance.Tori);
	else {}
}
*/