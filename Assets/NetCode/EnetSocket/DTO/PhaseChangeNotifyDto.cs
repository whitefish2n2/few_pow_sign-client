namespace NetCode.ENetCode
{
    // S2P PhaseChangeNotify(31): 어떤 페이즈로 들어갔는지(서버 InGamePhase 값과 동일) + 그 페이즈 지속시간
    public struct PhaseChangeNotifyDto
    {
        public byte phase;
        public float duration;
    }
}
