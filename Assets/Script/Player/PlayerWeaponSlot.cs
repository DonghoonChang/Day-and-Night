using UnityEngine;
using MyGame.Inventory.Weapon;

namespace MyGame.Player
{
    public class PlayerWeaponSlot : MonoBehaviour
    {

        public Weapon currentWeapon;


        #region Properties

        public Weapon Weapon
        {
            get
            {
                return currentWeapon;
            }

            set
            { 
                if (value != null)
                    currentWeapon = value;
            }
        }

        public WeaponProperties Properties
        {
            get
            {
                return currentWeapon.Properties;
            }
        }

        public WeaponStats Stats
        {
            get
            {
                return currentWeapon.Stats;
            }
        }

        public float FireDelay
        {
            get
            {
                return 60f / Stats.fireRate;
            }
        }


        #endregion


        #region Awake and Start

        void Awake()
        {
        }

        #endregion

        #region Main Functions


        public void ToggleAttack()
        {
            currentWeapon.Attack();
        }

        public void ShowWeapon()
        {
            if (currentWeapon != null)
                currentWeapon.gameObject.SetActive(true);

            else
                Debug.Log("Tring to Show Weapon Not Equipped");
        }

        public void HideWeapon()
        {
            if (currentWeapon != null)
                currentWeapon.gameObject.SetActive(false);

            else
                Debug.Log("Tring to Show Weapon Not Equipped");
        }

        public void ReleaseFireLock()
        {
            if (currentWeapon != null)
                currentWeapon.ReleaseFireLock();

            else
                Debug.Log("Tring to Show Weapon Not Equipped");
        }



        #endregion

    }
}
