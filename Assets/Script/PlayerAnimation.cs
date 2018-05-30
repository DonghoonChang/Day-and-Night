using UnityEngine;

[System.Serializable]
public class PlayerAnimation : MonoBehaviour {

    enum Walking : int { CROUCHING = 0, WALKING, SPRINTING }

    /* Control Charactor Animation, Interaction with the environment */
    [SerializeField] [Range(1f, 5f)] private float baseSpeed;
    [SerializeField] [Range(2f, 10f)] private float pushForce;
    [SerializeField] [Range(0f, 1f)] private float crouchMultiplier;
    [SerializeField] [Range(1f, 3f)] private float sprintingMultiplier;
    [SerializeField] private float gravityMultiplier;

    public GameObject bulletMark;

    /* Game Logic Related */
    PlayerWeapon weapon;
    PlayerController player;

    /* Camera */
    Camera mainCam;
    public GameObject cameraPivot;
    public ThirdPersonCameraControl mainCamController;
    public GameObject playerSpine;

    /* Animation */
    Animator animator;
    CharacterController charController;
    LayerMask noIgnoreRaycastLayer = ~(1 << 2);

    /* Firing Animation Related */
    bool equipPistol = false;
    bool equipRifle = false;
    bool equipMelee = false;
    bool isReloading = false;
    bool isFiring = false;

    /* Crouch Cam Control */
    float crouchCamSmoothness = 5f;
    Vector3 pivotOffset = new Vector3(0.5f, 1.8f, 0f);
    Vector3 pivotCrouchOffset = new Vector3(0.5f, 1.2f, 0f);

    /* Character Spine Tilt to Mouse Movement */
    float spineTiltCurrent = 0;
    float spineTiltRifle = -10f;
    float spineTiltPistol = -15f;
    float spineTiltSmoothness = 5f;

    /* Walking Status */
    Walking walkingStatus = Walking.WALKING;

    /* Animation Hash Integers */
    static int SpeedXID = Animator.StringToHash("SpeedX");
    static int SpeedYID = Animator.StringToHash("SpeedY");
    static int CrouchID = Animator.StringToHash("Crouch");
    static int SprintID = Animator.StringToHash("Sprint");
    static int PistolID = Animator.StringToHash("EquipPistol");
    static int RifleID = Animator.StringToHash("EquipRifle");
    static int MeleeID = Animator.StringToHash("EquipMelee");
    static int ReloadID = Animator.StringToHash("Reloading");
    static int FireID = Animator.StringToHash("Firing");


    #region Routines
    void Awake()
    {
        player = GetComponent<PlayerController>();
        weapon = GetComponentInChildren<PlayerWeapon>();

        mainCam = Camera.main;
        cameraPivot.transform.localPosition = pivotOffset;

        animator = GetComponent<Animator>();
        charController = GetComponent<CharacterController>();

    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update () {


        /* Toggle Crouch */
        if (Input.GetButtonDown("Crouch"))
        {
                            
            if (walkingStatus == Walking.WALKING || walkingStatus == Walking.SPRINTING)
            {
                walkingStatus = Walking.CROUCHING;
                animator.SetBool(CrouchID, true);
                animator.SetBool(SprintID, false);
            }

            else
            {
                walkingStatus = Walking.WALKING;
                animator.SetBool(CrouchID, false);
            }

        }

        /* Toggle Sprint */
        if (Input.GetButtonDown("Sprint") && (walkingStatus == Walking.WALKING || walkingStatus == Walking.SPRINTING))
        {
                walkingStatus = Walking.SPRINTING;
                equipPistol = false;
                equipRifle = false;
                animator.SetBool(CrouchID, false);
                animator.SetBool(SprintID, true);
                animator.SetBool(RifleID, false);
                animator.SetBool(PistolID, false);
        }

        if (Input.GetButtonUp("Sprint") && walkingStatus == Walking.SPRINTING)
        {
                walkingStatus = Walking.WALKING;
                animator.SetBool(CrouchID, false);
                animator.SetBool(SprintID, false);
        }

        /* Toggle Pistol */
        if (Input.GetButtonDown("Equip Pistol") && !(walkingStatus == Walking.SPRINTING))
        {
            if (equipPistol)
            {
                HolsterPistol();
            }

            else
            {
                HolsterMelee();
                DrawPistol();
                HolsterRifle();
            }
        }

        /* Toggle Rifle */
        if (Input.GetButtonDown("Equip Rifle") && !(walkingStatus == Walking.SPRINTING))
        {
            if (equipRifle)
            {
                HolsterRifle();
            }
            else
            {
                HolsterMelee();
                HolsterPistol();
                DrawRifle();
            }
        }

        /* Toggle Reload */
        if (Input.GetButtonDown("Reload") && ( equipPistol || equipRifle) && !isReloading)
        {
            isReloading = true;
            animator.SetBool(ReloadID, true);
        }

        /* Firing Weapon */
        if (Input.GetButtonDown("Fire"))
        {
            /* If ranged weapon is equipeed */
            if ((equipPistol || equipRifle) && !isFiring)
            {
                Fire();
            }

            /* If not, equip pistol */
            else if (!equipPistol && !equipRifle && walkingStatus != Walking.SPRINTING)
            {
                DrawPistol();
            }
        }

    }

    void LateUpdate()
    {
        /* Calling the function in LateUpdate to override animator rendering of the player character model */
        AdjustPlayerSpine();
    }

    void FixedUpdate()
    {
        /* Update Fields For Animation Blending */
        animator.SetFloat(SpeedXID, Input.GetAxis("Horizontal"));
        animator.SetFloat(SpeedYID, Input.GetAxis("Vertical"));

        MovePlayer();
        FaceCamera();
        CrouchCamera();

    }
    #endregion

    #region Routine Methods

    /* Adjust Player Spine to Face Cam Direction */
    void AdjustPlayerSpine()
    {
        float spineTilt = (equipRifle ? spineTiltRifle : equipPistol ? spineTiltPistol : 0);
        spineTiltCurrent = Mathf.Lerp(spineTiltCurrent, spineTilt, spineTiltSmoothness * Time.deltaTime);
        playerSpine.transform.localEulerAngles = new Vector3(mainCamController.Pitch + spineTiltCurrent, 0, playerSpine.transform.localEulerAngles.z);
    }

    /* Make Player Face the Camera Direction along y-axis */
    void FaceCamera()
    {
        transform.eulerAngles = new Vector3(0, mainCamController.Yaw, 0);
    }

    /* Character Movement */
    void MovePlayer()
    {
        Vector3 move_dir = (transform.forward * Input.GetAxis("Vertical")
                            + transform.right * Input.GetAxis("Horizontal")).normalized;

        RaycastHit sphereHit;

        if (Physics.SphereCast(transform.position, charController.radius, Vector3.down,
            out sphereHit, charController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, sphereHit.point, Color.green);
            Debug.DrawRay(transform.position - Vector3.down * charController.height / 2f, Vector3.ProjectOnPlane(move_dir, sphereHit.normal), Color.yellow);
        }

        move_dir = Vector3.ProjectOnPlane(move_dir, sphereHit.normal).normalized
                   * baseSpeed
                   * GetSpeedMultiplier(walkingStatus)
                   * Time.deltaTime;

        if (!charController.isGrounded)
        {
            move_dir += Physics.gravity * gravityMultiplier;
        }

        charController.Move(move_dir);
    }

    void CrouchCamera()
    {
        if (walkingStatus == Walking.CROUCHING)
            cameraPivot.transform.localPosition = Vector3.Lerp(cameraPivot.transform.localPosition, pivotCrouchOffset, crouchCamSmoothness * Time.deltaTime);

        else
            cameraPivot.transform.localPosition = Vector3.Lerp(cameraPivot.transform.localPosition, pivotOffset, crouchCamSmoothness * Time.deltaTime);
    }
#endregion

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.rigidbody != null)
        {
            Rigidbody rb = hit.rigidbody;
            rb.AddForce((hit.transform.position - transform.position).normalized * pushForce, ForceMode.Force);
        }

    }

    #region Weapon Animations

    void DrawPistol()
    {
        equipPistol = true;
        animator.SetBool(PistolID, true);
    }

    void HolsterPistol()
    {
        equipPistol = false;
        animator.SetBool(PistolID, false);
    }

    void DrawRifle()
    {
        equipRifle = true;
        animator.SetBool(RifleID, true);
    }

    void HolsterRifle()
    {
        equipRifle = false;
        animator.SetBool(RifleID, false);
    }

    void DrawMelee()
    {
        equipMelee = true;
        animator.SetBool(MeleeID, true);
    }

    void HolsterMelee()
    {
        equipMelee = false;
        animator.SetBool(MeleeID, false);
    }

    private void Fire()
    {

        RaycastHit hit;
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, noIgnoreRaycastLayer, QueryTriggerInteraction.Collide))
        {

            if (hit.transform.root.tag == "Enemy")
            {
                hit.transform.root.transform.GetComponent<EnemyController>().OnHit(player.Damage, weapon.Concussion, hit.transform.name, hit.point, ray);
            }
            else
            {
                GameObject mark = Instantiate(bulletMark, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(mark, 0.5f);
            }
        }

        isFiring = true;
        animator.SetBool(FireID, true);
        Invoke("FinishFiringBool", weapon.FireRatePerSec);
    }

#endregion

    float GetSpeedMultiplier(Walking wk)
    {
        if (wk == Walking.CROUCHING)
            return crouchMultiplier;
        else if (wk == Walking.WALKING)
            return 1f;
        else
            return sprintingMultiplier;
    }

    /* ANIMATION EVENT : reloading animation triggers this even */
    void FinishReloading()
    {
        isReloading = false;
        animator.SetBool(ReloadID, false);
    }

    void FinishFiring()
    {
        animator.SetBool(FireID, false);
    }

    void FinishFiringBool()
    {
        isFiring = false;
    }
}
