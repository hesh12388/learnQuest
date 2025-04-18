using UnityEngine;
using TMPro;  // Import TextMeshPro
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject landingPage;
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;
    public GameObject loginFailedMessage;

    [Header("Register Fields")]
    public TMP_InputField registerEmailField;
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public GameObject registrationFailedMessage;
    public GameObject registrationSuccessMessage;

    [Header("Course Selection Fields")]
    public GameObject courseSelectionPanel;

    [Header("Saved Games UI")]
    public Transform savedGamesContentPanel;  // The Content Panel inside Scroll View
    public GameObject savedGamePrefab; // Prefab for saved game entry
    public GameObject newGamePrefab; // Prefab for new game entry
    public GameObject savedGamesPanel; // Panel for saved games
    public GameObject deleteSavedGamePanel; // Panel for delete confirmation
    public GameObject deleteSavedGameBlurPanel; // Button to delete saved game
    private string deleteSavedGameId; // ID of the saved game to delete

    [Header("Course Selection")]
    public TMP_Text courseNameText;
    public GameObject courseSelectionErrorPanel;
    public GameObject blurPanel;

    [Header("Levels")]
    public GameObject levelsPanel;
    public GameObject levelsContentPanel;
    public GameObject levelPrefab;
    public GameObject lockedLevelPrefab;
    private int currentChapterIndex = 0;
    public TextMeshProUGUI chapterNameText;
    public GameObject levelsGameContentPanel;
    public TextMeshProUGUI chapterGameNameText;

    [Header("Loading Screen")]
    public GameObject loadingPanel;

    
    [Header("Navigation")]
    public GameObject characterSelectionPanel;
    public GameObject chatPanel;
    public GameObject leaderboardPanel;
    public GameObject objectivesPanel;
    public GameObject settingsPanel;
    public GameObject achievementsPanel;
    public GameObject levelsGamePanel;
    public GameObject shopPanel;
    public GameObject menuButton;

    [Header("Shop Item Info Panel")]
    public GameObject itemInfoPanel;
    public TextMeshProUGUI itemInfoName;
    public TextMeshProUGUI itemInfoDescription;
    public TextMeshProUGUI itemInfoUse;
    public TextMeshProUGUI itemInfoCost;
    public Image itemInfoCurrencySprite;
    public Image itemInfoSprite;

    [Header("Shop UI")]
    public Transform shopContentPanel;
    public GameObject shopItemPrefab;
    public TextMeshProUGUI user_coins_text;
    public TextMeshProUGUI user_gems_text;
    private string current_shop_category="Move";

    [Header("Objective Guide UI")]
    public GameObject objectiveGuidePanel;
    public TextMeshProUGUI objective_guide_text;
    public GameObject objectiveGuideButton;
    public GameObject objectivesCompleteButton;

    [Header("Leaderboard UI")]
    public Transform leaderboardContentPanel;  
    public GameObject leaderboardEntryPrefab; 
    private string current_leaderboard_category="Score";

    [Header("Objectives UI")]
    public Transform objectivesContentPanel;
    public GameObject objectivePrefab;

    [Header("Achievement UI")]
    public Transform achivementContentPanel;
    public GameObject achievementPrefab;
    public GameObject achievementCompletedPanel;
    private string current_achievement_category="All";

    public GameObject inGameUiPanel;
    public GameObject landingPanel;

    [Header("Evaluation UI")]
    public Image playerImage;
    public Image eval_npcImage;
    public TMP_Text eval_dialogueText;
    public Button[] answerButtons;
    public Button [] power_up_buttons;
    public GameObject power_up_panel;
    public TMP_Text eval_time;
    public GameObject evaluationPanel;
    public GameObject battlePanel;
    public GameObject npcIntroPanel;
    public Image battleBackground;
    public Image battleIntroBackground;
    public GameObject powerUpTooltip;
    public TMP_Text powerUpTooltipText;
    public GameObject answerToolTip;
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;
    public Transform playerHealthBar;
    public Button closeDialogue;
    public Button continueDialogue;
    public Button exitDialogue;
    public TMP_Text npcNameText;
    public TMP_Text npcLevelText;
    public Transform npcHealthBar;
    public Image npcIntroImage;
    public TMP_Text npcIntroNameText;

    [Header("UI Checkmarks")]
    public Toggle [] checkmarks;
    public GameObject [] checkmarkPanels;
    

    [Header("Feedback Panels")]
    public GameObject optionPanel;
    public bool isInGame { get; private set; } 
    public GameObject levelCompletePanel;
    public GameObject levelFailedPanel;
    public GameObject levelUnlockPanel;
    public TMP_Text levelUnlockText;
    public GameObject chapterCompletedPanel;
    public GameObject chapterCompletedText;

    [Header("Demonstration UI")]
    public Image demonstration_npcImage;
    public GameObject dialoguePanel;
    public TMP_Text demonstration_dialogueText;
    public GameObject graphicsPanel;
    public Image graphicsImage;
    public GameObject graphicsImagePanel;
    public Image graphicsInstructorImage;
    public GameObject npcImagePanel;
    public GameObject objectiveCompletionPanel;
    public Image enterKey;

    [Header("Questions UI")]
    public GameObject questionsPanel;
    public Button[] question_answerButtons;
    public TMP_Text question_text;

    [Header("Landing Settings & Help")]
    public GameObject settingsHelpPanel;
    public GameObject landingSettingsPanel;
    public GameObject helpPanel;
    public GameObject settingsButton;
    public GameObject helpButton;
    public GameObject ragPanel;
    public GameObject assistantButton;

    [Header("Player HUD")]
    public GameObject playerHUD;
    public TextMeshProUGUI playerCoins;
    public TextMeshProUGUI playerGems;
    public Button hudToolTipButton;
    public GameObject hudToolTipPanel;

    public static UIManager Instance { get; private set; } // Singleton instance


    private string currentMenu = "settings";
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


    public void setCheckMarkActive(int index){

        for(int i = 0; i < checkmarks.Length; i++){
            if(i != index){
                // Only turn off toggles that are currently on
                checkmarks[i].SetIsOnWithoutNotify(false);
                checkmarkPanels[i].SetActive(false);
            }
        }

        if(index >= 0 && index < checkmarks.Length){
            // Check if already set to avoid infinite recursion
            checkmarks[index].SetIsOnWithoutNotify(true);
            checkmarkPanels[index].SetActive(true);
            
        }
    }


    public bool isMenuOpen(){
        return inGameUiPanel.activeSelf;
    }

    public void setMenuOpen(bool open){
        
        inGameUiPanel.SetActive(open);
    }

    public void OnToggleMenu()
    {
        

        if(!isInGame || (EvaluationManager.Instance!=null && EvaluationManager.Instance.isEvaluating) || (NPCManager.Instance!=null && NPCManager.Instance.isInstructing) || (RagChatManager.Instance!=null && RagChatManager.Instance.isUsingAssistant)){
            return;
        }

        if(IsPointerOverInputField()){
            Debug.Log("Menu toggle ignored due to input field focus");
            return;
        }

        if(Player.Instance.stop_interaction){
            Debug.Log("Menu toggle ignored due to player interaction");
            return;
        }

        // Toggle the inGameUiPanel active state
        if (inGameUiPanel != null)
        {
            Debug.Log("Toggling inGameUiPanel");
            inGameUiPanel.SetActive(!inGameUiPanel.activeSelf);
            // Clear selection so Enter won't trigger it again
            EventSystem.current.SetSelectedGameObject(null);
            
            if (inGameUiPanel.activeSelf)
            {
                if (currentMenu == "settings")
                {
                    ShowSettings();
                }
                else if (currentMenu == "objectives")
                {
                    ShowObjectives();
                }
                else if (currentMenu == "chat")
                {
                    ShowChat();
                }
                else if (currentMenu == "achievements")
                {
                    ShowAchievements();
                }
                else if (currentMenu == "leaderboard")
                {
                    ShowLeaderboard();
                }
                else if (currentMenu == "character")
                {
                    ShowCharacterSelection();
                }
                else if (currentMenu == "shop")
                {
                    ShowShop();
                }
                else if (currentMenu == "levels")
                {
                    ShowGameLevels();
                }

                AudioController.Instance.PlayMenuOpen();
            }
        }
        else
        {
            Debug.LogWarning("inGameUiPanel reference not set in UIManager");
        }
    }

    // Check if the current selected object is an input field
    private bool IsPointerOverInputField() 
    {
        // Check if there's a selected object and if it's an input field
        if (EventSystem.current != null && 
            EventSystem.current.currentSelectedGameObject != null) 
        {
            TMP_InputField inputField = EventSystem.current.currentSelectedGameObject
                .GetComponent<TMP_InputField>();
                
            return inputField != null && inputField.isFocused;
        }
        
        return false;
    }


    public void ShowAssistant(){
        // Clear selection so Enter won't trigger it again
        EventSystem.current.SetSelectedGameObject(null);
        setGameUIPanelsInactive();
        RagChatManager.Instance.ShowRagPanel();
        Player.Instance.pausePlayer();
        Player.Instance.stopInteraction();
        UIManager.Instance.disablePlayerHUD();
    }

    public void showHelp(){
        // Clear selection so Enter won't trigger it again
        EventSystem.current.SetSelectedGameObject(null);

        helpPanel.SetActive(!helpPanel.activeSelf);
        if(helpPanel.activeSelf){
            settingsHelpPanel.SetActive(true);
            Player.Instance.stopInteraction();
            Player.Instance.pausePlayer();
        }
        else{
            settingsHelpPanel.SetActive(false);
            Player.Instance.resumeInteraction();
            Player.Instance.resumePlayer();
        }
        landingSettingsPanel.SetActive(false);
    }

    public void setObjectiveGuideText(string text, bool objectivesCompleted){

        if(!objectiveGuidePanel.activeSelf){
            objectiveGuidePanel.SetActive(true);
        }

        objective_guide_text.text = text;
        if(objectivesCompleted){
            objectiveGuideButton.SetActive(false);
            objectivesCompleteButton.SetActive(true);
        }
        else{
            objectiveGuideButton.SetActive(true);
            objectivesCompleteButton.SetActive(false);
        }
    }

    public void showObjectiveGuide(){
        // Clear selection so Enter won't trigger it again
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(NPCManager.Instance.showNpcIndicator(objective_guide_text.text));
    }

    public void showLandingSettings(){
        // Clear selection so Enter won't trigger it again
        EventSystem.current.SetSelectedGameObject(null);
        landingSettingsPanel.SetActive(!landingSettingsPanel.activeSelf);
        if(landingSettingsPanel.activeSelf){
            settingsHelpPanel.SetActive(true);
        }
        else{
            settingsHelpPanel.SetActive(false);
        }
        helpPanel.SetActive(false);
    }
   
    public void showCompletedLevel(){
        Player.Instance.stopInteraction();
        levelCompletePanel.SetActive(true);
        User loggedInUser = DatabaseManager.Instance.loggedInUser;
        Level current_level = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1];
        levelCompletePanel.GetComponent<LevelMessage>().SetLevelUpdateUI(loggedInUser.getLevelScore(), current_level.score, current_level.points, false);
        AudioController.Instance.PlayLevelComplete();
    }

    public void showFailedLevel(){
        Player.Instance.stopInteraction();
        levelFailedPanel.SetActive(true);
        User loggedInUser = DatabaseManager.Instance.loggedInUser;
        Level current_level = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1];
        levelCompletePanel.GetComponent<LevelMessage>().SetLevelUpdateUI(loggedInUser.getLevelScore(), current_level.score, current_level.points, true);
        AudioController.Instance.PlayLevelFailed();
    }

    public void LevelCompletedNext(){
        setGameUIPanelsInactive();

        if(DatabaseManager.Instance.loggedInUser.currentLevel<DatabaseManager.Instance.loggedInUser.courseStructure.chapters[DatabaseManager.Instance.loggedInUser.currentChapter].levels.Count){
            levelUnlockPanel.SetActive(true);
            levelUnlockText.GetComponent<TMP_Text>().text = DatabaseManager.Instance.loggedInUser.courseStructure.chapters[DatabaseManager.Instance.loggedInUser.currentChapter].levels[DatabaseManager.Instance.loggedInUser.currentLevel].level_name;
            AudioController.Instance.PlayLevelComplete();
        }
        else{
            chapterCompletedPanel.SetActive(true);
            chapterCompletedText.GetComponent<TMP_Text>().text = DatabaseManager.Instance.loggedInUser.courseStructure.chapters[DatabaseManager.Instance.loggedInUser.currentChapter].chapter_name;
            AudioController.Instance.PlayLevelComplete();
        }
    }

    public void setGameUIPanelsInactive(){
        characterSelectionPanel.SetActive(false);
        chatPanel.SetActive(false);
        objectivesPanel.SetActive(false);
        settingsPanel.SetActive(false);
        achievementsPanel.SetActive(false);
        chatPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        levelsGamePanel.SetActive(false);
        shopPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        levelFailedPanel.SetActive(false);
        levelUnlockPanel.SetActive(false);
        chapterCompletedPanel.SetActive(false);
        questionsPanel.SetActive(false);
        settingsHelpPanel.SetActive(false);
        ragPanel.SetActive(false);
        helpPanel.SetActive(false);
    }

    public void StartNextLevel(){
        setGameUIPanelsInactive();
        Player.Instance.resumeInteraction();
        objectiveGuidePanel.SetActive(false);
        playerHUD.SetActive(false);
        helpButton.SetActive(false);
        User loggedInUser = DatabaseManager.Instance.loggedInUser;
        if(loggedInUser.currentLevel<loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels.Count){
            startLevel(loggedInUser.currentLevel+1);
        }  
    }

    public void disablePlayerHUD(){
        playerHUD.SetActive(false);
        helpButton.SetActive(false);
        menuButton.SetActive(false);
        assistantButton.SetActive(false);
        objectiveGuidePanel.SetActive(false);
    }
    
    public void enablePlayerHUD(){
        playerHUD.SetActive(true);
        helpButton.SetActive(true);
        menuButton.SetActive(true);
        assistantButton.SetActive(true);
        objectiveGuidePanel.SetActive(true);
    }

    public void restartEvaluation(){
        setGameUIPanelsInactive();
        Player.Instance.resumeInteraction();
        EvaluationManager.Instance.StartEvaluation();
    }

    public void ShowHome(){
        Player.Instance.resumeInteraction();
        setGameUIPanelsInactive();
    }

    public void ShowCharacterSelection()
    {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }
        setCheckMarkActive(4);
        currentMenu = "character";
        setGameUIPanelsInactive();
        characterSelectionPanel.SetActive(true);
        characterSelectionPanel.GetComponent<PlayerSelector>().ShowCharacters();

        // Track Character Selection usage
        DatabaseManager.Instance.UpdateMetric("character_customizatio");
    }

    public void ShowGameLevels(){
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }
        setCheckMarkActive(2);
        currentMenu = "levels";
        setGameUIPanelsInactive();
        levelsGamePanel.SetActive(true);
        currentChapterIndex = 0; // Reset to first chapter
        DisplayChapterLevels(DatabaseManager.Instance.loggedInUser.courseStructure, levelsGameContentPanel, chapterGameNameText);
    }

    public IEnumerator ShowObjectiveComplete(Objective objective) {
        objectiveCompletionPanel.SetActive(true);
        Player.Instance.stopInteraction();
        Player.Instance.pausePlayer();
        updatePlayerCoins();
        objectiveCompletionPanel.GetComponent<ObjectiveCompleteUI>().SetObjectiveCompleteData(objective.objective_name, objective.points);
        AudioController.Instance.PlayObjectiveComplete();
        yield return new WaitForSeconds(2);
        objectiveCompletionPanel.SetActive(false);
        Player.Instance.resumeInteraction();
        Player.Instance.resumePlayer();
    }
    
    // method for showing chat
    public void ShowChat()
    {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }

        setCheckMarkActive(3); // Assuming chat is index 1
        currentMenu = "chat";
        setGameUIPanelsInactive();
        chatPanel.SetActive(true);

        // Track Chat usage
        DatabaseManager.Instance.UpdateMetric("chat_use");
    }

    public void ShowObjectives() {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }
        setCheckMarkActive(6);
        currentMenu = "objectives";
        setGameUIPanelsInactive();
        objectivesPanel.SetActive(true);
        
        // Clear existing objectives first
        foreach (Transform child in objectivesContentPanel) {
            Destroy(child.gameObject);
        }
        
        foreach (Objective objective in ObjectiveManager.Instance.GetAllObjectives()) {
            GameObject objectiveObject = Instantiate(objectivePrefab, objectivesContentPanel);
            ObjectiveUI objectiveUI = objectiveObject.GetComponent<ObjectiveUI>();
            
            if (objectiveUI != null) {
                // Determine if objective is completed
                bool isCompleted = objective.status.ToLower() == "completed";
                
                // Set the objective data in the UI component
                objectiveUI.SetObjective(
                    objective.objective_name, 
                    isCompleted, 
                    objective.description, 
                    objective.difficulty, 
                    objective.points
                );
            }
        }

        // Track Objectives usage
        DatabaseManager.Instance.UpdateMetric("objective_view");
      
    }

    public void ShowSettings(){
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }
        setCheckMarkActive(0);
        currentMenu = "settings";
        setGameUIPanelsInactive();
        settingsPanel.SetActive(true);
    }

    public void ShowAchievements() {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }
        setCheckMarkActive(5);
        currentMenu = "achievements";
        setGameUIPanelsInactive();
        achievementsPanel.SetActive(true);

        // Use cached achievement data
        if(current_achievement_category=="All"){
            ShowAllAchievements();
        }
        else if(current_achievement_category=="Completed"){
            ShowCompletedAchievements();
        }
        else if(current_achievement_category=="InProgress"){
            ShowInProgressAchievements();
        }

        // Track Achievements usage
        DatabaseManager.Instance.UpdateMetric("achievement_view");
        
    }


    public void ShowAllAchievements(){
            current_achievement_category = "All";
            // Clear any existing entries first
            foreach (Transform child in achivementContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            List<Achievement> achievements_list = AchievementManager.Instance.Achievements;
            achievements_list.Sort((a, b) => b.gems.CompareTo(a.gems));
            foreach (Achievement achievement in achievements_list)
            {
                GameObject achievementObject = Instantiate(achievementPrefab, achivementContentPanel);
                AchievementUI achievementUI = achievementObject.GetComponent<AchievementUI>();
                
                if (achievementUI != null)
                {
                    achievementUI.SetAchievement(
                        achievement.achievement_name,
                        achievement.status.ToLower()=="completed",
                        achievement.description,
                        achievement.gems
                    );
                }
            }
            
    }

    public void ShowCompletedAchievements(){
        current_achievement_category = "Completed";
        // Clear any existing entries first
            foreach (Transform child in achivementContentPanel)
            {
                Destroy(child.gameObject);
            }
            
        List<Achievement> achievements_list = AchievementManager.Instance.Achievements;
        achievements_list.Sort((a, b) => b.gems.CompareTo(a.gems));
        foreach (Achievement achievement in achievements_list)
        {
            if(achievement.status.ToLower()=="completed"){
                GameObject achievementObject = Instantiate(achievementPrefab, achivementContentPanel);
                AchievementUI achievementUI = achievementObject.GetComponent<AchievementUI>();
                
                if (achievementUI != null)
                {
                    achievementUI.SetAchievement(
                        achievement.achievement_name,
                        true,
                        achievement.description,
                        achievement.gems
                    );
                }
            }
        }
            
    }

    public void ShowInProgressAchievements(){
        current_achievement_category = "InProgress";
        // Clear any existing entries first
        foreach (Transform child in achivementContentPanel)
        {
            Destroy(child.gameObject);
        }
            
        List<Achievement> achievements_list = AchievementManager.Instance.Achievements;  
        achievements_list.Sort((a, b) => b.gems.CompareTo(a.gems));
        foreach (Achievement achievement in achievements_list)
        {
            if(achievement.status.ToLower()=="not completed"){
                GameObject achievementObject = Instantiate(achievementPrefab, achivementContentPanel);
                AchievementUI achievementUI = achievementObject.GetComponent<AchievementUI>();
                
                if (achievementUI != null)
                {
                    achievementUI.SetAchievement(
                        achievement.achievement_name,
                        false,
                        achievement.description,
                        achievement.gems
                    );
                }
            }
        }
            
    }

    public void ShowLeaderboard()
    {
        if (!isInGame)
        {
            return;
        }
        if (!inGameUiPanel.activeSelf)
        {
            inGameUiPanel.SetActive(true);
        }
        setCheckMarkActive(1);
        currentMenu = "leaderboard";
        setGameUIPanelsInactive();
        leaderboardPanel.SetActive(true);

        // Show loading while fetching leaderboard data
        ShowLoading();

        // Fetch leaderboard data from LeaderboardManager
        LeaderboardManager.Instance.LoadLeaderboard((entries) =>
        {
            // Hide loading indicator when data is retrieved
            HideLoading();

            if (entries != null && entries.Count > 0)
            {
                if (current_leaderboard_category == "Score")
                {
                    ShowLeaderboardByScore();
                }
                else if (current_leaderboard_category == "Achievements")
                {
                    ShowLeaderboardByAchievements();
                }
                else if (current_leaderboard_category == "Gems")
                {
                    ShowLeaderboardByNumGems();
                }
            }
        });

        // Track Leaderboard usage
        DatabaseManager.Instance.UpdateMetric("leaderboard_view");
    }

    public void ShowLeaderboardByScore()
    {
        current_leaderboard_category = "Score";
        List<LeaderboardEntry> sortedEntries = LeaderboardManager.Instance.GetLeaderboardByCategory("score");

        // Populate the leaderboard UI
        PopulateLeaderboardUI(sortedEntries);
    }

    public void ShowLeaderboardByAchievements()
    {
        current_leaderboard_category = "Achievements";
        List<LeaderboardEntry> sortedEntries = LeaderboardManager.Instance.GetLeaderboardByCategory("achievements");

        // Populate the leaderboard UI
        PopulateLeaderboardUI(sortedEntries);
    }

    public void ShowLeaderboardByNumGems()
    {
        current_leaderboard_category = "Gems";
        List<LeaderboardEntry> sortedEntries = LeaderboardManager.Instance.GetLeaderboardByCategory("gems");

        // Populate the leaderboard UI
        PopulateLeaderboardUI(sortedEntries);
    }

    private void PopulateLeaderboardUI(List<LeaderboardEntry> entries)
    {
        // Clear existing leaderboard entries
        foreach (Transform child in leaderboardContentPanel)
        {
            Destroy(child.gameObject);
        }

        int index=0;
        // Populate the leaderboard with new entries
        foreach (LeaderboardEntry entry in entries)
        {
            GameObject leaderboardEntryObject = Instantiate(leaderboardEntryPrefab, leaderboardContentPanel);
            LeaderboardEntryUI entryUI = leaderboardEntryObject.GetComponent<LeaderboardEntryUI>();
            if (entryUI != null)
            {
                if(current_leaderboard_category=="Score"){
                    entryUI.SetEntryData(entry.username, entry.score, index);
                }
                else if(current_leaderboard_category=="Achievements"){
                    entryUI.SetEntryData(entry.username, entry.numAchievements, index);
                }
                else{
                    entryUI.SetEntryData(entry.username, entry.numGems, index);
                }
                
            }

            index+=1;
        }
    }

    public void activate_hint(){
        EvaluationManager.Instance.GiveHint();
        AudioController.Instance.PlayPowerUp();
    }

    public void activate_extra_time(){
        EvaluationManager.Instance.AddExtraTime();
        AudioController.Instance.PlayPowerUp();
    }

    public void activate_power_reveal(){
        EvaluationManager.Instance.RevealAnswer();
        AudioController.Instance.PlayPowerUp();
    }
   
    // Start() method
    private void Start()
    {
        StartCoroutine(showLanding());
        AudioController.Instance.PlayBackgroundMusic();
    }

    IEnumerator showLanding(){
        yield return new WaitForSeconds(1);

        if(landingPage!=null){
            landingPage.SetActive(true);
        }
    }

    public void startLevel(int level) {
        Debug.Log("Starting level " + level);
        // Clear selection so Enter won't trigger it again
        EventSystem.current.SetSelectedGameObject(null);
        setPanelsInactive();
        setGameUIPanelsInactive();
        inGameUiPanel.SetActive(false);
        settingsButton.SetActive(false);
        helpButton.SetActive(false);
        menuButton.SetActive(false);
        objectiveGuidePanel.SetActive(false);    
        // Load game data at the start of the level
        CourseManager.Instance.StartLevel(level, (success) =>{
            if (success)
            {
                landingPanel.SetActive(false);
                isInGame = true;
                enablePlayerHUD();
                updatePlayerCoins();
                updatePlayerGems();
            }
            else
            {
                Debug.LogError("Failed to load level data");
            }
        });
    }

    public void showObjectiveInMap(string objectiveName){
       setGameUIPanelsInactive();
       setMenuOpen(false);
       StartCoroutine(NPCManager.Instance.showNpcIndicator(objectiveName));
    }

    public void updatePlayerCoins(){
        playerCoins.text = DatabaseManager.Instance.loggedInUser.score.ToString();
    }
    public void updatePlayerGems(){
        playerGems.text = DatabaseManager.Instance.loggedInUser.numGems.ToString();
    }

    public void ShowLogin()
    {
        AudioController.Instance.PlayMenuOpen();
        setLogInRegisterMessagesInactive();
        setPanelsInactive();
        loginPanel.SetActive(true);
    }

    public void setPanelsInactive(){
        landingPage.SetActive(false);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        courseSelectionPanel.SetActive(false);
        savedGamesPanel.SetActive(false);
        courseSelectionErrorPanel.SetActive(false);
        blurPanel.SetActive(false);
        levelsPanel.SetActive(false);
    }

    // Helper methods for loading panel
    private void ShowLoading()
    {
        loadingPanel.SetActive(true);
    }

    private void HideLoading()
    {
        loadingPanel.SetActive(false);
    }

    public void ShowRegister()
    {
        AudioController.Instance.PlayMenuOpen();
        setPanelsInactive();
        setLogInRegisterMessagesInactive();
        registerPanel.SetActive(true);
    }

    public void BackToLanding()
    {
        setPanelsInactive();
        landingPage.SetActive(true);
    }

    public void showCourseSelection()
    {
        AudioController.Instance.PlayMenuOpen();
        setPanelsInactive();
        courseSelectionPanel.SetActive(true);
    }
    
    public IEnumerator ShowAchievementUnlocked(string achievementName){
        achievementCompletedPanel.SetActive(true);
        Player.Instance.stopInteraction();
        Player.Instance.pausePlayer();
        List<Achievement> achievements_list = AchievementManager.Instance.Achievements;
        foreach(Achievement achievement in achievements_list){
            if(achievementName==achievement.achievement_name){
                achievementCompletedPanel.GetComponent<AchievementUnlocked>().SetAchievementUnlocked(achievementName, achievement.gems, achievement.description);
                updatePlayerGems();
                break;
            }
        }

        AudioController.Instance.PlayAchievementComplete();
        yield return new WaitForSeconds(2);
        Player.Instance.resumeInteraction();
        Player.Instance.resumePlayer();
        achievementCompletedPanel.SetActive(false);
       
    }
    public void showLevels() {
        AudioController.Instance.PlayMenuOpen();
        setPanelsInactive();
        ShowLoading();
        levelsPanel.SetActive(true);
        
        // Fetch course structure and display levels when ready
        CourseManager.Instance.GetCourseStructure((courseStructure) => {
            HideLoading();
            if (courseStructure != null) {
                Debug.Log("Course structure retrieved successfully");
                currentChapterIndex = 0; // Reset to first chapter
                DisplayChapterLevels(courseStructure, levelsContentPanel, chapterNameText);
                
            } else {
                Debug.LogError("Failed to retrieve course structure");
                // Consider showing an error message to the user
            }
        });
    }

    // Display the levels for the current chapter
    private void DisplayChapterLevels(CourseStructure courseStructure, GameObject contentPanel, TextMeshProUGUI chapterText) {

        DatabaseManager.Instance.loggedInUser.currentChapter = currentChapterIndex;
        // Clear existing level prefabs
        foreach (Transform child in contentPanel.transform) {
            Destroy(child.gameObject);
        }
        
        if (courseStructure.chapters.Count == 0 || currentChapterIndex >= courseStructure.chapters.Count) {
            Debug.LogError("No chapters available or invalid chapter index");
            return;
        }
        
        // Get current chapter
        Chapter currentChapter = courseStructure.chapters[currentChapterIndex];
        
        chapterText.text = currentChapter.chapter_name;
        // Create level prefabs for each level in the chapter
        foreach (Level level in currentChapter.levels) {
            GameObject levelObject;
            
            // Determine if level is locked
            bool isLocked = level.status.ToLower() == "locked";
            
            // Instantiate appropriate prefab
            if (isLocked) {
                levelObject = Instantiate(lockedLevelPrefab, contentPanel.transform);
                
            } else {
                levelObject = Instantiate(levelPrefab, contentPanel.transform);
                levelObject.GetComponent<LevelPrefab>().setLevelData(level);
            }
            
        }
    }

    // Navigate to the next chapter with looping behavior
    public void NextChapter() {
        CourseStructure courseStructure = DatabaseManager.Instance.loggedInUser.courseStructure;
        if (courseStructure != null && courseStructure.chapters.Count > 0) {
            // Using modulo to wrap around to first chapter when reaching the end
            currentChapterIndex = (currentChapterIndex + 1) % courseStructure.chapters.Count;

            if(isInGame){
                DisplayChapterLevels(courseStructure, levelsGameContentPanel, chapterGameNameText);
            }
            else{
                DisplayChapterLevels(courseStructure, levelsContentPanel, chapterNameText);
            }


            Canvas.ForceUpdateCanvases();
        }
    }

    // Navigate to the previous chapter with looping behavior
    public void PreviousChapter() {
        CourseStructure courseStructure = DatabaseManager.Instance.loggedInUser.courseStructure;
        if (courseStructure != null && courseStructure.chapters.Count > 0) {
            // Using modulo with addition to wrap around to last chapter when at first chapter
            currentChapterIndex = (currentChapterIndex - 1 + courseStructure.chapters.Count) % courseStructure.chapters.Count;
            if(isInGame){
                DisplayChapterLevels(courseStructure, levelsGameContentPanel, chapterGameNameText);
            }
            else{
                DisplayChapterLevels(courseStructure, levelsContentPanel, chapterNameText);
            }

            Canvas.ForceUpdateCanvases();
        }
    }

    public void startCourse(){
        string courseName = courseNameText.text;
        ShowLoading();
        CourseManager.Instance.StartCourse(courseName, (success) =>
        {
            HideLoading();
            if (success)
            {
                setPanelsInactive();
                DatabaseManager.Instance.setUserCourse(courseName);
                showLevels();
            }
            else
            {
                blurPanel.SetActive(true);
                courseSelectionErrorPanel.SetActive(true);
            }
        });
    }

    public void loginUser()
    {
        string email = loginEmailField.text;
        string password = loginPasswordField.text;

            DatabaseManager.Instance.Login(email, password, (bool success) =>
            {
                if (success)
                {
                    setPanelsInactive();
                    ShowSavedGames();
                    ShopManager.Instance.LoadShopItems();
                }
                else
                {
                    loginFailedMessage.SetActive(true);
                }
            });
    }

    public void deleteGame(string coursename){

            deleteSavedGameId = coursename;
            blurPanel.SetActive(true);
            deleteSavedGamePanel.SetActive(true);
    }

    public void confirmedDeleteGame(){
            blurPanel.SetActive(false);
            deleteSavedGamePanel.SetActive(false);
            ShowLoading();
            CourseManager.Instance.DeleteSavedGame(deleteSavedGameId, (success) =>
            {
                HideLoading();
                if (success)
                {
                    setPanelsInactive();
                    ShowSavedGames();
                }
                else
                {
                Debug.Log("Failed to delete course");
                }
            });
    }

    public void SignOut(){
        setGameUIPanelsInactive();
        setPanelsInactive();
        setMenuOpen(false);
        playerHUD.SetActive(false);
        settingsButton.SetActive(true);
        landingPanel.SetActive(true);
        isInGame = false;
        StartCoroutine(signOutSequence());
    }

    IEnumerator signOutSequence(){
        yield return StartCoroutine(TransitionManager.Instance.transitionLanding());
        ShowLogin();
    }

    public void ShowSavedGames()
    {
        setPanelsInactive();
        ShowLoading();
        savedGamesPanel.SetActive(true); // Show Saved Games UI
        AudioController.Instance.PlayMenuOpen();

        CourseManager.Instance.GetUserCourses((courses) =>
        {
            HideLoading();
            // Clear existing saved game entries before adding new ones
            foreach (Transform child in savedGamesContentPanel)
            {
                Destroy(child.gameObject);
            }

            // Loop through each saved game and create an entry in the UI
            foreach (Course course in courses)
            {
                GameObject savedGameEntry = Instantiate(savedGamePrefab, savedGamesContentPanel);
                savedGameEntry.GetComponent<SavedGame>().SetSavedGameData(course.courseName, course.timeStarted);
            }

            GameObject newGameEntry = Instantiate(newGamePrefab, savedGamesContentPanel);

        });
    }
    
    public void setLogInRegisterMessagesInactive(){
        loginFailedMessage.SetActive(false);
        registrationFailedMessage.SetActive(false);
        registrationSuccessMessage.SetActive(false);
    }

    public void registerUser()
    {
        setLogInRegisterMessagesInactive();
        string email = registerEmailField.text;
        string username = registerUsernameField.text;
        string password = registerPasswordField.text;
        DatabaseManager.Instance.Register(email, username, password, (bool success) =>
        {
            if (success)
            {
                registrationSuccessMessage.SetActive(true);
            }
            else
            {
                registrationFailedMessage.SetActive(true);
            }
        });
    }

    // Show the dialogue panel with NPC data
    public void ShowDialoguePanel(NPCDialogue dialogueData, int dialogueIndex)
    {
        dialoguePanel.SetActive(true);
        if(dialogueData == null)
        {
            Debug.LogError("Dialogue data is null");
            return;
        }

        demonstration_npcImage.sprite = dialogueData.npcSprite;
    }

    // Hide the dialogue panel
    public void HideDialoguePanel()
    {
        dialoguePanel.SetActive(false);
    }

    // Display dialogue text (with typing effect managed by UIManager)
    public IEnumerator TypeDialogueText(string text, float typingSpeed, System.Action onComplete = null)
    {
        demonstration_dialogueText.SetText("");
        
        foreach(char letter in text.ToCharArray())
        {
            demonstration_dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        if (onComplete != null)
            onComplete();
    }

    // Set the dialogue text directly
    public void SetDialogueText(string text)
    {
        demonstration_dialogueText.SetText(text);
    }

    // Show dialogue image
    public void ShowDialogueImage(Sprite npcSprite, Sprite dialogueImage)
    {
        // Set up the graphics panel
        graphicsInstructorImage.sprite = npcSprite;
        graphicsPanel.SetActive(true);
        graphicsImagePanel.SetActive(true);
        npcImagePanel.SetActive(false);
        
        // Set and scale the image
        graphicsImage.sprite = dialogueImage;
        graphicsImage.SetNativeSize();
        
        // Scale the image if needed
        RectTransform rt = graphicsImage.rectTransform;
        float currentWidth = rt.rect.width;
        float currentHeight = rt.rect.height;
        
        // Maximum allowed dimensions
        float maxWidth = 500f;
        float maxHeight = 300f;
        
        // Scale if image exceeds maximum dimensions
        if (currentWidth > maxWidth || currentHeight > maxHeight)
        {
            float widthScale = maxWidth / currentWidth;
            float heightScale = maxHeight / currentHeight;
            float scale = Mathf.Min(widthScale, heightScale);
            rt.sizeDelta = new Vector2(currentWidth * scale, currentHeight * scale);
        }
    }

    // Show NPC image without graphics
    public void ShowNPCImageOnly(Sprite npcSprite)
    {
        npcImagePanel.SetActive(true);
        graphicsPanel.SetActive(false);
        demonstration_npcImage.sprite = npcSprite;
    }

    // Start flashing the enter key
    public Coroutine StartFlashingEnterKey(float flashRate = 0.5f)
    {
        return StartCoroutine(FlashEnterKeyCoroutine(flashRate));
    }

    // Stop flashing the enter key
    public void StopFlashingEnterKey(Coroutine flashingCoroutine)
    {
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            enterKey.gameObject.SetActive(false);
        }
    }

    // Coroutine for flashing enter key
    private IEnumerator FlashEnterKeyCoroutine(float flashRate)
    {
        enterKey.gameObject.SetActive(true);
        
        while (true)
        {
            yield return new WaitForSeconds(flashRate);
            enterKey.gameObject.SetActive(!enterKey.gameObject.activeSelf);
        }
    }

    // Set up dialogue button listeners
    public void SetupDialogueButtons(System.Action onPause, System.Action onResume, System.Action onExit)
    {
        if (closeDialogue != null)
            closeDialogue.onClick.RemoveAllListeners();
            
        if (continueDialogue != null)
            continueDialogue.onClick.RemoveAllListeners();
            
        if (exitDialogue != null)
            exitDialogue.onClick.RemoveAllListeners();
            
        closeDialogue.onClick.AddListener(() => onPause());
        continueDialogue.onClick.AddListener(() => onResume());
        exitDialogue.onClick.AddListener(() => onExit());
    }

    #region Question UI Methods

    public void ShowQuestionPanel(bool show)
    {
        questionsPanel.SetActive(show);
    }

    public void SetQuestionText(string text)
    {
        question_text.text = text;
    }

    public void SetupQuestionAnswerButtons(string[] answers, System.Action<int>[] callbacks)
    {
        for (int i = 0; i < question_answerButtons.Length; i++)
        {
            if (i < answers.Length)
            {
                question_answerButtons[i].gameObject.SetActive(true);
                question_answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i];
                
                // Remove previous listeners and add new one
                question_answerButtons[i].onClick.RemoveAllListeners();
                
                // Capture the index for the closure
                int capturedIndex = i;
                
                // Add new listener with captured index
                question_answerButtons[i].onClick.AddListener(() => {
                    callbacks[capturedIndex]?.Invoke(capturedIndex);
                });
            }
            else
            {
                question_answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void DisableQuestionAnswerButtons()
    {
        foreach (Button button in question_answerButtons)
        {
            button.interactable = false;
        }
    }

    public void ResetQuestionAnswerButtons()
    {
        foreach (Button button in question_answerButtons)
        {
            button.interactable = true;
        }
    }

    #endregion

    #region Evaluation UI Methods

    public void SetupEvaluationUI(string playerName, string npcName, Sprite playerSprite, Sprite enemySprite, Sprite backgroundSprite)
    {
        // Set up player info
        playerNameText.text = playerName;
        playerLevelText.text = "Lvl 1";
        playerImage.sprite = playerSprite;
        
        // Set up enemy info
        npcNameText.text = npcName;
        npcLevelText.text = "Lvl 1";
        eval_npcImage.sprite = enemySprite;
        
        // Set up backgrounds
        battleBackground.sprite = backgroundSprite;
        battleIntroBackground.sprite = backgroundSprite;
        
        // Reset health bars
        playerHealthBar.localScale = new Vector3(1f, 1f, 1f);
        npcHealthBar.localScale = new Vector3(1f, 1f, 1f);
    }

    public void ShowEvaluationPanel(bool show)
    {
        evaluationPanel.SetActive(show);
    }

    public void ShowBattlePanel(bool show)
    {
        battlePanel.SetActive(show);
    }

    public void ShowNPCIntroPanel(bool show)
    {
        npcIntroPanel.SetActive(show);
    }

    public void ShowOptionPanel(bool show)
    {
        optionPanel.SetActive(show);
    }

    public void ShowPowerUpPanel(bool show)
    {
        power_up_panel.SetActive(show);
    }

    public void SetNPCIntroImage(Sprite sprite)
    {
        npcIntroImage.sprite = sprite;
    }

    public void ShowAnswerToolTip(){
        answerToolTip.SetActive(true);
    }

    public void HideAnswerToolTip(){
        answerToolTip.SetActive(false);
    }

    public void UpdateHealthBar(bool isPlayer, float healthValue)
    {
        healthValue = Mathf.Clamp(healthValue, 0f, 1f);
        
        if (isPlayer)
        {
            playerHealthBar.localScale = new Vector3(healthValue, 1f, 1f);
        }
        else
        {
            npcHealthBar.localScale = new Vector3(healthValue, 1f, 1f);
        }
    }

    public void SetupAnswerButtons(string[] answers, Action<int>[] callbacks)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < answers.Length)
            {
                TMP_Text answerText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                answerText.text = answers[i];
                
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].onClick.RemoveAllListeners();
                
                // Add click callback
                int capturedIndex = i;
                answerButtons[i].onClick.AddListener(() => callbacks[capturedIndex]?.Invoke(capturedIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ClearButtonEventTriggers()
    {
        foreach (Button button in answerButtons)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger.triggers.Clear();
            }
        }
    }

    public void showPowerUpTooltip(string powerUp)
    {
        powerUpTooltip.SetActive(true);

        if(powerUp=="Extra Time"){
            powerUpTooltipText.text = "Doubles time for each question";
        }
        else if(powerUp=="Hint Token"){
            powerUpTooltipText.text = "Removes two wrong options from one question";
        }
        else if(powerUp=="Power Reveal"){
            powerUpTooltipText.text = "Reveal the correct answer to one question";
        }
    }

    public void hidePowerUpTooltip()
    {
        powerUpTooltip.SetActive(false);
        powerUpTooltipText.text = "";
    }

    public void SetupPowerUpButton(int buttonIndex, bool interactable)
    {
        if (buttonIndex >= 0 && buttonIndex < power_up_buttons.Length)
        {
            power_up_buttons[buttonIndex].interactable = interactable;
        }
    }

    public void UpdateTimerText(float timeValue)
    {
        eval_time.text = Mathf.CeilToInt(timeValue).ToString();
    }

    public void ClearTimerText()
    {
        eval_time.text = "";
    }

    public IEnumerator FlashSprite(Image targetImage, float duration = 0.5f)
    {

        // Store the original sprite
        Sprite originalSprite = targetImage.sprite;
        Color originalColor = targetImage.color;
        
        // Flash duration and interval
        float endTime = Time.time + duration;
        float flashInterval = 0.1f;
        AudioController.Instance.PlayHit();
        // Flash the sprite by toggling visibility
        while (Time.time < endTime)
        {
            // Toggle visibility by changing alpha
            targetImage.color = new Color(
                originalColor.r, 
                originalColor.g, 
                originalColor.b, 
                targetImage.color.a > 0.5f ? 0.0f : 1.0f
            );
            
            yield return new WaitForSeconds(flashInterval);
        }
        
        // Ensure sprite is fully visible when done
        targetImage.sprite = originalSprite;
        targetImage.color = originalColor;
    }

    public IEnumerator TypeEvaluationText(string text, float typingSpeed, Action onComplete = null)
    {
        eval_dialogueText.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            eval_dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        onComplete?.Invoke();
    }

    public void SetupAnswerButtonHover(Button button, EventTriggerType eventID, UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventID;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    #endregion

    #region Shop UI Methods

    public void ShowShop(){
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
        }

        setCheckMarkActive(7);
        currentMenu = "shop";
        setGameUIPanelsInactive();

        // Show the shop panel
        shopPanel.SetActive(true);

        // Show the shop category
        ShowShopCategory(current_shop_category);
        // Track Shop usage
        DatabaseManager.Instance.UpdateMetric("shop_view");
    }
    // Show the shop panel
    public void ShowShopByCharacters(){
        current_shop_category = "character";
        ShowShopCategory("character");
    }

    public void ShowShopByMove(){
        current_shop_category = "Move";
        ShowShopCategory("Move");
    }

    public void ShowShopByBoosts(){
        current_shop_category = "Boost";
        ShowShopCategory("Boost");
    }
    
    // Display the shop panel and populate with appropriate items
    public void ShowShopCategory(string category)
    {
        // Update currency display
        UpdateShopCurrencyDisplay();
        
        // Populate items based on category
        PopulateShopItems(category);
    }

    // Hide the shop panel
    public void HideShop()
    {
        shopPanel.SetActive(false);
    }

    public void ShowItemInfo(string itemName, string description, string itemUse, int cost, Sprite itemSprite, Sprite currencySprite)
    {
        // Set the item info panel contents
        itemInfoName.text = itemName;
        itemInfoDescription.text = description;
        itemInfoUse.text = itemUse;
        itemInfoCost.text = cost.ToString();
        itemInfoSprite.sprite = itemSprite;
        itemInfoCurrencySprite.sprite = currencySprite;
        
        // Show the panel
        itemInfoPanel.SetActive(true);
    }

    // Add method to hide the info panel
    public void HideItemInfo()
    {
        itemInfoPanel.SetActive(false);
    }

    // Update the currency display in the shop
    public void UpdateShopCurrencyDisplay()
    {
        if (DatabaseManager.Instance.loggedInUser != null)
        {
            user_coins_text.text = DatabaseManager.Instance.loggedInUser.score.ToString();
            user_gems_text.text = DatabaseManager.Instance.loggedInUser.numGems.ToString();
        }
    }

    // Clear all items from the shop content panel
    public void ClearShopItems()
    {
        foreach (Transform child in shopContentPanel) {
            Destroy(child.gameObject);
        }
    }

    // Populate the shop with items from a specific category
    public void PopulateShopItems(string category)
    {
        // Clear existing items
        ClearShopItems();
        
        // Get items from ShopManager for the specified category
        List<ShopItemData> items = ShopManager.Instance.GetItemsForCategory(category);
        
        // Create UI elements for each item
        foreach (ShopItemData itemData in items)
        {
            CreateShopItemUI(itemData);
        }
    }

    // Create a UI element for a shop item
    public void CreateShopItemUI(ShopItemData itemData)
    {
        GameObject shopItemObject = Instantiate(shopItemPrefab, shopContentPanel);
        ShopItemUI shopItemUI = shopItemObject.GetComponent<ShopItemUI>();
        
        shopItemUI.itemCategory = itemData.category;
        shopItemUI.item = itemData.item;
        shopItemUI.UpdateUI(itemData.sprite, itemData.item.item_name, itemData.item.cost);
    }

    // Refresh all shop items (update their visual state)
    public void RefreshShopItems()
    {
        foreach (Transform child in shopContentPanel)
        {
            child.GetComponent<ShopItemUI>().refreshItem();
        }
    }

    #endregion

    // Clear dialogue button listeners
    public void ClearDialogueButtonListeners()
    {
        if (closeDialogue != null)
            closeDialogue.onClick.RemoveAllListeners();
            
        if (continueDialogue != null)
            continueDialogue.onClick.RemoveAllListeners();
            
        if (exitDialogue != null)
            exitDialogue.onClick.RemoveAllListeners();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
