using UnityEngine;
using UnityEngine.Events;

public sealed class InteractableEvent : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onInteract;

    [SerializeField]
    private AudioClip _interactSfx;

    [SerializeField]
    private AudioSource _audioSource;

    public void Interact()
    {
        if (_interactSfx != null)
        {
            if (_audioSource != null)
                _audioSource.PlayOneShot(_interactSfx);
            else
                AudioManager.Instance?.PlaySFX(_interactSfx);
        }

        _onInteract?.Invoke();
    }
}