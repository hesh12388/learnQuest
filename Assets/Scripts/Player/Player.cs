using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Player : MonoBehaviour
{
    // Movement speed of the player
    private float moveSpeed = 5f;

    // Flag to check if the player is currently moving
    private bool isMoving = false;

    // Stores the player's movement input
    private Vector2 input;

    private Animator animator;

    public LayerMask layerMask;

    public LayerMask interactableLayer;

    public bool isPaused;

    public bool stop_interaction;
    [Header("Player Stats")]
    public int maxHealth = 6;
    public int currentHealth;
    public float invulnerabilityTime = 1.0f;
    private bool isInvulnerable = false;

    [Header("Combat")]
    public int attackDamage = 1;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    private Camera mainCamera;

    public static Player Instance;
    public Transform PlayerTransform { get; private set; } // Reference to the player's Transform
    public PlayerInputActions playerControls;
    
    
    private InputAction movement;
    private InputAction interactAction;
    private InputAction toggleMenuAction;
    private InputAction openShopAction;
    private InputAction openLevelsAction;
    private InputAction openCharactersAction;
    private InputAction openSettingsAction;
    private InputAction openObjectivesAction;
    private InputAction openAchievementsAction;
    private InputAction openLeaderboardAction;
    
    
    public event Action<int> OnHealthChanged;

    private void Awake()
    {
         if (Instance == null)
         {
            Instance = this;
            DontDestroyOnLoad(gameObject);
         }
        else
        {
            Destroy(gameObject);
        }
        playerControls = new PlayerInputActions();
        PlayerTransform = transform;
        animator = GetComponent<Animator>();
    }

    // Add to the Player class:

    private void Start()
    {
        // Load saved bindings if they exist
        LoadBindings();
        currentHealth = maxHealth;
     
    }

    public void LoadBindings()
    {
        if (PlayerPrefs.HasKey("InputBindings"))
        {
            string overridesJson = PlayerPrefs.GetString("InputBindings");
            playerControls.LoadBindingOverridesFromJson(overridesJson);
        }
    }

    
    // Methods for temporarily disabling/enabling input during rebinding
    public void DisableInputTemporarily()
    {
        playerControls.Player.Disable();
        playerControls.UI.Disable();
        playerControls.UIShortCuts.Disable();
    }
    
    public void EnableInput()
    {
        playerControls.Player.Enable();
        playerControls.UI.Enable();
        playerControls.UIShortCuts.Enable();
    }

    private void OnEnable(){
        movement= playerControls.Player.Move;
        movement.Enable();
        interactAction = playerControls.Player.Interact;
        interactAction.Enable();

        toggleMenuAction = playerControls.UI.ToggleMenu;
        openShopAction = playerControls.UIShortCuts.OpenShop;
        openLevelsAction = playerControls.UIShortCuts.OpenLevels;
        openCharactersAction = playerControls.UIShortCuts.OpenCharacters;
        openSettingsAction = playerControls.UIShortCuts.OpenSettings;
        openObjectivesAction = playerControls.UIShortCuts.OpenObjectives;
        openAchievementsAction = playerControls.UIShortCuts.OpenAchievements;
        openLeaderboardAction = playerControls.UIShortCuts.OpenRankings;

        toggleMenuAction.Enable();
        openShopAction.Enable();
        openLevelsAction.Enable();
        openCharactersAction.Enable();
        openSettingsAction.Enable();
        openObjectivesAction.Enable();
        openAchievementsAction.Enable();
        openLeaderboardAction.Enable();

        toggleMenuAction.performed += ctx => {
            if (UIManager.Instance != null)
                UIManager.Instance.OnToggleMenu();
        };
        openShopAction.performed += OnOpenShop;
        openLevelsAction.performed += OnOpenLevels;
        openCharactersAction.performed += OnOpenCharacters;
        openSettingsAction.performed += OnOpenSettings;
        openObjectivesAction.performed += OnOpenObjectives;
        openAchievementsAction.performed += OnOpenAchievements;
        openLeaderboardAction.performed += OnOpenLeaderboard;



        // Subscribe to the performed event for interaction
        interactAction.performed += OnInteractPerformed;
        playerControls.Player.Attack.performed += OnAttackPerformed;
        playerControls.Player.Attack.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        interactAction.Disable();
        toggleMenuAction.Disable();
        openShopAction.Disable();
        openLevelsAction.Disable();
        openCharactersAction.Disable();
        openSettingsAction.Disable();
        openObjectivesAction.Disable();
        openAchievementsAction.Disable();
        openLeaderboardAction.Disable();

        toggleMenuAction.performed -= ctx => {
            if (UIManager.Instance != null)
                UIManager.Instance.OnToggleMenu();
        };
        openShopAction.performed -= OnOpenShop;
        openLevelsAction.performed -= OnOpenLevels;
        openCharactersAction.performed -= OnOpenCharacters;
        openSettingsAction.performed -= OnOpenSettings;
        openObjectivesAction.performed -= OnOpenObjectives;
        openAchievementsAction.performed -= OnOpenAchievements;
        openLeaderboardAction.performed -= OnOpenLeaderboard;

        // Unsubscribe from the performed event
        interactAction.performed -= OnInteractPerformed;
        playerControls.Player.Attack.performed -= OnAttackPerformed;
        playerControls.Player.Attack.Disable();
    }


    // Handle attack input
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Attack input received!");
        if (!UIManager.Instance.isInGame || stop_interaction || UIManager.Instance.isMenuOpen() || isPaused)
        {
            return;
        }
        
    
        var rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if(!rayHit.collider){
            Debug.Log("No collider hit!");
            return;
        }

        Debug.Log("Attack performed!");
        // Check if the ray hit a creature
        var target = rayHit.collider.GetComponent<Creature>();

        if(target != null)
        {
            Debug.Log("Creature hit!");
            QuestionManager.Instance.ShowQuestionForCreature(target);
        }
    }

    private void OnDestroy()
    {
        // Clean up input system
        playerControls?.Dispose();
    }
    

    private void OnOpenShop(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowShop();
    }
    
    // Named method for the event handler
    private void OnInteractPerformed(InputAction.CallbackContext context) {
        if (context.started){
            OnInteract();
        }
    }

    private void OnOpenLevels(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameLevels();
    }
    
    private void OnOpenCharacters(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowCharacterSelection();
    }
    
    private void OnOpenSettings(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowSettings();
    }
    
    private void OnOpenObjectives(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowObjectives();
    }
    
    private void OnOpenAchievements(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowAchievements();
    }
    
    private void OnOpenLeaderboard(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowLeaderboard();
    }

    public void stopInteraction(){
        stop_interaction = true;
    }

    public void SetPosition(Vector3 position)
    {
        isMoving = false;
        
        // Set the position directly
        transform.position = position;
    }

    public void resumeInteraction(){
        stop_interaction = false;
    }

    public void pausePlayer()
    {
        isPaused = true;
    }

    public void resumePlayer()
    {
        isPaused = false;
    }
    // Update is called once per frame
    void Update()
    {

        if(!UIManager.Instance.isInGame || stop_interaction || UIManager.Instance.isMenuOpen()|| isPaused){
            return;
        }
     

        // Check if the player is not already moving
        if (!isMoving)
        {
        
            input = movement.ReadValue<Vector2>();

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


                if(isWalkable(pos))
                {
                    // Start the movement coroutine
                    StartCoroutine(Move(pos));
                }
                
            }
        }

        animator.SetBool("isMoving", isMoving);
    }


    private void OnInteract()
    {
        if(!UIManager.Instance.isInGame || stop_interaction || UIManager.Instance.isMenuOpen()){
            return;
        }


        // Calculate the position in front of the player
        Vector2 facingDir = new Vector2(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        if (facingDir == Vector2.zero)
            facingDir = Vector2.down;

        Vector2 interactionPos = (Vector2)transform.position + facingDir;

        // Check for NPCs at the interaction position
        Collider2D hit = Physics2D.OverlapCircle(interactionPos, 1.5f, interactableLayer);

        if (hit != null)
        {
            // Try to get NPC component and call Interact method
            NPC npc = hit.GetComponent<NPC>();
            if (npc != null)
            {
                npc.Interact();
            }
        }
    }

    private bool isWalkable(Vector3 targetPos)
    {
        // Cast a box around the player to check for collisions
        var hit = Physics2D.OverlapCircle(targetPos, 0.45f, layerMask | interactableLayer);

        // If there is no collision, the target position is walkable
        return hit == null;
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


    // Method for creatures to damage the player
    public void TakeDamage(int damage)
    {
        // Check if player is invulnerable
        if (isInvulnerable)
            return;
        
        
        // Apply damage
        currentHealth -= damage;
        
        // Clamp health to prevent negative values
        currentHealth = Mathf.Max(0, currentHealth);

        // Trigger the event
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        
        // Temporary invulnerability
        StartCoroutine(InvulnerabilityPeriod());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator InvulnerabilityPeriod()
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
        Debug.Log("Player died!");

        DatabaseManager.Instance.loggedInUser.score -= 10;
        
        DatabaseManager.Instance.loggedInUser.score = Mathf.Max(0, DatabaseManager.Instance.loggedInUser.score);

        DatabaseManager.Instance.UpdateUserScore();

        //respawn player
        StartCoroutine(TransitionManager.Instance.contentTransition());
        PlayerPositionManager.Instance.PositionPlayerAtEntryPoint();
        UIManager.Instance.updatePlayerCoins();
        CreatureManager.Instance.ClearAllCreatures();
        currentHealth=maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
