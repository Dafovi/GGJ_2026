using UnityEngine;

public sealed class MobileVibration : MonoBehaviour
{
    [SerializeField]
    private bool _enableVibration = true;

    [SerializeField]
    private float _holdInterval = 0.25f;

    private float _nextHoldTime;

    public void VibrateTap()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!_enableVibration) return;
        Handheld.Vibrate();
#endif
    }

    public void VibrateHold()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!_enableVibration) return;

        if (Time.unscaledTime < _nextHoldTime) return;

        _nextHoldTime = Time.unscaledTime + _holdInterval;
        Handheld.Vibrate();
#endif
    }
}