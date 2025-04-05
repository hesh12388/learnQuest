using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }
    
    // Track the active objective for questions
    private string currentQuestionObjective = null;
    private int currentQuestionObjectiveIndex = -1;
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

    private void Start()
    {
        // Initialize the question objective on start
        InitializeCurrentQuestionObjective();
    }
    
    /// <summary>
    /// Get a list of all objectives from UIManager's list
    /// </summary>
    public List<string> GetAllObjectives()
    {
        if (UIManager.Instance == null || UIManager.Instance.objectives_list == null)
            return new List<string>();
        
        return UIManager.Instance.objectives_list
            .Select(o => o.objective_name)
            .ToList();
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
    /// Get a list of completed objective names
    /// </summary>
    public List<string> GetCompletedObjectives()
    {
        if (UIManager.Instance == null || UIManager.Instance.objectives_list == null)
            return new List<string>();
        
        return UIManager.Instance.objectives_list
            .Where(o => o.status.ToLower() == "completed")
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
    

    /// <summary>
    /// Called when any objective is completed
    /// </summary>
    public void OnObjectiveCompleted()
    {
        //update the current question objective to reflect the new state
        UpdateCurrentQuestionObjective();
     
    }

    /// <summary>
    /// Get the current objective for creatures to ask questions about
    /// (The most recently completed objective)
    /// </summary>
    public string GetCurrentQuestionObjective()
    {
        if (currentQuestionObjective == null)
        {
            UpdateCurrentQuestionObjective();
        }
        return currentQuestionObjective;
    }

    public void InitializeCurrentQuestionObjective()
    {

        // Initialize the question objective on start
        // Check for null references first
        if (UIManager.Instance == null || UIManager.Instance.objectives_list == null || UIManager.Instance.objectives_list.Count == 0)
        {
            currentQuestionObjective = null;
            currentQuestionObjectiveIndex = -1;
            return;
        }
        
        // Initialize the question objective on start
        // Get index of first incomplete objective
        List<Objective> allObjectives = UIManager.Instance?.objectives_list;
        int incompleteIndex = allObjectives.FindIndex(o => o.status.ToLower() != "completed");

        if(incompleteIndex>=0){
            int index = Mathf.Max(0, incompleteIndex - 1);
            currentQuestionObjective = allObjectives[index].objective_name;
            currentQuestionObjectiveIndex = index;
        }
        else{
            currentQuestionObjective = null;
            currentQuestionObjectiveIndex = -1;
        }
    }
    
    /// <summary>
    /// Update the current question objective based on completed objectives
    /// </summary>
    public void UpdateCurrentQuestionObjective()
    {

        // check for null references
        if (UIManager.Instance == null || UIManager.Instance.objectives_list == null || UIManager.Instance.objectives_list.Count == 0)
        {
            currentQuestionObjective = null;
            currentQuestionObjectiveIndex = -1;
            return;
        }

        // Get all objectives in their order
        List<Objective> allObjectives = UIManager.Instance?.objectives_list;
        if (allObjectives == null || allObjectives.Count == 0)
        {
            currentQuestionObjective = null;
            return;
        }
        
        // Get index of first incomplete objective
        int incompleteIndex = allObjectives.FindIndex(o => o.status.ToLower() != "completed");

        if(incompleteIndex<0){
            currentQuestionObjectiveIndex=-1;
            currentQuestionObjective = null;
            return;
        }

          // Increment the index (move to the next objective)
        currentQuestionObjectiveIndex += 1;
        
        // Make sure we don't go past the next incomplete objective
        currentQuestionObjectiveIndex = Mathf.Min(currentQuestionObjectiveIndex, incompleteIndex);
        
        
        currentQuestionObjective = allObjectives[currentQuestionObjectiveIndex].objective_name;
    }
}