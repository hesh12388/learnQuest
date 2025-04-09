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

    [Header("Questions Panel")]
    private GameObject questionPanel;
    private TMP_Text questionText;
    private Button[] answerButtons;

    [Header("Questions Data")]
    private string questionsFilePath = "Questions";
    private List<Question> allQuestions = new List<Question>();
    private Dictionary<string, List<Question>> questionsByObjective = new Dictionary<string, List<Question>>();

    private string currentQuestionObjective = null;
    private int currentQuestionObjectiveIndex = -1;

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

    private void Start()
    {
        questionPanel = UIManager.Instance.questionsPanel;
        answerButtons = UIManager.Instance.question_answerButtons;
        questionText = UIManager.Instance.question_text;
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

    /// <summary>
    /// Initialize the current question objective
    /// </summary>
    public void InitializeCurrentQuestionObjective()
    {
        List<Objective> allObjectives = ObjectiveManager.Instance.GetAllObjectives();
        if (allObjectives == null || allObjectives.Count == 0)
        {
            currentQuestionObjective = null;
            currentQuestionObjectiveIndex = -1;
            return;
        }

        int incompleteIndex = allObjectives.FindIndex(o => o.status.ToLower() != "completed");

        if (incompleteIndex >= 0)
        {
            int index = Mathf.Max(0, incompleteIndex - 1);
            currentQuestionObjective = allObjectives[index].objective_name;
            currentQuestionObjectiveIndex = index;
        }
        else
        {
            currentQuestionObjective = null;
            currentQuestionObjectiveIndex = -1;
        }
    }

    /// <summary>
    /// Update the current question objective based on completed objectives
    /// </summary>
    public void UpdateCurrentQuestionObjective()
    {
        List<Objective> allObjectives = ObjectiveManager.Instance.GetAllObjectives();
        if (allObjectives == null || allObjectives.Count == 0)
        {
            currentQuestionObjective = null;
            currentQuestionObjectiveIndex = -1;
            return;
        }

        int incompleteIndex = allObjectives.FindIndex(o => o.status.ToLower() != "completed");

        if (incompleteIndex < 0)
        {
            currentQuestionObjectiveIndex = -1;
            currentQuestionObjective = null;
            return;
        }

        currentQuestionObjectiveIndex += 1;
        currentQuestionObjectiveIndex = Mathf.Min(currentQuestionObjectiveIndex, incompleteIndex);
        currentQuestionObjective = allObjectives[currentQuestionObjectiveIndex].objective_name;
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
        
        // Get the current objective for questions
        string currentObjective = GetCurrentQuestionObjective();
        
        if (string.IsNullOrEmpty(currentObjective))
        {
            Debug.Log("No objective available for questions");
            return;
        }
        
        Debug.Log($"Showing question for objective: {currentObjective}");
        
        // Find a random question for the current objective
        if (questionsByObjective.ContainsKey(currentObjective) && 
            questionsByObjective[currentObjective].Count > 0)
        {
            int randomIndex = Random.Range(0, questionsByObjective[currentObjective].Count);
            Question question = questionsByObjective[currentObjective][randomIndex];
            
            // Show the question
            ShowQuestion(question);
        }
        else
        {
            Debug.Log($"No questions available for objective: {currentObjective}");
        }
    }
    
    private void ShowQuestion(Question question)
    {
        currentQuestion = question;
        hasAnsweredCorrectly = false;
        
        // Pause the player
        if (Player.Instance != null)
            Player.Instance.pausePlayer();
            Player.Instance.stopInteraction();
        
        if(UIManager.Instance != null && UIManager.Instance.isMenuOpen())
            UIManager.Instance.setMenuOpen(false);
        
        // Show the question panel
        questionPanel.SetActive(true);
        
        // Set question text
        questionText.text = question.questionText;
        
        
        // Set up answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].GetComponentInChildren<TMP_Text>().text = question.answers[i];
                
                int answerIndex = i; // Need to capture for lambda
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        
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
        questionText.text = responseText;
        
        // Play sound based on result
        if (AudioController.Instance != null)
        {
            if (isCorrect)
                AudioController.Instance.PlayAnswerCorrect();
            else
                AudioController.Instance.PlayAnswerIncorrect();
        }
        
        // If answered correctly, advance to the next objective for questions
        if (isCorrect)
        {
            // This ensures we move to the next objective for questions
            UpdateCurrentQuestionObjective();
        }
        
        // Disable all answer buttons
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }
        
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
        
        // Reset the buttons
        foreach (Button button in answerButtons)
        {
            button.interactable = true;
        }
        
        // Hide the question panel
        questionPanel.SetActive(false);
        
        // Resume the player
        if (Player.Instance != null)
            Player.Instance.resumePlayer();
            Player.Instance.resumeInteraction();
            
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