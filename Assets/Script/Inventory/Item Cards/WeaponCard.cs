using UnityEngine;

namespace MyGame.Inventory.Weapon
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/WeaponCard")]
    public class WeaponCard : ItemCard
    {

        public GameObject muzzleFlashSFX;
        public GameObject cartridgeSFX;
        public Sound attackSound;
        public Sound dryAttackSound;

        public WeaponProperties properties;
        public WeaponStats stats;
    }

    [System.Serializable]
    public class WeaponProperties
    {
        /*
         * Animation Related
         */

        public WeaponGroup group;
        public WeaponType type;
        public AmmoType ammo;

        /* Scope & Laser Sight */
        public bool isTopModifiable;

        /* Silencer & Bayonet */
        public bool isMuzzleModifiable;

        /* Grenade Launcher and Lights(UV exposing Enemy WeakSpot, or Just Flashlight) */
        public bool isBarrelModifiable;

        /* Automatic */
        public bool isAutomatic;
    }

    [System.Serializable]
    public class WeaponStats
    {
        /* Damage per Pellet*/
        public int damage;

        /* Physical Push Force */
        public int concussion;

        /* Number of Shots Per Reload*/
        public int magazineCapacity;

        /* # of Firing per Min *
        /* Max : 1200 (Exclusive)*/
        public int fireRate;

        /* # of Pellets per Fire *
        /* Example : 8+ for Shotguns*/
        public int pellet = 1;

        /* Weapon Bullet Spray */
        /* Angle between forward and up in degrees */
        public float minSpray;
        public float maxSpray;
        public float sprayAcceleration;
    }

    public enum WeaponGroup : int { Primary = 1, Secondary, Melee }
    public enum WeaponType : int { Rifle = 10, Shotgun, Cocking = 20, NoCocking }
    public enum AmmoType : int { Handgun = 0, Magnum, Rifle, Shotgun }
}


