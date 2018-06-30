using System.Collections.Generic;
using UnityEngine;
using MyGame.Player;

namespace MyGame.GameManagement
{
    public class GameManager : MonoBehaviour
    {

        [SerializeField]
        Player.Player player;

        [SerializeField]
        PlayerCamera playerCamera;

        [SerializeField]
        GameConfiguration gameConfig;

        [SerializeField]
        GraphicsConfiguration graphicsConfig;

        [SerializeField]
        MouseConfiguration mouseConfig;

        [SerializeField]
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        #region Properties

        public static GameManager Instance
        {
            get; private set;
        }

        public GameConfiguration GameConfiguration
        {
            get
            {
                return gameConfig;
            }
        }

        public GraphicsConfiguration GraphicsConfiguration
        {
            get
            {
                return graphicsConfig;
            }
        }

        public MouseConfiguration MouseConfiguration
        {
            get
            {
                return mouseConfig;
            }
        }

        public Player.Player Player
        {
            get
            {
                return player;
            }
        }

        public PlayerCamera PlayerCamera
        {
            get
            {
                return playerCamera;
            }
        }
        #endregion

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            DontDestroyOnLoad(gameObject);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !(Cursor.lockState == CursorLockMode.Locked);
        }

        #region Helpers


        public void AddSpawnPoint(SpawnPoint point)
        {
            spawnPoints.Add(point);
        }

        void SetNoiseTirggers(float multiplier)
        {
            foreach (SpawnPoint point in spawnPoints)
                point.SetNoiseTirggers(multiplier);
        }

        void Spawn()
        {
            foreach (SpawnPoint point in spawnPoints)
                point.Spawn();
        }


#endregion
    }

    public class RayCastLayers
    {
        public static LayerMask DefaultLayer = 0;
        public static LayerMask TransparentFXLayer = (1 << 1);
        public static LayerMask IgnoreRaycastLayer = (1 << 2);
        public static LayerMask WaterLayer = (1 << 4);
        public static LayerMask UILayer = (1 << 5);
        public static LayerMask PlayerLayer = (1 << 8);
    }

    [System.Serializable]
    public class GameConfiguration
    {
        public float walkNoiseMultiplier;
        public float crouchNoiseMultiplier;
        public float SprintNoiseMultiplier;
    }

    [System.Serializable]
    public class MouseConfiguration
    {
        public float sensitivityX = 3f;
        public float sensitivityY = 3f;
        public float sensitivityScroll = 3f;
    }

    [System.Serializable]
    public class GraphicsConfiguration
    {
        public float bulletMarkLifeTime = 10f;
        public float CartridgeLifeTime = 10f;
    }
}

