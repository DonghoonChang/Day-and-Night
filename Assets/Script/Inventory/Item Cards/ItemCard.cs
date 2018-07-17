using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Inventory
{
    public abstract class ItemCard : ScriptableObject
    {
        public Sprite icon;
        public string itemName = "No Name";
        public string description = "No Name";

        public bool isUsable;
        public bool isCombinable;
        public bool isEssential;
        public bool isDestoryedOnUse;
    }
}
