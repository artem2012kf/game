using UnityEngine;

public class RPGHud : MonoBehaviour
{
    [Header("Player")]
    public PlayerStats playerStats;

    [Header("Keys")]
    public KeyCode toggleKey1 = KeyCode.I;
    public KeyCode toggleKey2 = KeyCode.P;
    public KeyCode toggleKey3 = KeyCode.M;

    private bool statsMenuOpen = false;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;
    private GUIStyle plusButtonStyle;

    void Start()
    {
        if (playerStats == null)
        {
            playerStats = UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerStats == null)
        {
            Debug.LogWarning("RPGHud: PlayerStats not found. Drag your player into Player Stats.");
        }
    }

    void Update()
    {
        if (
            Input.GetKeyDown(toggleKey1) ||
            Input.GetKeyDown(toggleKey2) ||
            Input.GetKeyDown(toggleKey3)
        )
        {
            ToggleStatsMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && statsMenuOpen)
        {
            CloseStatsMenu();
        }
    }

    void OnGUI()
    {
        InitStyles();

        DrawMainHud();
        DrawOpenButton();

        if (statsMenuOpen)
        {
            DrawStatsMenu();
        }
    }

    void InitStyles()
    {
        if (panelStyle != null) return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeTexture(new Color(0f, 0f, 0f, 0.75f));
        panelStyle.padding = new RectOffset(15, 15, 15, 15);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 26;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 20;
        textStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.fontStyle = FontStyle.Bold;

        plusButtonStyle = new GUIStyle(GUI.skin.button);
        plusButtonStyle.fontSize = 28;
        plusButtonStyle.fontStyle = FontStyle.Bold;
    }

    void DrawMainHud()
    {
        GUILayout.BeginArea(new Rect(15, 15, 390, 180), panelStyle);

        if (playerStats == null)
        {
            GUILayout.Label("PlayerStats not found", titleStyle);
            GUILayout.Label("Drag Player into RPGHud", textStyle);
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label("Level: " + playerStats.level, titleStyle);

        DrawBar(
            "HP",
            playerStats.currentHealth,
            playerStats.maxHealth,
            new Color(0.85f, 0.05f, 0.05f),
            330,
            24
        );

        DrawBar(
            "XP",
            playerStats.experience,
            playerStats.experienceToNextLevel,
            new Color(0.1f, 0.45f, 1f),
            330,
            22
        );

        GUILayout.Label("I / P / M - upgrade menu", textStyle);

        GUILayout.EndArea();
    }

    void DrawOpenButton()
    {
        float width = 190f;
        float height = 55f;
        float x = Screen.width - width - 20f;
        float y = 20f;

        if (GUI.Button(new Rect(x, y, width, height), "UPGRADES", buttonStyle))
        {
            ToggleStatsMenu();
        }
    }

    void DrawStatsMenu()
    {
        float width = 460f;
        float height = 470f;
        float x = Screen.width / 2f - width / 2f;
        float y = Screen.height / 2f - height / 2f;

        GUILayout.BeginArea(new Rect(x, y, width, height), panelStyle);

        GUILayout.Label("Character Upgrades", titleStyle);
        GUILayout.Space(10);

        if (playerStats == null)
        {
            GUILayout.Label("PlayerStats not found", textStyle);
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label("Skill Points: " + playerStats.skillPoints, textStyle);
        GUILayout.Space(15);

        DrawStatRow("Strength", playerStats.strength, UpgradeStrength);
        DrawStatRow("Vitality", playerStats.vitality, UpgradeVitality);
        DrawStatRow("Agility", playerStats.agility, UpgradeAgility);

        GUILayout.Space(20);

        GUILayout.Label("Strength - damage", textStyle);
        GUILayout.Label("Vitality - health", textStyle);
        GUILayout.Label("Agility - speed and jump", textStyle);

        GUILayout.Space(20);

        if (GUILayout.Button("TEST: +3 Skill Points", buttonStyle, GUILayout.Height(45)))
        {
            playerStats.skillPoints += 3;
            Debug.Log("Added 3 skill points");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Close", buttonStyle, GUILayout.Height(45)))
        {
            CloseStatsMenu();
        }

        GUILayout.EndArea();
    }

    void DrawStatRow(string statName, int value, System.Action upgradeAction)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(statName + ": " + value, textStyle, GUILayout.Width(280));

        if (GUILayout.Button("+", plusButtonStyle, GUILayout.Width(70), GUILayout.Height(45)))
        {
            upgradeAction.Invoke();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
    }

    void DrawBar(string label, int current, int max, Color fillColor, float width, float height)
    {
        float percent = 0f;

        if (max > 0)
        {
            percent = (float)current / max;
        }

        percent = Mathf.Clamp01(percent);

        GUILayout.Label(label + ": " + current + " / " + max, textStyle);

        Rect backgroundRect = GUILayoutUtility.GetRect(width, height);

        GUI.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);

        Rect fillRect = new Rect(
            backgroundRect.x,
            backgroundRect.y,
            backgroundRect.width * percent,
            backgroundRect.height
        );

        GUI.color = fillColor;
        GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

        GUI.color = Color.white;
    }

    void ToggleStatsMenu()
    {
        statsMenuOpen = !statsMenuOpen;

        if (statsMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log("Upgrade menu: " + (statsMenuOpen ? "open" : "closed"));
    }

    void CloseStatsMenu()
    {
        statsMenuOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Upgrade menu closed");
    }

    void UpgradeStrength()
    {
        if (playerStats == null) return;

        if (playerStats.skillPoints <= 0)
        {
            Debug.Log("No skill points");
            return;
        }

        playerStats.UpgradeStrength();
    }

    void UpgradeVitality()
    {
        if (playerStats == null) return;

        if (playerStats.skillPoints <= 0)
        {
            Debug.Log("No skill points");
            return;
        }

        playerStats.UpgradeVitality();
    }

    void UpgradeAgility()
    {
        if (playerStats == null) return;

        if (playerStats.skillPoints <= 0)
        {
            Debug.Log("No skill points");
            return;
        }

        playerStats.UpgradeAgility();
    }

    Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}