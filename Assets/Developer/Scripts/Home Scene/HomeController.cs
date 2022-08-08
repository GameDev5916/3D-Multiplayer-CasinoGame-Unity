using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeController : MonoBehaviour
{
    public static HomeController instance;

    public SpriteRenderer BG;

    public List<Sprite> allBGSprites = new List<Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //BG.sprite = allBGSprites[Random.Range(0, allBGSprites.Count)];
    }

    public void RollateButtonClick()
    {
        SceneManager.LoadScene(3);
    }
}
