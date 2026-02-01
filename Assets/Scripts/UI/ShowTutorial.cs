using UnityEngine;

public class ShowTutorial : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(!GameManager.Instance.InstructionsReaded);
    }

    public void InstructionsReaded()
    {
        GameManager.Instance.InstructionsReaded = true;
    }

    public void ResetInstructions()
    {
        GameManager.Instance.InstructionsReaded = false;
    }
}
