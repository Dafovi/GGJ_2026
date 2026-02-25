using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    private UnityEvent _onDown;

    [SerializeField]
    private UnityEvent _onUp;

    private bool _isDown;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDown) return;
        _isDown = true;
        _onDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDown) return;
        _isDown = false;
        _onUp?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDown) return;
        _isDown = false;
        _onUp?.Invoke();
    }
}