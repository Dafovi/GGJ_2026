using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneTransitionController : MonoBehaviour
{
    [SerializeField]
    private CanvasGroupAlphaChanger _canvasChanger;

    [SerializeField]
    private string _sceneToLoad;

    private bool _requested;

    private void Awake()
    {
        if (_canvasChanger != null)
            _canvasChanger._onFadeOutEnded.AddListener(OnFadeOutEnded);
    }

    private void OnDestroy()
    {
        if (_canvasChanger != null)
            _canvasChanger._onFadeOutEnded.RemoveListener(OnFadeOutEnded);
    }

    public void ChangeScene()
    {
        if (_requested) return;
        _requested = true;

        _canvasChanger.TriggerFadeOut();
        AudioManager.Instance.StopMusic();
    }

    private void OnFadeOutEnded()
    {
        SceneManager.LoadScene(_sceneToLoad);
    }
}