using System;
using System.Collections;
using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioSource _tts;

    [SerializeField]
    private AudioSource _music;

    [SerializeField]
    private AudioSource _sfx;

    [SerializeField]
    private float _ttsFadeOut = 0.8f;

    [SerializeField]
    private float _musicFade = 1.5f;

    [SerializeField]
    private float _sfxFadeOut = 0.5f;

    public event Action OnTTSEnded;
    public event Action OnMusicEnded;

    private ChannelState _ttsState = new ChannelState();
    private ChannelState _musicState = new ChannelState();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        _ttsState.TryFireEndEvent(OnTTSEnded);
        _musicState.TryFireEndEvent(OnMusicEnded);
    }

    public void PlayTTS(AudioClip clip, float delay = 0f)
    {
        if (clip == null) return;
        if (GameManager.Instance != null && !GameManager.Instance.UseTTS) return;

        _ttsState.CancelAll(this);

        double start = AudioSettings.dspTime + Math.Max(0, delay);
        double end = start + clip.length;

        _tts.loop = false;
        _tts.clip = clip;
        _tts.PlayScheduled(start);

        _ttsState.ArmEndEvent(end, _tts);
    }

    public void StopTTS()
    {
        _ttsState.CancelEndEvent();
        _ttsState.StartFade(this, FadeOutAndStop(_tts, _ttsFadeOut));
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _sfx.PlayOneShot(clip);
    }

    public void StopSFX()
    {
        StartCoroutine(FadeOutAndStop(_sfx, _sfxFadeOut));
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool invokeEnded = false)
    {
        if (clip == null) return;

        _musicState.CancelAll(this);
        _musicState.StartFade(this, ReplaceWithFade(_music, clip, _musicFade, loop));

        if (invokeEnded && !loop)
        {
            double end = AudioSettings.dspTime + _musicFade + clip.length;
            _musicState.ArmEndEvent(end, _music);
        }
    }

    public void PlayMusicImmediate(AudioClip clip, bool loop = true, bool invokeEnded = false)
    {
        if (clip == null) return;

        _musicState.CancelAll(this);

        _music.loop = loop;
        _music.clip = clip;
        _music.volume = 1f;
        _music.Play();

        if (invokeEnded && !loop)
        {
            double end = AudioSettings.dspTime + clip.length;
            _musicState.ArmEndEvent(end, _music);
        }
    }

    public void StopMusic()
    {
        _musicState.CancelEndEvent();
        _musicState.StartFade(this, FadeOutAndStop(_music, _musicFade));
    }

    private static IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    private static IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
    {
        source.volume = 0f;
        source.Play();

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private static IEnumerator ReplaceWithFade(AudioSource source, AudioClip clip, float duration, bool loop)
    {
        float targetVolume = source.volume;

        if (source.isPlaying)
            yield return FadeOutAndStop(source, duration);

        source.loop = loop;
        source.clip = clip;

        yield return FadeIn(source, duration, targetVolume);
    }

    private sealed class ChannelState
    {
        private int _opId;
        private Coroutine _fadeCoroutine;

        private bool _armed;
        private double _endDspTime;
        private AudioSource _source;
        private int _endOpId;

        public void CancelAll(MonoBehaviour owner)
        {
            _opId++;
            CancelFade(owner);
            CancelEndEvent();
        }

        public void StartFade(MonoBehaviour owner, IEnumerator routine)
        {
            _opId++;
            CancelFade(owner);

            int id = _opId;
            _fadeCoroutine = owner.StartCoroutine(RunGuarded(routine, () => id != _opId));
        }

        public void CancelFade(MonoBehaviour owner)
        {
            if (_fadeCoroutine != null)
            {
                owner.StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        public void ArmEndEvent(double endTime, AudioSource source)
        {
            _armed = true;
            _endDspTime = endTime;
            _source = source;
            _endOpId = _opId;
        }

        public void CancelEndEvent()
        {
            _armed = false;
            _source = null;
        }

        public void TryFireEndEvent(Action action)
        {
            if (!_armed) return;
            if (_endOpId != _opId) { CancelEndEvent(); return; }
            if (AudioSettings.dspTime < _endDspTime) return;

            CancelEndEvent();
            action?.Invoke();
        }

        private static IEnumerator RunGuarded(IEnumerator routine, Func<bool> stop)
        {
            while (!stop())
            {
                if (!routine.MoveNext())
                    yield break;

                yield return routine.Current;
            }
        }
    }
}