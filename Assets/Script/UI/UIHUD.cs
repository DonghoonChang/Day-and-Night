using UnityEngine;

namespace MyGame.UI
{
    public class UIHUD : MonoBehaviour
    {
        public UIHUDAmmo ammoHUD;
        public UIHUDCrosshair crosshairHUD;
        public UIHUDHealthBar healthBarHUD;

        public void SetCurrentAmmo(int count)
        {
            ammoHUD.SetCurrentAmmo(count);
        }

        public void SetTotalAmmo(int count)
        {
            ammoHUD.SetTotalAmmo(count);
        }

        public void SetCrosshairSpread(float spread)
        {
            crosshairHUD.SetCurrentSpread(spread);
        }

        public void SetHealthBarAmount(int hp)
        {
            healthBarHUD.SetTargetHP(hp);
        }



    }
}

