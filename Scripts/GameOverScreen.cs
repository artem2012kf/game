using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Player")]
    public PlayerStats playerStats;

    [Header("UI Text")]
    public string titleText = "ПОРАЖЕНИЕ";
    public string descriptionText = "Ваш персонаж погиб.";
    public string restartButtonText = "Начать заново";
    public string quitButtonText = "Выйти";

    [Header("Settings")]
    public bool pauseGameOnDeath = true;
    public bool showQuitButton = true;

    private bool gameOver = false;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;

    void Start()
    {
        if (playerStats == null)
        {
            playerStats = UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (gameOver) return;
        if (playerStats == null) return;

        if (playerStats.currentHealth <= 0)
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        gameOver = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }
    }

    void OnGUI()
    {
        if (!gameOver) return;

        InitStyles();

        float width = 520f;
        float height = showQuitButton ? 330f : 260f;

        float x = Screen.width / 2f - width / 2f;
        float y = Screen.height / 2f - height / 2f;

        GUILayout.BeginArea(new Rect(x, y, width, height), panelStyle);

        GUILayout.Space(20);

        GUILayout.Label(titleText, titleStyle);

        GUILayout.Space(20);

        GUILayout.Label(descriptionText, textStyle);

        GUILayout.Space(30);

        if (GUILayout.Button(restartButtonText, buttonStyle, GUILayout.Height(55)))
        {
            RestartGame();
        }

        if (showQuitButton)
        {
            GUILayout.Space(15);

            if (GUILayout.Button(quitButtonText, buttonStyle, GUILayout.Height(55)))
            {
                QuitGame();
            }
        }

        GUILayout.EndArea();
    }

    void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void InitStyles()
    {
        if (panelStyle != null) return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeTexture(new Color(0f, 0f, 0f, 0.85f));
        panelStyle.padding = new RectOffset(25, 25, 25, 25);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 42;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.red;

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 24;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 24;
        buttonStyle.fontStyle = FontStyle.Bold;
    }

    Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}