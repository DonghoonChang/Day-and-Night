using UnityEngine;
using RaycastLayers = Game.GameManagement.RaycastLayers;
using ItemXrayRenderer = Game.VFX.ItemXrayRenderer;
using PlayerCharacter = Game.Player.PlayerCharacter;

namespace Game.Object
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Item : InteractableObject
    {
        public Vector3 scaleInGUI;

        protected Rigidbody _rigidbody;
        protected ItemXrayRenderer[] _xrayRenderers;

        protected bool _isInventoryModeOn = false;

        #region Properties

        public abstract string Description
        {
            get;
        }

        public override bool IsInteractable
        {
            get
            {
                return base.IsInteractable && !_isInventoryModeOn;
            }
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = false;

            _xrayRenderers = GetComponentsInChildren<ItemXrayRenderer>();
            foreach (ItemXrayRenderer rend in _xrayRenderers)
                rend.StopShine();

            InteractionEvent.AddListener(AddItemInteraction);
        }

        protected override void Start()
        {
            base.Start();
            ToggleItemShine(false);
        }

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (_isInventoryModeOn)
                return;
        }

        public override void OnTriggerStay(Collider other)
        {
            base.OnTriggerStay(other);

            if (_isInventoryModeOn)
                return;

            if (_isInteractable)
                ToggleItemShine(true);
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            if (_isInventoryModeOn)
                return;

            if (!_isInteractable)
                ToggleItemShine(false);
        }

        #region Main Functions

        private void AddItemInteraction()
        {
            if (_player.GetComponent<PlayerCharacter>().AddItem(this))
            {
                Debug.Log("here");
                ToggleInventoryMode(true);
            }
        }

        public void ToggleItemShine(bool on)
        {
            if (on)
            {
                foreach (ItemXrayRenderer rend in _xrayRenderers)
                    rend.StartShine();
            }

            else
            {
                foreach (ItemXrayRenderer rend in _xrayRenderers)
                    rend.StopShine();
            }
        }

        public void ToggleInventoryMode(bool on)
        {
            if (on)
            {
                gameObject.layer = RaycastLayers.LayerToInt(RaycastLayers.InventoryItemLayer); // Inventory Item Layer

                _isInventoryModeOn = true;
                _rigidbody.isKinematic = true;

                ToggleItemShine(false);
                ToggleInteractionTrigger(false);
            }

            else
            {
                gameObject.layer = RaycastLayers.LayerToInt(RaycastLayers.ItemLayer); // Item Layer

                _isInventoryModeOn = false;
                _rigidbody.isKinematic = false;

                ToggleInteractionTrigger(true);
            }
        }

        #endregion
    }

    public class ObjectInteractionResult
    {
        public bool isSuccessful = false;
        public string message = "Default Message";

        public ObjectInteractionResult(bool isSuccessful, string message)
        {
            this.isSuccessful = isSuccessful;
            this.message = message;
        }
    }
}

