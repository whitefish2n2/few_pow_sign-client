using System;
using System.Collections.Generic;
using Codes.InGame.Player_Ingame;
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
        public Dictionary<int, int> teamScores = new();   // team -> 누적 라운드 스코어
        private byte currentPhase;   // 서버 InGamePhase: 0=Initialize,1=Loading,2=Prepare,3=Fighting,4=Closing,5=Cleaning

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
                EnetClient.Instance.OnRoundEnded += ApplyRoundEnd;
                EnetClient.Instance.OnPhaseChanged += ApplyPhaseChanged;
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
                EnetClient.Instance.OnRoundEnded -= ApplyRoundEnd;
                EnetClient.Instance.OnPhaseChanged -= ApplyPhaseChanged;
            }
        }
        
        protected override void Initialize()
        {
            players.Clear();
            ingameMovers.Clear();
            syncObjects.Clear();
            teamScores.Clear();
        }

        public void PrepareToNewMatch()
        {
            players.Clear();
            ingameMovers.Clear();
            syncObjects.Clear();
            teamScores.Clear();
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
            var victim = GetPlayerByKey(dto.victimKey);
            var victimPc = victim ? victim.GetComponent<PlayerComponent>() : null;
            if (victimPc != null) victimPc.currentHp = dto.remainingHp;

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

                var pc = player.GetComponent<PlayerComponent>();
                if (pc != null) pc.currentHp = pc.maxHp;   // 리스폰 시 풀피 복구
            }
        }

        private void ApplyGameEnd(GameEndNotifyDto dto)
        {
            MatchMakeStatic.Instance.winningTeam = dto.winningTeam;   // GameEnd 씬은 여기서 읽음(MatchMakeStatic은 DontDestroyOnLoad라 씬 넘어가도 생존)
            SceneLoadingManager.Instance.LoadSceneAsync(SceneEnum.GameEnd, null);
        }

        private void ApplyRoundEnd(RoundEndNotifyDto dto)
        {
            teamScores[dto.winningTeam] = dto.winningTeamScore;
        }

        private void ApplyPhaseChanged(PhaseChangeNotifyDto dto)
        {
            currentPhase = dto.phase;
        }

        // ===== 인게임 UI 바인딩용 헬퍼 =====

        // 서버 InGamePhase enum과 순서 일치(Initialize,Loading,Prepare,Fighting,Closing,Cleaning)
        private static readonly string[] PhaseNames =
            { "Initialize", "Loading", "Prepare", "Fight", "Closing", "Cleaning" };

        public string GetCurrentPhaseText()
        {
            return currentPhase < PhaseNames.Length ? PhaseNames[currentPhase] : "?";
        }

        // MatchMakeStatic.playerConstructor(AnotherPlayerInfoDto)의 publicKey는 매치서버(외게임)가 채워주지 않는 필드라 항상 0으로 옴 —
        // 실제 인게임 publicKey는 데디케이트 서버가 AssignResponse로 내려준 InGameDataStatic.myPublicKey가 유일한 출처.
        // (예전엔 위 playerConstructor.publicKey를 썼는데 항상 0이라 다른 플레이어(publicKey 0인 사람)의 HP/탄약이 내 UI에 뜨는 버그였음)
        private byte GetMyPublicKey()
        {
            return InGameDataStatic.Instance.myPublicKey;
        }

        public int GetMyTeam()
        {
            byte myKey = GetMyPublicKey();
            foreach (var entry in InGameDataStatic.Instance.PlayerSpawnInfo)
            {
                if (entry.publicKey == myKey) return entry.team;
            }
            return 0;
        }

        // "HP:nn" 형식
        public string GetMyHpText()
        {
            var pc = GetPlayerByKey(GetMyPublicKey())?.GetComponent<PlayerComponent>();
            return pc != null ? $"HP:{pc.currentHp}" : "HP:-";
        }

        // "ammo:30/30" 형식
        public string GetMyAmmoText()
        {
            var ws = GetPlayerByKey(GetMyPublicKey())?.GetComponent<WeaponSystem>();
            if (ws != null && ws.TryGetCurrentAmmo(out int current, out int max))
                return $"ammo:{current}/{max}";
            return "ammo:-/-";
        }

        public int GetMyTeamScore()
        {
            return teamScores.TryGetValue(GetMyTeam(), out var score) ? score : 0;
        }

        // 상대팀이 여럿이면 그중 최고 점수
        public int GetBestEnemyTeamScore()
        {
            int myTeam = GetMyTeam();
            int best = 0;
            foreach (var kv in teamScores)
            {
                if (kv.Key == myTeam) continue;
                if (kv.Value > best) best = kv.Value;
            }
            return best;
        }
    }
}
