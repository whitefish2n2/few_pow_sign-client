using System;
using Plugins;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codes.InGame.Player_Ingame
{
    public class IngameInputDispatcher : MonoSingleton<IngameInputDispatcher>
    {
        [SerializeField] private InputActionAsset inputAsset;

        // 연속값 — raw payload만 던짐, 가공(누적·클램프)은 구독자 몫
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnMouseDelta;
        public event Action<Vector2> OnScroll;

        // 버튼 — performed(press) 시점
        public event Action OnJump;
        public event Action OnLeftClick;
        public event Action OnRightClick;
        public event Action OnThrow;
        public event Action OnReload;
        public event Action OnInteract;
        
        public bool GetIsAttackPressed() => _leftClick.IsPressed(); 
        

        private InputActionMap _map;
        private InputAction _move, _mouse, _scroll, _jump, _leftClick, _rightClick, _throw, _reload, _interact;

        protected override void Initialize()
        {
            _map        = inputAsset.FindActionMap("Player_Ingame", true);
            _move       = _map.FindAction("Move", true);
            _mouse      = _map.FindAction("Mouse", true);
            _scroll     = _map.FindAction("Scroll", true);
            _jump       = _map.FindAction("Jump", true);
            _leftClick  = _map.FindAction("Left Click", true);
            _rightClick = _map.FindAction("Right Click", true);
            _throw      = _map.FindAction("ThrowWeapon", true);
            _reload     = _map.FindAction("ReloadWeapon", true);
            _interact   = _map.FindAction("Interaction", true);

            _move.performed       += Move_Changed;
            _move.canceled        += Move_Changed;      // 키 떼면 0으로 리셋
            _mouse.performed      += Mouse_Changed;
            _mouse.canceled       += Mouse_Changed;
            _scroll.performed     += Scroll_Changed;
            _scroll.canceled      += Scroll_Changed;
            _jump.performed       += Jump_Performed;
            _leftClick.performed  += LeftClick_Performed;
            _rightClick.performed += RightClick_Performed;
            _throw.performed      += Throw_Performed;
            _reload.performed     += Reload_Performed;
            _interact.performed   += Interact_Performed;
            Enable();
        }

        // 인게임 진입/이탈 경계에서 호출 (DontDestroyOnLoad라 메뉴에서 입력 먹지 않게 게이트)
        public void Enable()  => _map?.Enable();
        public void Disable() => _map?.Disable();

        private void Move_Changed(InputAction.CallbackContext c)        => OnMove?.Invoke(c.ReadValue<Vector2>());
        private void Mouse_Changed(InputAction.CallbackContext c)       => OnMouseDelta?.Invoke(c.ReadValue<Vector2>());
        private void Scroll_Changed(InputAction.CallbackContext c)      => OnScroll?.Invoke(c.ReadValue<Vector2>());
        private void Jump_Performed(InputAction.CallbackContext c)      => OnJump?.Invoke();
        private void LeftClick_Performed(InputAction.CallbackContext c)  => OnLeftClick?.Invoke();
        private void RightClick_Performed(InputAction.CallbackContext c) => OnRightClick?.Invoke();
        private void Throw_Performed(InputAction.CallbackContext c)     => OnThrow?.Invoke();
        private void Reload_Performed(InputAction.CallbackContext c)    => OnReload?.Invoke();
        private void Interact_Performed(InputAction.CallbackContext c)  => OnInteract?.Invoke();

        protected override void OnDestroy()
        {
            if (_map != null)
            {
                _move.performed       -= Move_Changed;
                _move.canceled        -= Move_Changed;
                _mouse.performed      -= Mouse_Changed;
                _mouse.canceled       -= Mouse_Changed;
                _scroll.performed     -= Scroll_Changed;
                _scroll.canceled      -= Scroll_Changed;
                _jump.performed       -= Jump_Performed;
                _leftClick.performed  -= LeftClick_Performed;
                _rightClick.performed -= RightClick_Performed;
                _throw.performed      -= Throw_Performed;
                _reload.performed     -= Reload_Performed;
                _interact.performed   -= Interact_Performed;
                _map.Disable();
            }
            base.OnDestroy();
        }
    }
}