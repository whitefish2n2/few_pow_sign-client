using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoSingleton<SceneLoadingManager>
{
    protected override void Initialize()
    {
        
    }

    public float currentProgress;
    public async Task LoadSceneWithLoadingSceneAsync(SceneEnum sceneEnum, Action onLoadStartCallback,List<AsyncOperation> works,Action onLoadEndCallback)
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(nameof(SceneEnum.Loading), LoadSceneMode.Additive);
        var coroutine = StartCoroutine(LoadScene(sceneEnum,works,onLoadEndCallback,onLoadStartCallback));//todo:코루틴 await 못하냐
    }

    IEnumerator LoadScene(SceneEnum sceneEnum,List<AsyncOperation> works,Action onLoadStartCallback=null,Action onLoadEndCallback=null)
    {
        var o = SceneManager.LoadSceneAsync(sceneEnum.ToString(), LoadSceneMode.Additive);
        works.Add(o);
        onLoadStartCallback?.Invoke();
        foreach (var t in works)
        {
            while (!t.isDone)
            {
                currentProgress = t.progress;
                yield return null;
            }
        }onLoadEndCallback?.Invoke();
        yield break;
    }
    
}

public enum SceneEnum
{
    Loading,
    Sign,
    Main,
    Game
}
