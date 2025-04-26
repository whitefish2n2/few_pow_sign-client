using System;
using Codes.Util.Annotation;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
public class NetTestStatic : MonoBehaviour
{
    public static NetTestStatic instance;
    public string baseUrl;
    
    public string authId;
    public string authPassword;
    public string authName;
    
    public int serverPort;
    [ReadOnly] public string jwt;
    [ReadOnly] public string refreshToken;
    [ReadOnly] public string clientId;
    [ReadOnly] public string clientSecret;
    [ReadOnly] public string username;

    public string dedicatedBaseUrl;
    public string dedicatedPort;

    private void Awake()
    {
        instance = this;
    }
}
#endif