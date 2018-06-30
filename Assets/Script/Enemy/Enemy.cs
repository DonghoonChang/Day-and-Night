using UnityEngine;
using GameManager = MyGame.GameManagement.GameManager;
using WeaponStats = MyGame.Inventory.Weapon.WeaponStats;
using HitInfo = MyGame.Inventory.Weapon.HitInfo;


namespace MyGame.Enemy
{
    public class Enemy : MonoBehaviour
    {
        GameManager gameManager;

        [SerializeField]
        [Range(45, 1000)] int health = 150;

        [SerializeField]
        [Range(1, 3)]
        int headshotMultiplier;

        [SerializeField]
        [Range(15, 2000)]
        int exp = 15;

        [SerializeField]
        [Range(0f, 1f)]
        float staggerThreshhold = 0.3f;


        /* Components */
        EnemyAnimator animator;
        //EnemyInventory

        /* Game Objects */
        Player.Player player;

        /* Alert Triggers */
        public AlertTriggers alertTriggers;
        float noiseLevel = 1;

        /* Alertness Parameters */
        bool isDead = false;
        bool isWatching = false;
        bool isFollowing = false;
        bool isChasing = false;
        bool isAttacking = false;
        bool isPlayerNearby = false;
        bool isStaggered = false;


        /* Layer Mask */
        LayerMask onlyPlayerLayer = (1 << 8);

        #region Awake and Updates

        void Awake()
        {
            animator = GetComponent<EnemyAnimator>();

            /* Triggers */
            alertTriggers.AdjustRadiousToScale(transform.localScale.x);
            alertTriggers.AdjustTriggerRadious(noiseLevel);

            Transform[] transforms = GetComponentsInChildren<Transform>();
            foreach (Transform tf in transforms)
                tf.tag = "Enemy";
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            player = gameManager.Player;
        }

        void Update()
        {
            if (isChasing)
            {
                if (!PlayerInSight(alertTriggers.stopChaseDistance, alertTriggers.FieldOfView))
                    ResetAlertStatus();
            }

            else if (isPlayerNearby && !isDead && PlayerInSight(alertTriggers.watchDistance, alertTriggers.FieldOfView))
            {
                OnPlayerFOVEnter();
            }
        }

        #endregion

        public void OnHit(HitInfo[] hitInfos, WeaponStats stats){

            int collectiveDamage = 0;

            foreach (HitInfo hitInfo in hitInfos)
            {
                bool headshot = hitInfo.hit.transform.name.ToLower().Contains("head");
                int multipliedDamage = headshot ? stats.damage * headshotMultiplier : stats.damage;

                collectiveDamage += multipliedDamage;
            }

            bool staggered = collectiveDamage > health * staggerThreshhold;
            health -= collectiveDamage;
            isDead = health <= 0;

            /* Play Death Animation on Death */
            if (isDead)
            {
                alertTriggers.OnKilled();
                animator.OnKilled(hitInfos, stats);
            }

            else
                animator.OnHit(hitInfos, stats, staggered);
        }


        /* Enemy Alertness*/
        #region Triggers, Watch, Follow, Chase, Attack

        public void ResetAlertStatus()
        {
            isAttacking = false;
            isChasing = false;
            isFollowing = false;
            isWatching = false;

            if (isDead)
                return;

            animator.StopAllBehaviour();
        }

        public void OnAttackTriggerEnter()
        {
            if (isAttacking)
                return;

            ResetAlertStatus();

            isAttacking = true;
            animator.StartAttackBehaviour();
        }

        public void OnAttackTriggerExit()
        {
            ResetAlertStatus();
            animator.StopAttackBehaviour();
        }


        public void OnPlayerFOVEnter()
        {
            if (isAttacking || isChasing)
                return;

            ResetAlertStatus();
            isChasing = true;

            animator.StartChaseRoutine();
        }

        public void OnFollowTriggerEnter()
        {
            if (isAttacking || isChasing || isFollowing)
                return;

            ResetAlertStatus();
            isFollowing = true;

            animator.StartFollowBehaviour();
        }

        public void OnWatchTriggerEnter()
        {
            if (isAttacking || isChasing || isFollowing || isWatching)
                return;

            ResetAlertStatus();
            isPlayerNearby = true;
            isWatching = true;

            Vector3 relVec = player.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(relVec);

            if (Quaternion.Angle(transform.rotation, lookRotation) >= alertTriggers.watchAngularThreshhold)
            {
                bool right = Vector3.Dot(transform.right, (player.transform.position - transform.position)) > 0;
                animator.StartWatchBehaviour(right, lookRotation);
            }

            else
            {
                ResetAlertStatus();
            }
        }

        public void OnWatchTriggerExit()
        {
            isPlayerNearby = false;
        }
        #endregion



        bool PlayerInSight(float fovDistance, float FOVAngle)
        {

            /* Approximate FOV -> Left/Right 45 degrees */
            bool front = Vector3.Dot(transform.forward, player.transform.position - transform.position) > Mathf.Cos(FOVAngle);

            float distance = Vector3.Distance(transform.position, player.transform.position);
            bool close = distance < fovDistance;

            Vector3 relDir = (player.transform.position + player.transform.up) - (transform.position + transform.up);
            Ray ray = new Ray(transform.position + transform.up, relDir);
            bool lineOfSight = Physics.Raycast(ray, distance * 1.5f, onlyPlayerLayer);

            return front && close && lineOfSight;

        }

        [System.Serializable]
        public class AlertTriggers
        {
            /* Colliders as Triggers */
            public SphereCollider attackTrigger;
            public SphereCollider followTrigger;
            public SphereCollider watchTrigger;

            /* Radious in World-Space Unit */
            [Range(0f, 1f)]
            public float attackDistance = 0.5f;

            [Range(1f, 3f)]
            public float chaseDistance = 2f;

            [Range(2f, 4f)]
            public float followDistance = 3f;

            [Range(3f, 6f)]
            public float watchDistance = 4f;

            [Range(3f, 5f)]
            public float stopChaseDistance = 5f;

            [Range(0f, 180f)]
            public float FieldOfView = 45f;

            [Range(0f, 180f)]
            public float watchAngularThreshhold = 10f;

            /* Scaled Radious to Fit the Root Transform Scale */
            float scaledAttackRadious;
            float scaledFollowRadious;
            float scaledWatchRadious;

            bool radiousSet = false;

            public void AdjustRadiousToScale(float scale)
            {
                scaledAttackRadious = attackDistance / scale;
                scaledFollowRadious = followDistance / scale;
                scaledWatchRadious = watchDistance / scale;
            }

            public void AdjustTriggerRadious(float multiplier)
            {
                attackTrigger.radius = scaledAttackRadious * multiplier;
                followTrigger.radius = scaledFollowRadious * multiplier;
                watchTrigger.radius = scaledWatchRadious * multiplier;
            }

            public void OnKilled()
            {
                attackTrigger.enabled = false;
                followTrigger.enabled = false;
                watchTrigger.enabled = false;
            }
        }

        [System.Serializable]
        public class EnemyStatus
        {
            /* Alertness Parameters */


            public class PassiveStatus
            {
                bool isWatching = false;
                bool isFollowing = false;
                bool isChasing = false;
                bool isAttacking = false;
                bool isDead = false;
                bool isRaged = false;
            }

            public class ActiveStatus
            {
                /* Go to Player Position on Hit, or Look Towards Player on Hit*/
                bool isReactingToHit = false;
                bool isLookingAround = false;
                bool isWalkingAround = false;
                bool isChasingToDeath = false;
                bool isRunningAway = false;
            }

            public class OtherStatus
            {
                bool isPlayerNearby = false;
                bool isStaggered = false;
            }
        }
    }


}


