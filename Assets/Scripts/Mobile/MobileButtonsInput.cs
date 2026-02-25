using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class MobileButtonsInput : MonoBehaviour
{
    [SerializeField]
    private MobileVibration _vibration;

    [SerializeField]
    private Button _forwardButton;

    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Button _lookLeftButton;

    [SerializeField]
    private Button _lookRightButton;

    [SerializeField]
    private Button _interactButton;

    [SerializeField]
    private float _lookDegreesPerSecond = 120f;

    private bool _forwardHeld;
    private bool _backHeld;
    private bool _lookLeftHeld;
    private bool _lookRightHeld;

    private bool _interactPressed;

    private void Awake()
    {
        BindHold(_forwardButton, v => _forwardHeld = v);
        BindHold(_backButton, v => _backHeld = v);
        BindHold(_lookLeftButton, v => _lookLeftHeld = v);
        BindHold(_lookRightButton, v => _lookRightHeld = v);

        if (_interactButton != null)
            _interactButton.onClick.AddListener(() => _interactPressed = true);
    }

    private void Update()
    {
        if (_forwardHeld || _backHeld || _lookLeftHeld || _lookRightHeld)
            _vibration?.VibrateHold();
    }

    private void BindHold(Button button, System.Action<bool> setter)
    {
        if (button == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();

        AddTrigger(trigger, EventTriggerType.PointerDown, () =>
        {
            setter(true);
            _vibration?.VibrateTap();
        });

        AddTrigger(trigger, EventTriggerType.PointerUp, () =>
        {
            setter(false);
        });

        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            setter(false);
        });
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    public float GetMoveY()
    {
        if (_forwardHeld && _backHeld) return 0f;
        if (_forwardHeld) return 1f;
        if (_backHeld) return -1f;
        return 0f;
    }

    public float GetYawDelta()
    {
        float dir = 0f;

        if (_lookLeftHeld && _lookRightHeld) dir = 0f;
        else if (_lookRightHeld) dir = 1f;
        else if (_lookLeftHeld) dir = -1f;

        return dir * _lookDegreesPerSecond * Time.deltaTime;
    }

    public bool ConsumeInteractPressed()
    {
        bool v = _interactPressed;
        _interactPressed = false;
        return v;
    }
}