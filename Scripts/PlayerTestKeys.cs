using UnityEngine;

public class PlayerTestKeys : MonoBehaviour
{
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerStats.TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            playerStats.Heal(10);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            playerStats.AddExperience(50);
        }
    }
}