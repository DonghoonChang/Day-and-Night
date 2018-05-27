using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraControll : MonoBehaviour {

    Vector3 defaultOffset = new Vector3(0.5f, 2.5f, -1f);
    Vector3 offset = new Vector3(0, 0, -1);
    [SerializeField] [Range(1f, 20f)] float offsetLength;

    float xOffset = 0.75f;
    float yOffset = 3.0f;
    float zOffset = -1.0f;

    float yaw = 0;
    float pitch = 0;
    float sensitivityX = 3f;
    float sensitivityY = 3f;

    public float Yaw
    {
        get { return yaw; }
    }

    public float Pitch
    {
        get { return pitch; }
    }


    void FixedUpdate()
    {
        UpdatePitchYaw();
        RotateCamUpandDown();
    }

    void RotateCamUpandDown()
    {
        transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    void UpdatePitchYaw()
    {
        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        pitch += -Input.GetAxis("Mouse Y") * sensitivityY;

        yaw %= 360f;
        pitch = Mathf.Clamp(pitch, -50f, 50f);
    }

}
