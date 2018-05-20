using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCamera : MonoBehaviour {

    [Range (1f, 5f)]
    public float sensitivity_X =  2.5f;
    
    [Range(1f, 5f)]
    public float sensitivity_Y = 2.5f;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
    }

    private void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Fire();
        }
    }

    private void FixedUpdate()
    {

        _yaw += Input.GetAxis("Mouse Y") * sensitivity_Y;
        _pitch += Input.GetAxis("Mouse X") * sensitivity_X;


        transform.eulerAngles = new Vector3(-_yaw, _pitch, 0);

        // Use Quaternion.Slerp() for smooth rotation
    }

    private void Fire()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
        }
    }
}
