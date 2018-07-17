using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayCastLayers = MyGame.GameManagement.RayCastLayers;
using EnemyCharacter = MyGame.Enemy.EnemyCharacter;


namespace MyGame.Inventory.Weapon
{
    public class RangedWeapon : Weapon
    {
        public RangedWeaponModifications modifications;

        [SerializeField]
        RangedWeaponCard weaponCard;

        public Transform muzzleTip;
        public Transform cartridgeOutlet;

        Transform _weaponFocus;

        AudioSource _attackSound;
        AudioSource _dryAttackSound;
        AudioSource _silencedAttackSound;

        [SerializeField]
        int _currentAmmo = 0;
        float _currentSpread = 0f;

        #region Weapon Stats

        int _damage;
        int _concussion;
        int _magazineCapacity;
        int _fireRate;
        int _pellet;
        float _spreadStep;
        float _minSpread;
        float _maxSpread;

        #endregion

        #region Properties

        // Animation
        public override WeaponProperties Properties
        {
            get
            {
                if (weaponCard != null)
                    return weaponCard.properties;

                else
                    return null;
            }
        }

        public override WeaponStats Stats
        {
            get
            {
                if (weaponCard != null)
                    return weaponCard.stats;

                else
                    return null;
            }
        }

        // UI + Game Logic
        public override Sprite Icon
        {
            get
            {
                return weaponCard.icon;
            }
        }

        public override string Name
        {
            get
            {
                return weaponCard.name;
            }
        }

        public override string Description
        {
            get
            {
                return weaponCard.description;
            }
        }

        public override int Damage
        {
            get
            {
                return _damage;
            }
        }

        public override int FireRate
        {
            get
            {
                return _fireRate;
            }
        }

        public override float AttackDelay
        {
            get
            {
                return 60f / _fireRate;
            }
        }

        public int MagazineCapacity
        {
            get
            {
                return _magazineCapacity;
            }
        }

        public int CurrentAmmo
        {
            get
            {
                return _currentAmmo;
            }

            set
            {
                _currentAmmo = Mathf.Max(0, value);
            }
        }

        public float SpreadStep
        {
            get
            {
                return _spreadStep;
            }
        }

        public float MinSpread
        {
            get
            {
                return _minSpread;
            }
        }

        public float MaxSpread
        {
            get
            {
                return _maxSpread;
            }
        }

        public bool IsAutomatic
        {
            get
            {
                return weaponCard.properties.isAutomatic;
            }
        }

        #endregion

        #region Awake and Updates

        protected override void Awake()
        {
            base.Awake();

            _damage = weaponCard.stats.damage;
            _concussion = weaponCard.stats.concussion;
            _magazineCapacity = weaponCard.stats.magazineCapacity;
            _fireRate = weaponCard.stats.fireRate;
            _pellet = weaponCard.stats.pellet;
            _spreadStep = weaponCard.stats.spreadStep;
            _minSpread = weaponCard.stats.minSpread;
            _maxSpread = weaponCard.stats.maxSpread;

            _attackSound = gameObject.AddComponent<AudioSource>();
            _dryAttackSound = gameObject.AddComponent<AudioSource>();
            _silencedAttackSound = gameObject.AddComponent<AudioSource>();

            Sound.SoundtoSource(_attackSound, weaponCard.attackSound);
            Sound.SoundtoSource(_dryAttackSound, weaponCard.dryAttackSound);
            Sound.SoundtoSource(_silencedAttackSound, weaponCard.silencedAttackSound);
        }

        protected override void Start()
        {
            base.Start();
            _weaponFocus = _gameManager.PlayerCamera.AimPoint;
        }

        protected virtual void OnEnable()
        {
            _currentSpread = 0f;
        }

        #endregion

        #region Main Function

        public void Attack(float spread, bool dry)
        {
            _currentSpread = spread;
            CancelInvoke("ReleaseAttackLock");
            Invoke("ReleaseAttackLock", AttackDelay);

            if (dry)
            {
                _OnCharacterRenderingOver.AddListener(DryAttackRoutine);

            }

            else
            {
                _currentAmmo--;
                _OnCharacterRenderingOver.AddListener(AttackRoutine);
            }
        }

        protected override void AttackRoutine()
        {
            base.AttackRoutine();
            _attackSound.Play();

            StartCoroutine("FireVFX");

            RaycastHit hit;
            Ray ray;

            for (int i = 0; i < Stats.pellet; i++)
            {
                ray = new Ray(muzzleTip.position, GetBulletDirection(_currentSpread));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~RayCastLayers.IgnoreRaycastLayer, QueryTriggerInteraction.Ignore))
                {

                    string tag = hit.transform.tag;

                    if (tag == "Enemy")
                    {
                        EnemyCharacter hitEnemy = hit.transform.root.GetComponent<EnemyCharacter>();

                        if (hitTargets.ContainsKey(hitEnemy))
                        {
                            hitTargets[hitEnemy].Add(new HitInfo(ray, hit));
                        }

                        else
                        {
                            hitTargets[hitEnemy] = new List<HitInfo>
                                {
                                    new HitInfo(ray, hit)
                                };
                        }
                    }

                    else if (tag == "Concrete")
                    {
                        GameObject bulletImpactConcrete = Instantiate(_sfxManager.bulletImpactConcrete, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Metal")
                    {
                        GameObject bulletImpactConcrete = Instantiate(_sfxManager.bulletImpactMetal, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Wood")
                    {
                        GameObject bulletImpactConcrete = Instantiate(_sfxManager.bulletImpactWood, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Sand")
                    {
                        GameObject bulletImpactConcrete = Instantiate(_sfxManager.bulletImpactSand, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Water")
                    {
                        GameObject bulletImpactConcrete = Instantiate(_sfxManager.bulletImpactWater, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else
                    {
                        GameObject bulletImpactConcrete = Instantiate(_sfxManager.bulletImpactWood, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, _gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                }
            }

            foreach (EnemyCharacter enemy in hitTargets.Keys)
                enemy.OnHit(hitTargets[enemy].ToArray(), Stats);

            hitTargets.Clear();
        }

        protected void DryAttackRoutine()
        {
            base.AttackRoutine();
            _dryAttackSound.Play();
        }

        Vector3 GetBulletDirection(float spray)
        {
            float randSpray = Random.Range(0, spray);
            float randRotation = Random.Range(0, 360f);

            Vector3 bulletNormal = (_weaponFocus.position - muzzleTip.position).normalized;
            Vector3 bulletSpray = bulletNormal + Mathf.Tan(randSpray * (2 * Mathf.PI) / 360f) * muzzleTip.up; /* Times current spray */;
            bulletSpray = Quaternion.AngleAxis(randRotation, bulletNormal) * bulletSpray;

            return bulletSpray;
        }

        IEnumerator FireVFX()
        {
            yield return null;

            if (weaponCard.muzzleFlashSFX != null)
            {
                GameObject muzzleFlash = Instantiate(weaponCard.muzzleFlashSFX, muzzleTip.transform, false);
                muzzleFlash.transform.localPosition = Vector3.zero;
                muzzleFlash.transform.rotation = muzzleTip.transform.rotation;
            }

            if (weaponCard.cartridgeSFX != null)
            {
                GameObject cartridge = Instantiate(weaponCard.cartridgeSFX, cartridgeOutlet.position, cartridgeOutlet.rotation);
            }
        }

        public bool IsMagazineEmpty()
        {
            return _currentAmmo == 0;
        }

        #endregion
    }

    [System.Serializable]
    public class RangedWeaponModifications
    {
        public bool isMuzzleModified = false;
        public bool isScopeModified = false;
        public bool isBarrelModified = false;
    }
}

