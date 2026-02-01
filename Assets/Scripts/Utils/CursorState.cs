using UnityEngine;

public class CursorState : MonoBehaviour
{
    [SerializeField]
    private CursorLockMode _lockMode = CursorLockMode.Confined;

    [SerializeField]
    private bool _visible = false;
    private void OnEnable()
    {
        Cursor.lockState = _lockMode;
        Cursor.visible = false;
    }
}
