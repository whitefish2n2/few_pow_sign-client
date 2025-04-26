using System;
using System.Dynamic;
using Codes.InGame;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Interactable : MonoBehaviour
{
    public int interactableId;
    [HideInInspector] public UnityEvent onTarget;
    [HideInInspector] public UnityEvent onDisTarget;
    /// <summary>
    /// 게임에 영향을 미치는 액션을 실행할 땐 ServerManager.EventBroadCast(Interactable id:int)가 자동으로 호출되어요 
    /// </summary>
    [HideInInspector] public UnityEvent<Player> onInteract;
    
    [FormerlySerializedAs("interactable")] public bool isInteractable = true;

    public void Interact(Player player, bool triggerBroadcast = true)
    {
        if(!isInteractable) return;
        if (triggerBroadcast)
        {
            //ServerManager.EventBroadCast(EventType.Interact,interactableId);
        }
        onInteract?.Invoke(player);
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
}
