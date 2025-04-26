using System;
using System.Numerics;
using UnityEngine;

namespace Codes.InGame.Player_Ingame
{
    public class StaticInput
    {
        public static readonly StaticInput instance = new();
        public UnityEngine.Vector2 inputVector;
        public UnityEngine.Vector3 rotEular;

        //GET LITTLE ENDIAN BINARY
        public byte[] GetInputBinary()
        {
            byte[] binaryData = new byte[15];
            binaryData[0] = 1; // messageType: input
            binaryData[1] = (byte)inputVector.x;
            binaryData[2] = (byte)inputVector.y;
            Buffer.BlockCopy(BitConverter.GetBytes(rotEular.x), 0, binaryData, 3, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rotEular.y), 0, binaryData, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rotEular.z), 0, binaryData, 11, 4);
            string debug = "";
            for (int i = 0; i < binaryData.Length; ++i) {
                debug += binaryData[i].ToString("X2") + " ";
            }
            Debug.Log(debug);
            return binaryData;
        }
    }
}