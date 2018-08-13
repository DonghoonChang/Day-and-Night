using UnityEngine;


namespace MyGame.Object
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/ConsumableCard")]
    public class ConsumableCard : ItemCard
    {
        public bool isStackable;
        public int quantity;
    }
}

