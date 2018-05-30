using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraControl : MonoBehaviour {


    public GameObject cameraPivot;
    Camera mainCam;
    Vector3 camArm = new Vector3(0, 0, -1);


    float yaw = 0;
    float pitch = 0;
    float sensitivityX = 3f;
    float sensitivityY = 3f;
    float sensitivityScroll = 3f;

    float smoothness = 10f;
    float dampVelocity = 0f;
    float dampStepTime = 0.15f;

    float minDistance = 0.5f;
    float currDistance = 1.5f;
    float aimDistance = 1.5f;
    float maxDistance = 1.5f;

    bool isAiming = false;

    public float Yaw
    {
        get { return yaw; }
    }

    public float Pitch
    {
        get { return pitch; }
    }

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        aimDistance = currDistance * 2f / 3f;

        if (Input.GetButtonDown("Mouse Aim"))
        {
            isAiming = true;
        }

        else if (Input.GetButtonUp("Mouse Aim"))
        {
            isAiming = false;
        }
    }
    void LateUpdate()
    {
        transform.position = cameraPivot.transform.position;

    }

    void FixedUpdate()
    {
        UpdatePitchYawScroll();
        UpdateCamPosition();
        UpdateCamUpandDown();
    }

    void UpdateCamUpandDown()
    {
        transform.eulerAngles = new Vector3(pitch, yaw, 0);
    }


    void UpdatePitchYawScroll()
    {
        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        yaw %= 360f;

        pitch += -Input.GetAxis("Mouse Y") * sensitivityY;
        pitch = Mathf.Clamp(pitch, -50f, 50f);

        currDistance = Mathf.Clamp(currDistance - Input.GetAxis("Mouse Scroll") * sensitivityScroll, minDistance, maxDistance);
    }

    void UpdateCamPosition()
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position - transform.forward * minDistance,
                             transform.position - transform.forward * currDistance, out hit))
        {

            mainCam.transform.localPosition = Vector3.Slerp(mainCam.transform.localPosition, camArm * (hit.distance * 0.9f), smoothness * Time.deltaTime);

        }
        else
        {
            if (!isAiming)
                mainCam.transform.localPosition = new Vector3(camArm.x * currDistance, camArm.y * currDistance,
                                                              Mathf.SmoothDamp(mainCam.transform.localPosition.z, camArm.z * currDistance,
                                                                               ref dampVelocity, dampStepTime / 2f, Mathf.Infinity));

            else
                mainCam.transform.localPosition = new Vector3(camArm.x * aimDistance, camArm.y * aimDistance,
                                                              Mathf.SmoothDamp(mainCam.transform.localPosition.z, camArm.z * aimDistance,
                                                                               ref dampVelocity, dampStepTime / 2f, Mathf.Infinity));
        }
    }

}
