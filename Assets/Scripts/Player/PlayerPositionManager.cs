using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionManager : MonoBehaviour
{
    public static PlayerPositionManager Instance { get; private set; }
    
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
    }
    
    private void OnEnable()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        // Unsubscribe when disabled
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wait a frame to make sure everything is initialized
        Invoke("PositionPlayerAtEntryPoint", 0.1f);
    }
    
    public void PositionPlayerAtEntryPoint()
    {
        // Find the player
        Player player = FindObjectOfType<Player>();
        
        if (player == null)
        {
            Debug.LogWarning("PlayerPositionManager: Player not found in scene");
            return;
        }
        
        // Find the entry point
        LevelEntryPoint entryPoint = FindObjectOfType<LevelEntryPoint>();
        
        if (entryPoint == null)
        {
            Debug.LogWarning("PlayerPositionManager: No entry point found in scene");
            return;
        }
        
        // Set player position
        player.transform.position = entryPoint.transform.position;
        
        Debug.Log("PlayerPositionManager: Positioned player at entry point");
    }
}