using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class EnemyDeath : MonoBehaviour
{
    [Header("Enemy Root")]
    public GameObject enemyRoot;

    [Header("Health")]
    public int enemyMaxHealth = 100;

    [Header("Animator")]
    public Animator animator;

    [Header("Death Animation Clip")]
    public AnimationClip deathAnimationClip;

    [Header("Fallback Animator State")]
    public string fallbackDeathStateName = "Die";
    public string baseLayerPrefix = "Base Layer.";

    [Header("Destroy")]
    public float destroyDelay = 5f;

    [Header("Disable After Death")]
    public bool disableMovementScripts = true;
    public bool disableCollidersAfterDeath = true;
    public float disableCollidersDelay = 0.3f;

    private PlayerStats stats;
    private bool isDead = false;

    private PlayableGraph deathGraph;
    private AnimationClipPlayable deathPlayable;

    void Awake()
    {
        if (enemyRoot == null)
        {
            enemyRoot = gameObject;
        }

        stats = enemyRoot.GetComponent<PlayerStats>();

        if (stats == null)
        {
            stats = enemyRoot.AddComponent<PlayerStats>();
        }

        stats.maxHealth = enemyMaxHealth;
        stats.currentHealth = enemyMaxHealth;
    }

    void Start()
    {
        if (animator == null)
        {
            animator = enemyRoot.GetComponentInChildren<Animator>();
        }

        stats.maxHealth = enemyMaxHealth;
        stats.currentHealth = enemyMaxHealth;

        Debug.Log("Enemy HP: " + stats.currentHealth);
    }

    void Update()
    {
        if (isDead) return;
        if (stats == null) return;

        if (stats.currentHealth <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;
        stats.currentHealth = 0;

        Debug.Log("Enemy died. Playing death animation.");

        if (disableMovementScripts)
        {
            DisableMovementScripts();
        }

        PlayDeathAnimation();

        if (disableCollidersAfterDeath)
        {
            yield return new WaitForSeconds(disableCollidersDelay);
            DisableAllColliders();
        }

        yield return new WaitForSeconds(destroyDelay);

        if (enemyRoot != null)
        {
            Destroy(enemyRoot);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void PlayDeathAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("EnemyDeath: Animator �� ������.");
            return;
        }

        animator.enabled = true;
        animator.applyRootMotion = true;

        if (deathAnimationClip != null)
        {
            PlayDeathClipDirectly();
        }
        else
        {
            PlayFallbackDeathState();
        }
    }

    void PlayDeathClipDirectly()
    {
        if (deathGraph.IsValid())
        {
            deathGraph.Destroy();
        }

        deathGraph = PlayableGraph.Create("Enemy Death Animation");
        deathGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(
            deathGraph,
            "Death Output",
            animator
        );

        deathPlayable = AnimationClipPlayable.Create(deathGraph, deathAnimationClip);
        deathPlayable.SetTime(0);
        deathPlayable.SetSpeed(1);

        output.SetSourcePlayable(deathPlayable);

        deathGraph.Play();

        StartCoroutine(HoldDeathPoseAfterClip());

        Debug.Log("Playing death clip: " + deathAnimationClip.name);
    }

    IEnumerator HoldDeathPoseAfterClip()
    {
        if (deathAnimationClip == null) yield break;

        yield return new WaitForSeconds(deathAnimationClip.length);

        if (deathGraph.IsValid() && deathPlayable.IsValid())
        {
            double endTime = Mathf.Max(0f, deathAnimationClip.length - 0.02f);

            deathPlayable.SetTime(endTime);
            deathPlayable.SetSpeed(0);
            deathGraph.Evaluate();

            Debug.Log("Death animation finished. Holding last pose.");
        }
    }

    void PlayFallbackDeathState()
    {
        string fullStateName = baseLayerPrefix + fallbackDeathStateName;

        int fullHash = Animator.StringToHash(fullStateName);
        int simpleHash = Animator.StringToHash(fallbackDeathStateName);

        if (animator.HasState(0, fullHash))
        {
            animator.CrossFadeInFixedTime(fullStateName, 0.1f, 0);
        }
        else if (animator.HasState(0, simpleHash))
        {
            animator.CrossFadeInFixedTime(fallbackDeathStateName, 0.1f, 0);
        }
        else
        {
            Debug.LogWarning("EnemyDeath: �� ������ state ������: " + fallbackDeathStateName);
        }
    }

    void DisableMovementScripts()
    {
        ThirdPersonController thirdPersonController = enemyRoot.GetComponent<ThirdPersonController>();

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        PlayerTestKeys testKeys = enemyRoot.GetComponent<PlayerTestKeys>();

        if (testKeys != null)
        {
            testKeys.enabled = false;
        }

        CharacterController characterController = enemyRoot.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        Rigidbody rb = enemyRoot.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void DisableAllColliders()
    {
        Collider[] colliders = enemyRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        CharacterController[] controllers = enemyRoot.GetComponentsInChildren<CharacterController>(true);

        foreach (CharacterController controller in controllers)
        {
            controller.enabled = false;
        }
    }

    void OnDestroy()
    {
        if (deathGraph.IsValid())
        {
            deathGraph.Destroy();
        }
    }
}