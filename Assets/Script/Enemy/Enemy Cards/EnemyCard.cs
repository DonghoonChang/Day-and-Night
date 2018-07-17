using UnityEngine;

namespace MyGame.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/EnemyStatsCard")]
    public class EnemyStatsCard : ScriptableObject
    {
        public EnemyStats stats;
        public EnemyAlertness alertness;
    }

    [CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/EnemyBehaviorCard")]
    public class EnemyBehaviorCard : ScriptableObject
    {
        public EnemyAnimations animations;
    }

    [System.Serializable]
    public class EnemyStats
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
    public class EnemyAlertness
    {
        public float noiseSensitivity;
        public float alterTriggerRadius;

        public float fieldOfView;
        public float chaseStartDistance;
        public float chaseStopDistance;
    }

    [System.Serializable]
    public class EnemyAnimations
    {
        public float walkingSpeed;
        public float runningSpeed;
        public float rotateSpeedWatch;
        public float rotateMinimumAngle;
        public float rotateSpeedWalk;
        public float attackDelay;

        /* Number of Available Animations to choose from */
        public int totalAttackAnimations;
        public int totalIdleAnimations;
        public int totalWalkAnimations;
        public int totalChaseAnimations;
        public int totalStaggerAnimations;
    }
}

