using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Level")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    public int skillPoints = 0;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("RPG Stats")]
    public int strength = 5;
    public int vitality = 5;
    public int agility = 5;

    [Header("Gameplay Bonuses")]
    public float moveSpeedBonus = 0f;
    public float jumpBonus = 0f;

    [Header("Damage Popup")]
    public bool showDamagePopup = true;
    public Vector3 damagePopupOffset = new Vector3(0f, 2f, 0f);

    [Header("End Game UI")]
    public GameObject toBeCompletedText;

    private bool gameCompleted = false;

    public System.Action OnStatsChanged;

    void Start()
    {
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        if (toBeCompletedText != null)
        {
            toBeCompletedText.SetActive(false);
        }

        OnStatsChanged?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (showDamagePopup && damage > 0)
        {
            Vector3 popupPosition = transform.position + damagePopupOffset;
            DamagePopup.Create(popupPosition, damage);
        }

        OnStatsChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnStatsChanged?.Invoke();
    }

    public void AddExperience(int amount)
    {
        experience += amount;

        while (experience >= experienceToNextLevel)
        {
            experience -= experienceToNextLevel;
            LevelUp();
        }

        OnStatsChanged?.Invoke();
    }

    void LevelUp()
    {
        level++;
        skillPoints += 3;

        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.35f);

        maxHealth += 10;
        currentHealth = maxHealth;

        Debug.Log("Level Up! Новый уровень: " + level);

        OnStatsChanged?.Invoke();
    }

    public void UpgradeStrength()
    {
        if (skillPoints <= 0) return;

        skillPoints--;
        strength++;

        OnStatsChanged?.Invoke();
    }

    public void UpgradeVitality()
    {
        if (skillPoints <= 0) return;

        skillPoints--;
        vitality++;

        maxHealth += 15;
        currentHealth += 15;

        OnStatsChanged?.Invoke();
    }

    public void UpgradeAgility()
    {
        if (skillPoints <= 0) return;

        skillPoints--;
        agility++;

        moveSpeedBonus += 0.2f;
        jumpBonus += 0.05f;

        OnStatsChanged?.Invoke();
    }

    public int GetDamage()
    {
        return 10 + strength * 2;
    }

    public void BossKilled()
    {
        if (gameCompleted) return;

        gameCompleted = true;

        GameObject canvasObj = new GameObject("END_SCREEN_CANVAS");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject textObj = new GameObject("TO_BE_COMPLETED_TEXT");
        textObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = "TO BE COMPLETED";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 90;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("END SCREEN CREATED");
    }

    private void ShowCompletedText()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("EndCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        canvas.gameObject.SetActive(true);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject textObj;

        if (toBeCompletedText != null)
        {
            textObj = toBeCompletedText;
            textObj.SetActive(true);
        }
        else
        {
            textObj = new GameObject("ToBeCompletedText");
            textObj.transform.SetParent(canvas.transform, false);
            toBeCompletedText = textObj;
        }

        textObj.transform.SetParent(canvas.transform, false);
        textObj.transform.SetAsLastSibling();
        textObj.SetActive(true);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = textObj.AddComponent<RectTransform>();
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1000f, 250f);
        rect.localScale = Vector3.one;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            tmp = textObj.AddComponent<TextMeshProUGUI>();
        }

        tmp.text = "TO BE COMPLETED";
        tmp.fontSize = 72;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        Debug.Log("TO BE COMPLETED TEXT FORCED ON SCREEN");
    }

    void Die()
    {
        Debug.Log(gameObject.name + " умер");
    }
}