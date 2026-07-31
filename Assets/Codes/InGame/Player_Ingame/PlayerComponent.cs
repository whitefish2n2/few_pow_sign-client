using System.Text;
using Codes.Util;
using UnityEngine;

namespace Codes.InGame.Player_Ingame
{
    public class PlayerComponent:ServerComponent
    {
        public int publicKey;
        public float maxSpeed;
        public float acceleration;
        public float deceleration;
        [SerializeField] public float moveSpeed = 1;
        [SerializeField] public float jumpPower = 1;
        [SerializeField] public int maxHp;
        public int currentHp;   // 스폰 시 PlayerGenerateManager가 CharacterData 기준으로 maxHp와 함께 세팅함
        [SerializeField] public Vector3 aimOrigin;
        public float onGroundRadius = 0;
        public float onGroundYDistance = 0;
        [Header("Server Reconcile (client-only)")]
        public float reconcileThreshold = 1.5f;
        public float reconcileSpeed = 20f;
        
        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MaxSpeed:" + maxSpeed);
            sb.AppendLine("Acceleration:" + acceleration);
            sb.AppendLine("Deceleration:" + deceleration);
            sb.AppendLine("MoveSpeed:" + moveSpeed);
            sb.AppendLine("JumpPower:" + jumpPower);
            sb.AppendLine("MaxHp:" + maxHp);
            sb.AppendLine("OnGroundRadius:" + onGroundRadius);
            sb.AppendLine("OnGroundYDistance:" + onGroundYDistance);
            sb.AppendLine($"AimOrigin:{aimOrigin.x},{aimOrigin.y},{aimOrigin.z}");
            return sb.ToString();
        }
    }
}