using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using GameManager = MyGame.GameManagement.GameManager;
using SFXManager = MyGame.GameManagement.SFXManager;
using WeaponStats = MyGame.Inventory.Weapon.WeaponStats;
using HitInfo = MyGame.Inventory.Weapon.HitInfo;
using PlayerCharacter = MyGame.Player.PlayerCharacter;
using Random = UnityEngine.Random;

namespace MyGame.Enemy {

    public class EnemyAnimator: MonoBehaviour
    {

        #region Static Variables

        /* Animations */
        static int IdleId = Animator.StringToHash("Idle");
        static int IdleoffsetId = Animator.StringToHash("IdleOffset");
        static int WalkId = Animator.StringToHash("Walk");

        // Attack
        static int AttackAId = Animator.StringToHash("Attack A");
        static int AttackBId = Animator.StringToHash("Attack B");
        static int AttackCId = Animator.StringToHash("Attack C");
        static int AttackDId = Animator.StringToHash("Attack D");
        static int AttackEId = Animator.StringToHash("Attack E");
        static int AttackFId = Animator.StringToHash("Attack F");

        // Stagger
        static int StaggerAId = Animator.StringToHash("Stagger A");
        static int StaggerBId = Animator.StringToHash("Stagger B");
        static int StaggerCId = Animator.StringToHash("StaggerC");

        // Critical Health
        static int RoarId = Animator.StringToHash("Roar");
        static int FakeDeathId = Animator.StringToHash("Fake Death");

        static float TeleportDistance = 3f;
        static float TeleportDelay = 1.5f;
        static float AttackDistance = 1.15f;
        #endregion

        GameManager gameManager;
        SFXManager sfxManager;

        [SerializeField]
        EnemyBehaviorCard behaviorCard;

        // Components 
        Animator animator;
        NavMeshAgent navAgent;
        EnemyRagdollMapperRoot mappingController;

        // Others
        PlayerCharacter player;
        Rigidbody[] ragdollRigidBodies;
        Dictionary<string, Rigidbody> ragdollRBDic = new Dictionary<string, Rigidbody>();

        // Targets
        Transform target;
        Vector3 targetLastSeen;
        float stoppingDistance;
        bool lookAtTarget = false;

        #region Awake to Update

        private void Awake()
        {
            animator = GetComponent<Animator>();
            navAgent = GetComponent<NavMeshAgent>();
            mappingController = GetComponent<EnemyRagdollMapperRoot>();

            animator.SetFloat(IdleoffsetId, Random.Range(0f, 0.35f));
            stoppingDistance = navAgent.stoppingDistance + navAgent.radius / 2f + 0.25f;

            ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in ragdollRigidBodies)
            {
                rb.useGravity = false;
                ragdollRBDic.Add(rb.name, rb);
            }
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
            sfxManager = SFXManager.Instance;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (lookAtTarget)
            {
                animator.SetLookAtWeight(0.5f, 0.5f, 0.5f, 0f, 1f);
                animator.SetLookAtPosition(target.position + target.transform.up * .75f);
            }
        }

        #endregion

        #region Hit Reaction 

        void DisplayBloodSFX(int index, Transform parent, Vector3 normal)
        {
            GameObject bloodsfx = Instantiate(sfxManager.bulletImpactFleshA, parent, true);
            bloodsfx.transform.SetParent(parent, false);
            bloodsfx.transform.localPosition = Vector3.zero;
            bloodsfx.transform.rotation = Quaternion.LookRotation(normal);
            Destroy(bloodsfx, 0.5f);
        }

        public void OnHit(HitInfo[] hitInfos, WeaponStats weaponStats, bool staggered)
        {
            foreach(HitInfo hitInfo in hitInfos)
            {
                Rigidbody hitRB = ragdollRBDic[hitInfo.hit.transform.name];
                hitRB.AddForceAtPosition(hitInfo.ray.direction.normalized * weaponStats.concussion, hitInfo.hit.point, ForceMode.VelocityChange);
                hitRB.AddTorque(hitInfo.ray.direction.normalized * weaponStats.concussion * 2, ForceMode.VelocityChange);
                DisplayBloodSFX(1, hitRB.transform, hitInfo.hit.normal);
            }

            if (staggered)
                TriggerRandomStagger();
        }

        public void OnKilled(HitInfo[] hitInfos, WeaponStats stats)
        {
            ClearBehavior();

            animator.enabled = false;
            navAgent.enabled = false;

            foreach (Rigidbody rb in ragdollRigidBodies)
                rb.useGravity = true;

            foreach (HitInfo hitInfo in hitInfos)
            {
                Rigidbody hitRB = ragdollRBDic[hitInfo.hit.transform.name];
                hitRB.AddForceAtPosition(hitInfo.ray.direction.normalized * stats.concussion, hitInfo.hit.point, ForceMode.VelocityChange);
                hitRB.AddTorque(hitInfo.ray.direction.normalized * stats.concussion * 2, ForceMode.VelocityChange);
                DisplayBloodSFX(1, hitRB.transform, hitInfo.hit.normal);
            }

            mappingController.OnKilled();
        }

        #endregion

        #region Behaivors

        private void ClearBehavior()
        {
            ClearAnimation();
            StopAllCoroutines();
            navAgent.isStopped = true;
        }

        private void ClearAnimation()
        {
            lookAtTarget = false;

            animator.SetInteger(IdleId, 0);
            animator.SetInteger(WalkId, 0);
            animator.ResetTrigger(AttackAId);
            animator.ResetTrigger(AttackBId);
            animator.ResetTrigger(AttackCId);
            animator.ResetTrigger(AttackDId);
            animator.ResetTrigger(AttackEId);
            animator.ResetTrigger(AttackFId);
            animator.ResetTrigger(StaggerAId);
            animator.ResetTrigger(StaggerBId);
            animator.ResetTrigger(RoarId);
        }

        public void SetBehaviorTarget(Transform target)
        {
            this.target = target;
        }

        public void SetTargetLastSeen(Vector3 position)
        {
            targetLastSeen = position;
        }

        /* Idle Behavior */
        public void StartWalkToPointBehavior(Transform point)
        {
            ClearBehavior();
            StartCoroutine(WalkToPointBehavior(point));
        }

        private IEnumerator WalkToPointBehavior(Transform point)
        {
            while (true)
            {
                if (Vector3.Distance(transform.position, point.position) <= stoppingDistance)
                {
                    animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    animator.SetInteger(WalkId, 1);
                    navAgent.SetDestination(point.position);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        public void StartRunToBehavior(Transform point)
        {
            ClearBehavior();
            StartCoroutine(RunToPointBehavior(point));
        }

        private IEnumerator RunToPointBehavior(Transform point)
        {
            while (true)
            {
                if (Vector3.Distance(transform.position, point.position) <= stoppingDistance)
                {
                    animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    animator.SetInteger(WalkId, 2);
                    navAgent.SetDestination(point.position);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        public void StartWayPointBehavior(WayPoints wayPoints)
        {
            ClearBehavior();
            StartCoroutine(WayPointBehavior(wayPoints));
        }

        IEnumerator WayPointBehavior(WayPoints wayPoints)
        {
            Transform[] points = wayPoints.points;
            float waitTime = wayPoints.uponReachWaitTime;
            bool walk = wayPoints.walk;
            bool repeat = wayPoints.patrol;
            bool stopLoop = false;

            int currentIndex = FindCurrentClosestPoint(points);
            int startingIndex = currentIndex;
            Transform currentTargetPoint = points[currentIndex];

            navAgent.SetDestination(currentTargetPoint.position);
            animator.SetInteger(WalkId, walk ? 1 : 2);

            while (!stopLoop)
            {
                // Upon Reaching the Point, Wait for some seconds and Move On
                if (Vector3.Distance(transform.position, points[currentIndex].position) <= stoppingDistance)
                {
                    animator.SetInteger(WalkId, 0);

                    yield return new WaitForSeconds(waitTime);

                    currentIndex = (currentIndex + 1) % points.Length;

                    if (!repeat && currentIndex == startingIndex)
                        stopLoop = true;

                    else
                    {
                        currentTargetPoint = points[currentIndex];
                        navAgent.SetDestination(currentTargetPoint.position);
                        animator.SetInteger(WalkId, walk ? 1 : 2);
                    }
                }

                else
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        int FindCurrentClosestPoint(Transform[] points)
        {
            if (points.Length == 0)
                return -1;

            int currentClosestIndex = -1;
            float currentClosestDistance = -1f;

            for (int i = 0; i < points.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, points[i].position);

                if (distance < currentClosestDistance || currentClosestDistance < 0)
                {
                    currentClosestIndex = i;
                    currentClosestDistance = distance;
                }
            }

            return currentClosestIndex;
        }

        /* Watch Behavior */
        public void StartRotateToLastSeenBehavior()
        {
            ClearBehavior();
            StartCoroutine("WathBehavior");
        }

        private IEnumerator RotateToLastSeenBehavior()
        {
            bool stopLoop = false;
            while (!stopLoop)
            {
                Vector3 relVec = targetLastSeen - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);

                if (Quaternion.Angle(transform.rotation, lookRotation) >= behaviorCard.animations.rotateMinimumAngle)
                {
                    bool right = Vector3.Dot(transform.right, relVec) > 0;
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, behaviorCard.animations.rotateSpeedWatch * GameTime.deltaTime);
                    animator.SetInteger(IdleId, right ? 2 : 1);
                }

                else
                    animator.SetInteger(IdleId, 0);

                yield return null;
            }
        }

        /* Search Behaivor */
        public void StartWalkToLastSeenBehavior()
        {
            ClearBehavior();
            StartCoroutine("WalkToLastSeenBehavior");
        }

        private IEnumerator WalkToLastSeenBehavior()
        {
            animator.SetInteger(WalkId, 1);
            navAgent.SetDestination(targetLastSeen);

            while (true)
            {
                if (Vector3.Distance(transform.position, targetLastSeen) <= stoppingDistance)
                {
                    animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    animator.SetInteger(WalkId, 1);
                    navAgent.SetDestination(targetLastSeen);

                    Vector3 relVec = targetLastSeen - transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(relVec);
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, behaviorCard.animations.rotateSpeedWalk * GameTime.deltaTime);
                }

                yield return null;
            }
        }

        public void StartRunToLastSeenBehavior()
        {
            ClearBehavior();
            StartCoroutine("RunToLastSeenBehavior");
        }

        private IEnumerator RunToLastSeenBehavior()
        {
            animator.SetInteger(WalkId, 2);
            navAgent.SetDestination(targetLastSeen);

            bool stopLoop = false;
            while (!stopLoop)
            {
                if (Vector3.Distance(transform.position, targetLastSeen) <= stoppingDistance)
                {
                    animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    animator.SetInteger(WalkId, 1);
                    navAgent.SetDestination(targetLastSeen);

                    Vector3 relVec = targetLastSeen - transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(relVec);
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, behaviorCard.animations.rotateSpeedWalk * GameTime.deltaTime);
                }

                yield return null;
            }
        }

        public void StartTeleportLastSeenBehavior(float fraction)
        {
            ClearBehavior();
            StartCoroutine(TeleportLastSeenBehavior(fraction));
        }

        private IEnumerator TeleportLastSeenBehavior(float fraction)
        {

            yield return new WaitForSeconds(TeleportDelay / 2f);

            while (true)
            {
                if (Vector3.Distance(transform.position, targetLastSeen) <= TeleportDistance)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    Vector3 relVec = (targetLastSeen - transform.position);
                    Vector3 teleportPosition = transform.position + relVec * fraction;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(teleportPosition, out hit, TeleportDistance * .9f, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;

                        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
                        transform.rotation = lookRotation;

                        yield return new WaitForSeconds(TeleportDelay);
                    }

                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }

        public void StartTeleportLastSeenBehavior(bool front)
        {
            ClearBehavior();
            StartCoroutine(TeleportLastSeenBehavior(front));
        }

        private IEnumerator TeleportLastSeenBehavior(bool front)
        {

            yield return new WaitForSeconds(TeleportDelay / 2f);

            while (true)
            {
                if (Vector3.Distance(transform.position, targetLastSeen) <= TeleportDistance)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    Vector3 relVecNorm = (targetLastSeen - transform.position).normalized;
                    Vector3 teleportPosition = targetLastSeen + (front ? -relVecNorm * TeleportDistance * .5f : relVecNorm * TeleportDistance * .5f);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(teleportPosition, out hit, TeleportDistance * .9f, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;

                        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
                        transform.rotation = lookRotation;

                        yield return new WaitForSeconds(TeleportDelay);
                    }

                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }

        /* Chase and Attack Behavior */
        public void StartChaseAndAttackBehavior()
        {
            ClearBehavior();
            StartCoroutine("ChaseAndAttackBehavior");
        }

        private IEnumerator ChaseAndAttackBehavior()
        {
            lookAtTarget = true;

            while (true)
            {
                Vector3 relVec = target.position - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                transform.rotation = lookRotation;

                if (Vector3.Distance(transform.position, target.position) <= AttackDistance){
                    TriggerRandomAttack();
                    yield return new WaitForSeconds(behaviorCard.animations.attackDelay);
                }

                else
                {
                    animator.SetInteger(WalkId, 2);
                    navAgent.SetDestination(target.position);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        public void StartTeleportAndAttackBehavior(bool front)
        {
            ClearBehavior();
            StartCoroutine(TeleportAndAttackBehavior(front));
        }

        private IEnumerator TeleportAndAttackBehavior(bool front)
        {

            yield return new WaitForSeconds(TeleportDelay / 2f);

            while (true)
            {
                Vector3 relVec = target.position - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, behaviorCard.animations.rotateSpeedWalk * GameTime.deltaTime);

                if (Vector3.Distance(transform.position, target.position) <= AttackDistance)
                {
                    TriggerRandomAttack();
                    yield return new WaitForSeconds(behaviorCard.animations.attackDelay);
                }

                else
                {
                    Vector3 relVecNorm = (target.position - transform.position).normalized;
                    Vector3 teleportPosition = targetLastSeen + (front ? -relVecNorm * AttackDistance * .5f : relVecNorm * AttackDistance * .5f);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(teleportPosition, out hit, AttackDistance * .9f, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;
                        transform.rotation = Quaternion.LookRotation(target.position - transform.position);

                        yield return new WaitForSeconds(TeleportDelay / 3f);
                    }

                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
        }

        private void TriggerRandomAttack()
        {
            int intRandom = Random.Range(1, behaviorCard.animations.totalAttackAnimations + 1);

            switch (intRandom)
            {
                case 1:
                    animator.SetTrigger(AttackAId);
                    return;
                case 2:
                    animator.SetTrigger(AttackBId);
                    return;
                case 3:
                    animator.SetTrigger(AttackCId);
                    return;
                case 4:
                    animator.SetTrigger(AttackDId);
                    return;
                case 5:
                    animator.SetTrigger(AttackEId);
                    return;
                case 6:
                    animator.SetTrigger(AttackFId);
                    return;
                default:
                    animator.SetTrigger(AttackAId);
                    return;
            }
        }

        private void TriggerRandomStagger()
        {
            int intRandom = Random.Range(1, behaviorCard.animations.totalStaggerAnimations + 1);
            switch (intRandom)
            {
                case 1:
                    animator.SetTrigger(StaggerAId);
                    return;
                case 2:
                    animator.SetTrigger(StaggerBId);
                    return;
                case 3:
                    animator.SetTrigger(StaggerCId);
                    return;
                default:
                    animator.SetTrigger(StaggerAId);
                    return;
            }
        }

        private void SetNavAgentStop()
        {
            navAgent.isStopped = true;
            navAgent.speed = 0f;
        }

        private void SetNavAgentFollow()
        {
            navAgent.isStopped = false;
            navAgent.speed = behaviorCard.animations.walkingSpeed;
        }

        private void SetNavAgentChase()
        {
            navAgent.isStopped = false;
            navAgent.speed = behaviorCard.animations.runningSpeed;
        }



        /* Custom Behaviors */


        #endregion
    }
}
