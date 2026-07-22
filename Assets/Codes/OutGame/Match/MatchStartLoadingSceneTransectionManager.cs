using System;
using System.Collections;
using Codes.OutGame.Match;
using Codes.OutGame.PickCharacter;
using NetCode;
using UnityEngine;

/// <summary>
/// CharacterSceneManager의 StartGame을 구독해서 특정 초 후에 씬 넘기는 작업을 수행하는 모노비헤이비어
/// </summary>
public class MatchStartLoadingSceneTransectionManager : MonoBehaviour
{
    [SerializeField] private float loadTerm;
    private void Start()
    {
        CharacterPickSceneManager.Instance.StartGame  += Transection;
    }

    private void Transection(StartGameDto _)
    {
        StartCoroutine(TransectionIE());
    }

    IEnumerator TransectionIE()
    {
        yield return new WaitForSeconds(loadTerm);
        SceneLoadingManager.Instance.LoadSceneAsync(SceneEnum.LoadingMultiGame, () => 
        {
            Debug.Log("멀티플레이 로딩 씬 진입 완료!");
        });
    }
}