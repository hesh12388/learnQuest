using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.EventSystems;

[System.Serializable]
public class EvaluationQuestion
{
    public string question;
    public string[] options;
    public int correctIndex;
}

[System.Serializable]
public class LevelQuestions
{
    public string level_name;
    public List<EvaluationQuestion> questions;
}

[System.Serializable]
public class EvaluationData
{
    public List<LevelQuestions> levels;
}
public class EvaluationManager : MonoBehaviour{

    private Image playerImage;
    private Image npcImage;
    private TMP_Text dialogueText;
    private Button[] answerButtons;
    
    private GameObject evaluationPanel;
    private GameObject battlePanel;
    private GameObject npcIntroPanel;
    private GameObject optionPanel;

    //player hud variables
    private TMP_Text playerNameText;
    private TMP_Text playerLevelText;
    private Transform playerHealthBar;

    //npc hud variables
    private TMP_Text npcNameText;
    private TMP_Text npcLevelText;
    private Transform npcHealthBar;

    //intro panel variables
    private Image npcIntroImage;
    private TMP_Text npcIntroNameText;

    //placeholder variables for now
    public string npcName;
    public string playerName;
    public Sprite enemyImage;
    public Sprite PlayerImage;

    private float typingSpeed = 0.05f;

    private string levelName = "CSS Box Model";

    private float playerHealth = 1f; // Health is represented as a scale (1 = full health)
    private float enemyHealth = 1f; // Health is represented as a scale (1 = full health)
    private float healthDecreaseAmount = 0.25f; // Amount of health to decrease per turn
    private List<EvaluationQuestion> currentQuestions; // Store the loaded questions
    private int currentQuestionIndex = 0; // Track the current question

    private bool isEnemyTurn;

    public static EvaluationManager Instance;

    public string [] npcMoves;
    public string [] playerMoves;

     private void Awake()
    {
         if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        
    }

    private void Start(){
        playerImage = UIManager.Instance.playerImage;
        npcImage = UIManager.Instance.eval_npcImage;
        dialogueText = UIManager.Instance.eval_dialogueText;
        answerButtons = UIManager.Instance.answerButtons;
        evaluationPanel = UIManager.Instance.evaluationPanel;
        battlePanel = UIManager.Instance.battlePanel;
        npcIntroPanel = UIManager.Instance.npcIntroPanel;
        optionPanel = UIManager.Instance.optionPanel;
        playerNameText = UIManager.Instance.playerNameText;
        playerLevelText = UIManager.Instance.playerLevelText;
        playerHealthBar = UIManager.Instance.playerHealthBar;
        npcNameText = UIManager.Instance.npcNameText;
        npcLevelText = UIManager.Instance.npcLevelText;
        npcHealthBar = UIManager.Instance.npcHealthBar;
        npcIntroImage = UIManager.Instance.npcIntroImage;
        npcIntroNameText = UIManager.Instance.npcIntroNameText;
    }


    public List<EvaluationQuestion> LoadQuestionsForLevel()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "EvaluationQuestions.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"EvaluationQuestions.json file not found at: {filePath}");
            return null;
        }

        try
        {
            // Read the JSON file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON into the EvaluationData structure
            EvaluationData evaluationData = JsonConvert.DeserializeObject<EvaluationData>(json);

            if (evaluationData == null || evaluationData.levels == null)
            {
                Debug.LogError("Failed to parse EvaluationQuestions.json or no levels found.");
                return null;
            }

            // Find the level by name
            LevelQuestions level = evaluationData.levels.Find(l => l.level_name == levelName);

            if (level == null)
            {
                Debug.LogWarning($"No questions found for level: {levelName}");
                return null;
            }

            // Return the list of questions for the level
            return level.questions;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading questions for level {levelName}: {ex.Message}");
            return null;
        }
    }


    public void StartEvaluation(){
        // Load the questions for the level
        currentQuestions= LoadQuestionsForLevel();

        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogError("No questions found for the level.");
            return;
        }

        // Start the evaluation sequence
        StartCoroutine(evaluationSequence());
        
    }



    private IEnumerator evaluationSequence(){

        yield return StartCoroutine(TransitionManager.Instance.contentTransition());

        // show the npc intro panel
        evaluationPanel.SetActive(true);
        npcIntroPanel.SetActive(true);
        npcIntroImage.sprite = enemyImage;
        yield return StartCoroutine(TypeText(npcIntroNameText, npcName + " has appeared!", typingSpeed));
        yield return new WaitForSeconds(2f);

        //show the battle panel
        yield return StartCoroutine(TransitionManager.Instance.contentTransition());
        npcIntroPanel.SetActive(false);
        battlePanel.SetActive(true);
        npcNameText.text = npcName;
        npcLevelText.text = "Lvl 1";
        playerNameText.text = playerName;
        playerLevelText.text = "Lvl 1";
        playerImage.sprite = PlayerImage;
        npcImage.sprite = enemyImage;

        // Start the first question
        StartCoroutine(ShowQuestionCoroutine(currentQuestions[currentQuestionIndex]));
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

    private IEnumerator ShowQuestionCoroutine(EvaluationQuestion question)
    {
        optionPanel.SetActive(true);
        // Start typing the question
        yield return StartCoroutine(TypeText(dialogueText, question.question + "\n\n What will " + playerName + " do?", typingSpeed));

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.options.Length)
            {
                TMP_Text answerText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                answerText.text = playerMoves[i];
                int capturedIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => StartCoroutine(OnAnswerSelectedCoroutine(capturedIndex, question, playerMoves[capturedIndex])));

                 AddHoverListeners(answerButtons[capturedIndex], capturedIndex);

                
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        yield break;
    }

    private IEnumerator OnAnswerSelectedCoroutine(int index, EvaluationQuestion question, string text)
    {

        optionPanel.SetActive(false);
        yield return StartCoroutine(TypeText(dialogueText, playerName + " used " + text + "!", typingSpeed));
        yield return new WaitForSeconds(1f);
        if (index == question.correctIndex)
        {
            // Player answered correctly
            enemyHealth -= healthDecreaseAmount;
            npcHealthBar.localScale = new Vector3(enemyHealth, 1f, 1f);

            if (isEnemyTurn)
            {
                yield return StartCoroutine(TypeText(dialogueText, "Correct! You blocked the enemy's attack and countered to deal damage!", typingSpeed));
            }
            else
            {
                yield return StartCoroutine(TypeText(dialogueText, "Correct! You dealt damage to the enemy!", typingSpeed));
            }
        }
        else
        {
            playerHealth -= healthDecreaseAmount;
            playerHealthBar.localScale = new Vector3(playerHealth, 1f, 1f);
            // Player answered incorrectly
            if (isEnemyTurn)
            {
                yield return StartCoroutine(TypeText(dialogueText, "Incorrect! The enemy attacked successfully!", typingSpeed));
               
            }
            else
            {
                yield return StartCoroutine(TypeText(dialogueText, "Incorrect! You missed your attack and the enemy has countered to deal damage!", typingSpeed));
            }
        }

        yield return new WaitForSeconds(1f); // Add a small delay before the next turn
        // Toggle the turn
        isEnemyTurn = !isEnemyTurn;
        currentQuestionIndex++;
        // Start the next turn
        if (isEnemyTurn)
        {
            StartCoroutine(EnemyTurn());
        }
        else
        {
            if(enemyHealth <= 0 || playerHealth <= 0){
                EndBattle();
            }
            else
            {
                StartCoroutine(ShowQuestionCoroutine(currentQuestions[currentQuestionIndex]));
            }
        }
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(2f); // Wait before the enemy's turn


        if(enemyHealth <= 0 || playerHealth <= 0){
            EndBattle();
        }
        
        else
        {
            EvaluationQuestion question = currentQuestions[currentQuestionIndex];

            
            int randomIndex = Random.Range(0, 4);
            yield return StartCoroutine(TypeText(dialogueText, npcName + " used " + npcMoves[randomIndex] + "\n Answer correctly to block!", typingSpeed));
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(ShowQuestionCoroutine(question));
        }
    }

    void EndBattle()
    {
        optionPanel.SetActive(false);
        if (enemyHealth <= 0)
        {
            StartCoroutine(TypeText(dialogueText, "You defeated the enemy! Well done!", typingSpeed));
        }
        else if (playerHealth <= 0)
        {
            StartCoroutine(TypeText(dialogueText, "You were defeated. Better luck next time!", typingSpeed));
        }
    }

    // Function to Add Hover Listeners
    private void AddHoverListeners(Button button, int i)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter (Hover Start)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => OnHoverEnter(button, i));
        trigger.triggers.Add(entryEnter);

        // Pointer Exit (Hover End)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => OnHoverExit(button));
        trigger.triggers.Add(entryExit);
    }

    // Hover Event Handlers
    private void OnHoverEnter(Button button, int i)
    {
        dialogueText.text = currentQuestions[currentQuestionIndex].options[i];
    }

    private void OnHoverExit(Button button)
    {
        dialogueText.text = currentQuestions[currentQuestionIndex].question + "\n\n What will " + playerName + " do?";
    }


}