using UnityEngine;
using UnityEngine.UI;
using MyGame.Player;
using MyGame.Object;
using TMPro;

namespace MyGame.UI
{
    public class UIInventoryPanel : MonoBehaviour
    {
        [SerializeField]
        PlayerInventory inventory;

        public Button[] mainWeaponButtons;
        public Button[] meleeWeaponButtons;
        public Button[] itemButtons;
        public Button[] itemOptionButtons;
        public InventoryInformation inventoryInfo;


        // Global Index - 0 ~ 2(Main), 10 ~ 12(Melee), 20 ~ 30(Secondary & Item)
        int itemIndexA = -1, itemIndexB = -1;

        #region Awake to Updates

        #endregion

        #region Helpers

        private void ResetItemIndexes()
        {
            itemIndexA = itemIndexB = -1;
        }

        private void DisplayInventory()
        {
            for (int i = 0; i < inventory.MainWeaponInventory.Length; i++)
            {
                Button button = mainWeaponButtons[i];
                Item item = inventory.MainWeaponInventory[i];

                if (item == null)
                    button.interactable = false;

                else
                {
                    UIItemSlot itemSlot = button.GetComponent<UIItemSlot>();
                    itemSlot.Item = item;

                    button.interactable = true;
                }
            }

            for (int i = 0; i < inventory.MeleeWeaponInventory.Length; i++)
            {
                Button button = meleeWeaponButtons[i];
                Item item = inventory.MeleeWeaponInventory[i];


                if (item == null)
                    button.interactable = false;

                else
                {
                    UIItemSlot itemSlot = button.GetComponent<UIItemSlot>();
                    itemSlot.Item = item;

                    button.interactable = true;
                }
            }

            for (int i = 0; i < inventory.ItemInventory.Length; i++)
            {
                Button button = itemButtons[i];
                Item item = inventory.ItemInventory[i];


                if (inventory.ItemInventory[i] == null)
                    button.interactable = false;

                else
                {
                    UIItemSlot itemSlot = button.GetComponent<UIItemSlot>();
                    itemSlot.Item = item;

                    button.interactable = true;
                }
            }

            foreach (Button b in itemOptionButtons)
                b.interactable = false;

            foreach (Button b in mainWeaponButtons)
                if (b.interactable)
                {
                    b.Select();
                    return;
                }

            foreach (Button b in meleeWeaponButtons)
                if (b.interactable)
                {
                    b.Select();
                    return;
                }

            foreach (Button b in itemButtons)
                if (b.interactable)
                {
                    b.Select();
                    return;
                }

        }

        private void DisplayItemOptions()
        {
            foreach (Button b in mainWeaponButtons)
                b.interactable = false;

            foreach (Button b in meleeWeaponButtons)
                b.interactable = false;

            foreach (Button b in itemButtons)
                b.interactable = false;

            foreach (Button b in itemOptionButtons)
                b.interactable = true;

            itemOptionButtons[0].Select();
        }

        #endregion

        #region Main Functions

        public void ShowInventory(bool visible)
        {
            gameObject.SetActive(visible);
            
            if (visible)
            {
                DisplayInventory();
                ResetItemIndexes();
            }
        }

        public void OnItemSelected(int globalIndex)
        {
            ItemInfo info = inventory.GetItemInfo(globalIndex);

            inventoryInfo.name.text = info.name;
            inventoryInfo.description.text = info.description;
        }

        public void OnItemClicked(int globalIndex)
        {
            if (itemIndexA == -1)
            {
                itemIndexA = globalIndex;
                DisplayItemOptions();
            }

            else
            {

            }
        }

        public void OnUseClicked()
        {
            if (itemIndexA == -1)
                return;

            else
                inventory.UseItem(itemIndexA);

            ResetItemIndexes();
            DisplayInventory();
        }

        public void OnCombineClicked()
        {

        }

        public void OnDropClicked()
        {
            if (itemIndexA == -1)
                return;

            else
                inventory.DropItem(itemIndexA);

            ResetItemIndexes();
            DisplayInventory();
        }

        public void OnCancelClicked()
        {
            ResetItemIndexes();
            DisplayInventory();
        }

        #endregion

        [System.Serializable]
        public class InventoryInformation
        {
            public TextMeshProUGUI name;
            public TextMeshProUGUI description;
        }
    }
}
