using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cinemachine;

// Add this class for NPC to indicator mapping
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
    public bool isInstructing { get; set; } = false;

    private Dictionary<string, GameObject> npcIndicators = new Dictionary<string, GameObject>();
    [SerializeField] private List<NPCIndicatorMapping> npcIndicatorsList = new List<NPCIndicatorMapping>();

    private Dictionary<string, string> prerequisiteDialogues = new Dictionary<string, string>();
    private Dictionary<string, string> evaluationDialogues = new Dictionary<string, string>();
    private Dictionary<string, PostEvaluationDialogue> postEvaluationDialogues = new Dictionary<string, PostEvaluationDialogue>();
    private string nextObjective;
    private Image npcImage;
    private GameObject dialoguePanel;
    private TMP_Text dialogueText;
    private Button closeDialogue;

    public Sprite npcImageSprite;

    private float typingSpeed = 0.05f;

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

        InitializeDictionary();
        LoadPrerequisiteDialogues();
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

    private void Start()
    {
        LoadObjectivesFromDatabase();
        npcImage=UIManager.Instance.demonstration_npcImage;
        dialoguePanel= UIManager.Instance.dialoguePanel;
        dialogueText= UIManager.Instance.demonstration_dialogueText;
        closeDialogue= UIManager.Instance.closeDialogue;
    }

    // Load prerequisite and post-evaluation dialogues from JSON
    private void LoadPrerequisiteDialogues()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "PrerequisiteDialogues.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<PrerequisiteDialogues>(json);

            // Load objectives
            foreach (var objective in data.objectives)
            {
                prerequisiteDialogues[objective.objective_name] = objective.dialogue;
            }

            // Load evaluations (new)
            foreach (var eval in data.evaluations)
            {
                evaluationDialogues[eval.npcName] = eval.dialogue;
            }

            // Load post-evaluations
            foreach (var postEval in data.post_evaluation)
            {
                postEvaluationDialogues[postEval.npcName] = postEval;
            }
        }
        else
        {
            Debug.LogError("PrerequisiteDialogues.json not found!");
        }
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

    // Determine the next objective to complete
    public void DetermineNextObjective()
    {
        foreach (var objective in prerequisiteDialogues.Keys)
        {
            if (!completedNPCs.Contains(objective))
            {
                nextObjective = objective;
                return;
            }
        }

        // If all objectives are completed, set the next step to evaluation
        nextObjective = "evaluation";
    }

    // Show the dialogue and guide the player to the next NPC
    public IEnumerator ShowNextObjective()
    {
        if (nextObjective == "evaluation")
        {
            int currentChapter = DatabaseManager.Instance.loggedInUser.currentChapter;
            int currentLevel = DatabaseManager.Instance.loggedInUser.currentLevel - 1; // 0-based index
            bool isLevelCompleted = DatabaseManager.Instance.loggedInUser.courseStructure.chapters[currentChapter].levels[currentLevel].isCompleted;
            
            if(isLevelCompleted)
            {
                Debug.Log("Level already completed, no need to show evaluation dialogue");
                yield break;
            }
            else
            {
                // Get the appropriate enemy NPC name based on the current level/chapter
                string enemyNpcName = GetCurrentEnemyNPC();
                
                // Get the dialogue for this enemy, or use a default if not found
                string dialogue = evaluationDialogues.ContainsKey(enemyNpcName) 
                    ? evaluationDialogues[enemyNpcName] 
                    : "It's time for your evaluation!";
                    
                yield return StartCoroutine(ShowDialogueAndGuide(dialogue, enemyNpcName));
            }
        }
        else if (prerequisiteDialogues.ContainsKey(nextObjective))
        {
            string dialogue = prerequisiteDialogues[nextObjective];
            yield return StartCoroutine(ShowDialogueAndGuide(dialogue, nextObjective));
        }
    }


    private IEnumerator ShowDialogueAndGuide(string dialogue, string npcName)
    {
        Player.Instance.stopInteraction();
        UIManager.Instance.disablePlayerHUD();
        // Show the dialogue panel
        yield return StartCoroutine(ShowDialoguePanel(dialogue));

        // Show the guide indicator for the NPC
        if (npcIndicators.ContainsKey(npcName))
        {
            yield return StartCoroutine(showNpcIndicator(npcName));
        }
        else
        {
            Debug.LogWarning($"No indicator found for NPC: {npcName}");
        }

        Player.Instance.resumeInteraction();
        UIManager.Instance.enablePlayerHUD();
        isInstructing = false;
    }

    IEnumerator ShowDialoguePanel(string dialogue)
    {
        closeDialogue.gameObject.SetActive(false);
        AudioController.Instance.PlayDemonstrationMusic();
        isInstructing = true;
        // Show the dialogue panel
        AudioController.Instance.PlayMenuOpen();
        dialoguePanel.SetActive(true);
        npcImage.sprite = npcImageSprite; // Set the NPC image
        yield return StartCoroutine(TypeText(dialogueText, dialogue, typingSpeed));
        yield return new WaitForSeconds(2f); // Wait for a moment before closing
        AudioController.Instance.PlayMenuOpen();
        dialoguePanel.SetActive(false);
        npcImage.sprite = null; // Reset the NPC image
        AudioController.Instance.PlayBackgroundMusic();
        closeDialogue.gameObject.SetActive(true);
    }

    private IEnumerator TypeText(TMP_Text textObject, string message, float typingSpeed)
    {
        textObject.text = ""; // Clear the text before typing
        foreach (char letter in message.ToCharArray())
        {
            textObject.text += letter; // Add one character at a time
            yield return new WaitForSeconds(typingSpeed); // Wait before adding the next character
        }
    }

    private IEnumerator showNpcIndicator(string npcName)
    {
        CameraManager.SwitchCamera(wideCamera);
        npcIndicators[npcName].SetActive(true);
        yield return new WaitForSeconds(4f);
        CameraManager.SwitchCamera(shallowCamera);
        npcIndicators[npcName].SetActive(false);
    }

    // Show post-evaluation dialogue based on result
    public IEnumerator ShowPostEvaluationDialogue(string npcName, bool passed)
    {
        if (postEvaluationDialogues.ContainsKey(npcName))
        {
            string dialogue = passed ? postEvaluationDialogues[npcName].pass : postEvaluationDialogues[npcName].fail;
            yield return StartCoroutine(ShowDialoguePanel(dialogue));
        }
        else
        {
            Debug.LogWarning($"No post-evaluation dialogue found for NPC: {npcName}");
        }
    }

    private string GetCurrentEnemyNPC()
    {
        int currentLevel = DatabaseManager.Instance.loggedInUser.currentLevel;
        
        // Map chapters/levels to enemy NPCs
        if ( currentLevel == 1) return "Raven";  
        if (currentLevel == 2) return "Pyron";
        if (currentLevel == 3) return "Noir";
        if (currentLevel == 4) return "Fangor";
        if (currentLevel == 5) return "Shadow";
        if (currentLevel == 6) return "Frost";
        
        return "Enemy";  
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

// Helper classes for deserializing JSON
[System.Serializable]
public class PrerequisiteDialogues
{
    public List<ObjectiveDialogue> objectives;
    public List<EvaluationDialogue> evaluations;  
    public List<PostEvaluationDialogue> post_evaluation;
}


[System.Serializable]
public class ObjectiveDialogue
{
    public string objective_name;
    public string dialogue;
}

[System.Serializable]
public class EvaluationDialogue
{
    public string npcName;
    public string dialogue;
}

[System.Serializable]
public class PostEvaluationDialogue
{
    public string npcName;
    public string pass;
    public string fail;
}