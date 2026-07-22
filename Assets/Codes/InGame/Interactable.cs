using System;
using System.Dynamic;
using Codes.InGame;
using Codes.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Interactable : ServerComponent
{
    [HideInInspector] public UnityEvent onTarget;
    [HideInInspector] public UnityEvent onDisTarget;
    /// <summary>
    /// 게임에 영향을 미치는 액션을 실행할 땐 ServerManager.EventBroadCast(Interactable id:int)가 자동으로 호출되어요 
    /// </summary>
    [HideInInspector] public UnityEvent<PlayerBehaviour> onInteract;
    
    [FormerlySerializedAs("interactable")] public bool isInteractable = true;

    public void Interact(PlayerBehaviour playerBehaviour, bool triggerBroadcast = true)
    {
        if(!isInteractable) return;
        onInteract?.Invoke(playerBehaviour);
    }

    public Interactable Targeted(bool triggerBroadcast = false)
    {
        if(!isInteractable) return this;
        if (triggerBroadcast)
        {
            //ServerManager.EventBroadCast(EventType.Targeted,interactableId);
        }
        onTarget?.Invoke();
        return this;
    }

    public void DisTargeted(bool triggerBroadcast = false)
    {
        if (triggerBroadcast)
        {
            //ServerManager.EventBroadCast(EventType.DisTargeted,interactableId);
        }
        onDisTarget?.Invoke();
    }

    public override string Serialize()
    {
        return "";
    }
}
