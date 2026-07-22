using System.Collections.Generic;
using Codes.InGame;
using Codes.InGame.Weapons;
using NetCode.ENetCode;
using NetTest;
using UnityEngine;

public class WeaponNetworkManager : MonoBehaviour
{

        private void Start()
        {

            EnetClient.Instance.OnWeaponPickup += OnPickup;
            EnetClient.Instance.OnWeaponDrop   += OnDrop;
            EnetClient.Instance.OnWeaponSwap   += OnSwap;
            EnetClient.Instance.OnWeaponReload += OnReload;
        }

        private void OnDestroy()
        {
            if (EnetClient.Instance == null) return;
            EnetClient.Instance.OnWeaponPickup -= OnPickup;
            EnetClient.Instance.OnWeaponDrop   -= OnDrop;
            EnetClient.Instance.OnWeaponSwap   -= OnSwap;
            EnetClient.Instance.OnWeaponReload -= OnReload;
        }

        private readonly Dictionary<byte, WeaponSystem> systemCache = new();

        private WeaponSystem SystemOf(byte playerKey)
        {
            if (systemCache.TryGetValue(playerKey, out var cached) && cached) return cached;   // Unity 파괴체크 겸용
            var player = InGameLogicStatic.Instance.GetPlayerByKey(playerKey);
            var ws = player ? player.GetComponent<WeaponSystem>() : null;
            if (ws) systemCache[playerKey] = ws;
            return ws;
        }
        
        private readonly Dictionary<uint, Weapon> weaponCache = new();
        private Weapon WeaponOf(uint targetId)
        {
            if(weaponCache.TryGetValue(targetId, out var cached) && cached) return cached;
            if (!InGameLogicStatic.Instance.syncObjects.TryGetValue(targetId, out var so) || !so) return null;
            var weapon = so.GetComponent<Weapon>();
            weaponCache[targetId] = weapon;
            return so.GetComponent<Weapon>();
        }


        private void OnPickup(GetWeaponNotifyDto dto)
        {
            
            var ws = SystemOf(dto.pickerKey);
            var weapon = WeaponOf(dto.weaponTargetId);
            if (ws == null || !weapon)
            { Debug.LogError($"[WeaponApply] pickup 해석 실패 key:{dto.pickerKey} weapon:{dto.weaponTargetId}"); return; }
            
            ws.ApplyPickup(weapon, dto.slot, dto.holdingSlot);
        }

        private void OnDrop(DropWeaponNotifyDto dto)
        {
            var ws = SystemOf(dto.dropperKey);
            var weapon = WeaponOf(dto.weaponTargetId);
            if (ws == null || !weapon)
            { Debug.LogError($"[WeaponApply] drop 해석 실패 key:{dto.dropperKey} weapon:{dto.weaponTargetId}"); return; }
            ws.ApplyDrop(weapon, dto.position, dto.holdingSlot);
        }

        
        private void OnSwap(SwapWeaponNotifyDto dto)   => SystemOf(dto.playerKey)?.ApplySwap(dto.holdingSlot);
        private void OnReload(ReloadNotifyDto dto)     => SystemOf(dto.playerKey)?.ApplyReload(dto.slot, dto.currentAmmo);
}
