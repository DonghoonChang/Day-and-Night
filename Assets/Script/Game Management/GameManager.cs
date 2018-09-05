using System.Collections.Generic;
using UnityEngine;
using PlayerCharacter = Game.Player.PlayerCharacter;
using PlayerStatus = Game.Player.PlayerStatus;
using EnemyCharacter = Game.Enemy.EnemyCharacter;

namespace Game.GameManagement
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
        public static LayerMask GrenadeLayer = (1 << 15);
        public static LayerMask GrenadeThrownLayer = (1 << 16);
        
        public static LayerMask SurfaceSearchLayer = ~(IgnoreRaycastLayer + PlayerLayer + EnemyCapsuleLayer + CrouchVFXLayer + InventoryItemLayer);
        public static LayerMask BulletLayer = ~(IgnoreRaycastLayer + PlayerLayer + EnemyCapsuleLayer + InventoryItemLayer);
        public static LayerMask ExplosionLayer = (PlayerLayer + EnemyLayer + ItemLayer + GrenadeLayer + GrenadeThrownLayer);

        public static int LayerToInt(LayerMask layer)
        {
            switch (layer)
            {
                case 0:
                    return 0;

                case (1 << 1):
                    return 1;

                case (1 << 2):
                    return 2;

                case (1 << 3):
                    return 3;

                case (1 << 4):
                    return 4;

                case (1 << 5):
                    return 5;

                case (1 << 6):
                    return 6;

                case (1 << 7):
                    return 7;

                case (1 << 8):
                    return 8;

                case (1 << 9):
                    return 9;

                case (1 << 10):
                    return 10;

                case (1 << 11):
                    return 11;

                case (1 << 12):
                    return 12;

                case (1 << 13):
                    return 13;

                case (1 << 14):
                    return 14;

                case (1 << 15):
                    return 15;

                case (1 << 16):
                    return 16;

                case (1 << 17):
                    return 17;

                case (1 << 18):
                    return 18;

                case (1 << 19):
                    return 19;

                case (1 << 20):
                    return 20;

                case (1 << 21):
                    return 21;

                case (1 << 22):
                    return 22;

                case (1 << 23):
                    return 23;

                case (1 << 24):
                    return 24;

                case (1 << 25):
                    return 25;

                case (1 << 26):
                    return 26;

                case (1 << 27):
                    return 27;

                case (1 << 28):
                    return 28;

                case (1 << 29):
                    return 29;

                case (1 << 30):
                    return 30;

                case (1 << 31):
                    return 31;

                default:
                    return 0;
            }
        }
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

