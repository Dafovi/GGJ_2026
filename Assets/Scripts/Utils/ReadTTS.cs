using UnityEngine;

public class ReadTTS : MonoBehaviour
{
    [SerializeField]
    protected AudioClip _clip;

    [SerializeField]
    protected float _delayBeforeReading = 0.5f;

    protected virtual void OnEnable()
    {
        Read();
    }

    public virtual void Read()
    {
        if (GameManager.Instance.UseTTS)
            AudioManager.Instance.PlayTTS(_clip, _delayBeforeReading);
    }
}
