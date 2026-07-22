using UnityEngine;

namespace NetCode.ENetCode
{
    // S2P DropWeaponNotify(20): 누가 어떤 무기를 어디에 버렸는지 + 드롭 후 장착슬롯
    public struct DropWeaponNotifyDto
    {
        public byte dropperKey;
        public uint weaponTargetId;
        public Vector3 position;      // 드롭 원점 (서버는 여기서 forward 임펄스)
        public byte holdingSlot;      // 0xFF = 빈손
    }
}
