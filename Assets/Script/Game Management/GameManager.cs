using System.Collections.Generic;
using UnityEngine;
using MyGame.Player;

namespace MyGame.GameManagement
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        PlayerCharacter player;

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

        public PlayerCharacter Player
        {
            get
            {
                return player;
            }
        }

        public Vector3 PlayerPosition
        {
            get
            {
                return player.transform.position;
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
        // Built-i Layer
        public static LayerMask DefaultLayer = 0;
        public static LayerMask TransparentFXLayer = (1 << 1);
        public static LayerMask IgnoreRaycastLayer = (1 << 2);
        public static LayerMask WaterLayer = (1 << 4);

        // Custom Layer
        public static LayerMask EnvironmentLayer = (1 << 8);
        public static LayerMask EnemyLayer = (1 << 9);
        public static LayerMask PlayerLayer = (1 << 10);
        public static LayerMask VFXLayer = (1 << 11);
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

