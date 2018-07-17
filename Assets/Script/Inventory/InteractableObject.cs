using UnityEngine;
using UnityEngine.Events;
using MyGame.GameManagement;
using MyGame.Player;

public abstract class InteractableObject : MonoBehaviour {

    public UnityEvent InteractionEvent = new UnityEvent();
    protected bool _isInteractable;

    protected PlayerCharacter _player;

    public bool IsInteractable
    {
        get
        {
            return _isInteractable;
        }
    }

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        _player = GameManager.Instance.Player;
    }

    public void Interact()
    {
        if (_isInteractable)
        {
            if (InteractionEvent != null)
                InteractionEvent.Invoke();
        }
    }


}
