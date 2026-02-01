using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectHitable : MonoBehaviour
{
    [SerializeField]
    private AudioClip _hitSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _hitSound;
    }
    public void Hit()
    {
        if (_hitSound != null && !_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }
}