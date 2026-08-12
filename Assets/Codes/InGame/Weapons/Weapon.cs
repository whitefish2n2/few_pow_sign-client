using System.Collections;
using System.Text;
using Codes.InGame;
using UnityEngine;

namespace Codes.InGame.Weapons
{
    public class Weapon : Mover
    {
        private static readonly int Fire = Animator.StringToHash("Fire");
        private static readonly int Reload1 = Animator.StringToHash("Reload");
        private static readonly int Drop1 = Animator.StringToHash("Drop");
        private static readonly int Down = Animator.StringToHash("Down");
        private static readonly int Up = Animator.StringToHash("Up");
        private static readonly int Hold1 = Animator.StringToHash("Hold");
        private static readonly int Init1 = Animator.StringToHash("Init");

        public PlayerBehaviour owner;
        public WeaponStat stat;
        public Animator animator;
        private float lastShotTime = -999f;
        public bool CanShoot() => Time.time - lastShotTime >= stat.termToShot;
        public void RegisterShot() => lastShotTime = Time.time;

        [SerializeField]public GameObject bulletTrailPrefab;
        [SerializeField]public Transform muzzlePoint;
        [SerializeField]public float trailSpeed = 300f;

        public int currentAmmo;
        public bool isHolding;
        public bool isOnInventory;
        private Collider col;
        [HideInInspector] public GameObject handleWeaponInstance;
        private Animator handleWeaponAnimator;
    
        //interact component
        private Interactable interactable;
        private Material highlightMaterial;

        protected override void Awake()
        {
            base.Awake();
            col = GetComponent<Collider>();
            animator = GetComponent<Animator>();
            interactable = GetComponent<Interactable>();
            interactable.onInteract.AddListener(Get);
            interactable.onTarget.AddListener(OnTarget);
            interactable.onDisTarget.AddListener(DisTarget);
            highlightMaterial = GetComponent<Renderer>().material;
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
            foreach (Transform child in gameObject.transform)
                child.gameObject.layer = LayerMask.NameToLayer("gun");
        }

    
    
        public void Shot(Vector3 reachPosition)
        {
            FireAnim();

            if (SoundManager.IsInitialized) SoundManager.Instance.PlaySoundAtPosition(stat.shotSound, transform.position);

            if (bulletTrailPrefab != null && muzzlePoint != null)
            {
                var trail = Instantiate(bulletTrailPrefab, muzzlePoint.position, Quaternion.identity);
                StartCoroutine(FlyTrail(trail, reachPosition));
            }
        }

        private IEnumerator FlyTrail(GameObject trail, Vector3 target)
        {
            Vector3 start = trail.transform.position;
            float distance = Vector3.Distance(start, target);
            float duration = distance / trailSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                trail.transform.position = Vector3.Lerp(start, target, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            trail.transform.position = target;
            Destroy(trail);
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
            //rb.isKinematic = false;
            rb.useGravity = true;
            interactable.isInteractable = true;
            rb.AddForce(force,ForceMode.Impulse);
        }

        public void Get(PlayerBehaviour p)
        {
            ApplyPickupState();
            p.GetWeapon(this);
        }

        /// 인벤토리 편입 상태 전환 (서버 notify 미러 경로에서도 사용)
        public void ApplyPickupState()
        {
            EndServerDriven(); 
            gameObject.SetActive(false);
            interactable.isInteractable = false;
            col.enabled = false;
            isOnInventory = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            // 레이어(gun/gun_ui) 지정은 여기서 하지 않음 — 이 무기를 든 게 "나"인지 "남"인지에 따라 달라져야 하는데
            // ApplyPickupState는 누가 주웠는지 모름. WeaponSystem.ApplyPickup / HandledPlayerWeaponSystem.GetWeapon에서 지정.
        }
        public void Hold()
        {
            transform.localPosition = stat.handlePosition;
            transform.localRotation = Quaternion.Euler(stat.handleObjectRotation);
            gameObject.SetActive(true);
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
            if (handleWeaponAnimator != null) handleWeaponAnimator.SetTrigger(Hold1);
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
            if (handleWeaponAnimator != null) handleWeaponAnimator.SetTrigger(Fire);
        }

        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"WeaponName:{stat.weaponName.ToString()}");
            sb.AppendLine($"WeaponType:{stat.type.ToString()}");    
            sb.AppendLine($"MaxAmmo:{stat.maxAmmo}");
            sb.AppendLine($"CurrentAmmo:{currentAmmo}");
            sb.AppendLine($"HeadDamage:{stat.headDamage}");
            sb.AppendLine($"BodyDamage:{stat.bodyDamage}");
            sb.AppendLine($"TermToShot:{stat.termToShot}");
            sb.AppendLine($"WeaponId:{(int)stat.weaponName}");

            return sb.ToString();
        }
    }
}
