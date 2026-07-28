using System;
using System.Collections;
using System.Text;
using Codes.InGame;
using Codes.InGame.Player_Ingame;
using Codes.InGame.Weapons;
using Codes.Util;
using NetCode.ENetCode;
using NetTest;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveSystem : Mover
{
    [Header("카메라")]
    public float mouseSpeed = 3;
    private float _xRot;
    private float _yRot;
    private Vector2 _mouseDelta;
    public Camera cam;
    [Header("플레이어 움직임")]
    [HideInInspector] public Vector3 moveVector =  Vector3.zero;
    private Vector2 _inputVector = Vector2.zero;
    [HideInInspector] public Vector3 currentVelocity = Vector3.zero;
    PlayerComponent playerComponent;

    [SerializeField] private bool isOnGround = true;
    public bool isCanJump = true;
    private LayerMask _groundMask;


    public void Init()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        mouseSpeed = 3;
        rb.isKinematic = false;
        _groundMask = LayerMask.GetMask("Ground");
        playerComponent = GetComponent<PlayerComponent>();
        cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(transform);
            cam.transform.localPosition = playerComponent.aimOrigin;
            cam.transform.localRotation = Quaternion.identity;
            
            var hand = new GameObject("Hand").transform;
            hand.SetParent(cam.transform);
            hand.localPosition = Vector3.zero;
            hand.localRotation = Quaternion.identity;
            GetComponent<WeaponSystem>()?.SetWeaponHolder(hand.gameObject);
            
        }

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IngameInputDispatcher.Instance.OnMove += Move;
        IngameInputDispatcher.Instance.OnMouseDelta += MouseMove;
        IngameInputDispatcher.Instance.OnJump += Jump;
    }
    
    private bool _flag;
    private void FixedUpdate()
    {
        isOnGround = Physics.CheckSphere(transform.position-new Vector3(0,playerComponent.onGroundYDistance), playerComponent.onGroundRadius,_groundMask); 
        Vector3 targetVelocity = moveVector * playerComponent.maxSpeed;
        moveVector = transform.forward * _inputVector.y + transform.right * _inputVector.x;
        if (isOnGround)
        {
            if (moveVector.magnitude > 0.1)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity,  playerComponent.acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, playerComponent.deceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            targetVelocity = moveVector * currentVelocity.magnitude;
            if (moveVector.magnitude > 0.1)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, playerComponent.acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, playerComponent.deceleration * Time.fixedDeltaTime);
            }
        }
        EnetClient.Instance.SendMovePacket(new MoveRequestDto
        {
            SessionKey  = MatchMakeStatic.Instance.dedicatedServerIndex,
            Timestamp   = 0,
            InputVector = _inputVector,
            inputYaw    = _xRot,   // 바디 yaw
            inputPitch  = _yRot,   // 카메라 pitch
        });
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    private void Update()
    {
        Rotate();
    }

    public void Move(Vector2 input)
    {
        _inputVector = input;
    }
    
    private void Jump()
    {
        if (isOnGround && isCanJump)
        {
            rb.AddForce(Vector3.up*playerComponent.jumpPower, ForceMode.Impulse);
            GameUtil.instance.CoolBool(0.1f, a=>isCanJump = a,true);
            EnetClient.Instance.SendJumpPacket();
        }
    }

    
    void Rotate()
    {
        _yRot -= _mouseDelta.y * mouseSpeed * Time.deltaTime;
        _xRot += _mouseDelta.x * mouseSpeed * Time.deltaTime;
        _yRot = Mathf.Clamp(_yRot, -90f, 90f);
        cam.transform.rotation = Quaternion.Euler(_yRot,_xRot , 0);
        rb.rotation = Quaternion.Euler(0, _xRot, 0);
    }
    public void MouseMove(Vector2 delta)
    {
        _mouseDelta = delta;
    }

    public override string Serialize()
    {
        StringBuilder sb = new StringBuilder();

        return sb.ToString();
    }
    
    private void OnDestroy()
    {
        if (IngameInputDispatcher.TryGetInstance(out var dispatcher))
        {
            dispatcher.OnMove -= Move;
            dispatcher.OnMouseDelta -= MouseMove;
            dispatcher.OnJump -= Jump;
        }
    }
}
