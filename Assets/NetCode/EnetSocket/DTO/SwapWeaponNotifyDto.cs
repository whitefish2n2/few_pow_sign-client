namespace NetCode.ENetCode
{
    // S2P SwapWeaponNotify(22): 누가 어느 슬롯을 장착 중인지
    public struct SwapWeaponNotifyDto
    {
        public byte playerKey;
        public byte holdingSlot;
    }
}
