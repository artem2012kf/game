using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAIController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Stats")]
    public PlayerStats enemyStats;
    public PlayerStats playerStats;

    [Header("AI Distances")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float stopDistance = 1.6f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    public float attackHitDelay = 0.45f;

    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 4f;
    public bool useRun = true;

    [Header("Animator")]
    public Animator animator;

    [Header("Animation Clips")]
    public AnimationClip idleClip;
    public AnimationClip moveClip;
    public AnimationClip attackClip;

    [Header("Fallback Animator States")]
    public string baseLayerPrefix = "Base Layer.";
    public string idleState = "Idle01";
    public string moveState = "WalkForward";
    public string attackState = "Attack01";

    [Header("Visual Direction Fix")]
    public Transform visualRoot;
    public bool useVisualYawOffset = true;
    public float visualYawOffset = 180f;

    private NavMeshAgent agent;
    private bool isAttacking;
    private float lastAttackTime;

    private string currentState;
    private AnimationClip currentClip;
    private bool currentClipLoop;

    private PlayableGraph animationGraph;
    private AnimationClipPlayable clipPlayable;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (enemyStats == null)
        {
            enemyStats = GetComponent<PlayerStats>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (visualRoot == null && animator != null)
        {
            visualRoot = animator.transform;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null && playerStats == null)
        {
            playerStats = player.GetComponent<PlayerStats>();

            if (playerStats == null)
            {
                playerStats = player.GetComponentInChildren<PlayerStats>();
            }
        }

        agent.speed = useRun ? runSpeed : walkSpeed;
        agent.stoppingDistance = stopDistance;
        agent.updateRotation = true;
        agent.updatePosition = true;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        PlayIdleAnimation();
        ApplyVisualRotationFix();
    }

    void Update()
    {
        if (IsEnemyDead())
        {
            StopEnemy();
            return;
        }

        if (player == null || playerStats == null)
        {
            StopEnemy();
            PlayIdleAnimation();
            return;
        }

        if (playerStats.currentHealth <= 0)
        {
            StopEnemy();
            PlayIdleAnimation();
            return;
        }

        if (isAttacking)
        {
            StopEnemy();
            LookAtPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            StopEnemy();
            PlayIdleAnimation();
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            StopEnemy();
            LookAtPlayer();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                PlayIdleAnimation();
            }

            return;
        }

        ChasePlayer();
    }

    void LateUpdate()
    {
        ApplyVisualRotationFix();
    }

    void ChasePlayer()
    {
        if (agent == null || !agent.enabled) return;
        if (player == null) return;

        agent.isStopped = false;
        agent.speed = useRun ? runSpeed : walkSpeed;
        agent.SetDestination(player.position);

        PlayMoveAnimation();
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        StopEnemy();
        LookAtPlayer();

        PlayAttackAnimation();

        yield return new WaitForSeconds(attackHitDelay);

        TryDamagePlayer();

        float attackLength = 0.6f;

        if (attackClip != null)
        {
            attackLength = attackClip.length;
        }

        float waitTime = Mathf.Max(0.1f, attackLength - attackHitDelay);
        yield return new WaitForSeconds(waitTime);

        isAttacking = false;
    }

    void TryDamagePlayer()
    {
        if (player == null || playerStats == null) return;
        if (playerStats.currentHealth <= 0) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange + 0.5f)
        {
            playerStats.TakeDamage(attackDamage);
            Debug.Log("Enemy hit player. Damage: " + attackDamage);
        }
    }

    void StopEnemy()
    {
        if (agent == null || !agent.enabled) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
        );
    }

    bool IsEnemyDead()
    {
        if (enemyStats == null) return false;

        return enemyStats.currentHealth <= 0;
    }

    void PlayIdleAnimation()
    {
        if (idleClip != null)
        {
            PlayClip(idleClip, true);
        }
        else
        {
            PlayAnimatorState(idleState);
        }
    }

    void PlayMoveAnimation()
    {
        if (moveClip != null)
        {
            PlayClip(moveClip, true);
        }
        else
        {
            PlayAnimatorState(moveState);
        }
    }

    void PlayAttackAnimation()
    {
        if (attackClip != null)
        {
            PlayClip(attackClip, false);
        }
        else
        {
            PlayAnimatorState(attackState);
        }
    }

    void PlayClip(AnimationClip clip, bool loop)
    {
        if (animator == null) return;
        if (clip == null) return;

        if (currentClip == clip && currentClipLoop == loop)
        {
            return;
        }

        DestroyAnimationGraph();

        currentClip = clip;
        currentClipLoop = loop;
        currentState = "";

        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

        animationGraph = PlayableGraph.Create("Enemy AI Animation Graph");
        animationGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(
            animationGraph,
            "Animation Output",
            animator
        );

        clipPlayable = AnimationClipPlayable.Create(animationGraph, clip);
        clipPlayable.SetTime(0);
        clipPlayable.SetSpeed(1);
        clipPlayable.SetApplyFootIK(true);

        output.SetSourcePlayable(clipPlayable);

        animationGraph.Play();
    }

    void PlayAnimatorState(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;
        if (currentState == stateName) return;

        DestroyAnimationGraph();

        currentState = stateName;
        currentClip = null;

        string fullStateName = baseLayerPrefix + stateName;

        int fullHash = Animator.StringToHash(fullStateName);
        int simpleHash = Animator.StringToHash(stateName);

        if (animator.HasState(0, fullHash))
        {
            animator.CrossFadeInFixedTime(fullStateName, 0.15f, 0);
        }
        else if (animator.HasState(0, simpleHash))
        {
            animator.CrossFadeInFixedTime(stateName, 0.15f, 0);
        }
        else
        {
            Debug.LogWarning("EnemyAI: animation not found: " + stateName);
        }
    }

    void ApplyVisualRotationFix()
    {
        if (!useVisualYawOffset) return;
        if (visualRoot == null) return;
        if (visualRoot == transform) return;

        visualRoot.localRotation = Quaternion.Euler(0f, visualYawOffset, 0f);
    }

    void DestroyAnimationGraph()
    {
        if (animationGraph.IsValid())
        {
            animationGraph.Destroy();
        }
    }

    void OnDisable()
    {
        DestroyAnimationGraph();
    }

    void OnDestroy()
    {
        DestroyAnimationGraph();
    }
}