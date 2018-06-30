using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GameManager = MyGame.GameManagement.GameManager;
using WeaponProperties = MyGame.Inventory.Weapon.WeaponProperties;

namespace MyGame.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        /*
         * Player Animation
         */

        #region Statics Variables

        static int SpeedXID = Animator.StringToHash("SpeedX");
        static int SpeedYID = Animator.StringToHash("SpeedY");
        static int CrouchID = Animator.StringToHash("Crouch");
        static int SprintID = Animator.StringToHash("Sprint");
        static int WeaponGroupID = Animator.StringToHash("Weapon Group");
        static int WeaponTypeID = Animator.StringToHash("Weapon Type");
        static int ReloadID = Animator.StringToHash("Reload");
        static int AttackID = Animator.StringToHash("Attack");
        static int AimID = Animator.StringToHash("Aim");

        static float WaistMakeUp = 40f;
        static float objectPushForce = 2f;
        static float LowerCameraSpeed = 5f;
        static float GravityMultiplier = 2f;

        #endregion

        public UnityEvent OnAnimationRenderingOver;

        /* Camera */
        [SerializeField]
        Transform cameraPivot;

        /* Posture & Weapon */
        [SerializeField]
        PartsToFollowAim partsToFollowAim;

        /* Camera Movement */
        [SerializeField]
        PivotPositions pivotPositions;


        /* Player Movement */
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

        /* Animation Behaviour */
        float horizontalAxis = 0f;
        float verticalAxis = 0f;
        float jogClamp = 0.5f;

        bool inIdleAnimation = false;



        #region Properties

        public Transform CameraPivot
        {
            get
            {
                return cameraPivot;
            }
        }

        #endregion

        /* Others */
        #region Awake to Updates

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerAudio = GetComponent<PlayerAudioPlayer>();
            charController = GetComponent<CharacterController>();
            playerStatus = GetComponent<Player>().PlayerStatus;

            weaponSlot = GetComponentInChildren<PlayerWeaponSlot>();
            partsToFollowAim.weaponSlot = weaponSlot.transform;
        }

        void Start()
        {
            // Set Aim Focus Points
            partsToFollowAim.weaponFocus = GameManager.Instance.PlayerCamera.WeaponFocus;
            partsToFollowAim.bodyFocus = GameManager.Instance.PlayerCamera.BodyFocus;

            // Set the Initial Camera Pivot Follow Position
            playerCamera = GameManager.Instance.PlayerCamera;
            cameraPivot.localPosition = pivotPositions.walkPosition;
        }

        void Update()
        {
            horizontalAxis = Mathf.Clamp(Input.GetAxis("Horizontal"), 
                                        (playerStatus.lowerBody.isJogSet && !playerStatus.upperBody.isAiming) ? -1.0f : -0.5f,
                                        (playerStatus.lowerBody.isJogSet && !playerStatus.upperBody.isAiming) ? 1.0f : 0.5f);

            verticalAxis = Mathf.Clamp(Input.GetAxis("Vertical"), 
                                      (playerStatus.lowerBody.isJogSet && !playerStatus.upperBody.isAiming) ? -1.0f : -0.5f, 
                                      (playerStatus.lowerBody.isJogSet && !playerStatus.upperBody.isAiming) ? 1.0f : 0.5f);

            if (playerStatus.lowerBody.isSprinting)
                horizontalAxis = 0f;

            animator.SetFloat(SpeedXID, horizontalAxis);
            animator.SetFloat(SpeedYID, verticalAxis);

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

        void AdjustPosture()
        {

            Vector3 relVec;
            Quaternion lookRotation;

            /* Waist */
            if (!playerStatus.lowerBody.isSprinting)
            {
                relVec = partsToFollowAim.bodyFocus.position - partsToFollowAim.spine.position;
                lookRotation = Quaternion.LookRotation(relVec);
                partsToFollowAim.spine.rotation = lookRotation;
                partsToFollowAim.spine.Rotate(transform.up, WaistMakeUp);
            }
            else
            {
                partsToFollowAim.spine.Rotate(partsToFollowAim.spine.position + transform.right, 35f);
                partsToFollowAim.spine.Rotate(partsToFollowAim.spine.position + transform.up, -20f);
            }

            /* Head */
            relVec = partsToFollowAim.bodyFocus.position - partsToFollowAim.head.position;
            lookRotation = Quaternion.LookRotation(relVec); 
            partsToFollowAim.head.rotation = lookRotation;
        }

        void AdjustGun()
        {
            if (!playerStatus.lowerBody.isSprinting && 
                (playerStatus.upperBody.isPrimaryWeaponOut && inIdleAnimation )
                || (playerStatus.upperBody.isSecondaryWeaponOut && playerStatus.upperBody.isAiming))
            {
                Vector3 relVec = partsToFollowAim.weaponFocus.position - partsToFollowAim.weaponSlot.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                partsToFollowAim.weaponSlot.rotation = lookRotation;
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
            Vector3 move_dir = transform.forward * verticalAxis + transform.right * horizontalAxis;

            /* Find Surface and Change Footstep */

            RaycastHit sphereHit;

            if (Physics.SphereCast(transform.position, charController.radius, Vector3.down,
                out sphereHit, charController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(transform.position + charController.center, sphereHit.point, Color.blue);
            }

            move_dir = Vector3.ProjectOnPlane(move_dir, sphereHit.normal) * (playerStatus.lowerBody.isSprinting ? sprintSpeed : playerStatus.lowerBody.isCrouching ? crouchSpeed : baseSpeed) * Time.deltaTime;

            if (!charController.isGrounded)
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

        public void WalkAnimation()
        {
            animator.SetBool(CrouchID, false);
            animator.SetBool(SprintID, false);

            LowerCameraPivot(1);
        }

        public void CrouchAnimation()
        {
            animator.SetBool(CrouchID, true);
            animator.SetBool(SprintID, false);

            LowerCameraPivot(2);
        }

        public void SprintAnimation()
        {
            animator.SetBool(CrouchID, false);
            animator.SetBool(SprintID, true);

            LowerCameraPivot(3);
        }

        public void HolsterAnimation()
        {
            inIdleAnimation = false;
            animator.SetInteger(WeaponGroupID, 0);
            animator.SetInteger(WeaponTypeID, 0);
        }

        public void DrawAnimation(WeaponProperties properties)
        {
            inIdleAnimation = false;
            animator.SetInteger(WeaponGroupID, (int) properties.group);
            animator.SetInteger(WeaponTypeID, (int) properties.type);
        }

        public void ShowWeapon()
        {
            weaponSlot.ShowWeapon();
        }

        public void HideWeapon()
        {
            weaponSlot.HideWeapon();
        }

        public void IdleAnimation()
        {
            inIdleAnimation = true;
            weaponSlot.ReleaseFireLock();
        }

        public void AttackAnimation()
        {
            inIdleAnimation = false;
            animator.SetTrigger(AttackID);

            weaponSlot.ToggleAttack();
        }

        public void ReloadAnimation()
        {
            inIdleAnimation = false;
            animator.SetTrigger(ReloadID);
        }

        public void AimAnimation(bool toggle)
        {
            animator.SetBool(AimID, toggle);
        }

        public void ToggleJog(bool toggle)
        {

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
        private class PartsToFollowAim
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
    }


}

