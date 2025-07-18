using System;
using System.Collections.Generic;
using Codes;
using Codes.OutGame.Match;
using MapFile.MapCode;
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
    public string gameId;
    public string userWebsocketKey;
    public string userDedicatedServerVerifyKey;
    public Map.MapEnum map;
    
    public string dedicatedBaseUrl;
    public UInt16 dedicatedServerIndex;
    public List<NewPlayerDto> playerConstructor = new List<NewPlayerDto>();

    /// <summary>
    /// NewPlayerDto->현재 자기 자신(플레이어)인지 확인
    /// </summary>
    /// <returns></returns>
    public bool isCurrentPlayer(NewPlayerDto dto)
    {
        return dto.Id == ClientStatic.Instance.authId;
    }
    public bool IsCurrentPlayerById(string id)
    {
        return id == ClientStatic.Instance.authId;
    }
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
        gameId = "";
        userWebsocketKey = "";
        userDedicatedServerVerifyKey = "";
        dedicatedServerIndex = 0;
        map = Map.MapEnum.Test;
        playerConstructor.Clear();
    }
    private void OnMatchFound(MatchFoundDto matchFoundDto)
    {
        gameId = matchFoundDto.gameId;
        userDedicatedServerVerifyKey = matchFoundDto.sessionVerifyKey;
        dedicatedServerIndex = Convert.ToUInt16(matchFoundDto.sessionIndex);
        playerConstructor = matchFoundDto.players;
        userDedicatedServerVerifyKey = matchFoundDto.sessionVerifyKey;
        map = matchFoundDto.map;
        
    }

    private void OnMatchEnqueueEnsured(EnsureMatchEnqueueDto matchEnqueueDto)
    {
        userWebsocketKey = matchEnqueueDto.key;
    }

    protected override void Initialize() { }
}
