using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReadTTSOnSelected : ReadTTS, ISelectHandler, IPointerEnterHandler
{
    [SerializeField]
    private Button _button;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    protected override void OnEnable()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    public override void Read()
    {
        AudioManager.Instance.PlayTTS(_clip);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Read();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Read();
    }
}