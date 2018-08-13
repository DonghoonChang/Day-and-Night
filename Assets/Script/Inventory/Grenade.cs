using System.Collections.Generic;
using UnityEngine;
using MyGame.Interface.ITakeHit;
using GameManager = MyGame.GameManagement.GameManager;
using CameraManager = MyGame.GameManagement.CameraManager;
using RaycastLayers = MyGame.GameManagement.RaycastLayers;
using PlayerCamera = MyGame.Player.PlayerCamera;
using PlayerCharacter = MyGame.Player.PlayerCharacter;

namespace MyGame.Object.Weapon
{
    public class Grenade : Item, ITakeHit
    {
        static float ExplosionTime = 3f;

        [SerializeField]
        GrenadeCard _grenadeCard;

        [SerializeField]
        int _count;
        bool _exploding = false;

        #region Properties

        public override string Description
        {
            get
            {
                return _grenadeCard.description;
            }
        }

        public override string Name
        {
            get
            {
                return _grenadeCard.name;
            }
        }

        public GrenadeType Type
        {
            get
            {
                return _grenadeCard.type;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }

            set
            {
                _count = Mathf.Max(0, value);
            }
        }

        #endregion

        private void Explode()
        {
            _exploding = true;

            // VFX
            if (_grenadeCard.explosionVFX != null)
            {
                GameObject explosion = Instantiate(_grenadeCard.explosionVFX);
                explosion.transform.position = transform.position;
            }

            // Camera Shake
            PlayerCharacter player = GameManager.Instance.Player;
            PlayerCamera playerCamera = CameraManager.Instance.PlayerCamera;

            if (playerCamera != null && player != null)
            {
                if (Vector3.Distance(transform.position, player.transform.position) < _grenadeCard.explosionRadius)
                {
                    playerCamera.ShakeCameraExplosionMajor();
                }

                else
                {
                    playerCamera.ShakeCameraExplosionMedium();
                }
            }

            // Search and Damage characters or grenades
            HashSet<ITakeHit> hitSet = new HashSet<ITakeHit>();
            Dictionary<ITakeHit, List<Transform>> transformListDic = new Dictionary<ITakeHit, List<Transform>>();
            Dictionary<ITakeHit, List<Vector3>> normalListDic = new Dictionary<ITakeHit, List<Vector3>>();

            RaycastHit[] raycastHits = Physics.SphereCastAll(new Ray(transform.position, transform.up), _grenadeCard.explosionRadius, 0.1f, RaycastLayers.ExplosionLayer);

            foreach(RaycastHit hit in raycastHits)
            {
                // Exclude itself
                if (hit.transform == transform)
                    continue;

                // Check if there's a wall (or any environmetal object blocking the blast)
                if (Physics.Raycast(new Ray(transform.position, hit.transform.position), hit.distance * 1.01f, RaycastLayers.EnvironmentLayer))
                    continue;

                Rigidbody hitRB = hit.rigidbody;
                ITakeHit hitTarget = hit.transform.root.GetComponent<ITakeHit>();

                if (hitRB != null)
                {
                    Vector3 relVec = (hit.transform.position - transform.position);
                    float forceMagnitude = Mathf.Clamp(_grenadeCard.explosionRadius / Vector3.Magnitude(relVec), 0.01f, 5f) * _grenadeCard.explosionForce;

                    hitRB.AddForceAtPosition(relVec.normalized * forceMagnitude, hit.point, ForceMode.Impulse);
                }

                if (hitTarget != null)
                {
                    if (hitSet.Contains(hitTarget))
                    {
                        transformListDic[hitTarget].Add(hit.transform);
                        normalListDic[hitTarget].Add(hit.normal);
                    }

                    else
                    {
                        hitSet.Add(hitTarget);
                        transformListDic[hitTarget] = new List<Transform> { hit.transform };
                        normalListDic[hitTarget] = new List<Vector3> { hit.normal };
                    }
                }
            }

            foreach (ITakeHit hitTarget in hitSet)
            {
                Transform[] transformArray = transformListDic[hitTarget].ToArray();
                Vector3[] normalArray = normalListDic[hitTarget].ToArray();

                if (transformArray.Length != normalArray.Length)
                    continue;

                else
                    hitTarget.OnHit(transformArray, normalArray, _grenadeCard.damage, _grenadeCard.concussion, true);
            }

            Destroy(gameObject);
        }

        public void SetExplosionMode()
        {
            _isInventoryModeOn = false;
            _rigidbody.isKinematic = false;

            ToggleItemShine(false);
            ToggleInteractionTrigger(false);
        }

        public void SetExplosion()
        {
            Invoke("Explode", ExplosionTime);
        }

        public void OnHit(Transform[] transformList, Vector3[] normalList, float damage, float concussion, bool applyDamageOnce)
        {
            if (!_exploding)
                Explode();
        }
    }
}
