using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public enum SoundEnums
    {
        ButtonClick,
        GameWin,
        ChipCollect,
        OneSpinComplete,
        Spin,
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource BGSound;
    [SerializeField] private AudioSource OtherSounds;

    [Header("Audio Clips For Home And Spins")]
    [Space]
    [SerializeField] private AudioClip ButtonClip;
    [SerializeField] private AudioClip GameWinClip;
    [SerializeField] private AudioClip ChipCollectClip;
    [SerializeField] private AudioClip OneSpinCompleteClip;
    [SerializeField] private AudioClip SpinClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        BgSound();
    }

    public void PlaySound(SoundEnums soundtoplay)
    {
        if(Constants.SOUND == 0)
        {
            switch (soundtoplay)
            {
                case SoundEnums.ButtonClick:
                    OtherSounds.PlayOneShot(ButtonClip);
                    //Debug.Log("ButtonClick Sound");
                    break;
                case SoundEnums.GameWin:
                    OtherSounds.PlayOneShot(GameWinClip);
                    //Debug.Log("ButtonClick Sound");
                    break;
                case SoundEnums.ChipCollect:
                    OtherSounds.PlayOneShot(ChipCollectClip);
                    //Debug.Log("ButtonClick Sound");
                    break;
                case SoundEnums.OneSpinComplete:
                    OtherSounds.PlayOneShot(OneSpinCompleteClip);
                    //Debug.Log("ButtonClick Sound");
                    break;
                case SoundEnums.Spin:
                    OtherSounds.PlayOneShot(SpinClip);
                    //Debug.Log("ButtonClick Sound");
                    break;

                default:
                    break;
            }
        }
    }

    public void BgSound()
    {
        if (Constants.MUSIC == 1)
            BGSound.Stop();
        else
            BGSound.Play();
    }
}
