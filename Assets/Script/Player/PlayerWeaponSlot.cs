using UnityEngine;
using UnityEngine.Events;
using MyGame.Inventory.Weapon;
using GameManager = MyGame.GameManagement.GameManager;

namespace MyGame.Player
{
    public class PlayerWeaponSlot : MonoBehaviour
    {
        // Events
        public class WeaponAnimationEvent : UnityEvent<WeaponGroup, WeaponType> { }
        public WeaponAnimationEvent OnWeaponAnimationChanged = new WeaponAnimationEvent();
        public UnityEvent OnWeaponAttack = new UnityEvent();

        static float spreadDecrement = 0.5f;

        Weapon _currentWeapon;

        PlayerStatus _playerStatus;
        float _currentSpread = 0;

        #region Properties

        public Weapon Weapon
        {
            get
            {
                return _currentWeapon;
            }

            set
            { 
                if (value != null)
                {
                    // From null To non-null
                    if (_currentWeapon == null)
                    {
                        _currentWeapon = value;

                        if (OnWeaponAnimationChanged != null)
                            OnWeaponAnimationChanged.Invoke(value.Properties.weaponGroup, value.Properties.weaponType);
                    }

                    // Fron non-null To non-null
                    else
                    {
                        WeaponProperties prevProperties = _currentWeapon.Properties;
                        WeaponGroup prevGroup = prevProperties.weaponGroup;
                        WeaponType prevtype = prevProperties.weaponType;

                        WeaponProperties newProperties = value.Properties;
                        WeaponGroup newGroup = newProperties.weaponGroup;
                        WeaponType newType = newProperties.weaponType;

                        _currentWeapon = value;

                        if (prevGroup == newGroup && prevtype == newType)
                            return;

                        else
                        {
                            if (OnWeaponAnimationChanged != null)
                                OnWeaponAnimationChanged.Invoke(newGroup, newType);
                        }
                    }
                }

                else
                {
                    // From non-null To null
                    if (_currentWeapon != null)
                    {
                        _currentWeapon = value;

                        if (OnWeaponAnimationChanged != null)
                            OnWeaponAnimationChanged.Invoke(0, 0);
                    }

                    // From null To null
                    else
                    {
                        return;
                    }
                }

                _currentSpread = 0f;
            }
        }

        public WeaponProperties Properties
        {
            get
            {
                if (Weapon != null)
                    return _currentWeapon.Properties;

                else
                    return null;
            }
        }

        public WeaponStats Stats
        {
            get
            {
                if (Weapon != null)
                    return _currentWeapon.Stats;

                else
                    return null;
            }
        }

        public WeaponGroup Group
        {
            get
            {
                return Properties.weaponGroup;
            }
        }

        public bool isWeaponActive
        {
            get
            {
                return Weapon.gameObject.activeSelf;
            }
        }

        public float CurrentSpread
        {
            get
            {
                if (_currentWeapon != null)
                {
                    float value = _currentSpread;
                    value *= _playerStatus.isAiming ? .5f : 1f;
                    value *= 1f + (Mathf.Sqrt(Mathf.Pow(_playerStatus.horizontalAxis, 2f) + Mathf.Pow(_playerStatus.verticalAxis, 2f)));
                    value *= _playerStatus.isCrouching ? .25f : 1f;
                    return value;
                }

                else
                {
                    return 0f;
                }
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

        private void Start()
        {
            _playerStatus = GameManager.Instance.Player.PlayerStatus;
        }

        void Update()
        {
            if (_currentWeapon != null)
            {
                if (_currentWeapon.Properties.weaponGroup == WeaponGroup.Main
                    || _currentWeapon.Properties.weaponGroup == WeaponGroup.Secondary)
                {

                    RangedWeapon rangedWeapon = Weapon as RangedWeapon;

                    if (_currentSpread < rangedWeapon.MinSpread)
                    {
                        _currentSpread = rangedWeapon.MinSpread;
                    }

                    else
                    {
                        _currentSpread = Mathf.Lerp(_currentSpread, rangedWeapon.MinSpread, Mathf.Abs(_currentSpread - rangedWeapon.MinSpread) * spreadDecrement * GameTime.deltaTime);

                    }
                }

                else
                    _currentSpread = 0f;
            }

            else
            {
                _currentSpread = 0f;
            }
        }

        #endregion

        #region Main Functions

        public void Attack()
        {
            if (_currentWeapon.Properties.weaponGroup == WeaponGroup.Main || _currentWeapon.Properties.weaponGroup == WeaponGroup.Secondary)
            {
                RangedWeapon weapon = _currentWeapon as RangedWeapon;

                _currentSpread += weapon.SpreadStep;
                weapon.Attack(CurrentSpread, weapon.CurrentAmmo == 0);
            }

            else
            {
                MeleeWeapon weapon = _currentWeapon as MeleeWeapon;
                weapon.Attack();
            }

            if (OnWeaponAttack != null)
                OnWeaponAttack.Invoke();
        }

        public void ShowWeapon()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.gameObject.SetActive(true);
            }

            else
            {
                Debug.Log("Trying to Show Weapon Not Equipped");
            }
        }

        public void HideWeapon()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.gameObject.SetActive(false);
            }

            else
            {
                Debug.Log("Trying to Hide Weapon Not Equipped");
            }
        }

        public void ReleaseAttackLock()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.ReleaseAttackLock();
            }

            else
            {
                Debug.Log("Trying to Release Fire Lock on Null Weapon");
            }
        }

        #endregion

    }
}
