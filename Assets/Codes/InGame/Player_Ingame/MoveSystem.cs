using System;
using System.Collections;
using Codes.InGame;
using Codes.InGame.Player_Ingame;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveSystem : Mover
{
    [Header("카메라")]
    public float mouseSpeed;
    private float _xRot;
    private float _yRot;
    private Vector2 _mouseDelta;
    public Camera cam;
    [Header("플레이어 움직임")]
    [SerializeField] public float moveSpeed = 1;
    [SerializeField] public float jumpPower = 1;
    [HideInInspector] public Vector3 moveVector =  Vector3.zero;
    private Vector2 _inputVector = Vector2.zero;
    [HideInInspector] public Vector3 currentVelocity = Vector3.zero;
    public float maxSpeed;
    public float acceleration;
    public float deceleration;

    [SerializeField] private bool isOnGround = true;
    public bool isCanJump = true;
    private LayerMask _groundMask;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        _groundMask = LayerMask.GetMask("Ground");
        cam = Camera.main;
        Debug.LogError(cam);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private bool _flag;
    [Header("InOnGround 체크용 Raycast value")]
    [SerializeField] private float r;
    [SerializeField] private float d;
    private void FixedUpdate()
    {
        isOnGround = Physics.CheckSphere(transform.position-new Vector3(0,d), r,_groundMask); 
        Vector3 targetVelocity = moveVector * maxSpeed;
        moveVector = transform.forward * _inputVector.y + transform.right * _inputVector.x;
        if (isOnGround)
        {
            if (moveVector.magnitude > 0.1)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            targetVelocity = moveVector * currentVelocity.magnitude;
            if (moveVector.magnitude > 0.1)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            }
        }
        StaticInput.instance.rotEular = cam.transform.localRotation.eulerAngles;
        StaticInput.instance.inputVector = _inputVector;
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    private void Update()
    {
        Rotate();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOnGround && isCanJump)
            {
                rb.AddForce(Vector3.up*jumpPower, ForceMode.Impulse);
                GameUtil.instance.CoolBool(0.1f, a=>isCanJump = a,true);
            }
        }

        if (Input.GetKeyDown(KeyCode.WheelDown))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        //Debug.Log(context.phase);
        _inputVector =  context.ReadValue<Vector2>();
    }
    void Rotate()
    {
        _yRot -= _mouseDelta.y * mouseSpeed * Time.deltaTime;
        _xRot += _mouseDelta.x * mouseSpeed * Time.deltaTime;
        _yRot = Mathf.Clamp(_yRot, -90f, 90f);
        cam.transform.rotation = Quaternion.Euler(_yRot,_xRot , 0);
        rb.rotation = Quaternion.Euler(0, _xRot, 0);
    }
    public void MouseMove(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
    }
}
