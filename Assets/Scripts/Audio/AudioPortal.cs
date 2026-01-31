using UnityEngine;

public enum PortalType
{
    OpenDoor,
    ClosedDoor,
    Window
}

public sealed class AudioPortal : MonoBehaviour
{
    [SerializeField]

    private PortalType _type = PortalType.OpenDoor;

    public PortalType Type => _type;
}