using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnimation : MonoBehaviour {

    enum Walking : int { CROUCHING = 0, WALKING, SPRINTING }

    /* Control Charactor Animation, Interaction with the environment */
    [SerializeField] [Range(1f, 5f)] private float baseSpeed;
    [SerializeField] [Range(20f, 5f)] private float pushForce;
    [SerializeField] [Range(0.1f, 1f)] private float crouchMultiplier;
    [SerializeField] [Range(1f, 2f)] private float sprintingMultiplier;
    [SerializeField] private float gravityMultiplier;

    public GameObject bulletMark;
    public GameObject playerSpine;

    PlayerController player;
    PlayerWeapon weapon;

    /* Animation */
    Animator animator;
    CharacterController charCtrl;
    LayerMask noIgnoreRaycastLayer = ~(1 << 2);
    Camera mainCam;
    ThirdPersonCameraControll camCtrl;

    /* Firing Animation Related */
    bool equipPistol = false;
    bool equipRifle = false;
    bool equipMelee = false;
    bool isReloading = false;
    bool isFiring = false;

    /* Character Rotation to Mouse Movement */
    float spineOffset = -15f;

    /* Walking Status */
    Walking walkingStatus = Walking.WALKING;

    int speedXID = Animator.StringToHash("SpeedX");
    int speedYID = Animator.StringToHash("SpeedY");
    int crouchID = Animator.StringToHash("Crouch");
    int pistolID = Animator.StringToHash("EquipPistol");
    int reloadID = Animator.StringToHash("Reloading");
    int fireID = Animator.StringToHash("Firing");

    #region Routines
    void Awake()
    {
        mainCam = Camera.main;
        camCtrl = GetComponentInChildren<ThirdPersonCameraControll>();
        animator = GetComponent<Animator>();
        charCtrl = GetComponent<CharacterController>();
        player = GetComponent<PlayerController>();
        weapon = GetComponentInChildren<PlayerWeapon>();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update () {
        animator.SetFloat(speedXID, Input.GetAxis("Horizontal"));
        animator.SetFloat(speedYID, Input.GetAxis("Vertical"));

        /* Toggle Crouch */
        if (Input.GetButtonDown("Crouch") && (walkingStatus == Walking.WALKING || walkingStatus == Walking.CROUCHING))
        {
            walkingStatus = walkingStatus == Walking.WALKING ? Walking.CROUCHING : Walking.WALKING;
            animator.SetBool(crouchID, !animator.GetBool(crouchID));
        }

        /* Toggle Pistol */
        if (Input.GetButtonDown("Equip Pistol"))
        {
            equipPistol = !equipPistol;
            animator.SetBool(pistolID, !animator.GetBool(pistolID));
        }

        /* Toggle Reload */
        if (Input.GetButtonDown("Reload") && ( equipPistol || equipRifle) && !isReloading)
        {
            isReloading = true;
            animator.SetBool(reloadID, true);
        }

        /* Firing Weapon */
        if (Input.GetButtonDown("Fire"))
        {
            /* If ranged weapon is equipeed */
            if ((equipPistol || equipRifle) && !isFiring)
            {
                Fire();

            /* If not, equip pistol */
            } else if (!equipPistol && !equipRifle)
            {
                equipPistol = true;
                animator.SetBool(pistolID, !animator.GetBool(pistolID));
            }
        }
    }
    void LateUpdate()
    {
        /* Calling the function in LateUpdate to override animator rendering of the player character model */
        AdjustPlayerSpine();
        FaceCamera();
    }
    void FixedUpdate()
    {
        MovePlayer();
    }
    #endregion
    #region Routine Methods

    /* Adjust Player Spine to Face Cam Direction */
    void AdjustPlayerSpine()
    {
        playerSpine.transform.localEulerAngles = new Vector3(0, -camCtrl.Pitch + spineOffset, playerSpine.transform.localEulerAngles.z);
    }

    /* Make Player Face the Camera Direction along y-axis */
    void FaceCamera()
    {
        transform.eulerAngles = new Vector3(0, camCtrl.Yaw, 0);
    }

    /* Character Movement */
    void MovePlayer()
    {
        Vector3 move_dir = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;

        RaycastHit sphereHit;
        if (Physics.SphereCast(transform.position, charCtrl.radius, Vector3.down,
            out sphereHit, charCtrl.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, sphereHit.point, Color.green);
            Debug.DrawRay(transform.position - Vector3.down * charCtrl.height / 2f, Vector3.ProjectOnPlane(move_dir, sphereHit.normal), Color.yellow);
        }

        float speed_multi = GetSpeedMultiplier(walkingStatus);
        move_dir = Vector3.ProjectOnPlane(move_dir, sphereHit.normal).normalized * baseSpeed * speed_multi * Time.deltaTime;

        if (!charCtrl.isGrounded)
        {
            move_dir += Physics.gravity * gravityMultiplier;
        }

        charCtrl.Move(move_dir);
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

    private void Fire()
    {
        RaycastHit hit;
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, noIgnoreRaycastLayer, QueryTriggerInteraction.Collide))
        {

            if (hit.transform.root.tag == "Enemy")
            {
                bool headshot = hit.transform.name.ToLower().Contains("head");
                hit.transform.root.transform.GetComponent<EnemyController>().TakeDamage(player.Damage, weapon.Concussion, headshot, hit.point, ray);
            }
            else
            {
                GameObject mark = Instantiate(bulletMark, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(mark, 0.5f);
            }
        }

        isFiring = true;
        animator.SetBool(fireID, true);
        Invoke("FinishFiring", weapon.FireRate);
    }

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
        animator.SetBool(reloadID, false);
    }

    void FinishFiring()
    {
        isFiring = false;
        animator.SetBool(fireID, false);
    }
}
