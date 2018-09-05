using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Game.Interface.ITakeHit;
using Behavior = Game.Enemy.EnemyBehavior;
using UIFloatingStats = Game.UI.UIFloatingStats;
using EnemyXrayRenderer = Game.VFX.EnemyXrayRenderer;
using GameManager = Game.GameManagement.GameManager;
using RaycastLayers = Game.GameManagement.RaycastLayers;

namespace Game.Enemy
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(EnemyAnimator))]
    [RequireComponent(typeof(EnemyInventory))]
    [RequireComponent(typeof(EnemyAudioPlayer))]
    [RequireComponent(typeof(EnemyRagdollMapperRoot))]
    public class EnemyCharacter : MonoBehaviour, ITakeHit
    {

        #region Static Variables

        static float MaxAlertLevel = 50f;
        static float AlertLevelCool = 2.5f;

        static float WatchAlertLevelThreshold = 4f;
        static float SearchAlertLevelThreshold = 10f;
        static float ChaseAlertLevelThreshold = 20f;

        static float TargetInFOVAlertMultiplier = 5f;
        static float AlertIncrementPerDamage = 1f / 5f;

        #endregion

        GameManager _gameManager;
        UIFloatingStats _statsUI;
        EnemyXrayRenderer[] _alertnessRenderers;

        EnemyAnimator _animator;
        EnemyInventory _inventory;

        [SerializeField]
        EnemyStatsCard _enemyCard;

        EnemyStats _stats;
        EnemyAlertness _alertness;

        SphereCollider _alertTrigger;
        float _defaultTriggerRadius;
        float _currentTriggerRadius;

        float _maxHealth;
        float _prevHealth;
        float _currentHealth;

        [SerializeField] bool _targetInFOV = false;
        [SerializeField] bool _targetNearby = false;

        [SerializeField]
        float _prevAlertLevel = 0f;
        float _currentAlertLevel = 0f;
        float _damageThisFrame = 0f;

        public Transform target;
        public Behavior currentBehavior = Behavior.Idle;
        public EnemyStatus status = new EnemyStatus();

        #region Behaviors and Events

        [SerializeField] UnityEvent StartBehavior;
        [SerializeField] UnityEvent IdleBehavior;
        [SerializeField] UnityEvent WatchBehavior;
        [SerializeField] UnityEvent SearchBehavior;
        [SerializeField] UnityEvent ChaseBehavior;

        [SerializeField] UnityEvent OnPlayerFirstSeenEvent;
        [SerializeField] UnityEvent OnPlayerLostEvent;
        bool _playerFirstSeenEventTriggered = false;

        [SerializeField] UnityEvent FirstHitEvent;
        [SerializeField] UnityEvent EveryHitEvent;

        [SerializeField] UnityEvent CriticalHealthEvent;
        [SerializeField] UnityEvent NearDeathEvent;
        [SerializeField] UnityEvent DeathEvent;

        #endregion

        #region Properties

        public float Damage
        {
            get
            {
                return _stats.damage;
            }
        }

        public float FieldOfViewAngle
        {
            get
            {
                return _enemyCard.alertness.fieldOfViewAngle;
            }
        }

        public float FieldOfViewDistance
        {
            get
            {
                return _enemyCard.alertness.fieldOfViewDistance;
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
            _statsUI = GetComponentInChildren<UIFloatingStats>();
            _alertnessRenderers = GetComponentsInChildren<EnemyXrayRenderer>();

            _animator = GetComponent<EnemyAnimator>();
            _inventory = GetComponent<EnemyInventory>();
            _alertTrigger = GetComponent<SphereCollider>();

            _stats = _enemyCard.stats;
            _alertness = _enemyCard.alertness;
            _currentHealth = _prevHealth = _maxHealth = _stats.health;

            _alertTrigger.isTrigger = true;
            SetTriggerRadious(_alertness.alertTriggerRadius);
            AdjustTriggerRadious(1f);

            if (_statsUI != null)
                _statsUI.SetTargetFill(1f);

        }

        void Start()
        {
            _gameManager = GameManager.Instance;
            _gameManager.AddEnemy(this);

            if (target == null)
            {
                SetBehaviorTarget(GameManager.Instance.Player.transform);
                SetTargetLastSeen(GameManager.Instance.Player.transform.position);
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
                _targetInFOV = TargetInFOV(_alertness.fieldOfViewAngle, _alertness.fieldOfViewDistance);

                if (_targetInFOV
                    && !_playerFirstSeenEventTriggered
                    && OnPlayerFirstSeenEvent != null)
                {
                    _playerFirstSeenEventTriggered = true;
                    OnPlayerFirstSeenEvent.Invoke();
                }

                _prevAlertLevel = _currentAlertLevel;

                if (_targetNearby)
                    _currentAlertLevel = Mathf.Min(_currentAlertLevel + GetAlertIncrement(), MaxAlertLevel);

                else
                    _currentAlertLevel = Mathf.Clamp(_currentAlertLevel - GameTime.deltaTime * AlertLevelCool + GetAlertIncrement(), 0, MaxAlertLevel);

                _damageThisFrame = 0;

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

        public void Roar(int intensity)
        {
            _animator.Roar(intensity);
        }

        public void RunAway()
        {
            status.isRunningAway = true;
        }

        public void OnHit(Transform[] transformList, Vector3[] normalList, float damage, float concussion, bool applyDamageOnce)
        {
            if (transformList.Length != normalList.Length)
                return;

            if (currentBehavior == Behavior.Dead)
            {
                _animator.OnHit(transformList, normalList, false);
                return;
            }

            float totalDamage = 0f;

            if (applyDamageOnce)
                totalDamage = damage;

            else
            {
                foreach (Transform tf in transformList)
                {
                    // bool headshot = hitInfo.hit.transform.name.ToLower().Contains("head");
                    // int damage = headshot ? Mathf.FloorToInt(stats.damagePerPellet * _enemyCard.stats.headshotMultiplier) : stats.damagePerPellet;
                    totalDamage += damage;
                }
            }

            _prevHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - totalDamage);
            _damageThisFrame += totalDamage;

            bool staggered = totalDamage > _maxHealth * _stats.staggerHealthMultiplier 
                             || transformList.Length * concussion > _stats.staggerConcussionThreshold;

            _animator.OnHit(transformList, normalList, staggered);

            if (_statsUI != null)
                _statsUI.SetTargetFill(_currentHealth / _maxHealth);


            // Health Based Events
            if (_currentHealth == 0)
            {
                SetCurrentBehavior(Behavior.Dead);

                _animator.OnKilled();
                _gameManager.RemoveEnemy(this);

                _alertTrigger.enabled = false;

                if (_inventory != null)
                    _inventory.DropLoots();

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

                else if (_currentHealth < _maxHealth * _stats.criticalHealthMultiplier && _maxHealth * _stats.criticalHealthMultiplier <= _prevHealth)
                {
                    if (CriticalHealthEvent != null)
                        CriticalHealthEvent.Invoke();
                }

                else if (_currentHealth < _maxHealth * _stats.nearDeathHealthMultiplier && _maxHealth * _stats.nearDeathHealthMultiplier <= _prevHealth)
                {
                    if (NearDeathEvent != null)
                        NearDeathEvent.Invoke();
                }

                else
                {
                    if (EveryHitEvent != null)
                        EveryHitEvent.Invoke();
                }
            }
        }

        #endregion

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
            if (other.isTrigger)
                return;

            if (other.transform != target)
                return;

            _targetNearby = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.isTrigger)
                return;

            if (other.transform != target)
                return;

            SetTargetLastSeen(other.transform.position);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger)
                return;

            if (other.transform != target)
                return;

            _targetNearby = false;
        }

        #endregion

        #region Helpers (Behaivor)

        bool TargetInFOV(float fovAngle, float fovDistance)
        {
            Vector3 targetPosition = target.position;

            bool angle = Vector3.Dot(transform.forward, targetPosition - transform.position) > Mathf.Cos(fovAngle);

            Vector3 relDir = (targetPosition + transform.up) - (transform.position + transform.up);
            Ray ray = new Ray(transform.position + transform.up, relDir);

            bool sight = false;
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, fovDistance * 1.1f, ~(RaycastLayers.IgnoreRaycastLayer + RaycastLayers.EnemyLayer)))
            {
                Debug.DrawLine(transform.position + transform.up, hit.point, Color.red);

                if (hit.transform.root.tag == "Player")
                    sight = true;
            }

            return angle && sight;
        }

        float GetAlertIncrement()
        {
            return (_currentTriggerRadius / Vector3.Distance(transform.position, target.position)) // How Close
                   * (_targetInFOV ? TargetInFOVAlertMultiplier : 1f) // Target In FOV
                   * _enemyCard.alertness.noiseSensitivity // How Sensitive to Noise
                   * GameTime.deltaTime
                   + _damageThisFrame * AlertIncrementPerDamage;
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
    public struct EnemyStatus
    {
        public bool isRaged;
        public bool isCrawling;
        public bool isRunningAway;
        public bool isUndisturbable;
        public bool isInvincible;
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


