using UnityEngine;
using MyGame.Inventory;

namespace MyGame.Player
{
    public class PlayerRaycastHitPoint : MonoBehaviour
    {

        static float IntertactionRadius = .5f;

        SphereCollider _interactionTrigger;
        [SerializeField]
        InteractableObject _object;

        public InteractableObject InteractableObject
        {
            get
            {
                return _object;
            }
        }

        private void Awake()
        {
            _interactionTrigger = GetComponent<SphereCollider>();

            if (_interactionTrigger != null)
            {
                _interactionTrigger.isTrigger = true;
                _interactionTrigger.radius = IntertactionRadius;
            }

            else
                Debug.Log("Player Interaction Trigger Not Set");
        }

        private void OnTriggerEnter(Collider other)
        {
            InteractableObject interactable = other.transform.root.transform.GetComponent<InteractableObject>();

            if (interactable == null)
                return;

            else
            {
                _object = interactable;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            InteractableObject interactable = other.transform.root.GetComponent<InteractableObject>();

            if (interactable == null)
                return;

            else
            {
                if (interactable == _object)
                    _object = null;
            }
        }
    }
}
