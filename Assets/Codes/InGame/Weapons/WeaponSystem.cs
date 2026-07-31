using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codes.InGame.Player_Ingame;
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
        protected PlayerBehaviour parentPlayerBehaviour;
        protected Weapon characterHand;//플레이어 주먹
        


        
        
        
        
        
        public virtual void Init()
        {
            parentPlayerBehaviour = GetComponent<PlayerBehaviour>();
            
            if (weaponHolder == null)   // 로컬은 MoveSystem이 카메라 아래에 생성·주입 → 스킵. 원격만 여기서
            {
                var pc = GetComponent<PlayerComponent>();
                var t = new GameObject("Hand").transform;
                t.SetParent(transform);                              // 몸 자식 → body yaw 자동 추종
                t.localPosition = pc ? pc.aimOrigin : Vector3.zero;
                t.localRotation = Quaternion.identity;
                weaponHolder = t.gameObject;
            }
        }
        public void ApplyPickup(Weapon weapon, int slot, int holdingSlot)
        {
            weapon.ApplyPickupState();
            weapon.gameObject.transform.SetParent(weaponHolder.transform);
            weapon.disHold();
            weapons[slot] = weapon;
            ApplyHolding(holdingSlot);
        }

        public void ApplyDrop(Weapon weapon, Vector3 position, int holdingSlot)
        {
            int slot = weapons.IndexOf(weapon);
            if (slot >= 0) weapons[slot] = null;
            if (holdingWeapon == weapon) holdingWeapon = null;   // 방금 드롭한 무기가 참조로 남아있으면 ApplyHolding의 disHold가 재비활성화시킴
            weapon.Drop(transform.forward);        // 상태복구+임펄스(서버와 동일 크기1·yaw방향)
            weapon.transform.SetParent(null);
            weapon.transform.position = position;  // 회전은 손 포즈 유지 (서버 스트림이 확정)
            weapon.BeginServerDriven();            // 이후 궤적은 ObjectMove 스트림이 구동
            ApplyHolding(holdingSlot);
        }
        public void ApplySwap(int holdingSlot) => ApplyHolding(holdingSlot);

        public void ApplyReload(int slot, int ammo)
        {
            if (slot >= 0 && slot < weapons.Count && weapons[slot] != null)
                weapons[slot].currentAmmo = ammo;
        }

        private void ApplyHolding(int holdingSlot)
        {
            if (holdingWeapon) holdingWeapon.disHold();
            if (holdingSlot < 0 || holdingSlot >= weapons.Count || weapons[holdingSlot] == null)
            {
                holdingWeapon = null;              // 0xFF(빈손)도 여기로 떨어짐
                return;
            }
            holdingWeapon = weapons[holdingSlot];
            holdingWeaponType = (WeaponType)holdingSlot;
            WeaponHold();
        }
        public virtual void Swap(bool dir)
        {
            var r =SwapWeapon(dir);
            if (r is null) return;
            holdingWeapon = r;
        }

        public virtual void Swap(int idx)
        {
            
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
            //총 발사해요
            Debug.Log("빵");
        }

        // ShotNotify 수신 시 원격 플레이어 트레일 재생용 (UnHandledPlayerBehavior.Shot에서 호출)
        public void PlayShotEffect(Vector3 origin, Vector3 dir)
        {
            if (holdingWeapon == null) return;
            Vector3 reachPosition = Physics.Raycast(origin, dir, out RaycastHit hit, 300f)
                ? hit.point
                : origin + dir * 300f;
            holdingWeapon.Shot(reachPosition);
        }

        // ShotNotify 수신 시 탄약 표시 차감용 (본인 포함 전원, InGameLogicStatic에서 호출)
        public void ConsumeAmmoVisual()
        {
            if (holdingWeapon != null && holdingWeapon.currentAmmo > 0) holdingWeapon.currentAmmo--;
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


        public void SetWeaponHolder(GameObject holder) => weaponHolder = holder;

        public bool TryGetCurrentAmmo(out int current, out int max)
        {
            if (holdingWeapon == null)
            {
                current = 0;
                max = 0;
                return false;
            }
            current = holdingWeapon.currentAmmo;
            max = holdingWeapon.stat.maxAmmo;
            return true;
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