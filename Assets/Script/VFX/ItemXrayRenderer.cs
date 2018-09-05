using UnityEngine;
using VFXManager = Game.GameManagement.VFXManager;

namespace Game.VFX
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
