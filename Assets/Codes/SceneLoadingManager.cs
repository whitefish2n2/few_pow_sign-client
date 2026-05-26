using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Plugins;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoSingleton<SceneLoadingManager>
{

    public event Action<float> OnProgressUpdated;
    public SceneEnum currentScene = SceneEnum.LoginUi;
    protected override void Initialize()
    {
        
    }

    public void LoadSceneAsync(SceneEnum scene,Action onLoadEndCallback)
    {
        StartCoroutine(LoadSceneIE(scene, onLoadEndCallback));
    }

    private IEnumerator LoadSceneIE(SceneEnum target, Action onLoadEndCallback)
    {
        var loadSceneOp = SceneManager.LoadSceneAsync(target.ToString(), LoadSceneMode.Single);
        while (loadSceneOp is { isDone: false })
        {
            currentProgress = loadSceneOp.progress;
            OnProgressUpdated?.Invoke(currentProgress);
            yield return null;
        }
        
        onLoadEndCallback?.Invoke();

    }
    
    public float currentProgress = 0f;
    public void LoadSceneWithLoadingScene(SceneEnum targetScene,SceneEnum loadingScene, Action onLoadStartCallback=null, List<AsyncOperation> works=null, Action onLoadEndCallback = null)
    {
        StartCoroutine(LoadSceneWithLoadSceneSequence(targetScene, loadingScene, works, onLoadStartCallback, onLoadEndCallback));
    }

    private IEnumerator LoadSceneWithLoadSceneSequence(SceneEnum targetScene,SceneEnum loadingScene,  List<AsyncOperation> works, Action onLoadStartCallback, Action onLoadEndCallback)
    {
        // 로딩 씬 로드
        var loadLoadingSceneOp = SceneManager.LoadSceneAsync(loadingScene.ToString(), LoadSceneMode.Single);
        yield return loadLoadingSceneOp;
        

        // 실제 씬 로드 시작
        var loadSceneOp = SceneManager.LoadSceneAsync(targetScene.ToString(), LoadSceneMode.Single);

        onLoadStartCallback?.Invoke();

        // 로딩 진행 대기 및 진행률 업데이트, 진행률 업데이트 이벤트 호출
        while (loadSceneOp is { isDone: false })
        {
            currentProgress = loadSceneOp.progress;
            OnProgressUpdated?.Invoke(currentProgress);
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
    }
    
}
public enum SceneEnum
{
    Loading,
    LoginUi,
    OutgameSkeleton,
    LoadingPickScene,
    PickSkeleton,
    LoadingMultiGame,
    TestScene,
    Black,//not exist
}




