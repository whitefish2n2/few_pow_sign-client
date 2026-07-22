using System.Collections.Generic;
using Codes.Util;
using Plugins;
using UnityEngine;

namespace DynamicPrefab
{
    public class PrefabDispenser : MonoSingleton<PrefabDispenser>
    {
        [SerializeField] private IdToPrefabMap idToPrefabMap;
        private Dictionary<int, GameObject> prefabDic = new();

        protected override void Awake()
        {
            base.Awake();
        
            if (idToPrefabMap == null)
            {
                Debug.LogError("[PrefabDispenser] IdToPrefabMap이 인스펙터에 할당되지 않았습니다!");
                return;
            };
        
            foreach (var item in idToPrefabMap.mappings)
            {
                if (!prefabDic.ContainsKey(item.id))
                {
                    prefabDic.Add(item.id, item.prefab);
                }
            }
        }

        protected override void Initialize() { }

        public GameObject GetPrefabById(int id)
        {
            if (prefabDic.TryGetValue(id, out GameObject prefab))
            {
                return prefab;
            }
        
            Debug.LogWarning($"[PrefabDispenser] 프리팹을 찾을 수 없습니다. 요청된 ID: {id}");
            return null;
        }
    }
}