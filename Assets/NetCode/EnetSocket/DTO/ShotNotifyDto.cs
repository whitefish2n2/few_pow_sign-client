using UnityEngine;

namespace NetCode.ENetCode
{
    // S2P ShotNotify(26): 누가 어디서 어느 방향으로 쐈는지 — 사운드 텔레메트리
    public struct ShotNotifyDto
    {
        public byte playerKey;
        public Vector3 origin;
        public Vector3 dir;
    }
}
