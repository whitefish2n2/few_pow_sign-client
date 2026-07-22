using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codes.InGame.Weapons;
using NetTest;
using Unity.Mathematics;
using UnityEngine;

namespace Codes.InGame.Player_Ingame
{
    public class HandledPlayerWeaponSystem : WeaponSystem
    {
        private readonly RaycastHit[] _hits = new RaycastHit[16];
        public bool shotAble = true;
        private MoveSystem moveSystem;
        int weaponIndex = 0;
        
        
        private void Update()
        {
            if (!IngameInputDispatcher.Instance.GetIsAttackPressed()) return;
            if (holdingWeapon == null) return;
            if (!holdingWeapon.CanShoot()) return;

            holdingWeapon.RegisterShot();
            Shot(moveSystem.cam.transform.position,moveSystem.cam.transform.forward);
        }

        public override void Init()
        {
            base.Init();
            moveSystem = GetComponent<MoveSystem>();
        }

        public override void Shot(Vector3 position, Vector3 dir)
        {
            if (holdingWeapon == null) return;
            if (holdingWeapon.currentAmmo != 0)
            {
                Vector3 reachPosition = Physics.Raycast(position, dir, out RaycastHit reachHit, 300f)
                    ? reachHit.point
                    : position + dir * 300f;
                holdingWeapon.Shot(reachPosition);

                var size = Physics.RaycastNonAlloc(position, dir, _hits, 300);

                // 서버 PhysicsSystem::Raycast와 동일한 방식: 자기 자신만 제외하고 선형 스캔으로 최근접 후보 하나 추적
                int bestIndex = -1;
                float bestDistance = float.MaxValue;
                for (int i = 0; i < size; i++)
                {
                    if (_hits[i].collider.gameObject.CompareTag("Player"))
                    {
                        var pc = _hits[i].collider.gameObject.GetComponent<PlayerComponent>();
                        if (pc != null && pc.publicKey == InGameDataStatic.Instance.myPublicKey)
                            continue;   // 자기 자신은 후보에서 제외
                    }

                    if (_hits[i].distance < bestDistance)
                    {
                        bestDistance = _hits[i].distance;
                        bestIndex = i;
                    }
                }

                if (bestIndex < 0)
                {
                    EnetClient.Instance.SendShotPacket();   // 사거리 안에 자기 자신 말곤 아무것도 없음
                    return;
                }

                var best = _hits[bestIndex];
                if (best.collider.gameObject.CompareTag("Player"))
                {
                    var pc = best.collider.gameObject.GetComponent<PlayerComponent>();
                    EnetClient.Instance.SendHitThisPacket((byte)pc.publicKey, position, dir);
                }
                else
                {
                    //벽에 총알 부딫힘(todo: 벽에 안 부딫혔는데도 다른 변수로 인해 여기로 올 경우 확인 필요)
                    Debug.Log(best.collider.gameObject.name);
                    EnetClient.Instance.SendShotPacket();
                }
            }
            base.Shot(position,dir);
        }
        
        
        public override void Swap(bool up)
        {
            var r =SwapWeapon(up);
            if (r is null) return;
            holdingWeapon?.disHold();
            holdingWeapon = r;
            WeaponHold();
            holdingWeaponType = r.stat.type;
        }
        public override void Swap(int idx)
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