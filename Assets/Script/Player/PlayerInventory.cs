using UnityEngine;
using MyGame.Inventory;
using MyGame.Inventory.Weapon;



namespace MyGame.Player
{
    public class PlayerInventory : MonoBehaviour
    {

        public PlayerWeaponSlot weaponSlot;
        public Weapon activePrimary;
        public Weapon activeSecondary;
        public Weapon activeMelee;

        [SerializeField]
        Item[] inventory;
        int capacity = 10;
        int currentIndex = 0;

        #region Properties

        public Weapon CurrentWeapon
        {
            get
            {
                return weaponSlot.Weapon;
            }
        }

        #endregion


        #region Awake to Updates

        void Awake()
        {
            inventory = new Item[capacity];
        }

        void Update()
        {
        }
        #endregion

        public ItemInteractionResult AddItem(Item item)
        {
            /* Capacity Limit */
            if (currentIndex == capacity)
                return new ItemInteractionResult(false, "Inventory Full");

            item.gameObject.SetActive(false);
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            inventory[currentIndex++] = item;

            return new ItemInteractionResult(false, "Item Succesfully Added");
        }

        public void EquipWeapon(Weapon newWeapon)
        {

            int newWeaponIndex = GetItemIndex(newWeapon);

            if (newWeaponIndex == -1)
            {
                Debug.LogWarning("Item Equipment : Item Not in Inventory");
                return;
            }


            if (newWeapon.Properties.group == WeaponGroup.Primary)
                activePrimary = newWeapon;

            else if (newWeapon.Properties.group == WeaponGroup.Secondary)
                activeSecondary = newWeapon;
            
            else if (newWeapon.Properties.group == WeaponGroup.Melee)
                activeMelee = newWeapon;


            /* No Item Equipped Before*/
            if (weaponSlot.Weapon == null)
            {
                newWeapon.transform.SetParent(weaponSlot.transform);
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = weaponSlot.transform.rotation;
                weaponSlot.Weapon = newWeapon;

            }

            /* Item Equipped Before*/
            else
            {
                Weapon oldWeapon = weaponSlot.Weapon;
                bool sameGroup = oldWeapon.Properties.group == newWeapon.Properties.group;

                oldWeapon.gameObject.SetActive(false);
                oldWeapon.transform.SetParent(transform);
                oldWeapon.transform.localPosition = Vector3.zero;
                oldWeapon.transform.rotation = transform.rotation;

                newWeapon.transform.SetParent(weaponSlot.transform);
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = weaponSlot.transform.rotation;
                newWeapon.gameObject.SetActive(sameGroup);

                weaponSlot.Weapon = newWeapon;
            }
        }

        public Item GetItemByIndex(int index)
        {
            return inventory[index];
        }

        public int GetItemIndex(Item item)
        {
            for(int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == item)
                    return i;
            }

                return -1;
        }

        public int FindWeapon(WeaponGroup group)
        {
            if (group == WeaponGroup.Primary)
                if (activePrimary != null)
                    return GetItemIndex(activePrimary);

            else if (group == WeaponGroup.Secondary)
                if (activeSecondary != null)
                    return GetItemIndex(activeSecondary);

            else if (group == WeaponGroup.Melee)
                    if (activeMelee != null)
                        return GetItemIndex(activeMelee);

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null)
                    return -1;

                if (inventory[i] is Weapon && ((Weapon) inventory[i]).Properties.group == group)
                {
                    return i;
                }
            }

            return -1;
        }

        public void UseItem(int index)
        {
        }

        public bool RemoveItem(int index)
        {
            /* Indexing Checking */
            if (index < 0 || index >= inventory.Length)
                return false;

            /* Re-order the Rest */
            for (int i = index; i < inventory.Length - 1; i++)
                inventory[i] = inventory[i + 1];


            return true;
        }

        void display()
        {

        }
    }

}
