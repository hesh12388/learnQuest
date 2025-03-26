using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;
    
    // Add a new field for player detection
    [Header("Player Detection")]
    public bool shouldStopNearPlayer = true;
    public float playerDetectionRadius = 10f;
    public LayerMask playerLayer;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting;
    private bool shouldStop = false; // New flag for player collision
    
    private Animator animator;
    private NPC npc;

    public GameObject interactionObject;
    

    void Start()
    {
        animator = GetComponent<Animator>();
        npc = GetComponent<NPC>();
        waypoints = new Transform[waypointParent.childCount];
        for(int i=0; i<waypoints.Length; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }
    }

    void Update()
    {
        // Check for player proximity if feature is enabled
        if (shouldStopNearPlayer)
        {
            CheckForPlayer();
        }

        // Don't move if waiting, instructing, or should stop due to player proximity
        if(isWaiting || npc.isInstructing || shouldStop)
        {
            animator.SetBool("isMoving", false);
            return;
        }

        MoveToWaypoint();
    }

    // New method to check for player proximity
    void CheckForPlayer()
    {
        // Use a non-allocating overlap check for better performance
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, playerLayer);
        
        shouldStop = (playerCollider != null);
        
        // Optional: React when player is first detected
        if (shouldStop)
        {
            if(!interactionObject.activeSelf){
                // Play detection sound
                AudioController.Instance.PlayNpcInteract();
            }
            
            interactionObject.SetActive(true);
            // Player just entered detection range
            OnPlayerDetected(playerCollider.transform);
        }
        else{
            interactionObject.SetActive(false);
        }
    }
    
    // Called when player is first detected
    void OnPlayerDetected(Transform player)
    {
        // Face the player
        Vector2 direction = (player.position - transform.position).normalized;
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
    }
    
    void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];

        Vector2 direction = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetBool("isMoving", direction.magnitude>0f);
        if(Vector2.Distance(transform.position, target.position) < 0.1f)
        {
          StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        animator.SetBool("isMoving", false);
        yield return new WaitForSeconds(waitTime);
        currentWaypointIndex = loopWaypoints ? (currentWaypointIndex + 1) % waypoints.Length : Mathf.Min(currentWaypointIndex+1, waypoints.Length-1);
        isWaiting = false;
    }
    
    // Optional: Visualize the player detection radius in the editor
    private void OnDrawGizmosSelected()
    {
        if (shouldStopNearPlayer)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
}