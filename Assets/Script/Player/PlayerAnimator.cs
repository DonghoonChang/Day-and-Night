using UnityEngine;
using UnityEngine.Events;
using GameManager = MyGame.GameManagement.GameManager;
using WeaponProperties = MyGame.Inventory.Weapon.WeaponProperties;
using WeaponGroup = MyGame.Inventory.Weapon.WeaponGroup;
using WeaponType = MyGame.Inventory.Weapon.WeaponType;

namespace MyGame.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        #region Statics Variables

        static int SpeedXID = Animator.StringToHash("SpeedX");
        static int SpeedYID = Animator.StringToHash("SpeedY");
        static int CrouchID = Animator.StringToHash("Crouch");
        static int SprintID = Animator.StringToHash("Sprint");
        static int WeaponOutID = Animator.StringToHash("Weapon Out");
        static int WeaponGroupID = Animator.StringToHash("Weapon Group");
        static int WeaponTypeID = Animator.StringToHash("Weapon Type");
        static int AimID = Animator.StringToHash("Aim");
        static int AttackID = Animator.StringToHash("Attack");
        static int ReloadID = Animator.StringToHash("Reload");

        static float objectPushForce = 2f;
        static float GravityMultiplier = 10f;

        #endregion

        public UnityEvent OnAnimationRenderingOver;

        // Camera
        [SerializeField]
        Transform cameraPivot;

        [SerializeField]
        PivotPositions pivotPositions;

        // Posture
        [SerializeField]
        PartsLookAtAim partsLookAtAim;

        [SerializeField]
        PostureAdjustment postureAdjustment;

        Vector3 currentSpineAdjustment;

        // Movement Speed
        [SerializeField]
        [Range(0.1f, 5f)]
        float baseSpeed;

        [SerializeField]
        [Range(1f, 5f)]
        float sprintSpeed;

        [SerializeField]
        [Range(0.1f, 5f)]
        float crouchSpeed;
        
        /* Components */
        Animator animator;
        PlayerAudioPlayer playerAudio;
        CharacterController charController;

        PlayerStatus playerStatus;
        PlayerCamera playerCamera;
        PlayerWeaponSlot weaponSlot;

        // Animation Status Tracking
        public bool isWeaponOut = false; // Weapon Drawn Out (Firing or Not)
        public bool isWeaponFiring = false; // Weapon in Firing Animation (Different from Fire Locked)
        public bool isWeaponReloading = false; // Weapon Ready to Fire

        #region Properties

        #endregion

        #region Awake to Updates

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerAudio = GetComponent<PlayerAudioPlayer>();
            charController = GetComponent<CharacterController>();
            playerStatus = GetComponent<PlayerCharacter>().PlayerStatus;
            weaponSlot = GetComponentInChildren<PlayerWeaponSlot>();

            partsLookAtAim.weaponSlot = weaponSlot.transform;
            currentSpineAdjustment = postureAdjustment.spineHipAdjustment;

            weaponSlot.OnWeaponAnimationChanged.AddListener(ChangeWeaponAnimationGroup);
        }

        void Start()
        {
            // Set Aim Focus Points
            partsLookAtAim.weaponFocus = GameManager.Instance.PlayerCamera.AimPoint;
            partsLookAtAim.bodyFocus = GameManager.Instance.PlayerCamera.BodyOrientation;

            // Set the Initial Camera Pivot Follow Position
            playerCamera = GameManager.Instance.PlayerCamera;
            cameraPivot.localPosition = pivotPositions.walkPosition;
        }

        void Update()
        {
            animator.SetFloat(SpeedXID, playerStatus.horizontalAxis);
            animator.SetFloat(SpeedYID, playerStatus.verticalAxis);

            MovePlayer();
            RotateBodytoCamera();
        }

        void LateUpdate()
        {
            AdjustPosture();
            AdjustGun();

            if (OnAnimationRenderingOver != null)
            {
                OnAnimationRenderingOver.Invoke();
                OnAnimationRenderingOver.RemoveAllListeners();
            }
        }

        #endregion

        #region Player Movement and Animation Adjustment

        private void OnAnimatorIK(int layerIndex)
        {
            if (!playerStatus.isSprinting)
            {
                animator.SetLookAtWeight(1, 1, 1, 0, 1f);
                animator.SetLookAtPosition(partsLookAtAim.bodyFocus.position);
            }
        }

        private void AdjustPosture()
        {
            partsLookAtAim.spine.Rotate(currentSpineAdjustment, Space.Self);

            // Waist Sprinting
            if (!playerStatus.isSprinting)
            {
                if (playerStatus.isAiming)
                    currentSpineAdjustment = Vector3.Lerp(currentSpineAdjustment, postureAdjustment.spineAimAdjustment, 10f * GameTime.deltaTime);

                else
                    currentSpineAdjustment = Vector3.Lerp(currentSpineAdjustment, postureAdjustment.spineHipAdjustment, 10f * GameTime.deltaTime);
            }

            // Wasit Sprinting
            else
                partsLookAtAim.spine.Rotate(postureAdjustment.spineSprintAdjustment, Space.Self);

            // Head
            partsLookAtAim.head.Rotate(postureAdjustment.headAimAdjustment, Space.Self);
        }

        private void AdjustGun()
        {
            if (!playerStatus.isSprinting && isWeaponOut && !isWeaponFiring && !isWeaponReloading )
            {
                Vector3 relVec = partsLookAtAim.bodyFocus.position - partsLookAtAim.weaponSlot.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                partsLookAtAim.weaponSlot.rotation = lookRotation;
            }
        }


        /* Make Player Face the Camera Direction AROUND y-axis */
        void RotateBodytoCamera()
        {
            transform.eulerAngles = new Vector3(0, playerCamera.Yaw, 0);
        }

        /* Character Movement */
        void MovePlayer()
        {
            Vector3 move_dir = transform.forward * playerStatus.verticalAxis + transform.right * playerStatus.horizontalAxis;

            /* Find Surface and Change Footstep */

            RaycastHit sphereHit;

            if (Physics.SphereCast(transform.position, charController.radius, Vector3.down,
                out sphereHit, charController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(transform.position + charController.center, sphereHit.point, Color.blue);
            }

            move_dir = Vector3.ProjectOnPlane(move_dir, sphereHit.normal) * (playerStatus.isSprinting ? sprintSpeed : playerStatus.isCrouching ? crouchSpeed : baseSpeed) * GameTime.deltaTime;
            move_dir += Physics.gravity * GravityMultiplier;

            charController.Move(move_dir);
        }

        void LowerCameraPivot(int location)
        {
            if (location == 1)
                cameraPivot.localPosition = pivotPositions.walkPosition;

            else if (location == 2)
                cameraPivot.localPosition = pivotPositions.crouchPosition;

            else if (location == 3)
                cameraPivot.localPosition = pivotPositions.sprintPosition;
        }

        #endregion

        #region Animations

        public void Walk()
        {
            animator.SetBool(CrouchID, false);
            animator.SetBool(SprintID, false);

            LowerCameraPivot(1);
        }

        public void Crouch()
        {
            animator.SetBool(CrouchID, true);
            animator.SetBool(SprintID, false);

            LowerCameraPivot(2);
        }

        public void Sprint()
        {
            animator.SetBool(CrouchID, false);
            animator.SetBool(SprintID, true);

            LowerCameraPivot(3);
        }

        public void DrawWeapon()
        {
            isWeaponFiring = false;
            isWeaponReloading = false;
            animator.SetBool(WeaponOutID, true);
        }

        public void HolsterWeapon()
        {
            isWeaponOut = false;
            isWeaponFiring = false;
            isWeaponReloading = false;
            animator.SetBool(WeaponOutID, false);
        }

        public void ShowWeapon()
        {
            weaponSlot.ShowWeapon();
        }

        public void HideWeapon()
        {
            weaponSlot.HideWeapon();
        }

        public void SetWeaponIdle()
        {
            isWeaponOut = true;
            isWeaponFiring = false;
            isWeaponReloading = false;
            weaponSlot.ReleaseAttackLock();
        }

        public void Attack()
        {
            weaponSlot.Attack();
            isWeaponFiring = true;
            animator.SetTrigger(AttackID);
        }

        public void Reload()
        {
            isWeaponReloading = true;
            animator.SetTrigger(ReloadID);
        }

        public void AimAnimation(bool toggle)
        {
            animator.SetBool(AimID, toggle);
        }

        public void ChangeWeaponAnimationGroup(WeaponGroup group, WeaponType type)
        {
            animator.SetInteger(WeaponGroupID, (int) group);
            animator.SetInteger(WeaponTypeID, (int) type);
        }

        #endregion

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.rigidbody != null)
            {
                Rigidbody rb = hit.rigidbody;
                rb.AddForce((hit.transform.position - transform.position).normalized * objectPushForce, ForceMode.Force);
            }
        }

        [System.Serializable]
        private class PartsLookAtAim
        {
            public Transform head;
            public Transform spine;
            public Transform weaponSlot;
            public Transform bodyFocus;
            public Transform weaponFocus;
        }

        [System.Serializable]
        private class PivotPositions
        {
            public Vector3 walkPosition = new Vector3(0.5f, 1.8f, 0f);
            public Vector3 crouchPosition = new Vector3(0.5f, 1.35f, 0f);
            public Vector3 sprintPosition = new Vector3(0.5f, 1.6f, 0f);
        }

        [System.Serializable]
        private class PostureAdjustment
        {
            public Vector3 spineHipAdjustment = new Vector3(-37.5f, 0f, 0f);
            public Vector3 spineAimAdjustment = new Vector3(-37.5f, 0f, -15f);
            public Vector3 headAimAdjustment = new Vector3(0f, 0f, -30f);
            public Vector3 spineSprintAdjustment = new Vector3(7.5f, 0f, -40f);
        }
    }
}

