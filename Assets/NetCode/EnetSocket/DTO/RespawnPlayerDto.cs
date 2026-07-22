using System.Collections.Generic;
using UnityEngine;

namespace NetCode.ENetCode
{
    public class RespawnPlayerDto
    {
        public List<RespawnPlayerEntry> players = new List<RespawnPlayerEntry>();
    }

    public struct RespawnPlayerEntry
    {
        public byte publicKey;
        public Vector3 position;
    }
}