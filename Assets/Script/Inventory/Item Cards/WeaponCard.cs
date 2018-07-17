namespace MyGame.Inventory.Weapon
{
    public class WeaponCard : ItemCard
    {
        public Sound attackSound;
        public WeaponProperties properties;
        public WeaponStats stats;
    }

    [System.Serializable]
    public class WeaponProperties
    {
        public WeaponGroup weaponGroup;
        public WeaponType weaponType;
        public AmmoType ammoType;

        /* Scope & Laser Sight */
        public bool isScopeModifiable;

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
        public float spreadStep;
        public float minSpread;
        public float maxSpread;
    }

    public enum WeaponGroup : int { Main = 1, Secondary, Melee }
    public enum WeaponType : int { Rifle = 10, Shotgun, Cocking = 20, NoCocking }
    public enum AmmoType : int { Rifle = 10, Shotgun, MachineGun, Pistol = 20, SubmachineGun, Revolver}
}


