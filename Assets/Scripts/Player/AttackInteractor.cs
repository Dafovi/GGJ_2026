using UnityEngine;
using UnityEngine.InputSystem;

public sealed class AttackInteractor : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _attack;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float _range = 3f;

    [SerializeField]
    private LayerMask _interactableMask = ~0;

    [SerializeField]
    private PlayerFoleyController _playerFoley;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void OnEnable()
    {
        _attack.action.Enable();
        _attack.action.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        _attack.action.performed -= OnAttackPerformed;
        _attack.action.Disable();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (_camera == null) return;

        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, _range, _interactableMask, QueryTriggerInteraction.Collide))
            return;

        if (!hit.collider.TryGetComponent(out InteractableEvent interactable))
            interactable = hit.collider.GetComponentInParent<InteractableEvent>();

        if (interactable == null) return;

        _playerFoley?.PlayInteractEffort();

        interactable.Interact();
    }
}