using System;
using System.Collections.Generic;
using Codes.OutGame.Match;
using NetCode;
using Plugins;
using UnityEngine;

/// <summary>
/// outgame Scene(메인 씬)에 있는 Match Found되었을때 값 저장
/// 매치에 필요한 값 저장(플레이어 기본 정보(초상화, 닉네임, id, 선택 캐릭터 등))
/// 플레이어같은 경우엔 여기서의 값을 기반으로 생성
/// </summary>
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

    /// <summary>
    /// 매칭 화면에 들어올 때 OutGameMatchController가 한번씩 실행
    /// </summary>
    public void PrepareToNewMatch()
    {
        userWebsocketKey = "";
        userDedicatedServerVerifyKey = "";
        dedicatedServerIndex = 0;
        playerConstructor.Clear();
    }
    private void OnMatchFound(MatchFoundDto matchFoundDto)
    {
        userDedicatedServerVerifyKey = matchFoundDto.sessionVerifyKey;
        dedicatedServerIndex = Convert.ToUInt16(matchFoundDto.sessionIndex);
        playerConstructor = matchFoundDto.players;
        userDedicatedServerVerifyKey = matchFoundDto.sessionVerifyKey;
    }

    private void OnMatchEnqueueEnsured(EnsureMatchEnqueueDto matchEnqueueDto)
    {
        userWebsocketKey = matchEnqueueDto.key;
    }

    protected override void Initialize() { }
}
