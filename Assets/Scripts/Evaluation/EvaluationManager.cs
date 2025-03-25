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
    public string enemy_name;
    public string [] enemy_moves;
    public List<EvaluationQuestion> questions;
}

[System.Serializable]
public class EvaluationData
{
    public List<LevelQuestions> levels;
}

[System.Serializable]
public class CharacterBattleSprite
{
    public string characterName; // The name of the character
    public Sprite battleSprite; // The corresponding battle sprite
}

[System.Serializable]
public class EnemyBattleSprite
{
    public string characterName; // The name of the character
    public Sprite battleSprite; // The corresponding battle sprite
    public Sprite battleBackground;
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
    private GameObject power_up_panel;

    //background variables
    private Image battleBackground;
    private Image battleIntroBackground;

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
    private string npcName;
    private string playerName;
    private Sprite enemyImage;
    private Sprite PlayerImage;

    public List<CharacterBattleSprite> characterBattleSprites;
    private Dictionary<string, Sprite> characterBattleSpritesDict;
    public List<EnemyBattleSprite> enemyBattleSprites;
    private Dictionary<string, Sprite> enemyBattleSpritesDict;
    private Dictionary<string, Sprite> enemyBackgroundSpritesDict;
    private float typingSpeed = 0.05f;

    private string levelName;

    private float playerHealth = 1f; // Health is represented as a scale (1 = full health)
    private float enemyHealth = 1f; // Health is represented as a scale (1 = full health)
    private float healthDecreaseAmount = 0.25f; // Amount of health to decrease per turn
    private List<EvaluationQuestion> currentQuestions; // Store the loaded questions
    private int currentQuestionIndex = 0; // Track the current question
    private Button [] power_up_buttons;
    private bool isEnemyTurn;
    private TMP_Text eval_time;
    public static EvaluationManager Instance;

    private string [] npcMoves;
    private string [] playerMoves;

    private float questionTimeLimit = 30f; // Time limit in seconds
    private float currentQuestionTime; // Current remaining time
    private Coroutine timerCoroutine; // Reference to manage the timer coroutine

    public bool isEvaluating;

    private bool isTyping;

    private int numCorrectAnswers;
    private int numQuestions=5;
     private void Awake()
    {
         if (Instance == null)
         {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object between scenes
         }
        else
        {
            Destroy(gameObject);
        }
        
        
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
        power_up_buttons = UIManager.Instance.power_up_buttons;
        eval_time = UIManager.Instance.eval_time;
        power_up_panel = UIManager.Instance.power_up_panel;
        battleBackground = UIManager.Instance.battleBackground;
        battleIntroBackground = UIManager.Instance.battleIntroBackground;
    }

    public void initializeBattleSprites(){
        characterBattleSpritesDict = new Dictionary<string, Sprite>();
        foreach(CharacterBattleSprite character in characterBattleSprites){
            characterBattleSpritesDict[character.characterName] = character.battleSprite;
        }
        enemyBattleSpritesDict = new Dictionary<string, Sprite>();
        foreach(EnemyBattleSprite enemy in enemyBattleSprites){
            enemyBattleSpritesDict[enemy.characterName] = enemy.battleSprite;
        }
        enemyBackgroundSpritesDict = new Dictionary<string, Sprite>();
        foreach(EnemyBattleSprite enemy in enemyBattleSprites){
            enemyBackgroundSpritesDict[enemy.characterName] = enemy.battleBackground;
        }
        enemyImage = enemyBattleSpritesDict[npcName];
        battleBackground.sprite = enemyBackgroundSpritesDict[npcName];
        battleIntroBackground.sprite = enemyBackgroundSpritesDict[npcName];
        PlayerImage = characterBattleSpritesDict[DatabaseManager.Instance.loggedInUser.equippedCharacter];
    }

    public void LoadQuestionsForLevel()
    {
        playerName = DatabaseManager.Instance.loggedInUser.username;
        levelName = DatabaseManager.Instance.loggedInUser.courseStructure.chapters[DatabaseManager.Instance.loggedInUser.currentChapter].levels[DatabaseManager.Instance.loggedInUser.currentLevel-1].level_name;
        playerMoves = DatabaseManager.Instance.loggedInUser.playerMoves;
        string filePath = Path.Combine(Application.streamingAssetsPath, "EvaluationQuestions.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"EvaluationQuestions.json file not found at: {filePath}");
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
            }

            // Find the level by name
            LevelQuestions level = evaluationData.levels.Find(l => l.level_name == levelName);

            if (level == null)
            {
                Debug.LogWarning($"No questions found for level: {levelName}");

            }

            npcName= level.enemy_name;
            npcMoves = level.enemy_moves;
            // Return the list of questions for the level
            currentQuestions=level.questions;
            
            initializeBattleSprites();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading questions for level {levelName}: {ex.Message}");
        }
    }


    public void StartEvaluation(){
        numCorrectAnswers=0;
        isEvaluating = true;
        Player.Instance.pausePlayer();
        // Load the questions for the level

        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogError("No questions found for the level.");
            return;
        }

        // Reset current question index and health
        currentQuestionIndex = 0;
        playerHealth = 1f;
        enemyHealth = 1f;
        
        // Check purchased boost items and set up power-up buttons
        SetupPowerUpButtons();


        // Start the evaluation sequence
        StartCoroutine(evaluationSequence());
        
    }


    private void SetupPowerUpButtons()
    {
        // Default: disable all power-up buttons
        foreach (Button button in power_up_buttons)
        {
            button.interactable = false;
        }
        
        // Check if user is logged in
        if (DatabaseManager.Instance.loggedInUser == null || 
            DatabaseManager.Instance.loggedInUser.purchasedItems == null)
        {
            Debug.LogWarning("No user logged in or no purchased items available");
            return;
        }
        
        // Get all purchased boost items
        List<UserItem> boostItems = new List<UserItem>();
        foreach (UserItem item in DatabaseManager.Instance.loggedInUser.purchasedItems)
        {
            if (item.item_type == "Boost")
            {
                boostItems.Add(item);
            }
        }
        
        // Check for each specific boost and enable the corresponding button
        foreach (UserItem boostItem in boostItems)
        {
            switch (boostItem.item_name)
            {
                case "Extra Time":
                    if (power_up_buttons.Length > 0)
                        power_up_buttons[0].interactable = true;
                    break;
                    
                case "Power Reveal":
                    if (power_up_buttons.Length > 1)
                        power_up_buttons[1].interactable = true;
                    break;
                    
                case "Hint Token":
                    if (power_up_buttons.Length > 2)
                        power_up_buttons[2].interactable = true;
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown boost item: {boostItem.item_name}");
                    break;
            }
        }
        
        Debug.Log($"Enabled {boostItems.Count} power-up buttons based on purchased items");
    }

    public IEnumerator NotReady(){
        isEvaluating = true;
        evaluationPanel.SetActive(true);
        battlePanel.SetActive(false);
        npcIntroPanel.SetActive(true);
        npcIntroImage.sprite = enemyImage;
        yield return StartCoroutine(TypeText(npcIntroNameText, "You are not ready to battle me yet!", typingSpeed));
        yield return new WaitForSeconds(2f);
        evaluationPanel.SetActive(false);
        npcIntroPanel.SetActive(false);
        isEvaluating=false;
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


    // Power-up: Remove two wrong answers
    public void GiveHint()
    {
        // Check if we're in a valid state to use power-up
        if (currentQuestionIndex >= currentQuestions.Count || !optionPanel.activeSelf)
            return;
        
        // Disable the power-up button to prevent multiple uses
        if (power_up_buttons.Length > 2)
            power_up_buttons[2].interactable = false;

        EvaluationQuestion currentQuestion = currentQuestions[currentQuestionIndex];
        int correctIndex = currentQuestion.correctIndex;
        
        // Find all wrong answers
        List<int> wrongAnswerIndexes = new List<int>();
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i != correctIndex && answerButtons[i].gameObject.activeSelf)
            {
                wrongAnswerIndexes.Add(i);
            }
        }
        
        // Randomly remove two wrong answers if possible
        if (wrongAnswerIndexes.Count >= 2)
        {
            // Shuffle the list
            for (int i = 0; i < wrongAnswerIndexes.Count; i++)
            {
                int temp = wrongAnswerIndexes[i];
                int randomIndex = Random.Range(0, wrongAnswerIndexes.Count);
                wrongAnswerIndexes[i] = wrongAnswerIndexes[randomIndex];
                wrongAnswerIndexes[randomIndex] = temp;
            }
            
            // Disable the first two wrong answers
            answerButtons[wrongAnswerIndexes[0]].gameObject.SetActive(false);
            answerButtons[wrongAnswerIndexes[1]].gameObject.SetActive(false);
        }


        
        // Remove the item from the user's inventory
        DatabaseManager.Instance.RemoveUserItem("Hint Token", (success) => {
            if (success)
                Debug.Log("Hint Token item used and removed from inventory");
            else
                Debug.LogError("Failed to remove Hint Token item from inventory");
        });
        
    }

    // Power-up: Add extra time (30 seconds)
    public void AddExtraTime()
    {
        // Check if we're in a valid state to use power-up
        if (!optionPanel.activeSelf || timerCoroutine == null)
            return;

        // Disable the power-up button to prevent multiple uses
        if (power_up_buttons.Length > 0)
            power_up_buttons[0].interactable = false;
            
        // Add 30 seconds to the current time
        currentQuestionTime += 30f;
        questionTimeLimit = 60f;
        
        // Update the timer display immediately
        eval_time.text = Mathf.CeilToInt(currentQuestionTime).ToString();
        
        
        // Remove the item from the user's inventory
        DatabaseManager.Instance.RemoveUserItem("Extra Time", (success) => {
            if (success)
                Debug.Log("Extra Time item used and removed from inventory");
            else
                Debug.LogError("Failed to remove Extra Time item from inventory");
        });
    }

    // Power-up: Reveal the correct answer (disables all wrong answers)
    public void RevealAnswer()
    {
        // Check if we're in a valid state to use power-up
        if (currentQuestionIndex >= currentQuestions.Count || !optionPanel.activeSelf)
            return;

        // Disable the power-up button to prevent multiple uses
        if (power_up_buttons.Length > 1)
            power_up_buttons[1].interactable = false;
        
        EvaluationQuestion currentQuestion = currentQuestions[currentQuestionIndex];
        int correctIndex = currentQuestion.correctIndex;
        
        // Disable all wrong answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i != correctIndex)
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        
        
        // Remove the item from the user's inventory
        DatabaseManager.Instance.RemoveUserItem("Power Reveal", (success) => {
            if (success)
                Debug.Log("Power Reveal item used and removed from inventory");
            else
                Debug.LogError("Failed to remove Power Reveal item from inventory");
        });
    }

    private IEnumerator QuestionTimerCoroutine()
    {
        currentQuestionTime = questionTimeLimit;
        
        while (currentQuestionTime > 0)
        {
            // Update the timer display
            eval_time.text = Mathf.CeilToInt(currentQuestionTime).ToString();
            
            // Wait for a small interval
            yield return new WaitForSeconds(0.1f);
            
            // Decrease time
            currentQuestionTime -= 0.1f;
        }
        
        // Time's up - handle as incorrect answer
        TimeUp();
        
        yield break;
    }

    private void TimeUp()
    {
        // Stop the timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // Disable answer buttons
        optionPanel.SetActive(false);
        power_up_panel.SetActive(false);
        eval_time.text="";
        // Handle the timeout as an incorrect answer
        if (isEnemyTurn)
        {
            // If enemy is attacking, player health decreases
            playerHealth -= (healthDecreaseAmount*2);
            playerHealth= Mathf.Clamp(playerHealth, 0f, 1f);
            playerHealthBar.localScale = new Vector3(playerHealth, 1f, 1f);
            
            StartCoroutine(TimeUpSequence("Time's up! The enemy's attack was successful!"));
        }
        else
        {
            // If player is attacking, just mark it as a miss
            StartCoroutine(TimeUpSequence("Time's up! Your attack missed and the enemy countered!"));
            playerHealth -= healthDecreaseAmount;
            playerHealthBar.localScale = new Vector3(playerHealth, 1f, 1f);
        }
    }


    private IEnumerator TimeUpSequence(string message)
    {
        yield return StartCoroutine(TypeText(dialogueText, message, typingSpeed));
        yield return new WaitForSeconds(1f);
        eval_time.text="";
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
            if (enemyHealth <= 0 || playerHealth <= 0)
            {
                EndBattle();
            }
            else
            {
                StartCoroutine(ShowQuestionCoroutine(currentQuestions[currentQuestionIndex]));
            }
        }
    }

    private IEnumerator TypeText(TMP_Text textObject, string message, float typingSpeed)
    {   
        isTyping = true;
        textObject.text = ""; // Clear the text before typing
        foreach (char letter in message.ToCharArray())
        {
            textObject.text += letter; // Add one character at a time
            yield return new WaitForSeconds(typingSpeed); // Wait before adding the next character
        }

        isTyping = false;
    }

    private IEnumerator ShowQuestionCoroutine(EvaluationQuestion question)
    {
        optionPanel.SetActive(true);
        power_up_panel.SetActive(true);
        // Start typing the question
        StartCoroutine(TypeText(dialogueText, question.question + "\n\n What will " + playerName + " do?", typingSpeed));

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.options.Length)
            {
                TMP_Text answerText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                answerText.text = playerMoves[i];
                int capturedIndex = i;
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => StartCoroutine(OnAnswerSelectedCoroutine(capturedIndex, question, playerMoves[capturedIndex])));

                 AddHoverListeners(answerButtons[capturedIndex], capturedIndex);

                
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // Start the timer after setting up the question
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(QuestionTimerCoroutine());

        yield break;
    }

    private IEnumerator OnAnswerSelectedCoroutine(int index, EvaluationQuestion question, string text)
    {
        // Stop the timer when an answer is selected
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
            eval_time.text="";
        }

        optionPanel.SetActive(false);
        power_up_panel.SetActive(false);
        yield return StartCoroutine(TypeText(dialogueText, playerName + " used " + text + "!", typingSpeed));
        yield return new WaitForSeconds(1f);
        if (index == question.correctIndex)
        {
            // Player answered correctly
            numCorrectAnswers++;
            enemyHealth -= healthDecreaseAmount;
            enemyHealth= Mathf.Clamp(enemyHealth, 0f, 1f);
            npcHealthBar.localScale = new Vector3(enemyHealth, 1f, 1f);
            AchievementManager.Instance.TrackAnswer(true);
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
            playerHealth -= (healthDecreaseAmount*2);
            playerHealth= Mathf.Clamp(playerHealth, 0f, 1f);
            playerHealthBar.localScale = new Vector3(playerHealth, 1f, 1f);
            AchievementManager.Instance.TrackAnswer(false);
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
                StartCoroutine(EndBattle());
            }
            else
            {
                StartCoroutine(ShowQuestionCoroutine(currentQuestions[currentQuestionIndex]));
            }
        }
    }

    private IEnumerator EnemyTurn()
    {
        if(enemyHealth <= 0 || playerHealth <= 0){
            yield return StartCoroutine(EndBattle());
            yield break;
        }

        else
        {
             yield return new WaitForSeconds(2f); // Wait before the enemy's turn
            EvaluationQuestion question = currentQuestions[currentQuestionIndex];

            
            int randomIndex = Random.Range(0, 4);
            yield return StartCoroutine(TypeText(dialogueText, npcName + " used " + npcMoves[randomIndex] + "\n Answer correctly to block!", typingSpeed));
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(ShowQuestionCoroutine(question));
        }
    }

    IEnumerator EndBattle()
    {
        optionPanel.SetActive(false);
        power_up_panel.SetActive(false);
        DatabaseManager.Instance.loggedInUser.setLevelScore(((float)numCorrectAnswers/numQuestions) * 100);
        bool hasFailed= playerHealth <= 0;
        if (enemyHealth <= 0)
        {
            yield return StartCoroutine(TypeText(dialogueText, "You defeated the enemy! Well done!", typingSpeed));
            
        }
        else if (playerHealth <= 0)
        {
            yield return StartCoroutine(TypeText(dialogueText, "You were defeated. Better luck next time!", typingSpeed));
        }

        DatabaseManager.Instance.CompleteLevel(hasFailed, (success) => {
            if (success)
            {
                Debug.Log("Level completed successfully");
            }
            else
            {
                Debug.LogError("Failed to complete level");
            }
        });
        
        yield return new WaitForSeconds(2f);
        battlePanel.SetActive(false);
        evaluationPanel.SetActive(false);
        isEvaluating = false;
        

        Player.Instance.resumePlayer();

        AchievementManager.Instance.CheckAchievements(levelName, DatabaseManager.Instance.loggedInUser.getLevelTime(), numQuestions, numCorrectAnswers, hasFailed);

         // Wait until achievement processing is complete
        yield return StartCoroutine(WaitForAchievementProcessing());
        
        // Now show the completion/failure UI
        if(hasFailed){
            UIManager.Instance.showFailedLevel();
        }
        else{
            UIManager.Instance.showCompletedLevel();
        }
    }

    // Helper coroutine to wait for achievement processing
    private IEnumerator WaitForAchievementProcessing()
    {
        // Add a small delay to ensure achievement check has started
        yield return new WaitForSeconds(0.5f);
        
        // Wait until achievement manager is no longer processing
        while (AchievementManager.Instance != null && 
            AchievementManager.Instance.isProcessingAchievements)
        {
            yield return null; // Wait one frame
        }
        
        // Add a small buffer after processing completes
        yield return new WaitForSeconds(0.5f);
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
        if(!isTyping)
        {
            dialogueText.text = currentQuestions[currentQuestionIndex].options[i];
        }
    }

    private void OnHoverExit(Button button)
    {
        if(!isTyping)
        {
            dialogueText.text = currentQuestions[currentQuestionIndex].question + "\n\n What will " + playerName + " do?";
        }
    }


}