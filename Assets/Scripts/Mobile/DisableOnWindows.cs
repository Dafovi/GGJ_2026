using UnityEngine;

public class DisableOnWindows : MonoBehaviour
{
    private void Awake()
    {
#if (!UNITY_ANDROID || !UNITY_IOS) && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
