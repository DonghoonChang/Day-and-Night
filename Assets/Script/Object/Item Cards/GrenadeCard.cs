using UnityEngine;

namespace Game.Object
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/GrenadeCard")]
    public class GrenadeCard : ItemCard
    {
        public float damage;
        public float concussion;
        public float explosionRadius;
        public float explosionForce;
        public GrenadeType type;

        public GameObject explosionVFX;
        public Sound explosionSFX;
    }

    public enum GrenadeType: int
    {
        F1Genade = 0,
        M67Grenade,
        RGD5Grenade,
        PineappleGrenade,
        StickGrenade,
        StunGrenade,
        SmokeGrenade,

    }
}
