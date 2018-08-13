using UnityEngine;
using MyGame.Interface.ITakeHit;
using MyGame.Object;
using UIManager = MyGame.GameManagement.UIManager;
using VFXManager = MyGame.GameManagement.VFXManager;
using GameManager = MyGame.GameManagement.GameManager;
using CameraManager = MyGame.GameManagement.CameraManager;
using UIHUD = MyGame.UI.UIHUD;
using UIFeedbackPanel = MyGame.UI.UIFeedbackPanel;
using EnemyCharacter = MyGame.Enemy.EnemyCharacter;


namespace MyGame.Player
{
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerAudioPlayer))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour, ITakeHit
    {

        #region Settings

        static float DamageImmuneTime = 0.3f;
        static float MaxStamina = 5f; // Sprinting duration in Sec
        static float RecoveryStamina = 0.3f; // Portion from Max Stamina until player can sprint again

        static float MinorDamageHealthMultiplier = .15f;
        static float MediumDamageHealthMultipler = .3f;
        static float MajorDamageHealthMultipler = .45f;

        static float DefaultNoiseMultiplier = 1f;
        static float CrouchNoiseMultiplier = .5f;
        static float SprintNoiseMultiplier = 2f;

        #endregion

        // Managers
        UIHUD _uiHUD;
        VFXManager _vfxManager;
        GameManager _gameManager;

        public float currentHealth = 100f;
        public float maxHealth = 100f;

        // Components
        PlayerCamera _camera;
        PlayerAnimator _animator;

        PlayerInventory _inventory;
        PlayerWeaponSlot _weaponSlot;
        PlayerFlashlight _flashlight;

        public PlayerStatus _playerStatus = new PlayerStatus();

        [SerializeField]
        float _stamina = MaxStamina;
        float _damageImmuneTime = 0f;


        #region Properties

        public PlayerCamera Camera
        {
            get
            {
                return _camera;
            }
        }

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

        public float NoiseLevel
        {
            get
            {
                return 0.5f
                       + (_playerStatus.isSprinting ? SprintNoiseMultiplier : _playerStatus.isCrouching ? CrouchNoiseMultiplier : 1f)
                       * Mathf.Sqrt(Mathf.Pow(_playerStatus.verticalAxis, 2f) + Mathf.Pow(_playerStatus.horizontalAxis, 2f));
            }
        }

        #endregion  

        #region Awake to Update

        void Awake()
        {
            _animator = GetComponent<PlayerAnimator>();
            _inventory = GetComponentInChildren<PlayerInventory>();
            _flashlight = GetComponentInChildren<PlayerFlashlight>();
            _weaponSlot = GetComponentInChildren<PlayerWeaponSlot>();

            _playerStatus.isWalking = true;
        }

        void Start()
        {
            _vfxManager = VFXManager.Instance;
            _gameManager = GameManager.Instance;

            _uiHUD = UIManager.Instance.HUDPanel;
            _camera = CameraManager.Instance.PlayerCamera;
        }

        void Update()
        {
            if (GameTime.isPaused || _playerStatus.isDead)
                return;

            // Input 
            // Lower Body
            SetMovement();
            TriggerCrouch(Input.GetButtonDown("Crouch"));
            TriggerJog(Input.GetButtonDown("Toggle Jog"));
            TriggerSprint(Input.GetButton("Sprint"), Input.GetAxis("Vertical") > 0);
            TriggerGrenade(Input.GetButtonDown("Grenade"), Input.GetButton("Grenade"), Input.GetButtonUp("Grenade"));


            // Upper Body
            TriggerAim(Input.GetButton("Aim"));
            TriggerReload(Input.GetButtonDown("Reload"));
            TriggerInteract(Input.GetButtonDown("Interact"));
            TriggerFlashlight(Input.GetButtonDown("Flashlight"));

            TriggerAttack(Input.GetButtonDown("Attack"));
            TriggerAutomaticAttack(Input.GetButton("Attack"));

            TriggerMainWeapon(Input.GetButtonDown("Main Weapon"));
            TriggerMeleeWeapon(Input.GetButtonDown("Melee Weapon"));
            TriggerSecondaryWeapon(Input.GetButtonDown("Secondary Weapon"));

            // Stamina & Battery
            if (_playerStatus.isSprinting)
            {
                _stamina = Mathf.Max(_stamina - GameTime.deltaTime, 0);

                if (_stamina == 0)
                {
                    _playerStatus.isRecoveringStamina = true;
                    // Play Sound
                }
            }

            else
            {
                _stamina = Mathf.Min(_stamina + GameTime.deltaTime * 0.75f, MaxStamina);

                if (_stamina / MaxStamina > RecoveryStamina)
                    _playerStatus.isRecoveringStamina = false;
            }

            _uiHUD.SetStaminaBarValues(_stamina / MaxStamina, (int) ((_stamina / MaxStamina) * 100));
            _uiHUD.SetBatteryBarValues(_flashlight.BatteryLife / _flashlight.MaxBatteryLife, (int) ((_flashlight.BatteryLife / _flashlight.MaxBatteryLife) * 100));
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

        void TriggerCrouch(bool buttonDown)
        {
            if (!buttonDown
                || _playerStatus.isSprinting)
                return;

            if (_playerStatus.isCrouching)
                ToggleWalkAnimation();

            else if (_playerStatus.isWalking)
                ToggleCrouchAnimation();
        }

        void TriggerJog(bool buttonDown)
        {
            if (!buttonDown)
                return;

            _playerStatus.isJogSet = !_playerStatus.isJogSet;
        }

        void TriggerSprint(bool sprintButton, bool forwardButtonDown)
        {
            if (sprintButton && forwardButtonDown && !_playerStatus.isRecoveringStamina)
            {
                if (_playerStatus.isSprinting)
                    return;

                else
                    ToggleSprintAnimation();
            }

            else
            {
                if (_playerStatus.isSprinting)
                    ToggleWalkAnimation();
            }
        }

        void TriggerMainWeapon(bool buttonDown)
        {
            // No Button Pressed Or Sprinting
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            // No Weapon Equipped
            if (_weaponSlot.Weapon == null)
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
                        DrawWeaponAnimation();
                    }

                    // Not Found
                    else
                    {
                        _uiHUD.ShowResult(false, "No Main Weapon In Inventory");
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveMainIndex);
                    DrawWeaponAnimation();
                }
            }

            // Main Weapon Equipped But Holstered
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main
                     && _animator.animationStatus.isHoldingNothing)
                DrawWeaponAnimation();

            // Main Weapon Equipped And Drawn Out
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main
                     && (_animator.animationStatus.isHoldingRangedWeaponHips || _animator.animationStatus.isHoldingRangedWeaponIronsight))
                HolsterWeaponAnimation();

            // If Non-Main Weapon is Equipped and Not Firing or Reloading
            else if ((_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary)
                     && (!_animator.animationStatus.inAttackAnimation ||!_animator.animationStatus.inReloadAnimation))
            {
                // If No Main Weapon Active
                if (_inventory.ActiveMainIndex == -1)
                {
                    // Try to Find One
                    int index = _inventory.FindWeapon(WeaponGroup.Main);

                    // Found
                    if (index != -1)
                    {
                        _inventory.UseItem(index);
                        DrawWeaponAnimation();
                    }

                    // Not Found
                    else
                    {
                        _uiHUD.ShowResult(false, "No Main Weapon In Inventory");
                    }
                }

                // If There is a Main Weapon Active
                else
                {
                    _inventory.UseItem(_inventory.ActiveMainIndex);
                    DrawWeaponAnimation();
                }
            }
        }

        void TriggerMeleeWeapon(bool buttonDown)
        {
            // No Button Pressed Or Sprinting
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            // No Weapon Equipped
            if (_weaponSlot.Weapon == null)
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
                        DrawWeaponAnimation();
                    }

                    // Not Found
                    else
                    {
                        _uiHUD.ShowResult(false, "No Main Weapon In Inventory");
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveMeleeIndex + 10);
                    DrawWeaponAnimation();
                }
            }

            // Melee Weapon Equipped But Holstered
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee
                     && _animator.animationStatus.isHoldingNothing)
                DrawWeaponAnimation();

            // Melee Weapon Equipped And Drawn Out
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee
                     && _animator.animationStatus.isHoldingMeleeWeapon)
                HolsterWeaponAnimation();

            // Non-Secondary Weapon is Equipped
            else if ((_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary)
                     && (!_animator.animationStatus.inAttackAnimation || !_animator.animationStatus.inReloadAnimation))
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
                        DrawWeaponAnimation();
                    }

                    // Not Found
                    else
                    {
                        _uiHUD.ShowResult(false, "No Melee Weapon In Inventory");
                    }
                }

                /* There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveMeleeIndex + 10);
                    DrawWeaponAnimation();
                }
            }
        }

        void TriggerSecondaryWeapon(bool buttonDown)
        {
            // No Button Pressed Or Sprinting
            if (!buttonDown || _playerStatus.isSprinting)
                return;

            // No Weapon Equipped or Drawn Out
            if (_weaponSlot.Weapon == null)
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
                        DrawWeaponAnimation();
                    }

                    // Not Found
                    else
                    {
                        _uiHUD.ShowResult(false, "No Secondary Weapon In Inventory");
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveSecondaryIndex + 20);
                    DrawWeaponAnimation();
                }
            }

            // Secondary Weapon Equipped But Holstered
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary
                     && _animator.animationStatus.isHoldingNothing)
                DrawWeaponAnimation();

            // Secondary Weapon Equipped And Drawn Out
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary
                     && (_animator.animationStatus.isHoldingRangedWeaponHips || _animator.animationStatus.isHoldingRangedWeaponIronsight))
                HolsterWeaponAnimation();

            // Non-Secondary Weapon  Equipped
            else if ((_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee)
                     && (!_animator.animationStatus.inAttackAnimation || !_animator.animationStatus.inReloadAnimation))
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
                        DrawWeaponAnimation();
                    }

                    // Not Found
                    else
                    {
                        _uiHUD.ShowResult(false, "No Main Weapon In Inventory");
                    }
                }

                /* There is a Primary Weapon Active */
                else
                {
                    _inventory.UseItem(_inventory.ActiveSecondaryIndex + 20);
                    DrawWeaponAnimation();
                }
            }
        }

        void TriggerAttack(bool buttonDown)
        {
            if (!buttonDown
                ||_playerStatus.isSprinting
                || _weaponSlot.Weapon == null
                || (_weaponSlot.IsAutomatic && _weaponSlot.CurrentAmmo != 0)
                ||_weaponSlot.IsAttackLocked
                || _animator.animationStatus.isHoldingGrenade
                || _animator.animationStatus.inDrawAnimation
                || _animator.animationStatus.inHolsterAnimation
                || _animator.animationStatus.inReloadAnimation)
                return;

            // Weapon Equipped and Holstered
            if (_weaponSlot.Weapon != null
                && _animator.animationStatus.isHoldingNothing)
            {
                if (_inventory.ActiveMainIndex != -1)
                    TriggerMainWeapon(true);

                else if (_inventory.ActiveSecondaryIndex != -1)
                    TriggerSecondaryWeapon(true);

                else if (_inventory.ActiveMeleeIndex != -1)
                    TriggerMeleeWeapon(true);

                else
                    _uiHUD.ShowResult(false, "Player Has No Weapon Equipped");
            }

            // Weapon Equipped
            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main
                     && (_animator.animationStatus.isHoldingRangedWeaponHips || _animator.animationStatus.isHoldingRangedWeaponIronsight))
                ToggleAttackAnimation();

            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary
                     && (_animator.animationStatus.isHoldingRangedWeaponIronsight && _playerStatus.isAiming))
                ToggleAttackAnimation();

            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee
                     && _animator.animationStatus.inAttackAnimation)
                ToggleAttackAnimation();
        }

        void TriggerAutomaticAttack(bool button)
        {
            if (!button
                || _playerStatus.isSprinting
                || _weaponSlot.Weapon == null
                || !_weaponSlot.IsAutomatic
                || _weaponSlot.IsAttackLocked
                || _weaponSlot.CurrentAmmo == 0
                || _animator.animationStatus.isHoldingGrenade
                || _animator.animationStatus.inReloadAnimation
                || _animator.animationStatus.inDrawAnimation
                || _animator.animationStatus.inHolsterAnimation)
                return;

            // Weapon Equipped
            else if (_weaponSlot.Properties.weaponGroup == WeaponGroup.Main
                     && (_animator.animationStatus.isHoldingRangedWeaponHips || _animator.animationStatus.isHoldingRangedWeaponIronsight))
                ToggleAttackAnimation();

            else if (_weaponSlot.Properties.weaponGroup == WeaponGroup.Secondary
                     && _playerStatus.isAiming
                     && _animator.animationStatus.isHoldingRangedWeaponIronsight)
                ToggleAttackAnimation();

            else if (_weaponSlot.Properties.weaponGroup == WeaponGroup.Melee
                     && _animator.animationStatus.isHoldingMeleeWeapon)
                ToggleAttackAnimation();
        }

        void TriggerReload(bool buttonDown)
        {
            if (!buttonDown
                || _weaponSlot.Weapon == null
                || _weaponSlot.Properties.weaponGroup == WeaponGroup.Melee
                || _animator.animationStatus.isHoldingGrenade
                || _animator.animationStatus.inAttackAnimation
                || _animator.animationStatus.inReloadAnimation
                || _animator.animationStatus.inDrawAnimation
                || _animator.animationStatus.inHolsterAnimation)
                return;

            else
            {
                if (_inventory.Reload())
                    ToggleReloadAnimation();
            }
        }

        void TriggerAim(bool button)
        {
            if (button)
            {
                if (!_playerStatus.isAiming)
                {
                    _playerStatus.isAiming = true;
                    _animator.AimAnimation(button);
                }
            }

            else
            {
                if (_playerStatus.isAiming)
                {
                    _playerStatus.isAiming = false;
                    _animator.AimAnimation(button);
                }
            }
        }

        void TriggerInteract(bool buttonDown)
        {
            if (!buttonDown)
                return;

            if (_camera.InteractableObject == null)
                _uiHUD.ShowResult(false, "No Object to Interact With");

            else if (_camera.InteractableObject.IsInteractable)
                _camera.InteractableObject.Interact();

            else
                _uiHUD.ShowResult(false, "Object Not Interactable");
        }

        void TriggerFlashlight(bool buttonDown)
        {
            if (buttonDown)
                _flashlight.SwitchIntensity();
        }

        void TriggerGrenade(bool buttonDown, bool button, bool buttonUp)
        {
            if ((!buttonDown && !buttonUp)
                || _animator.animationStatus.inReloadAnimation
                || _animator.animationStatus.inDrawAnimation
                || _animator.animationStatus.inHolsterAnimation)
                return;

            if (buttonDown)
            {
                // No Grenade Equipped
                if (_inventory.ActiveGrenade == null)
                {
                    // Look for One
                    int grenadeIndex = _inventory.FindAnyGrenade();

                    // Found
                    if (grenadeIndex != -1)
                    {
                        _inventory.UseItem(grenadeIndex);
                        _animator.GrenadeAnimation(true);
                    }

                    // Not Found
                    else
                        return;
                }

                // Grenade Equipped
                else
                    _animator.GrenadeAnimation(true);
            }

            else if (button)
            {

            }

            else if (buttonUp)
            {
                if (_inventory.ActiveGrenade == null)
                    return;

                else if (_animator.animationStatus.isHoldingGrenade)
                    _animator.GrenadeAnimation(false);
            }
        }

        #endregion

        #region Helpers (Animation)

        void ToggleWalkAnimation()
        {
            _playerStatus.isWalking = true;
            _playerStatus.isCrouching = false;
            _playerStatus.isSprinting = false;

            _animator.WalkAnimation();
            _vfxManager.StopVFXTime();
        }

        void ToggleCrouchAnimation()
        {
            _playerStatus.isWalking = false;
            _playerStatus.isCrouching = true;
            _playerStatus.isSprinting = false;

            _animator.CrouchAnimation();
            _vfxManager.StartVFXTime();
        }

        void ToggleSprintAnimation()
        {
            _playerStatus.isWalking = false;
            _playerStatus.isCrouching = false;
            _playerStatus.isSprinting = true;

            _animator.SprintAnimation();
            _vfxManager.StopVFXTime();
        }

        void ToggleAttackAnimation()
        {
            if (_weaponSlot.Attack())
                _animator.AttackAnimation();
        }

        void ToggleReloadAnimation()
        {
            _animator.ReloadAnimation();
        }

        void DrawWeaponAnimation()
        {
            _animator.DrawWeaponAnimation();
        }

        void HolsterWeaponAnimation()
        {
            _animator.HolsterWeaponAnimation();
        }

        #endregion

        #region Helpers (Inventory)

        public bool AddItem(Item item)
        {
            return _inventory.AddItem(item);
        }

        public bool IsCurrentWeaponScoped()
        {
            if (_weaponSlot.Weapon == null 
                || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Melee)
                return false;

            else if (_weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Main
                     || _weaponSlot.Weapon.Properties.weaponGroup == WeaponGroup.Secondary)
            {
                RangedWeapon weapon = _weaponSlot.Weapon as RangedWeapon;

                return weapon.modificationEffects.isScoped;
            }

            else
                return false;
            
        }

        #endregion

        #region Take Damage, Dodge and Collision

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.isTrigger)
                return;

            EnemyCharacter enemy = collision.transform.root.GetComponent<EnemyCharacter>();

            if (enemy != null)
            {
                // Ignore dead bodies
                if (enemy.currentBehavior == Enemy.EnemyBehavior.Dead)
                    return;

                float magnitude = collision.relativeVelocity.magnitude;

                // Filter out small collisions
                if (magnitude < 5f)
                    return;

                else
                    magnitude = Mathf.Max(magnitude, 15f); // Upper limit for damage

                _animator.PushPlayer(magnitude, transform.position - collision.transform.position);

                float damage = enemy.Damage * (magnitude - 5f) * _gameManager.DamageMultiplier;

                TakeDamage(enemy.Damage  * magnitude * _gameManager.DamageMultiplier);
            }

        }

        public void TakeDamage(float damage)
        {
            if (_damageImmuneTime != 0 && Time.time < _damageImmuneTime)
                return;

            else
                _damageImmuneTime = Time.time + DamageImmuneTime;

            currentHealth = Mathf.Max(currentHealth - (int)damage, 0);

            // Minor Damage
            if (damage < maxHealth * MinorDamageHealthMultiplier)
                _camera.ShakeCameraDamageMinor();

            // Medium Damage
            else if (damage < maxHealth * MediumDamageHealthMultipler)
                _camera.ShakeCameraDamageMedium();

            // Larger than Medium Damage and Damage from Zombies
            else
                _camera.ShakeCameraDamageMajor();


            if (currentHealth == 0)
            {
                _playerStatus.isDead = true;

                _animator.OnKilled();
                _inventory.OnKilled();
            }

            _uiHUD.SetHealthBarValues(currentHealth/maxHealth, (int) currentHealth);
        }

        // Non Zombie Damages
        public void OnHit(Transform[] transformList, Vector3[] normalList, float damage, float concussion, bool applyDamageOnce)
        {
            TakeDamage(damage);
        }

        #endregion

    }

    [System.Serializable]
    public class PlayerStatus
    {
        public bool isDead = false;
        public bool isJogSet = true;
        public bool isAiming = false;

        public bool isWalking = false;
        public bool isCrouching = false;
        public bool isSprinting = false;
        public bool isRecoveringStamina = false;

        public float dodgeDamage = 0f;
        public bool isDodgingSuccessful = false;
        public bool isDodgeTimeActivated = false;

        public float verticalAxis = 0f;
        public float horizontalAxis = 0f;
    }


}

