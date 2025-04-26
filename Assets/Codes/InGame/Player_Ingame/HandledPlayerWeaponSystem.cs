using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codes.InGame.Weapons;
using Unity.Mathematics;
using UnityEngine;

namespace Codes.InGame.Player_Ingame
{
    public class HandledPlayerWeaponSystem : WeaponSystem
    {
        public bool shotAble = true;
        
        int weaponIndex = 0;
        public override void Shot(Vector3 dir, Vector3 position)
        {
            if (holdingWeapon.currentAmmo != 0)
            {
                holdingWeapon.Shot();
                RaycastHit[] hit = new RaycastHit[] { };
                var size = Physics.RaycastNonAlloc(position, dir, hit, 300);
                foreach (var obj in hit)
                {
                    Debug.Log(obj.point);
                }
            }
            else
            {
                
            }
            base.Shot(dir, position);
        }
        
        
        public void Swap(bool up)
        {
            var r =SwapWeapon(up);
            if (r is null) return;
            holdingWeapon?.disHold();
            holdingWeapon = r;
            WeaponHold();
            holdingWeaponType = r.stat.type;
        }
        public void Swap(int idx)
        {
            var r = weapons[idx];
            if (r is null) return;
            holdingWeapon?.disHold();
            holdingWeapon = r;
            WeaponHold();
            holdingWeaponType = r.stat.type;
        }
        
        
        public override void GetWeapon(Weapon weapon)
        {
            bool isEmpty = (SwapWeapon(true) is null && SwapWeapon(false) is null) && holdingWeapon == null;
            weapon.gameObject.SetActive(false);
            weapon.gameObject.transform.SetParent(weaponHolder.transform);
            weapon.gameObject.transform.localPosition = weapon.stat.handlePosition;
            weapon.gameObject.transform.localRotation = Quaternion.Euler((weapon.stat.handleObjectRotation ));
            var beforeItem = weapons[(int)weapon.stat.type];
            //Debug.Log(beforeItem?.gameObject.name);
            weapons[(int)weapon.stat.type] = weapon;
            //Debug.Log("player get weapon");
            if (holdingWeaponType == weapon.stat.type || isEmpty )
            {
                holdingWeapon = weapon;
                
                holdingWeaponType = weapon.stat.type;
                WeaponHold();
            }
            if(beforeItem != null)
                DropWeaponOnChangeWeapon(beforeItem);
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
        IEnumerator SwapIE()
        {
            yield return null;
        }
    }
}