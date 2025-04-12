using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }

    [System.Serializable]
    public class Question
    {
        public string objectiveName;
        public string questionText;
        public string[] answers;
        public int correctAnswerIndex;
        public string correctResponse;
        public string incorrectResponse;
    }

    [Header("Questions Data")]
    private string questionsFilePath = "Questions";
    private List<Question> allQuestions = new List<Question>();
    private Dictionary<string, List<Question>> questionsByObjective = new Dictionary<string, List<Question>>();

    private Creature currentTarget;
    private Question currentQuestion;
    private bool hasAnsweredCorrectly = false;

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

        LoadQuestions();
    }


    /// <summary>
    /// Get the current objective for creatures to ask questions about
    /// (The first incomplete objective)
    /// </summary>
    public string GetCurrentIncompleteObjective()
    {
        List<Objective> allObjectives = ObjectiveManager.Instance.GetAllObjectives();
        if (allObjectives == null || allObjectives.Count == 0)
        {
            return null;
        }

        int incompleteIndex = allObjectives.FindIndex(o => o.status.ToLower() != "completed");
        
        if (incompleteIndex >= 0)
        {
            return allObjectives[incompleteIndex].objective_name;
        }
        
        // If all objectives are complete, return the last one
        if (allObjectives.Count > 0)
        {
            return allObjectives[allObjectives.Count - 1].objective_name;
        }
        
        return null;
    }

    
    private void LoadQuestions()
    {
        // Try loading from StreamingAssets first
        string filePath = Path.Combine(Application.streamingAssetsPath, "Questions.json");
        string jsonContent = "";
        
        // In WebGL, load directly from Resources
        TextAsset jsonAsset = Resources.Load<TextAsset>(questionsFilePath);
        if (jsonAsset != null)
        {
            jsonContent = jsonAsset.text;
        }
        else
        {
            Debug.LogError("Questions file not found in Resources folder. Make sure 'Questions.json' is in Assets/Resources/");
            return;
        }
        
        // Parse JSON data
        if (!string.IsNullOrEmpty(jsonContent))
        {
            QuestionsData data = JsonUtility.FromJson<QuestionsData>(jsonContent);
            allQuestions = data.questions;
            
            // Organize questions by objective
            foreach (var question in allQuestions)
            {
                if (!questionsByObjective.ContainsKey(question.objectiveName))
                {
                    questionsByObjective[question.objectiveName] = new List<Question>();
                }
                questionsByObjective[question.objectiveName].Add(question);
            }
            
            Debug.Log($"Loaded {allQuestions.Count} questions for {questionsByObjective.Count} objectives");
        }
        else
        {
            Debug.LogError("Failed to load questions - empty JSON content");
        }
    }
    
    /// <summary>
    /// Show a question when a creature is clicked
    /// </summary>
    public void ShowQuestionForCreature(Creature creature)
    {
        // Store reference to current target
        currentTarget = creature;
        
        // Get all valid objectives for questions
        List<string> validObjectives = GetValidObjectives();
        
        if (validObjectives.Count == 0)
        {
            Debug.Log("No objectives available for questions");
            return;
        }
        
        // Randomly select one of the valid objectives
        string selectedObjective = validObjectives[Random.Range(0, validObjectives.Count)];
        Debug.Log($"Showing question for objective: {selectedObjective}");
        
        // Find a random question for the selected objective
        if (questionsByObjective.ContainsKey(selectedObjective) && 
            questionsByObjective[selectedObjective].Count > 0)
        {
            int randomIndex = Random.Range(0, questionsByObjective[selectedObjective].Count);
            Question question = questionsByObjective[selectedObjective][randomIndex];
            
            // Show the question
            ShowQuestion(question);
        }
        else
        {
            Debug.Log($"No questions available for objective: {selectedObjective}");
        }
    }

    /// <summary>
    /// Get all valid objectives for questions (all completed + current incomplete)
    /// </summary>
    private List<string> GetValidObjectives()
    {
        List<string> validObjectives = new List<string>();
        List<Objective> allObjectives = ObjectiveManager.Instance.GetAllObjectives();
        
        if (allObjectives == null || allObjectives.Count == 0)
        {
            return validObjectives;
        }
        
        // Find the index of the first incomplete objective
        int incompleteIndex = allObjectives.FindIndex(o => o.status.ToLower() != "completed");
        
        // Add all completed objectives
        for (int i = 0; i < allObjectives.Count; i++)
        {
            // Include all completed objectives and the first incomplete one
            if (allObjectives[i].status.ToLower() == "completed" || i == incompleteIndex)
            {
                validObjectives.Add(allObjectives[i].objective_name);
            }
            
            // Stop once we've added the first incomplete objective
            if (i == incompleteIndex)
            {
                break;
            }
        }
        
        // If all objectives are complete, include them all
        if (incompleteIndex < 0)
        {
            validObjectives = allObjectives.Select(o => o.objective_name).ToList();
        }
        
        return validObjectives;
    }
    
    private void ShowQuestion(Question question)
    {
        UIManager.Instance.ResetQuestionAnswerButtons();
        currentQuestion = question;
        hasAnsweredCorrectly = false;
        
        // Pause the player
        if (Player.Instance != null) {
            Player.Instance.pausePlayer();
            Player.Instance.stopInteraction();
        }
        
        if(UIManager.Instance != null && UIManager.Instance.isMenuOpen())
            UIManager.Instance.setMenuOpen(false);
        
        // Set up UI through UIManager
        UIManager.Instance.ShowQuestionPanel(true);
        UIManager.Instance.SetQuestionText(question.questionText);
        
        // Set up answer button callbacks
        System.Action<int>[] callbacks = new System.Action<int>[question.answers.Length];
        for (int i = 0; i < question.answers.Length; i++)
        {
            int capturedIndex = i;
            callbacks[i] = (index) => OnAnswerSelected(capturedIndex);
        }
        
        // Set up answer buttons through UIManager
        UIManager.Instance.SetupQuestionAnswerButtons(question.answers, callbacks);
        
        // Play sound
        if (AudioController.Instance != null)
            AudioController.Instance.PlayMenuOpen();
    }
    
    private void OnAnswerSelected(int selectedAnswerIndex)
    {
        if (currentQuestion == null)
            return;
        
        bool isCorrect = (selectedAnswerIndex == currentQuestion.correctAnswerIndex);
        hasAnsweredCorrectly = isCorrect;
        
        // Show response based on correctness
        string responseText = isCorrect ? currentQuestion.correctResponse : currentQuestion.incorrectResponse;
        UIManager.Instance.SetQuestionText(responseText);
        
        // Play sound based on result
        if (AudioController.Instance != null)
        {
            if (isCorrect)
                AudioController.Instance.PlayAnswerCorrect();
            else
                AudioController.Instance.PlayAnswerIncorrect();
        }
        
        // Disable all answer buttons through UIManager
        UIManager.Instance.DisableQuestionAnswerButtons();
        
        StartCoroutine(CloseQuestionPanel());
    }
        
    private IEnumerator CloseQuestionPanel()
    {
        // Wait for a short duration before closing the panel
        yield return new WaitForSeconds(2f);
        
        // If answered correctly, destroy the creature
        if (hasAnsweredCorrectly && currentTarget != null)
        {
            // Kill the creature
            currentTarget.TakeDamage(999); // Large damage to ensure death
        }
        
        // Hide the question panel through UIManager
        UIManager.Instance.ShowQuestionPanel(false);
        
        // Resume the player
        if (Player.Instance != null) {
            Player.Instance.resumePlayer();
            Player.Instance.resumeInteraction();
        }
            
        // Clear references
        currentTarget = null;
        currentQuestion = null;
    }
    
    [System.Serializable]
    private class QuestionsData
    {
        public List<Question> questions = new List<Question>();
    }
}