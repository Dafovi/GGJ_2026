using UnityEngine;

public sealed class AudioPortalLink : MonoBehaviour
{
    [SerializeField]
    private AudioRoom _roomA;

    [SerializeField]
    private AudioRoom _roomB;

    [SerializeField]
    private OcclusionMaterial _material = OcclusionMaterial.DoorClosed;

    [SerializeField]
    private bool _enabled = true;

    public bool Enabled => _enabled;
    public OcclusionMaterial Material => _material;

    public AudioRoom RoomA => _roomA;
    public AudioRoom RoomB => _roomB;

    public AudioRoom GetOther(AudioRoom room)
    {
        if (room == _roomA) return _roomB;
        if (room == _roomB) return _roomA;
        return null;
    }

    public void SetEnabled(bool value) => _enabled = value;
    public void SetMaterial(OcclusionMaterial value) => _material = value;
}