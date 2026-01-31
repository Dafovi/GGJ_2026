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
        Vector2 input = _move.action.ReadValue<Vector2>();

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        _controller.Move(move * _moveSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        Vector2 look = _look.action.ReadValue<Vector2>();

        float yaw = look.x * _rotationSpeed * Time.deltaTime;
        _yRotation += yaw;

        transform.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }
}