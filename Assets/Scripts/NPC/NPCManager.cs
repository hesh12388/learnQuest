using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;

[System.Serializable]
public class NPCIndicatorMapping
{
    public string npcName;
    public GameObject indicator;
}

public class NPCManager : MonoBehaviour
{
   
    private HashSet<string> completedNPCs = new HashSet<string>();

    private int numNpcs;
    public static NPCManager Instance;
    public CinemachineVirtualCamera shallowCamera;
    public CinemachineVirtualCamera wideCamera;

    private Dictionary<string, GameObject> npcIndicators = new Dictionary<string, GameObject>();

    [SerializeField]
    private List<NPCIndicatorMapping> npcIndicatorsList = new List<NPCIndicatorMapping>();

    private void Awake()
    {
        // Setup singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Convert the list to a dictionary at runtime
        InitializeDictionary();
    }

    private void Start()
    {
        // Load objectives from database when the scene starts
        LoadObjectivesFromDatabase();
    }

    // Load objectives from database and initialize the completedNPCs HashSet
    private void LoadObjectivesFromDatabase()
    {
        // Check if there's a logged in user
        if (DatabaseManager.Instance.loggedInUser == null)
        {
            Debug.LogWarning("Cannot load objectives: No user is logged in");
            return;
        }

        // Call the DatabaseManager to get objectives
        DatabaseManager.Instance.GetObjectives((objectives) => {
            if (objectives != null)
            {
                completedNPCs.Clear(); // Clear the current HashSet

                // Add completed objectives to the HashSet
                foreach (var objective in objectives)
                {
                    if (objective.status.ToLower() == "completed")
                    {
                        completedNPCs.Add(objective.objective_name);
                    }
                }

                Debug.Log($"Loaded {completedNPCs.Count} completed NPCs/objectives from database");
                
                // Update the numNpcs based on the total number of objectives
                numNpcs = objectives.Count;
            }
            else
            {
                Debug.LogError("Failed to load objectives from database");
            }
        });
    }

    // Initialize the dictionary from inspector-assigned list
    private void InitializeDictionary()
    {
        npcIndicators.Clear();
        
        foreach (var mapping in npcIndicatorsList)
        {
            if (!string.IsNullOrEmpty(mapping.npcName) && mapping.indicator != null)
            {
                npcIndicators[mapping.npcName] = mapping.indicator;
                // Hide all indicators initially
                mapping.indicator.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Invalid NPC indicator mapping detected. Check your Inspector settings.");
            }
        }
    }


    public void showIndicator(string npcName){
        StartCoroutine(showNpcIndicator(npcName));
    }

    private IEnumerator showNpcIndicator(string npcName){
        CameraManager.SwitchCamera(wideCamera);
        npcIndicators[npcName].SetActive(true);
        yield return new WaitForSeconds(4f);
        CameraManager.SwitchCamera(shallowCamera);
        npcIndicators[npcName].SetActive(false);
    }


    public bool HasCompletedNPC(string npcName)
    {
        return completedNPCs.Contains(npcName);
    }

    public void MarkNPCCompleted(string npcName)
    {
        // Only proceed if NPC is not already marked as completed
        if (!completedNPCs.Contains(npcName))
        {
            // Add to local HashSet for immediate feedback
            completedNPCs.Add(npcName);
            
            // Call DatabaseManager to update the database
            DatabaseManager.Instance.CompleteObjective(npcName, (success) => {
                if (success)
                {
                    Debug.Log($"NPC/Objective '{npcName}' marked as completed in the database");
                    StartCoroutine(UIManager.Instance.ShowObjectiveComplete(npcName));
                    
                }
                else
                {
                    // If the database update failed, remove from local HashSet to maintain consistency
                    Debug.LogError($"Failed to mark NPC/Objective '{npcName}' as completed in database");
                    completedNPCs.Remove(npcName);
                }
            });
        }
    }

    public bool AreAllNPCsCompleted()
    {
        return completedNPCs.Count == numNpcs;
    }
}
