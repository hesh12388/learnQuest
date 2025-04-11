using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }
    private List<Objective> objectives = new List<Objective>(); // Store objectives for the current level

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
            return;
        }
    }

    
     /// <summary>
    /// Fetch objectives for the current level from the database
    /// </summary>
    public void LoadObjectives()
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.loggedInUser == null)
        {
            Debug.LogError("Cannot load objectives: No user is logged in or DatabaseManager is unavailable");
            return;
        }

        DatabaseManager.Instance.GetObjectives((fetchedObjectives) =>
        {
            if (fetchedObjectives != null)
            {
                objectives = fetchedObjectives;
                Debug.Log($"Loaded {objectives.Count} objectives for the current level");
            }
            else
            {
                Debug.LogError("Failed to load objectives from the database");
            }
        });
    }

    /// <summary>
    /// Get all objectives for the current level
    /// </summary>
    public List<Objective> GetAllObjectives()
    {
        return objectives;
    }

    /// <summary>
    /// Get incomplete objectives
    /// </summary>
    public List<Objective> GetIncompleteObjectives()
    {
        return objectives.Where(o => o.status.ToLower() != "completed").ToList();
    }

    /// <summary>
    /// Get completed objectives
    /// </summary>
    public List<Objective> GetCompletedObjectives()
    {
        return objectives.Where(o => o.status.ToLower() == "completed").ToList();
    }
    
    /// <summary>
    /// Check if an objective is completed
    /// </summary>
    public bool IsObjectiveCompleted(string objectiveName)
    {
        Objective objective = objectives.Find(o => o.objective_name == objectiveName);
        return objective != null && objective.status.ToLower() == "completed";
    }

    /// <summary>
    /// Check if all objectives are completed
    /// </summary>
    public bool AreAllObjectivesCompleted()
    {
        return objectives.All(o => o.status.ToLower() == "completed");
    }

    /// <summary>
    /// Mark an objective as completed
    /// </summary>
    public void MarkObjectiveCompleted(string objectiveName)
    {
        Objective objective = objectives.Find(o => o.objective_name == objectiveName);
        

        // Call DatabaseManager to update the database
        DatabaseManager.Instance.CompleteObjective(objectiveName, (success) => {
            if (success)
            {
                Debug.Log($"NPC/Objective '{objectiveName}' marked as completed in the database");
                objective.status = "completed";
                DatabaseManager.Instance.loggedInUser.score+= objective.points;
                StartCoroutine(ShowObjectiveComplete(objective));    
            }
            else
            {
                // If the database update failed, remove from local HashSet to maintain consistency
                Debug.LogError($"Failed to mark NPC/Objective '{objectiveName}' as completed in database");
            }
        });
    }

    public IEnumerator ShowObjectiveComplete(Objective objective){
        yield return StartCoroutine(UIManager.Instance.ShowObjectiveComplete(objective));
        yield return StartCoroutine(NPCManager.Instance.showNextGuide());  
    }
    
    /// <summary>
    /// Get the next incomplete objective name
    /// </summary>
    public string GetNextIncompleteObjective()
    {
        List<Objective> incompleteObjectives = GetIncompleteObjectives();
        return incompleteObjectives.Count > 0 ? incompleteObjectives[0].objective_name : string.Empty;
    }
    

}