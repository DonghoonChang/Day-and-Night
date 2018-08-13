using UnityEngine;

namespace MyGame.Object
{
    public abstract class ItemCard : ScriptableObject
    {
        public string itemName = "No Name";
        public string description = "No Name";

        public bool isUsable;
        public bool isCombinable;
        public bool isEssential;
        public bool isDestoryedOnUse;
    }
}
