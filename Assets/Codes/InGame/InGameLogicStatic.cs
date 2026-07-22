using System;
using System.Collections.Generic;
using Codes.InGame.Weapons;
using Codes.Util;
using NetCode.ENetCode;
using NetTest;
using Plugins;
using UnityEngine;

namespace Codes.InGame
{
    public class InGameLogicStatic : MonoBungleton<InGameLogicStatic>
    {
        public Dictionary<byte, PlayerBehaviour> players = new();
        public Dictionary<uint, SynchronizedObject> syncObjects = new();
        public Dictionary<uint, Mover> ingameMovers = new();

        [SerializeField] public GameObject hitImpactBodyPrefab;
        [SerializeField] public GameObject hitImpactHeadPrefab;

        
        private void Start()
        {
            if (EnetClient.Instance != null)
            {
                EnetClient.Instance.OnPlayerMoveReceived += ApplyPlayerMove;
                EnetClient.Instance.OnObjectMoveReceived += ApplyObjectMove;
                EnetClient.Instance.OnDeathReceived += ApplyDeath;
                EnetClient.Instance.OnRespawnReceived += ApplyRespawn;
                EnetClient.Instance.OnShotFired += ApplyShotFired;
                EnetClient.Instance.OnHit += ApplyHit;
                EnetClient.Instance.OnGameEnded += ApplyGameEnd;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (EnetClient.Instance != null)
            {
                EnetClient.Instance.OnPlayerMoveReceived -= ApplyPlayerMove;
                EnetClient.Instance.OnObjectMoveReceived -= ApplyObjectMove;
                EnetClient.Instance.OnDeathReceived -= ApplyDeath;
                EnetClient.Instance.OnRespawnReceived -= ApplyRespawn;
                EnetClient.Instance.OnShotFired -= ApplyShotFired;
                EnetClient.Instance.OnHit -= ApplyHit;
                EnetClient.Instance.OnGameEnded -= ApplyGameEnd;
            }
        }
        
        protected override void Initialize()
        {
            players.Clear();
            ingameMovers.Clear();
            syncObjects.Clear();
        }

        public void PrepareToNewMatch()
        {
            players.Clear();
            ingameMovers.Clear();
            syncObjects.Clear();
        }
    
        public PlayerBehaviour GetPlayerByKey(byte publicKey)
        {
            if (players.TryGetValue(publicKey, out var player))
            {
                return player;
            }
            return null;
        }

        // 스포너가 캡슐을 생성한 직후 호출할 등록 함수
        public void RegisterPlayer(byte publicKey, PlayerBehaviour player)
        {
            players[publicKey] = player;
        }

        
        public void RegisterSyncObject(uint objectId, SynchronizedObject so)
        {
            syncObjects[objectId] = so;
            if (so.TryGetComponent(out Mover mover))
            {
                ingameMovers[objectId] = mover;
                mover.BeginServerDriven();
            }
        }
        
        private void ApplyPlayerMove(byte publicKey, Vector3 pos, Vector3 rot, Vector3 vel)
        {
            var player = GetPlayerByKey(publicKey);
            if (player == null) return;
            player.ChangePosition(pos);
            player.ChangeDirection(rot);
            player.ChangeVelocity(vel.x, vel.y, vel.z);
        }

        private void ApplyObjectMove(uint targetId, Vector3 pos, Vector3 rot)
        {
            if (!ingameMovers.TryGetValue(targetId, out var mover) || mover == null) return;
            mover.ApplyServerMove(pos, rot);
        }

        private void ApplyDeath(DeathDto dto)
        {
            var player = GetPlayerByKey(dto.victimKey);
            if (player == null) return;
            player.Die();

            foreach (var info in MatchMakeStatic.Instance.playerConstructor)
            {
                if (info.publicKey == dto.killerKey) info.kill++;
                else if (info.publicKey == dto.victimKey) info.death++;
            }
        }

        private void ApplyShotFired(ShotNotifyDto dto)
        {
            var player = GetPlayerByKey(dto.playerKey);
            if (player == null) return;
            player.Shot(dto.dir, dto.origin);                       // 트레일(핸들드=noop, 언핸들드=발사)
            player.GetComponent<WeaponSystem>()?.ConsumeAmmoVisual();   // 탄약 표시 차감(본인 포함 전원)
        }

        private void ApplyHit(HitDto dto)
        {
            var prefab = dto.hitPart == 1 ? hitImpactHeadPrefab : hitImpactBodyPrefab;
            if (prefab != null)
                Instantiate(prefab, dto.hitPosition, Quaternion.identity);
        }

        private void ApplyRespawn(RespawnPlayerDto dto)
        {
            foreach (var entry in dto.players)
            {
                var player = GetPlayerByKey(entry.publicKey);
                if (player == null) continue;
                player.gameObject.SetActive(true);   // Die()에서 비활성화됐던 것 복구
                player.ChangePosition(entry.position);
            }
        }

        private void ApplyGameEnd(GameEndNotifyDto dto)
        {
            MatchMakeStatic.Instance.winningTeam = dto.winningTeam;   // GameEnd 씬은 여기서 읽음(MatchMakeStatic은 DontDestroyOnLoad라 씬 넘어가도 생존)
            SceneLoadingManager.Instance.LoadSceneAsync(SceneEnum.GameEnd, null);
        }

    }
}
