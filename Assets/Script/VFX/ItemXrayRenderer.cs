using UnityEngine;
using VFXManager = MyGame.GameManagement.VFXManager;

namespace MyGame.VFX
{
    public class ItemXrayRenderer : MonoBehaviour
    {
        VFXManager _vfxManager;

        public void StartShine()
        {
            gameObject.SetActive(true);
        }

        public void StopShine()
        {
            gameObject.SetActive(false);
        }
    }
}
