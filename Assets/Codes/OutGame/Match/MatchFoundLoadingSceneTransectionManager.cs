using System;
using System.Collections;
using Codes.OutGame.Match;
using UnityEngine;

/// <summary>
/// OutGameMatchController의 OnMatchFoundAction을 구독해서 특정 초 후에 씬 넘기는 작업을 수행하는 모노비헤이비어
/// </summary>
public class MatchFoundLoadingSceneTransectionManager : MonoBehaviour
{
    [SerializeField] private float loadTerm;
    private void Start()
    {
        OutGameMatchController.Instance.OnMatchFoundAction += Transection;
    }

    private void Transection()
    {
        StartCoroutine(TransectionIE());
    }

    IEnumerator TransectionIE()
    {
        yield return new WaitForSeconds(loadTerm);
        SceneLoadingManager.Instance.LoadSceneWithLoadingScene(SceneEnum.PickSkeleton, SceneEnum.LoadingPickScene);
    }
}
