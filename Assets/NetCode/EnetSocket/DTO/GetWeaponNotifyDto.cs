namespace NetCode.ENetCode
{
    // S2P GetWeaponNotify(18): 누가 어떤 무기를 어느 슬롯에 주웠는지 + 현재 장착슬롯
    public struct GetWeaponNotifyDto
    {
        public byte pickerKey;
        public uint weaponTargetId;   // MapInit id 매핑으로 월드 무기 특정
        public byte slot;
        public byte holdingSlot;      // 0xFF = 빈손
    }
}
