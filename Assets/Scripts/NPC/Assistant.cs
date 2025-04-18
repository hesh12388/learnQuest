using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Makes an NPC follow the player using the NavMesh system
/// Attach this to an NPC GameObject that has a NavMeshAgent component
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Assistant : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("How close the NPC should get to the player before stopping")]
    public float stoppingDistance = 7.0f;
    
    [Tooltip("How often to update the destination (seconds)")]
    public float updateFrequency = 0.2f;
    
    [Tooltip("Maximum distance the player can be before the NPC teleports to them")]
    public float maxFollowDistance = 200.0f;
    
    [Header("Movement Visuals")]
    [Tooltip("Reference to the animator")]
    public Animator animator;
    
    // Private variables
    private NavMeshAgent agent;
    private Transform playerTransform;
    private float updateTimer;
    private Vector3 lastPlayerPosition;
    private bool isFollowing = true;
    
    // Optional reference to the NPC component for interaction logic
    private NPC npcComponent;
    
    void Awake()
    {
        // Get required components
        agent = GetComponent<NavMeshAgent>();
        
        // If animator wasn't set in inspector, try to get it from this object
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Try to get NPC component (optional)
        npcComponent = GetComponent<NPC>();
    }
    
    void Start()
    {
        // Configure NavMeshAgent for 2D movement
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.stoppingDistance = stoppingDistance;
        }
        
        // Find the player
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
            lastPlayerPosition = playerTransform.position;
        }
        else
        {
            Debug.LogWarning("Player Instance not found. The follower won't work until Player is available.");
        }
    }
    
    void Update()
    {
        // If the player reference is missing, try to get it
        if (playerTransform == null && Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
            lastPlayerPosition = playerTransform.position;
        }
        
        // If we still don't have player or agent, can't do anything
        if (playerTransform == null || agent == null)
            return;
        
        // Don't follow during instruction or evaluation
        if (npcComponent != null && npcComponent.isInstructing)
        {
            StopFollowing();
            return;
        }
        
        // Check if player is paused
        if (Player.Instance.isPaused || Player.Instance.stop_interaction)
        {
            StopFollowing();
            return;
        }
        
        // If we were stopped, resume following
        if (!isFollowing)
        {
            ResumeFollowing();
        }
        
        // Update timer for path recalculation
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateFrequency;
            UpdateDestination();
        }
        
        // Update animator based on current movement
        UpdateAnimator();
        
        // Check for teleportation (if player gets too far)
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > maxFollowDistance)
        {
            TeleportNearPlayer();
        }
    }
    
    void UpdateDestination()
    {
        if (playerTransform == null || agent == null)
            return;
            
        // Only update destination if player has moved
        if (Vector3.Distance(lastPlayerPosition, playerTransform.position) > 0.1f)
        {
            // Set the destination to the player's position
            Vector3 targetPosition = playerTransform.position;
            
            // Ensure we're on the NavMesh
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(targetPosition);
                lastPlayerPosition = playerTransform.position;
            }
        }
    }
    
    void UpdateAnimator()
    {
        if (animator == null || agent == null)
            return;
            
        // Get the movement direction
        Vector2 movement = agent.velocity.normalized;
        bool isMoving = agent.velocity.magnitude > 0.1f;
        
        // Update animator parameters
        animator.SetBool("isMoving", isMoving);
        
        if (isMoving)
        {
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
        }
    }
    
    void StopFollowing()
    {
        if (agent != null && isFollowing)
        {
            agent.isStopped = true;
            isFollowing = false;
            
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }
    
    void ResumeFollowing()
    {
        if (agent != null && !isFollowing)
        {
            agent.isStopped = false;
            isFollowing = true;
            updateTimer = 0f; // Force immediate destination update
        }
    }
    
    void TeleportNearPlayer()
    {
        if (playerTransform == null)
            return;
            
        // Find a position near the player
        Vector3 teleportPosition = playerTransform.position;
        
        // Add a small offset so we're not exactly on top of the player
        teleportPosition += new Vector3(stoppingDistance * 0.8f, 0f, 0f);
        
        // Ensure the position is on the NavMesh
        if (NavMesh.SamplePosition(teleportPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            // Teleport the NPC
            agent.Warp(hit.position);
            
            // Immediately update the destination
            UpdateDestination();
            
            Debug.Log("NPC Follower teleported to player due to exceeding max follow distance");
        }
    }
    
    // Optional: Public methods to control following behavior
    public void EnableFollowing()
    {
        isFollowing = false; // Setting to false so ResumeFollowing will work
        ResumeFollowing();
    }
    
    public void DisableFollowing()
    {
        StopFollowing();
    }
    
    // Visualization for debugging
    private void OnDrawGizmosSelected()
    {
        // Draw stopping distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        
        // Draw max follow distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxFollowDistance);
    }
}