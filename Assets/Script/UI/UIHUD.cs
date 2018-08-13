using UnityEngine;

namespace MyGame.UI
{
    public class UIHUD : MonoBehaviour
    {
        [SerializeField] UIHUDAmmo _ammo;
        [SerializeField] UIHUDMessage _message;
        [SerializeField] UIHUDStatusBars _statusBars;
        [SerializeField] UIHUDCrosshair _crosshair;
        [SerializeField] UIHUDSniperScope _sniperScope;

        // Ammo HUD
        public void ShowAmmo()
        {
            _ammo.gameObject.SetActive(true);
        }

        public void HideAmmo()
        {
            _ammo.gameObject.SetActive(false);
        }

        public void SetCurrentAmmo(int count)
        {
            _ammo.SetCurrentAmmo(count);
        }

        public void SetTotalAmmo(int count)
        {
            _ammo.SetTotalAmmo(count);
        }

        public void SetCurrentGrenade(int count)
        {
            _ammo.SetCurrentGrenade(count);
        }

        // Crosshair HUD
        public void SetCrosshairSpread(float spread)
        {
            _crosshair.SetCurrentSpread(spread);
        }
        
        // Message HUD
        public void ShowResult(bool success, string msg)
        {
            _message.ShowResult(success, msg);
        }

        public void SetInteractionName(string name)
        {
            _message.ShowInteractableName(name);
        }

        public void SetPromptMessage(string msg, int speed)
        {
            _message.ShowPrompt(msg, speed);
        }

        // Status HUD
        public void SetHealthBarValues(float fill, int value)
        {
            _statusBars.SetHealthBarValues(fill, value);
        }

        // Status HUD
        public void SetBatteryBarValues(float fill, int value)
        {
            _statusBars.SetBatteryBarValues(fill, value);
        }

        // Status HUD
        public void SetStaminaBarValues(float fill, int value)
        {
            _statusBars.SetStaminaBarValues(fill, value);
        }

        // Scope HUD
        public void ShowSniperScope()
        {
            _sniperScope.ShowScope();
        }

        public void HideSniperScope()
        {
            _sniperScope.HideScope();
        }
    }
}

