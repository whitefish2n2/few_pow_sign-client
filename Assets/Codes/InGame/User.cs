using System;
using UnityEngine;

namespace Codes.InGame
{
    [Serializable]
    public class InGameUser
    {
        public int sessionPlayerId;
        public float playerXVelocity;
        public float playerYVelocity;
        public float playerZVelocity;
        public float playerXRotation;
        public float playerYRotation;
        public float playerZRotation;
    }
}
