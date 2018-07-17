using UnityEngine;
using GameManager = MyGame.GameManagement.GameManager;
using RayCastLayers = MyGame.GameManagement.RayCastLayers;
using Item = MyGame.Inventory.Item;

namespace MyGame.Player
{
    public class PlayerCamera : MonoBehaviour
    {

        GameManager gameManager;
        PlayerCharacter player;

        [SerializeField]
        Transform cameraBase;

        [SerializeField]
        Camera mainCamera;

        [SerializeField]
        Camera vfxCamera;

        [SerializeField]
        PlayerRaycastHitPoint _hitPoint;

        [SerializeField]
        Transform playerBodyOrientation;

        [SerializeField]
        CamConfiguration camConfig;

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
        
        public Transform BodyOrientation
        {
            get
            {
                return playerBodyOrientation;
            }
        }

        public Transform AimPoint
        {
            get
            {
                return _hitPoint.transform;
            }
        }

        public InteractableObject InteractableObject
        {
            get
            {
                return _hitPoint.InteractableObject;
            }
        }

        #endregion

        #region Awake and Updates


        void Awake()
        {
            if (mainCamera ==  null)
                mainCamera = Camera.main;

            vfxCamera.gameObject.SetActive(false);
            _hitPoint = GetComponentInChildren<PlayerRaycastHitPoint>();
        }

        private void FixedUpdate()
        {
            if (player.PlayerStatus.isCrouching)
                vfxCamera.gameObject.SetActive(true);

            else
                vfxCamera.gameObject.SetActive(false);

        }

        void Start()
        {
            gameManager = GameManager.Instance;
            player = gameManager.Player;
        }

        void Update()
        {
            if (GameTime.isPaused)
                return;

            UpdatePitchYawScroll();
            UpdateCamPitchAndYaw();
            UpdateCamPosition();
        }

        void LateUpdate()
        {
            UpdateCamBase();
            AdjustPlayerAimPoint();
        }

        #endregion

        #region Helpers

        void UpdateCamBase()
        {
            transform.position = Vector3.Lerp(transform.position, cameraBase.position, camConfig.camTightness * GameTime.deltaTime);
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
            if (Physics.Linecast(transform.position - transform.forward * camConfig.minDistance, transform.position - transform.forward * camConfig.currDistance, out hit, RayCastLayers.EnvironmentLayer, QueryTriggerInteraction.UseGlobal))
            {
                mainCamera.transform.localPosition = Vector3.Slerp(mainCamera.transform.localPosition, -Vector3.forward * (hit.distance * 0.9f), camConfig.camTightness * GameTime.deltaTime);
                vfxCamera.transform.localPosition = mainCamera.transform.localPosition;
            }

            else
            {
                if (!player.PlayerStatus.isAiming)
                {
                    mainCamera.transform.localPosition = Vector3.Slerp(mainCamera.transform.localPosition, -Vector3.forward * camConfig.currDistance, camConfig.camTightness * GameTime.deltaTime);
                    vfxCamera.transform.localPosition = mainCamera.transform.localPosition;
                }

                else
                {
                    mainCamera.transform.localPosition = Vector3.Slerp(mainCamera.transform.localPosition, -Vector3.forward * camConfig.currDistance * camConfig.zoomInFactor, camConfig.camTightness * GameTime.deltaTime);
                    vfxCamera.transform.localPosition = mainCamera.transform.localPosition;
                }
            }
        }

        void AdjustPlayerAimPoint()
        {
            RaycastHit hit;
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(RayCastLayers.IgnoreRaycastLayer + RayCastLayers.PlayerLayer), QueryTriggerInteraction.Ignore))
            {
                _hitPoint.transform.position = hit.point;
            }

            else
            {
                _hitPoint.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 20f;
            }
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
