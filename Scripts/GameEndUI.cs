using UnityEngine;

public class GameEndUI : MonoBehaviour
{
    public GameObject toBeCompletedText;

    public void ShowToBeCompleted()
    {
        toBeCompletedText.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }
}