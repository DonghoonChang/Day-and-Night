using UnityEngine;
using UIHUD = Game.UI.UIHUD;
using UIManager = Game.GameManagement.UIManager;

namespace Game.Object
{
    [RequireComponent(typeof(Animator))]
    public class OpenableObject : InteractableObject
    {

        #region Settings

        protected static int OpenId = Animator.StringToHash("Open");

        #endregion

        UIHUD _uihud;
        Animator _animator;

        public string objectName;
        public bool open = false;

        public override string Name
        {
            get
            {
                return objectName;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InteractionEvent.AddListener(Open);

            _animator = GetComponent<Animator>();
            _animator.SetBool(OpenId, false);
        }

        protected override void Start()
        {
            base.Start();

            _uihud = UIManager.Instance.HUDPanel;
        }

        #region Main Methods

        public void Open()
        {

            if (_isInteractable)
            {
                if (open)
                {
                    open = false;
                    _animator.SetBool(OpenId, false);
                }

                else
                {
                    open = true;
                    _animator.SetBool(OpenId, true);
                }

            }
        }

        #endregion

        #region Trigger Events

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
        }

        public override void OnTriggerStay(Collider other)
        {
            base.OnTriggerStay(other);
        }

        #endregion
    }
}
