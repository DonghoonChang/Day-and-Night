using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour {

    [SerializeField] [Range(0, 200)] private int m_Health = 200;
    [SerializeField] [Range(1f, 10f)] private float m_SpeedWalk = 5f;
    [SerializeField] [Range(5f, 20f)] private float m_SpeedRun = 10f;
    [SerializeField] [Range(5f, 20f)] private float m_SensitivityX = 5f;
    [SerializeField] [Range(5f, 20f)] private float m_SensitivityY = 5f;
    [SerializeField] [Range(20f, 5f)] private float m_PushForce = 30f;
    [SerializeField] private float m_gravityMultiplier = 2f;
    [SerializeField] bool m_isWalking = true;

    public Weapon weapon;
    public GameObject cam;
    public CharacterController characterCtrl;
    public GameObject bulletMark;
    private float yaw = 0;
    private float pitch = 0;

    void Awake()
    {
        characterCtrl = GetComponent<CharacterController>();
    }
    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        yaw += Input.GetAxis("Mouse Y") * m_SensitivityY;
        pitch += Input.GetAxis("Mouse X") * m_SensitivityX;
        yaw = Mathf.Min(yaw, 90f);
        yaw = Mathf.Max(yaw, -90f);
        pitch %= 360f;

        transform.eulerAngles = new Vector3(0, pitch, 0);
        cam.transform.localEulerAngles = new Vector3(-yaw, 0, 0);

        float speed_multi = m_isWalking ? m_SpeedWalk : m_SpeedRun;
        Vector3 move_dir = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, characterCtrl.radius, Vector3.down,
            out hit, characterCtrl.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, hit.point, Color.green);
            Debug.DrawRay(transform.position - Vector3.down * characterCtrl.height/2f, Vector3.ProjectOnPlane(move_dir, hit.normal), Color.yellow);
        }

        move_dir = Vector3.ProjectOnPlane(move_dir, hit.normal).normalized * speed_multi * Time.deltaTime;

        if (!characterCtrl.isGrounded)
        {
            move_dir += Physics.gravity * m_gravityMultiplier;
        }

        characterCtrl.Move(move_dir);

        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {   
        if (hit.rigidbody != null)
        {
            Rigidbody rb = hit.rigidbody;
            rb.AddForce((hit.transform.position - transform.position).normalized * m_PushForce, ForceMode.Force);
        }

    }
    private void Fire()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Collide))
        {
            string rootTag = hit.transform.root.tag;
            
            if (rootTag == "Enemy")
            {
                bool headshot = hit.transform.name.ToLower().Contains("head");
                hit.transform.root.transform.GetComponent<Zombie>().TakeDamage(weapon.BaseDamage, headshot);
            }
            else
            {
                Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * hit.distance, Color.red, 0.5f);
                GameObject mark = Instantiate(bulletMark, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(mark, 0.5f);
            }

            Debug.Log(hit.transform.name);
            Debug.Log(hit.transform.tag);
        }
    }
}
