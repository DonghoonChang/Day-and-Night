using UnityEngine;
using UnityEngine.UI;
using MyGame.Player;
using MyGame.Inventory.Weapon;
using TMPro;

namespace MyGame.UI
{
    public class UIInventoryPanel : MonoBehaviour
    {
        [SerializeField]
        PlayerInventory inventory;

        public Sprite DefaultMainWeaponImage;
        public Sprite DefaultMeleeWeaponImage;
        public Sprite DefaultItemImage;

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

                if (inventory.MainWeaponInventory[i] == null)
                {
                    button.GetComponent<Image>().sprite = DefaultMainWeaponImage;
                    button.interactable = false;
                }

                else
                {
                    button.GetComponent<Image>().sprite = inventory.MainWeaponInventory[i].Icon;
                    button.interactable = true;
                }
            }

            for (int i = 0; i < inventory.MeleeWeaponInventory.Length; i++)
            {
                Button button = meleeWeaponButtons[i];

                if (inventory.MeleeWeaponInventory[i] == null)
                {
                    button.GetComponent<Image>().sprite = DefaultMeleeWeaponImage;
                    meleeWeaponButtons[i].interactable = false;
                }

                else
                {
                    button.GetComponent<Image>().sprite = inventory.MeleeWeaponInventory[i].Icon;
                    button.interactable = true;
                }
            }

            for (int i = 0; i < inventory.ItemInventory.Length; i++)
            {
                Button button = itemButtons[i];

                if (inventory.ItemInventory[i] == null)
                {
                    button.GetComponent<Image>().sprite = DefaultItemImage;
                    button.interactable = false;
                }

                else if (inventory.ItemInventory[i] is Weapon)
                {
                    Weapon weapon = inventory.ItemInventory[i] as Weapon;

                    button.GetComponent<Image>().sprite = weapon.Icon;
                    button.interactable = true;
                }

                else
                {
                    button.GetComponent<Image>().sprite = inventory.ItemInventory[i].Icon;
                    button.interactable = true;
                }
            }

            foreach (Button b in itemOptionButtons)
            {
                b.interactable = false;
            }

            foreach (Button b in mainWeaponButtons)
            {
                if (b.interactable)
                {
                    b.Select();
                    return;
                }
            }

            foreach (Button b in meleeWeaponButtons)
            {
                if (b.interactable)
                {
                    b.Select();
                    return;
                }
            }

            foreach (Button b in itemButtons)
            {
                if (b.interactable)
                {
                    b.Select();
                    return;
                }
            }

            //If the program reaches here, it means there's no item in the inventory
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
                ResetItemIndexes();
                DisplayInventory();
            }
        }

        public void OnItemSelected(int globalIndex)
        {
            ItemInfo info = inventory.GetItemInfo(globalIndex);

            inventoryInfo.name.text = info.name;
            inventoryInfo.description.text = info.description;
            inventoryInfo.damage.text = info.damage;
            inventoryInfo.ammoCapcity.text = info.capacity;
            inventoryInfo.spread.text = info.spread;
            inventoryInfo.fireRate.text = info.fireRate;
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
                UIErrorPanel.ReportResult(new Inventory.ObjectInteractionResult(false, "Program Logic Error(Indexing Item with -1)"));

            else
            {
                Debug.Log(itemIndexA);
                inventory.UseItem(itemIndexA);
            }

            ResetItemIndexes();
            DisplayInventory();
        }

        public void OnCombineClicked()
        {

        }

        public void OnDropClicked()
        {

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
            public TextMeshProUGUI damage;
            public TextMeshProUGUI ammoCapcity;
            public TextMeshProUGUI spread;
            public TextMeshProUGUI fireRate;
        }
    }
}
