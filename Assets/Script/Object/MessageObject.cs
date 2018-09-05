using UnityEngine;
using UIHUD = Game.UI.UIHUD;
using UIManager = Game.GameManagement.UIManager;

namespace Game.Object
{
    public class MessageObject : InteractableObject
    {
        UIHUD _uihud;

        public string objectName;
        public string message;
        public int speed;
        public float clearTime;

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

            InteractionEvent.AddListener(ShowMessage);
        }

        protected override void Start()
        {
            base.Start();

            _uihud = UIManager.Instance.HUDPanel;
        }

        public void ShowMessage()
        {
            _uihud.ShowPrompt(message, speed, clearTime);
        }

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


    }
}
