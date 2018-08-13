using UnityEngine;

namespace MyGame.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/EnemyStatsCard")]
    public class EnemyStatsCard : ScriptableObject
    {
        public EnemyStats stats;
        public EnemyAlertness alertness;
    }

    [System.Serializable]
    public struct EnemyStats
    {
        public float health;
        public float exp;
        public float damage;
        public float defense;

        public float headshotMultiplier;
        public float staggerHealthMultiplier;
        public float criticalHealthMultiplier;
        public float nearDeathHealthMultiplier;
        public float staggerConcussionThreshold;
    }

    [System.Serializable]
    public struct EnemyAlertness
    {
        public float fieldOfViewAngle;
        public float fieldOfViewDistance;

        public float noiseSensitivity;
        public float alertTriggerRadius;
    }

    [System.Serializable]
    public struct EnemyAnimations
    {
        public float attackDelay;
        public float speedWalking;
        public float speedRunning;
    }
}

