using System;
using System.Collections.Generic;
using DynamicPrefab;
using NetTest;
using UnityEngine;

namespace Codes.InGame
{
    // MapInit(id↔name)과 씬의 SynchronizedObject를 이름 매칭으로 바인딩
    public class SyncObjectBinder : MonoBehaviour
    {
        private void Start()
        {
            var byName = new Dictionary<string, SynchronizedObject>();
            foreach (var so in FindObjectsByType<SynchronizedObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!byName.TryAdd(so.gameObject.name, so))
                    Debug.LogWarning($"[SyncBind] 이름 중복: {so.gameObject.name}");
            }

            foreach (var pair in InGameDataStatic.Instance.ObjectIdToNameMap)
            {
                if (byName.TryGetValue(pair.Value, out var so))
                {
                    so.objectId = (int)pair.Key;
                    InGameLogicStatic.Instance.RegisterSyncObject(pair.Key, so);
                }
                else
                {
                    Debug.LogWarning($"[SyncBind] 씬에 없는 서버 오브젝트: {pair.Value}({pair.Key})");
                }
            }
            Debug.Log($"[SyncBind] {InGameLogicStatic.Instance.syncObjects.Count}개 바인딩 완료");
            EnetClient.Instance.OnGenerateObjectReceived += OnGenerateObject;
        }

        private void OnDestroy()
        {
            if (EnetClient.IsInitialized)
                EnetClient.Instance.OnGenerateObjectReceived -= OnGenerateObject;
        }

        private void OnGenerateObject(uint targetId, byte prefabId, Vector3 pos)
        {
            var prefab = PrefabDispenser.Instance.GetPrefabById(prefabId);
            if (prefab == null) return;   // 매핑 없는 프리팹 스킵

            var obj = Instantiate(prefab, pos, Quaternion.identity);
            if (obj.TryGetComponent(out SynchronizedObject so))
            {
                so.objectId = (int)targetId;
                InGameLogicStatic.Instance.RegisterSyncObject(targetId, so);   // Mover면 자동 서버구동 전환
            }
        }
    }
}