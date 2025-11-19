using System.Collections;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;

public class FloatingHeadBoss : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 3f;
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 1f;
    public float minDistanceFromPlayer = 5f;
    public float maxDistanceFromPlayer = 10f;

    [Header("Enhanced Movement")]
    public MovementType movementType = MovementType.PatternBased;
    public float patrolSpeed = 4f;
    public float chaseSpeed = 6f;
    public float strafeSpeed = 3f;
    public float movementChangeInterval = 3f;
    public float aggressiveMovementRadius = 8f;

    [Header("Waypoint System")]
    public Transform[] waypoints;
    public float waypointThreshold = 1f;

    [Header("Strafe Settings")]
    public float strafeDistance = 4f;
    public float strafeDuration = 2f;
    public bool canStrafe = true;

    private Vector3 currentMovementTarget;
    private float movementChangeTimer;
    private int currentWaypointIndex = 0;
    private bool isStrafing = false;
    private float strafeTimer = 0f;
    private float strafeDirection = 1f;
    public enum MovementType
    {
        PatternBased,
        WaypointBased,
        ChaseAndStrafe,
        Circular
    }

    [Header("Combat Settings")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed = 8f;
    public float attackCooldown = 2f;
    public int fireballsPerVolley = 3;
    public float volleySpreadAngle = 15f;

    [Header("Phase Settings")]
    public int maxHealth = 30;
    public float phase2HealthThreshold = 0.5f; // 50%
    public float phase3HealthThreshold = 0.2f; // 20%

    private int currentHealth;
    private bool phase2Activated = false;
    private bool phase3Activated = false;

    [Header("Animation")]
    public Animator headAnimator;

    private Transform player;
    private Vector3 startPosition;
    private float lastAttackTime;
    private bool isAttacking = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;

        if (headAnimator == null)
            headAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null) return;

        
        HandleMovement();
        HoverAnimation();

        // Check if it's time to attack
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(AttackSequence());
        }

    }


    #region Movement Types
    private void HandleMovement()
    {
        movementChangeTimer += Time.deltaTime;

        switch (movementType)
        {
            case MovementType.PatternBased:
                PatternBasedMovement();
                break;
            case MovementType.WaypointBased:
                WaypointBasedMovement();
                break;
            case MovementType.ChaseAndStrafe:
                ChaseAndStrafeMovement();
                break;
            case MovementType.Circular:
                CircularMovement();
                break;
        }
    }

    private void CircularMovement()
    {
        movementChangeTimer += Time.deltaTime;

        // Change circle parameters periodically
        if (movementChangeTimer >= movementChangeInterval)
        {
            movementChangeTimer = 0f;
            currentCircleRadius = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            currentCircleHeight = Random.Range(2f, 6f);
            circleSpeed = Random.Range(0.5f, 2f);
        }

        // Calculate circular position around player
        float angle = Time.time * circleSpeed;
        Vector3 circlePosition = new Vector3(
            Mathf.Cos(angle) * currentCircleRadius,
            currentCircleHeight,
            Mathf.Sin(angle) * currentCircleRadius
        );

        MoveToPosition(player.position + circlePosition, patrolSpeed);
    }

    private float currentCircleRadius = 5f;
    private float currentCircleHeight = 3f;
    private float circleSpeed = 1f;

    private void ChaseAndStrafeMovement()
    {
        if (isStrafing)
        {
            StrafeMovement();
        }
        else
        {
            ChaseMovement();
        }

        // Switch between chasing and strafing
        if (movementChangeTimer >= movementChangeInterval)
        {
            movementChangeTimer = 0f;
            isStrafing = !isStrafing;

            if (isStrafing)
            {
                // Choose strafe direction (left or right relative to player)
                strafeDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
                strafeTimer = 0f;
            }
        }
    }

    private void ChaseMovement()
    {
        float currentDistance = Vector3.Distance(transform.position, player.position);

        if (currentDistance > maxDistanceFromPlayer)
        {
            // Move towards player
            MoveToPosition(player.position, chaseSpeed);
        }
        else if (currentDistance < minDistanceFromPlayer)
        {
            // Move away from player
            Vector3 directionAway = (transform.position - player.position).normalized;
            Vector3 targetPosition = transform.position + directionAway * 2f;
            MoveToPosition(targetPosition, chaseSpeed);
        }
        else
        {
            // Move to a position near player but not too close
            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(1f, 3f),
                Random.Range(-2f, 2f)
            );
            MoveToPosition(player.position + randomOffset, patrolSpeed);
        }
    }

    private void StrafeMovement()
    {
        strafeTimer += Time.deltaTime;

        if (strafeTimer >= strafeDuration)
        {
            isStrafing = false;
            return;
        }

        // Calculate strafe direction (perpendicular to player direction)
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 strafeDirectionVector = Vector3.Cross(toPlayer, Vector3.up).normalized * strafeDirection;

        // Move in strafe direction
        transform.position += strafeDirectionVector * strafeSpeed * Time.deltaTime;

        // Also maintain some distance from player
        Vector3 idealPosition = player.position + toPlayer * aggressiveMovementRadius;
        transform.position = Vector3.MoveTowards(transform.position, idealPosition, strafeSpeed * 0.5f * Time.deltaTime);
    }
    private void WaypointBasedMovement()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            // Fallback to pattern-based if no waypoints
            PatternBasedMovement();
            return;
        }

        // Check if reached current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) <= waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        MoveToPosition(waypoints[currentWaypointIndex].position, patrolSpeed);
    }

    private void PatternBasedMovement()
    {
        if (movementChangeTimer >= movementChangeInterval)
        {
            movementChangeTimer = 0f;

            // Randomly choose a movement pattern
            int pattern = Random.Range(0, 4);

            switch (pattern)
            {
                case 0: // Circle around player
                    currentMovementTarget = GetCirclePosition();
                    break;
                case 1: // Move to player's side
                    currentMovementTarget = GetSidePosition();
                    break;
                case 2: // Move above player
                    currentMovementTarget = GetAbovePosition();
                    break;
                case 3: // Random position near player
                    currentMovementTarget = GetRandomNearbyPosition();
                    break;
            }
        }

        // Move towards current target
        MoveToPosition(currentMovementTarget, patrolSpeed);
    }

    private Vector3 GetCirclePosition()
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
            Random.Range(2f, 5f), // Random height
            Mathf.Sin(angle * Mathf.Deg2Rad) * distance
        );
        return player.position + offset;
    }

    private Vector3 GetSidePosition()
    {
        float side = Random.Range(0, 2) == 0 ? -1f : 1f;
        return player.position + new Vector3(side * aggressiveMovementRadius, 3f, 0f);
    }

    private Vector3 GetAbovePosition()
    {
        return player.position + new Vector3(0f, 5f, 0f);
    }

    private Vector3 GetRandomNearbyPosition()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-aggressiveMovementRadius, aggressiveMovementRadius),
            Random.Range(2f, 6f),
            Random.Range(-aggressiveMovementRadius, aggressiveMovementRadius)
        );
        return player.position + randomOffset;
    }

    void MoveToPosition(Vector3 targetPosition, float moveSpeed)
    {
        // Smooth movement towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Ensure minimum height
        Vector3 position = transform.position;
        position.y = Mathf.Max(position.y, 1f);
        transform.position = position;
    }

    private void MoveTowardsPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float currentDistance = Vector3.Distance(transform.position, player.position);

        // Maintain optimal distance from player
        if (currentDistance > maxDistanceFromPlayer)
        {
            // Move closer to player
            transform.position += directionToPlayer * movementSpeed * Time.deltaTime;
        }
        else if (currentDistance < minDistanceFromPlayer)
        {
            // Move away from player
            transform.position -= directionToPlayer * movementSpeed * Time.deltaTime;
        }

        // Keep boss at a certain height (adjust based on your scene)
        Vector3 position = transform.position;
        position.y = Mathf.Max(position.y, 2f);
        transform.position = position;
    }

    private void HoverAnimation()
    {
        // Simple hover effect using sine wave
        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(
            transform.position.x,
            startPosition.y + hoverOffset,
            transform.position.z
        );
    }
    #endregion

    private IEnumerator AttackSequence()
    {
        isAttacking = true;

        // Trigger attack animation
        if (headAnimator != null)
            headAnimator.SetTrigger("Attack");

        // Brief pause before shooting
        yield return new WaitForSeconds(0.5f);

        // Shoot volley of fireballs
        ShootFireballVolley();

        // Reset attack timer
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private void ShootFireballVolley()
    {
        for (int i = 0; i < fireballsPerVolley; i++)
        {
            // Calculate spread angle for this fireball
            float spreadAngle = 0f;
            if (fireballsPerVolley > 1)
            {
                float angleStep = volleySpreadAngle / (fireballsPerVolley - 1);
                spreadAngle = -volleySpreadAngle / 2f + angleStep * i;
            }

            StartCoroutine(ShootSingleFireball(spreadAngle, i * 0.1f));
        }
    }

    private IEnumerator ShootSingleFireball(float spreadAngle, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (fireballPrefab == null || fireballSpawnPoint == null)
        {
            Debug.LogWarning("Fireball prefab or spawn point not assigned!");
            yield break;
        }

        // Create fireball
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);

        // Apply spread angle
        Vector3 fireballDirection = Quaternion.Euler(0, spreadAngle, 0) * fireballSpawnPoint.forward;

        // Set up fireball
        //Fireball fireballScript = fireball.GetComponent<Fireball>();
        //if (fireballScript != null)
        //{
        //    fireballScript.Initialize(fireballDirection, fireballSpeed, player);
        //}
        //else
        //{
        //    // Fallback: use rigidbody
        //    Rigidbody rb = fireball.GetComponent<Rigidbody>();
        //    if (rb != null)
        //    {
        //        rb.velocity = fireballDirection * fireballSpeed;
        //    }
        //}
    }

    // Public method to be called when boss takes damage
    public void TakeDamage()
    {
        // Check for phase transitions
        float currentHealth = GetComponent<Life>().hp;
        float healthPercent = (float)currentHealth / maxHealth;

        if (!phase2Activated && healthPercent <= phase2HealthThreshold)
        {
            ActivatePhase2();
        }
        else if (!phase3Activated && healthPercent <= phase3HealthThreshold)
        {
            ActivatePhase3();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ActivatePhase2()
    {
        phase2Activated = true;
        attackCooldown *= 0.7f;
        fireballsPerVolley += 1;

        // Enhanced movement for phase 2
        movementChangeInterval *= 0.7f; 
        chaseSpeed *= 1.3f; 
        movementType = MovementType.ChaseAndStrafe; 
    }

    private void ActivatePhase3()
    {
        phase3Activated = true;
        attackCooldown *= 0.5f;
        movementSpeed *= 1.5f;
        fireballsPerVolley += 2;

        // Chaotic movement for phase 3
        movementChangeInterval *= 0.5f;
        chaseSpeed *= 1.5f;
        patrolSpeed *= 1.3f;

        // Combine movement types or create more chaotic patterns
        StartCoroutine(ChaoticPhase3Movement());
    }

    private IEnumerator ChaoticPhase3Movement()
    {
        while (phase3Activated)
        {
            // Randomly switch between movement types for chaos
            movementType = (MovementType)Random.Range(0, 4);
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    private void Die()
    {
        // Death logic here
        if (headAnimator != null) { headAnimator.SetTrigger("Die"); }

        // Disable attacking and movement
        enabled = false;

        // Play death effects, etc.
        Destroy(gameObject, 3f);
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);

        // Draw fireball direction
        if (fireballSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(fireballSpawnPoint.position, fireballSpawnPoint.forward * 3f);
        }
    }
}