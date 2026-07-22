using System;
using UnityEngine;


namespace NetCode.ENetCode
{
    public struct MoveRequestDto
    {
        public UInt16 SessionKey;
        public UInt64 Timestamp;
        public Vector2 InputVector;
        public float inputYaw;
        public float inputPitch;
        public const int InputPayloadLength = 18;

        public void Encode(byte[] buf)
        {
            buf[0] = (byte)(sbyte)(InputVector.x * 127f);
            buf[1] = (byte)(sbyte)(InputVector.y * 127f);
            Buffer.BlockCopy(BitConverter.GetBytes(inputPitch), 0, buf, 2, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(inputYaw),   0, buf, 6, 4);
        }
    }
    
    
}