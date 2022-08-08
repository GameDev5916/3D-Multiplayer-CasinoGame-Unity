using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class LoadSceenFromMenu : MonoBehaviour
{
#if UNITY_EDITOR

    [MenuItem("Sceen/Splash")]
    static void LoadSplashSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/Splash.unity");
    }

    [MenuItem ("Sceen/Login")]
    static void LoadLoginSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/Login.unity");
    }

    [MenuItem("Sceen/Home")]
    static void LoadHomeSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/Home.unity");
    }

    [MenuItem("Sceen/Free Spin")]
    static void LoadFreeSpinSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/FreeSpin.unity");
    }

    [MenuItem("Sceen/Bonus Spin")]
    static void LoadBonusSpinSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/Bonus Spins.unity");
    }

    [MenuItem("Sceen/SevenSlots")]
    static void LoadSlotSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/SevenSlots.unity");
    }

    [MenuItem("Sceen/20 Solt New")]
    static void Load20SlotSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/20 Solt New.unity");
    }

    [MenuItem("Sceen/Poker")]
    static void LoadPokerSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/Poker.unity");
    }

    [MenuItem("Sceen/BlackJack")]
    static void LoadBlackJackSceen()
    {
        EditorSceneManager.OpenScene("Assets/Developer/Scenes/BlackJack.unity");
    }
#endif
}
