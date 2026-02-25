using UnityEngine;
using UnityEngine.UI;

public class DebugButton : MonoBehaviour
{
#if UNITY_EDITOR
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    [ContextMenu("Click")]
    public void Click()
    {
        _button.onClick.Invoke();
        Debug.Log("DebugButton clicked!");
    }

#endif
}