namespace NetCode.ENetCode
{
    // S2P ReloadNotify(24): 누가 어느 슬롯을 리로드해 잔탄이 얼마인지
    public struct ReloadNotifyDto
    {
        public byte playerKey;
        public byte slot;
        public ushort currentAmmo;
    }
}
