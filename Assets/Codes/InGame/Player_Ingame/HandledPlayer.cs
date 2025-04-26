using System;
using Codes.InGame;
using Codes.InGame.Player_Ingame;
using Codes.InGame.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class HandledPlayer : Player
{
    [HideInInspector] public MoveSystem moveSystem;
    [HideInInspector] public HandledPlayerWeaponSystem handledPlayerWeaponSystem;

    private void Awake()
    {
        moveSystem = GetComponent<MoveSystem>();
        handledPlayerWeaponSystem = GetComponent<HandledPlayerWeaponSystem>();
    }

    public override void Init()
    {
        handledPlayerWeaponSystem = GetComponent<HandledPlayerWeaponSystem>();
        base.Init();
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
        //Debug.Log(hit.transform?.name);
    }
    
    //Function that depend on Event
    public override void GetWeapon(Weapon got)
    {
        handledPlayerWeaponSystem.GetWeapon(got);
    }
    

    public void SwapWeapon_Input(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var dir = context.ReadValue<Vector2>();
            if (dir.y != 0)
            {
                var d = dir.y < 0;
                SwapWeapon(d);
            }
        }
    }
    public override void SwapWeapon(bool dir)
    {
        handledPlayerWeaponSystem.Swap(dir);
    }
    

    public void SwapWeaponWithIndex(InputAction.CallbackContext context)
    {
        var idx = context.ReadValue<int>();
        handledPlayerWeaponSystem.Swap(idx);
    }

    public void ThrowWeapon_Input(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("click g");
            ThrowWeapon();
        }
    }
    public override void ThrowWeapon()
    {
        handledPlayerWeaponSystem.DropWeapon_Direct();
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started && currentInteractable)
        {
            currentInteractable.Interact(this);
        }
    }
    public void Move(InputAction.CallbackContext context)
    {
        moveSystem.Move(context);
    }

    public void Click_Leftt(InputAction.CallbackContext context)
    { ;
        if(context.performed) Shot(moveSystem.cam.transform.position, moveSystem.cam.transform.forward);//cam대신 head같은거 만들어서 대체하셈
        
    }
    public override void Shot(Vector3? dir, Vector3? position)
    {
        handledPlayerWeaponSystem.Shot(dir??transform.rotation.eulerAngles,position??transform.position);
    }


    //Instant Change Value Function(for server Re-synchro)
    public override void ChangePosition(Vector3 pos)
    {
        Shot(null,null);
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
    
    //state
    public override void Die()
    {
        throw new System.NotImplementedException();
    }
}
