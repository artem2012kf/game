using UnityEngine;

public class MenuOnScene : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject helpPanel;

    [Header("Player Scripts")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraLookScript;

    [Header("Game UI")]
    public GameObject gameUI;

    private void Start()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        helpPanel.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(false);

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (cameraLookScript != null)
            cameraLookScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        helpPanel.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (cameraLookScript != null)
            cameraLookScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    public void OpenHelp()
    {
        mainMenuPanel.SetActive(false);
        helpPanel.SetActive(true);
    }

    public void CloseHelp()
    {
        helpPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }
}