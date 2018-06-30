using UnityEngine;
using GameManager = MyGame.GameManagement.GameManager;
using RayCastLayers = MyGame.GameManagement.RayCastLayers;

namespace MyGame.Player
{
    public class PlayerCamera : MonoBehaviour
    {

        GameManager gameManager;

        [SerializeField]
        Transform cameraPivot;

        [SerializeField]
        Transform bodyFocus;

        [SerializeField]
        Transform weaponFocus;

        [SerializeField]
        CamConfiguration camConfig;

        Player player;
        Camera mainCamera;

        float yaw = 0;
        float pitch = 0;

        public float Yaw
        {
            get { return yaw; }
        }

        public float Pitch
        {
            get { return pitch; }
        }

        #region Properties
        
        public Transform BodyFocus
        {
            get
            {
                return bodyFocus;
            }
        }

        public Transform WeaponFocus
        {
            get
            {
                return weaponFocus;
            }
        }

        #endregion

        #region Awake and Updates


        void Awake()
        {
            mainCamera = Camera.main;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            player = gameManager.Player;
        }

        void Update()
        {
            UpdatePitchYawScroll();
            UpdateCamPitchAndYaw();
            UpdateCamPosition();
        }

        void LateUpdate()
        {
            UpdateCamBase();
            AdjustWeaponFocusPoint();
        }

        #endregion

        #region Helpers

        void UpdateCamBase()
        {
            transform.position = Vector3.Lerp(transform.position, cameraPivot.position, camConfig.camTightness * Time.deltaTime);
        }

        void UpdatePitchYawScroll()
        {
            yaw += Input.GetAxis("Mouse X") * gameManager.MouseConfiguration.sensitivityX;
            yaw %= 360f;

            pitch += Input.GetAxis("Mouse Y") * gameManager.MouseConfiguration.sensitivityY;
            pitch = Mathf.Clamp(pitch, -camConfig.maxCameraDown, camConfig.maxCameraUp);

            camConfig.currDistance = Mathf.Clamp(camConfig.currDistance - Input.GetAxis("Mouse Scroll") * gameManager.MouseConfiguration.sensitivityScroll, camConfig.minDistance, camConfig.maxDistance);
        }

        void UpdateCamPitchAndYaw()
        {
            transform.eulerAngles = new Vector3(-pitch, yaw, 0);
        }

        void UpdateCamPosition()
        {
            RaycastHit hit;

            /* Camera Collision */
            if (Physics.Linecast(transform.position - transform.forward * camConfig.minDistance, transform.position - transform.forward * camConfig.currDistance, out hit))
            {
                mainCamera.transform.localPosition = Vector3.Slerp(mainCamera.transform.localPosition, -Vector3.forward * (hit.distance * 0.9f), camConfig.camTightness * Time.deltaTime);
            }

            else
            {
                if (!player.PlayerStatus.upperBody.isAiming)
                    mainCamera.transform.localPosition = Vector3.Slerp(mainCamera.transform.localPosition, -Vector3.forward * camConfig.currDistance, camConfig.camTightness * Time.deltaTime);

                else
                    mainCamera.transform.localPosition = Vector3.Slerp(mainCamera.transform.localPosition, -Vector3.forward * camConfig.currDistance * camConfig.zoomInFactor, camConfig.camTightness * Time.deltaTime);
            }
        }

        void AdjustWeaponFocusPoint()
        {
            RaycastHit hit;
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~RayCastLayers.IgnoreRaycastLayer, QueryTriggerInteraction.Ignore))
                weaponFocus.position = hit.point;

            else
                weaponFocus.position = mainCamera.transform.position + mainCamera.transform.forward * 20f;
        }

        #endregion

        [System.Serializable]
        private class CamConfiguration
        {
            [Range(0, 90f)]
            public float maxCameraUp;

            [Range(0, 90f)]
            public float maxCameraDown;

            [Range(1f, 5f)]
            public float minDistance = 0.5f;

            [Range(1f, 5f)]
            public float currDistance = 1.5f;

            [Range(1f, 5f)]
            public float maxDistance = 1.5f;

            [Range(0f, 1f)]
            public float zoomInFactor = 0.8f;

            public float camTightness = 10f;
        }
    }
}
