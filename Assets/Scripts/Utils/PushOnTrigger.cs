using UnityEngine;

public sealed class PushOnTrigger : MonoBehaviour
{
    [SerializeField]
    private float _pushSpeed = 2.5f;

    [SerializeField]
    private Vector3 _localDirection = Vector3.forward;

    private void OnTriggerStay(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null)
            cc = other.GetComponentInParent<CharacterController>();

        if (cc == null) return;

        Vector3 dir = transform.TransformDirection(_localDirection).normalized;
        cc.Move(dir * _pushSpeed * Time.deltaTime);
    }
}