using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSmoothTime = 0.1f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Footsteps")]
    public AudioSource footstepAudio;
    public float stepDelay = 0.45f;
    public bool isCrouching = false;

    [Header("Combat")]
    public Transform attackPoint;
    public float attackDistance = 1.4f;
    public float attackRadius = 1.1f;
    public LayerMask attackLayer = ~0;

    public int xpPerHit = 10;
    public int xpPerKill = 50;
    public float attackHitDelay = 0.25f;

    [Header("Projectile Attack Level 3")]
    public int projectileAttackRequiredLevel = 3;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 3f;
    public float projectileLaunchDelay = 0.25f;
    public float projectileDamageMultiplier = 1.5f;
    public string projectileAttackState = "Attack02Start";
    public float projectileAttackLockTime = 0.8f;

    [Header("Projectile Aim")]
    public KeyCode projectileAimKey = KeyCode.Alpha2;
    public bool showProjectileAim = true;
    public float aimRotationSpeed = 15f;
    public float aimRayDistance = 100f;
    public LayerMask aimRayLayers = ~0;

    public float crosshairSize = 18f;
    public float crosshairGap = 8f;
    public float crosshairThickness = 3f;

    [Header("Explosive Projectile Level 5")]
    public int specialAttackRequiredLevel = 5;
    public KeyCode explosiveAimKey = KeyCode.Alpha3;
    public bool showExplosiveAim = true;

    public GameObject explosiveProjectilePrefab;
    public GameObject explosionEffectPrefab;
    public Transform explosiveProjectileSpawnPoint;

    public string specialAttackState = "Attack02Maintain";
    public float specialAttackLockTime = 1f;

    public float explosiveProjectileSpeed = 9f;
    public float explosiveProjectileLifetime = 3f;
    public float explosiveProjectileScale = 0.9f;
    public float explosiveLaunchDelay = 0.3f;

    public int explosiveDirectDamage = 15;
    public int explosiveExplosionDamage = 35;
    public float explosiveRadius = 3f;
    public LayerMask explosiveHitLayers = ~0;

    [Header("Ability Cooldowns")]
    public float projectileCooldown = 2f;
    public float explosiveCooldown = 5f;

    [Header("Messages")]
    public float messageDuration = 2f;

    [Header("Battle Mode")]
    public bool battleMode = false;
    public KeyCode toggleBattleModeKey = KeyCode.Tab;

    [Header("Animation Settings")]
    public string baseLayerPrefix = "Base Layer.";
    public float animationFade = 0.12f;

    [Header("Locomotion Animation Names")]
    public string idleState = "Idle01";
    public string walkForwardState = "WalkForward";
    public string runForwardState = "BattleRunForward";

    public string battleWalkForwardState = "BattleWalkForward";
    public string battleRunForwardState = "BattleRunForward";
    public string battleWalkBackState = "BattleWalkBack";
    public string battleWalkLeftState = "BattleWalkLeft";
    public string battleWalkRightState = "BattleWalkRight";

    [Header("Jump Animation Names")]
    public string jumpStartState = "JumpStart";
    public string jumpUpState = "JumpUp";
    public string jumpAirState = "JumpAir";
    public string jumpLandState = "JumpLand";

    [Header("Attack Animation Names")]
    public string attack01State = "Attack01";
    public string attack02StartState = "Attack02Start";
    public string attack02MaintainState = "Attack02Maintain";

    [Header("Action Animation Names")]
    public string defendStartState = "DefendStart";
    public string defendMaintainState = "DefendMaintain";
    public string defendHitState = "DefendHit";

    public string gotHitState = "GotHit";
    public string potionDrinkState = "PotionDrink";
    public string pickupState = "PickUp";
    public string interactState = "Interact";
    public string dieState = "Die";

    [Header("Extra Animation Names")]
    public string idle02State = "Idle02";
    public string idle03State = "Idle03";
    public string dizzyState = "Dizzy";
    public string victoryStartState = "VictoryStart";
    public string victoryMaintainState = "VictoryMaintain";
    public string dieRecoveryState = "DieRecovery";

    [Header("Action Timings")]
    public float attack01LockTime = 0.7f;
    public float defendStartLockTime = 0.2f;
    public float gotHitLockTime = 0.55f;
    public float potionDrinkLockTime = 1.2f;
    public float pickupLockTime = 1.0f;
    public float interactLockTime = 0.9f;
    public float jumpStartTime = 0.2f;
    public float jumpLandLockTime = 0.25f;

    private CharacterController controller;
    private PlayerStats playerStats;

    private Vector3 velocity;
    private float rotationVelocity;

    private string currentAnimation;
    private float actionLockTimer;
    private float jumpStartTimer;

    private bool wasGrounded;
    private bool isDefending;
    private bool isDead;

    private int lastHealth;

    private string screenMessage = "";
    private float screenMessageTimer = 0f;

    private bool isProjectileAiming = false;
    private bool isExplosiveAiming = false;

    private float stepTimer = 0f;

    private float nextProjectileTime = 0f;
    private float nextExplosiveTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (playerStats != null)
        {
            lastHealth = playerStats.currentHealth;
        }

        wasGrounded = controller.isGrounded;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateTimers();

        if (isProjectileAiming || isExplosiveAiming)
        {
            RotatePlayerToAimDirection();
        }

        if (Input.GetKeyDown(toggleBattleModeKey))
        {
            battleMode = !battleMode;
        }

        if (HandleDeathAndDamage())
        {
            ApplyGravity();
            return;
        }

        HandleActions();
        HandleFootsteps();

        if (
            actionLockTimer <= 0f &&
            !isDefending &&
            !isProjectileAiming &&
            !isExplosiveAiming
        )
        {
            HandleMovement();
        }

        ApplyGravity();
        HandleJumpAnimation();
    }

    void UpdateTimers()
    {
        if (actionLockTimer > 0f)
        {
            actionLockTimer -= Time.deltaTime;
        }

        if (jumpStartTimer > 0f)
        {
            jumpStartTimer -= Time.deltaTime;
        }

        if (screenMessageTimer > 0f)
        {
            screenMessageTimer -= Time.deltaTime;

            if (screenMessageTimer <= 0f)
            {
                screenMessage = "";
            }
        }
    }

    bool HandleDeathAndDamage()
    {
        if (playerStats == null) return false;

        if (playerStats.currentHealth <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                isProjectileAiming = false;
                isExplosiveAiming = false;
                PlayAnimation(dieState, 0.05f);
            }

            return true;
        }

        if (playerStats.currentHealth < lastHealth && !isDead)
        {
            if (isDefending)
            {
                PlayAction(defendHitState, gotHitLockTime);
            }
            else
            {
                PlayAction(gotHitState, gotHitLockTime);
            }
        }

        lastHealth = playerStats.currentHealth;
        return false;
    }

    void HandleActions()
    {
        if (isProjectileAiming)
        {
            if (Input.GetKeyUp(projectileAimKey))
            {
                ReleaseProjectileAim();
            }

            return;
        }

        if (isExplosiveAiming)
        {
            if (Input.GetKeyUp(explosiveAimKey))
            {
                ReleaseExplosiveAim();
            }

            return;
        }

        if (!Input.GetKey(KeyCode.Mouse1))
        {
            isDefending = false;
        }

        if (actionLockTimer > 0f)
        {
            return;
        }

        if (Input.GetKeyDown(projectileAimKey))
        {
            StartProjectileAim();
            return;
        }

        if (Input.GetKeyDown(explosiveAimKey))
        {
            StartExplosiveAim();
            return;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (!isDefending)
            {
                isDefending = true;
                PlayAction(defendStartState, defendStartLockTime);
            }
            else
            {
                PlayAnimation(defendMaintainState);
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayAction(attack01State, attack01LockTime);
            StartAttackHit(1f);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayAction(attack01State, attack01LockTime);
            StartAttackHit(1f);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayAction(potionDrinkState, potionDrinkLockTime);
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayAction(pickupState, pickupLockTime);
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayAction(interactState, interactLockTime);
            return;
        }
    }

    void StartProjectileAim()
    {
        if (playerStats == null)
        {
            ShowScreenMessage("PlayerStats не найден");
            return;
        }

        if (playerStats.level < projectileAttackRequiredLevel)
        {
            ShowScreenMessage("Не хватает уровня! Нужен " + projectileAttackRequiredLevel + " уровень.");
            return;
        }

        if (Time.time < nextProjectileTime)
        {
            int secondsLeft = Mathf.CeilToInt(nextProjectileTime - Time.time);
            ShowScreenMessage("Способность 2 перезаряжается: " + secondsLeft + " сек.");
            return;
        }

        isProjectileAiming = true;
        isDefending = false;
        currentAnimation = "";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ReleaseProjectileAim()
    {
        if (!isProjectileAiming) return;

        isProjectileAiming = false;

        nextProjectileTime = Time.time + projectileCooldown;

        PlayAction(projectileAttackState, projectileAttackLockTime);
        StartCoroutine(ShootProjectileAfterDelay());
    }

    void StartExplosiveAim()
    {
        if (playerStats == null)
        {
            ShowScreenMessage("PlayerStats не найден");
            return;
        }

        if (playerStats.level < specialAttackRequiredLevel)
        {
            ShowScreenMessage("Не хватает уровня! Нужен " + specialAttackRequiredLevel + " уровень.");
            return;
        }

        if (Time.time < nextExplosiveTime)
        {
            int secondsLeft = Mathf.CeilToInt(nextExplosiveTime - Time.time);
            ShowScreenMessage("Способность 3 перезаряжается: " + secondsLeft + " сек.");
            return;
        }

        isExplosiveAiming = true;
        isDefending = false;
        currentAnimation = "";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ReleaseExplosiveAim()
    {
        if (!isExplosiveAiming) return;

        isExplosiveAiming = false;

        nextExplosiveTime = Time.time + explosiveCooldown;

        PlayAction(specialAttackState, specialAttackLockTime);
        StartCoroutine(ShootExplosiveProjectileAfterDelay());
    }

    void RotatePlayerToAimDirection()
    {
        if (cameraTransform == null) return;

        Vector3 direction = cameraTransform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            aimRotationSpeed * Time.deltaTime
        );
    }

    Vector3 GetAimShootDirection(Vector3 spawnPosition)
    {
        if (cameraTransform == null)
        {
            return transform.forward;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            aimRayDistance,
            aimRayLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;

                if (hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                return (hit.point - spawnPosition).normalized;
            }
        }

        Vector3 targetPoint = cameraTransform.position + cameraTransform.forward * aimRayDistance;
        return (targetPoint - spawnPosition).normalized;
    }

    IEnumerator ShootProjectileAfterDelay()
    {
        yield return new WaitForSeconds(projectileLaunchDelay);
        ShootProjectile();
    }

    void ShootProjectile()
    {
        if (playerStats == null) return;

        Vector3 spawnPosition;

        if (projectileSpawnPoint != null)
        {
            spawnPosition = projectileSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + transform.forward * 1.2f + Vector3.up * 1.3f;
        }

        Vector3 shootDirection = GetAimShootDirection(spawnPosition);

        GameObject projectileObject;

        if (projectilePrefab != null)
        {
            projectileObject = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(shootDirection)
            );
        }
        else
        {
            projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Magic Projectile";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.LookRotation(shootDirection);
            projectileObject.transform.localScale = Vector3.one * 0.35f;
        }

        MagicProjectile projectile = projectileObject.GetComponent<MagicProjectile>();

        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<MagicProjectile>();
        }

        int projectileDamage = Mathf.RoundToInt(playerStats.GetDamage() * projectileDamageMultiplier);

        projectile.Init(
            shootDirection,
            playerStats,
            projectileDamage,
            projectileSpeed,
            projectileLifetime,
            xpPerHit,
            xpPerKill
        );
    }

    IEnumerator ShootExplosiveProjectileAfterDelay()
    {
        yield return new WaitForSeconds(explosiveLaunchDelay);
        ShootExplosiveProjectile();
    }

    void ShootExplosiveProjectile()
    {
        if (playerStats == null) return;

        Vector3 spawnPosition;

        if (explosiveProjectileSpawnPoint != null)
        {
            spawnPosition = explosiveProjectileSpawnPoint.position;
        }
        else if (projectileSpawnPoint != null)
        {
            spawnPosition = projectileSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + transform.forward * 1.5f + Vector3.up * 1.4f;
        }

        Vector3 shootDirection = GetAimShootDirection(spawnPosition);

        GameObject projectileObject;

        if (explosiveProjectilePrefab != null)
        {
            projectileObject = Instantiate(
                explosiveProjectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(shootDirection)
            );
        }
        else
        {
            projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Big Explosive Projectile";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.rotation = Quaternion.LookRotation(shootDirection);
            projectileObject.transform.localScale = Vector3.one * explosiveProjectileScale;

            Renderer renderer = projectileObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.15f, 0f, 1f);
                renderer.material = mat;
            }
        }

        ExplosiveProjectile projectile = projectileObject.GetComponent<ExplosiveProjectile>();

        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<ExplosiveProjectile>();
        }

        projectile.Init(
            shootDirection,
            playerStats,
            explosiveProjectileSpeed,
            explosiveProjectileLifetime,
            explosiveDirectDamage,
            explosiveExplosionDamage,
            explosiveRadius,
            xpPerHit,
            xpPerKill,
            explosiveHitLayers,
            explosionEffectPrefab
        );
    }

    void StartAttackHit(float damageMultiplier)
    {
        StartCoroutine(AttackHitAfterDelay(damageMultiplier));
    }

    IEnumerator AttackHitAfterDelay(float damageMultiplier)
    {
        yield return new WaitForSeconds(attackHitDelay);
        DealAttackDamage(damageMultiplier);
    }

    void DealAttackDamage(float damageMultiplier)
    {
        if (playerStats == null) return;

        Vector3 hitCenter;

        if (attackPoint != null)
        {
            hitCenter = attackPoint.position;
        }
        else
        {
            hitCenter = transform.position + transform.forward * attackDistance + Vector3.up * 1f;
        }

        Collider[] hits = Physics.OverlapSphere(
            hitCenter,
            attackRadius,
            attackLayer,
            QueryTriggerInteraction.Ignore
        );

        HashSet<PlayerStats> damagedTargets = new HashSet<PlayerStats>();

        foreach (Collider hit in hits)
        {
            PlayerStats targetStats = hit.GetComponentInParent<PlayerStats>();

            if (targetStats == null) continue;
            if (targetStats == playerStats) continue;
            if (damagedTargets.Contains(targetStats)) continue;
            if (targetStats.currentHealth <= 0) continue;

            damagedTargets.Add(targetStats);

            int damage = Mathf.RoundToInt(playerStats.GetDamage() * damageMultiplier);

            bool targetWasAlive = targetStats.currentHealth > 0;

            targetStats.TakeDamage(damage);

            playerStats.AddExperience(xpPerHit);

            if (targetWasAlive && targetStats.currentHealth <= 0)
            {
                playerStats.AddExperience(xpPerKill);
            }
        }
    }

    void HandleFootsteps()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool pressingWASD = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        bool grounded = controller != null && controller.isGrounded;

        bool canPlaySteps =
            pressingWASD &&
            grounded &&
            !isCrouching &&
            actionLockTimer <= 0f &&
            !isDefending &&
            !isDead &&
            !isProjectileAiming &&
            !isExplosiveAiming;

        if (canPlaySteps)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                if (footstepAudio != null && footstepAudio.clip != null)
                {
                    footstepAudio.PlayOneShot(footstepAudio.clip);
                }

                stepTimer = stepDelay;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void HandleMovement()
    {
        if (cameraTransform == null) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
            return;
        }

        if (battleMode)
        {
            HandleBattleMovement(horizontal, vertical, inputDirection);
        }
        else
        {
            HandleNormalMovement(inputDirection);
        }
    }

    void HandleNormalMovement(Vector3 inputDirection)
    {
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                + cameraTransform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection =
                Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            float bonusSpeed = playerStats != null ? playerStats.moveSpeedBonus : 0f;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isSprinting ? sprintSpeed + bonusSpeed : moveSpeed + bonusSpeed;

            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

            if (controller.isGrounded)
            {
                if (isSprinting)
                {
                    PlayAnimation(runForwardState);
                }
                else
                {
                    PlayAnimation(walkForwardState);
                }
            }
        }
        else
        {
            if (controller.isGrounded)
            {
                PlayAnimation(idleState);
            }
        }
    }

    void HandleBattleMovement(float horizontal, float vertical, Vector3 inputDirection)
    {
        float targetAngle = cameraTransform.eulerAngles.y;

        float angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection =
                cameraForward * vertical +
                cameraRight * horizontal;

            float bonusSpeed = playerStats != null ? playerStats.moveSpeedBonus : 0f;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isSprinting ? sprintSpeed + bonusSpeed : moveSpeed + bonusSpeed;

            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

            if (controller.isGrounded)
            {
                PlayBattleMoveAnimation(horizontal, vertical, isSprinting);
            }
        }
        else
        {
            if (controller.isGrounded)
            {
                PlayAnimation(idleState);
            }
        }
    }

    void PlayBattleMoveAnimation(float horizontal, float vertical, bool isSprinting)
    {
        if (vertical > 0.1f)
        {
            if (isSprinting)
            {
                PlayAnimation(battleRunForwardState);
            }
            else
            {
                PlayAnimation(battleWalkForwardState);
            }
        }
        else if (vertical < -0.1f)
        {
            PlayAnimation(battleWalkBackState);
        }
        else if (horizontal < -0.1f)
        {
            PlayAnimation(battleWalkLeftState);
        }
        else if (horizontal > 0.1f)
        {
            PlayAnimation(battleWalkRightState);
        }
    }

    void Jump()
    {
        float bonusJump = playerStats != null ? playerStats.jumpBonus : 0f;

        velocity.y = Mathf.Sqrt((jumpHeight + bonusJump) * -2f * gravity);

        jumpStartTimer = jumpStartTime;
        PlayAnimation(jumpStartState);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJumpAnimation()
    {
        bool grounded = controller.isGrounded;

        if (grounded && !wasGrounded)
        {
            if (
                actionLockTimer <= 0f &&
                !isDefending &&
                !isProjectileAiming &&
                !isExplosiveAiming
            )
            {
                PlayAction(jumpLandState, jumpLandLockTime);
            }
        }
        else if (!grounded)
        {
            if (
                actionLockTimer <= 0f &&
                !isDefending &&
                !isProjectileAiming &&
                !isExplosiveAiming
            )
            {
                if (jumpStartTimer > 0f)
                {
                    PlayAnimation(jumpStartState);
                }
                else if (velocity.y > 0.2f)
                {
                    PlayAnimation(jumpUpState);
                }
                else
                {
                    PlayAnimation(jumpAirState);
                }
            }
        }

        wasGrounded = grounded;
    }

    void PlayAction(string stateName, float lockTime)
    {
        actionLockTimer = lockTime;
        PlayAnimation(stateName);
    }

    void PlayAnimation(string stateName)
    {
        PlayAnimation(stateName, animationFade);
    }

    void PlayAnimation(string stateName, float fadeTime)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;
        if (currentAnimation == stateName) return;

        currentAnimation = stateName;

        string fullStateName = baseLayerPrefix + stateName;

        int fullHash = Animator.StringToHash(fullStateName);
        int simpleHash = Animator.StringToHash(stateName);

        if (animator.HasState(0, fullHash))
        {
            animator.CrossFadeInFixedTime(fullStateName, fadeTime, 0);
        }
        else if (animator.HasState(0, simpleHash))
        {
            animator.CrossFadeInFixedTime(stateName, fadeTime, 0);
        }
        else
        {
            Debug.LogWarning("ThirdPersonController: State не найден: " + stateName);
        }
    }

    void ShowScreenMessage(string message)
    {
        screenMessage = message;
        screenMessageTimer = messageDuration;
    }

    void OnGUI()
    {
        if (isProjectileAiming && showProjectileAim)
        {
            DrawCrosshair(Color.cyan);
        }

        if (isExplosiveAiming && showExplosiveAim)
        {
            DrawCrosshair(new Color(1f, 0.35f, 0f, 1f));
        }

        if (!string.IsNullOrEmpty(screenMessage))
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 28;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;

            Rect rect = new Rect(
                Screen.width / 2f - 350f,
                Screen.height - 170f,
                700f,
                70f
            );

            GUI.Label(rect, screenMessage, style);
        }
    }

    void DrawCrosshair(Color color)
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        GUI.color = color;

        GUI.DrawTexture(
            new Rect(
                centerX - crosshairGap - crosshairSize,
                centerY - crosshairThickness / 2f,
                crosshairSize,
                crosshairThickness
            ),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                centerX + crosshairGap,
                centerY - crosshairThickness / 2f,
                crosshairSize,
                crosshairThickness
            ),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                centerX - crosshairThickness / 2f,
                centerY - crosshairGap - crosshairSize,
                crosshairThickness,
                crosshairSize
            ),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                centerX - crosshairThickness / 2f,
                centerY + crosshairGap,
                crosshairThickness,
                crosshairSize
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;
    }

    public bool IsAiming()
    {
        return isProjectileAiming || isExplosiveAiming;
    }

    public void PlayIdle02()
    {
        PlayAction(idle02State, 1f);
    }

    public void PlayIdle03()
    {
        PlayAction(idle03State, 1f);
    }

    public void PlayDizzy()
    {
        PlayAction(dizzyState, 1.5f);
    }

    public void PlayVictory()
    {
        PlayAction(victoryStartState, 1.2f);
    }

    public void PlayVictoryMaintain()
    {
        PlayAnimation(victoryMaintainState);
    }

    public void PlayDieRecovery()
    {
        isDead = false;
        PlayAction(dieRecoveryState, 1.2f);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 hitCenter;

        if (attackPoint != null)
        {
            hitCenter = attackPoint.position;
        }
        else
        {
            hitCenter = transform.position + transform.forward * attackDistance + Vector3.up * 1f;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackRadius);

        Vector3 projectilePoint;

        if (projectileSpawnPoint != null)
        {
            projectilePoint = projectileSpawnPoint.position;
        }
        else
        {
            projectilePoint = transform.position + transform.forward * 1.2f + Vector3.up * 1.3f;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(projectilePoint, 0.25f);

        Vector3 explosivePoint;

        if (explosiveProjectileSpawnPoint != null)
        {
            explosivePoint = explosiveProjectileSpawnPoint.position;
        }
        else if (projectileSpawnPoint != null)
        {
            explosivePoint = projectileSpawnPoint.position;
        }
        else
        {
            explosivePoint = transform.position + transform.forward * 1.5f + Vector3.up * 1.4f;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(explosivePoint, 0.45f);
    }
}