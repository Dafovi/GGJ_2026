using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class MenuWrapNavigator : MonoBehaviour
{
    [SerializeField]
    private Color _selectedColor;

    [SerializeField]
    private Color _deselectedColor;

    [SerializeField]
    private float _textScaleSelected = 1.2f;

    [SerializeField]
    private InputActionReference _move;

    [SerializeField]
    private InputActionReference _submit;

    [SerializeField]
    private List<Selectable> _items = new List<Selectable>();

    [SerializeField]
    private float _startDelay = 0.2f;

    [SerializeField]
    private int _startIndex = 0;

    [SerializeField]
    private float _threshold = 0.5f;

    [SerializeField]
    private float _deadzone = 0.2f;

    private int _currentIndex;
    private bool _lockedY;
    private bool _lockedX;

    private bool _prevSendNavEvents;

    private void Awake()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            Selectable item = _items[i];
            if (item == null) continue;

            if (item.TryGetComponent(out ReadTTSOnSelected tts))
            {
                tts.SetColors(_selectedColor, _deselectedColor);
                tts.SetTextScaler(_textScaleSelected);
                tts.ChangeTextColorAndScale(false);
            }
        }
    }

    private void OnEnable()
    {
        if (EventSystem.current != null)
        {
            _prevSendNavEvents = EventSystem.current.sendNavigationEvents;
            EventSystem.current.sendNavigationEvents = false;
        }

        _move.action.Enable();

        if (_submit != null)
        {
            _submit.action.Enable();
            _submit.action.performed += OnSubmit;
        }

        StartNavigation();
    }

    private async void StartNavigation()
    {
        await Task.Delay(Mathf.RoundToInt(_startDelay * 1000));

        DisableUnityNavigation();

        _currentIndex = Mathf.Clamp(_startIndex, 0, Mathf.Max(0, _items.Count - 1));
        ForceSelectIndex(_currentIndex);
    }

    private void OnDisable()
    {
        if (_submit != null)
        {
            _submit.action.performed -= OnSubmit;
            _submit.action.Disable();
        }

        _move.action.Disable();

        _lockedY = false;
        _lockedX = false;

        if (EventSystem.current != null)
            EventSystem.current.sendNavigationEvents = _prevSendNavEvents;
    }

    private void Update()
    {
        if (_items == null || _items.Count == 0) return;

        Vector2 input = _move.action.ReadValue<Vector2>();

        HandleVertical(input.y);
        HandleRightClick(input.x);
    }

    private void HandleVertical(float y)
    {
        if (_lockedY)
        {
            if (Mathf.Abs(y) <= _deadzone)
                _lockedY = false;

            return;
        }

        int dir = 0;

        if (y > _threshold) dir = -1;
        else if (y < -_threshold) dir = +1;

        if (dir == 0) return;

        _lockedY = true;

        int next = WrapIndex(_currentIndex + dir, _items.Count);
        ForceSelectIndex(next);
    }

    private void HandleRightClick(float x)
    {
        if (_lockedX)
        {
            if (Mathf.Abs(x) <= _deadzone)
                _lockedX = false;

            return;
        }

        if (x <= _threshold) return;

        _lockedX = true;

        ClickCurrent();
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        ClickCurrent();
    }

    private void ClickCurrent()
    {
        if (_currentIndex < 0 || _currentIndex >= _items.Count) return;

        Selectable current = _items[_currentIndex];
        if (current == null) return;

        if (current.TryGetComponent(out Button button))
            button.onClick.Invoke();
        else
        {
            Button parentButton = current.GetComponentInParent<Button>();
            if (parentButton != null)
                parentButton.onClick.Invoke();
        }
    }

    private void ForceSelectIndex(int index)
    {
        if (_items == null || _items.Count == 0) return;

        index = Mathf.Clamp(index, 0, _items.Count - 1);

        Selectable previous = (_currentIndex >= 0 && _currentIndex < _items.Count) ? _items[_currentIndex] : null;
        _currentIndex = index;

        Selectable target = _items[_currentIndex];
        if (target == null) return;

        if (previous != null && previous != target)
        {
            if (previous.TryGetComponent(out ReadTTSOnSelected prevTts))
                prevTts.ChangeTextColorAndScale(false);
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target.gameObject);

        target.Select();

        if (target.TryGetComponent(out ReadTTSOnSelected tts))
            tts.ChangeTextColorAndScale(true);
    }

    private void DisableUnityNavigation()
    {
        foreach (var item in _items)
        {
            if (item == null) continue;

            var nav = item.navigation;
            nav.mode = Navigation.Mode.None;
            item.navigation = nav;
        }
    }

    private static int WrapIndex(int i, int count)
    {
        if (count <= 0) return 0;
        i %= count;
        if (i < 0) i += count;
        return i;
    }
}