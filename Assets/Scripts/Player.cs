using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Movement speed of the player
    private float moveSpeed = 5f;

    // Flag to check if the player is currently moving
    private bool isMoving = false;

    // Stores the player's movement input
    private Vector2 input;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Currently empty, but can be used for initialization
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is not already moving
        if (!isMoving)
        {
            // Get movement input from player (either -1, 0, or 1)
            input.x = Input.GetAxisRaw("Horizontal"); // Left (-1) or Right (1)
            input.y = Input.GetAxisRaw("Vertical");   // Down (-1) or Up (1)

            // Restrict movement to one direction at a time (no diagonal movement)
            if (input.x != 0) input.y = 0;

            // If there is movement input
            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
            
                // Get the current position of the player
                var pos = transform.position;

                // Update the position based on input
                pos.x += input.x;
                pos.y += input.y;

                // Start the movement coroutine
                StartCoroutine(Move(pos));
            }
        }

        animator.SetBool("isMoving", isMoving);
    }

    // Coroutine to smoothly move the player to the target position
    IEnumerator Move(Vector3 pos)
    {
        // Mark the player as moving
        isMoving = true;

        // Move the player towards the target position until it reaches it
        while ((pos - transform.position).sqrMagnitude > Mathf.Epsilon) // Mathf.Epsilon is a small value close to zero
        {
            // Move the player towards the target position over time
            transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);

            // Wait until the next frame before continuing the loop
            yield return null;
        }

        // Ensure the player reaches the exact position
        transform.position = pos;

        // Mark the player as not moving
        isMoving = false;
    }
}
