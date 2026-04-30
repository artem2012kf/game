using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 9f;
    public float lifeTime = 3f;
    public float hitRadius = 0.7f;

    [Header("Explosion Effect")]
    public GameObject explosionEffectPrefab;
    public float explosionEffectDestroyTime = 3f;

    [Header("Damage")]
    public int directDamage = 15;
    public int explosionDamage = 35;
    public float explosionRadius = 3f;

    [Header("Player Damage")]
    public bool damageOwnerInExplosion = true;
    public float ownerDamageMultiplier = 1f;

    [Header("Hit Settings")]
    public LayerMask hitLayers = ~0;
    public bool explodeOnAnyHit = true;
    public bool explodeOnLifeTimeEnd = true;
    public float ignoreOwnerCollisionTime = 0.2f;

    [Header("XP")]
    public int xpPerHit = 10;
    public int xpPerKill = 50;

    [Header("Debug")]
    public bool debugExplosion = true;

    private Vector3 direction;
    private PlayerStats ownerStats;
    private Transform ownerRoot;

    private bool initialized;
    private bool exploded;
    private float spawnTime;

    public void Init(
        Vector3 shootDirection,
        PlayerStats owner,
        float projectileSpeed,
        float projectileLifeTime,
        int projectileDirectDamage,
        int projectileExplosionDamage,
        float projectileExplosionRadius,
        int hitXP,
        int killXP,
        LayerMask layers,
        GameObject explosionPrefab
    )
    {
        if (shootDirection.sqrMagnitude < 0.01f)
        {
            shootDirection = transform.forward;
        }

        direction = shootDirection.normalized;
        ownerStats = owner;

        if (ownerStats != null)
        {
            ownerRoot = ownerStats.transform.root;
        }

        speed = projectileSpeed;
        lifeTime = Mathf.Max(0.5f, projectileLifeTime);

        directDamage = projectileDirectDamage;
        explosionDamage = projectileExplosionDamage;
        explosionRadius = projectileExplosionRadius;

        xpPerHit = hitXP;
        xpPerKill = killXP;
        hitLayers = layers;

        explosionEffectPrefab = explosionPrefab;

        initialized = true;
        exploded = false;
        spawnTime = Time.time;
    }

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        SphereCollider sphere = GetComponent<SphereCollider>();

        if (sphere == null)
        {
            sphere = gameObject.AddComponent<SphereCollider>();
        }

        sphere.radius = hitRadius;
        sphere.isTrigger = true;
    }

    void Update()
    {
        if (!initialized) return;
        if (exploded) return;

        if (Time.time - spawnTime >= lifeTime)
        {
            if (explodeOnLifeTimeEnd)
            {
                Explode(transform.position, null);
            }
            else
            {
                Destroy(gameObject);
            }

            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + direction * speed * Time.deltaTime;

        if (CheckHitBetween(currentPosition, nextPosition))
        {
            return;
        }

        if (CheckHitAtPosition(nextPosition))
        {
            return;
        }

        transform.position = nextPosition;
    }

    bool CheckHitBetween(Vector3 from, Vector3 to)
    {
        Vector3 move = to - from;
        float distance = move.magnitude;

        if (distance <= 0.001f) return false;

        RaycastHit[] hits = Physics.SphereCastAll(
            from,
            hitRadius,
            move.normalized,
            distance,
            hitLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;

            if (ShouldIgnoreCollider(hit.collider))
            {
                continue;
            }

            PlayerStats targetStats = GetPlayerStatsFromCollider(hit.collider);

            if (targetStats != null && targetStats != ownerStats)
            {
                Explode(hit.point, targetStats);
                return true;
            }

            if (explodeOnAnyHit)
            {
                Explode(hit.point, null);
                return true;
            }
        }

        return false;
    }

    bool CheckHitAtPosition(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(
            position,
            hitRadius,
            hitLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider col in hits)
        {
            if (col == null) continue;

            if (ShouldIgnoreCollider(col))
            {
                continue;
            }

            PlayerStats targetStats = GetPlayerStatsFromCollider(col);

            if (targetStats != null && targetStats != ownerStats)
            {
                Explode(position, targetStats);
                return true;
            }

            if (explodeOnAnyHit)
            {
                Explode(position, null);
                return true;
            }
        }

        return false;
    }

    bool ShouldIgnoreCollider(Collider col)
    {
        if (col == null) return true;

        // „тобы пул€ не взрывалась сразу внутри игрока после выстрела
        if (Time.time - spawnTime < ignoreOwnerCollisionTime && IsOwnerCollider(col))
        {
            return true;
        }

        // ¬ладелец не должен быть причиной столкновени€ пули
        if (IsOwnerCollider(col))
        {
            return true;
        }

        // »гнорируем собственные collider'ы пули
        if (col.transform == transform || col.transform.IsChildOf(transform))
        {
            return true;
        }

        return false;
    }

    void Explode(Vector3 explosionPosition, PlayerStats directTarget)
    {
        if (exploded) return;

        exploded = true;

        SpawnExplosionEffect(explosionPosition);

        HashSet<PlayerStats> damagedTargets = new HashSet<PlayerStats>();

        Collider[] hits = Physics.OverlapSphere(
            explosionPosition,
            explosionRadius,
            hitLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider col in hits)
        {
            if (col == null) continue;

            PlayerStats targetStats = GetPlayerStatsFromCollider(col);

            if (targetStats == null) continue;
            if (targetStats.currentHealth <= 0) continue;

            bool isOwner = targetStats == ownerStats;

            if (isOwner && !damageOwnerInExplosion)
            {
                continue;
            }

            if (damagedTargets.Contains(targetStats))
            {
                continue;
            }

            damagedTargets.Add(targetStats);

            int finalDamage = explosionDamage;

            if (targetStats == directTarget)
            {
                finalDamage += directDamage;
            }

            if (isOwner)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * ownerDamageMultiplier);
            }

            DamageTarget(targetStats, finalDamage);
        }

        if (debugExplosion)
        {
            Debug.Log("Explosion appeared. Damaged targets: " + damagedTargets.Count);
        }

        Destroy(gameObject);
    }

    void SpawnExplosionEffect(Vector3 position)
    {
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionEffectPrefab,
                position,
                Quaternion.identity
            );

            Destroy(explosion, explosionEffectDestroyTime);
        }
        else
        {
            CreateDefaultExplosionVisual(position);
        }
    }

    void CreateDefaultExplosionVisual(Vector3 position)
    {
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.name = "Default Explosion Visual";
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * explosionRadius * 2f;

        Collider col = explosion.GetComponent<Collider>();

        if (col != null)
        {
            Destroy(col);
        }

        Renderer renderer = explosion.GetComponent<Renderer>();

        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.35f, 0f, 0.45f);
            renderer.material = mat;
        }

        Destroy(explosion, 0.25f);
    }

    PlayerStats GetPlayerStatsFromCollider(Collider col)
    {
        if (col == null) return null;

        PlayerStats stats = col.GetComponentInParent<PlayerStats>();

        if (stats != null)
        {
            return stats;
        }

        stats = col.GetComponent<PlayerStats>();

        if (stats != null)
        {
            return stats;
        }

        stats = col.transform.root.GetComponent<PlayerStats>();

        return stats;
    }

    bool IsOwnerCollider(Collider col)
    {
        if (ownerRoot == null) return false;

        return col.transform.root == ownerRoot;
    }

    void DamageTarget(PlayerStats targetStats, int damage)
    {
        if (targetStats == null) return;
        if (targetStats.currentHealth <= 0) return;

        bool targetWasAlive = targetStats.currentHealth > 0;
        bool targetIsOwner = targetStats == ownerStats;

        targetStats.TakeDamage(damage);

        if (ownerStats != null && !targetIsOwner)
        {
            ownerStats.AddExperience(xpPerHit);
        }

        Debug.Log("Explosion damaged: " + targetStats.name + ". Damage: " + damage);

        if (
            ownerStats != null &&
            !targetIsOwner &&
            targetWasAlive &&
            targetStats.currentHealth <= 0
        )
        {
            ownerStats.AddExperience(xpPerKill);
            Debug.Log("Explosion killed enemy. XP +" + xpPerKill);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}