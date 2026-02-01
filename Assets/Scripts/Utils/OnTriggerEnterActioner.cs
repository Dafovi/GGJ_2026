using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterActioner : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        _onTriggerEnter?.Invoke();
    }
}
