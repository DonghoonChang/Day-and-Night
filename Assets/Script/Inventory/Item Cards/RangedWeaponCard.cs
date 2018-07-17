using UnityEngine;

namespace MyGame.Inventory.Weapon
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/RangedWeaponCard")]
    public class RangedWeaponCard : WeaponCard
    {
        public GameObject muzzleFlashSFX;
        public GameObject cartridgeSFX;

        public Sound dryAttackSound;
        public Sound silencedAttackSound;
    }
}
