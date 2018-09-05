using System.Collections;
using UnityEngine;
using GameManager = Game.GameManagement.GameManager;
using RaycastLayers = Game.GameManagement.RaycastLayers;
using UIManager = Game.GameManagement.UIManager;
using UIHUD = Game.UI.UIHUD;
using EZCameraShake;

namespace Game.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        UIHUD _uiHUD;
        PlayerCharacter _player;
        GameManager _gameManager;

        [SerializeField] Transform _cameraPivot;
        [SerializeField] Transform _camerasPosition;
        [SerializeField] Camera _mainCamera;
        [SerializeField] Camera _vfxCamera;
        [SerializeField] Transform _raycastPoint;
        [SerializeField] Transform _mecanimLookAtPoint;

        [SerializeField]
        CamConfiguration camConfig = new CamConfiguration();

        float _yaw = 0;
        float _pitch = 0;
        bool _scopeVisible = false;

        InteractableObject _interactable = null;
        IEnumerator scopeCoroutine;

        #region Properties

        public float Yaw
        {
            get { return _yaw; }
        }

        public float Pitch
        {
            get { return _pitch; }
        }

        public Camera MainCamera
        {
            get
            {
                return _mainCamera;
            }
        }

        public Transform AimPoint
        {
            get
            {
                return _raycastPoint.transform;
            }
        }

        public Transform MecanimLookAtPoint
        {
            get
            {
                return _mecanimLookAtPoint;
            }
        }

        public InteractableObject InteractableObject
        {
            get
            {
                return _interactable;
            }
            
            private set
            {
                _interactable = value;

                _uiHUD.SetInteractionName((_interactable == null) ? "" : _interactable.Name);
            }
        }

        #endregion

        #region Awake and Updates

        void Awake()
        {
            if (_mainCamera ==  null)
                _mainCamera = Camera.main;

            _vfxCamera.gameObject.SetActive(false);
        }

        void Start()
        {
            _uiHUD = UIManager.Instance.HUDPanel;
            _gameManager = GameManager.Instance;

            _player = _gameManager.Player;
        }

        void Update()
        {
            if (GameTime.isPaused)
                return;

            UpdatePitchYawScroll();
            UpdateCamPitchAndYaw();
            UpdateCamDistance();
        }

        void LateUpdate()
        {
            UpdateCamBase();
            AdjustRaycastPoint();
        }

        private void FixedUpdate()
        {
            if (_player.PlayerStatus.isCrouching)
                _vfxCamera.gameObject.SetActive(true);

            else
                _vfxCamera.gameObject.SetActive(false);

        }

        #endregion

        #region Helpers

        void UpdateCamBase()
        {
            transform.position = Vector3.Lerp(transform.position, _cameraPivot.position, camConfig.camTightness * GameTime.deltaTime);
        }

        void UpdatePitchYawScroll()
        {
            _yaw += Input.GetAxis("Mouse X") * _gameManager.MouseConfiguration.sensitivityX;
            _yaw %= 360f;

            _pitch += Input.GetAxis("Mouse Y") * _gameManager.MouseConfiguration.sensitivityY;
            _pitch = Mathf.Clamp(_pitch, -camConfig.maxCameraDown, camConfig.maxCameraUp);

            camConfig.currDistance = Mathf.Clamp(camConfig.currDistance - Input.GetAxis("Mouse Scroll") * _gameManager.MouseConfiguration.sensitivityScroll, camConfig.minDistance, camConfig.maxDistance);
        }

        void UpdateCamPitchAndYaw()
        {
            transform.eulerAngles = new Vector3(-_pitch, _yaw, 0);
        }

        void UpdateCamDistance()
        {
            RaycastHit hit;

            /* Camera Collision */
            if (Physics.Linecast(transform.position - transform.forward * camConfig.minDistance, transform.position - transform.forward * camConfig.currDistance, out hit, RaycastLayers.EnvironmentLayer, QueryTriggerInteraction.UseGlobal))
            {
                _camerasPosition.localPosition = Vector3.Slerp(_camerasPosition.localPosition, -Vector3.forward * (hit.distance * 0.9f), camConfig.camTightness * GameTime.deltaTime);
            }

            else
            {
                if (!_player.PlayerStatus.isAiming)
                {
                    _camerasPosition.localPosition = Vector3.Slerp(_camerasPosition.localPosition, -Vector3.forward * camConfig.currDistance, camConfig.camTightness * GameTime.deltaTime);

                    if (_scopeVisible)
                    {
                        StopAllCoroutines();
                        scopeCoroutine = HideScope(0f);
                        StartCoroutine(scopeCoroutine);
                    }
                }

                else
                {
                    _camerasPosition.localPosition = Vector3.Slerp(_camerasPosition.localPosition, -Vector3.forward * camConfig.currDistance * camConfig.zoomInFactor, camConfig.camTightness * GameTime.deltaTime);

                    if (_player.IsCurrentWeaponScoped() && !_scopeVisible)
                    {
                        StopAllCoroutines();
                        scopeCoroutine = ShowScope(0.5f);
                        StartCoroutine(scopeCoroutine);
                    }

                    else if(!_player.IsCurrentWeaponScoped() && _scopeVisible)
                    {
                        StopAllCoroutines();
                        scopeCoroutine = HideScope(0f);
                        StartCoroutine(scopeCoroutine);
                    }
                }
            }
        }

        void AdjustRaycastPoint()
        {
            RaycastHit hit;
            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, RaycastLayers.SurfaceSearchLayer, QueryTriggerInteraction.Ignore))
            {
                _raycastPoint.transform.position = hit.point;

                InteractableObject interactable;
                interactable = hit.transform.GetComponent<InteractableObject>();

                // Itself
                if (interactable != null)
                    InteractableObject = interactable;
                
                else
                {
                    // Parent
                    if (hit.transform.parent != null)
                        interactable = hit.transform.parent.GetComponent<InteractableObject>();

                    if (interactable != null)
                        InteractableObject = interactable;

                    else
                    {

                        // Root (End)
                        interactable = hit.transform.root.GetComponent<InteractableObject>();

                        InteractableObject = interactable;
                    }
                }
            }

            else
            {
                _raycastPoint.transform.position = _mainCamera.transform.position + _mainCamera.transform.forward * 20f;
            }
        }

        #endregion

        public void ShakeCameraExplosionMedium()
        {
            CameraShaker.Instance.ShakeOnce(5f, 15f, .2f, .2f, Vector3.zero, new Vector3(2f, 2f, 2f));
        }

        public void ShakeCameraExplosionMajor()
        {
            CameraShaker.Instance.ShakeOnce(7.5f, 15f, .2f, .2f, Vector3.zero, new Vector3(2f, 2f, 2f));
        }

        public void ShakeCameraRoarMinor()
        {
            CameraShaker.Instance.ShakeOnce(1.75f, 20f, 2.5f, 2.5f, Vector3.zero, new Vector3(2f, 2f, 2f));

        }

        public void ShakeCameraRoarMedium()
        {
            CameraShaker.Instance.ShakeOnce(3.5f, 20f, 2.5f, 2.5f, Vector3.zero, new Vector3(.5f, .5f, .5f));

        }

        public void ShakeCameraRoarMajor()
        {
            CameraShaker.Instance.ShakeOnce(5.25f, 20f, 2.5f, 2.5f, Vector3.zero, new Vector3(.5f, .5f, .5f));

        }

        public void ShakeCameraDamageMinor()
        {
            CameraShaker.Instance.ShakeOnce(2.5f, 20f, .15f, .15f, Vector3.zero, new Vector3(4f, 4f, 0f));

        }

        public void ShakeCameraDamageMedium()
        {
            CameraShaker.Instance.ShakeOnce(5f, 20f, .15f, .15f, Vector3.zero, new Vector3(4f, 4f, 0f));

        }

        public void ShakeCameraDamageMajor()
        {
            CameraShaker.Instance.ShakeOnce(7.5f, 20f, .15f, .15f, Vector3.zero, new Vector3(4f, 4f, 0f));

        }

        IEnumerator ShowScope(float time)
        {
            _scopeVisible = true;

            yield return new WaitForSeconds(time);

            _uiHUD.ShowSniperScope();

            _mainCamera.fieldOfView = 15;
            _vfxCamera.fieldOfView = 15;
        }

        IEnumerator HideScope(float time)
        {
            _scopeVisible = false;

            yield return new WaitForSeconds(time);

            _uiHUD.HideSniperScope();

            _mainCamera.fieldOfView = 60;
            _vfxCamera.fieldOfView = 60;
        }

        [System.Serializable]
        private struct CamConfiguration
        {
            [Range(0, 90f)] public float maxCameraUp;
            [Range(0, 90f)] public float maxCameraDown;
            [Range(1f, 5f)] public float minDistance;
            [Range(1f, 5f)] public float currDistance;
            [Range(1f, 5f)] public float maxDistance;
            [Range(0f, 1f)] public float zoomInFactor;
            [Range(0f, 20f)] public float camTightness;
        }

    }
}
