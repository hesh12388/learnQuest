using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class Creature : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int damage = 1;
    
    [Header("Movement")]
    [SerializeField] private float attackDistance = 1.2f;
    [SerializeField] private float retreatDistance = 3f;
    [SerializeField] private float retreatDuration = 1f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Path Validation")]
    [SerializeField] private float pathCheckDelay = 3f;
    [SerializeField] private float maxStuckTime = 10f;
    [SerializeField] private float minMovementThreshold = 0.1f;
    
    // State tracking
    private int currentHealth;
    private bool isRetreating = false;
    private bool canAttack = true;
    private float attackTimer = 0f;
    public float invulnerabilityTime = 1.0f;
    private bool isInvulnerable = false;

    [Header("Bounce Animation")]
    [SerializeField] private float bounceInterval = 3f; // Time between bounces
    [SerializeField] private float bounceDuration = 1f; // How long the bounce takes
    [SerializeField] private float bounceHeight = 0.5f; // How high the creature bounces
    [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Animation curve for bounce
    
    
    // Bounce state
    private bool isBouncing = false;
    private float nextBounceTime;
    private Vector3 originalPosition;
    private Transform spriteTransform; // We'll use this to bounce just the sprite, not the collider

    // Path validation
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float pathInvalidTimer = 0f;

    // Components
    private Animator animator;
    private NavMeshAgent agent;
    
    public GameObject messageBubble;
    // Events
    public event Action<Creature> OnCreatureDefeated;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        // Find or create sprite transform
        spriteTransform = transform.Find("Sprite");
        if (spriteTransform == null)
        {
            // If there's no separate sprite child, use the main transform
            // or create a child sprite container if needed
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                spriteTransform = transform; // Use main transform if it has the sprite renderer
            }
            else
            {
                // Find first child with sprite renderer
                sr = GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    spriteTransform = sr.transform;
                }
                else
                {
                    // Fallback to main transform
                    spriteTransform = transform;
                }
            }
        }
        
        // Configure NavMeshAgent for 2D
        if (agent != null)
        {
            // Critical for 2D movement - constrain to XY plane
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            
            // For 2D, we need to use world coordinates directly
            agent.transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                0
            );
        }
        
        currentHealth = maxHealth;
        lastPosition = transform.position;
        StartCoroutine(ValidatePathRoutine());
        
        // Set first bounce time
        nextBounceTime = Time.time + UnityEngine.Random.Range(1f, bounceInterval);
    }
    
    private void Start()
    {
        // Make sure we're on the NavMesh
        if (agent != null && !agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
        
        // Store original position for bouncing
        originalPosition = spriteTransform.localPosition;
    }
    
    private IEnumerator ValidatePathRoutine()
    {
        while (true)
        {
            // Wait for next check
            yield return new WaitForSeconds(pathCheckDelay);
            
            // Skip if player is not available or agent is not active
            if (Player.Instance == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
                continue;
                
            // Skip validation and reset timers if player is in menu/tutorial/paused
            if (Player.Instance.isPaused || Player.Instance.stop_interaction || UIManager.Instance.isMenuOpen())
            {
                // Reset timers since we're legitimately waiting
                stuckTimer = 0f;
                pathInvalidTimer = 0f;
                continue;
            }
        
            // In Creature.cs
            // Set destination to player position
            agent.SetDestination(Player.Instance.transform.position);
            
            // Check path status
            if (agent.pathStatus == NavMeshPathStatus.PathPartial || 
                agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                pathInvalidTimer += pathCheckDelay;
                
                // If path has been invalid for too long, destroy this creature
                if (pathInvalidTimer >= maxStuckTime)
                {
                    Debug.Log($"Creature has invalid path for too long - destroying: {gameObject.name}");
                    OnCreatureDefeated?.Invoke(this);
                    Destroy(gameObject);
                    yield break;
                }
            }
            else
            {
                // Reset timer if path becomes valid again
                pathInvalidTimer = 0f;
            }
            
            // Check if creature is stuck (not moving)
            float movedDistance = Vector3.Distance(transform.position, lastPosition);
            if (movedDistance < minMovementThreshold && !isRetreating)
            {
                stuckTimer += pathCheckDelay;
                
                // If stuck for too long, destroy this creature
                if (stuckTimer >= maxStuckTime)
                {
                    Debug.Log($"Creature stuck for too long - destroying: {gameObject.name}");
                    OnCreatureDefeated?.Invoke(this);
                    Destroy(gameObject);
                    yield break;
                }
            }
            else
            {
                // Reset timer if creature moves
                stuckTimer = 0f;
            }
            
            // Update last position
            lastPosition = transform.position;
        }
    }


    private void Update()
    {

         if (agent == null || !agent.isActiveAndEnabled)
        {
            Debug.LogWarning($"Cannot set destination: Agent is null or disabled for {gameObject.name}");
            return;
        }
        
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"Cannot set destination: Agent is not on NavMesh for {gameObject.name}");
            return;
        }

        if (Player.Instance == null || agent == null)
            return;
            
        // Skip AI if player is paused or UI is open
        if (Player.Instance.isPaused || Player.Instance.stop_interaction || UIManager.Instance.isMenuOpen())
        {
            StopMovement();
            return;
        }

        // Check if it's time to bounce
        if (!isBouncing && !isRetreating && Time.time >= nextBounceTime)
        {
            StartCoroutine(PerformBounce());
            return;
        }

        // Skip normal movement if bouncing
        if (isBouncing)
            return;

        agent.isStopped = false;
        float distanceToPlayer = Vector2.Distance(transform.position, Player.Instance.transform.position);
        
        // Handle attack cooldown
        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                canAttack = true;
            }
        }
        
        // Handle retreat state
        if (isRetreating)
        {
            // Continue the retreat movement
            return;
        }
        
        if (Player.Instance != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(Player.Instance.transform.position);
            
            // If close enough to attack
            if (distanceToPlayer <= attackDistance && canAttack)
            {
                Attack();
            }
        }
        

        // Update animator
        UpdateAnimator();
       
    }

    // method for bounce animation
    private IEnumerator PerformBounce()
    {
        isBouncing = true;
        
        // Stop movement
        if (agent != null)
        {
            agent.isStopped = true;
        }
        
        // Set animation to idle
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("moveX", 0f);
            animator.SetFloat("moveY", 0f);
        }
        
        // Get sprite renderer or transform to animate
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Transform targetTransform = spriteRenderer != null ? spriteRenderer.transform : transform;
            
        // Store original position
        Vector3 startPos = targetTransform.position;
        
        // Multiple bounces with decreasing height
        int numberOfBounces = 5;
        float currentBounceHeight = bounceHeight;
        messageBubble.SetActive(true);
        for (int i = 0; i < numberOfBounces; i++)
        {
            // Calculate the duration for this bounce (make subsequent bounces faster)
            float currentBounceDuration = bounceDuration * (1f - (i * 0.2f));
            
            // Move up (first half of bounce)
            float moveUpTime = currentBounceDuration * 0.5f;
            float elapsedTime = 0f;
            
            while (elapsedTime < moveUpTime)
            {
                float ratio = elapsedTime / moveUpTime;
                float yOffset = currentBounceHeight * ratio;
                
                targetTransform.position = new Vector3(
                    startPos.x,
                    startPos.y + yOffset,
                    startPos.z
                );
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Move down (second half of bounce)
            elapsedTime = 0f;
            float moveDownTime = currentBounceDuration * 0.5f;
            Vector3 topPosition = targetTransform.position;
            
            while (elapsedTime < moveDownTime)
            {
                float ratio = elapsedTime / moveDownTime;
                
                targetTransform.position = Vector3.Lerp(
                    topPosition, 
                    startPos, 
                    ratio
                );
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure we're back at the start position
            targetTransform.position = startPos;
            
            // Reduce bounce height for next bounce (makes it look more natural)
            currentBounceHeight *= 0.5f;
            
            // Small pause between bounces
            if (i < numberOfBounces - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Resume movement
        if (agent != null)
        {
            agent.isStopped = false;
        }
        
        // Schedule next bounce
        nextBounceTime = Time.time + bounceInterval;
        
        isBouncing = false;
        messageBubble.SetActive(false);
    }
    
    private void UpdateAnimator()
    {
       
       if(agent == null)
            return;

        // Get the movement direction
        Vector2 movementDir = agent.velocity.normalized;
        
        animator.SetFloat("moveX", movementDir.x);
        animator.SetFloat("moveY", movementDir.y);
        animator.SetBool("isMoving", true);
       
    }
    
    private void Attack()
    {
        // Set attack cooldown
        canAttack = false;
        attackTimer = attackCooldown;
        
        // damage player
        Player.Instance.TakeDamage(damage);
        
        // Start retreating
        StartCoroutine(RetreatAfterAttack());
    }
    
    private IEnumerator RetreatAfterAttack()
    {

        // Safety check
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning($"Cannot retreat: Agent is invalid for {gameObject.name}");
            isRetreating = false;
            yield break;
        }
    
        isRetreating = true;
        
        // Get direction away from player
        Vector3 retreatDirection = transform.position - Player.Instance.transform.position;
        retreatDirection.Normalize();
        
        // Set destination to retreat position
        Vector3 retreatPosition = transform.position + retreatDirection * retreatDistance;
        
        // Sample a valid NavMesh position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPosition, out hit, retreatDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        
        // Wait for the retreat duration
        yield return new WaitForSeconds(retreatDuration);
        
        isRetreating = false;
    }
    
   public void TakeDamage(int damageAmount)
    {
        // Check if player is invulnerable
        if (isInvulnerable)
            return;

        if (currentHealth <= 0)
            return;
            
        currentHealth -= damageAmount;
        
        // Visual feedback
        StartCoroutine(FlashOnHit());
        
        // Retreat after being hit
        StartCoroutine(RetreatAfterAttack());
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashOnHit()
    {
        isInvulnerable = true;
         AudioController.Instance.PlayHit();
        // Optional: Visual feedback for invulnerability
        float endTime = Time.time + invulnerabilityTime;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Flash the player sprite
        while (Time.time < endTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        // Ensure sprite is visible when done
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        isInvulnerable = false;
    }
    
    private void Die()
    {
        // Notify the CreatureManager
        OnCreatureDefeated?.Invoke(this);
        
        // Destroy this game object
        Destroy(gameObject);
    }
    
    private void StopMovement()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
        animator.SetBool("isMoving", false);
    }
}