using Codes.InGame.Weapons;
using UnityEngine;

namespace Codes.InGame.Player_Ingame
{
    public class UnHandledPlayerBehavior:PlayerBehaviour
    {
        private Rigidbody rb;
        private PlayerComponent pc;
        private Vector3 serverPos;
        private Vector3 serverVel;
        private float serverYaw;
        private float lastPacketTime;
        private bool hasTarget;

        public override void Init()
        {
            base.Init();
            rb = GetComponent<Rigidbody>();
            pc = GetComponent<PlayerComponent>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; } // 직접 구동, 물리 끔
        }

        public override void ChangePosition(Vector3 pos) { serverPos = pos; lastPacketTime = Time.time; hasTarget = true; }
        public override void ChangeDirection(Vector3 dir) { serverYaw = dir.y; }
        public override void ChangeVelocity(float vx, float vy, float vz) { serverVel = new Vector3(vx, vy, vz); }

        public override void Teleport(Vector3 pos)
        {
            serverPos = pos;
            serverVel = Vector3.zero;
            lastPacketTime = Time.time;
            hasTarget = true;
            transform.position = pos;
        }

        private void LateUpdate()
        {
            if (!hasTarget || pc == null) return;
            float since = Time.time - lastPacketTime;
            Vector3 target = serverPos + serverVel * since;  // velocity로 외삽 (패킷 사이 coasting)
            transform.position = Vector3.MoveTowards(transform.position, target, pc.reconcileSpeed * Time.deltaTime); // 트레이서
            transform.rotation = Quaternion.Euler(0f, serverYaw, 0f);
        }
        
        public override void GetWeapon(Weapon got)
        {
            throw new System.NotImplementedException();
        }

        public override void SwapWeapon(bool dir)
        {
            throw new System.NotImplementedException();
        }

        public override void ThrowWeapon()
        {
            throw new System.NotImplementedException();
        }

        public override void Shot(Vector3? dir, Vector3? position)
        {
            if (dir == null || position == null) return;
            GetComponent<WeaponSystem>()?.PlayShotEffect(position.Value, dir.Value);
        }
        
        public override void ChangePlayerState(PlayerState state)
        {
            throw new System.NotImplementedException();
        }

        public override void Die()
        {
            gameObject.SetActive(false);
        }
    }
}