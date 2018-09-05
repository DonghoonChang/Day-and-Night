using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interface.ITakeHit;
using CameraManager = Game.GameManagement.CameraManager;
using RaycastLayers = Game.GameManagement.RaycastLayers;
using PlayerCamera = Game.Player.PlayerCamera;


namespace Game.Object
{
    public class RangedWeapon : PlayerWeapon
    {
        [SerializeField] Transform _muzzleTip;
        [SerializeField] Transform _cartridgeOutlet;
        [SerializeField] RangedWeaponCard _weaponCard;

        PlayerCamera _playerCam;
        Transform _aimPoint;

        WeaponStats _stats;
        WeaponProperties _props;

        AudioSource _attackSound;
        AudioSource _dryAttackSound;
        AudioSource _silencedAttackSound;

        [SerializeField] int _currentAmmo = 0;
        [SerializeField] float _currentSpread = 0f;

        public RangedWeaponModifications modifications;
        public RangedWeaponModificationEffects modificationEffects;

        #region Properties

        public override WeaponStats Stats
        {
            get
            {
                return _stats;
            }
        }

        public override WeaponProperties Properties
        {
            get
            {
                return _props;
            }
        }

        public override string Name
        {
            get
            {
                return transform.name;
            }
        }

        public override string Description
        {
            get
            {
                return _weaponCard.description;
            }
        }

        public override int DamagaPerPellet
        {
            get
            {
                return _stats.damagePerPellet;
            }
        }

        public override int RateOfFire
        {
            get
            {
                return _stats.rateOfFire;
            }
        }

        public override float AttackDelay
        {
            get
            {
                return 60f / RateOfFire;
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
                return _stats.spreadStep;
            }
        }

        public float MinSpread
        {
            get
            {
                return _stats.minSpread;
            }
        }

        public float MaxSpread
        {
            get
            {
                return _stats.maxSpread;
            }
        }

        public bool IsAutomatic
        {
            get
            {
                return _props.isAutomatic;
            }
        }

        public int MagazineCapacity
        {
            get
            {
                return _stats.magazineCapacity;
            }
        }

        #endregion

        #region Awake and Updates

        protected override void Awake()
        {
            base.Awake();

            _stats = _weaponCard.stats;
            _props = _weaponCard.properties;

            if (_weaponCard.attackSound != null)
            {
                _attackSound = gameObject.AddComponent<AudioSource>();
                Sound.SoundtoSource(_attackSound, _weaponCard.attackSound);
            }

            if (_weaponCard.dryAttackSound != null)
            {
                _dryAttackSound = gameObject.AddComponent<AudioSource>();
                Sound.SoundtoSource(_dryAttackSound, _weaponCard.dryAttackSound);
            }

            if (_weaponCard.silencedAttackSound != null)
            {
                _silencedAttackSound = gameObject.AddComponent<AudioSource>();
                Sound.SoundtoSource(_silencedAttackSound, _weaponCard.silencedAttackSound);
            }

        }

        protected override void Start()
        {
            base.Start();

            _playerCam = CameraManager.Instance.PlayerCamera;
            _aimPoint = _playerCam.AimPoint;
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

            LockAttack();
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

        protected void AttackRoutine()
        {
            _attackSound.Play();
            StartCoroutine("FireVFX");

            HashSet<ITakeHit> hitSet = new HashSet<ITakeHit>();
            Dictionary<ITakeHit, List<Transform>> transformListDic = new Dictionary<ITakeHit, List<Transform>>();
            Dictionary<ITakeHit, List<Vector3>> normalListDic = new Dictionary<ITakeHit, List<Vector3>>();

            // Penetrating Effect
            if (modificationEffects.isPenetrating)
            {
                Ray environmentRay;
                RaycastHit environmentHit;

                for (int i = 0; i < Stats.pelletsPerShot; i++)
                {
                    // Looks For Any Walls Blocking the Ray
                    environmentRay = GetBulletSpreadRay(_currentSpread);

                    // If There Is
                    if (Physics.Raycast(environmentRay, out environmentHit, 100f, RaycastLayers.EnvironmentLayer, QueryTriggerInteraction.Ignore))
                    {

                        float distance = Vector3.Distance(_playerCam.transform.position, environmentHit.point);

                        RaycastHit[] hits = Physics.RaycastAll(environmentRay, distance * 1.01f, RaycastLayers.BulletLayer, QueryTriggerInteraction.Ignore);

                        if (hits.Length > 0)
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                ITakeHit hitTarget = hit.transform.root.GetComponent<ITakeHit>();

                                if (hitTarget != null)
                                {
                                    if (hitSet.Contains(hitTarget))
                                    {
                                        transformListDic[hitTarget].Add(hit.transform);
                                        normalListDic[hitTarget].Add(hit.normal);
                                    }

                                    else
                                    {
                                        hitSet.Add(hitTarget);
                                        transformListDic[hitTarget] = new List<Transform> { hit.transform };
                                        normalListDic[hitTarget] = new List<Vector3> { hit.normal };
                                    }
                                }

                                else
                                {
                                    InstantiateHitImpact(hit.transform.tag, hit.transform, hit.point, hit.normal);
                                }

                                // Bullet Push
                                Rigidbody rb = hit.rigidbody;

                                if (rb != null)
                                {
                                    rb.AddForceAtPosition(environmentRay.direction.normalized * _stats.concussionPerPellet, hit.point, ForceMode.VelocityChange);
                                    rb.AddTorque(environmentRay.direction.normalized * _stats.concussionPerPellet * 2, ForceMode.VelocityChange);
                                }
                            }
                        }
                    }

                    // If There Is Not
                    else
                    {
                        Debug.Log("Did not hit wall");

                        RaycastHit[] hits = Physics.RaycastAll(environmentRay, 100f, (RaycastLayers.BulletLayer & ~RaycastLayers.EnvironmentLayer), QueryTriggerInteraction.Ignore);

                        if (hits.Length > 0)
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                string hitTag = hit.transform.tag;
                                ITakeHit hitTarget = hit.transform.root.GetComponent<ITakeHit>();

                                if (hitTarget != null)
                                {
                                    if (hitSet.Contains(hitTarget))
                                    {
                                        transformListDic[hitTarget].Add(hit.transform);
                                        normalListDic[hitTarget].Add(hit.normal);
                                    }

                                    else
                                    {
                                        hitSet.Add(hitTarget);
                                        transformListDic[hitTarget] = new List<Transform> { hit.transform };
                                        normalListDic[hitTarget] = new List<Vector3> { hit.normal };
                                    }
                                }

                                else
                                {
                                    InstantiateHitImpact(hit.transform.tag, hit.transform, hit.point, hit.normal);
                                }

                                // Bullet Push
                                Rigidbody rb = hit.rigidbody;

                                if (rb != null)
                                {
                                    rb.AddForceAtPosition(environmentRay.direction.normalized * _stats.concussionPerPellet, hit.point, ForceMode.VelocityChange);
                                    rb.AddTorque(environmentRay.direction.normalized * _stats.concussionPerPellet * 2, ForceMode.VelocityChange);
                                }
                            }
                        }
                    }
                }

                foreach (ITakeHit hitTarget in hitSet)
                {
                    Transform[] transformArray = transformListDic[hitTarget].ToArray();
                    Vector3[] normalArray = normalListDic[hitTarget].ToArray();

                    if (transformArray.Length != normalArray.Length)
                        continue;

                    else
                        hitTarget.OnHit(transformArray, normalArray, _stats.damagePerPellet, _stats.concussionPerPellet, false);
                }
            }

            else
            {
                Ray ray;
                RaycastHit hit;

                for (int i = 0; i < Stats.pelletsPerShot; i++)
                {
                    ray = GetBulletSpreadRay(_currentSpread);

                    if (Physics.Raycast(ray, out hit, 100f, RaycastLayers.BulletLayer, QueryTriggerInteraction.Ignore))
                    {
                        ITakeHit hitTarget = hit.transform.root.GetComponent<ITakeHit>();

                        if (hitTarget != null)
                        {
                            if (hitSet.Contains(hitTarget))
                            {
                                transformListDic[hitTarget].Add(hit.transform);
                                normalListDic[hitTarget].Add(hit.normal);
                            }

                            else
                            {
                                hitSet.Add(hitTarget);
                                transformListDic[hitTarget] = new List<Transform> { hit.transform };
                                normalListDic[hitTarget] = new List<Vector3> { hit.normal };
                            }
                        }

                        else
                        {
                            InstantiateHitImpact(hit.transform.tag, hit.transform, hit.point, hit.normal);
                        }

                        // Bullet Push
                        Rigidbody rb = hit.rigidbody;

                        if (rb != null)
                        {
                            rb.AddForceAtPosition(ray.direction.normalized * _stats.concussionPerPellet, hit.point, ForceMode.VelocityChange);
                            rb.AddTorque(ray.direction.normalized * _stats.concussionPerPellet * 2, ForceMode.VelocityChange);
                        }
                    }
                }

                foreach (ITakeHit hitTarget in hitSet)
                {
                    Transform[] transformArray = transformListDic[hitTarget].ToArray();
                    Vector3[] normalArray = normalListDic[hitTarget].ToArray();

                    if (transformArray.Length != normalArray.Length)
                        continue;

                    else
                        hitTarget.OnHit(transformArray, normalArray, _stats.damagePerPellet, _stats.concussionPerPellet, false);
                }
            }
        }

        protected void DryAttackRoutine()
        {
            _dryAttackSound.Play();
        }

        Ray GetBulletSpreadRay(float spread)
        {
            float randSpread = Random.Range(0, spread);
            float randRotation = Random.Range(0, 360f);

            Vector3 bulletDirNoSpread = (_aimPoint.position - _playerCam.MainCamera.transform.position).normalized;
            Vector3 bulletDir = bulletDirNoSpread + Mathf.Tan(randSpread * (2 * Mathf.PI) / 360f) * _playerCam.MainCamera.transform.up;
            bulletDir = Quaternion.AngleAxis(randRotation, bulletDirNoSpread) * bulletDir;

            return new Ray(_playerCam.transform.position, bulletDir);
        }

        IEnumerator FireVFX()
        {
            yield return new WaitForEndOfFrame();

            if (_weaponCard.muzzleFlashSFX != null)
            {
                GameObject muzzleFlash = Instantiate(_weaponCard.muzzleFlashSFX, _muzzleTip.transform, false);
                muzzleFlash.transform.localPosition = Vector3.zero;
                muzzleFlash.transform.rotation = _muzzleTip.transform.rotation;
            }

            if (_weaponCard.cartridgeSFX != null)
            {
                Instantiate(_weaponCard.cartridgeSFX, _cartridgeOutlet.position, _cartridgeOutlet.rotation);
            }
        }

        #endregion
    }

    [System.Serializable]
    public struct RangedWeaponModifications
    {
        public bool isMuzzleModified;
        public bool isScopeModified;
        public bool isBarrelModified;
    }

    [System.Serializable]
    public struct RangedWeaponModificationEffects
    {
        public bool isScoped;
        public bool isSilenced;
        public bool isPenetrating;
        public bool isMagazineExtended;
    }
}

