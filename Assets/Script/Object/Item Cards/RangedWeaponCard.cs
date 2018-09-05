using UnityEngine;

namespace Game.Object
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/RangedWeaponCard")]
    public class RangedWeaponCard : WeaponCard
    {
        public Sound dryAttackSound;
        public Sound silencedAttackSound;

        public GameObject cartridgeSFX;
        public GameObject muzzleFlashSFX;
    }
}
