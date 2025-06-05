using UnityEngine;

namespace Codes.InGame.Player_Ingame
{
    public class UnHandledPlayer:Player
    {
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
            throw new System.NotImplementedException();
        }

        public override void ChangePosition(Vector3 pos)
        {
            throw new System.NotImplementedException();
        }

        public override void ChangeDirection(Vector3 dir)
        {
            throw new System.NotImplementedException();
        }

        public override void ChangeVelocity(float velocityX, float velocityY, float velocityZ)
        {
            throw new System.NotImplementedException();
        }

        public override void ChangePlayerState(PlayerState state)
        {
            throw new System.NotImplementedException();
        }

        public override void Die()
        {
            throw new System.NotImplementedException();
        }
    }
}