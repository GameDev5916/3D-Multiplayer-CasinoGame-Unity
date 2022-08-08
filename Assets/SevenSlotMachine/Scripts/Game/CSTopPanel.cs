using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CSTopPanel : MonoBehaviour {
    public CSGameStore store;
    public CSExperiencePanel xpPanel;
    public CSBankCoinPanel coinPanel;

    public void OnBuyCoins()
    {
        store.Appear();
    }

    public void AddXPValue(float value)
    {
        xpPanel.AddValue(value);
    }

    public void AddCoins(float coins)
    {
        coinPanel.Add(coins);
    }

    public virtual void OnLobby(string sceneName)
    {
        CSSoundManager.instance.Stop("reel_spin");
    }
}
