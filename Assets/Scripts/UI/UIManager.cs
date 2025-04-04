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
    public List<LeaderboardEntry> leaderboard;

    [Header("Objectives UI")]
    public Transform objectivesContentPanel;
    public GameObject objectivePrefab;
    public List<Objective> objectives_list;

    [Header("Achievement UI")]
    public Transform achivementContentPanel;
    public GameObject achievementPrefab;
    public List<Achievement> achievements_list;
    public GameObject achievementCompletedPanel;

    private ShopItemsResponse shop;
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
    [Header("Player HUD")]
    public GameObject playerHUD;
    public TextMeshProUGUI playerCoins;
    public TextMeshProUGUI playerGems;

    public static UIManager Instance { get; private set; } // Singleton instance
    public bool isMenuOpen { get; private set; } = false;


    private string currentMenu = "settings";
    public string current_shop_category = "Move";
    private string current_achievement_category="All";
    private string current_leaderboard_category="Score";
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
        if(!isInGame || (EvaluationManager.Instance!=null && EvaluationManager.Instance.isEvaluating) || (NPCManager.Instance!=null && NPCManager.Instance.isInstructing)){
            if(inGameUiPanel.activeSelf){
                inGameUiPanel.SetActive(false);
                isMenuOpen = false;
            }
            return;
        }

        if(IsPointerOverInputField()){
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
            else{
                if(currentMenu=="chat"){
                    // Disconnect from the chat server when this UI is disabled
                    StartCoroutine(DisconnectFromServer());
                }
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
        }
        else{
            settingsHelpPanel.SetActive(false);
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

    public IEnumerator ShowObjectiveComplete(string objective_name) {
        int points = 0;
        objectiveCompletionPanel.SetActive(true);
        
        // Find the objective in our local list
        foreach (Objective objective in objectives_list) {
            if (objective.objective_name == objective_name) {
                points = objective.points;
                
                // Update the objective status in our local list
                objective.status = "completed";
                break;
            }
        }

        DatabaseManager.Instance.loggedInUser.score+=points;
        updatePlayerCoins();
        objectiveCompletionPanel.GetComponent<ObjectiveCompleteUI>().SetObjectiveCompleteData(objective_name, points);
        AudioController.Instance.PlayObjectiveComplete();
        yield return new WaitForSeconds(2);
        objectiveCompletionPanel.SetActive(false);
        yield return new WaitForSeconds(2);
        // Show guidance for the next objective
        NPCManager.Instance.DetermineNextObjective();
        StartCoroutine(NPCManager.Instance.ShowNextObjective());
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
   
        ShopManager.Instance.getBoughtItems(shop);
        
    }

    public void ShowShopByCharacters(){
        current_shop_category = "character";
         ShopManager.Instance.PopulateCharacterItems(shop);
    }

    public void ShowShopByMove(){
        current_shop_category = "Move";
         ShopManager.Instance.PopulateMoveItems(shop);
    }

    public void ShowShopByBoosts(){
        current_shop_category = "Boost";
         ShopManager.Instance.PopulateBoostItems(shop);
    }
    
    private IEnumerator DisconnectFromServer()
    {
        if (ChatManager.Instance != null && ChatManager.Instance.IsConnected())
        {
            var task = ChatManager.Instance.Disconnect();
            while (!task.IsCompleted)
                yield return null;
                
            if (task.Exception != null)
                Debug.LogError($"Error disconnecting: {task.Exception.Message}");
        }
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
        
        if (objectives_list != null && objectives_list.Count > 0) {
            // Populate the panel with objective prefabs from cached data
            foreach (Objective objective in objectives_list) {
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
        } else {
            Debug.Log("No objectives available to display");
            
            // If objectives weren't loaded yet for some reason, load them now
            if (isInGame && (objectives_list == null || objectives_list.Count == 0)) {
                LoadLevelObjectives();
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
        
        if (achievements_list != null && achievements_list.Count > 0) {
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
    }


    public void ShowAllAchievements(){
        current_achievement_category = "All";
        // Clear any existing entries first
            foreach (Transform child in achivementContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (achievements_list != null && achievements_list.Count > 0)
            {
                achievements_list.Sort((a, b) => b.gems.CompareTo(a.gems));
                // Populate the leaderboard with entries
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
    }

    public void ShowCompletedAchievements(){
        current_achievement_category = "Completed";
        // Clear any existing entries first
            foreach (Transform child in achivementContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (achievements_list != null && achievements_list.Count > 0)
            {
                achievements_list.Sort((a, b) => b.gems.CompareTo(a.gems));
                // Populate the leaderboard with entries
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
    }

    public void ShowInProgressAchievements(){
        current_achievement_category = "InProgress";
        // Clear any existing entries first
            foreach (Transform child in achivementContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (achievements_list != null && achievements_list.Count > 0)
            {
                achievements_list.Sort((a, b) => b.gems.CompareTo(a.gems));
                // Populate the leaderboard with entries
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
    }

    public void ShowLeaderboard()
    {
        if(!isInGame){
            return;
        }
        if(!inGameUiPanel.activeSelf){
            inGameUiPanel.SetActive(true);
            isMenuOpen=true;
        }
        setCheckMarkActive(1);
        currentMenu = "leaderboard";
        setGameUIPanelsInactive();
        leaderboardPanel.SetActive(true);
        
        // Show loading while fetching leaderboard data
        ShowLoading();
        
        // Get leaderboard data from the database
        DatabaseManager.Instance.GetLeaderboard((leaderboardEntries) => {
            // Hide loading indicator when data is retrieved
            HideLoading();
            
           
            if (leaderboardEntries != null && leaderboardEntries.Count > 0)
            {
                
                leaderboard = leaderboardEntries;
                if(current_leaderboard_category=="Score"){
                    ShowLeaderboardbyScore();
                }
                else if(current_leaderboard_category=="Achievements"){
                    ShowLeaderboardbyAchievements();
                }
                else if(current_leaderboard_category=="Gems"){
                    ShowLeaderboardbyNumGems();
                }
            }
        });
    }

    public void ShowLeaderboardbyNumGems(){
        current_leaderboard_category="Gems";
        // Clear any existing entries first
            foreach (Transform child in leaderboardContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (leaderboard != null && leaderboard.Count > 0)
            {
                leaderboard.Sort((a, b) => a.numGems.CompareTo(b.numGems));
                int i=0;
                // Populate the leaderboard with entries
                foreach (LeaderboardEntry entry in leaderboard)
                {
                    GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContentPanel);
                    LeaderboardEntryUI entryUI = entryObject.GetComponent<LeaderboardEntryUI>();
                    
                    if (entryUI != null)
                    {
                        entryUI.SetEntryData(entry.username, entry.numGems, i);
                        i+=1;
                    }
                }
            }
    }

    public void ShowLeaderboardbyAchievements(){
        current_leaderboard_category="Achievements";
        // Clear any existing entries first
            foreach (Transform child in leaderboardContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (leaderboard != null && leaderboard.Count > 0)
            {
                leaderboard.Sort((a, b) => b.numAchievements.CompareTo(a.numAchievements));
                int i=0;
                // Populate the leaderboard with entries
                foreach (LeaderboardEntry entry in leaderboard)
                {
                    GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContentPanel);
                    LeaderboardEntryUI entryUI = entryObject.GetComponent<LeaderboardEntryUI>();
                    
                    if (entryUI != null)
                    {
                        entryUI.SetEntryData(entry.username, entry.numAchievements, i);
                        i+=1;
                    }
                }
            }
    }

    public void ShowLeaderboardbyScore(){
        current_leaderboard_category="Score";
        // Clear any existing entries first
            foreach (Transform child in leaderboardContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (leaderboard != null && leaderboard.Count > 0)
            {
                leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
                int i=0;
                // Populate the leaderboard with entries
                foreach (LeaderboardEntry entry in leaderboard)
                {
                    GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContentPanel);
                    LeaderboardEntryUI entryUI = entryObject.GetComponent<LeaderboardEntryUI>();
                    
                    if (entryUI != null)
                    {
                        entryUI.SetEntryData(entry.username, entry.score, i);
                        i+=1;
                    }
                }
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
        settingsButton.SetActive(false);
        DatabaseManager.Instance.loggedInUser.currentLevel = level;
        DatabaseManager.Instance.StartLevelTime();
        
        // Load game data at the start of the level
        LoadLevelObjectives();
        LoadShopItems();
        LoadAchievements();
        
        DatabaseManager.Instance.StartLevel();
        StartCoroutine(TransitionManager.Instance.transition(level));
        landingPanel.SetActive(false);
        isInGame = true;
        EvaluationManager.Instance.LoadQuestionsForLevel();
        
        playerHUD.SetActive(true);
        updatePlayerCoins();
        updatePlayerGems();
        // After loading the level, determine and show the first objective guidance
        StartCoroutine(ShowNextObjective());
    }

    public void updatePlayerCoins(){
        playerCoins.text = DatabaseManager.Instance.loggedInUser.score.ToString();
    }
    public void updatePlayerGems(){
        playerGems.text = DatabaseManager.Instance.loggedInUser.numGems.ToString();
    }

    IEnumerator ShowNextObjective(){
        while(NPCManager.Instance == null) {
            yield return null; // Wait until the next frame
        }

        yield return new WaitForSeconds(2);
        // Show the next objective
        NPCManager.Instance.DetermineNextObjective();
        StartCoroutine(NPCManager.Instance.ShowNextObjective());
    }

    // New method to preload achievements
    private void LoadAchievements() {
        // Only load if we don't already have achievements data cached
        if (achievements_list != null && achievements_list.Count > 0) return;
        
        DatabaseManager.Instance.GetUserAchievements((achievements) => {
            if (achievements != null) {
                achievements_list = achievements;
                Debug.Log($"Preloaded {achievements.Count} achievements");
            } else {
                Debug.LogWarning("Failed to preload achievements");
                achievements_list = new List<Achievement>();
            }
        });
    }

    // New method to preload shop items
    private void LoadShopItems() {
        // Only load if we don't already have shop data cached
        if (shop != null) return;
        
        DatabaseManager.Instance.GetShopItems((shopItemsResponse) => {
            if (shopItemsResponse != null) {
                shop = shopItemsResponse;
                Debug.Log("Preloaded shop items");
            } else {
                Debug.LogWarning("Failed to preload shop items");
            }
        });
    }

    // New method to load objectives for the current level
    private void LoadLevelObjectives() {
        // Show loading indicator
        ShowLoading();
        
        DatabaseManager.Instance.GetObjectives((objectives) => {
            // Hide loading indicator when data is retrieved
            HideLoading();
            
            if (objectives != null) {
                objectives_list = objectives;
                Debug.Log($"Loaded {objectives.Count} objectives for the current level");
            } else {
                Debug.LogWarning("Failed to load objectives or no objectives available");
                objectives_list = new List<Objective>();
            }
        });
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

        foreach(Achievement achievement in achievements_list){
            if(achievementName==achievement.achievement_name){
                achievementCompletedPanel.GetComponent<AchievementUnlocked>().SetAchievementUnlocked(achievementName, achievement.gems, achievement.description);
                DatabaseManager.Instance.loggedInUser.numGems+=achievement.gems;
                updatePlayerGems();
                break;
            }
        }

        AudioController.Instance.PlayAchievementComplete();
        yield return new WaitForSeconds(2);
        achievementCompletedPanel.SetActive(false);
       
    }
    public void showLevels() {
        AudioController.Instance.PlayMenuOpen();
        setPanelsInactive();
        ShowLoading();
        levelsPanel.SetActive(true);
        
        // Fetch course structure and display levels when ready
        DatabaseManager.Instance.GetCourseStructure((courseStructure) => {
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
        DatabaseManager.Instance.startCourse(courseName, (bool success) =>
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
            DatabaseManager.Instance.deleteSavedGame(deleteSavedGameId, (bool success) =>
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

        DatabaseManager.Instance.GetUserCourses((Course[] courses) =>
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
