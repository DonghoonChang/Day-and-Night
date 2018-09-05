using UnityEngine;
using UnityEngine.Events;
using GameManager = Game.GameManagement.GameManager;

[RequireComponent(typeof(SphereCollider))]
public abstract class InteractableObject : MonoBehaviour {

    #region Setting

    static float InteractionTriggerRadius = 2f;

    #endregion

    public UnityEvent InteractionEvent = new UnityEvent();

    protected Transform _player;
    protected SphereCollider _interactionTrigger;

    protected bool _isInteractable;

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
        _player = GameManager.Instance.Player.transform;
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

    protected void ToggleInteractionTrigger(bool on)
    {
        if (on)
            _interactionTrigger.enabled = true;

        else
            _interactionTrigger.enabled = false;
    }

    public virtual void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.transform != _player)
            return;

        _isInteractable = true;
    }

    public virtual void OnTriggerStay(Collider other)
    {
        if (other.isTrigger || other.transform != _player)
            return;

        _isInteractable = true;
    }

    public virtual void OnTriggerExit(Collider other)
    {
        if (other.isTrigger || other.transform != _player)
            return;

        _isInteractable = false;
    }
}
