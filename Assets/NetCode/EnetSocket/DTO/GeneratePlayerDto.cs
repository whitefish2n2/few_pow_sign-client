using System.Collections.Generic;
using UnityEngine;

namespace NetCode.ENetCode
{
    public class GeneratePlayerDto
    {
        public List<GeneratePlayerEntry> players = new List<GeneratePlayerEntry>();
    }

    public struct GeneratePlayerEntry
    {
        public byte publicKey;
        public byte team;
        public byte charId;      
        public Vector3 spawnPos;
    }
}