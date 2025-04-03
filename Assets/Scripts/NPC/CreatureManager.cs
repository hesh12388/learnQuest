using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }
    
    [Header("Creature Prefabs")]
    [SerializeField] private GameObject[] creaturePrefabs; // Different creatures for different levels
    
    [Header("Spawn Settings")]
    [SerializeField] private int maxCreatures = 5;
    [SerializeField] private float minSpawnDistance = 8f;
    [SerializeField] private float maxSpawnDistance = 15f;
    [SerializeField] private float spawnInterval = 15f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private LayerMask obstacleLayer; // Layer mask for obstacles
    
    // List to track active creatures
    private List<Creature> activeCreatures = new List<Creature>();
    private bool isSpawning = false;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            Debug.Log("Starting creature spawning...");
            isSpawning = true;
            StartCoroutine(SpawnCreatureRoutine());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
    }
    
    private IEnumerator SpawnCreatureRoutine()
    {
        while (isSpawning)
        {
            if (activeCreatures.Count < maxCreatures && Player.Instance != null)
            {
                SpawnCreatureAroundPlayer();
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    private void SpawnCreatureAroundPlayer()
    {

        Debug.Log("Attempting to spawn a creature...");
        if (creaturePrefabs.Length == 0 || Player.Instance == null || 
            DatabaseManager.Instance == null || DatabaseManager.Instance.loggedInUser == null)
            return;
            
        // Find a valid spawn position
        Vector2 spawnPos = GetValidSpawnPosition();
        if (spawnPos == Vector2.zero){ // Invalid position
            Debug.Log("Failed to find a valid spawn position.");
            return;
        }
            
        // Get the current level (use as index for creature selection)
        int currentLevel = DatabaseManager.Instance.loggedInUser.currentLevel-1;
        
        // Make sure the level is valid for array access
        int creatureIndex = Mathf.Clamp(currentLevel % creaturePrefabs.Length, 0, creaturePrefabs.Length - 1);

        Debug.Log($"Spawning creature of level {currentLevel} at {spawnPos}");
        
        // Spawn the level-appropriate creature
        GameObject creaturePrefab = creaturePrefabs[creatureIndex];
        
        // Spawn the creature
        GameObject creatureObj = Instantiate(creaturePrefab, spawnPos, Quaternion.identity);
        Creature creature = creatureObj.GetComponent<Creature>();
        
        if (creature != null)
        {
            creature.OnCreatureDefeated += HandleCreatureDefeated;
            activeCreatures.Add(creature);
            Debug.Log($"Spawned level {currentLevel} creature at {spawnPos}. Total creatures: {activeCreatures.Count}");
        }
    }
    
    private Vector2 GetValidSpawnPosition()
    {
        if (Player.Instance == null)
            return Vector2.zero;
            
        Vector2 playerPos = Player.Instance.transform.position;
        Vector2 spawnPos = Vector2.zero;
        bool validPosition = false;
        int attempts = 0;
        
        NavMeshHit hit;
        
        do {
            // Calculate a random position at a random distance from the player
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
            
            // Convert polar coordinates to Cartesian
            float x = playerPos.x + distance * Mathf.Cos(angle);
            float y = playerPos.y + distance * Mathf.Sin(angle);
            
            Vector3 potentialPos = new Vector3(x, y, 0);
            
            // Check if the position is on the NavMesh
            if (NavMesh.SamplePosition(potentialPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                // Check if there's no obstacle at that position
                if (!Physics2D.OverlapCircle(hit.position, 1.0f, obstacleLayer))
                {
                    spawnPos = hit.position;
                    validPosition = true;
                }
            }
            
            attempts++;
        } while (!validPosition && attempts < 10);
        
        return spawnPos;
    }
    
    private void HandleCreatureDefeated(Creature creature)
    {
        if (creature != null)
        {
            creature.OnCreatureDefeated -= HandleCreatureDefeated;
            activeCreatures.Remove(creature);
            Debug.Log($"Creature defeated. Remaining creatures: {activeCreatures.Count}");
        }
    }
    
    public void ClearAllCreatures()
    {
        foreach (var creature in activeCreatures.ToArray())
        {
            if (creature != null)
            {
                creature.OnCreatureDefeated -= HandleCreatureDefeated;
                Destroy(creature.gameObject);
            }
        }
        
        activeCreatures.Clear();
    }
    
    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearAllCreatures();
    }
}