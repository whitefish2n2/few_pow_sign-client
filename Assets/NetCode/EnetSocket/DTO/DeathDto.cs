namespace NetCode.ENetCode
{
    // S2P Death(16): 누가(victimKey) 누구한테(killerKey) 죽었는지
    public struct DeathDto
    {
        public byte victimKey;
        public byte killerKey;
    }
}
