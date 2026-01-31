using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class MenuWrapNavigator : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _move;

    [SerializeField]
    private List<Selectable> _items = new List<Selectable>();

    [SerializeField]
    private int _startIndex = 0;

    [SerializeField]
    private float _threshold = 0.5f;

    [SerializeField]
    private float _deadzone = 0.2f;

    private int _currentIndex;
    private bool _locked;

    private void OnEnable()
    {
        _move.action.Enable();

        DisableUnityNavigation();

        _currentIndex = Mathf.Clamp(_startIndex, 0, Mathf.Max(0, _items.Count - 1));
        ForceSelectIndex(_currentIndex);
    }

    private void OnDisable()
    {
        _move.action.Disable();
        _locked = false;
    }

    private void Update()
    {
        if (_items == null || _items.Count == 0) return;

        Vector2 input = _move.action.ReadValue<Vector2>();
        float y = input.y;

        if (_locked)
        {
            if (Mathf.Abs(y) <= _deadzone)
                _locked = false;

            return;
        }

        int dir = 0;

        if (y > _threshold) dir = -1;
        else if (y < -_threshold) dir = +1;

        if (dir == 0) return;

        _locked = true;

        int next = WrapIndex(_currentIndex + dir, _items.Count);
        ForceSelectIndex(next);
    }

    private void ForceSelectIndex(int index)
    {
        _currentIndex = index;

        Selectable target = _items[_currentIndex];
        if (target == null) return;

        if (EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject == target.gameObject)
                return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }

        target.Select();
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