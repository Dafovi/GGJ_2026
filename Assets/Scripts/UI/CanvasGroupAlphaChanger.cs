using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CanvasGroupAlphaChanger : MonoBehaviour
{
    [SerializeField]
    private bool _fadeIn = true;
    
    [SerializeField]
    private float _duration = 1f;
    
    [SerializeField]
    private bool _fadeOut = true;

    [SerializeField]
    private float _delayBeforeFadeOut = 1f;

    public UnityEvent _onFadeInEnded;
    public UnityEvent _onFadeOutEnded;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_fadeIn) _canvasGroup.alpha = 0f;
        else _canvasGroup.alpha = 1f;
    }

    IEnumerator Start()
    {
        if (_fadeIn) yield return FadeInCoroutine();

        yield return new WaitForSeconds(_delayBeforeFadeOut);

        if (_fadeOut) yield return FadeOutCoroutine();
    }

    public void TriggerFadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    public void TriggerFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < _duration)
        {
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / _duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1f;

        _onFadeInEnded?.Invoke();
    }

    IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < _duration)
        {
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / _duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 0f;

        _onFadeOutEnded?.Invoke();
    }
}