using UnityEngine;

public sealed class AudioRoomTracker : MonoBehaviour
{
    public AudioRoom CurrentRoom { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        AudioRoom room = other.GetComponent<AudioRoom>();
        if (room != null) CurrentRoom = room;
    }

    private void OnTriggerExit(Collider other)
    {
        AudioRoom room = other.GetComponent<AudioRoom>();
        if (room != null && CurrentRoom == room) CurrentRoom = null;
    }
}