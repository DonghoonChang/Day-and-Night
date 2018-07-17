using UnityEngine;
using MyGame.UI;
using MyGame.GameManagement;
using MyGame.Inventory;
using MyGame.Inventory.Weapon;

namespace MyGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerAudioPlayer))]
    public class PlayerCharacter : MonoBehaviour
    {
        GameManager _gameManager;
        VFXManager _VFXManager;

        [Range(0, 500)]
        public int health = 200;

        // Components
        PlayerAnimator _animator;
        PlayerInventory _inventory;
        PlayerCamera _camera;

        [SerializeField]
        PlayerStatus _playerStatus;

        [SerializeField]
        Item _currentItem;

        #region Properties

        public PlayerInventory Inventory
        {
            get
            {
                return _inventory;
            }
        }

        public PlayerStatus PlayerStatus
        {
            get
            {
                return _playerStatus;
            }
        }

        #endregion  

        #region Awake to Update

        void Awake()
        {
            _animator = GetComponent<PlayerAnimator>();
            _inventory = GetComponentInChildren<PlayerInventory>();
        }

        void Start()
        {
            _gameManager = GameManager.Instance;
            _VFXManager = VFXManager.Instance;

            _camera = _gameManager.PlayerCamera;
        }

        void Update()
        {
            if (GameTime.isPaused)
                return;

            /* Lower Body */
            SetMovement();
            ToggleCrouch(Input.GetButtonDown("Crouch"));
            ToggleSprint(Input.GetButton("Sprint"), Input.GetAxis("Vertical") > 0);
            ToggleJog(Input.GetButtonDown("Toggle Jog"));


            /* Upper Body */
            ToggleMainWeapon(Input.GetButtonDown("Main Weapon"));
            ToggleMeleeWeapon(Input.GetButtonDown("Melee Weapon"));
            ToggleSecondaryWeapon(Input.GetButtonDown("Secondary Weapon"));
            ToggleAttack(Input.GetButtonDown("Attack"));
            ToggleAutomaticAttack(Input.GetButton("Attack"));
            ToggleReload(Input.GetButtonDown("Reload"));
            ToggleAim(Input.GetButton("Aim"));
            ToggleInteraction(Input.GetButtonDown("Interact"));
        }

        #endregion

        #region Inputs

        void SetMovement()
        {
            _playerStatus.horizontalAxis = Mathf.Clamp(Input.GetAxis("Horizontal"),
                            (_playerStatus.isJogSet && !_playerStatus.isAiming) ? -1.0f : -0.5f,
                            (_playerStatus.isJogSet && !_playerStatus.isAiming) ? 1.0f : 0.5f);

            _playerStatus.verticalAxis = Mathf.Clamp(Input.GetAxis("Vertical"),
                                      (_playerStatus.isJogSet && !_playerStatus.isAiming) || _playerStatus.isSprinting ? -1.0f : -0.5f,
                                      (_playerStatus.isJogSet && !_playerStatus.isAiming) || _playerStatus.isSprinting ? 1.0f : 0.5f);

            if (_playerStatus.isSprinting)
                _playerStatus.horizontalAxis = 0f;
        }

        void ToggleCrouch(bool buttonDown)
        {
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            if (_playerStatus.isCrouching)
                Walk();

            else if (IsWalking())
                Crouch();
        }

        void ToggleJog(bool buttonDown)
        {
            if (!buttonDown)
                return;

            _playerStatus.isJogSet = !_playerStatus.isJogSet;
        }

        void ToggleSprint(bool sprintButton, bool forwardButtonDown)
        {
            if (sprintButton && forwardButtonDown)
            {
                if (_playerStatus.isSprinting)
                    return;

                else
                    Sprint();
            }

            else
            {
                if (_playerStatus.isSprinting)
                    Walk();
            }
        }

        void ToggleMainWeapon(bool buttonDown)
        {
            // No Button Pressed Or Sprinting
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            // No Weapon Equipped
            if (!_inventory.IsWeaponEquipped() && !_animator.isWeaponOut)
            {
                // When No Primary Weapon is Active
                if (_inventory.ActiveMainIndex == -1)
                {
                    // Try to Find One
                    int index = _inventory.FindWeapon(WeaponGroup.Main);

                    // Found
                    if (index != -1)
                    {
                        _inventory.UseItem(index);
                        DrawWeapon();
                    }

                    // Not Found
                    else
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Main Weapon In Inventory"));
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveMainIndex);
                    DrawWeapon();
                }
            }

            // Main Weapon Equipped But Holstered
            else if (_inventory.IsWeaponEquipped() && _inventory.GetCurrentWeaponGroup() == WeaponGroup.Main && !_animator.isWeaponOut)
                DrawWeapon();

            // Main Weapon Equipped And Drawn Out
            else if (_inventory.IsWeaponEquipped() && _inventory.GetCurrentWeaponGroup() == WeaponGroup.Main 
                     && _animator.isWeaponOut && !_animator.isWeaponReloading && !_inventory.IsCurrentWeaponLocked())
                HolsterWeapon();

            // If Non-Main Weapon is Equipped
            else if (_inventory.IsWeaponEquipped() && 
                    (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee || _inventory.GetCurrentWeaponGroup() == WeaponGroup.Secondary)
                    && !_animator.isWeaponReloading)
            {
                // If No Primary Weapon Active
                if (_inventory.ActiveMainIndex == -1)
                {
                    // Try to Find One
                    int index = _inventory.FindWeapon(WeaponGroup.Main);

                    // Found
                    if (index != -1)
                    {
                        _inventory.UseItem(index);
                        DrawWeapon();
                    }

                    // Not Found
                    else
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Main Weapon In Inventory"));
                    }
                }

                // If There is a Primary Weapon Active
                else
                {
                    _inventory.UseItem(_inventory.ActiveMainIndex);
                    DrawWeapon();
                }
            }
        }

        void ToggleMeleeWeapon(bool buttonDown)
        {
            // No Button Pressed Or Sprinting
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            // No Weapon Equipped or Drawn Out
            if (!_inventory.IsWeaponEquipped() && !_animator.isWeaponReloading)
            {
                // When No Primary Weapon is Active
                if (_inventory.ActiveMeleeIndex == -1)
                {
                    // Try to Find One
                    int index = _inventory.FindWeapon(WeaponGroup.Melee);

                    // Found
                    if (index != -1)
                    {
                        _inventory.UseItem(index + 10);
                        DrawWeapon();
                    }

                    // Not Found
                    else
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Main Weapon In Inventory"));
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveMeleeIndex + 10);
                    DrawWeapon();
                }
            }

            // Melee Weapon Equipped But Holstered
            else if (_inventory.IsWeaponEquipped() && _inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee && !_animator.isWeaponOut)
                DrawWeapon();

            // Melee Weapon Equipped And Drawn Out
            else if (_inventory.IsWeaponEquipped() && _inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee
                     && _animator.isWeaponOut && !_animator.isWeaponReloading && !_inventory.IsCurrentWeaponLocked())
                HolsterWeapon();

            // Non-Secondary Weapon is Equipped
            else if (_inventory.IsWeaponEquipped() &&
                    (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Main || _inventory.GetCurrentWeaponGroup() == WeaponGroup.Secondary)
                    && !_animator.isWeaponReloading)
            {
                // No Primary Weapon is Active
                if (_inventory.ActiveMeleeIndex == -1)
                {
                    // Look for One
                    int globalIndex = _inventory.FindWeapon(WeaponGroup.Melee);

                    // Found
                    if (globalIndex != -1)
                    {
                        _inventory.UseItem(globalIndex);
                        DrawWeapon();
                    }

                    // Not Found
                    else
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Melee Weapon In Inventory"));
                    }
                }

                /* There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveMeleeIndex + 10);
                    DrawWeapon();
                }
            }
        }

        void ToggleSecondaryWeapon(bool buttonDown)
        {
            // No Button Pressed Or Sprinting
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            // No Weapon Equipped or Drawn Out
            if (!_inventory.IsWeaponEquipped() && !_animator.isWeaponOut)
            {
                // When No Primary Weapon is Active
                if (_inventory.ActiveSecondaryIndex == -1)
                {
                    // Try to Find One
                    int globalIndex = _inventory.FindWeapon(WeaponGroup.Secondary);

                    // Found
                    if (globalIndex != -1)
                    {
                        Debug.Log(globalIndex);
                        _inventory.UseItem(globalIndex);
                        DrawWeapon();
                    }

                    // Not Found
                    else
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Secondary Weapon In Inventory"));
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveSecondaryIndex + 20);
                    DrawWeapon();
                }
            }

            // Secondary Weapon Equipped But Holstered
            else if (_inventory.IsWeaponEquipped() && _inventory.GetCurrentWeaponGroup() == WeaponGroup.Secondary && !_animator.isWeaponOut)
                DrawWeapon();

            // Secondary Weapon Equipped And Drawn Out
            else if (_inventory.IsWeaponEquipped() && _inventory.GetCurrentWeaponGroup() == WeaponGroup.Secondary
                     && _animator.isWeaponOut && !_animator.isWeaponReloading && !_inventory.IsCurrentWeaponLocked())
                HolsterWeapon();

            // Non-Secondary Weapon  Equipped
            else if (_inventory.IsWeaponEquipped() &&
                    (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Main || _inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee)
                    && !_animator.isWeaponReloading)
            {
                // No Secondary Weapon Active
                if (_inventory.ActiveSecondaryIndex == -1)
                {
                    // Try to Find One
                    int globalIndex = _inventory.FindWeapon(WeaponGroup.Secondary);

                    // Found
                    if (globalIndex != -1)
                    {
                        _inventory.UseItem(globalIndex);
                        DrawWeapon();
                    }

                    // Not Found
                    else
                    {
                        UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Main Weapon In Inventory"));
                    }
                }

                /* There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveSecondaryIndex + 20);
                    DrawWeapon();
                }
            }
        }

        void ToggleAttack(bool buttonDown)
        {
            if (!buttonDown || _playerStatus.isSprinting || !_inventory.IsWeaponEquipped())
                return;

            if (_inventory.IsCurrentWeaponLocked() || _inventory.IsCurrentWeaponAutomatic() || _animator.isWeaponReloading)
                return;

            // No Weapon Equipped
            if (!_inventory.IsWeaponEquipped())
            {
                if (_inventory.ActiveMainIndex != -1)
                    ToggleMainWeapon(true);

                else if (_inventory.ActiveSecondaryIndex != -1)
                    ToggleSecondaryWeapon(true);

                else if (_inventory.ActiveMeleeIndex != -1)
                    ToggleMeleeWeapon(true);

                else
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Player Has No Weapon Equipped"));
            }

            // Weapon Equipped
            else if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Main && _animator.isWeaponOut)
                Attack();

            else if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Secondary
                     && _animator.isWeaponOut && _playerStatus.isAiming)
                Attack();

            else if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee && _animator.isWeaponReloading)
            {
                Attack();
            }
        }

        void ToggleAutomaticAttack(bool button)
        {
            if (!button || _playerStatus.isSprinting || !_inventory.IsWeaponEquipped())
                return;

            if (_inventory.IsCurrentWeaponLocked() || !_inventory.IsCurrentWeaponAutomatic())
                return;

            if (!_inventory.IsWeaponEquipped())
                return;

            // Weapon Equipped
            else if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Main && _animator.isWeaponOut)
                Attack();

            else if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Secondary
                     && _animator.isWeaponOut && _playerStatus.isAiming)
                Attack();

            else if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee && _animator.isWeaponOut)
                Attack();
        }

        void ToggleReload(bool buttonDown)
        {

            if (!buttonDown || !_inventory.IsWeaponEquipped())
                return;

            if (_inventory.GetCurrentWeaponGroup() == WeaponGroup.Melee || !_animator.isWeaponOut || _animator.isWeaponReloading)
                return;

            if (_inventory.ActiveAmmo == null)
                UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Insufficient Ammo"));

            else
            {
                if (_inventory.Reload())
                {
                    Reload();
                }

                else
                {
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Insufficient Ammo"));
                }
            }
        }

        void ToggleAim(bool button)
        {
            if (button)
            {
                if (!_playerStatus.isAiming)
                {
                    _animator.AimAnimation(button);
                    _playerStatus.isAiming = true;
                }
            }

            else
            {
                if (_playerStatus.isAiming)
                {
                    _animator.AimAnimation(button);
                    _playerStatus.isAiming = false;
                }
            }
        }

        void ToggleInteraction(bool buttonDown)
        {
            if (_camera.InteractableObject == null)
            {
                UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "No Object to Interact with"));
                return;
            }

            if (buttonDown)
            {
                if (_camera.InteractableObject.IsInteractable)
                    _camera.InteractableObject.Interact();

                else
                    UIErrorPanel.ReportResult(new ObjectInteractionResult(false, "Object not interactbale"));
            }
        }
        #endregion

        #region Helpers (Animation)

        void Walk()
        {
            _playerStatus.isCrouching = false;
            _playerStatus.isSprinting = false;

            _animator.Walk();
            _VFXManager.StopVFXTime();
        }

        void Crouch()
        {
            _playerStatus.isCrouching = true;
            _playerStatus.isSprinting = false;

            _animator.Crouch();
            _VFXManager.StartVFXTime();
        }

        void Sprint()
        {
            _playerStatus.isCrouching = false;
            _playerStatus.isSprinting = true;

            _animator.Sprint();
            _VFXManager.StopVFXTime();
        }

        bool IsWalking()
        {
            return !_playerStatus.isCrouching && !_playerStatus.isSprinting;
        }

        void HolsterWeapon()
        {
            _animator.HolsterWeapon();
        }

        void DrawWeapon()
        {
            _animator.DrawWeapon();
        }

        void Attack()
        {
            _animator.Attack();
        }

        void Reload()
        {
            _animator.Reload();
        }

        #endregion

        #region Inventory

        public bool AddItem(Item item)
        {
            return _inventory.AddItem(item);
        }

        #endregion
    }

    [System.Serializable]
    public class PlayerStatus
    {
        public bool isJogSet = true;
        public bool isAiming = false;
        public bool isCrouching = false;
        public bool isSprinting = false;

        public float verticalAxis = 0f;
        public float horizontalAxis = 0f;
    }


}

