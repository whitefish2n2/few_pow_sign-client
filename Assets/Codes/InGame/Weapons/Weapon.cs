using System;
using Codes.InGame;
using UnityEngine;
using UnityEngine.Serialization;

public class Weapon : Mover
{
    private static readonly int Fire = Animator.StringToHash("Fire");
    private static readonly int Reload1 = Animator.StringToHash("Reload");
    private static readonly int Drop1 = Animator.StringToHash("Drop");
    private static readonly int Down = Animator.StringToHash("Down");
    private static readonly int Up = Animator.StringToHash("Up");
    private static readonly int Hold1 = Animator.StringToHash("Hold");
    private static readonly int Init1 = Animator.StringToHash("Init");

    public Player owner;
    public WeaponStat stat;
    public Animator animator;
    public bool canFire;
    public int currentAmmo;
    public bool isHolding;
    public bool isOnInventory;
    private Collider col;
    [HideInInspector] public GameObject handleWeaponInstance;
    private Animator handleWeaponAnimator;
    
    //interact component
    private Interactable interactable;
    private Material highlightMaterial;

    private void Awake()
    {
        col = GetComponent<Collider>();
        //handleWeaponInstance = Instantiate(stat.handleObjectPrefab);
        //handleWeaponInstance.SetActive(false);
        animator = GetComponent<Animator>();
        interactable = GetComponent<Interactable>();
        interactable.onInteract.AddListener(Get);
        interactable.onTarget.AddListener(OnTarget);
        interactable.onDisTarget.AddListener(DisTarget);
        highlightMaterial = GetComponent<Renderer>().material;
        rb = GetComponent<Rigidbody>();
        Init();
    }

    public void Init()
    {
        interactable.isInteractable = true;
        currentAmmo = stat.maxAmmo;
        //handleWeaponInstance.SetActive(false);
        isHolding = false;
        isOnInventory = false;
        col.enabled = true;
        gameObject.transform.parent = null;
        animator.SetTrigger(Init1);
        gameObject.layer = LayerMask.NameToLayer("gun");
        rb.isKinematic = false;
        foreach (Transform child in gameObject.transform)
            child.gameObject.layer = LayerMask.NameToLayer("gun");
    }

    public void Shot()
    {
        if (canFire)
        {
            
        }
        FireAnim();
    }
    public void Reload()
    {
        animator.SetTrigger(Reload1);
        currentAmmo = stat.maxAmmo;
    }

    public void Drop(Vector3 force)
    {
        Debug.Log(gameObject.name + " is drop");
        owner = null;
        gameObject.SetActive(true);
        //handleWeaponAnimator.SetTrigger(Drop1);
        isHolding = false;
        isOnInventory = false;
        col.enabled = true;
        animator.SetTrigger(Drop1);
        gameObject.transform.parent = null;
        gameObject.layer = LayerMask.NameToLayer("gun");
        foreach (Transform child in gameObject.transform)
            child.gameObject.layer = LayerMask.NameToLayer("gun");
        rb.isKinematic = false;
        rb.useGravity = true;
        interactable.isInteractable = true;
        rb.AddForce(force,ForceMode.Impulse);
    }

    public void Get(Player p)
    {
        gameObject.SetActive(false);
        interactable.isInteractable = false;
        col.enabled = false;
        isOnInventory = true;
        rb.useGravity = false;
        rb.isKinematic = true;
        p.GetWeapon(this);
        gameObject.layer = LayerMask.NameToLayer("gun_ui");
        foreach (Transform child in gameObject.transform)
            child.gameObject.layer = LayerMask.NameToLayer("gun_ui");
    }
    public void Hold()
    {
        //Debug.Log("Player hold the gun");
        transform.localPosition = stat.handlePosition;
        gameObject.SetActive(true); //HoldAnim();
        isHolding = true;
    }

    public void disHold()
    {
        gameObject.SetActive(false); //HoldAnim();
        isHolding = false;
    }

    public void OnTarget()
    {
        //Debug.Log("weapon targetted on " + gameObject.name);
        highlightMaterial.color = stat.interactHighlightColor;
    }

    public void DisTarget()
    {
        //Debug.Log("weapon disTargetted on " + gameObject.name);
        highlightMaterial.color = Color.clear;
    }
    public void HoldAnim()
    {
        animator.SetTrigger(Hold1);
        handleWeaponAnimator.SetTrigger(Hold1);
    }
    public void DownAnim()
    {
        animator.SetTrigger(Down);
    }
    public void UpAnim()
    {
        animator.SetTrigger(Up);
    }
    public void FireAnim()
    {
        animator.SetTrigger(Fire);
        handleWeaponAnimator.SetTrigger(Fire);
    }
}
