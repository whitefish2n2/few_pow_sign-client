using System;
using Codes;
using UnityEngine;

#if UNITY_EDITOR
public class ClientStaticViewer : MonoBehaviour
{
    private static ClientStaticViewer instance;
    private void Start()
    {
        if (instance) Destroy(gameObject);
        else instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void OnGUI()
    {

    }
}
#endif
