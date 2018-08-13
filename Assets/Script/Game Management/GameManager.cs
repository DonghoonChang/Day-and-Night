using System.Collections.Generic;
using UnityEngine;
using PlayerCharacter = MyGame.Player.PlayerCharacter;
using PlayerStatus = MyGame.Player.PlayerStatus;
using EnemyCharacter = MyGame.Enemy.EnemyCharacter;

namespace MyGame.GameManagement
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        PlayerCharacter _player;

        [SerializeField]
        GameConfiguration _gameConfig;

        [SerializeField]
        MouseConfiguration _mouseConfig;

        [SerializeField]
        GraphicsConfiguration _graphicsConfig;

        [SerializeField]
        List<EnemyCharacter> _enemies= new List<EnemyCharacter>();

        #region Properties

        public static GameManager Instance
        {
            get; private set;
        }

        public PlayerCharacter Player
        {
            get
            {
                return _player;
            }
        }

        public PlayerStatus PlayerStatus
        {
            get
            {
                return Player.PlayerStatus;
            }
        }

        public GameConfiguration GameConfiguration
        {
            get
            {
                return _gameConfig;
            }
        }

        public MouseConfiguration MouseConfiguration
        {
            get
            {
                return _mouseConfig;
            }
        }

        public GraphicsConfiguration GraphicsConfiguration
        {
            get
            {
                return _graphicsConfig;
            }
        }

        public float DamageMultiplier
        {
            get
            {
                switch (_gameConfig.difficulty){

                    case DifficultyConfiguration.Easy:
                        return 1f;

                    case DifficultyConfiguration.Intermediate:
                        return 1.25f;

                    case DifficultyConfiguration.Hard:
                        return 1.5f;

                    case DifficultyConfiguration.Insane:
                        return 1.75f;

                    case DifficultyConfiguration.Nightmare:
                        return 2f;

                    default:
                        return 1f;
                }
            }
        }

        #endregion

        void Awake()
        {
            if (Instance == null)
                Instance = this;

        }

        private void Update()
        {
            AdjustNoiseTirggers(Player.NoiseLevel);
        }

        #region Helpers

        public void AddEnemy(EnemyCharacter enemy)
        {
            _enemies.Add(enemy);
        }

        public void RemoveEnemy(EnemyCharacter enemy)
        {
            _enemies.Remove(enemy);
        }

        void AdjustNoiseTirggers(float multiplier)
        {
            foreach (EnemyCharacter enemy in _enemies)
                enemy.AdjustTriggerRadious(multiplier);
        }

        #endregion
    }

    public class RaycastLayers
    {
        // Built-i Layer
        public static LayerMask DefaultLayer = 0;
        public static LayerMask TransparentFXLayer = (1 << 1);
        public static LayerMask IgnoreRaycastLayer = (1 << 2);
        public static LayerMask WaterLayer = (1 << 4);

        // Custom Layer
        public static LayerMask EnvironmentLayer = (1 << 8);
        public static LayerMask PlayerLayer = (1 << 9);
        public static LayerMask EnemyLayer = (1 << 10);
        public static LayerMask EnemyCapsuleLayer = (1 << 11);
        public static LayerMask CrouchVFXLayer = (1 << 12);
        public static LayerMask ItemLayer = (1 << 13);
        public static LayerMask InventoryItemLayer = (1 << 14);
        public static LayerMask GrenadeThrownLayer = (1 << 15);

        public static LayerMask SurfaceSearchLayer = ~(IgnoreRaycastLayer + PlayerLayer + EnemyCapsuleLayer + CrouchVFXLayer + InventoryItemLayer);
        public static LayerMask BulletLayer = ~(IgnoreRaycastLayer + PlayerLayer + EnemyCapsuleLayer + InventoryItemLayer);
        public static LayerMask ExplosionLayer = (PlayerLayer + EnemyLayer + ItemLayer + GrenadeThrownLayer);
    }

    [System.Serializable]
    public struct GameConfiguration
    {
        public float walkNoiseMultiplier;
        public float crouchNoiseMultiplier;
        public float SprintNoiseMultiplier;

        public DifficultyConfiguration difficulty;
    }

    [System.Serializable]
    public enum DifficultyConfiguration: int
    {
        Easy = 0,
        Intermediate,
        Hard,
        Insane,
        Nightmare
    }

    [System.Serializable]
    public struct MouseConfiguration
    {
        public float sensitivityX;
        public float sensitivityY;
        public float sensitivityScroll;
    }

    [System.Serializable]
    public struct GraphicsConfiguration
    {
        public float bulletMarkLifeTime;
        public float CartridgeLifeTime;
    }
}

