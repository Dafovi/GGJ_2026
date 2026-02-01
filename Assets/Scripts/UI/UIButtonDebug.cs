using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIButtonDebug : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [SerializeField]
    private Button _button;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button != null)
            _button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log($"[UIButton] SELECT {name}");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Debug.Log($"[UIButton] DESELECT {name}");
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log($"[UIButton] SUBMIT {name}");
    }

    private void OnClicked()
    {
        string es = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null
            ? EventSystem.current.currentSelectedGameObject.name
            : "NULL";

        Debug.Log($"[UIButton] CLICK {name} | EventSystem selected={es}");
    }
}