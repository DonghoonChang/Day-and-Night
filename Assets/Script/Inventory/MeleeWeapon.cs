using UnityEngine;

namespace MyGame.Inventory.Weapon
{
    public class MeleeWeapon : Weapon
    {
        [SerializeField]
        MeleeWeaponCard weaponCard;

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
                return weaponCard.stats.damage;
            }
        }

        public override int FireRate
        {
            get
            {
                return weaponCard.stats.fireRate;
            }
        }

        public override float AttackDelay
        {
            get
            {
                return 60f / Stats.fireRate;
            }
        }

        #endregion

        public void Attack()
        {

        }
    }
}

