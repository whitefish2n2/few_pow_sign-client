using System.Collections.Generic;

namespace NetCode.ENetCode
{
    public class AssignResponseDto
    {
        public byte myPublicKey;
        public Dictionary<byte, string> otherPlayers;

        public AssignResponseDto(byte myPublicKey)
        {
            this.myPublicKey = myPublicKey;
            otherPlayers = new Dictionary<byte, string>();
        }
    }
}