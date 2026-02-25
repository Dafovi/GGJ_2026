using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _move;

    [SerializeField]
    private InputActionReference _look;

    [SerializeField]
    private MobileButtonsInput _mobileInput;

    [SerializeField]
    private Transform _cameraRoot;

    [SerializeField]
    private float _moveSpeed = 4f;

    [SerializeField]
    private float _rotationSpeed = 120f;

    private CharacterController _controller;

    private float _yRotation;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _move.action.Enable();
        _look.action.Enable();
    }

    private void OnDisable()
    {
        _move.action.Disable();
        _look.action.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        float inputY = 0f;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (_mobileInput != null)
            inputY = _mobileInput.GetMoveY();
#else
        inputY = _move.action.ReadValue<Vector2>().y;
#endif

        Vector3 move = transform.forward * inputY;

        _controller.Move(move * _moveSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        float yawDelta = 0f;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        yawDelta = _mobileInput.GetYawDelta();
#else
        yawDelta = _look.action.ReadValue<Vector2>().x * _rotationSpeed * Time.deltaTime;
#endif

        _yRotation += yawDelta;

        transform.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }

    public bool ConsumeMobileInteract()
    {
        if (_mobileInput == null) return false;
        return _mobileInput.ConsumeInteractPressed();
    }
}