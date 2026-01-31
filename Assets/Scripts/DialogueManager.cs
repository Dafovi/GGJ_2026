using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI _text;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if(_text != null)
        {
            _text.gameObject.SetActive(false);
        }
    }

    public void ShowDialogue(string message)
    {
        if(_text != null)
        {
            _text.text = message;
            _text.gameObject.SetActive(true);
        }
    }

    public void WaitToHideDialogue(AudioClip audioClip)
    {
        if(audioClip != null)
        {
            float delay = audioClip.length;
            Invoke(nameof(HideDialogue), delay);
        }
    }

    public void HideDialogue()
    {
        if(_text != null)
        {
            _text.gameObject.SetActive(false);
        }
    }
}