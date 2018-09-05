using UnityEngine;
using PlayerCamera = Game.Player.PlayerCamera;

namespace Game.GameManagement
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] PlayerCamera _playerCamera;
        [SerializeField] Camera mainShoulderCam;
        [SerializeField] Camera vfxShoulderCam;

        #region Properties

        public static CameraManager Instance
        {
            get; private set;
        }

        public PlayerCamera PlayerCamera
        {
            get
            {
                return _playerCamera;
            }
        }

        #endregion

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}

