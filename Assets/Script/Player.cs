using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour {

    [SerializeField] [Range(0, 200)] private int m_Health = 200;
    [SerializeField] [Range(1f, 10f)] private float m_Speed_Walk = 5f;
    [SerializeField] [Range(5f, 20f)] private float m_Speed_Run = 10f;
    [SerializeField] [Range(5f, 20f)] private float m_Sensitivity_X = 5f;
    [SerializeField] [Range(5f, 20f)] private float m_Sensitivity_Y = 5f;
    [SerializeField] private float m_gravityMultiplier = 2f;
    [SerializeField] bool m_isWalking = true;

    public GameObject cam;
    public CharacterController character_ctrl;
    private float yaw = 0;
    private float pitch = 0;

    void Awake()
    {
        character_ctrl = GetComponent<CharacterController>();
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
        yaw += Input.GetAxis("Mouse Y") * m_Sensitivity_Y;
        yaw = Mathf.Min(yaw, 90f);
        yaw = Mathf.Max(yaw, -90f);
        pitch += Input.GetAxis("Mouse X") * m_Sensitivity_X;
        pitch %= 360f;

        transform.eulerAngles = new Vector3(0, pitch, 0);
        cam.transform.localEulerAngles = new Vector3(-yaw, 0, 0);

        float speed_multi = m_isWalking ? m_Speed_Walk : m_Speed_Run;
        Vector3 move_dir = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, character_ctrl.radius, Vector3.down,
            out hit, character_ctrl.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, hit.point, Color.green);
            Debug.DrawRay(transform.position - Vector3.down * character_ctrl.height/2f, Vector3.ProjectOnPlane(move_dir, hit.normal), Color.yellow);
        }

        move_dir = Vector3.ProjectOnPlane(move_dir, hit.normal).normalized * speed_multi * Time.deltaTime;

        if (!character_ctrl.isGrounded)
        {
            move_dir += Physics.gravity * m_gravityMultiplier;
        }

        character_ctrl.Move(move_dir);

        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    private void Fire()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Collide))
        {
            Debug.Log(hit.transform.name);
            Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward) * hit.distance, Color.red, 0.5f);
        }
    }
}
