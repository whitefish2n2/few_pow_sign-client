#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


//[InitializeOnLoad]
public class AutoSave
{
    static AutoSave()
    {
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Save(state);
            }
        };
        lastSaveTime = DateTime.Now;
        EditorApplication.update += Update;
        Debug.Log("AutoSave Utility Initialized");
    }
    static DateTime lastSaveTime;
    static readonly double interval = 300;
    static void Update()
    {
        if (!EditorApplication.isPlaying && (DateTime.Now - lastSaveTime).TotalSeconds > interval)
        {
            Save(PlayModeStateChange.ExitingPlayMode);
            lastSaveTime = DateTime.Now;
        }
    }

    static void Save(PlayModeStateChange state)
    {
        if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        Debug.Log("Auto Saving All Open Scenes " + state);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }

}
#endif