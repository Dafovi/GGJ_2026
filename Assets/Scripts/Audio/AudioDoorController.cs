using UnityEngine;

public sealed class AudioDoorController : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip _openClip;

    [SerializeField]
    private AudioClip _closeClip;

    [SerializeField]
    private Collider _physicalCollider;

    [SerializeField]
    private AudioPortalLink _portal;

    [SerializeField]
    private bool _startsOpen = false;

    private bool _isOpen;

    private void Awake()
    {
        _isOpen = _startsOpen;
        ApplyState(initial: true);
    }

    public void Toggle()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (_isOpen) return;

        _isOpen = true;

        Play(_openClip);
        ApplyState();
    }

    public void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;

        Play(_closeClip);
        ApplyState();
    }

    private void ApplyState(bool initial = false)
    {
        if (_physicalCollider != null)
            _physicalCollider.enabled = !_isOpen;

        if (_portal != null)
        {
            _portal.SetEnabled(true);
            _portal.SetMaterial(_isOpen
                ? OcclusionMaterial.DoorOpen
                : OcclusionMaterial.DoorClosed);
        }

        if (!initial)
            AnnounceState();
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip);
    }

    private void AnnounceState()
    {
        if (_portal == null) return;

        if (_isOpen)
            Debug.Log("Puerta abierta");
        else
            Debug.Log("Puerta cerrada");
    }

    public bool IsOpen => _isOpen;
}