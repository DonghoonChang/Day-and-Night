using UnityEngine;
using UnityEngine.Events;
using GameManager = MyGame.GameManagement.GameManager;
using PlayerCharacter = MyGame.Player.PlayerCharacter;

[RequireComponent(typeof(SphereCollider))]
public abstract class InteractableObject : MonoBehaviour {

    public UnityEvent InteractionEvent = new UnityEvent();

    protected PlayerCharacter _player;

    protected bool _isInteractable;
    protected string _promptMessage;
    protected float InteractionTriggerRadius = 2f;
    protected SphereCollider _interactionTrigger;

    #region Properties

    public virtual bool IsInteractable
    {
        get
        {
            return _isInteractable;
        }
    }

    public abstract string Name
    {
        get;
    }

    public string PromptMessage
    {
        get
        {
            return _promptMessage;
        }
    }
    #endregion

    #region Awake To Update

    protected virtual void Awake()
    {
        _interactionTrigger = GetComponent<SphereCollider>();

        if (_interactionTrigger == null)
            _interactionTrigger = gameObject.AddComponent<SphereCollider>();

        _interactionTrigger.isTrigger = true;
        _interactionTrigger.radius = InteractionTriggerRadius / transform.root.localScale.x;
    }

    protected virtual void Start()
    {
        _player = GameManager.Instance.Player;
    }

    #endregion

    public void Interact()
    {
        if (_isInteractable)
        {
            if (InteractionEvent != null)
                InteractionEvent.Invoke();
        }
    }

    public void ToggleInteractionTrigger(bool on)
    {
        if (on)
            _interactionTrigger.enabled = true;

        else
            _interactionTrigger.enabled = false;
    }

    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);
}
