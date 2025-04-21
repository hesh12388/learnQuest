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
    private float stoppingDistance = 12.0f;
    
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
        }
        else
        {
            Debug.LogWarning("Player Instance not found. The follower won't work until Player is available.");
        }
    }
    

    // method to make the assistant always face the player
    void FacePlayer()
    {
        if (playerTransform == null || animator == null)
            return;
            
        // Calculate direction from assistant to player
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        // Only update animation parameters if the direction is significant
        if (direction.magnitude > 0.1f)
        {
            // Set animation parameters to face the player
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
        }
    }

    void Update()
    {
        // If the player reference is missing, try to get it
        if (playerTransform == null && Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
        }
        
        // If we still don't have player or agent, can't do anything
        if (playerTransform == null || agent == null)
            return;
            
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
        
        // Check if we should be moving or not
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool shouldMove = distanceToPlayer > agent.stoppingDistance;
        
        if (shouldMove)
        {
            // Update timer for path recalculation
            updateTimer -= Time.deltaTime;
            if (updateTimer <= 0f)
            {
                updateTimer = updateFrequency;
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
            }
            // Update animator based on current movement
            UpdateAnimator();
        }
        else
        {
            // face the player
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
            FacePlayer();
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
    
}