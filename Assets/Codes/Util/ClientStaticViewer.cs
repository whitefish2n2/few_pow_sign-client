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
        var data = ClientStatic.Instance;
        GUILayout.BeginVertical("box");
        GUILayout.Label($"authId: {data.authId}");
        GUILayout.Label($"authPassword: {data.authPassword}");
        GUILayout.Label($"authName: {data.authName}");
        GUILayout.Label($"current Name:{data.username}");
        GUILayout.Label($"refresh token: {data.refreshToken}");
        GUILayout.Label($"jwt:{data.jwt}");
        GUILayout.EndVertical();
    }
}
#endif
