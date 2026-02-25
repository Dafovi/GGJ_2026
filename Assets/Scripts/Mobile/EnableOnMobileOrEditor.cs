using UnityEngine;

public sealed class EnableOnMobileOrEditor : MonoBehaviour
{
    private void Awake()
    {
        bool isMobile = Application.isMobilePlatform || Application.isEditor;

        gameObject.SetActive(isMobile);
    }
}