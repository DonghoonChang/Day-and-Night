using UnityEngine;

namespace Game.Object
{
    public class MeleeWeapon : PlayerWeapon
    {
        [SerializeField]
        MeleeWeaponCard weaponCard;

        WeaponStats _stats;
        WeaponProperties _props;

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

        public override int DamagaPerPellet
        {
            get
            {
                return weaponCard.stats.damagePerPellet;
            }
        }

        public override int RateOfFire
        {
            get
            {
                return weaponCard.stats.rateOfFire;
            }
        }

        public override float AttackDelay
        {
            get
            {
                return 60f / Stats.rateOfFire;
            }
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();


        }

        protected override void Start()
        {
            base.Start();
        }

        public void Attack()
        {

        }
    }
}

