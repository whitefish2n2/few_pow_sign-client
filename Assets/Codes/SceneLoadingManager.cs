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

    public async Task LoadSceneAsync(SceneEnum scene)
    {
        await SceneManager.LoadSceneAsync(scene.ToString());
    }
    
    public float currentProgress = 0f;
    public void LoadSceneWithLoadingScene(SceneEnum targetScene,SceneEnum loadingScene, Action onLoadStartCallback=null, List<AsyncOperation> works=null, Action onLoadEndCallback = null)
    {
        StartCoroutine(LoadSceneWithLoadSceneSequence(targetScene, loadingScene, works, onLoadStartCallback, onLoadEndCallback));
    }

    private IEnumerator LoadSceneWithLoadSceneSequence(SceneEnum targetScene,SceneEnum loadingScene,  List<AsyncOperation> works, Action onLoadStartCallback, Action onLoadEndCallback)
    {
        // 로딩 씬 로드
        var loadLoadingSceneOp = SceneManager.LoadSceneAsync(loadingScene.ToString(), LoadSceneMode.Additive);
        yield return loadLoadingSceneOp;
        // 현재 씬 언로드 대기
        var unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        

        // 실제 씬 로드 시작
        var loadSceneOp = SceneManager.LoadSceneAsync(targetScene.ToString(), LoadSceneMode.Additive);
        works?.Add(loadSceneOp);

        onLoadStartCallback?.Invoke();

        // 로딩 진행 대기 및 진행률 업데이트
        while (loadSceneOp is { isDone: false })
        {
            currentProgress = loadSceneOp.progress;
            yield return null;
        }

        if (works != null)
        {
            foreach (var work in works)
            {
                if(work!=null)
                    yield return work;
            }
        }

        currentScene = targetScene;

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
    LoadingPick,
    Pick,
    Game,
    Black,
}




