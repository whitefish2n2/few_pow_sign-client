using System;
using Codes.InGame.Player_Ingame;
using Codes.InGame.Weapons;
using Codes.Util;
using DynamicPrefab;
using UnityEngine;

namespace Codes.InGame
{
    public class PlayerGenerateManager : ServerComponent
    {
        private void Start()
        {
            var data = InGameDataStatic.Instance.PlayerSpawnInfo;
            foreach (var item in data)
            {
                // characterId → prefabId → prefab
                var charData = CharacterInfoLoader.Instance.GetById(item.charId);
                if (charData == null)
                {
                    Debug.LogError($"[PlayerGenerate] CharacterData 없음: charId={item.charId}");
                    continue;
                }
                var prefab = PrefabDispenser.Instance.GetPrefabById(charData.prefabId);
                if (prefab == null)
                {
                    Debug.LogError($"[PlayerGenerate] 프리팹 없음: prefabId={charData.prefabId} ({item.charId})");
                    continue;
                }

                var obj = Instantiate(prefab, item.spawnPos, Quaternion.identity);
                var pc = obj.GetComponent<PlayerComponent>();
                pc.publicKey = item.publicKey;
                pc.maxHp = charData.baseStats.maxHp;
                pc.currentHp = pc.maxHp;
                
                PlayerBehaviour beh;
                if (item.publicKey == InGameDataStatic.Instance.myPublicKey)
                {
                    var hp  = obj.AddComponent<HandledPlayerBehavior>();
                    var ms  = obj.AddComponent<MoveSystem>();
                    var hpw = obj.AddComponent<HandledPlayerWeaponSystem>();
                    hp.Init();
                    ms.Init();
                    hpw.Init();
                    beh = hp;
                }
                else
                {
                    var up = obj.AddComponent<UnHandledPlayerBehavior>();
                    var ws = obj.AddComponent<WeaponSystem>();
                    up.Init();
                    ws.Init();
                    beh = up;
                }

                InGameLogicStatic.Instance.RegisterPlayer(item.publicKey, beh);
            }
        }


        public override string Serialize()
        {
            return "";
        }
    
    
        //MonoBungleton
        private static PlayerGenerateManager _instance;
        private static bool _initialized;
        private static readonly object _lock = new object();
        public static PlayerGenerateManager Instance
        {
            get
            {
                if (!_instance)
                    Debug.LogError($"[MonoSingleton<{nameof(PlayerGenerateManager)}>] is not initialized!");
                return _instance;
            }
        }
        public static bool IsInitialized => _initialized;

        public static bool TryGetInstance(out PlayerGenerateManager instance)
        {
            instance = _instance;
            return _initialized;
        }

        protected void Awake()
        {
            lock (_lock)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(gameObject);
                    return;
                }

                _instance = this;
                _initialized = true;
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _initialized = false;
            }
        }
    
    }
}

