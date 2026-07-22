using Codes.Util;
using UnityEngine;

namespace Codes.InGame
{
    public class SynchronizedObject :ServerComponent
    {
        [HideInInspector] public int objectId;
        public override string Serialize()
        {
            return $"ObjectId: {objectId}";
        }
    }
}