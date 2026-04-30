using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    public void StartGame()
    {
        Debug.Log("START BUTTON PRESSED");
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("EXIT BUTTON PRESSED");
        Application.Quit();
    }
}