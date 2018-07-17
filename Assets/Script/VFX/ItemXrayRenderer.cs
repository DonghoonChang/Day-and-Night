using UnityEngine;

namespace MyGame.VFX
{
    public class ItemXrayRenderer : MonoBehaviour
    {
        VFXManager _vfxManager;

        public void ShowXray()
        {
            gameObject.SetActive(true);
        }

        public void HideXray()
        {
            gameObject.SetActive(false);
        }
    }
}
