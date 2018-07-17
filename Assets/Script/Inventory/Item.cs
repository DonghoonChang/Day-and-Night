using UnityEngine;
using ItemXrayRenderer = MyGame.VFX.ItemXrayRenderer;
using UIFloatingCanvas = MyGame.UI.UIFloatingCanvas;

namespace MyGame.Inventory
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public abstract class Item : InteractableObject
    {
        static float InteractionTriggerRadius = 2f;

        Rigidbody _rigidbody;
        SphereCollider _interactionTrigger;
        ItemXrayRenderer[] _xrayRenderers;
        UIFloatingCanvas _floatingCanvas;

        #region Properties

        public abstract Sprite Icon
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();

            _interactionTrigger = GetComponent<SphereCollider>();

            if (_interactionTrigger == null)
            {
                _interactionTrigger = gameObject.AddComponent<SphereCollider>();
            }

            _interactionTrigger.isTrigger = true;
            _interactionTrigger.radius = InteractionTriggerRadius / transform.root.localScale.x;

            _floatingCanvas = GetComponentInChildren<UIFloatingCanvas>();
            _xrayRenderers = GetComponentsInChildren<ItemXrayRenderer>();
            foreach (ItemXrayRenderer rend in _xrayRenderers)
                rend.HideXray();
        }

        protected override void Start()
        {
            base.Start();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.tag == "Player")
            {
                _isInteractable = true;
                ToggleEffects(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.transform.root.tag == "Player")
            {
                _isInteractable = true;
                ToggleEffects(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.tag == "Player")
            {
                _isInteractable = false;
                ToggleEffects(false);
            }
        }

        public void AddItemInteraction()
        {
            if (_player.AddItem(this))
                ToggleEffects(false);
        }

        #region Helpers

        public void ToggleEffects(bool on)
        {
            if (on)
            {
                _floatingCanvas.ToggleUI(true);
                foreach (ItemXrayRenderer rend in _xrayRenderers)
                    rend.ShowXray();
            }

            else
            {
                _floatingCanvas.ToggleUI(false);
                foreach (ItemXrayRenderer rend in _xrayRenderers)
                    rend.HideXray();
            }
        }

        #endregion
    }

    public class ObjectInteractionResult
    {
        public bool isSuccessful;
        public string message;

        ObjectInteractionResult()
        {
            isSuccessful = false;
            message = "Default Message";
        }

        public ObjectInteractionResult(bool isSuccessful, string message)
        {
            this.isSuccessful = isSuccessful;
            this.message = message;
        }
    }
}

