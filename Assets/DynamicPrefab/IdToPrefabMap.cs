using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codes.Util
{
    [Serializable]
    public struct PrefabMapping
    {
        public int id;
        public GameObject prefab;
    }

    [CreateAssetMenu(fileName = "IdToPrefab", menuName = "ScriptableObjects/IdToPrefab Map")]
    public class IdToPrefabMap : ScriptableObject
    {
        // 유니티 인스펙터 직렬화를 위해 List 구조체 사용
        public List<PrefabMapping> mappings = new List<PrefabMapping>();
    }
}