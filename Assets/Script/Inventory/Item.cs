using UnityEngine;
using CameraManager = MyGame.GameManagement.CameraManager;
using PlayerCamera = MyGame.Player.PlayerCamera;
using ItemXrayRenderer = MyGame.VFX.ItemXrayRenderer;

namespace MyGame.Object
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
        }

        protected override void Start()
        {
            base.Start();
            ToggleItemShine(false);
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger)
                return;

            if (_isInventoryModeOn)
                return;

            if (other.transform.root.tag == "Player")
            {
            }
        }

        public override void OnTriggerStay(Collider other)
        {
            if (other.isTrigger)
                return;

            if (_isInventoryModeOn)
                return;

            if (other.transform.root.tag == "Player")
            {
                _isInteractable = true;
                ToggleItemShine(true);
            }
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.isTrigger)
                return;

            if (_isInventoryModeOn)
                return;

            if (other.transform.root.tag == "Player")
            {
                _isInteractable = false;

                ToggleItemShine(false);
            }
        }

        #region Main Functions

        public void AddItemInteraction()
        {
            if (_player.AddItem(this))
            {
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
                gameObject.layer = 14; // Inventory Item Layer

                _isInventoryModeOn = true;
                _rigidbody.isKinematic = true;

                ToggleItemShine(false);
                ToggleInteractionTrigger(false);
            }

            else
            {
                gameObject.layer = 13; // Item Layer

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

