using UnityEngine;
using MyGame.Inventory;
using MyGame.Inventory.Weapon;
using GameManager = MyGame.GameManagement.GameManager;

namespace MyGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimator))]
    public class Player : MonoBehaviour
    {

        /*
         * Player Logic and Input Management
         */

        GameManager gameManager;


        /* Inspector Variables */
        [SerializeField]
        [Range(0, 500)]
        int health = 200;

        /* Components */
        PlayerAnimator animator;
        PlayerInventory inventory;

        [SerializeField]
        PlayerStatus playerStatus;


        #region Properties

        public Transform CameraPivot
        {
            get
            {
                return animator.CameraPivot;
            }
        }

        public PlayerStatus PlayerStatus
        {
            get
            {
                return playerStatus;
            }
        }

        #endregion  

        #region Awake to Update

        void Awake()
        {
            animator = GetComponent<PlayerAnimator>();
            inventory = GetComponentInChildren<PlayerInventory>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
        }

        void Update()
        {
            /* Lower Body */
            ToggleCrouch(Input.GetButtonDown("Crouch"));
            ToggleSprint(Input.GetButton("Sprint"), Input.GetAxis("Vertical") > 0);
            ToggleJog(Input.GetButtonDown("Toggle Jog"));


            /* Upper Body */
            TogglePrimaryWeapon(Input.GetButtonDown("Primary Weapon"));
            ToggleSecondaryWeapon(Input.GetButtonDown("Secondary Weapon"));
            ToggleMeleeWeapon(Input.GetButtonDown("Melee Weapon"));
            ToggleAttack(Input.GetButtonDown("Attack"));
            ToggleAutomaticAttack(Input.GetButton("Attack"));
            ToggleReload(Input.GetButtonDown("Reload"));
            ToggleAim(Input.GetButton("Aim"));
        }


        #endregion

        #region Animation Toggles

        void ToggleCrouch(bool buttonDown)
        {
            if (!buttonDown || playerStatus.lowerBody.isSprinting)
                return;

            if (playerStatus.lowerBody.isCrouching)
                Walk();

            else if (IsWalking())
                Crouch();
        }

        void ToggleJog(bool buttonDown)
        {
            if (!buttonDown)
                return;

            playerStatus.lowerBody.isJogSet = !playerStatus.lowerBody.isJogSet;
        }

        void ToggleSprint(bool sprintButton, bool forwardButtonDown)
        {
            if (sprintButton && forwardButtonDown)
            {
                if (playerStatus.lowerBody.isSprinting)
                    return;

                else
                    Sprint();
            }

            else
            {
                if (playerStatus.lowerBody.isSprinting)
                    Walk();
            }
        }

        void TogglePrimaryWeapon(bool buttonDown)
        {
            if (!buttonDown || playerStatus.lowerBody.isSprinting)
                return;

            if (playerStatus.upperBody.isPrimaryWeaponOut)
                HolsterWeapon();

            /* If Non-Primary Weapon is Drawn Out */
            else if (playerStatus.upperBody.isSecondaryWeaponOut || playerStatus.upperBody.isMeleeWeaponOut)
            {
                /* If No Primary Weapon Active */
                if (inventory.activePrimary == null)
                {

                    /* Try to Find One */
                    int index = inventory.FindWeapon(WeaponGroup.Primary);

                    /* Found */
                    if (index != -1)
                    {
                        Weapon weapon = inventory.GetItemByIndex(index) as Weapon;
                        inventory.EquipWeapon(weapon);
                        DrawCurrentWeapon();
                    }

                    /* Not Found */
                    else
                    {
                        Debug.Log("Player Does Not Have Any Primary Weapon");
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    inventory.EquipWeapon(inventory.activePrimary);
                    DrawCurrentWeapon();
                }
            }

            /* If There is No Weapon Out */
            else if (!IsWeaponOut())
            {
                /* When No Primary Weapon is Active */
                if (inventory.activePrimary == null)
                {

                    /* Try to Find One */
                    int index = inventory.FindWeapon(WeaponGroup.Primary);

                    /* Found */
                    if (index != -1)
                    {
                        Weapon weapon = inventory.GetItemByIndex(index) as Weapon;
                        inventory.EquipWeapon(weapon);
                        DrawCurrentWeapon();
                    }

                    /* Not Found */
                    else
                    {
                        Debug.Log("Player Does Not Have Any Primary Weapon");
                    }
                }

                /* If There is a Primary Weapon Active */
                else
                {
                    inventory.EquipWeapon(inventory.activePrimary);
                    DrawCurrentWeapon();
                }
            }

           
        }

        void ToggleSecondaryWeapon(bool buttonDown)
        {
            if (!buttonDown || playerStatus.lowerBody.isSprinting)
                return;

            if (playerStatus.upperBody.isSecondaryWeaponOut)
                HolsterWeapon();

            /* If a Non-Secondary Weapon is Drawn Out */
            else if (playerStatus.upperBody.isPrimaryWeaponOut || playerStatus.upperBody.isMeleeWeaponOut)
            {
                /* If No Secondary Weapon Active */
                if (inventory.activeSecondary == null)
                {
                    /* Try to Find One */
                    int index = inventory.FindWeapon(WeaponGroup.Secondary);

                    /* Found */
                    if (index != -1)
                    {
                        Weapon weapon = inventory.GetItemByIndex(index) as Weapon;
                        inventory.EquipWeapon(weapon);
                        DrawCurrentWeapon();
                    }

                    /* Not Found */
                    else
                    {
                        Debug.Log("Player Does Not Have Any Secondary Weapon");
                    }
                }

                /* If There is a Secondary Weapon Active */
                else
                {
                    inventory.EquipWeapon(inventory.activeSecondary);
                    DrawCurrentWeapon();
                }
            }

            /* If There is No Weapon Out */
            else if (!IsWeaponOut())
            {
                /* If No Secondary Weapon Active */
                if (inventory.activeSecondary == null)
                {
                    /* Try to Find One */
                    int index = inventory.FindWeapon(WeaponGroup.Secondary);

                    /* Found */
                    if (index != -1)
                    {
                        Weapon weapon = inventory.GetItemByIndex(index) as Weapon;
                        inventory.EquipWeapon(weapon);
                        DrawCurrentWeapon();
                    }

                    /* Not Found */
                    else
                    {
                        Debug.Log("Player Does Not Have Any Secondary Weapon");
                    }
                }

                /* If There is a Secondary Weapon Active */
                else
                {
                    inventory.EquipWeapon(inventory.activeSecondary);
                    DrawCurrentWeapon();
                }
            }
        }

        void ToggleMeleeWeapon(bool buttonDown)
        {
            if (!buttonDown || playerStatus.lowerBody.isSprinting)
                return;

            if (playerStatus.upperBody.isMeleeWeaponOut)
                HolsterWeapon();

            else
            {
                if (inventory.activeMelee == null)
                {
                    int index = inventory.FindWeapon(WeaponGroup.Melee);

                    if (index != -1)
                    {
                        Weapon weapon = inventory.GetItemByIndex(index) as Weapon;
                        inventory.EquipWeapon(weapon);
                        DrawCurrentWeapon();
                    }

                    else
                    {
                        Debug.Log("Player Does Not Have Any Melee Weapon");
                    }
                }

                else
                {
                    DrawCurrentWeapon();
                }
            }
        }

        void ToggleAttack(bool buttonDown)
        {
            if (!buttonDown || playerStatus.lowerBody.isSprinting || inventory.CurrentWeapon == null)
                return;

            if (inventory.CurrentWeapon.IsAttackLocked || inventory.CurrentWeapon.Properties.isAutomatic)
                return;

            if (!IsWeaponOut())
            {
                if (inventory.activePrimary != null)
                    TogglePrimaryWeapon(true);

                else if (inventory.activeSecondary != null)
                    ToggleSecondaryWeapon(true);

                else if (inventory.activeMelee != null)
                    ToggleMeleeWeapon(true);

                else
                    Debug.Log("Player Does Not Have Any Weapon");
            }

            /* If weapon equipped */
            if (playerStatus.upperBody.isPrimaryWeaponOut)
                ToggleAttack();

            else if (playerStatus.upperBody.isSecondaryWeaponOut && playerStatus.upperBody.isAiming)
                ToggleAttack();

            else if (playerStatus.upperBody.isMeleeWeaponOut)
            {
                /* FLAG: DO SOMETHING */
            }
        }

        void ToggleAutomaticAttack(bool button)
        {
            if (!button || playerStatus.lowerBody.isSprinting || inventory.CurrentWeapon == null)
                return;

            if (inventory.CurrentWeapon.IsAttackLocked || !inventory.CurrentWeapon.Properties.isAutomatic)
                return;

            if (!IsWeaponOut())
                return;

            if (!IsWeaponOut())
            {
                if (inventory.activePrimary != null)
                    TogglePrimaryWeapon(true);

                else if (inventory.activeSecondary != null)
                    ToggleSecondaryWeapon(true);

                else if (inventory.activeMelee != null)
                    ToggleMeleeWeapon(true);

                else
                    Debug.Log("Player Does Not Have Any Weapon");
            }

            /* If weapon equipped */
            if (playerStatus.upperBody.isPrimaryWeaponOut)
                ToggleAttack();

            else if (playerStatus.upperBody.isSecondaryWeaponOut && playerStatus.upperBody.isAiming)
                ToggleAttack();

            else if (playerStatus.upperBody.isMeleeWeaponOut)
            {
                /* FLAG: DO SOMETHING */
            }
        }


        void ToggleReload(bool buttonDown)
        {
            if (!buttonDown || !(playerStatus.upperBody.isPrimaryWeaponOut || playerStatus.upperBody.isSecondaryWeaponOut))
                return;

            Reload();
        }

        void ToggleAim(bool button)
        {
            if (button)
            {
                if (!playerStatus.upperBody.isAiming)
                {
                    animator.AimAnimation(button);
                    playerStatus.upperBody.isAiming = true;
                }
            }

            else
            {
                if (playerStatus.upperBody.isAiming)
                {
                    animator.AimAnimation(button);
                    playerStatus.upperBody.isAiming = false;
                }
            }
        }


        #endregion

        #region Toggle Helpers


        /* Movement */

        void Walk()
        {
            playerStatus.lowerBody.isCrouching = false;
            playerStatus.lowerBody.isSprinting = false;

            animator.WalkAnimation();
        }

        void Crouch()
        {
            playerStatus.lowerBody.isCrouching = true;
            playerStatus.lowerBody.isSprinting = false;

            animator.CrouchAnimation();
        }

        void Sprint()
        {
            playerStatus.lowerBody.isCrouching = false;
            playerStatus.lowerBody.isSprinting = true;

            animator.SprintAnimation();
        }

        bool IsWalking()
        {
            return !playerStatus.lowerBody.isCrouching && !playerStatus.lowerBody.isSprinting;
        }

        bool IsWeaponOut()
        {
            return playerStatus.upperBody.isPrimaryWeaponOut || playerStatus.upperBody.isSecondaryWeaponOut || playerStatus.upperBody.isMeleeWeaponOut;
        }

        /* Equipment */

        void ResetWeaponParams()
        {
            playerStatus.upperBody.isPrimaryWeaponOut = false;
            playerStatus.upperBody.isSecondaryWeaponOut = false;
            playerStatus.upperBody.isMeleeWeaponOut = false;
        }

        void HolsterWeapon()
        {
            ResetWeaponParams();
            animator.HolsterAnimation();
        }

        void DrawCurrentWeapon()
        {
            ResetWeaponParams();

            if (inventory.CurrentWeapon == null)
                return;

            WeaponGroup weaponGroup = inventory.CurrentWeapon.Properties.group;

            if (weaponGroup == WeaponGroup.Primary)
                playerStatus.upperBody.isPrimaryWeaponOut = true;

            else if (weaponGroup == WeaponGroup.Secondary)
                playerStatus.upperBody.isSecondaryWeaponOut = true;

            else if (weaponGroup == WeaponGroup.Melee)
                playerStatus.upperBody.isMeleeWeaponOut = true;

            else
                Debug.LogWarning("Player Attempted to Draw Weapon with Unknown WeaponClass");


            animator.DrawAnimation(inventory.CurrentWeapon.Properties);
        }

        void ToggleAttack()
        {
            animator.AttackAnimation();
        }

        void Reload()
        {
            if (playerStatus.upperBody.isMeleeWeaponOut)
                return;

            else if (playerStatus.upperBody.isPrimaryWeaponOut)
            {
                animator.ReloadAnimation();

            } else if (playerStatus.upperBody.isSecondaryWeaponOut)
            {
                animator.ReloadAnimation();
            }
        }

        #endregion

        #region Inventory

        public void AddItem(Item item)
        {
            inventory.AddItem(item);
        }

        #endregion
    }

    [System.Serializable]
    public class PlayerStatus
    {
        public UpperBody upperBody;
        public LowerBody lowerBody;

        [System.Serializable]
        public class UpperBody
        {
            public bool isPrimaryWeaponOut = false;
            public bool isSecondaryWeaponOut = false;
            public bool isMeleeWeaponOut = false;
            public bool isAiming = false;
        }

        [System.Serializable]
        public class LowerBody
        {
            public bool isCrouching = false;
            public bool isSprinting = false;
            public bool isJogSet = true;
        }
    }


}

