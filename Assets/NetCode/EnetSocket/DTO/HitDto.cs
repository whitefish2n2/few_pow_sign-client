using UnityEngine;

namespace NetCode.ENetCode
{
    // S2P HitNotify(28): 누가(attackerKey) 누구를(victimKey) 어디를(hitPart) 맞춰 잔여체력(remainingHp)이 얼마인지
    public struct HitDto
    {
        public byte victimKey;
        public byte attackerKey;
        public byte hitPart;      // 0=body, 1=head
        public ushort remainingHp;
        public Vector3 hitPosition;   // 이펙트 표시용
    }
}
