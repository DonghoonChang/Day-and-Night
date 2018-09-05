using UnityEngine;
using UnityEngine.UI;
using Game.Player;
using Game.Object;
using TMPro;

namespace Game.UI
{
    public class UIInventoryPanel : MonoBehaviour
    {
        [SerializeField]
        PlayerInventory _inventory;

        [SerializeField]
        Light _inventoryLight;

        [SerializeField]
        Transform _commandShade;

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
            _commandShade.gameObject.SetActive(false);

            for (int i = 0; i < _inventory.MainWeaponInventory.Length; i++)
            {
                Button button = mainWeaponButtons[i];
                Item item = _inventory.MainWeaponInventory[i];

                if (item == null)
                    button.interactable = false;

                else
                {
                    UIItemSlot itemSlot = button.GetComponent<UIItemSlot>();
                    itemSlot.Item = item;

                    button.interactable = true;
                }
            }

            for (int i = 0; i < _inventory.MeleeWeaponInventory.Length; i++)
            {
                Button button = meleeWeaponButtons[i];
                Item item = _inventory.MeleeWeaponInventory[i];


                if (item == null)
                    button.interactable = false;

                else
                {
                    UIItemSlot itemSlot = button.GetComponent<UIItemSlot>();
                    itemSlot.Item = item;

                    button.interactable = true;
                }
            }

            for (int i = 0; i < _inventory.ItemInventory.Length; i++)
            {
                Button button = itemButtons[i];
                Item item = _inventory.ItemInventory[i];


                if (_inventory.ItemInventory[i] == null)
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

        private void ActivateItemCommands()
        {
            foreach (Button b in mainWeaponButtons)
                b.interactable = false;

            foreach (Button b in meleeWeaponButtons)
                b.interactable = false;

            foreach (Button b in itemButtons)
                b.interactable = false;

            foreach (Button b in itemOptionButtons)
                b.interactable = true;

            _commandShade.gameObject.SetActive(true);
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

        // Button Events
        public void OnButtonSelected(int buttonIndex)
        {
            ItemInfo info = _inventory.GetItemInfo(buttonIndex);

            inventoryInfo.name.text = info.name;
            inventoryInfo.description.text = info.description;

            Button button = GetButton(buttonIndex);
            _inventoryLight.transform.LookAt(button.transform);
        }

        public void OnButtonClicked(int buttonIndex)
        {
            if (itemIndexA == -1)
            {
                itemIndexA = buttonIndex;
                ActivateItemCommands();
            }

            else
            {

            }
        }

        private Button GetButton(int buttonIndex)
        {
            if (buttonIndex < 10)
                return mainWeaponButtons[buttonIndex];

            else if (buttonIndex < 20)
                return meleeWeaponButtons[buttonIndex - 10];

            else if (buttonIndex < 30)
                return itemButtons[buttonIndex - 20];

            else
                return null;
        }

        // Item Command
        public void OnUseClicked()
        {
            if (itemIndexA == -1)
                return;

            else
                _inventory.UseItem(itemIndexA);

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
                _inventory.DropItem(itemIndexA);

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
