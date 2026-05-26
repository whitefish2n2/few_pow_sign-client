using System;
using System.Collections.Generic;

namespace NetCode
{
    public class MatchFoundDto
    {
        public string gameId;
        public string gameMode; // 모드 명 (enum name)
        public int teamInfo;    // 0: Blue, 1: Red 등
        public string map;      // 맵 이름 (enum name)
        public List<AnotherPlayerInfoDto> teamPlayers; // 아군 정보만 전송
        public long pickEndTime; // 픽 마감 시간 (밀리초 단위의 Unix Timestamp)
    }
}