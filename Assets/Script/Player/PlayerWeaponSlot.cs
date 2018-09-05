using UnityEngine;
using UnityEngine.Events;
using Game.Object;
using GameManager = Game.GameManagement.GameManager;

namespace Game.Player
{
    public class PlayerWeaponSlot : MonoBehaviour
    {
        public class WeaponAnimationEvent : UnityEvent<WeaponGroup, WeaponType> { }

        [HideInInspector]
        public UnityEvent OnWeaponAttack = new UnityEvent();

        [HideInInspector]
        public WeaponAnimationEvent OnWeaponAnimationChanged = new WeaponAnimationEvent();

        [SerializeField]
        PlayerWeapon _weapon;
        PlayerStatus _playerStatus;

        float _currentSpread = 0;
        float spreadDecrement = 0.75f;

        #region Properties

        public PlayerWeapon Weapon
        {
            get
            {
                return _weapon;
            }

            set
            { 
                if (value != null)
                {
                    // From null To non-null
                    if (_weapon == null)
                    {
                        _weapon = value;

                        if (OnWeaponAnimationChanged != null)
                            OnWeaponAnimationChanged.Invoke(value.Properties.weaponGroup, value.Properties.weaponType);
                    }

                    // Fron non-null To non-null
                    else
                    {
                        WeaponProperties prevProperties = _weapon.Properties;
                        WeaponGroup prevGroup = prevProperties.weaponGroup;
                        WeaponType prevtype = prevProperties.weaponType;

                        WeaponProperties newProperties = value.Properties;
                        WeaponGroup newGroup = newProperties.weaponGroup;
                        WeaponType newType = newProperties.weaponType;

                        _weapon = value;

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
                    if (_weapon != null)
                    {
                        _weapon = value;

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

        public WeaponStats Stats
        {
            get
            {
                return _weapon.Stats;
            }
        }

        public WeaponProperties Properties
        {
            get
            {
                return _weapon.Properties;
            }
        }

        public int CurrentAmmo
        {
            get
            {
                if (_weapon == null)
                    return -1;

                else
                {
                    if (_weapon is RangedWeapon)
                        return ((RangedWeapon)_weapon).CurrentAmmo;

                    else
                        return -1;
                }
            }
        }

        public bool IsAutomatic
        {
            get
            {
                if (_weapon == null)
                    return false;

                else
                {
                    if (_weapon is RangedWeapon)
                        return ((RangedWeapon)_weapon).IsAutomatic;

                    else
                        return false;
                }
            }
        }

        public bool IsAttackLocked
        {
            get
            {
                if (_weapon == null)
                    return true;

                else return _weapon.AttackLocked;
            }
        }

        public float FireDelay
        {
            get
            {
                return 60f / Stats.rateOfFire;
            }
        }

        public float CurrentSpread
        {
            get
            {
                if (_weapon != null)
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

        #endregion

        #region Awake and Start

        private void Start()
        {
            _playerStatus = GameManager.Instance.Player.PlayerStatus;
        }

        void Update()
        {
            if (_weapon != null)
            {
                if (_weapon.Properties.weaponGroup == WeaponGroup.Main
                    || _weapon.Properties.weaponGroup == WeaponGroup.Secondary)
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

        /// <summary>
        /// Attempts use the current weapon to attack/fire
        /// </summary>
        /// <returns>bool: true, if the attempted attack was successful(bullets fired for ranged weapons)</returns>
        public bool Attack()
        {
            if (_weapon.Properties.weaponGroup == WeaponGroup.Main 
                || _weapon.Properties.weaponGroup == WeaponGroup.Secondary)
            {
                RangedWeapon weapon = _weapon as RangedWeapon;

                if (weapon.CurrentAmmo == 0)
                {
                    weapon.Attack(CurrentSpread, true);
                    return false;
                }

                else
                {
                    weapon.Attack(CurrentSpread, false);
                    _currentSpread += weapon.SpreadStep;

                    if (OnWeaponAttack != null)
                        OnWeaponAttack.Invoke();

                    return true;
                }
            }

            else
            {
                MeleeWeapon weapon = _weapon as MeleeWeapon;
                weapon.Attack();
                return true;
            }
        }

        public void ShowWeapon()
        {
            if (_weapon == null)
                return;

            _weapon.gameObject.SetActive(true);
        }

        public void HideWeapon()
        {
            if (_weapon == null)
                return;

            _weapon.gameObject.SetActive(false);
        }

        public void ReleaseAttackLock()
        {
            if (_weapon == null)
                return;

            _weapon.ReleaseAttackLock();
        }

        #endregion

    }
}
