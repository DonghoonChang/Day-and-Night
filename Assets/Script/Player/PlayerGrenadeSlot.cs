using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using RaycastLayers = Game.GameManagement.RaycastLayers;
using Grenade = Game.Object.Weapon.Grenade;

namespace Game.Player
{
    public class PlayerGrenadeSlot : MonoBehaviour
    {
        #region Settings

        const float GrenadeThrowForce = 10f;
        const float GrenadeTorqueForce = 500f;

        #endregion

        Grenade _grenade;

        [HideInInspector]
        public UnityEvent OnGrenadeChanged;

        [HideInInspector]
        public UnityEvent OnGrenadeThrown;

        public Grenade Grenade
        {
            get
            {
                return _grenade;
            }

            set
            {
                if (_grenade == value)
                    return;

                _grenade = value;

                if (OnGrenadeChanged != null)
                    OnGrenadeChanged.Invoke();
            }
        }

        public void ShowGrenade()
        {
            if (_grenade != null)
                _grenade.gameObject.SetActive(true);
        }

        public void HideGrenade()
        {
            if (_grenade != null)
                _grenade.gameObject.SetActive(false);
        }

        public void ThrowGrenade(Transform raycastPoint)
        {
            StartCoroutine(ThrowGrenadeCo(raycastPoint));
        }

        IEnumerator ThrowGrenadeCo(Transform aimPoint)
        {
            yield return new WaitForEndOfFrame();

            if (_grenade != null)
            {
                _grenade.Count--;

                GameObject grenadeObject = Instantiate(_grenade.gameObject, transform.position, transform.rotation, null);
                Grenade grenade = grenadeObject.GetComponent<Grenade>();
                Rigidbody rb = grenadeObject.GetComponent<Rigidbody>();

                if (grenadeObject != null)
                {
                    grenadeObject.SetActive(true);
                    grenadeObject.layer = RaycastLayers.LayerToInt(RaycastLayers.GrenadeThrownLayer); // Grenade Thrown Layer
                }

                if (grenade != null)
                {
                    grenade.SetExplosionMode();
                    grenade.SetExplosion();
                }

                if (rb != null)
                {
                    grenadeObject.GetComponent<Rigidbody>().AddForce((aimPoint.position - transform.position).normalized * GrenadeThrowForce, ForceMode.Impulse);
                    grenadeObject.GetComponent<Rigidbody>().AddTorque(_grenade.transform.forward * GrenadeTorqueForce, ForceMode.Impulse);
                }

                if (OnGrenadeChanged != null)
                    OnGrenadeChanged.Invoke();
            }
        }
    }
}
