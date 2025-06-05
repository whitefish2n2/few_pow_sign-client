using System;
using UnityEngine;


namespace NetCode.ENetCode
{
    public struct MoveRequestDto
    {
        public UInt16 SessionKey;
        public UInt64 UserPrivateKey;
        public UInt64 Timestamp;
        public Vector2 InputVector;
        public Vector3 RotEular;
    }
    
    
}