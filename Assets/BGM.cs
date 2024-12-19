using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectsOfType<BGM>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
    }
}
