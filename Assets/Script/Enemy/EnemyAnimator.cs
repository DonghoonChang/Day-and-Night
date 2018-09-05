using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UIFloatingStats = Game.UI.UIFloatingStats;
using VFXManager = Game.GameManagement.VFXManager;
using CameraManager = Game.GameManagement.CameraManager;
using PlayerCamera = Game.Player.PlayerCamera;


namespace Game.Enemy {

    public class EnemyAnimator: MonoBehaviour
    {

        #region Static Variables

        /* Animations */
        static int IdleId = Animator.StringToHash("Idle");
        static int WalkId = Animator.StringToHash("Walk");
        static int IdleoffsetId = Animator.StringToHash("IdleOffset");

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
        static int StaggerCId = Animator.StringToHash("Stagger C");

        // Other
        static int RoarId = Animator.StringToHash("Roar");

        static float TeleportDelay = 1.5f;
        static float TeleportDistance = 3f;
        static float rotateSpeedWatch = 2f;
        static float rotateSpeedSearch =  5f;
        static float watchAngularThreshold = 10f;

        #endregion

        VFXManager _vfxManager;

        [SerializeField]
        EnemyBehaviorCard _behaviorCard;
        EnemyAnimations _animations;

        Animator _animator;
        NavMeshAgent _navAgent;
        UIFloatingStats _floatingUI;
        EnemyRagdollMapperRoot _mappingController;

        Rigidbody[] _rigidbodies;
        Dictionary<string, Rigidbody> _rbDic = new Dictionary<string, Rigidbody>();

        Collider[] _colliders;
        Dictionary<string, Collider> _colDic = new Dictionary<string, Collider>();

        Transform _target;
        Vector3 _targetLastSeen;

        bool _ikOnTarget = false;
        float _attackDistance;
        float _stoppingDistance;

        #region Properties

        public float SpeedWalking
        {
            get
            {
                return _animations.speedWalking;
            }
        }

        public float SpeedRunning
        {
            get
            {
                return _animations.speedRunning;
            }
        }

        public float AttackDelay
        {
            get
            {
                return _animations.attackDelay;
            }
        }

        #endregion

        #region Awake to Update

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            _floatingUI = GetComponentInChildren<UIFloatingStats>();
            _mappingController = GetComponent<EnemyRagdollMapperRoot>();

            _animations = _behaviorCard.animations;
            _animator.SetFloat(IdleoffsetId, Random.Range(0f, 0.35f));
            _stoppingDistance = _navAgent.stoppingDistance;
            _attackDistance = _navAgent.stoppingDistance * 1.15f;

            _colliders = GetComponentsInChildren<Collider>();
            _rigidbodies = GetComponentsInChildren<Rigidbody>();

            foreach (Collider col in _colliders)
            {
                _colDic.Add(col.name, col);
            }

            foreach (Rigidbody rb in _rigidbodies)
            {
                rb.useGravity = false;
                _rbDic.Add(rb.name, rb);
            }
        }

        private void Start()
        {
            _vfxManager = VFXManager.Instance;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_ikOnTarget)
            {
                _animator.SetLookAtWeight(.5f, .5f, .5f, .5f, .5f);
                _animator.SetLookAtPosition(_target.position + _target.transform.up * 1f);
            }
        }

        #endregion

        #region Hit Reaction 

        void DisplayBloodSFX(int index, Transform parent, Vector3 normal)
        {
            GameObject bloodsfx = Instantiate(_vfxManager.bulletImpactFleshA, parent, true);
            bloodsfx.transform.SetParent(parent, false);
            bloodsfx.transform.localPosition = Vector3.zero;
            bloodsfx.transform.rotation = Quaternion.LookRotation(normal);
            Destroy(bloodsfx, 0.5f);
        }

        public void OnHit(Transform[] transformList, Vector3[] normalList, bool staggered)
        {
            if (transformList.Length != normalList.Length)
                return;

            for (int i = 0; i < transformList.Length; i++)
            {
                DisplayBloodSFX(1, transformList[i], normalList[i]);
            }

            if (staggered)
                TriggerRandomStagger();
        }

        public void OnKilled()
        {
            ClearBehavior();

            _animator.enabled = false;
            _navAgent.enabled = false;
            _floatingUI.gameObject.SetActive(false);

            foreach (Rigidbody rb in _rigidbodies)
                rb.useGravity = true;

            _mappingController.OnKilled();
        }

        #endregion

        #region Behaivors

        private void ClearBehavior()
        {
            ClearAnimation();
            StopAllCoroutines();

            _ikOnTarget = false;
            _navAgent.isStopped = true;
        }

        private void ClearAnimation()
        {
            _ikOnTarget = false;

            _animator.SetInteger(IdleId, 0);
            _animator.SetInteger(WalkId, 0);
            _animator.ResetTrigger(AttackAId);
            _animator.ResetTrigger(AttackBId);
            _animator.ResetTrigger(AttackCId);
            _animator.ResetTrigger(AttackDId);
            _animator.ResetTrigger(AttackEId);
            _animator.ResetTrigger(AttackFId);
            _animator.ResetTrigger(StaggerAId);
            _animator.ResetTrigger(StaggerBId);
            _animator.ResetTrigger(RoarId);
        }

        public void SetBehaviorTarget(Transform target)
        {
            this._target = target;
        }

        public void SetTargetLastSeen(Vector3 position)
        {
            _targetLastSeen = position;
        }

        // Idle Behaviors
        public void StartWalkToPointBehavior(Transform point)
        {
            ClearBehavior();
            StartCoroutine(WalkToPointBehavior(point));
        }

        private IEnumerator WalkToPointBehavior(Transform point)
        {
            while (true)
            {
                if (Vector3.Distance(transform.position, point.position) <= _stoppingDistance)
                {
                    _animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    _animator.SetInteger(WalkId, 1);
                    _navAgent.SetDestination(point.position);
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
                if (Vector3.Distance(transform.position, point.position) <= _stoppingDistance)
                {
                    _animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    _animator.SetInteger(WalkId, 2);
                    _navAgent.SetDestination(point.position);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        public void StartWayPointBehavior(WayPoints wayPoints)
        {
            ClearBehavior();
            StartCoroutine(WayPointBehavior(wayPoints));
        }

        private IEnumerator WayPointBehavior(WayPoints wayPoints)
        {
            Transform[] points = wayPoints.points;
            float waitTime = wayPoints.uponReachWaitTime;
            bool walk = wayPoints.walk;
            bool repeat = wayPoints.patrol;
            bool stopLoop = false;

            int currentIndex = FindCurrentClosestPoint(points);
            int startingIndex = currentIndex;
            Transform currentTargetPoint = points[currentIndex];

            _navAgent.SetDestination(currentTargetPoint.position);
            _animator.SetInteger(WalkId, walk ? 1 : 2);

            while (!stopLoop)
            {
                // Upon Reaching the Point, Wait for some seconds and Move On
                if (Vector3.Distance(transform.position, points[currentIndex].position) <= _stoppingDistance)
                {
                    _animator.SetInteger(WalkId, 0);

                    yield return new WaitForSeconds(waitTime);

                    currentIndex = (currentIndex + 1) % points.Length;

                    if (!repeat && currentIndex == startingIndex)
                        stopLoop = true;

                    else
                    {
                        currentTargetPoint = points[currentIndex];
                        _navAgent.SetDestination(currentTargetPoint.position);
                        _animator.SetInteger(WalkId, walk ? 1 : 2);
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

        // Watch Behaviors 
        public void StartRotateToLastSeenBehavior()
        {
            ClearBehavior();
            StartCoroutine("RotateToLastSeenBehavior");
        }

        private IEnumerator RotateToLastSeenBehavior()
        {
            bool stopLoop = false;
            while (!stopLoop)
            {
                Vector3 relVec = _targetLastSeen - transform.position;
                relVec = Vector3.ProjectOnPlane(relVec, transform.up);
                Quaternion lookRotation = Quaternion.LookRotation(relVec);

                if (Quaternion.Angle(transform.rotation, lookRotation) >= watchAngularThreshold)
                {
                    bool right = Vector3.Dot(transform.right, relVec) > 0;
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotateSpeedWatch * GameTime.deltaTime);
                    _animator.SetInteger(IdleId, right ? 2 : 1);
                }

                else
                    _animator.SetInteger(IdleId, 0);

                yield return null;
            }
        }

        // Search Behaivors 
        public void StartWalkToLastSeenBehavior()
        {
            ClearBehavior();
            StartCoroutine("WalkToLastSeenBehavior");
        }

        private IEnumerator WalkToLastSeenBehavior()
        {
            _animator.SetInteger(WalkId, 1);
            _navAgent.SetDestination(_targetLastSeen);

            while (true)
            {
                if (Vector3.Distance(transform.position, _targetLastSeen) <= _stoppingDistance)
                {
                    _animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    _animator.SetInteger(WalkId, 1);
                    _navAgent.SetDestination(_targetLastSeen);

                    Vector3 relVec = _targetLastSeen - transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(relVec);
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotateSpeedSearch * GameTime.deltaTime);
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
            _animator.SetInteger(WalkId, 2);
            _navAgent.SetDestination(_targetLastSeen);

            bool stopLoop = false;
            while (!stopLoop)
            {
                if (Vector3.Distance(transform.position, _targetLastSeen) <= _stoppingDistance)
                {
                    _animator.SetInteger(WalkId, 0);
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    _animator.SetInteger(WalkId, 1);
                    _navAgent.SetDestination(_targetLastSeen);

                    Vector3 relVec = _targetLastSeen - transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(relVec);
                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotateSpeedSearch * GameTime.deltaTime);
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
                if (Vector3.Distance(transform.position, _targetLastSeen) <= TeleportDistance)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    Vector3 relVec = (_targetLastSeen - transform.position);
                    Vector3 teleportPosition = transform.position + relVec * fraction;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(teleportPosition, out hit, TeleportDistance * .9f, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;

                        Quaternion lookRotation = Quaternion.LookRotation(_target.position - transform.position);
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
                if (Vector3.Distance(transform.position, _targetLastSeen) <= TeleportDistance)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                else
                {
                    Vector3 relVecNorm = (_targetLastSeen - transform.position).normalized;
                    Vector3 teleportPosition = _targetLastSeen + (front ? -relVecNorm * TeleportDistance * .5f : relVecNorm * TeleportDistance * .5f);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(teleportPosition, out hit, TeleportDistance * .9f, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;

                        Quaternion lookRotation = Quaternion.LookRotation(_target.position - transform.position);
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

        // Chase and Attack Behavior
        public void StartChaseAndAttackBehavior()
        {
            ClearBehavior();
            StartCoroutine("ChaseAndAttackBehavior");
        }

        private IEnumerator ChaseAndAttackBehavior()
        {
            _ikOnTarget = true;

            float attackTimer = AttackDelay;

            while (true)
            {

                Vector3 relVec = _target.position - transform.position;
                relVec = Vector3.ProjectOnPlane(relVec, transform.up);

                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                transform.rotation = lookRotation;

                if (Vector3.Distance(transform.position, _target.position) <= _attackDistance){

                    _animator.SetInteger(WalkId, 0);

                    if (attackTimer < AttackDelay)
                        attackTimer += Time.deltaTime;

                    else
                    {

                        TriggerRandomAttack();
                        attackTimer = 0f;
                    }   

                    yield return null;
                }

                else
                {
                    _animator.SetInteger(WalkId, 2);
                    _navAgent.SetDestination(_target.position);
                    yield return null;
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
            _ikOnTarget = true;

            float attackTimer = 0f;
            float teleportTimer = 0f;

            while (true)
            {
                Vector3 relVec = _target.position - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(relVec);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotateSpeedSearch * GameTime.deltaTime);

                if (Vector3.Distance(transform.position, _target.position) <= _attackDistance)
                {
                    _animator.SetInteger(IdleId, 0);

                    if (attackTimer < AttackDelay)
                    {
                        attackTimer += Time.deltaTime;
                    }

                    else
                    {
                        attackTimer = 0f;
                        TriggerRandomAttack();
                    }

                    yield return null;
                }

                else
                {
                    teleportTimer += Time.deltaTime;

                    if (teleportTimer > TeleportDelay)
                    {
                        NavMeshHit hit;

                        Vector3 teleportPosition = _targetLastSeen + (front ? -relVec.normalized * _attackDistance * .5f : relVec.normalized * _attackDistance * .5f);

                        if (NavMesh.SamplePosition(teleportPosition, out hit, _attackDistance * .9f, NavMesh.AllAreas))
                        {
                            // Teleport and THEN, Look at the Target
                            transform.position = hit.position;
                            transform.rotation = Quaternion.LookRotation(_target.position - transform.position);

                            teleportTimer = 0f;
                        }

                        yield return null;
                    }

                    else
                        yield return null;
                }
            }
        }

        // Other Behaviors
        public void Roar(int intensity)
        {
            _animator.SetTrigger(RoarId);

            PlayerCamera playerCamera = CameraManager.Instance.PlayerCamera;
            switch (intensity)
            {
                case 1:
                    playerCamera.ShakeCameraRoarMinor();
                    return;

                case 2:
                    playerCamera.ShakeCameraRoarMedium();
                    return;

                case 3:
                    playerCamera.ShakeCameraRoarMajor();
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        /// Randomly Triggers an Attack Animation B/W 1 - 6
        /// </summary>
        private void TriggerRandomAttack()
        {
            int intRandom = Random.Range(1, 7);

            switch (intRandom)
            {
                case 1:
                    _animator.SetTrigger(AttackAId);
                    return;
                case 2:
                    _animator.SetTrigger(AttackBId);
                    return;
                case 3:
                    _animator.SetTrigger(AttackCId);
                    return;
                case 4:
                    _animator.SetTrigger(AttackDId);
                    return;
                case 5:
                    _animator.SetTrigger(AttackEId);
                    return;
                case 6:
                    _animator.SetTrigger(AttackFId);
                    return;
                default:
                    _animator.SetTrigger(AttackAId);
                    return;
            }
        }

        /// <summary>
        /// Randomly Triggers an Stagger Animation B/W 1 - 3
        /// </summary>
        private void TriggerRandomStagger()
        {
            int intRandom = Random.Range(1, 4);
            switch (intRandom)
            {
                case 1:
                    _animator.SetTrigger(StaggerAId);
                    return;
                case 2:
                    _animator.SetTrigger(StaggerBId);
                    return;
                case 3:
                    _animator.SetTrigger(StaggerCId);
                    return;
                default:
                    _animator.SetTrigger(StaggerAId);
                    return;
            }
        }

        private void SetNavAgentStop()
        {
            _navAgent.isStopped = true;
            _navAgent.speed = 0f;
        }

        private void SetNavAgentSlide()
        {
            _navAgent.isStopped = false;
            _navAgent.speed = 0.2f;
        }

        private void SetNavAgentFollow()
        {
            _navAgent.isStopped = false;
            _navAgent.speed = _animations.speedWalking;
        }

        private void SetNavAgentChase()
        {
            _navAgent.isStopped = false;
            _navAgent.speed = _animations.speedRunning;
        }

        #endregion


    }
}
