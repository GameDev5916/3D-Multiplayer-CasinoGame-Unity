using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotDestoryOnload : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
