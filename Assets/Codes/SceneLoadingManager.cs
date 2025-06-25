using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Plugins;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoSingleton<SceneLoadingManager>
{
    public SceneEnum currentScene = SceneEnum.Sign;
    protected override void Initialize()
    {
        
    }

    public float currentProgress = 0f;
    public void LoadSceneWithLoadingSceneAsync(SceneEnum sceneEnum, Action onLoadStartCallback, List<AsyncOperation> works, Action onLoadEndCallback)
    {
        StartCoroutine(LoadSceneWithLoadSceneSequence(sceneEnum, works, onLoadStartCallback, onLoadEndCallback));
    }

    private IEnumerator LoadSceneWithLoadSceneSequence(SceneEnum sceneEnum, List<AsyncOperation> works, Action onLoadStartCallback, Action onLoadEndCallback)
    {
        // 로딩 씬 로드
        var loadLoadingSceneOp = SceneManager.LoadSceneAsync(SceneEnum.Loading.ToString(), LoadSceneMode.Additive);
        yield return loadLoadingSceneOp;
        // 현재 씬 언로드 대기
        var unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        

        // 실제 씬 로드 시작
        var loadSceneOp = SceneManager.LoadSceneAsync(sceneEnum.ToString(), LoadSceneMode.Additive);
        works.Add(loadSceneOp);

        onLoadStartCallback?.Invoke();

        // 로딩 진행 대기 및 진행률 업데이트
        while (loadSceneOp is { isDone: false })
        {
            currentProgress = loadSceneOp.progress;
            yield return null;
        }

        currentScene = sceneEnum;

        //  로딩 완료 콜백
        onLoadEndCallback?.Invoke();

        // 로딩 씬 언로드 
        var unloadLoadingOp = SceneManager.UnloadSceneAsync(SceneEnum.Loading.ToString());
        yield return unloadLoadingOp;
    }
    
}

public enum SceneEnum
{
    Loading,
    Sign,
    Main,
    Game
}




