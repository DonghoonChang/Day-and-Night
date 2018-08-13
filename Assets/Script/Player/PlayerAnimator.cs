using UnityEngine;
using UnityEngine.Events;
using CameraManager = MyGame.GameManagement.CameraManager;
using WeaponGroup = MyGame.Object.WeaponGroup;
using WeaponType = MyGame.Object.WeaponType;
using EnemyCharacter = MyGame.Enemy.EnemyCharacter;

namespace MyGame.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        static int PushCount = 30;

        #region Statics Variables

        static int AimID = Animator.StringToHash("Aim");
        static int AttackID = Animator.StringToHash("Attack");
        static int ReloadID = Animator.StringToHash("Reload");
        static int DodgeAID = Animator.StringToHash("Dodge A");
        static int DodgeBID = Animator.StringToHash("Dodge B");
        static int DodgeCID = Animator.StringToHash("Dodge C");

        static int SpeedXID = Animator.StringToHash("SpeedX");
        static int SpeedYID = Animator.StringToHash("SpeedY");
        static int CrouchID = Animator.StringToHash("Crouch");
        static int SprintID = Animator.StringToHash("Sprint");

        static int GrenadeReadyID = Animator.StringToHash("Grenade Ready");
        static int GrenadeThrowID = Animator.StringToHash("Grenade Throw");

        static int WeaponOutID = Animator.StringToHash("Weapon Out");
        static int WeaponGroupID = Animator.StringToHash("Weapon Group");
        static int WeaponTypeID = Animator.StringToHash("Weapon Type");

        static float PlayerVerticalSpeed = 1f;
        static float PlayerHorizontalSpeed = 0.75f;

        static float EnemyPlayerPushForce = 0.00025f;

        #endregion

        public UnityEvent OnPostureAdjustmentOver;

        // Components
        Animator _animator;
        CharacterController _charController;
        Rigidbody[] _ragdoll;

        PlayerStatus _playerStatus;
        PlayerCamera _playerCamera;
        PlayerAudioPlayer _playerAudio;

        PlayerWeaponSlot _weaponSlot;
        PlayerGrenadeSlot _grenadeSlot;

        // Camera
        [SerializeField] Transform cameraPivot;

        // Animation and Movement
        [SerializeField] MovementSpeeds _movementSpeeds = new MovementSpeeds();
        [SerializeField] CameraPositions _camPositions = new CameraPositions();
        [SerializeField] PostureAdjustmentParts _adjustmentParts = new PostureAdjustmentParts();
        [SerializeField] PostureAdjustment _postureAdjustment = new PostureAdjustment();

        Vector3 _currentSpineAdjustment = new Vector3(0, 0, 0);
        public PlayerAnimationStatus animationStatus = new PlayerAnimationStatus();

        #region Awake to Updates

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerAudio = GetComponent<PlayerAudioPlayer>();
            _charController = GetComponent<CharacterController>();
            _ragdoll = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in _ragdoll)
                rb.isKinematic = true;

            _weaponSlot = GetComponentInChildren<PlayerWeaponSlot>();
            _grenadeSlot = GetComponentInChildren<PlayerGrenadeSlot>();
            _playerStatus = GetComponent<PlayerCharacter>().PlayerStatus;

            _adjustmentParts.weaponSlot = _weaponSlot.transform;
            _currentSpineAdjustment = _postureAdjustment.spineHipAdjustment;

            _weaponSlot.OnWeaponAnimationChanged.AddListener(ChangeWeaponAnimationGroup);

            animationStatus.isHoldingNothing = true;
        }

        void Start()
        {
            // Set Aim Focus Points
            _playerCamera = CameraManager.Instance.PlayerCamera;
            _adjustmentParts.raycastPoint = CameraManager.Instance.PlayerCamera.AimPoint;
            _adjustmentParts.mecanimLookAtPoint = CameraManager.Instance.PlayerCamera.MecanimLookAtPoint;

            // Set the Initial Camera Pivot Follow Position
            cameraPivot.localPosition = _camPositions.walkPosition;
        }

        void Update()
        {
            if (!_playerStatus.isDead)
            {
                _animator.SetFloat(SpeedXID, _playerStatus.horizontalAxis);
                _animator.SetFloat(SpeedYID, _playerStatus.verticalAxis);

                MovePlayer();
                RotateBodytoCamera();
            }
        }

        void LateUpdate()
        {
            if (!_playerStatus.isDead)
            {
                AdjustPosture();
                AdjustFlashlight();
                AdjustWeaponSlot();

                if (OnPostureAdjustmentOver != null)
                {
                    OnPostureAdjustmentOver.Invoke();
                    OnPostureAdjustmentOver.RemoveAllListeners();
                }
            }
        }

        #endregion

        #region Player Movement and Animation Adjustment

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_playerStatus.isSprinting)
            {
                _animator.SetLookAtWeight(1f, 1f, 1f, 0, 1f);
                _animator.SetLookAtPosition(_adjustmentParts.mecanimLookAtPoint.position);
            }
        }

        private void AdjustPosture()
        {
            _adjustmentParts.spine.Rotate(_currentSpineAdjustment, Space.Self);

            // Waist Adjustment Not Sprinting
            if (!_playerStatus.isSprinting)
            {
                if (_playerStatus.isAiming)
                    _currentSpineAdjustment = Vector3.Lerp(_currentSpineAdjustment, _postureAdjustment.spineAimAdjustment, 10f * GameTime.deltaTime);

                else if (animationStatus.isHoldingGrenade)
                    _currentSpineAdjustment = Vector3.Lerp(_currentSpineAdjustment, _postureAdjustment.spineGrenadeAdjustment, 10f * GameTime.deltaTime);

                else
                    _currentSpineAdjustment = Vector3.Lerp(_currentSpineAdjustment, _postureAdjustment.spineHipAdjustment, 10f * GameTime.deltaTime);
            }

            // Waist Adjustment Sprinting
            else
                _adjustmentParts.spine.Rotate(_postureAdjustment.spineSprintAdjustment, Space.Self);

            // Head
            _adjustmentParts.head.Rotate(_postureAdjustment.headAimAdjustment, Space.Self);
        }

        private void AdjustWeaponSlot()
        {
            if (!_playerStatus.isSprinting 
                && (animationStatus.isHoldingRangedWeaponHips || animationStatus.isHoldingRangedWeaponIronsight)
                && !animationStatus.inAttackAnimation && !animationStatus.inReloadAnimation )
            {
                Vector3 relVec = _adjustmentParts.mecanimLookAtPoint.position - _adjustmentParts.weaponSlot.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                _adjustmentParts.weaponSlot.rotation = lookRotation;
            }
        }

        private void AdjustFlashlight()
        {
            Vector3 relVec = _adjustmentParts.mecanimLookAtPoint.position - _adjustmentParts.flashlight.position;
            Quaternion lookRotation = Quaternion.LookRotation(relVec);
            _adjustmentParts.flashlight.rotation = lookRotation;
        }


        /* Character Movement */
        void MovePlayer()
        {
            Vector3 move_dir = transform.forward * _playerStatus.verticalAxis * PlayerVerticalSpeed + transform.right * _playerStatus.horizontalAxis * PlayerHorizontalSpeed;

            /* Find Surface and Change Footstep */

            RaycastHit sphereHit;

            if (Physics.SphereCast(transform.position, _charController.radius, Vector3.down,
                out sphereHit, _charController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(transform.position + _charController.center, sphereHit.point, Color.blue);
            }

            move_dir = Vector3.ProjectOnPlane(move_dir, sphereHit.normal)
                        * (_playerStatus.isSprinting ? _movementSpeeds.sprintSpeed : _playerStatus.isCrouching ? _movementSpeeds.crouchSpeed : _movementSpeeds.baseSpeed)
                        * GameTime.deltaTime;

            if (!_charController.isGrounded)
                move_dir.y = Physics.gravity.y;

            if (animationStatus.isBeingPushed >= 0)
            {
                move_dir.x += animationStatus.isBeingPushed * animationStatus.pushVector.x;
                move_dir.z += animationStatus.isBeingPushed * animationStatus.pushVector.z;

                animationStatus.isBeingPushed--;
            }

            _charController.Move(move_dir);
        }

        void RotateBodytoCamera()
        {
            transform.eulerAngles = new Vector3(0, _playerCamera.Yaw, 0);
        }

        void ChangeCameraPivotPosition(int location)
        {
            if (location == 1)
                cameraPivot.localPosition = _camPositions.walkPosition;

            else if (location == 2)
                cameraPivot.localPosition = _camPositions.crouchPosition;

            else if (location == 3)
                cameraPivot.localPosition = _camPositions.sprintPosition;
        }

        #endregion

        #region Animations

        public void WalkAnimation()
        {
            _animator.SetBool(CrouchID, false);
            _animator.SetBool(SprintID, false);

            ChangeCameraPivotPosition(1);
        }

        public void CrouchAnimation()
        {
            _animator.SetBool(CrouchID, true);
            _animator.SetBool(SprintID, false);

            ChangeCameraPivotPosition(2);
        }

        public void SprintAnimation()
        {
            _animator.SetBool(CrouchID, false);
            _animator.SetBool(SprintID, true);

            ChangeCameraPivotPosition(3);
        }

        public void DrawWeaponAnimation()
        {
            _animator.SetBool(WeaponOutID, true);
        }

        public void HolsterWeaponAnimation()
        {
            _animator.SetBool(WeaponOutID, false);
        }

        public void AttackAnimation()
        {
            _animator.SetTrigger(AttackID);
        }

        public void ReloadAnimation()
        {
            _animator.SetTrigger(ReloadID);
        }

        public void GrenadeAnimation(bool on)
        {
            if (on)
            {
                _animator.SetTrigger(GrenadeReadyID);
            }

            else
            {
                _animator.SetTrigger(GrenadeThrowID);
            }

        }

        public void AimAnimation(bool toggle)
        {
            _animator.SetBool(AimID, toggle);
        }

        public void ShowWeapon()
        {
            _weaponSlot.ShowWeapon();
        }

        public void HideWeapon()
        {
            _weaponSlot.HideWeapon();
        }

        public void ShowGrenade()
        {
            _grenadeSlot.ShowGrenade();
        }

        public void HideGrenade()
        {
            _grenadeSlot.HideGrenade();
        }

        public void ThrowGrenade()
        {
            _grenadeSlot.ThrowGrenade(_adjustmentParts.raycastPoint);
        }

        public void SetDefaultAnimationStatus()
        {
            animationStatus.isHoldingNothing = false;
            animationStatus.isHoldingGrenade = false;
            animationStatus.isHoldingMeleeWeapon = false;
            animationStatus.isHoldingRangedWeaponHips = false;
            animationStatus.isHoldingRangedWeaponIronsight = false;

            animationStatus.inAttackAnimation = false;
            animationStatus.inReloadAnimation = false;

            animationStatus.inDrawAnimation = false;
            animationStatus.inHolsterAnimation = false;
        }

        public void SetStatusHoldingNothing()
        {
            SetDefaultAnimationStatus();
            animationStatus.isHoldingNothing = true;
        }

        public void SetStatusHoldingGrenade()
        {
            SetDefaultAnimationStatus();
            animationStatus.isHoldingGrenade = true;
        }

        public void SetStatusHoldingMeleeWeapon()
        {
            SetDefaultAnimationStatus();
            animationStatus.isHoldingMeleeWeapon = true;
        }

        public void SetStatusHoldingWeaponHips()
        {
            SetDefaultAnimationStatus();
            animationStatus.isHoldingRangedWeaponHips = true;
        }

        public void SetStatusHoldingWeaponIronsight()
        {
            SetDefaultAnimationStatus();
            animationStatus.isHoldingRangedWeaponIronsight = true;
        }

        public void SetStatusAttackAnimation()
        {
            SetDefaultAnimationStatus();
            animationStatus.inAttackAnimation = true;
        }

        public void SetStatusReloadAnimation()
        {
            SetDefaultAnimationStatus();
            animationStatus.inReloadAnimation = true;
        }

        public void SetStatusDrawingWeapon()
        {
            SetDefaultAnimationStatus();
            animationStatus.inDrawAnimation = true;
        }

        public void SetStatusHolsteringWeapon()
        {
            SetDefaultAnimationStatus();
            animationStatus.inHolsterAnimation = true;
        }

        public void ChangeWeaponAnimationGroup(WeaponGroup group, WeaponType type)
        {
            _animator.SetInteger(WeaponGroupID, (int) group);
            _animator.SetInteger(WeaponTypeID, (int) type);

            if (group == 0 || type == 0)
                HolsterWeaponAnimation();

            else
                DrawWeaponAnimation();
        }

        #endregion

        #region On Hit or Killed

        public void PushPlayer(float magnitude, Vector3 direction)
        {
            animationStatus.isBeingPushed = PushCount;
            animationStatus.pushVector = direction * magnitude* EnemyPlayerPushForce;
        }

        public void OnKilled()
        { 
            enabled = false;
            _animator.enabled = false;
            _charController.enabled = false;

            foreach (Rigidbody rb in _ragdoll)
                rb.isKinematic = false;
        }

        #endregion

        [System.Serializable]
        private struct CameraPositions
        {
            public Vector3 walkPosition;
            public Vector3 crouchPosition;
            public Vector3 sprintPosition;
        }

        [System.Serializable]
        private struct PostureAdjustmentParts
        {
            public Transform head;
            public Transform spine;
            public Transform weaponSlot;
            public Transform flashlight;
            public Transform raycastPoint;
            public Transform mecanimLookAtPoint;
        }

        [System.Serializable]
        private struct PostureAdjustment
        {
            public Vector3 headAimAdjustment;
            public Vector3 spineHipAdjustment;
            public Vector3 spineAimAdjustment;
            public Vector3 spineSprintAdjustment;
            public Vector3 spineGrenadeAdjustment;
        }

        [System.Serializable]
        private struct MovementSpeeds
        {
            [Range(0f, 5f)] public float baseSpeed;
            [Range(0f, 5f)] public float sprintSpeed;
            [Range(0f, 5f)] public float crouchSpeed;
        }

        [System.Serializable]
        public struct PlayerAnimationStatus
        {
            public int isBeingPushed;
            public Vector3 pushVector;

            public bool isHoldingNothing; 
            public bool isHoldingGrenade;
            public bool isHoldingMeleeWeapon;
            public bool isHoldingRangedWeaponHips;
            public bool isHoldingRangedWeaponIronsight;

            public bool inDodgeAnimation;
            public bool inAttackAnimation;
            public bool inReloadAnimation;

            public bool inDrawAnimation;
            public bool inHolsterAnimation;
        }
    }
}

