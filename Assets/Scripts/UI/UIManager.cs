using UnityEngine;
using TMPro;  // Import TextMeshPro
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
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

    //player hud variables
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;
    public Transform playerHealthBar;
    public Button closeDialogue;
    public Button continueDialogue;
    public Button exitDialogue;

    [Header("UI Checkmarks")]
    public Toggle [] checkmarks;
    public GameObject [] checkmarkPanels;

    //npc hud variables
    public TMP_Text npcNameText;
    public TMP_Text npcLevelText;
    public Transform npcHealthBar;

    //intro panel variables
    public Image npcIntroImage;
    public TMP_Text npcIntroNameText;
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

    [Header("Player HUD")]
    public GameObject playerHUD;
    public TextMeshProUGUI playerCoins;
    public TextMeshProUGUI playerGems;

    public static UIManager Instance { get; private set; } // Singleton instance
    public bool isMenuOpen { get; private set; } = false;


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

    public void OnToggleMenu()
    {
        if(!isInGame || (EvaluationManager.Instance!=null && EvaluationManager.Instance.isEvaluating) || (NPCManager.Instance!=null && NPCManager.Instance.isInstructing) || (RagChatManager.Instance!=null && RagChatManager.Instance.isUsingAssistant)){
            if(inGameUiPanel.activeSelf){
                inGameUiPanel.SetActive(false);
                isMenuOpen = false;
            }
            return;
        }

        if(IsPointerOverInputField()){
            return;
        }

        if(Player.Instance.stop_interaction){
            return;
        }


        // Toggle the inGameUiPanel active state
        if (inGameUiPanel != null)
        {
            inGameUiPanel.SetActive(!inGameUiPanel.activeSelf);
            isMenuOpen = !isMenuOpen;
            if (isMenuOpen)
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
    
    public void showHelp(){
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

    public void showLandingSettings(){
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
        isMenuOpen = false;
    }

    public void StartNextLevel(){
        setGameUIPanelsInactive();
        Player.Instance.resumeInteraction();
        User loggedInUser = DatabaseManager.Instance.loggedInUser;
        if(loggedInUser.currentLevel<loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels.Count){
            startLevel(loggedInUser.currentLevel+1);
        }  
    }

    public void disablePlayerHUD(){
        playerHUD.SetActive(false);
        helpButton.SetActive(false);
    }
    
    public void enablePlayerHUD(){
        playerHUD.SetActive(true);
        helpButton.SetActive(true);
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
            isMenuOpen=true;
        }
        setCheckMarkActive(4);
        currentMenu = "character";
        setGameUIPanelsInactive();
        characterSelectionPanel.SetActive(true);
        characterSelectionPanel.GetComponent<PlayerSelector>().ShowCharacters();
    }

    public void ShowGameLevels(){
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
            isMenuOpen=true;
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

    public void ShowShop() {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
            isMenuOpen=true;
        }

        setCheckMarkActive(7);
        currentMenu = "shop";
        setGameUIPanelsInactive();
        shopPanel.SetActive(true);
        ShopManager.Instance.ShowShop();
    }

    public void ShowShopByCharacters(){
        ShopManager.Instance.PopulateCharacterItems();
    }

    public void ShowShopByMove(){
        ShopManager.Instance.PopulateMoveItems();
    }

    public void ShowShopByBoosts(){
        ShopManager.Instance.PopulateBoostItems();
    }
    
    // Add this to your existing UIManager.cs
    public void ShowChat()
    {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
            isMenuOpen=true;
        }

        setCheckMarkActive(3); // Assuming chat is index 1
        currentMenu = "chat";
        setGameUIPanelsInactive();
        chatPanel.SetActive(true);
    }

    public void ShowObjectives() {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
            isMenuOpen=true;
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
      
    }

    public void ShowSettings(){
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
            isMenuOpen=true;
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
            isMenuOpen=true;
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
            isMenuOpen = true;
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
   
    private void Start(){
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
        setPanelsInactive();
        setGameUIPanelsInactive();
        inGameUiPanel.SetActive(false);
        settingsButton.SetActive(false);    
        // Load game data at the start of the level
        CourseManager.Instance.StartLevel(level, (success) =>{
            if (success)
            {
                landingPanel.SetActive(false);
                isInGame = true;
                playerHUD.SetActive(true);
                updatePlayerCoins();
                updatePlayerGems();
            }
            else
            {
                Debug.LogError("Failed to load level data");
            }
        });
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
        playerHUD.SetActive(false);
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
