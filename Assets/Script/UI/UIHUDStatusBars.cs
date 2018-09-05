using UnityEngine;

namespace Game.UI
{
    public class UIHUDStatusBars : MonoBehaviour
    {

        [SerializeField] UIHUDStatusBar _healthBar;
        [SerializeField] UIHUDStatusBar _batteryBar;
        [SerializeField] UIHUDStatusBar _staminaBar;

        public void SetHealthBarValues(float fill, int value)
        {
            _healthBar.SetTargetFillandValue(fill, value);
        }

        public void SetBatteryBarValues(float fill, int value)
        {
            _batteryBar.SetTargetFillandValue(fill, value);
        }

        public void SetStaminaBarValues(float fill, int value)
        {
            _staminaBar.SetTargetFillandValue(fill, value);
        }
    }
}
