using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyGame.Player;
using GameManager = MyGame.GameManagement.GameManager;
using SFXManager = MyGame.GameManagement.SFXManager;


namespace MyGame.Inventory.Weapon
{
    public abstract class Weapon : Item
    {
        protected GameManager gameManager;
        protected SFXManager sfxManager;
        protected UnityEvent OnAnimationRenderingOver;

        [SerializeField]
        protected WeaponCard weaponCard;

        [SerializeField]
        protected Transform muzzleTip;

        [SerializeField]
        protected Transform cartridgeOutlet;

        protected AudioSource AttackSound;

        protected bool isAttackLocked = true;

        protected Dictionary<Enemy.Enemy, List<HitInfo>> hitTargets = new Dictionary<Enemy.Enemy, List<HitInfo>>();

        #region Properties

        public WeaponProperties Properties
        {
            get
            {
                if (weaponCard != null)
                    return weaponCard.properties;

                else
                    return null;
            }
        }

        public WeaponStats Stats
        {
            get
            {
                if (weaponCard != null)
                    return weaponCard.stats;

                else
                    return null;
            }
        }

        float AttackDelay
        {
            get
            {
                return 60f / Stats.fireRate;
            }
        }

        public bool IsAttackLocked
        {
            get
            {
                return isAttackLocked;
            }
        }

        #endregion


        #region Awake and Updates

        protected virtual void Awake()
        {
            AttackSound = gameObject.AddComponent<AudioSource>();
            Sound.SoundtoSource(AttackSound, weaponCard.attackSound);
        }

        protected virtual void Start()
        {
            gameManager = GameManager.Instance;
            sfxManager = SFXManager.Instance;

            OnAnimationRenderingOver = gameManager.Player.GetComponent<PlayerAnimator>().OnAnimationRenderingOver;
        }

        #endregion


        #region Main Function

        public virtual void Attack()
        {
            OnAnimationRenderingOver.AddListener(AttackListener);
        }

        protected virtual void AttackListener()
        {
            isAttackLocked = true;
            Invoke("ReleaseFireLock", AttackDelay);
            AttackSound.Play();
        }

        public virtual void ReleaseFireLock()
        {
            CancelInvoke("ReleaseFireLock");
            isAttackLocked = false;
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