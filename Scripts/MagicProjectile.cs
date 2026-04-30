using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 12f;
    public float lifeTime = 3f;
    public int damage = 20;
    public float hitRadius = 0.6f;

    [Header("XP")]
    public int xpPerHit = 10;
    public int xpPerKill = 50;

    [Header("Hit Settings")]
    public LayerMask hitLayers = ~0;
    public bool debugHits = true;

    private Vector3 direction;
    private PlayerStats ownerStats;
    private Transform ownerRoot;

    private bool initialized;
    private Vector3 previousPosition;

    public void Init(
        Vector3 shootDirection,
        PlayerStats owner,
        int projectileDamage,
        float projectileSpeed,
        float projectileLifeTime,
        int hitXP,
        int killXP
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

        damage = projectileDamage;
        speed = projectileSpeed;
        lifeTime = Mathf.Max(0.5f, projectileLifeTime);

        xpPerHit = hitXP;
        xpPerKill = killXP;

        initialized = true;
        previousPosition = transform.position;

        Destroy(gameObject, lifeTime);
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

        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + direction * speed * Time.deltaTime;

        if (CheckHitAtPosition(currentPosition)) return;
        if (CheckHitBetween(currentPosition, nextPosition)) return;
        if (CheckHitAtPosition(nextPosition)) return;

        transform.position = nextPosition;
        previousPosition = nextPosition;
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
            if (TryDamageCollider(col))
            {
                return true;
            }
        }

        return false;
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

            if (TryDamageCollider(hit.collider))
            {
                return true;
            }
        }

        return false;
    }

    bool TryDamageCollider(Collider col)
    {
        if (col == null) return false;

        if (IsOwnerCollider(col))
        {
            return false;
        }

        PlayerStats targetStats = col.GetComponentInParent<PlayerStats>();

        if (targetStats == null)
        {
            targetStats = col.transform.root.GetComponent<PlayerStats>();
        }

        if (targetStats == null)
        {
            if (debugHits)
            {
                Debug.Log("Projectile touched object without PlayerStats: " + col.name);
            }

            return false;
        }

        if (targetStats == ownerStats)
        {
            return false;
        }

        if (targetStats.currentHealth <= 0)
        {
            return false;
        }

        DamageTarget(targetStats);
        return true;
    }

    bool IsOwnerCollider(Collider col)
    {
        if (ownerRoot == null) return false;

        return col.transform.root == ownerRoot;
    }

    void DamageTarget(PlayerStats targetStats)
    {
        bool targetWasAlive = targetStats.currentHealth > 0;

        targetStats.TakeDamage(damage);

        if (ownerStats != null)
        {
            ownerStats.AddExperience(xpPerHit);
        }

        Debug.Log("Projectile hit enemy: " + targetStats.name + ". Damage: " + damage);

        if (ownerStats != null && targetWasAlive && targetStats.currentHealth <= 0)
        {
            ownerStats.AddExperience(xpPerKill);
            Debug.Log("Projectile killed enemy. XP +" + xpPerKill);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}