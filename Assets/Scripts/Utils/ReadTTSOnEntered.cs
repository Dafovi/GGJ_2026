using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReadTTSOnSelected : ReadTTS, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TextMeshProUGUI _text;

    private Color _selectedColor;
    private Color _deselectedColor;
    private float _textScale = 1f;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    protected override void OnEnable()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        ChangeTextColorAndScale(false);
    }

    public override void Read()
    {
        AudioManager.Instance.PlayTTS(_clip);
        ChangeTextColorAndScale(true);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Read();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ChangeTextColorAndScale(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Read();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChangeTextColorAndScale(false);
    }

    public void SetColors(Color selectedColor, Color deselectedColor)
    {
        _selectedColor = selectedColor;
        _deselectedColor = deselectedColor;
    }

    public void SetTextScaler(float scale)
    {
        _textScale = scale;
    }

    public void ChangeTextColorAndScale(bool selected)
    {
        if (_text == null) return;

        _text.color = selected ? _selectedColor : _deselectedColor;
        _text.transform.localScale = Vector3.one * (selected ? _textScale : 1f);
    }
}