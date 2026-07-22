namespace NetCode.ENetCode
{
    // S2P RoundEndNotify(33): 이번 라운드 승리팀 + 그 팀의 누적 스코어
    public struct RoundEndNotifyDto
    {
        public byte winningTeam;
        public byte winningTeamScore;
    }
}
