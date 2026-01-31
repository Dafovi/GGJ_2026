using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class AudioRoom : MonoBehaviour
{
    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }
}