using UnityEngine;

namespace Game.Player
{
    public class PlayerFlashlight : MonoBehaviour
    {

        #region Settings

        const float MaximumIntensity = 5f;
        const float ReserveIntensity = 2.5f;
        const float NoIntensity = 0f;

        #endregion

        [SerializeField]
        Light _flashlight;

        int _currentIntensity = 0;
        float _batteryLife = 1000f;
        float _maxBatteryLife = 1000f;

        #region Properties

        public float BatteryLife
        {
            get
            {
                return _batteryLife;
            }
        }

        public float MaxBatteryLife
        {
            get
            {
                return _maxBatteryLife;
            }
        }

        #endregion
        public void Update()
        {
            if (_batteryLife > 0)
            {
                _batteryLife = Mathf.Max(_batteryLife -= GameTime.deltaTime * GetCurrentBatterUsage(), 0);

                if (_batteryLife <= 0)
                    TurnOffFlashlight();
            }
        }

        public void SwitchIntensity()
        {
            _currentIntensity++;
            _currentIntensity %= 3;

            switch (_currentIntensity)
            {
                case 0:
                    _flashlight.intensity = NoIntensity;
                    return;

                case 1:
                    _flashlight.intensity = ReserveIntensity;
                    return;

                case 2:
                    _flashlight.intensity = MaximumIntensity;
                    return;
            }
        }

        public void TurnOffFlashlight()
        {
            _currentIntensity = 0;
            _flashlight.intensity = 0f;
        }

        public void ChargeBattery(float amount)
        {
            _batteryLife = Mathf.Min(_batteryLife + amount, _maxBatteryLife);
        }

        private float GetCurrentBatterUsage()
        {
            switch (_currentIntensity)
            {
                case 0:
                    return NoIntensity;

                case 1:
                    return ReserveIntensity;

                case 2:
                    return MaximumIntensity;

                default:
                    return NoIntensity;
            }
        }

    }

}
