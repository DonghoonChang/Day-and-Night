using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayCastLayers = MyGame.GameManagement.RayCastLayers;


namespace MyGame.Inventory.Weapon
{
    public class RangedWeapon : Weapon
    {
        static float sprayDeceleration = 2.5f;

        AudioSource dryAttackSound;
        AudioSource silencedAttackSound;

        [SerializeField]
        int currentAmmo;

        [SerializeField]
        float currentSpray;

        [SerializeField]
        bool isChockAttaced = false;

        Transform weaponFocus;


        #region Awake and Updates

        protected override void Awake()
        {
            base.Awake();

            dryAttackSound = gameObject.AddComponent<AudioSource>();
            silencedAttackSound = gameObject.AddComponent<AudioSource>();

            Sound.SoundtoSource(dryAttackSound, weaponCard.dryAttackSound);
            Sound.SoundtoSource(silencedAttackSound, weaponCard.dryAttackSound);
        }

        protected override void Start()
        {
            base.Start();

            weaponFocus = gameManager.PlayerCamera.WeaponFocus;
        }

        void Update()
        {
            currentSpray = Mathf.Lerp(currentSpray, weaponCard.stats.minSpray, sprayDeceleration * Time.deltaTime);
        }

        #endregion


        #region Main Function

        public override void Attack()
        {
            base.Attack();
        }

        protected override void AttackListener()
        {
            base.AttackListener();
            StartCoroutine("FireSFX");

            RaycastHit hit;
            Ray ray;

            for (int i = 0; i < Stats.pellet; i++)
            {
                ray = new Ray(muzzleTip.position, GetBulletDirection(currentSpray));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~RayCastLayers.IgnoreRaycastLayer, QueryTriggerInteraction.Ignore))
                {

                    string tag = hit.transform.tag;

                    if (tag == "Enemy")
                    {
                        Enemy.Enemy hitEnemy = hit.transform.root.GetComponent<Enemy.Enemy>();

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
                        GameObject bulletImpactConcrete = Instantiate(sfxManager.bulletImpactConcrete, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Metal")
                    {
                        GameObject bulletImpactConcrete = Instantiate(sfxManager.bulletImpactMetal, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Wood")
                    {
                        GameObject bulletImpactConcrete = Instantiate(sfxManager.bulletImpactWood, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Sand")
                    {
                        GameObject bulletImpactConcrete = Instantiate(sfxManager.bulletImpactSand, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else if (tag == "Water")
                    {
                        GameObject bulletImpactConcrete = Instantiate(sfxManager.bulletImpactWater, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                    else
                    {
                        GameObject bulletImpactConcrete = Instantiate(sfxManager.bulletImpactWood, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(bulletImpactConcrete, gameManager.GraphicsConfiguration.bulletMarkLifeTime);
                    }

                }
            }

            foreach (Enemy.Enemy enemy in hitTargets.Keys)
            {
                enemy.OnHit(hitTargets[enemy].ToArray(), Stats);
            }

            currentSpray = Mathf.Clamp(currentSpray + weaponCard.stats.sprayAcceleration, weaponCard.stats.minSpray, weaponCard.stats.maxSpray);
            hitTargets.Clear();
        }

        public override void ReleaseFireLock()
        {
            isAttackLocked = false;
        }

        Vector3 GetBulletDirection(float spray)
        {
            float randSpray = Random.Range(0, spray);
            float randRotation = Random.Range(0, 360f);

            Vector3 bulletNormal = (weaponFocus.position - muzzleTip.position).normalized;
            Vector3 bulletSpray = bulletNormal + Mathf.Tan(randSpray * (2 * Mathf.PI) / 360f) * muzzleTip.up; /* Times current spray */;
            bulletSpray = Quaternion.AngleAxis(randRotation, bulletNormal) * bulletSpray;

            return bulletSpray;
        }

        IEnumerator FireSFX()
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

        #endregion
    }
}

