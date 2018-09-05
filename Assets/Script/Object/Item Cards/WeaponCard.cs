namespace Game.Object
{
    public class WeaponCard : ItemCard
    {
        public Sound attackSound;
        public WeaponStats stats;
        public WeaponProperties properties;
    }

    [System.Serializable]
    public struct WeaponProperties
    {
        public AmmoType ammoType;
        public WeaponType weaponType;
        public WeaponGroup weaponGroup;

        public bool isAutomatic; // Automatic
        public bool isScopeModifiable; // Scope & Laser Sight
        public bool isMuzzleModifiable; // Silencer & Bayonet
        public bool isBarrelModifiable; // Grenade Launcher and Lights(UV exposing Enemy WeakSpot, or Just Flashlight) */
    }

    [System.Serializable]
    public struct WeaponStats
    {
        public int rateOfFire;
        public int magazineCapacity;

        public int pelletsPerShot;
        public int damagePerPellet;
        public int concussionPerPellet;

        public float spreadStep;
        public float minSpread;
        public float maxSpread;
    }

    public enum WeaponGroup : int { Unarmed = 0, Main, Secondary, Melee }
    public enum WeaponType : int { Rifle = 10, Shotgun, Cocking = 20, NoCocking }
    public enum AmmoType : int { Rifle = 10, Shotgun, MachineGun, Pistol = 20, SubmachineGun, Revolver}
}


