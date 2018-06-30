using UnityEngine;

namespace MyGame.Inventory
{
    public abstract class Item : MonoBehaviour
    {

    }

    public class ItemInteractionResult
    {
        public bool isSuccessful;
        public string message;

        ItemInteractionResult()
        {
            isSuccessful = false;
            message = "Default Message";
        }

        public ItemInteractionResult(bool isSuccessful, string message)
        {
            this.isSuccessful = isSuccessful;
            this.message = message;
        }
    }
}

