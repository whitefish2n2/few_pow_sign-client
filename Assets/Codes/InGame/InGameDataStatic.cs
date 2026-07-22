using System.Collections.Generic;
using NetCode.ENetCode;
using Plugins;

namespace Codes.InGame
{
    /// <summary>
    /// 네트워크를 통해 받은 인게임 정적 데이터(매핑 정보 등)를 보관하는 순수 데이터 컨테이너
    /// </summary>
    public class InGameDataStatic : MonoSingleton<InGameDataStatic>
    {
        // 1. 플레이어 매핑 정보 (ENet 통신용 byte Key <-> 유저 ID)
        public byte myPublicKey { get; private set; }
        public Dictionary<byte, string> keyToUserIdMap { get; private set; } = new();

        // 2. 동적 맵 오브젝트 매핑 정보 (서버 Object ID <-> 오브젝트 이름/타입)
        public Dictionary<uint, string> ObjectIdToNameMap { get; private set; } = new();
        
        public List<GeneratePlayerEntry> PlayerSpawnInfo { get; private set; } = new();
        
        

        protected override void Initialize() { }

        public void PrepareToNewMatch()
        {
            myPublicKey = 0;
            keyToUserIdMap.Clear();
            ObjectIdToNameMap.Clear();
        }

        // 로딩 씬 매니저가 데이터를 꽂아줄 때 쓰는 함수들
        public void SetAssignData(byte myKey, Dictionary<byte, string> otherPlayers, string myUserId)
        {
            myPublicKey = myKey;
            keyToUserIdMap[myKey] = myUserId;
            foreach (var kvp in otherPlayers)
            {
                keyToUserIdMap[kvp.Key] = kvp.Value;
            }
        }

        public void SetMapInitData(Dictionary<uint, string> mapData)
        {
            ObjectIdToNameMap = new Dictionary<uint, string>(mapData);
        }
        
        public void SetPlayerSpawnInfo(List<GeneratePlayerEntry> players)
        {
            PlayerSpawnInfo = players;
        }
    }
}