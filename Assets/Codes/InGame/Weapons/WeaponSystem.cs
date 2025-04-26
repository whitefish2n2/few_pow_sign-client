using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Codes.InGame.Weapons
{
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] protected readonly List<Weapon> weapons = new(5) { null, null, null, null, null };
        [SerializeField] protected WeaponType holdingWeaponType;
        [SerializeField] protected Weapon holdingWeapon;
        [SerializeField] protected GameObject weaponHolder;//총의 transform 부모 개체 
        protected Player ParentPlayer;
        protected Weapon characterHand;//플레이어 주먹
        private void Awake()
        {
            ParentPlayer = GetComponent<Player>();
        }
        public void Swap(bool dir)
        {
            var r =SwapWeapon(dir);
            if (r is null) return;
            holdingWeapon = r;
        }
        private Weapon SwapWeapon(bool dir)
        {
            if (weapons.Count == 0) return null;
            return dir switch
            {
                true => weapons.Skip((int)holdingWeaponType + 1).FirstOrDefault(a => a),//무기를 위로 스왑했을 때 존재하는 무기를 반환(미존재시 null 반환, 스왑 캔슬)
                _ => weapons.Take((int)holdingWeaponType).LastOrDefault(a => a)//무기를 아래로 스왑했을 때 존재하는 무기를 반환(미존재시 null 반환, 스왑 캔슬)
            };
        }
        public virtual void Shot(Vector3 position, Vector3 direction)
        {
            //총 발사 이펙트에요
            Debug.Log("빵");
        }

        public virtual void Reload()
        {
            //재장전 애니메이션 실행이에요
            if (holdingWeapon.currentAmmo == holdingWeapon.stat.maxAmmo) return;
            holdingWeapon.Reload();
        }
        
        public virtual void GetWeapon(Weapon weapon)
        {
            /*todo: 게임 내에 존재하는 weapon에 id를 매기는 로직을 작성하고 해당 함수에서는 id를 받아
            해당 id를 기반으로(id:key weapon:value인 딕셔너리 존재) 상응하는 weapon을 탐색해 받아와요*/
            weapon.gameObject.SetActive(false);
            weapon.gameObject.transform.SetParent(weaponHolder.transform);
            weapon.disHold();
            var beforeItem = weapons[(int)weapon.stat.type];
            weapons[(int)weapon.stat.type] = weapon;
            if (holdingWeaponType == weapon.stat.type)
            {
                DropWeaponOnChangeWeapon(holdingWeapon);
                holdingWeapon = weapon;
                WeaponHold();
            }
            else if(beforeItem != null)
                DropWeaponOnChangeWeapon(beforeItem);
        }

        public void DropWeaponOnChangeWeapon([NotNull]Weapon beforeItem)
        {
            beforeItem.Drop(gameObject.transform.forward);
        }
        public void WeaponHold()
        {
            StartCoroutine(WeaponHoldIE());
        }

        public IEnumerator WeaponHoldIE()
        {
            /*if (ParentPlayer.playerId == GameStatic.Instance.LocalPovPlayer.playerId)
            {
                holdingWeapon.손에 든 애니메이션 트리거
            }*/
            holdingWeapon.Hold();
            yield break;
        }
        
        /// <summary>
        /// 무기 버리기 버튼을 눌렀을 떄 작동(무기 교체는 다른 함수 사용)
        /// </summary>
        /// <param name="throweWeaponIndex"></param>
        public virtual void DropWeapon_Direct()
        {
            Debug.Log(holdingWeapon);
            if (!holdingWeapon) return;
            weapons[(int)holdingWeapon.stat.type] = null;
            holdingWeapon.Drop(transform.forward);
            holdingWeapon = SwapWeapon(true) ?? SwapWeapon(false);
            if(holdingWeapon)
            {
                holdingWeaponType = holdingWeapon.stat.type;
                WeaponHold();
            }
        }
        
        
    }
    
    public enum WeaponType
    {
        MainWeapon,
        SubWeapon,
        Knife,
        Hand,
        Skill
    }
}