using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyGame.Player;
using MyGame.Interface.ITakeHit;
using GameManager = MyGame.GameManagement.GameManager;
using VFXManager = MyGame.GameManagement.VFXManager;


namespace MyGame.Object
{
    public abstract class PlayerWeapon : Item
    {
        protected VFXManager _vfxManager;
        protected GameManager _gameManager;
        protected UnityEvent _OnCharacterRenderingOver;

        protected bool _isAttackLocked = false;

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

        public abstract int DamagaPerPellet
        {
            get;
        }

        public abstract int RateOfFire
        {
            get;
        }

        public abstract float AttackDelay
        {
            get;
        }

        public bool AttackLocked
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

            _vfxManager = VFXManager.Instance;
            _gameManager = GameManager.Instance;

            _OnCharacterRenderingOver = _gameManager.Player.GetComponent<PlayerAnimator>().OnPostureAdjustmentOver;
        }

        #endregion

        #region Main Function

        public virtual void LockAttack()
        {
            _isAttackLocked = true;
        }

        public virtual void ReleaseAttackLock()
        {
            _isAttackLocked = false;
        }

        protected virtual void InstantiateHitImpact(string tag, Transform hitTF, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (tag == "Concrete")
            {
                GameObject bulletImpactConcrete = Instantiate(_vfxManager.bulletImpactConcrete, hitPoint, Quaternion.LookRotation(hitNormal));
                bulletImpactConcrete.transform.SetParent(hitTF);
                Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
            }

            else if (tag == "Metal")
            {
                GameObject bulletImpactConcrete = Instantiate(_vfxManager.bulletImpactMetal, hitPoint, Quaternion.LookRotation(hitNormal));
                bulletImpactConcrete.transform.SetParent(hitTF);
                Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
            }

            else if (tag == "Wood")
            {
                GameObject bulletImpactConcrete = Instantiate(_vfxManager.bulletImpactWood, hitPoint, Quaternion.LookRotation(hitNormal));
                bulletImpactConcrete.transform.SetParent(hitTF);
                Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
            }

            else if (tag == "Sand")
            {
                GameObject bulletImpactConcrete = Instantiate(_vfxManager.bulletImpactSand, hitPoint, Quaternion.LookRotation(hitNormal));
                bulletImpactConcrete.transform.SetParent(hitTF);
                Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
            }

            else if (tag == "Water")
            {
                GameObject bulletImpactConcrete = Instantiate(_vfxManager.bulletImpactWater, hitPoint, Quaternion.LookRotation(hitNormal));
                bulletImpactConcrete.transform.SetParent(hitTF);
                Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
            }

            else
            {
                GameObject bulletImpactConcrete = Instantiate(_vfxManager.bulletImpactWood, hitPoint, Quaternion.LookRotation(hitNormal));
                bulletImpactConcrete.transform.SetParent(hitTF);
                Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
            }
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