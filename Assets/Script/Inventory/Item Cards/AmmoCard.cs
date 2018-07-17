using UnityEngine;

namespace MyGame.Inventory.Weapon
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/AmmoCard")]
    public class AmmoCard : ItemCard
    {
        public AmmoType type;

    }
}

