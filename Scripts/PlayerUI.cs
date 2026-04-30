using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Player")]
    public PlayerStats playerStats;

    [Header("Health UI")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("Experience UI")]
    public Slider experienceSlider;
    public TMP_Text experienceText;

    [Header("Level UI")]
    public TMP_Text levelText;
    public TMP_Text skillPointsText;

    [Header("Stats UI")]
    public TMP_Text strengthText;
    public TMP_Text vitalityText;
    public TMP_Text agilityText;

    void Start()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateUI;
            UpdateUI();
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        if (playerStats == null) return;

        healthSlider.maxValue = playerStats.maxHealth;
        healthSlider.value = playerStats.currentHealth;

        experienceSlider.maxValue = playerStats.experienceToNextLevel;
        experienceSlider.value = playerStats.experience;

        healthText.text = playerStats.currentHealth + " / " + playerStats.maxHealth;
        experienceText.text = playerStats.experience + " / " + playerStats.experienceToNextLevel;

        levelText.text = "Level: " + playerStats.level;
        skillPointsText.text = "Skill Points: " + playerStats.skillPoints;

        strengthText.text = "Strength: " + playerStats.strength;
        vitalityText.text = "Vitality: " + playerStats.vitality;
        agilityText.text = "Agility: " + playerStats.agility;
    }

    public void UpgradeStrengthButton()
    {
        playerStats.UpgradeStrength();
    }

    public void UpgradeVitalityButton()
    {
        playerStats.UpgradeVitality();
    }

    public void UpgradeAgilityButton()
    {
        playerStats.UpgradeAgility();
    }
}