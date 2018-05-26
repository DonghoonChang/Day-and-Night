using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnimation : MonoBehaviour {

    /*
     * Control Charactor Animation, Interaction with the environment
     */
    [SerializeField] [Range(1f, 10f)] private float speedWalk = 5f;
    [SerializeField] [Range(5f, 20f)] private float speedRun = 10f;
    [SerializeField] [Range(5f, 20f)] private float sensitivityX = 5f;
    [SerializeField] [Range(5f, 20f)] private float sensitivityY = 5f;
    [SerializeField] [Range(20f, 5f)] private float pushForce = 30f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] bool isWalking = true;

    public GameObject bulletMark;

    PlayerController player;
    PlayerWeapon weapon;

    /*
     * Animation
     */
    Camera cam;
    Animator animator;
    CharacterController charCtrl;
    LayerMask noIgnoreRaycastLayer = ~(1 << 2);

    bool equipPistol = false;
    bool equipRifle = false;
    bool equipMelee = false;
    bool isReloading = false;
    bool isFiring = false;

    float camYaw = 0;
    float camPitch = 0;

    int speedXID = Animator.StringToHash("SpeedX");
    int speedYID = Animator.StringToHash("SpeedY");
    int crouchID = Animator.StringToHash("Crouch");
    int pistolID = Animator.StringToHash("EquipPistol");
    int reloadID = Animator.StringToHash("Reloading");
    int fireID = Animator.StringToHash("Firing");

    void Awake()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();
        charCtrl = GetComponent<CharacterController>();
        player = GetComponent<PlayerController>();
        weapon = GetComponentInChildren<PlayerWeapon>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
    }

    // Update is called once per frame
    void Update () {
        animator.SetFloat(speedXID, Input.GetAxis("Horizontal"));
        animator.SetFloat(speedYID, Input.GetAxis("Vertical"));

        /* Toggle Crouch */
        if (Input.GetButtonDown("Crouch"))
            animator.SetBool(crouchID, !animator.GetBool(crouchID));

        /* Toggle Pistol */
        if (Input.GetButtonDown("Equip Pistol"))
        {
            equipPistol = true;
            animator.SetBool(pistolID, !animator.GetBool(pistolID));
        }

        if (Input.GetButtonDown("Reload") && ( equipPistol || equipRifle) && !isReloading)
        {
            isReloading = true;
            animator.SetBool(reloadID, true);
        }

        if (Input.GetButtonDown("Fire") && (equipPistol || equipRifle) && !isFiring)
        {
            Fire();
        }
    }

    void FixedUpdate()
    {
        camYaw += Input.GetAxis("Mouse Y") * sensitivityY;
        camPitch += Input.GetAxis("Mouse X") * sensitivityX;
        camYaw = Mathf.Clamp(camYaw, -90f, 90f);
        camPitch %= 360f;

        transform.eulerAngles = new Vector3(0, camPitch, 0);
        cam.transform.localEulerAngles = new Vector3(-camYaw, 0, 0);

        float speed_multi = isWalking ? speedWalk : speedRun;
        Vector3 move_dir = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, charCtrl.radius, Vector3.down,
            out hit, charCtrl.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, hit.point, Color.green);
            Debug.DrawRay(transform.position - Vector3.down * charCtrl.height / 2f, Vector3.ProjectOnPlane(move_dir, hit.normal), Color.yellow);
        }

        move_dir = Vector3.ProjectOnPlane(move_dir, hit.normal).normalized * speed_multi * Time.deltaTime;

        if (!charCtrl.isGrounded)
        {
            move_dir += Physics.gravity * gravityMultiplier;
        }

        charCtrl.Move(move_dir);


    }

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
        Ray ray = new Ray(cam.transform.position, cam.transform.TransformDirection(Vector3.forward));

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
