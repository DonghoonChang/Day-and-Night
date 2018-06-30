using UnityEngine;

namespace MyGame.Inventory
{
    public abstract class ItemCard : ScriptableObject
    {
        public string itemName = "No Name";
        public string description = "No Name";
        public Texture2D icon;

        public bool isUsable;
        public bool isCombinable;
        public bool isEssential;
        public bool isDestoryedOnUse;

    }
}
