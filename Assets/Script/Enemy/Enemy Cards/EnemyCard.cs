using UnityEngine;

public abstract class EnemyCard : ScriptableObject {

    public EnemyStats stats;
    public EnemyProperties properties;

    public class EnemyStats
    {
        public int baseHealth;
        public int basExp;
        public int baseDamage;
        public int baseDefense;

        public float baseSpeed;
        public float chaseSpeed;
        public float rotateSpeedWatch;
        public float rotateSpeedFollow;
        public float watchTriggerAngle;

        public float startWatchDistance;
        public float startFollowDistance;
        public float startChaseDistance;
        public float stopChaseDistance;
        public float startAttackDistance;
    }

    public class EnemyProperties
    {
        /* Enemy rages if currentHealth < totalHealth * staggerThreshhold */
        public float rageThreshhold;

        /* Enemy staggers if damage taken in a single hit > totalHealth * staggerThreshhold */
        public float staggerThreshhold;

        /* Enemy runs away if damage taken in a single hit > totalHealth * runawayThreshhold */
        public float runawayThreshhold;

    }
}
