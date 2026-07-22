using Codes.Util;
using UnityEngine;

namespace Codes.InGame
{
    [RequireComponent(typeof(SynchronizedObject))]
    public abstract class Mover : ServerComponent
    {
        // ===== 서버 위치 동기화 (ObjectMove) =====
        public const float syncMoveSpeed = 20f;    // m/s, 오브젝트 최고 속도보다 커야 수렴
        public const float syncRotSpeed = 720f;    // deg/s
        private Vector3 _targetPos;
        private Vector3 _targetRot;
        private bool _serverDriven;
        [HideInInspector]public Rigidbody rb;
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.isKinematic = true;
        }
        
        public void BeginServerDriven()
        {
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
            _targetPos = transform.position;
            _targetRot = transform.rotation.eulerAngles;
            _serverDriven = true;
        }
        public void EndServerDriven()
        {
            _serverDriven = false;
        }
        
        public void ApplyServerMove(Vector3 pos, Vector3 rotEuler)
        {
            _targetPos = pos;
            _targetRot = rotEuler;
        }
        
        private void LateUpdate()
        {
            if (!_serverDriven) return;
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, syncMoveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(_targetRot), syncRotSpeed * Time.deltaTime);
        }
        
    }
}
