using UnityEngine;
using Game.UI;
using UIManager = Game.GameManagement.UIManager;
using Game.Object;
using Game.Object.Weapon;

namespace Game.Player
{
    public class PlayerInventory : MonoBehaviour
    {

        UIHUD _uiHUD;

        PlayerCharacter _player;
        PlayerFlashlight _flashlight;
        PlayerWeaponSlot _weaponSlot;
        PlayerGrenadeSlot _grenadeSlot;

        [SerializeField]
        int _activeWeaponIndex = -1,
            _activeMainIndex = -1,
            _activeMeleeIndex = -1,
            _activeSecondaryIndex = -1,
            _activeAmmoIndex = -1,
            _activeGrenadeIndex = -1;

        [SerializeField] RangedWeapon[] _mainWeaponInventory = new RangedWeapon[2];
        [SerializeField] MeleeWeapon[] _meleeWeaponInventory = new MeleeWeapon[2];
        [SerializeField] Item[] _itemInventory = new Item[6];

        Ammo _activeAmmo;

        #region Awake to Update

        private void Awake()
        {
            _weaponSlot = transform.root.GetComponentInChildren<PlayerWeaponSlot>();
            _grenadeSlot = transform.root.GetComponentInChildren<PlayerGrenadeSlot>();
            _flashlight = transform.root.GetComponentInChildren<PlayerFlashlight>();

            _weaponSlot.OnWeaponAttack.AddListener(UpdateAmmoHUD);
            _grenadeSlot.OnGrenadeChanged.AddListener(UpdateGrenadeHUD);
        }

        private void Start()
        {
            _uiHUD = UIManager.Instance.HUDPanel;
        }

        private void Update()
        {
            _uiHUD.SetCrosshairSpread(_weaponSlot.CurrentSpread);
        }

        #endregion

        #region Properties

        public PlayerWeapon[] MainWeaponInventory
        {
            get
            {
                return _mainWeaponInventory;
            }
        }

        public PlayerWeapon[] MeleeWeaponInventory
        {
            get
            {
                return _meleeWeaponInventory;
            }
        }

        public Item[] ItemInventory
        {
            get
            {
                return _itemInventory;
            }
        }

        public Ammo ActiveAmmo
        {
            get
            {
                return _activeAmmo;
            }
        }

        public Grenade ActiveGrenade
        {
            get
            {
                return _grenadeSlot.Grenade;
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

        public int ActiveGrenadeIndex
        {
            get
            {
                return _activeGrenadeIndex;
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

        public void OnKilled()
        {
            for (int i = 0; i < _mainWeaponInventory.Length; i++)
            {
                if (_mainWeaponInventory[i] != null)
                    DropItem(i);
            }

            for (int i = 0; i < _meleeWeaponInventory.Length; i++)
            {
                if (_meleeWeaponInventory[i] != null)
                    DropItem(i + 10);
            }

            for (int i = 0; i < _itemInventory.Length; i++)
            {
                if (_itemInventory[i] != null)
                    DropItem(i + 20);
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

        // Returns Local Index
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
            for (int i = 0; i < _mainWeaponInventory.Length; i++)
            {
                if (_mainWeaponInventory[i] != null)
                    return i;
            }

            return -1;
        }

        // Returns Global Index
        private int FindAnyMeleeWeapon()
        {
            for (int i = 0; i < _meleeWeaponInventory.Length; i++)
            {
                if (_meleeWeaponInventory[i] != null)
                    return i + 10;
            }

            return -1;
        }

        // Returns Global Index
        private int FindAnySecondaryWeapon()
        {
            for (int i = 0; i < _itemInventory.Length; i++)
            {
                if (_itemInventory[i] is PlayerWeapon)
                {
                    PlayerWeapon weapon = _itemInventory[i] as PlayerWeapon;

                    if (weapon.Properties.weaponGroup == WeaponGroup.Secondary)
                        return i + 20;
                }
            }

            return -1;
        }

        // Returns GlobalIndex
        private int FindAmmo(AmmoType type)
        {
            for (int i = 0; i < _itemInventory.Length; i++)
            {
                if (_itemInventory[i] is Ammo)
                {
                    Ammo ammo = _itemInventory[i] as Ammo;

                    if (ammo.Type == type)
                        return i + 20;
                }
            }

            return -1;
        }

        // Returns GlobalIndex
        public int FindAnyGrenade()
        {
            for (int i = 0; i < _itemInventory.Length; i++)
            {
                if (_itemInventory[i] is Grenade)
                {
                    return i + 20;
                }
            }

            return -1;
        }

        // Returns GlobalIndex
        private int FindGrenade(GrenadeType type)
        {
            for (int i = 0; i < _itemInventory.Length; i++)
            {
                if (_itemInventory[i] is Grenade)
                {
                    Grenade grenade = _itemInventory[i] as Grenade;

                    if (grenade.Type == type)
                        return i + 20;
                }
            }

            return -1;
        }

        private void EquipWeapon(int globalIndex, bool active)
        {
            if (0 <= globalIndex && globalIndex <= _mainWeaponInventory.Length)
            {
                RangedWeapon newWeapon = _mainWeaponInventory[globalIndex] as RangedWeapon;

                if (newWeapon == null)
                {
                    _uiHUD.ShowResult(false, "Program Logic Error(Indexed Item Null Or Non-Ranged Weapon in the Main Inventory)");
                    return;
                }

                _activeWeaponIndex = globalIndex;
                _activeMainIndex = globalIndex;

                newWeapon.gameObject.SetActive(active);
                newWeapon.transform.SetParent(_weaponSlot.transform);
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = _weaponSlot.transform.rotation;
                _weaponSlot.Weapon = newWeapon;

                _uiHUD.ShowResult(true, newWeapon.Name + " Equipped");

                _activeAmmoIndex = FindAmmo(newWeapon.Properties.ammoType);
                _activeAmmo = GetItem(_activeAmmoIndex) as Ammo;

                _uiHUD.ShowAmmo();
                _uiHUD.SetCurrentAmmo(newWeapon.CurrentAmmo);
                _uiHUD.SetTotalAmmo(_activeAmmo == null ? 0 : _activeAmmo.Count);
            }

            else if (10 <= globalIndex && globalIndex < 10 + _mainWeaponInventory.Length)
            {
                PlayerWeapon newWeapon = _meleeWeaponInventory[globalIndex - 10];

                if (newWeapon == null)
                {
                    _uiHUD.ShowResult(false, "Program Logic Error(Indexed Item Null)");
                    return;
                }

                _activeWeaponIndex = globalIndex;
                _activeMeleeIndex = globalIndex - 10;

                newWeapon.gameObject.SetActive(active);
                newWeapon.transform.SetParent(_weaponSlot.transform);
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = _weaponSlot.transform.rotation;
                _weaponSlot.Weapon = newWeapon;

                _uiHUD.ShowResult(true, newWeapon.Name + " Equipped");

                _activeAmmo = null;
                _activeAmmoIndex = -1;
                _uiHUD.HideAmmo();
            }

            else if (20 <= globalIndex && globalIndex < 20 + _itemInventory.Length)
            {
                Item item = _itemInventory[globalIndex - 20];

                if (item == null)
                {
                    _uiHUD.ShowResult(false, "Program Logic Error(Indexed Item Null)");
                    return;
                }

                if (item is PlayerWeapon)
                {
                    RangedWeapon newWeapon = item as RangedWeapon;

                    // Is the Inventory Correct?
                    if (newWeapon.Properties.weaponGroup != WeaponGroup.Secondary)
                    {
                        _uiHUD.ShowResult(false, "Program Logic Error(Main/Melee Item in Item Inventory)");
                        return;
                    }

                    _activeWeaponIndex = globalIndex;
                    _activeSecondaryIndex = globalIndex - 20;

                    newWeapon.gameObject.SetActive(active);
                    newWeapon.transform.SetParent(_weaponSlot.transform);
                    newWeapon.transform.localPosition = Vector3.zero;
                    newWeapon.transform.rotation = _weaponSlot.transform.rotation;
                    _weaponSlot.Weapon = newWeapon;

                    _uiHUD.ShowResult(true, newWeapon.Name + " Equipped");

                    _activeAmmoIndex = FindAmmo(newWeapon.Properties.ammoType);
                    _activeAmmo = GetItem(_activeAmmoIndex) as Ammo;

                    _uiHUD.ShowAmmo();
                    _uiHUD.SetCurrentAmmo(newWeapon.CurrentAmmo);
                    _uiHUD.SetTotalAmmo(_activeAmmo == null ? 0 : _activeAmmo.Count);
                }

                else
                {
                    _uiHUD.ShowResult(false, "Program Logic Error(Indexed Item Not Weapon");
                    return;
                }
            }
        }

        private void UnequipCurrentWeapon()
        {
            if (_activeWeaponIndex != -1)
            {
                PlayerWeapon current = _weaponSlot.Weapon;
                WeaponGroup group = current.Properties.weaponGroup;

                _activeWeaponIndex = -1;

                if (group == WeaponGroup.Main)
                    _activeMainIndex = -1;

                else if (group == WeaponGroup.Melee)
                    _activeMeleeIndex = -1;

                else if (group == WeaponGroup.Secondary)
                    _activeSecondaryIndex = -1;

                
                _weaponSlot.Weapon = null;

                current.gameObject.SetActive(false);
                current.transform.SetParent(transform);
                current.transform.localPosition = Vector3.zero;
                current.transform.rotation = transform.rotation;

                _activeAmmo = null;
                _activeAmmoIndex = -1;
                UpdateAmmoHUD();

                _uiHUD.ShowResult(true, "Current Weapon Unequipped");
            }

            else
            {
                _uiHUD.ShowResult(false, "No Weapon to Unequip");
            }
        }

        private Item GetItem(int globalIndex)
        {
            // Main Weapon
            if (0 <= globalIndex && globalIndex < _mainWeaponInventory.Length)
            {
                return _mainWeaponInventory[globalIndex];
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + _meleeWeaponInventory.Length)
            {
                return _meleeWeaponInventory[globalIndex - 10];
            }

            else if (20 <= globalIndex && globalIndex < 20 + _itemInventory.Length)
            {
                return _itemInventory[globalIndex - 20];
            }

            else
                return null;
        }

        private void OrganizeInventories()
        {
            RangedWeapon[] mainWeaponInventoryCopy = new RangedWeapon[_mainWeaponInventory.Length];
            MeleeWeapon[] meleeWeaponInventoryCopy = new MeleeWeapon[_meleeWeaponInventory.Length];
            Item[] itemInventoryCopy = new Item[_itemInventory.Length];

            int copy = 0;
            for (int i = 0; i < _mainWeaponInventory.Length; i++)
            {
                if (_mainWeaponInventory[i] != null)
                    mainWeaponInventoryCopy[copy++] = _mainWeaponInventory[i];
            }

            copy = 0;
            for (int i = 0; i < _meleeWeaponInventory.Length; i++)
            {
                if (_meleeWeaponInventory[i] != null)
                    meleeWeaponInventoryCopy[copy++] = _meleeWeaponInventory[i];
            }

            copy = 0;
            for (int i = 0; i < _itemInventory.Length; i++)
            {
                if (_itemInventory[i] != null)
                    itemInventoryCopy[copy++] = _itemInventory[i];
            }

            _mainWeaponInventory = mainWeaponInventoryCopy;
            _meleeWeaponInventory = meleeWeaponInventoryCopy;
            _itemInventory = itemInventoryCopy;

        }

        #endregion

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

        // Reloads Current Weapon 
        // Return if reload animation is necessary
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
                if (_weaponSlot.Weapon is RangedWeapon)
                {
                    RangedWeapon weapon = _weaponSlot.Weapon as RangedWeapon;

                    int availableAmmo = _activeAmmo.Count;
                    int requiredAmmo = weapon.MagazineCapacity - weapon.CurrentAmmo;

                    if (requiredAmmo <= 0)
                        return false;

                    else if (availableAmmo > requiredAmmo)
                    {
                        _activeAmmo.Count -=  requiredAmmo;
                        weapon.CurrentAmmo += requiredAmmo;

                        UpdateAmmoHUD();
                        return true;
                    }

                    else
                    {
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

        public void ChargeBattery(float value)
        {
            _flashlight.ChargeBattery(value);
        }

         // Inventory Interface Related
        public ItemInfo GetItemInfo(int globalIndex)
        {
            // Main Weapon
            if (0 <= globalIndex && globalIndex < _mainWeaponInventory.Length)
            {
                RangedWeapon weapon = _mainWeaponInventory[globalIndex];
                ItemInfo info = new ItemInfo(weapon.Name, weapon.Description,
                                             weapon.DamagaPerPellet.ToString(), weapon.MagazineCapacity.ToString(),
                                             weapon.SpreadStep.ToString(), weapon.RateOfFire.ToString());

                return info;
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + _meleeWeaponInventory.Length)
            {
                PlayerWeapon weapon = _mainWeaponInventory[globalIndex - 10];
                ItemInfo info = new ItemInfo(weapon.Name, weapon.Description,
                                             weapon.DamagaPerPellet.ToString(), "", "", weapon.RateOfFire.ToString());

                return info;
            }

            else if (20 <= globalIndex && globalIndex < 20 + _itemInventory.Length)
            {
                Item item = _itemInventory[globalIndex - 20];

                if (item is RangedWeapon)
                {
                    RangedWeapon weapon = item as RangedWeapon;
                    ItemInfo info = new ItemInfo(weapon.Name, weapon.Description,
                                                 weapon.DamagaPerPellet.ToString(), weapon.MagazineCapacity.ToString(),
                                                 weapon.SpreadStep.ToString(), weapon.RateOfFire.ToString());

                    return info;
                }

                else
                {
                    ItemInfo info = new ItemInfo(item.Name, item.Description, "", "", "", "");
                    return info;
                }
            }

            else
                return new ItemInfo("Wrong Indexing", "Wrong Indexing", "Wrong Indexing", "Wrong Indexing", "Wrong Indexing", "Wrong Indexing");
        }

        public void UseItem(int globalIndex)
        {
            // Main Weapon
            if (0 <= globalIndex && globalIndex < _mainWeaponInventory.Length)
            {
                if (_activeWeaponIndex == -1)
                    EquipWeapon(globalIndex, false);

                else
                {
                    bool wasActive = _weaponSlot.Weapon.gameObject.activeSelf;

                    UnequipCurrentWeapon();
                    EquipWeapon(globalIndex, wasActive);
                }
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + _meleeWeaponInventory.Length)
            {
                if (_activeWeaponIndex == -1)
                    EquipWeapon(globalIndex, false);

                else
                {
                    bool wasActive = _weaponSlot.Weapon.gameObject.activeSelf;
                    UnequipCurrentWeapon();
                    EquipWeapon(globalIndex, wasActive);
                }
            }

            else if (20 <= globalIndex && globalIndex < 20 + _itemInventory.Length)
            {
                int itemIndex = globalIndex - 20;

                Item item = _itemInventory[itemIndex];

                // Is the Indexing Correct?
                if (item == null)
                {
                    _uiHUD.ShowResult(false, "Program Logic Error(Indexed Item Null)");
                    return;
                }

                // Is the Item a Weapon?
                if (item is PlayerWeapon)
                {
                    if (_activeWeaponIndex == -1)
                        EquipWeapon(globalIndex, false);

                    else
                    {
                        bool wasActive = _weaponSlot.Weapon.gameObject.activeSelf;
                        UnequipCurrentWeapon();
                        EquipWeapon(globalIndex, wasActive);
                    }
                }

                else if (item is Grenade)
                {
                    if (ActiveGrenadeIndex == -1)
                    {
                        Grenade grenade = item as Grenade;

                        _grenadeSlot.Grenade = grenade;
                        _activeGrenadeIndex = globalIndex;

                        grenade.gameObject.SetActive(false);
                        grenade.transform.SetParent(_grenadeSlot.transform);
                        grenade.transform.localPosition = Vector3.zero;
                        grenade.transform.rotation = _grenadeSlot.transform.rotation;

                        _uiHUD.ShowResult(true, "Grenade Equipped");
                    }

                    else
                    {
                        //Exchange Locations
                        Grenade prevGrenade = _grenadeSlot.Grenade;

                        prevGrenade.gameObject.SetActive(false);
                        prevGrenade.transform.SetParent(transform);
                        prevGrenade.transform.localPosition = Vector3.zero;
                        prevGrenade.transform.rotation = transform.rotation;

                        Grenade grenade = item as Grenade;

                        grenade.gameObject.SetActive(false);
                        grenade.transform.SetParent(_grenadeSlot.transform);
                        grenade.transform.localPosition = Vector3.zero;
                        grenade.transform.rotation = _grenadeSlot.transform.rotation;

                        _grenadeSlot.Grenade = grenade;
                        _activeGrenadeIndex = globalIndex;


                        _uiHUD.ShowResult(true, "Grenade Equipped");
                    }

                }
                // Item is not a Weapon
                else if (item is Battery)
                {
                    Battery battery = item as Battery;

                    _flashlight.ChargeBattery(battery.amount);
                    DestroyItem(globalIndex);
                }
            }

            else
            {
                _uiHUD.ShowResult(false, "Program Logic Error(Wrong Item Index)");
                return;
            }
        }

        public bool AddItem(Item item)
        {
            if (item is PlayerWeapon)
            {
                PlayerWeapon weapon = item as PlayerWeapon;

                // Main Weapon
                if (weapon.Properties.weaponGroup == WeaponGroup.Main)
                {
                    // Inventory Full
                    if (IsInventoryFull(_mainWeaponInventory))
                    {
                        _uiHUD.ShowResult(false, "Inventory Full");
                        return false;
                    }

                    // Inventory Not FUll
                    else
                    {

                        int emptySlotIndex = GetEmptyItemSlot(_mainWeaponInventory);

                        if (emptySlotIndex == -1)
                        {
                            _uiHUD.ShowResult(false, "Program Logic Error");
                            return false;
                        }

                        else
                        {
                            weapon.ToggleInventoryMode(true);
                            weapon.gameObject.SetActive(false);

                            weapon.transform.SetParent(transform);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = transform.rotation;

                            _mainWeaponInventory[emptySlotIndex] = weapon as RangedWeapon;

                            _uiHUD.ShowResult(true, weapon.name + " Added");
                            return true;
                        }
                    }
                }

                else if (weapon.Properties.weaponGroup == WeaponGroup.Melee)
                {
                    if (IsInventoryFull(_meleeWeaponInventory))
                    {
                        _uiHUD.ShowResult(false, "Inventory Full");
                        return false;
                    }

                    else
                    {

                        int emptySlotIndex = GetEmptyItemSlot(_meleeWeaponInventory);

                        if (emptySlotIndex == -1)
                        {
                            _uiHUD.ShowResult(false, "Program Logic Error");
                            return false;
                        }

                        else
                        {
                            weapon.ToggleInventoryMode(true);
                            weapon.gameObject.SetActive(false);

                            weapon.transform.SetParent(transform);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = transform.rotation;

                            _meleeWeaponInventory[emptySlotIndex] = weapon as MeleeWeapon;
                            _uiHUD.ShowResult(true, weapon.name + " Added");

                            return true;
                        }
                    }
                }

                else if (weapon.Properties.weaponGroup == WeaponGroup.Secondary)
                {
                    if (IsInventoryFull(_itemInventory))
                    {
                        _uiHUD.ShowResult(false, "Inventory Full");
                        return false;
                    }

                    else
                    {

                        int emptySlotIndex = GetEmptyItemSlot(_itemInventory);

                        if (emptySlotIndex == -1)
                        {
                            _uiHUD.ShowResult(false, "Program Logic Error");
                            return false;
                        }

                        else
                        {
                            weapon.ToggleInventoryMode(true);
                            weapon.gameObject.SetActive(false);

                            weapon.transform.SetParent(transform);
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = transform.rotation;

                            _itemInventory[emptySlotIndex] = weapon;
                            _uiHUD.ShowResult(true, weapon.name + " Added");
                            return true;
                        }
                    }
                }
            }

            else
            {
                if (IsInventoryFull(_itemInventory))
                {
                    _uiHUD.ShowResult(false, "Inventory Full");
                    return false;
                }

                else
                {

                    int emptySlotIndex = GetEmptyItemSlot(_itemInventory);

                    if (emptySlotIndex == -1)
                    {
                        _uiHUD.ShowResult(false, "Program Logic Error");
                        return false;
                    }

                    else
                    {
                        if (item is Ammo)
                        {

                            Ammo ammo = item as Ammo;

                            // If there's Another Ammo of the Same Type
                            int ammoIndex = FindAmmo(ammo.Type);

                            Debug.Log(ammoIndex);
                            if (ammoIndex != -1)
                            {
                                Ammo prevAmmo = GetItem(ammoIndex) as Ammo;
                                prevAmmo.Count += ammo.Count;

                                Destroy(ammo.gameObject);
                                UpdateAmmoHUD();

                                _uiHUD.ShowResult(true, prevAmmo.Name + " Added(" + prevAmmo.Count.ToString() + ")");
                                return true;
                            }

                            else
                            {
                                item.ToggleInventoryMode(true);
                                item.gameObject.SetActive(false);

                                item.transform.SetParent(transform);
                                item.transform.localPosition = Vector3.zero;
                                item.transform.localRotation = transform.rotation;

                                _itemInventory[emptySlotIndex] = item;

                                if (_weaponSlot.Weapon != null)
                                {
                                    if (ammo.Type == _weaponSlot.Weapon.Properties.ammoType)
                                    {
                                        _activeAmmo = ammo;
                                        _activeAmmoIndex = emptySlotIndex + 20;
                                        UpdateAmmoHUD();
                                    }
                                }

                                _uiHUD.ShowResult(true, item.Name + " Added(" + ((Ammo) item).Count.ToString() + ")");
                                return true;
                            }
                        }

                        else if (item is Grenade)
                        {
                            Grenade grenade = item as Grenade;

                            // If there's Another Ammo of the Same Type
                            int grenadeIndex = FindGrenade(grenade.Type);

                            if (grenadeIndex != -1)
                            {
                                Grenade prevGrenade = GetItem(grenadeIndex) as Grenade;

                                prevGrenade.Count += grenade.Count;
                                Destroy(grenade.gameObject);

                                UpdateGrenadeHUD();

                                _uiHUD.ShowResult(true, prevGrenade.name + " Added(" + prevGrenade.Count.ToString() + ")");

                                return true;
                            }

                            else
                            {
                                item.ToggleInventoryMode(true);
                                item.gameObject.SetActive(false);

                                item.transform.SetParent(transform);
                                item.transform.localPosition = Vector3.zero;
                                item.transform.localRotation = transform.rotation;

                                _itemInventory[emptySlotIndex] = item;

                                _uiHUD.ShowResult(true, item.name + " Added");

                                if (_grenadeSlot.Grenade == null)
                                    UseItem(emptySlotIndex + 20);

                                return true;
                            }
                        }

                        else
                        {
                            item.ToggleInventoryMode(true);
                            item.gameObject.SetActive(false);

                            item.transform.SetParent(transform);
                            item.transform.localPosition = Vector3.zero;
                            item.transform.localRotation = transform.rotation;

                            _itemInventory[emptySlotIndex] = item;
                            _uiHUD.ShowResult(true, item.name + " Added");

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void DropItem(int globalIndex)
        {
            if (globalIndex == _activeWeaponIndex)
                UnequipCurrentWeapon();

            // Main Weapon
            if (0 <= globalIndex && globalIndex < _mainWeaponInventory.Length)
            {
                Item item = GetItem(globalIndex);

                _mainWeaponInventory[globalIndex] = null;

                item.ToggleInventoryMode(false);
                item.gameObject.SetActive(true);
                item.transform.SetParent(null);
                item.transform.position = transform.root.position + transform.root.forward + transform.root.up;
                item.GetComponent<Rigidbody>().AddForce(transform.root.forward + transform.root.up, ForceMode.Impulse);
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + _meleeWeaponInventory.Length)
            {
                Item item = GetItem(globalIndex);

                _meleeWeaponInventory[globalIndex - 10] = null;

                item.ToggleInventoryMode(false);
                item.gameObject.SetActive(true);
                item.transform.SetParent(null);
                item.transform.position = transform.root.position + transform.root.forward + transform.root.up;
                item.GetComponent<Rigidbody>().AddForce(transform.root.forward + transform.root.up, ForceMode.Impulse);
            }

            else if (20 <= globalIndex && globalIndex < 20 + _itemInventory.Length)
            {
                if (globalIndex == _activeAmmoIndex)
                {
                    _activeAmmo = null;
                    UpdateAmmoHUD();
                }

                else if (globalIndex == _activeGrenadeIndex)
                {
                    _grenadeSlot.Grenade = null;
                    UpdateGrenadeHUD();
                }

                Item item = GetItem(globalIndex);

                _itemInventory[globalIndex - 20] = null;

                item.ToggleInventoryMode(false);
                item.gameObject.SetActive(true);
                item.transform.SetParent(null);
                item.transform.position = transform.root.position + transform.root.forward + transform.root.up;
                item.GetComponent<Rigidbody>().AddForce(transform.root.forward + transform.root.up, ForceMode.Impulse);
            }

            else
            {
                _uiHUD.ShowResult(false, "Program Logic Error(Wrong Item Index)");
                return;
            }

            OrganizeInventories();
        }
        
        public void DestroyItem(int globalIndex)
        {
            if (globalIndex == _activeWeaponIndex)
            {
                _activeWeaponIndex = -1;
                _weaponSlot.Weapon = null;
            }

            if (globalIndex == _activeMainIndex)
                _activeMainIndex = -1;

            else if (globalIndex == _activeMeleeIndex)
                _activeMeleeIndex = -1;

            else if (globalIndex == _activeSecondaryIndex)
                _activeSecondaryIndex = -1;

            else if (globalIndex == _activeAmmoIndex)
            {
                _activeAmmoIndex = -1;
                _activeAmmo = null;

                UpdateAmmoHUD();
            }

            else if (globalIndex == _activeGrenadeIndex)
            {
                _activeGrenadeIndex = -1;
                _grenadeSlot.Grenade = null;
            }

            // Main Weapon
            if (0 <= globalIndex && globalIndex < _mainWeaponInventory.Length)
            {
                Item item = GetItem(globalIndex);
                _mainWeaponInventory[globalIndex] = null;
                Destroy(item.gameObject);
            }

            // Melee Weapon
            else if (10 <= globalIndex && globalIndex < 10 + _meleeWeaponInventory.Length)
            {
                Item item = GetItem(globalIndex);
                _meleeWeaponInventory[globalIndex - 10] = null;
                Destroy(item.gameObject);
            }

            else if (20 <= globalIndex && globalIndex < 20 + _itemInventory.Length)
            {
                Item item = GetItem(globalIndex);
                _itemInventory[globalIndex - 20] = null;
                Destroy(item.gameObject);
            }

            else
            {
                _uiHUD.ShowResult(false, "Program Logic Error(Wrong Item Index), Item Index: " + globalIndex.ToString());
                return;
            }
        }

        public void CombineItem()
        {

        }

        // UI Related
        public void UpdateAmmoHUD()
        {
            if (_weaponSlot.Weapon == null || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee)
            {
                _uiHUD.SetCurrentAmmo(0);
                _uiHUD.SetTotalAmmo(0);
            }

            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main 
                || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary)
            {
                RangedWeapon weapon = _weaponSlot.Weapon as RangedWeapon;

                _uiHUD.SetCurrentAmmo(weapon.CurrentAmmo);
                _uiHUD.SetTotalAmmo(_activeAmmo == null ? 0 : _activeAmmo.Count);
            }
        }

        private void UpdateGrenadeHUD()
        {
            if (_grenadeSlot.Grenade == null)
                _uiHUD.SetCurrentGrenade(0);

            else if (_grenadeSlot.Grenade.Count <= 0)
                DestroyItem(_activeGrenadeIndex);

            else
                _uiHUD.SetCurrentGrenade(_grenadeSlot.Grenade.Count);
        }
    }

    public struct ItemInfo
    {
        public string name;
        public string description;
        public string damage;
        public string capacity;
        public string spread;
        public string rateOfFire;

        public ItemInfo(string name, string description, string damage, string capacity, string spread, string fireRate)
        {
            this.name = name;
            this.description = description;
            this.damage = damage;
            this.capacity = capacity;
            this.spread = spread;
            this.rateOfFire = fireRate;
        }
    }
}
