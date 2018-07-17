using UnityEngine;
using MyGame.UI;
using UIManager = MyGame.GameManagement.UIManager;
using MyGame.Inventory;
using MyGame.Inventory.Weapon;

namespace MyGame.Player
{
    public class PlayerInventory : MonoBehaviour
    {

        UIHUD hud;

        Ammo _activeAmmo;
        PlayerWeaponSlot _weaponSlot;

        // Active Weapons
        int _activeWeaponIndex = -1, _activeMainIndex = -1,
            _activeMeleeIndex = -1, _activeSecondaryIndex = -1,
            _activeAmmoIndex = -1;

        RangedWeapon[] mainWeaponInventory = new RangedWeapon[3];
        MeleeWeapon[] meleeWeaponInventory = new MeleeWeapon[3];
        Item[] itemInventory = new Item[10];

        #region Awake to Update

        private void Awake()
        {
            _weaponSlot = transform.root.GetComponentInChildren<PlayerWeaponSlot>();
            _weaponSlot.OnWeaponAttack.AddListener(UpdateAmmoHUD);
        }

        private void Start()
        {
            hud = UIManager.Instance.HUDPanel;
        }

        private void Update()
        {
            hud.crosshairHUD.SetCurrentSpread(_weaponSlot.CurrentSpread);
        }

        #endregion

        #region Properties

        public Weapon[] MainWeaponInventory
        {
            get
            {
                return mainWeaponInventory;
            }
        }

        public Weapon[] MeleeWeaponInventory
        {
            get
            {
                return meleeWeaponInventory;
            }
        }

        public Item[] ItemInventory
        {
            get
            {
                return itemInventory;
            }
        }

        public Weapon ActiveWeapon
        {
            get
            {
                return _weaponSlot.Weapon;
            }
        }

        public Ammo ActiveAmmo
        {
            get
            {
                return _activeAmmo;
            }
        }

        public int ActiveWeaponIndex
        {
            get
            {
                return _activeWeaponIndex;
            }
        }

        public int ActiveAmmoIndex
        {
            get
            {
                return _activeAmmoIndex;
            }
        }

        public int ActiveMainIndex
        {
            get
            {
                return _activeMainIndex;
            }
        }

        public int ActiveMeleeIndex
        {
            get
            {
                return _activeMeleeIndex;
            }
        }

        public int ActiveSecondaryIndex
        {
            get
            {
                return _activeSecondaryIndex;
            }
        }

        #endregion

        #region Helpers

        private bool IsInventoryFull(Item[] inventory)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null)
                    return false;
            }

            return true;
        }

        private int GetEmptyItemSlot(Item[] inventory)
        {
            for(int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null)
                    return i;
            }

            return -1;
        }

        // Returns Global Index
        private int FindAnyMainWeapon()
        {
            for (int i = 0; i < mainWeaponInventory.Length; i++)
            {
                if (mainWeaponInventory[i] != null)
                    return i;
            }

            return -1;
        }

        // Returns Global Index
        private int FindAnyMeleeWeapon()
        {
            for (int i = 0; i < meleeWeaponInventory.Length; i++)
            {
                if (meleeWeaponInventory[i] != null)
                    return i + 10;
            }

            return -1;
        }

        // Returns Global Index
        private int FindAnySecondaryWeapon()
        {
            for (int i = 0; i < itemInventory.Length; i++)
            {
                if (itemInventory[i] is Weapon)
                {
                    Weapon weapon = itemInventory[i] as Weapon;

                    if (weapon.Properties.weaponGroup == WeaponGroup.Secondary)
                        return i + 20;
                }
            }

            return -1;
        }

        private void UnequipCurrentWeapon()
        {
            if (_activeWeaponIndex != -1)
            {
                Weapon current = _weaponSlot.Weapon;

                _activeWeaponIndex = -1;
                _weaponSlot.Weapon = null;

                current.gameObject.SetActive(false);
                current.transform.SetParent(transform);
                current.transform.localPosition = Vector3.zero;
                current.transform.rotation = transform.rotation;

                UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Current Weapon Unequipped"));
            }

            else
            {
                UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Weapon to Unequip"));
            }
        }

        private void EquipWeapon(int globalIndex, bool active)
        {
            if (0 <= globalIndex && globalIndex <= mainWeaponInventory.Length)
            {
                RangedWeapon newWeapon = mainWeaponInventory[globalIndex] as RangedWeapon;

                if (newWeapon == null)
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Indexed Item Null Or Non-Ranged Weapon in the Main Inventory)"));
                    return;
                }

                _activeWeaponIndex = globalIndex;
                _activeMainIndex = globalIndex;

                newWeapon.gameObject.SetActive(active);
                newWeapon.transform.SetParent(_weaponSlot.transform);
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = _weaponSlot.transform.rotation;
                _weaponSlot.Weapon = newWeapon;

                UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Main Weapon Equipped"));

                _activeAmmoIndex = FindAmmo(newWeapon.Properties.ammoType);
                _activeAmmo = GetItem(_activeAmmoIndex) as Ammo;

                hud.ammoHUD.gameObject.SetActive(true);
                hud.ammoHUD.SetCurrentAmmo(newWeapon.CurrentAmmo);
                hud.ammoHUD.SetTotalAmmo(_activeAmmo == null ? 0 : _activeAmmo.Quantity);
            }

            else if (10 <= globalIndex && globalIndex < 10 + mainWeaponInventory.Length)
            {
                Weapon newWeapon = meleeWeaponInventory[globalIndex - 10];

                if (newWeapon == null)
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Indexed Item Null)"));
                    return;
                }

                _activeWeaponIndex = globalIndex;
                _activeMeleeIndex = globalIndex - 10;

                newWeapon.gameObject.SetActive(active);
                newWeapon.transform.SetParent(_weaponSlot.transform);
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = _weaponSlot.transform.rotation;
                _weaponSlot.Weapon = newWeapon;

                UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Melee Weapon Equipped"));

                _activeAmmo = null;
                _activeAmmoIndex = -1;
                hud.ammoHUD.gameObject.SetActive(false);
            }

            else if (20 <= globalIndex && globalIndex < 20 + itemInventory.Length)
            {
                Item item = itemInventory[globalIndex - 20];

                if (item == null)
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Indexed Item Null)"));
                    return;
                }

                if (item is Weapon)
                {
                    RangedWeapon newWeapon = item as RangedWeapon;

                    // Is the Inventory Correct?
                    if (newWeapon.Properties.weaponGroup != WeaponGroup.Secondary)
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Main/Melee Item in Item Inventory)"));
                        return;
                    }

                    _activeWeaponIndex = globalIndex;
                    _activeSecondaryIndex = globalIndex - 20;

                    newWeapon.gameObject.SetActive(active);
                    newWeapon.transform.SetParent(_weaponSlot.transform);
                    newWeapon.transform.localPosition = Vector3.zero;
                    newWeapon.transform.rotation = _weaponSlot.transform.rotation;
                    _weaponSlot.Weapon = newWeapon;

                    UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Secondary Weapon Equipped"));

                    _activeAmmoIndex = FindAmmo(newWeapon.Properties.ammoType);
                    _activeAmmo = GetItem(_activeAmmoIndex) as Ammo;

                    hud.ammoHUD.gameObject.SetActive(true);
                    hud.ammoHUD.SetCurrentAmmo(newWeapon.CurrentAmmo);
                    hud.ammoHUD.SetTotalAmmo(_activeAmmo == null ? 0 : _activeAmmo.Quantity);
                }

                else
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Indexed Item Not Weapon"));
                    return;
                }
            }
        }

        // Returns GlobalIndex
        private int FindAmmo(AmmoType type)
        {
            for(int i = 0; i < itemInventory.Length; i++)
            {
                if (itemInventory[i] is Ammo)
                {
                    Ammo ammo = itemInventory[i] as Ammo;

                    if (ammo.Type == type)
                        return i + 20;
                }
            }

            return -1;
        }

        private Item GetItem(int globalIndex)
        {
            // Main Weapon
            if (0 <= globalIndex && globalIndex < mainWeaponInventory.Length)
            {
                return mainWeaponInventory[globalIndex];
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + meleeWeaponInventory.Length)
            {
                return meleeWeaponInventory[globalIndex - 10];
            }

            else if (20 <= globalIndex && globalIndex < 20 + itemInventory.Length)
            {
                return itemInventory[globalIndex - 20];
            }

            else
                return null;
        }

        #endregion

        // Weapon Related
        public bool IsCurrentWeaponLocked()
        {
            if (_activeWeaponIndex == -1)
                return true;

            else if (_weaponSlot.Weapon.IsAttackLocked)
                return true;

            else
                return false;
        }

        public bool IsCurrentWeaponAutomatic()
        {
            if (_activeWeaponIndex == -1)
                return false;

            else
            {
                Item item = GetItem(_activeWeaponIndex);

                if (item is RangedWeapon)
                {
                    RangedWeapon weapon = item as RangedWeapon;
                    return weapon.IsAutomatic;
                }

                else
                    return false;

            }
        }

        public bool IsWeaponEquipped()
        {
            return _activeWeaponIndex != -1;
        }

        public int FindWeapon(WeaponGroup group)
        {
            switch (group)
            {
                case WeaponGroup.Main:
                    return FindAnyMainWeapon();
                case WeaponGroup.Melee:
                    return FindAnyMeleeWeapon();
                case WeaponGroup.Secondary:
                    return FindAnySecondaryWeapon();
                default:
                    return -1;
            }
        }

        public WeaponGroup GetCurrentWeaponGroup()
        {
            if (0 <= _activeWeaponIndex && _activeWeaponIndex < mainWeaponInventory.Length)
                return WeaponGroup.Main;

            else if (10 <= _activeWeaponIndex && _activeWeaponIndex < 10 + meleeWeaponInventory.Length)
                return WeaponGroup.Melee;

            else
                return WeaponGroup.Secondary;
        }

        public bool Reload()
        {
            if (_activeAmmo == null || _activeAmmoIndex == -1)
            {
                _activeAmmo = null;
                _activeAmmoIndex = -1;

                return false;
            }

            else
            {
                if (ActiveWeapon is RangedWeapon)
                {
                    RangedWeapon weapon = ActiveWeapon as RangedWeapon;

                    int availableAmmo = _activeAmmo.Quantity;
                    int requiredAmmo = weapon.MagazineCapacity - weapon.CurrentAmmo;

                    if (availableAmmo > requiredAmmo)
                    {
                        _activeAmmo.Quantity -=  requiredAmmo;
                        weapon.CurrentAmmo += requiredAmmo;

                        UpdateAmmoHUD();

                        return true;
                    }

                    else
                    {
                        _activeAmmo.Quantity = 0;
                        weapon.CurrentAmmo += availableAmmo;

                        DestroyItem(_activeAmmoIndex);

                        _activeAmmo = null;
                        _activeAmmoIndex = -1;

                        UpdateAmmoHUD();

                        return true;
                    }
                }

                // Case where melee weapon is equipped but current ammo is still active
                else
                {
                    _activeAmmo = null;
                    _activeAmmoIndex = -1;
                    return false;
                }
            }
        }

        // Inventory Interface Related
        public ItemInfo GetItemInfo(int globalIndex)
        {
            // Main Weapon
            if (0 <= globalIndex && globalIndex < mainWeaponInventory.Length)
            {
                RangedWeapon weapon = mainWeaponInventory[globalIndex];
                ItemInfo info = new ItemInfo(weapon.Name, weapon.Description,
                                             weapon.Damage.ToString(), weapon.MagazineCapacity.ToString(),
                                             weapon.SpreadStep.ToString(), weapon.FireRate.ToString());

                return info;
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + meleeWeaponInventory.Length)
            {
                Weapon weapon = mainWeaponInventory[globalIndex - 10];
                ItemInfo info = new ItemInfo(weapon.Name, weapon.Description,
                                             weapon.Damage.ToString(), "", "", weapon.FireRate.ToString());

                return info;
            }

            else if (20 <= globalIndex && globalIndex < 20 + itemInventory.Length)
            {
                Item item = itemInventory[globalIndex - 20];

                if (item is RangedWeapon)
                {
                    RangedWeapon weapon = item as RangedWeapon;
                    ItemInfo info = new ItemInfo(weapon.Name, weapon.Description,
                                                 weapon.Damage.ToString(), weapon.MagazineCapacity.ToString(),
                                                 weapon.SpreadStep.ToString(), weapon.FireRate.ToString());

                    return info;
                }

                else
                {
                    ItemInfo info = new ItemInfo(item.Name, item.Description, "", "", "", "");
                    return info;
                }
            }

            else
                return null;
        }

        public void UseItem(int globalIndex)
        {
            // Main Weapon
            if (0 <= globalIndex && globalIndex < mainWeaponInventory.Length)
            {
                if (_activeWeaponIndex == -1)
                    EquipWeapon(globalIndex, false);

                else
                {
                    bool wasActive = _weaponSlot.isWeaponActive;
                    UnequipCurrentWeapon();
                    EquipWeapon(globalIndex, wasActive);
                }
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + meleeWeaponInventory.Length)
            {
                if (_activeWeaponIndex == -1)
                    EquipWeapon(globalIndex, false);

                else
                {
                    bool wasActive = _weaponSlot.isWeaponActive;
                    UnequipCurrentWeapon();
                    EquipWeapon(globalIndex, wasActive);
                }
            }

            else if (20 <= globalIndex && globalIndex < 20 + itemInventory.Length)
            {
                int itemIndex = globalIndex - 20;

                Item item = itemInventory[itemIndex];

                // Is the Indexing Correct?
                if (item == null)
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Indexed Item Null)"));
                    return;
                }

                // Is the Item a Weapon?
                if (item is Weapon)
                {
                    if (_activeWeaponIndex == -1)
                        EquipWeapon(globalIndex, false);

                    else
                    {
                        bool wasActive = _weaponSlot.isWeaponActive;
                        UnequipCurrentWeapon();
                        EquipWeapon(globalIndex, wasActive);
                    }
                }

                // Item is not a Weapon
                else
                {

                }
            }

            else
            {
                UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error(Wrong Item Index)"));
                return;
            }
        }

        public bool AddItem(Item item)
        {
            if (item is Weapon)
            {
                Weapon weapon = item as Weapon;

                // Main Weapon
                if (weapon.Properties.weaponGroup == WeaponGroup.Main)
                {
                    // Inventory Full
                    if (IsInventoryFull(mainWeaponInventory))
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Inventory Full"));
                        return false;
                    }

                    // Inventory Not FUll
                    else {

                        int emptySlotIndex = GetEmptyItemSlot(mainWeaponInventory);

                        if (emptySlotIndex == -1)
                        {
                            UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error"));
                            return false;
                        }

                        else
                        {
                            weapon.gameObject.SetActive(false);
                            weapon.transform.SetParent(transform);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = transform.rotation;

                            mainWeaponInventory[emptySlotIndex] = weapon as RangedWeapon;
                            UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Item Added"));
                            return true;
                        }
                    }
                }

                else if (weapon.Properties.weaponGroup == WeaponGroup.Melee)
                {
                    if (IsInventoryFull(meleeWeaponInventory))
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Inventory Full"));
                        return false;
                    }

                    else
                    {

                        int emptySlotIndex = GetEmptyItemSlot(meleeWeaponInventory);

                        if (emptySlotIndex == -1)
                        {
                            UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error"));
                            return false;
                        }

                        else
                        {
                            weapon.gameObject.SetActive(false);
                            weapon.transform.SetParent(transform);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = transform.rotation;

                            meleeWeaponInventory[emptySlotIndex] = weapon as MeleeWeapon;
                            UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Item Added"));
                            return true;
                        }
                    }
                }

                else if (weapon.Properties.weaponGroup == WeaponGroup.Secondary)
                {
                    if (IsInventoryFull(itemInventory))
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Inventory Full"));
                        return false;
                    }

                    else
                    {

                        int emptySlotIndex = GetEmptyItemSlot(itemInventory);

                        if (emptySlotIndex == -1)
                        {
                            UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error"));
                            return false;
                        }

                        else
                        {
                            weapon.gameObject.SetActive(false);
                            weapon.transform.SetParent(transform);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = transform.rotation;

                            itemInventory[emptySlotIndex] = weapon;
                            UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Item Added"));
                            return true;
                        }
                    }
                }
            }

            else
            {
                if (IsInventoryFull(itemInventory))
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Inventory Full"));
                    return false;
                }

                else
                {

                    int emptySlotIndex = GetEmptyItemSlot(itemInventory);

                    if (emptySlotIndex == -1)
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Program Logic Error"));
                        return false;
                    }

                    else
                    {
                        item.gameObject.SetActive(false);
                        item.transform.SetParent(transform);
                        item.transform.localPosition = Vector3.zero;
                        item.transform.localRotation = transform.rotation;

                        itemInventory[emptySlotIndex] = item;
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(true, "Item Added"));
                        return true;
                    }
                }
            }

            return false;
        }

        public bool DropItem(int globalIndex)
        {
            /* Indexing Checking */
            if (globalIndex < 0 || globalIndex >= itemInventory.Length)
                return false;

            /* Re-order the Rest */
            for (int i = globalIndex; i < itemInventory.Length - 1; i++)
                itemInventory[i] = itemInventory[i + 1];


            return true;
        }
        
        public void DestroyItem(int globalIndex)
        {
            Debug.Log("DestroyItem(int globalIndex) not implemented");
        }

        public void CombineItem()
        {

        }

        // UI Related
        public void UpdateAmmoHUD()
        {
            if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main 
                || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary)
            {
                RangedWeapon weapon = _weaponSlot.Weapon as RangedWeapon;

                hud.ammoHUD.SetCurrentAmmo(weapon.CurrentAmmo);
                hud.ammoHUD.SetTotalAmmo(ActiveAmmo == null ? 0 : ActiveAmmo.Quantity);
            }
        }
    }

    public class ItemInfo
    {
        public string name;
        public string description;
        public string damage;
        public string capacity;
        public string spread;
        public string fireRate;

        public ItemInfo(string name, string description, string damage, string capacity, string spread, string fireRate)
        {
            this.name = name;
            this.description = description;
            this.damage = damage;
            this.capacity = capacity;
            this.spread = spread;
            this.fireRate = fireRate;
        }
    }
}
