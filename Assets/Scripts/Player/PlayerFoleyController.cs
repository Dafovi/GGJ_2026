using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerFoleyController : MonoBehaviour
{
    [SerializeField]
    private AudioSource _breathingSource;

    [SerializeField]
    private AudioSource _dragSource;

    [SerializeField]
    private AudioSource _effortSource;

    [SerializeField]
    private AudioClip _breathingLoop;

    [SerializeField]
    private AudioClip _dragLoopA;

    [SerializeField]
    private AudioClip _dragLoopB;

    [SerializeField]
    private AudioClip[] _dragAccentClips;

    [SerializeField]
    private AudioClip[] _pantClips;

    [SerializeField]
    private AudioClip[] _interactEffortClips;

    [SerializeField]
    private float _moveThreshold = 0.05f;

    [SerializeField]
    private float _dragVolume = 0.38f;

    [SerializeField]
    private float _dragFadeSpeed = 10f;

    [SerializeField]
    private Vector2 _dragSwapIntervalRange = new Vector2(2.0f, 4.5f);

    [SerializeField]
    private Vector2 _dragAccentIntervalRange = new Vector2(3.0f, 6.5f);

    [SerializeField]
    private float _dragAccentVolume = 0.35f;

    [SerializeField]
    private Vector2 _dragPitchRange = new Vector2(0.95f, 1.05f);

    [SerializeField]
    private Vector2 _pantIntervalRange = new Vector2(2.0f, 4.2f);

    [SerializeField]
    private float _pantMinSpeed = 0.35f;

    [SerializeField]
    private float _pantVolume = 0.8f;

    [SerializeField]
    private float _interactEffortVolume = 0.85f;

    [SerializeField]
    private Vector2 _effortPitchRange = new Vector2(0.95f, 1.05f);

    private CharacterController _controller;

    private float _dragTarget;
    private bool _wasMoving;

    private float _nextDragSwapTime;
    private float _nextDragAccentTime;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        SetupBreathing();
        SetupDrag();
    }

    private void OnEnable()
    {
        SetupBreathing();
        SetupDrag();
    }

    private void Update()
    {
        float speed = GetPlanarSpeed();
        bool isMoving = speed > _moveThreshold;

        _dragTarget = isMoving ? _dragVolume : 0f;

        if (_dragSource != null)
        {
            float k = 1f - Mathf.Exp(-_dragFadeSpeed * Time.deltaTime);
            _dragSource.volume = Mathf.Lerp(_dragSource.volume, _dragTarget, k);
        }

        if (isMoving && !_wasMoving)
        {
            ScheduleDragSwap(0.2f);
            ScheduleDragAccent(0.6f);
        }

        if (isMoving)
        {
            if (Time.time >= _nextDragSwapTime)
                SwapDragLoop();

            if (_dragAccentClips != null && _dragAccentClips.Length > 0 && Time.time >= _nextDragAccentTime)
                PlayDragAccent();
        }

        _wasMoving = isMoving;
    }

    private float GetPlanarSpeed()
    {
        Vector3 v = _controller.velocity;
        v.y = 0f;
        return v.magnitude;
    }

    private void SetupBreathing()
    {
        if (_breathingSource == null || _breathingLoop == null) return;

        _breathingSource.loop = true;
        _breathingSource.playOnAwake = false;
        _breathingSource.clip = _breathingLoop;

        if (!_breathingSource.isPlaying)
            _breathingSource.Play();
    }

    private void SetupDrag()
    {
        if (_dragSource == null) return;

        _dragSource.loop = true;
        _dragSource.playOnAwake = false;

        AudioClip chosen = ChooseDragClip();
        if (chosen != null)
        {
            _dragSource.clip = chosen;
            _dragSource.pitch = Random.Range(_dragPitchRange.x, _dragPitchRange.y);

            if (!_dragSource.isPlaying)
                _dragSource.Play();
        }

        _dragSource.volume = 0f;

        ScheduleDragSwap(0.5f);
        ScheduleDragAccent(1.2f);
    }

    private AudioClip ChooseDragClip()
    {
        if (_dragLoopA == null && _dragLoopB == null) return null;
        if (_dragLoopA != null && _dragLoopB == null) return _dragLoopA;
        if (_dragLoopA == null && _dragLoopB != null) return _dragLoopB;

        return Random.value < 0.5f ? _dragLoopA : _dragLoopB;
    }

    private void SwapDragLoop()
    {
        if (_dragSource == null) return;

        AudioClip next = _dragSource.clip == _dragLoopA ? _dragLoopB : _dragLoopA;
        if (next == null) next = ChooseDragClip();
        if (next == null) return;

        _dragSource.clip = next;
        _dragSource.pitch = Random.Range(_dragPitchRange.x, _dragPitchRange.y);

        if (!_dragSource.isPlaying)
            _dragSource.Play();

        ScheduleDragSwap(0f);
    }

    private void PlayDragAccent()
    {
        if (_dragSource == null) return;
        if (_dragAccentClips == null || _dragAccentClips.Length == 0) return;

        AudioClip clip = _dragAccentClips[Random.Range(0, _dragAccentClips.Length)];
        if (clip == null) return;

        float pitch = Random.Range(_dragPitchRange.x, _dragPitchRange.y);
        AudioManager.Instance.PlaySFXOnSource(_dragSource, clip, _dragAccentVolume, pitch);

        ScheduleDragAccent(0f);
    }

    private void ScheduleDragSwap(float extraDelay)
    {
        _nextDragSwapTime = Time.time + Random.Range(_dragSwapIntervalRange.x, _dragSwapIntervalRange.y) + extraDelay;
    }

    private void ScheduleDragAccent(float extraDelay)
    {
        _nextDragAccentTime = Time.time + Random.Range(_dragAccentIntervalRange.x, _dragAccentIntervalRange.y) + extraDelay;
    }

    private Coroutine _pantCoroutine;

    private void Start()
    {
        if (_pantCoroutine == null)
            _pantCoroutine = StartCoroutine(PantRoutine());
    }

    private void OnDisablePant()
    {
        if (_pantCoroutine != null)
        {
            StopCoroutine(_pantCoroutine);
            _pantCoroutine = null;
        }
    }

    private IEnumerator PantRoutine()
    {
        while (true)
        {
            float wait = Random.Range(_pantIntervalRange.x, _pantIntervalRange.y);
            yield return new WaitForSeconds(wait);

            if (_effortSource == null) continue;
            if (_pantClips == null || _pantClips.Length == 0) continue;

            float speed = GetPlanarSpeed();
            if (speed < _pantMinSpeed) continue;

            float pitch = Random.Range(_effortPitchRange.x, _effortPitchRange.y);
            AudioManager.Instance.PlaySFXRandomOnSource(_effortSource, _pantClips, _pantVolume, pitch, pitch);
        }
    }

    public void PlayInteractEffort()
    {
        if (_effortSource == null) return;
        if (_interactEffortClips == null || _interactEffortClips.Length == 0) return;

        float pitch = Random.Range(_effortPitchRange.x, _effortPitchRange.y);
        AudioManager.Instance.PlaySFXRandomOnSource(_effortSource, _interactEffortClips, _interactEffortVolume, pitch, pitch);
    }
}