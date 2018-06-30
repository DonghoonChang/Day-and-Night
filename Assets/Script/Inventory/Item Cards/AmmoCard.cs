using UnityEngine;

namespace MyGame.Inventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/AmmoCard")]
    public class AmmoCard : ItemCard
    {
        public string ammoType;
        public int quantity;
    }
}

