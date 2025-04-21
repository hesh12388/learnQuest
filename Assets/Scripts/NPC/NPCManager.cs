using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cinemachine;

// class for NPC to indicator mapping
[System.Serializable]
public class NPCIndicatorMapping
{
    public string npcName;
    public GameObject indicator;
}

public class NPCManager : MonoBehaviour
{
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
    public Sprite npcImageSprite;
    private float typingSpeed = 0.05f;
    public bool isGuiding = false;

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
        StartCoroutine(showNextGuide());
    }

    // Load prerequisite and post-evaluation dialogues from JSON
    private void LoadPrerequisiteDialogues()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("PrerequisiteDialogues");
        if (jsonAsset != null)
        {
            string json = jsonAsset.text;
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


    public IEnumerator showNextGuide(){
        yield return new WaitForSeconds(2f);
        DetermineNextObjective();
        StartCoroutine(ShowNextObjective());

    }

    // Determine the next objective to complete
   public void DetermineNextObjective()
    {
        // Get incomplete objectives from ObjectiveManager
        List<Objective> incompleteObjectives = ObjectiveManager.Instance.GetIncompleteObjectives();

        if (incompleteObjectives.Count > 0)
        {
            // Set the next objective to the first incomplete one
            nextObjective = incompleteObjectives[0].objective_name;
        }
        else
        {
            // If all objectives are completed, set to evaluation
            nextObjective = "evaluation";
        }
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
                UIManager.Instance.setObjectiveGuideText("All Objectives Completed", true);
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
                UIManager.Instance.setObjectiveGuideText(enemyNpcName, false);
            }
        }
        else if (prerequisiteDialogues.ContainsKey(nextObjective))
        {
            string dialogue = prerequisiteDialogues[nextObjective];
            yield return StartCoroutine(ShowDialogueAndGuide(dialogue, nextObjective));
            UIManager.Instance.setObjectiveGuideText(nextObjective, false);
        }
    }


    private IEnumerator ShowDialogueAndGuide(string dialogue, string npcName)
    {
        // Show the dialogue for guiding the player
        yield return StartCoroutine(ShowDialoguePanel(dialogue));

        // Show the guide indicator for the next NPC to visit
        if (npcIndicators.ContainsKey(npcName))
        {
            yield return StartCoroutine(showNpcIndicator(npcName));
        }
        else
        {
            Debug.LogWarning($"No indicator found for NPC: {npcName}");
        }

        isInstructing = false;
        isGuiding = false;
    }

    IEnumerator ShowDialoguePanel(string dialogue)
    {
        UIManager.Instance.setGameUIPanelsInactive();
        UIManager.Instance.setMenuOpen(false);
        Player.Instance.stopInteraction();
        Player.Instance.pausePlayer();
        UIManager.Instance.disablePlayerHUD();
        AudioController.Instance.PlayDemonstrationMusic();
        isInstructing = true;
        
        // Show the dialogue panel using UIManager methods
        AudioController.Instance.PlayMenuOpen();
        
        // Use existing UIManager dialogue methods
        UIManager.Instance.ShowDialoguePanel(null, 0);
        UIManager.Instance.ShowNPCImageOnly(npcImageSprite);
        UIManager.Instance.closeDialogue.gameObject.SetActive(false);
        
        // Type the dialogue using UIManager's typing method
        yield return StartCoroutine(UIManager.Instance.TypeDialogueText(dialogue, typingSpeed));
        
        // Wait for a moment before closing
        yield return new WaitForSeconds(2f);
        
        // Close dialogue and reset state
        AudioController.Instance.PlayMenuOpen();
        UIManager.Instance.HideDialoguePanel();
        AudioController.Instance.PlayBackgroundMusic();
        UIManager.Instance.closeDialogue.gameObject.SetActive(true);
        Player.Instance.resumeInteraction();
        Player.Instance.resumePlayer();
        UIManager.Instance.enablePlayerHUD();
        isInstructing = false;
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

    public IEnumerator showNpcIndicator(string npcName)
    {
        UIManager.Instance.disablePlayerHUD();
        CameraManager.SwitchCamera(wideCamera);
        npcIndicators[npcName].SetActive(true);
        yield return new WaitForSeconds(4f);
        CameraManager.SwitchCamera(shallowCamera);
        npcIndicators[npcName].SetActive(false);
        UIManager.Instance.enablePlayerHUD();
    }

    // Show post-evaluation dialogue based on result
    public IEnumerator ShowPostEvaluationDialogue(string npcName, bool hasFailed)
    {
        if (postEvaluationDialogues.ContainsKey(npcName))
        {
            string dialogue = hasFailed ? postEvaluationDialogues[npcName].fail : postEvaluationDialogues[npcName].pass;
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