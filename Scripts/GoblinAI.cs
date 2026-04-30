using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoblinAI : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("Target")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Stats")]
    public PlayerStats goblinStats;
    public PlayerStats playerStats;

    [Header("AI")]
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float loseTargetRange = 22f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 12f;
    public float stoppingDistance = 1.7f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.4f;
    public float attackHitDelay = 0.45f;
    public float attackAnimationTime = 1.0f;

    [Header("Animator")]
    public Animator animator;
    public string idleState = "Idle";
    public string walkState = "Walk";
    public string attackState = "Attack";

    [Header("Model Direction Fix")]
    public Transform visualRoot;
    public bool useVisualYawOffset = true;
    public float visualYawOffset = 180f;

    private NavMeshAgent agent;
    private State state = State.Idle;

    private bool isAttacking = false;
    private bool bossKillNotified = false;
    private float nextAttackTime = 0f;
    private string currentAnimation = "";

    private Vector3 attackStartPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (goblinStats == null)
        {
            goblinStats = GetComponent<PlayerStats>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (visualRoot == null && animator != null)
        {
            visualRoot = animator.transform;
        }
    }

    void Start()
    {
        FindPlayer();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.isStopped = false;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        PlayAnimation(idleState);
    }

    void Update()
    {
        if (IsDead())
        {
            SetState(State.Dead);
            return;
        }

        FindPlayer();

        if (player == null || playerStats == null || playerStats.currentHealth <= 0)
        {
            SetState(State.Idle);
            return;
        }

        if (isAttacking)
        {
            StopAgent();
            LockHeightDuringAttack();
            RotateToPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > loseTargetRange)
        {
            SetState(State.Idle);
            return;
        }

        if (distance <= attackRange)
        {
            SetState(State.Attack);
            return;
        }

        if (distance <= detectionRange)
        {
            SetState(State.Chase);
            return;
        }

        SetState(State.Idle);
    }

    void LateUpdate()
    {
        ApplyVisualYaw();
    }

    void SetState(State newState)
    {
        state = newState;

        if (state == State.Idle)
        {
            Idle();
        }
        else if (state == State.Chase)
        {
            Chase();
        }
        else if (state == State.Attack)
        {
            Attack();
        }
        else if (state == State.Dead)
        {
            Dead();
        }
    }

    void Idle()
    {
        StopAgent();
        PlayAnimation(idleState);
    }

    void Chase()
    {
        if (player == null) return;
        if (agent == null || !agent.enabled) return;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(player.position);

        RotateToPlayer();
        PlayAnimation(walkState);
    }

    void Attack()
    {
        StopAgent();
        RotateToPlayer();

        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            PlayAnimation(idleState);
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        attackStartPosition = transform.position;

        StopAgent();
        RotateToPlayer();

        PlayAnimation(attackState, true);

        yield return new WaitForSeconds(attackHitDelay);

        HitPlayer();

        float waitAfterHit = Mathf.Max(0.05f, attackAnimationTime - attackHitDelay);
        yield return new WaitForSeconds(waitAfterHit);

        isAttacking = false;
        currentAnimation = "";

        ResumeAfterAttack();
    }

    void ResumeAfterAttack()
    {
        if (IsDead())
        {
            SetState(State.Dead);
            return;
        }

        if (player == null || playerStats == null || playerStats.currentHealth <= 0)
        {
            SetState(State.Idle);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            SetState(State.Attack);
        }
        else if (distance <= detectionRange)
        {
            SetState(State.Chase);
        }
        else
        {
            SetState(State.Idle);
        }
    }

    void HitPlayer()
    {
        if (player == null || playerStats == null) return;
        if (playerStats.currentHealth <= 0) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange + 0.5f)
        {
            playerStats.TakeDamage(attackDamage);
            Debug.Log("Goblin hit player. Damage: " + attackDamage);
        }
    }

    void Dead()
    {
        StopAgent();

        if (!bossKillNotified)
        {
            bossKillNotified = true;

            if (playerStats == null)
            {
                FindPlayer();
            }

            if (playerStats != null)
            {
                playerStats.BossKilled();
            }
        }

        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }

        enabled = false;
    }

    void FindPlayer()
    {
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
    }

    bool IsDead()
    {
        if (goblinStats == null) return false;
        return goblinStats.currentHealth <= 0;
    }

    void StopAgent()
    {
        if (agent == null || !agent.enabled) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    void RotateToPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void LockHeightDuringAttack()
    {
        Vector3 pos = transform.position;
        pos.y = attackStartPosition.y;
        transform.position = pos;
    }

    void PlayAnimation(string stateName)
    {
        PlayAnimation(stateName, false);
    }

    void PlayAnimation(string stateName, bool forceRestart)
    {
        if (animator == null)
        {
            Debug.LogWarning("GoblinAI: Animator �� ��������.");
            return;
        }

        if (string.IsNullOrEmpty(stateName)) return;

        if (!forceRestart && currentAnimation == stateName)
        {
            return;
        }

        currentAnimation = stateName;

        int hash = Animator.StringToHash(stateName);

        if (animator.HasState(0, hash))
        {
            animator.CrossFadeInFixedTime(stateName, 0.12f, 0);
        }
        else
        {
            Debug.LogWarning("GoblinAI: State �� ������ � Animator: " + stateName);
        }
    }

    void ApplyVisualYaw()
    {
        if (!useVisualYawOffset) return;
        if (visualRoot == null) return;
        if (visualRoot == transform) return;

        visualRoot.localRotation = Quaternion.Euler(0f, visualYawOffset, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}