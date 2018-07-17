  using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using GameManager = MyGame.GameManagement.GameManager;
using RayCastLayers = MyGame.GameManagement.RayCastLayers;
using WeaponStats = MyGame.Inventory.Weapon.WeaponStats;
using HitInfo = MyGame.Inventory.Weapon.HitInfo;
using Behavior = MyGame.Enemy.EnemyBehavior;
using UIFloatingStats = MyGame.UI.UIFloatingStats;
using EnemyXrayRenderer = MyGame.VFX.EnemyXrayRenderer;


namespace MyGame.Enemy
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyAnimator))]
    [RequireComponent(typeof(EnemyAudioPlayer))]
    [RequireComponent(typeof(EnemyRagdollMapperRoot))]
    [RequireComponent(typeof(SphereCollider))]
    public class EnemyCharacter : MonoBehaviour
    {

        #region Static Variables

        static float ChaseAlertLevelThreshold = 20f;
        static float SearchAlertLevelThreshold = 10f;
        static float WatchAlertLevelThreshold = 4f;
        static float MaxAlertLevel = 30f;
        static float AlertLevelCool = 1f;

        #endregion

        GameManager _gameManager;

        EnemyAnimator _animator;
        EnemyXrayRenderer[] _alertnessRenderers;
        UIFloatingStats _statsUI;

        [SerializeField]
        EnemyStatsCard enemyCard;

        // Status
        float _maxHealth;
        float _prevHealth;
        float _currentHealth;

        float _staggerHealth;
        float _criticalHealth;
        float _nearDeathHealth;
        float _staggerConcussion;

        float _exp;
        float _damage;
        float _defense;

        [SerializeField]
        Transform target;

        [SerializeField]
        Behavior currentBehavior = Behavior.Idle;

        // Behavior
        [SerializeField]
        SphereCollider _alertTrigger;
        float _defaultTriggerRadius;
        float _currentTriggerRadius;

        [SerializeField]
        float _currentAlertLevel = 0f;
        float _prevAlertLevel = 0f;
        bool _targetNearby = false;

        [SerializeField]
        EnemyStatus status;

        #region Behaviors and Events

        [SerializeField]
        UnityEvent StartBehavior;

        [SerializeField]
        UnityEvent IdleBehavior;

        [SerializeField]
        UnityEvent WatchBehavior;

        [SerializeField]
        UnityEvent SearchBehavior;

        [SerializeField]
        UnityEvent ChaseBehavior;


        [SerializeField]
        UnityEvent PlayerFirstSeenEvent;
        bool playerFirstSeenBehaviorTriggered = false;

        [SerializeField]
        UnityEvent PlayerLostEvent;


        [SerializeField]
        UnityEvent FirstHitEvent;

        [SerializeField]
        UnityEvent EveryHitEvent;


        [SerializeField]
        UnityEvent CriticalHealthEvent;

        [SerializeField]
        UnityEvent NearDeathEvent;

        [SerializeField]
        UnityEvent DeathEvent;

        #endregion

        #region Properties

        public float FieldOfView
        {
            get
            {
                return enemyCard.alertness.fieldOfView;
            }
        }
        public float ChaseDistanceStart
        {
            get
            {
                return enemyCard.alertness.chaseStartDistance;
            }
        }

        public float ChaseDistanceStop
        {
            get
            {
                return enemyCard.alertness.chaseStopDistance;
            }
        }

        public float CurrentTriggerRadius
        {
            get
            {
                return GetTriggerRadiusWorldSpace();
            }
        }

        #endregion

        #region Awake and Updates

        void Awake()
        {
            // Components
            _animator = GetComponent<EnemyAnimator>();
            _alertTrigger = GetComponent<SphereCollider>();
            _alertnessRenderers = GetComponentsInChildren<EnemyXrayRenderer>();
            _statsUI = GetComponentInChildren<UIFloatingStats>();

            // Stats
            _maxHealth = enemyCard.stats.health;
            _currentHealth = _maxHealth;
            _prevHealth = _maxHealth;

            _staggerHealth = _maxHealth * enemyCard.stats.staggerHealthMultiplier;
            _criticalHealth = _maxHealth * enemyCard.stats.criticalHealthMultiplier;
            _nearDeathHealth = _maxHealth * enemyCard.stats.nearDeathHealthMultiplier;
            _staggerConcussion = enemyCard.stats.staggerConcussionThreshold;

            _exp = enemyCard.stats.exp;
            _damage = enemyCard.stats.damage;
            _defense = enemyCard.stats.defense;

            // Alert Trigger
            _alertTrigger.isTrigger = true;
            SetTriggerRadious(enemyCard.alertness.alterTriggerRadius);
            AdjustTriggerRadious(1f);

            //UI
            if (_statsUI != null)
                _statsUI.SetTargetFill(1f);
        }

        void Start()
        {
            _gameManager = GameManager.Instance;

            if (target == null)
            {
                SetBehaviorTarget(_gameManager.Player.transform);
                SetTargetLastSeen(_gameManager.PlayerPosition);
            }

            else
            {
                SetBehaviorTarget(target);
                SetTargetLastSeen(target.position);
            }

            if (StartBehavior != null)
                StartBehavior.Invoke();

            else if (IdleBehavior != null)
                IdleBehavior.Invoke();

        }

        void Update()
        {
            if (currentBehavior != Behavior.Dead)
            {
                if (_targetNearby)
                {
                    _prevAlertLevel = _currentAlertLevel;
                    _currentAlertLevel += GetAlertIncrement();
                }

                else
                {
                    _prevAlertLevel = _currentAlertLevel;
                    _currentAlertLevel = Mathf.Clamp(_currentAlertLevel - GameTime.deltaTime * AlertLevelCool, 0, MaxAlertLevel);
                }


                // Alert Level On the Rise
                if (_prevAlertLevel < _currentAlertLevel)
                {
                    if (_prevAlertLevel < ChaseAlertLevelThreshold && ChaseAlertLevelThreshold < _currentAlertLevel)
                    {
                        SetCurrentBehavior(Behavior.Chase);

                        if (ChaseBehavior != null)
                            ChaseBehavior.Invoke();
                    }

                    else if (_prevAlertLevel < SearchAlertLevelThreshold && SearchAlertLevelThreshold < _currentAlertLevel)
                    {
                        SetCurrentBehavior(Behavior.Search);

                        if (SearchBehavior != null)
                            SearchBehavior.Invoke();
                    }

                    else if (_prevAlertLevel < WatchAlertLevelThreshold && WatchAlertLevelThreshold < _currentAlertLevel)
                    {
                        SetCurrentBehavior(Behavior.Watch);

                        if (WatchBehavior != null)
                            WatchBehavior.Invoke();
                    }
                }

                // Alert Level On the Fall
                else
                {
                    if (_currentAlertLevel < ChaseAlertLevelThreshold && ChaseAlertLevelThreshold < _prevAlertLevel)
                    {
                        SetCurrentBehavior(Behavior.Search);

                        if (SearchBehavior != null)
                            SearchBehavior.Invoke();
                    }

                    else if (_currentAlertLevel < SearchAlertLevelThreshold && SearchAlertLevelThreshold < _prevAlertLevel)
                    {
                        SetCurrentBehavior(Behavior.Watch);

                        if (WatchBehavior != null)
                            WatchBehavior.Invoke();
                    }

                    else if (_currentAlertLevel < WatchAlertLevelThreshold && WatchAlertLevelThreshold < _prevAlertLevel)
                    {
                        SetCurrentBehavior(Behavior.Idle);

                        if (IdleBehavior != null)
                            IdleBehavior.Invoke();
                    }
                }
            }
        }

        #endregion

        #region Trigger Behaviors

        public void Roar()
        {

        }

        public void FakeDeath()
        {

        }

        public void RunAway()
        {
            status.isRunningAway = true;
        }

        #endregion

        public void OnHit(HitInfo[] hitInfos, WeaponStats stats) {

            if (currentBehavior == Behavior.Dead)
            {
                _animator.OnHit(hitInfos, stats, false);
                return;
            }

            float collectiveDamage = 0;

            foreach (HitInfo hitInfo in hitInfos)
            {
                bool headshot = hitInfo.hit.transform.name.ToLower().Contains("head");
                int multipliedDamage = headshot ? Mathf.FloorToInt(stats.damage * enemyCard.stats.headshotMultiplier) : stats.damage;
                collectiveDamage += multipliedDamage;
            }

            _prevHealth = _currentHealth;
            _currentHealth -= collectiveDamage;

            bool staggerTriggered = collectiveDamage > _staggerHealth || hitInfos.Length * stats.concussion > _staggerConcussion;
            bool isDead = _currentHealth <= 0;

            if(_statsUI != null)
                _statsUI.SetTargetFill(_currentHealth / _maxHealth);


            if (isDead)
            {
                _alertTrigger.enabled = false;
                SetCurrentBehavior(Behavior.Dead);
                _animator.OnKilled(hitInfos, stats);

                if (DeathEvent != null)
                    DeathEvent.Invoke();
            }

            else
            {
                if (_prevHealth == _maxHealth && _currentHealth < _maxHealth)
                {
                    if (FirstHitEvent != null)
                        FirstHitEvent.Invoke();
                }

                else if (_currentHealth < _criticalHealth && _criticalHealth <= _prevHealth)
                {
                    if (CriticalHealthEvent != null)
                        CriticalHealthEvent.Invoke();
                }

                else if (_currentHealth < _nearDeathHealth && _nearDeathHealth <= _prevHealth)
                {
                    if (NearDeathEvent != null)
                        NearDeathEvent.Invoke();
                }

                else
                {
                    if (EveryHitEvent != null)
                        EveryHitEvent.Invoke();
                }

                _animator.OnHit(hitInfos, stats, staggerTriggered);
            }
        }

        #region Alert Trigger

        public void SetBehaviorTarget(Transform target)
        {
            this.target = target;
            _animator.SetBehaviorTarget(target);
        }

        private void SetTargetLastSeen(Vector3 position)
        {
            _animator.SetTargetLastSeen(position);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform != target)
                return;

            _targetNearby = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.transform != target)
                return;

            SetTargetLastSeen(other.transform.position);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform != target)
                return;

            _targetNearby = false;
        }

        #endregion

        #region Helpers (Behaivor)

        bool TargetInSight(float fovAngle, float fovDistance)
        {
            Vector3 targetPosition = target.position;

            bool angle = Vector3.Dot(transform.forward, targetPosition - transform.position) > Mathf.Cos(fovAngle);

            Vector3 relDir = (targetPosition + transform.up) - (transform.position + transform.up);
            Ray ray = new Ray(transform.position + transform.up, relDir);

            bool sight = false;
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, fovDistance * 1.1f, ~(RayCastLayers.IgnoreRaycastLayer + RayCastLayers.EnemyLayer)))
            {
                Debug.DrawLine(transform.position + transform.up, hit.point, Color.red);

                if (hit.transform.root.tag == "Player")
                    sight = true;
            }

            return angle && sight;
        }

        float GetAlertIncrement()
        {
            return (GetTriggerRadiusWorldSpace() / Vector3.Distance(transform.position, target.position)) * enemyCard.alertness.noiseSensitivity * GameTime.deltaTime;
        }

        private void SetTriggerRadious(float radius)
        {
            _defaultTriggerRadius = radius;
            _currentTriggerRadius = _defaultTriggerRadius;
        }

        public void AdjustTriggerRadious(float multiplier)
        {
            _currentTriggerRadius = _defaultTriggerRadius * multiplier;
            _alertTrigger.radius = _currentTriggerRadius / transform.root.localScale.x;
        }

        public float GetTriggerRadiusWorldSpace()
        {
            return _currentTriggerRadius;
        }

        private void SetCurrentBehavior(Behavior behavior)
        {
            currentBehavior = behavior;
            SetXRayToBehavior(currentBehavior);
        }

        private void SetXRayToBehavior(Behavior behavior)
        {
            foreach (EnemyXrayRenderer renderer in _alertnessRenderers)
                renderer.SetXRayToBehavior(behavior);
        }

        #endregion
    }

    [System.Serializable]
    public class EnemyStatus
    {
        public bool isRaged = false;
        public bool isRunningAway = false;
        public bool isUndisturbable = false;
        public bool isInvincible = false;
        public bool isCrawling = false;
    }

    public enum EnemyBehavior: int
    {
        Idle = 0, 
        Watch,
        Search,
        Chase,
        Dead
    }
}


