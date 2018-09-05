using UnityEngine;

namespace Game.Interface.ITakeHit
{
    public interface ITakeHit
    {
        void OnHit(Transform[] transformList, Vector3[] normalList, float damage, float concussion, bool applyDamageOnce);
    }

}

