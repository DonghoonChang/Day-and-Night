using UnityEngine;

namespace MyGame.Object
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
        StickGrenade = 0,
    }
}
