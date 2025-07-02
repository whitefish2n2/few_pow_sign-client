using System;
using System.Collections.Generic;
using Codes.OutGame.Match;
using NetCode;
using Plugins;
using UnityEngine;

public class MatchMakeStatic : MonoSingleton<MatchMakeStatic>
{
    public string userWebsocketKey;
    public string userDedicatedServerVerifyKey;
    public UInt16 dedicatedServerIndex;
    private List<NewPlayerDto> playerConstructor = new List<NewPlayerDto>();
    protected override void Start()
    {
        //이벤트 구독
        MatchingWsManager.Instance.OnMatchFound += OnMatchFound;
        MatchingWsManager.Instance.OnEnsureMatchEnqueue += OnMatchEnqueueEnsured;
    }

    private void OnMatchFound(MatchFoundDto matchFoundDto)
    {
        userDedicatedServerVerifyKey = matchFoundDto.sessionVerifyKey;
        Instance.dedicatedServerIndex = Convert.ToUInt16(matchFoundDto.sessionIndex);
    }

    private void OnMatchEnqueueEnsured(EnsureMatchEnqueueDto matchEnqueueDto)
    {
        userWebsocketKey = matchEnqueueDto.key;
    }

    protected override void Initialize() { }
}
