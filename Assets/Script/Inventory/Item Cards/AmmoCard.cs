using UnityEngine;

namespace MyGame.Object.Weapon
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/AmmoCard")]
    public class AmmoCard : ItemCard
    {
        public AmmoType type;

    }
}

