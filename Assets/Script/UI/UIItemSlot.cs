using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MyGame.UI {

    public class UIItemSlot : MonoBehaviour, ISelectHandler
    {

        public UIInventoryPanel inventoryPanel;
        public int index;

        public void OnSelect(BaseEventData eventData)
        {
            inventoryPanel.OnItemSelected(index);
        }
    }
}

