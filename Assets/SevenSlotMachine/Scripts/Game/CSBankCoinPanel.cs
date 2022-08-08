using UnityEngine;
using UnityEngine.UI;

public class CSBankCoinPanel : MonoBehaviour {
    public System.Action<CSBankCoinPanel> bankValueChanged;

    public Text text;
    public GameObject particle;
    public RectTransform coinIcon;
    public bool formatText;

    private float _bank;
    public float bank {
        get { return _bank; }
        set {
            text.text = Format(value);
            _bank = value;
            ValueChanged();
        }
    }

    private void OnDestroy()
    {
        LeanTween.cancel(text.gameObject);
    }

    private void Awake()
    {
        _bank = CSGameSettings.instance.coins;
        text.text = Format(_bank);
    }

    private void OnEnable()
    {
        //CSIAPManager.instance.handleSuccessPurchase += HandleSuccessPurchase;
    }

    private void OnDisable()
    {
        //CSIAPManager.instance.handleSuccessPurchase -= HandleSuccessPurchase;
    }

    public string Format(float value)
    {
        return formatText ? CSUtilities.FormatNumber(value) : string.Format("{0:0.00}", value);
    }

    public void Add(float coins, bool animate = true)
    {
        if (animate)
        {
            AddWithAnimation(coins, 1f);
        }
        else
        {
            bank += coins;
        }
    }

    public void Add(float coins, RectTransform t)
    {
        AddWithAnimation(coins, AddParticle(t));
    }

    public void AddWithAnimation(float value, float duration)
    {
        CSSoundManager.instance.Play("slot_coins");
        LabelAction(text, _bank, _bank + value, duration);
        _bank += value;
        ValueChanged();
    }

    public float AddParticle(RectTransform t)
    {
        ParticleSystem p = Instantiate(particle, t.position, Quaternion.identity).GetComponent<ParticleSystem>();
        LeanTween.move(p.gameObject, coinIcon.position, p.main.duration * 0.9f);
        return p.main.duration;
    }

    public LTDescr LabelAction(Text label, float from, float to, float time)
    {
        return LeanTween.value(label.gameObject, from, to, time).setOnUpdate((float value) => {
            label.text = Format(value);
        });
    }

    private void ValueChanged()
    {
        CSGameSettings.instance.coins = _bank;
        if (bankValueChanged != null)
            bankValueChanged(this);
    }

    //private void HandleSuccessPurchase(UnityEngine.Purchasing.Product product, CSIAPProduct data)
    //{
    //    bank += data.coins;
    //}
}
