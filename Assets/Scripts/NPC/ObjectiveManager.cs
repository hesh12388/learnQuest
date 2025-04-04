using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// Get a list of incomplete objective names from UIManager's objectives list
    /// </summary>
    public List<string> GetIncompleteObjectives()
    {
        if (UIManager.Instance == null || UIManager.Instance.objectives_list == null)
            return new List<string>();
        
        return UIManager.Instance.objectives_list
            .Where(o => o.status.ToLower() != "completed")
            .Select(o => o.objective_name)
            .ToList();
    }
    
    /// <summary>
    /// Check if an objective is completed
    /// </summary>
    public bool IsObjectiveCompleted(string objectiveName)
    {
        if (UIManager.Instance == null || UIManager.Instance.objectives_list == null)
            return false;
        
        Objective objective = UIManager.Instance.objectives_list.Find(o => o.objective_name == objectiveName);
        return objective != null && objective.status.ToLower() == "completed";
    }
    
    /// <summary>
    /// Get the next incomplete objective name
    /// </summary>
    public string GetNextIncompleteObjective()
    {
        List<string> incompleteObjectives = GetIncompleteObjectives();
        foreach (string objective in incompleteObjectives)
        {
            Debug.Log("Incomplete Objective: " + objective);
        }
        return incompleteObjectives.Count > 0 ? incompleteObjectives[0] : string.Empty;
    }
}