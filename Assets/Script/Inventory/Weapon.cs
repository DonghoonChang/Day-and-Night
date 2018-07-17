using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyGame.Player;
using GameManager = MyGame.GameManagement.GameManager;
using SFXManager = MyGame.GameManagement.SFXManager;
using EnemyCharacter = MyGame.Enemy.EnemyCharacter;


namespace MyGame.Inventory.Weapon
{
    public abstract class Weapon : Item
    {
        protected SFXManager _sfxManager;
        protected GameManager _gameManager;
        protected UnityEvent _OnCharacterRenderingOver;

        protected Dictionary<EnemyCharacter, List<HitInfo>> hitTargets = new Dictionary<EnemyCharacter, List<HitInfo>>();

        protected bool _isAttackLocked = true;

        #region Properties

        // Animation
        public abstract WeaponProperties Properties
        {
            get;
        }

        public abstract WeaponStats Stats
        {
            get;
        }

        public abstract int Damage
        {
            get;
        }

        public abstract int FireRate
        {
            get;
        }

        public abstract float AttackDelay
        {
            get;
        }

        public bool IsAttackLocked
        {
            get
            {
                return _isAttackLocked;
            }
        }

        #endregion

        #region Awake and Updates

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            _sfxManager = SFXManager.Instance;
            _gameManager = GameManager.Instance;

            _OnCharacterRenderingOver = _gameManager.Player.GetComponent<PlayerAnimator>().OnAnimationRenderingOver;
        }

        #endregion

        #region Main Function

        protected virtual void AttackRoutine()
        {
            _isAttackLocked = true;
        }

        public virtual void ReleaseAttackLock()
        {
            _isAttackLocked = false;
        }

        #endregion
    }

    public class HitInfo
    {
        public Ray ray;
        public RaycastHit hit;

        public HitInfo(Ray ray, RaycastHit hit)
        {
            this.ray = ray;
            this.hit = hit;
        }
    }
}