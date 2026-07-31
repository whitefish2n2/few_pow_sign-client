using System;
using Codes.InGame;
using Codes.InGame.Player_Ingame;
using Codes.InGame.Weapons;
using NetTest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class HandledPlayerBehavior : PlayerBehaviour
{
    [HideInInspector] public MoveSystem moveSystem;
    [HideInInspector] public HandledPlayerWeaponSystem handledPlayerWeaponSystem;
    

    public override void Init()
    {
        moveSystem = GetComponent<MoveSystem>();
        handledPlayerWeaponSystem = GetComponent<HandledPlayerWeaponSystem>();
        
        base.Init();
        pc = GetComponent<PlayerComponent>();
        rb = GetComponent<Rigidbody>();
        rb.maxDepenetrationVelocity = 2f;
        IngameInputDispatcher.Instance.OnInteract  += Interact;
        IngameInputDispatcher.Instance.OnThrow     += ThrowWeapon_Input;
        IngameInputDispatcher.Instance.OnScroll    += SwapWeapon_Input;
        IngameInputDispatcher.Instance.OnReload    += Reload_Input;
    }
    //Interactiong
    Interactable currentInteractable;
    private void FixedUpdate()
    {
        if (!Physics.Raycast(moveSystem.cam.transform.position,  moveSystem.cam.transform.forward, out RaycastHit hit, 5f))
        {
            if(currentInteractable)
                currentInteractable.DisTargeted();
            
        }
        else if (hit.transform && hit.transform.CompareTag("Interactable"))
        {
            if (currentInteractable?.gameObject != hit.transform.gameObject)
            {
                if(currentInteractable)
                    currentInteractable.DisTargeted();
                currentInteractable = hit.transform.GetComponent<Interactable>().Targeted();
            }
        }
        else if (hit.transform)
        {
            if(currentInteractable)
                currentInteractable.DisTargeted();
            currentInteractable = null;
        }
        if (!hasServer || !pc || !rb) return;

        // 현재 물리 위치와 서버 위치의 차이 계산
        float distance = Vector3.Distance(rb.position, serverPos);

        if (distance < pc.reconcileThreshold) 
        {
            return; // 오차가 작으면 내 클라이언트 예측(MoveSystem)을 100% 신뢰
        }

        if (distance > 8.0f)
        {
            // 텔레포트 시 기존 물리 가속도를 완전히 초기화해야 폭발하지 않습니다.
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
        
            rb.position = serverPos;
            return;
        }

        // 부드러운 위치 보정 (물리 충돌을 유지하는 MovePosition 사용!)
        Vector3 correctedPos = Vector3.MoveTowards(rb.position, serverPos, pc.reconcileSpeed * Time.fixedDeltaTime);
        rb.MovePosition(correctedPos);

        // 속도 보정
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, serverVel, pc.reconcileSpeed * Time.fixedDeltaTime);
    }
    
    //Function that depend on Event
    public override void GetWeapon(Weapon got)
    {
        handledPlayerWeaponSystem.GetWeapon(got);
    }
    

    public void SwapWeapon_Input(Vector2 dir)
    {
        if (dir.y != 0)
        {
            var d = dir.y < 0;
            SwapWeapon(d);
        }
    }
    public override void SwapWeapon(bool dir)
    {
        EnetClient.Instance.SendSwapWeaponPacket(dir);   // true=위(높은 슬롯), 서버 SwapDir(up)과 동일 의미
    }

    public void SwapWeaponWithIndex(InputAction.CallbackContext context)
    {
        var idx = context.ReadValue<int>();
        handledPlayerWeaponSystem.Swap(idx);
    }
    
    public void ThrowWeapon_Input()
    {
        Debug.Log("click g");
        ThrowWeapon();
    }
    public override void ThrowWeapon()
    {
        EnetClient.Instance.SendDropWeaponPacket();
    }

    public override void Shot(Vector3? dir, Vector3? position)
    { }

    public void Interact()
    {
        if (currentInteractable)   // 조준 힌트 있을 때만 송신
            EnetClient.Instance.SendInteractPacket();
    }
    public void Reload_Input()
    {
        EnetClient.Instance.SendReloadPacket();
    }

    
    private Rigidbody rb;
    private PlayerComponent pc;
    private Vector3 serverPos;
    private Vector3 serverVel;
    private bool hasServer;

    public override void ChangePosition(Vector3 pos) { serverPos = pos; hasServer = true; }
    public override void ChangeVelocity(float vx, float vy, float vz) { serverVel = new Vector3(vx, vy, vz); }
    public override void ChangeDirection(Vector3 dir) { } // 본인 시야=로컬 마우스 권위

    public override void Teleport(Vector3 pos)
    {
        serverPos = pos;
        serverVel = Vector3.zero;
        hasServer = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = pos;
    }



    public override void ChangePlayerState(PlayerState state)
    {
        throw new System.NotImplementedException();
    }
    
    //state
    public override void Die()
    {
        gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (IngameInputDispatcher.TryGetInstance(out var dispatcher))
        {
            dispatcher.OnInteract  -= Interact;
            dispatcher.OnThrow     -= ThrowWeapon_Input;
            dispatcher.OnScroll    -= SwapWeapon_Input;
            dispatcher.OnReload    -= Reload_Input;
        }
    }
}
