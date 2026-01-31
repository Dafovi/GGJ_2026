using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuMusicController : MonoBehaviour
{
    [SerializeField]
    private AudioClip _menuMusic;

    [SerializeField]
    private AudioClip _loop;

    private void Awake()
    {
        AudioManager.Instance.OnMusicEnded += PlayLoop;
    }

    private void OnEnable()
    {
        AudioManager.Instance.PlayMusic(_menuMusic, false, true);
    }

    private void PlayLoop()
    {
        AudioManager.Instance.PlayMusicImmediate(_loop);
    }

    private void OnDisable()
    {
        AudioManager.Instance.OnMusicEnded -= PlayLoop;
    }
}