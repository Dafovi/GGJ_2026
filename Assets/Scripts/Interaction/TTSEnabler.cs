using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TTSEnabler : MonoBehaviour
{
    [SerializeField]
    private AudioClip _intro;

    [SerializeField]
    private float _delayBeforeIntro = 1f;

    [SerializeField]
    private float _waitTimeToContinue = 10f;

    [SerializeField]
    private InputActionReference _keys;

    [SerializeField]
    private CanvasGroupAlphaChanger _canvasChanger;

    private Coroutine _waitCoroutine;
    private bool _resolved;

    private void Awake()
    {
        AudioManager.Instance.OnTTSEnded += HandleTTSEnded;
    }

    private void OnEnable()
    {
        _resolved = false;

        _keys.action.Enable();
        _keys.action.performed += OnKeysPerformed;

        AudioManager.Instance.PlayTTS(_intro, _delayBeforeIntro);
    }

    private void OnDisable()
    {
        _keys.action.performed -= OnKeysPerformed;
        _keys.action.Disable();

        StopWaitCoroutine();
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnTTSEnded -= HandleTTSEnded;
    }

    private void HandleTTSEnded()
    {
        if (!isActiveAndEnabled) return;
        if (_resolved) return;

        StopWaitCoroutine();
        _waitCoroutine = StartCoroutine(WaitToContinue());
    }

    private IEnumerator WaitToContinue()
    {
        yield return new WaitForSecondsRealtime(_waitTimeToContinue);
        Resolve(useTts: true);
    }

    private void OnKeysPerformed(InputAction.CallbackContext context)
    {
        if (_resolved) return;

        Vector2 input = context.ReadValue<Vector2>();
        float horizontal = input.x;

        if (horizontal > 0f)
        {
            Resolve(useTts: false);
        }
        else if (horizontal < 0f)
        {
            Resolve(useTts: true);
        }
    }

    private void Resolve(bool useTts)
    {
        if (_resolved) return;
        _resolved = true;

        StopWaitCoroutine();

        GameManager.Instance.UseTTS = useTts;

        AudioManager.Instance.StopTTS();
        _canvasChanger.TriggerFadeOut();
    }

    private void StopWaitCoroutine()
    {
        if (_waitCoroutine == null) return;
        StopCoroutine(_waitCoroutine);
        _waitCoroutine = null;
    }
}