using System;
using System.Numerics;
using ENet;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Codes.InGame.Player_Ingame
{
    public class StaticInput
    {
        public static readonly StaticInput instance = new();
        
        public UnityEngine.Vector2 inputVector;
        
        public UnityEngine.Vector3 rotEular;

        public UnityEngine.Vector3 rotTemp;
        //
        /// <summary>
        /// GET LITTLE ENDIAN BINARY
        /// </summary>
        /// <returns>binary input data(should be return to arrayPool</returns>
        public byte[] GetInputBinary()
        {
            if (inputVector == Vector2.zero && Approximately(rotEular, rotTemp))
                return null;
            rotTemp = rotEular;
            byte[] binaryData = System.Buffers.ArrayPool<byte>.Shared.Rent(22);
            Buffer.BlockCopy(BitConverter.GetBytes(NetTestStatic.instance.userPrivateKey), 0, binaryData, 0, 8);
            binaryData[8] = (byte)(sbyte)inputVector.x;
            binaryData[9] = (byte)(sbyte)inputVector.y;
            Buffer.BlockCopy(BitConverter.GetBytes(rotEular.x), 0, binaryData, 10, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rotEular.y), 0, binaryData, 14, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rotEular.z), 0, binaryData, 18, 4);
            string debug = "";
            for (int i = 0; i < binaryData.Length; ++i) {
                debug += binaryData[i].ToString("X2") + " ";
            }
            Debug.Log(debug);
            return binaryData;
        }
        bool Approximately(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z);
        }
    }
}