using UnityEngine;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public sealed class CollisionHitOverlay : MonoBehaviour
{
    [SerializeField]
    private ProximityOverlayDriver _overlay;

    [SerializeField]
    private LayerMask _hitMask = ~0;

    [SerializeField]
    private Color _hitColor = Color.red;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_overlay == null) return;

        int layerBit = 1 << hit.collider.gameObject.layer;
        if ((_hitMask.value & layerBit) == 0) return;

        _overlay.SetGlowColor(_hitColor);
    }
}