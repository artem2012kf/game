using UnityEngine;
using TMPro;

public class NPC1 : MonoBehaviour
{
    public TextMeshProUGUI textObject;

    private bool playerNear;
    private int currentLine = 0;
    private bool isTalking = false;
    private float hideTimer;

    public float hideDelay = 10f;

    private string[] dialogueLines = new string[]
    {
        "Привет путник, как ты тут оказался?",
        "Не важно.",
        "Чтобы сбежать отсюда тебе нужно победить гоблинов и главного босса.",
        "Удачи!"
    };

    void Start()
    {
        textObject.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.T))
        {
            if (!isTalking)
            {
                StartDialogue();
            }
            else
            {
                NextLine();
            }
        }

        if (isTalking)
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0f)
            {
                EndDialogue();
            }
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        currentLine = 0;
        textObject.gameObject.SetActive(true);
        textObject.text = dialogueLines[currentLine];
        hideTimer = hideDelay;
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            textObject.text = dialogueLines[currentLine];
            hideTimer = hideDelay;
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        textObject.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            EndDialogue();
        }
    }
}