using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

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


public class EvaluationManager : MonoBehaviour
{
    public static EvaluationManager Instance;
    
    [Header("Battle Sprites")]
    public List<CharacterBattleSprite> characterBattleSprites;
    public List<EnemyBattleSprite> enemyBattleSprites;
    
    // Private state variables
    private Dictionary<string, Sprite> characterBattleSpritesDict;
    private Dictionary<string, Sprite> enemyBattleSpritesDict;
    private Dictionary<string, Sprite> enemyBackgroundSpritesDict;
    private float typingSpeed = 0.05f;
    private string npcName;
    private string playerName;
    private string levelName;
    private Sprite enemyImage;
    private Sprite PlayerImage;
    private string[] npcMoves;
    private string[] playerMoves;
    private List<EvaluationQuestion> currentQuestions;
    private int currentQuestionIndex = 0;
    private float playerHealth = 1f;
    private float enemyHealth = 1f;
    private float healthDecreaseAmount = 0.25f;
    private float questionTimeLimit = 30f;
    private float currentQuestionTime;
    private Coroutine timerCoroutine;
    private bool isEnemyTurn;
    public bool isEvaluating;
    private bool isTyping;
    private int numCorrectAnswers;
    private int numQuestions = 0;
    private bool isPaused = false;
    private float tooltipDelay = 5f; // Time in seconds before showing the tooltip
    private Coroutine tooltipCoroutine; // Reference to track the tooltip timing
    private bool isTooltipShowing = false;
    
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
        }
    }
    
    public void initializeBattleSprites()
    {
        characterBattleSpritesDict = new Dictionary<string, Sprite>();
        foreach (CharacterBattleSprite character in characterBattleSprites)
        {
            characterBattleSpritesDict[character.characterName] = character.battleSprite;
        }
        
        enemyBattleSpritesDict = new Dictionary<string, Sprite>();
        enemyBackgroundSpritesDict = new Dictionary<string, Sprite>();
        foreach (EnemyBattleSprite enemy in enemyBattleSprites)
        {
            enemyBattleSpritesDict[enemy.characterName] = enemy.battleSprite;
            enemyBackgroundSpritesDict[enemy.characterName] = enemy.battleBackground;
        }
    
    }
    
    public void LoadQuestionsForLevel()
    {
        playerName = DatabaseManager.Instance.loggedInUser.username;
        levelName = DatabaseManager.Instance.loggedInUser.courseStructure.chapters[DatabaseManager.Instance.loggedInUser.currentChapter].levels[DatabaseManager.Instance.loggedInUser.currentLevel-1].level_name;
        
        // Load questions from Resources
        TextAsset jsonAsset = Resources.Load<TextAsset>("EvaluationQuestions");
        if (jsonAsset == null)
        {
            Debug.LogError("EvaluationQuestions.json not found in Resources folder!");
            return;
        }
        
        string json = jsonAsset.text;
        
        try
        {
            // Deserialize the JSON into the EvaluationData structure
            EvaluationData evaluationData = JsonConvert.DeserializeObject<EvaluationData>(json);
            
            if (evaluationData == null || evaluationData.levels == null)
            {
                Debug.LogError("Failed to parse EvaluationQuestions.json or no levels found.");
                return;
            }
            
            // Find the level by name
            LevelQuestions level = evaluationData.levels.Find(l => l.level_name == levelName);
            
            if (level == null)
            {
                Debug.LogWarning($"No questions found for level: {levelName}");
                return;
            }
            
            npcName = level.enemy_name;
            npcMoves = level.enemy_moves;
            currentQuestions = level.questions;
            
            initializeBattleSprites();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading questions for level {levelName}: {ex.Message}");
        }
    }
    
    public void StartEvaluation()
    {
        enemyImage = enemyBattleSpritesDict[npcName];
        PlayerImage = characterBattleSpritesDict[DatabaseManager.Instance.loggedInUser.equippedCharacter];
        playerMoves = DatabaseManager.Instance.loggedInUser.playerMoves;
        UIManager.Instance.disablePlayerHUD();
        numCorrectAnswers = 0;
        numQuestions = 0;
        isEvaluating = true;
        Player.Instance.pausePlayer();
        questionTimeLimit = 30f;
        
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

        // Track a level evaluation attempt
        User loggedInUser = DatabaseManager.Instance.loggedInUser;
        string level_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1].level_name;
        string chapter_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].chapter_name;
        DatabaseManager.Instance.UpdateMetric("level_evaluation", level_name + "|" + chapter_name);
        
        // Start the evaluation sequence
        StartCoroutine(evaluationSequence());
    }
    
    private void SetupPowerUpButtons()
    {
        // Default: disable all power-up buttons
        for (int i = 0; i < 3; i++)
        {
            UIManager.Instance.SetupPowerUpButton(i, false);
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
                    UIManager.Instance.SetupPowerUpButton(0, true);
                    break;
                    
                case "Power Reveal":
                    UIManager.Instance.SetupPowerUpButton(1, true);
                    break;
                    
                case "Hint Token":
                    UIManager.Instance.SetupPowerUpButton(2, true);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown boost item: {boostItem.item_name}");
                    break;
            }
        }

        SetupPowerUpTooltip(0, "Extra Time");
        SetupPowerUpTooltip(1, "Power Reveal");
        SetupPowerUpTooltip(2, "Hint Token");
        Debug.Log($"Enabled {boostItems.Count} power-up buttons based on purchased items");
    }

    //method to set up tooltips
    private void SetupPowerUpTooltip(int buttonIndex, string powerUpName)
    {
        if (buttonIndex >= 0 && buttonIndex < UIManager.Instance.power_up_buttons.Length)
        {
            Button button = UIManager.Instance.power_up_buttons[buttonIndex];
            
            // Add event trigger component if it doesn't exist
            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            }
            
            // Create pointer enter event
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { UIManager.Instance.showPowerUpTooltip(powerUpName); });
            eventTrigger.triggers.Add(enterEntry);
            
            // Create pointer exit event
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { UIManager.Instance.hidePowerUpTooltip(); });
            eventTrigger.triggers.Add(exitEntry);
            
            // Also hide tooltip when clicking
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => { UIManager.Instance.hidePowerUpTooltip(); });
            eventTrigger.triggers.Add(clickEntry);
        }
    }

    // method to clear tooltip events when needed
    public void ClearPowerUpTooltips()
    {
        for (int i = 0; i < UIManager.Instance.power_up_buttons.Length; i++)
        {
            Button button = UIManager.Instance.power_up_buttons[i];
            
            // Get and clear event trigger
            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger != null)
            {
                eventTrigger.triggers.Clear();
            }
        }
    }
        
    public IEnumerator NotReady()
    {
        enemyImage = enemyBattleSpritesDict[npcName];
        UIManager.Instance.disablePlayerHUD();
        AudioController.Instance.PlayBattleMusic();
        Player.Instance.pausePlayer();
        Player.Instance.stopInteraction();
        isEvaluating = true;
        
        UIManager.Instance.ShowEvaluationPanel(true);
        UIManager.Instance.ShowBattlePanel(false);
        UIManager.Instance.ShowNPCIntroPanel(true);
        UIManager.Instance.SetNPCIntroImage(enemyImage);
        
        yield return StartCoroutine(UIManager.Instance.TypeIntroText("You are not ready to battle me yet!", typingSpeed));
        yield return new WaitForSeconds(2f);
        
        UIManager.Instance.ShowEvaluationPanel(false);
        UIManager.Instance.ShowNPCIntroPanel(false);
        isEvaluating = false;
        AudioController.Instance.PlayBackgroundMusic();
        UIManager.Instance.enablePlayerHUD();
        Player.Instance.resumePlayer();
        Player.Instance.resumeInteraction();
    }
    
    private IEnumerator evaluationSequence()
    {
        yield return StartCoroutine(TransitionManager.Instance.contentTransition());
        AudioController.Instance.PlayBattleMusic();
        
        // Show the npc intro panel
        UIManager.Instance.ShowEvaluationPanel(true);
        UIManager.Instance.ShowNPCIntroPanel(true);
        UIManager.Instance.SetNPCIntroImage(enemyImage);
        
        yield return StartCoroutine(UIManager.Instance.TypeIntroText(npcName + " has appeared!", typingSpeed));
        yield return new WaitForSeconds(2f);
        
        // Show the battle panel
        yield return StartCoroutine(TransitionManager.Instance.contentTransition());
        UIManager.Instance.ShowNPCIntroPanel(false);
        UIManager.Instance.ShowBattlePanel(true);
        
        // Setup the battle UI
        UIManager.Instance.SetupEvaluationUI(
            playerName, 
            npcName, 
            PlayerImage, 
            enemyImage, 
            enemyBackgroundSpritesDict[npcName]
        );
        
        // Start the first question
        StartCoroutine(ShowQuestionCoroutine(currentQuestions[currentQuestionIndex]));
    }
    
    // Power-up: Remove two wrong answers
    public void GiveHint()
    {
        // Check if we're in a question state
        if (isPaused || timerCoroutine == null || currentQuestionIndex >= currentQuestions.Count)
        {
            Debug.LogWarning("Cannot use hint - not in an active question state");
            return;
        }
        
        // Disable the power-up button to prevent multiple uses
        UIManager.Instance.SetupPowerUpButton(2, false);
        
        // Get the current question
        EvaluationQuestion question = currentQuestions[currentQuestionIndex];
        
        // Identify wrong answer indices
        List<int> wrongAnswerIndices = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (i != question.correctIndex)
            {
                wrongAnswerIndices.Add(i);
            }
        }
        
        // Shuffle and select two wrong answers to disable
        if (wrongAnswerIndices.Count >= 2)
        {
            // Shuffle the wrong answers
            for (int i = wrongAnswerIndices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = wrongAnswerIndices[i];
                wrongAnswerIndices[i] = wrongAnswerIndices[j];
                wrongAnswerIndices[j] = temp;
            }
            
            // Disable two wrong answers through UIManager
            for (int i = 0; i < 2; i++)
            {
                int idx = wrongAnswerIndices[i];
                UIManager.Instance.answerButtons[idx].gameObject.SetActive(false);
            }
        }
        
        // Remove the item from the user's inventory
        DatabaseManager.Instance.RemoveUserItem("Hint Token", (success) => {
            if (success)
                Debug.Log("Hint Token used and removed from inventory");
            else
                Debug.LogError("Failed to remove Hint Token from inventory");
        });
    }

    
    // Power-up: Add extra time (30 seconds)
    public void AddExtraTime()
    {
        // Check if we're in a valid state to use power-up
        if (!isPaused && timerCoroutine != null)
        {
            // Disable the power-up button to prevent multiple uses
            UIManager.Instance.SetupPowerUpButton(0, false);
                
            // Add 30 seconds to the current time
            currentQuestionTime += 30f;
            questionTimeLimit = 60f;
            
            // Update the timer display immediately
            UIManager.Instance.UpdateTimerText(currentQuestionTime);
            
            // Remove the item from the user's inventory
            DatabaseManager.Instance.RemoveUserItem("Extra Time", (success) => {
                if (success)
                    Debug.Log("Extra Time item used and removed from inventory");
                else
                    Debug.LogError("Failed to remove Extra Time item from inventory");
            });
        }
    }
    
    // Power-up: Reveal the correct answer (disables all wrong answers)
    public void RevealAnswer()
    {
        // Check if we're in a question state
        if (isPaused || timerCoroutine == null || currentQuestionIndex >= currentQuestions.Count)
        {
            Debug.LogWarning("Cannot use power reveal - not in an active question state");
            return;
        }
        
        // Disable the power-up button to prevent multiple uses
        UIManager.Instance.SetupPowerUpButton(1, false);
        
        // Get the current question
        EvaluationQuestion question = currentQuestions[currentQuestionIndex];
        
        // Disable all wrong answers through UIManager
        for (int i = 0; i < 4; i++)
        {
            if (i != question.correctIndex)
            {
                UIManager.Instance.answerButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Remove the item from the user's inventory
        DatabaseManager.Instance.RemoveUserItem("Power Reveal", (success) => {
            if (success)
                Debug.Log("Power Reveal used and removed from inventory");
            else
                Debug.LogError("Failed to remove Power Reveal from inventory");
        });
    }
    
    // QuestionTimerCoroutine to handle the countdown
    private IEnumerator QuestionTimerCoroutine()
    {
        currentQuestionTime = questionTimeLimit;
        
        while (currentQuestionTime > 0)
        {
            // Only decrease time if not paused
            if (!isPaused)
            {
                // Update the timer display
                UIManager.Instance.UpdateTimerText(currentQuestionTime);
                
                // Decrease time
                currentQuestionTime -= 0.1f;
            }
            
            // Wait for a small interval
            yield return new WaitForSeconds(0.1f);
        }
        
        // Time's up - handle as incorrect answer
        TimeUp();
        
        yield break;
    }
    
    // Implement the pauseBattle method
    public void pauseBattle()
    {
        // Toggle the pause state
        isPaused = !isPaused; 

        // If paused, hide any tooltips
        if (isPaused && isTooltipShowing)
        {
            UIManager.Instance.HideAnswerToolTip();
            isTooltipShowing = false;
        }
        
        // If unpaused, reset the tooltip timer
        if (!isPaused)
        {
            StartTooltipTimer();
        }
    }
    
    // Optional: Add a method to explicitly resume the battle
    public void resumeBattle()
    {
        isPaused = false;
        Debug.Log("Battle resumed");
    }
    
    private void TimeUp()
    {
        // Stop the timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // Disable answer panels
        UIManager.Instance.ShowOptionPanel(false);
        UIManager.Instance.ShowPowerUpPanel(false);
        UIManager.Instance.ClearTimerText();
        
        // Track answer (incorrect)
        AchievementManager.Instance.TrackAnswer(false);
        
        // Handle the timeout as an incorrect answer
        if (isEnemyTurn)
        {
            // If enemy is attacking, player health decreases
            playerHealth -= (healthDecreaseAmount * 2);
            playerHealth = Mathf.Clamp(playerHealth, 0f, 1f);
            UIManager.Instance.UpdateHealthBar(true, playerHealth);
            
            StartCoroutine(TimeUpSequence("Time's up! The enemy's attack was successful!"));
        }
        else
        {
            // If player is attacking, just mark it as a miss
            StartCoroutine(TimeUpSequence("Time's up! Your attack missed and the enemy countered!"));
            playerHealth -= (healthDecreaseAmount * 2);
            playerHealth = Mathf.Clamp(playerHealth, 0f, 1f);
            UIManager.Instance.UpdateHealthBar(true, playerHealth);
        }
    }
    
    private IEnumerator TimeUpSequence(string message)
    {
        numQuestions += 1;
        yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(message, typingSpeed));
        yield return StartCoroutine(UIManager.Instance.FlashSprite(UIManager.Instance.playerImage));
        yield return new WaitForSeconds(1f);
        
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
                StartCoroutine(EndBattle());
            }
            else
            {
                StartCoroutine(ShowQuestionCoroutine(currentQuestions[currentQuestionIndex]));
            }
        }
    }
    
    private IEnumerator ShowQuestionCoroutine(EvaluationQuestion question)
    {
        UIManager.Instance.ShowOptionPanel(true);
        UIManager.Instance.ShowPowerUpPanel(true);
        
        // Clear previous hover events from all buttons first
        UIManager.Instance.ClearButtonEventTriggers();

        // Start typing the question
        yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
            question.question + "\n\n What will " + playerName + " do?", 
            typingSpeed
        ));
        
        // Set up answer callbacks
        Action<int>[] callbacks = new Action<int>[4];
        for (int i = 0; i < 4; i++)
        {
            int capturedIndex = i;
            callbacks[i] = (index) => StartCoroutine(OnAnswerSelectedCoroutine(capturedIndex, question, playerMoves[capturedIndex]));
        }
        
        // Setup the buttons through UIManager
        UIManager.Instance.SetupAnswerButtons(playerMoves, callbacks);
        
        // Setup hover listeners for each button
        for (int i = 0; i < playerMoves.Length && i < 4; i++)
        {
            int capturedIndex = i;
            
            // Add hover enter listener
            UIManager.Instance.SetupAnswerButtonHover(
                UIManager.Instance.answerButtons[i],
                EventTriggerType.PointerEnter,
                (data) => OnHoverEnter(capturedIndex)
            );
            
            // Add hover exit listener
            UIManager.Instance.SetupAnswerButtonHover(
                UIManager.Instance.answerButtons[i],
                EventTriggerType.PointerExit,
                (data) => OnHoverExit()
            );
        }
        
        // Start the timer after setting up the question
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(QuestionTimerCoroutine());

        // Start the tooltip timer
        StartTooltipTimer();
        yield break;
    }
    
    private IEnumerator OnAnswerSelectedCoroutine(int index, EvaluationQuestion question, string text)
    {
        numQuestions += 1;
        
        // Stop the timer when an answer is selected
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
            UIManager.Instance.ClearTimerText();
        }

        // Clear tooltip timer and hide tooltip
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
            tooltipCoroutine = null;
        }
        
        if (isTooltipShowing)
        {
            UIManager.Instance.HideAnswerToolTip();
            isTooltipShowing = false;
        }
        
        UIManager.Instance.ShowOptionPanel(false);
        UIManager.Instance.ShowPowerUpPanel(false);
        
        yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(playerName + " used " + text + "!", typingSpeed));
        yield return new WaitForSeconds(1f);
        
        if (index == question.correctIndex)
        {
            // Player answered correctly
            numCorrectAnswers++;
            enemyHealth -= healthDecreaseAmount;
            enemyHealth = Mathf.Clamp(enemyHealth, 0f, 1f);
            UIManager.Instance.UpdateHealthBar(false, enemyHealth);
            AchievementManager.Instance.TrackAnswer(true);
            
            if (isEnemyTurn)
            {
                AudioController.Instance.PlayAnswerCorrect();
                yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                    "Correct! You blocked the enemy's attack and countered to deal damage!", 
                    typingSpeed
                ));
            }
            else
            {
                AudioController.Instance.PlayAnswerCorrect();
                yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                    "Correct! You dealt damage to the enemy!", 
                    typingSpeed
                ));
            }
            
            // Flash enemy sprite to indicate damage
            StartCoroutine(UIManager.Instance.FlashSprite(UIManager.Instance.eval_npcImage));
        }
        else
        {
            // Player answered incorrectly
            playerHealth -= (healthDecreaseAmount * 2);
            playerHealth = Mathf.Clamp(playerHealth, 0f, 1f);
            UIManager.Instance.UpdateHealthBar(true, playerHealth);
            AchievementManager.Instance.TrackAnswer(false);
            
            if (isEnemyTurn)
            {
                AudioController.Instance.PlayAnswerIncorrect();
                yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                    "Incorrect! The enemy attacked successfully!", 
                    typingSpeed
                ));
            }
            else
            {
                AudioController.Instance.PlayAnswerIncorrect();
                yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                    "Incorrect! You missed your attack and the enemy has countered to deal damage!", 
                    typingSpeed
                ));
            }
            
            // Flash player sprite to indicate damage
            StartCoroutine(UIManager.Instance.FlashSprite(UIManager.Instance.playerImage));
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
            if (enemyHealth <= 0 || playerHealth <= 0)
            {
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
        if (enemyHealth <= 0 || playerHealth <= 0)
        {
            yield return StartCoroutine(EndBattle());
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(2f); // Wait before the enemy's turn
            EvaluationQuestion question = currentQuestions[currentQuestionIndex];
            
            int randomIndex = UnityEngine.Random.Range(0, 4);
            yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                npcName + " used " + npcMoves[randomIndex] + "\n Answer correctly to block!", 
                typingSpeed
            ));
            
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(ShowQuestionCoroutine(question));
        }
    }
    
    public void closeBattle()
    {
        // Stop all coroutines to prevent any lingering processes
        StopAllCoroutines();

        // Ensure tooltip is hidden
        if (isTooltipShowing)
        {
            UIManager.Instance.HideAnswerToolTip();
            isTooltipShowing = false;
        }

        UIManager.Instance.ShowBattlePanel(false);
        UIManager.Instance.ShowEvaluationPanel(false);
        isEvaluating = false;
        Player.Instance.resumePlayer();
        Player.Instance.resumeInteraction();
        UIManager.Instance.enablePlayerHUD();
        AudioController.Instance.PlayBackgroundMusic();
    }
    
    IEnumerator EndBattle()
    {
        // Reset pause state
        isPaused = false;
        UIManager.Instance.ShowOptionPanel(false);
        UIManager.Instance.ShowPowerUpPanel(false);
        bool hasFailed = playerHealth <= 0;
        
        if (enemyHealth <= 0)
        {
            AudioController.Instance.ToggleMusic(false);
            AudioController.Instance.PlayBattleVictory();
            yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                "You defeated the enemy! Well done!", 
                typingSpeed
            ));
        }
        else if (playerHealth <= 0)
        {
            AudioController.Instance.ToggleMusic(false);
            AudioController.Instance.PlayBattleLoss();
            yield return StartCoroutine(UIManager.Instance.TypeEvaluationText(
                "You were defeated. Better luck next time!", 
                typingSpeed
            ));
        }
        
        yield return new WaitForSeconds(2f);
        UIManager.Instance.ShowBattlePanel(false);
        UIManager.Instance.ShowEvaluationPanel(false);
        UIManager.Instance.enablePlayerHUD();
        isEvaluating = false;
        
        yield return new WaitForSeconds(2f);
        
        User loggedInUser = DatabaseManager.Instance.loggedInUser;
        Chapter currentChapter = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter];
        Level currentLevel = currentChapter.levels[loggedInUser.currentLevel - 1];
        
        if (!currentLevel.isCompleted)
        {
            loggedInUser.setLevelScore(((float)numCorrectAnswers / numQuestions) * 100);
            
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
            
            // Show post-evaluation success dialogue
            yield return StartCoroutine(NPCManager.Instance.ShowPostEvaluationDialogue(npcName, hasFailed));
            
            AudioController.Instance.ToggleMusic(true);
            AudioController.Instance.PlayBackgroundMusic();
            Player.Instance.resumePlayer();
            
            AchievementManager.Instance.CheckAchievements(levelName, DatabaseManager.Instance.loggedInUser.getLevelTime(), numQuestions, numCorrectAnswers, hasFailed);
            
            // Wait until achievement processing is complete
            yield return StartCoroutine(WaitForAchievementProcessing());
            
            // Now show the completion/failure UI
            if (hasFailed)
            {
                UIManager.Instance.showFailedLevel();
            }
            else
            {
                UIManager.Instance.showCompletedLevel();
            }
        }
        else
        {
            // If already completed, no need to show post evaluation dialogues
            AudioController.Instance.ToggleMusic(true);
            AudioController.Instance.PlayBackgroundMusic();
            Player.Instance.resumePlayer();
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
    
    // method to start the tooltip timer
    private void StartTooltipTimer()
    {
        // Clear any existing tooltip coroutine
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
            tooltipCoroutine = null;
        }
        
        // Hide the tooltip if it's showing
        if (isTooltipShowing)
        {
            UIManager.Instance.HideAnswerToolTip();
            isTooltipShowing = false;
        }
        
        // Start a new tooltip timer
        tooltipCoroutine = StartCoroutine(TooltipTimerCoroutine());
    }

    // coroutine to handle tooltip timing
    private IEnumerator TooltipTimerCoroutine()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(tooltipDelay);
        
        // Only show tooltip if we're still in question mode and not paused
        if (timerCoroutine != null && !isPaused)
        {
            UIManager.Instance.ShowAnswerToolTip();
            isTooltipShowing = true;
        }
    }

    // Hover handlers
    private void OnHoverEnter(int index)
    {
        // Reset the tooltip timer when user hovers over an answer
        StartTooltipTimer();

        if (!isTyping && currentQuestionIndex < currentQuestions.Count)
        {
            UIManager.Instance.eval_dialogueText.text = currentQuestions[currentQuestionIndex].options[index];
        }
    }
    
    private void OnHoverExit()
    {
        if (!isTyping && currentQuestionIndex < currentQuestions.Count)
        {
            UIManager.Instance.eval_dialogueText.text = currentQuestions[currentQuestionIndex].question + 
                "\n\n What will " + playerName + " do?";
        }
    }
}