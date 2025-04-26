using Codes.InGame.Weapons;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Codes.InGame
{
    public abstract class Player : MonoBehaviour
    {
        [HideInInspector] public CharacterAnimSystem  animSystem;
        public int playerId;
        public float currentHp;

        public virtual void Init()
        {
            animSystem = GetComponent<CharacterAnimSystem>();
        }
    
        //Weapon
        public abstract void GetWeapon(Weapon got);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir">true:swap up, false:swap down</param>
        public abstract void SwapWeapon(bool dir);
        public abstract void ThrowWeapon();
        /// <summary>
        /// 막 총을 쏨!!.!!!>1>!!(player_handled가 사용 시에는 파라미터에 null 사용 요망
        /// </summary>
        /// <param name="dir">eularAngles</param>
        /// <param name="position"></param>
        public abstract void Shot(Vector3? dir, Vector3? position);
        
        //Position
        public abstract void ChangePosition(Vector3 pos);
        public abstract void ChangeDirection(Vector3 dir);
        public abstract void ChangeVelocity(float velocityX, float velocityY, float velocityZ);
        
        //Action
        public abstract void ChangePlayerState(PlayerState state);
        public abstract void Die();
    }

    public enum PlayerState
    {
        Default,
        Walking,
        Running,
        Sitting,
    }
}
