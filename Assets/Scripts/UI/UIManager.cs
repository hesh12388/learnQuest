using UnityEngine;
using TMPro;  // Import TextMeshPro
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    [Header("Loading Screen")]
    public GameObject loadingPanel;

    [Header("Player Avatars")]
    public GameObject [] playerAvatarPrefabs; // List of available player avatar prefabs
    public GameObject currentPlayerAvatar;      // Reference to the current player in the scene
    private int currentAvatarIndex = 0;         // Track which avatar is currently active


    [Header("Navigation")]
    public GameObject characterSelectionPanel;
    public GameObject chatPanel;
    public GameObject leaderboardPanel;
    public GameObject skillsPanel;
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


    private ShopItemsResponse shop;
    public GameObject inGameUiPanel;
    public GameObject landingPanel;

    [Header("Evaluation UI")]
    public Image playerImage;
    public Image eval_npcImage;
    public TMP_Text eval_dialogueText;
    public Button[] answerButtons;
    
    public GameObject evaluationPanel;
    public GameObject battlePanel;
    public GameObject npcIntroPanel;

    //player hud variables
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;
    public Transform playerHealthBar;

    //npc hud variables
    public TMP_Text npcNameText;
    public TMP_Text npcLevelText;
    public Transform npcHealthBar;

    //intro panel variables
    public Image npcIntroImage;
    public TMP_Text npcIntroNameText;
    public GameObject optionPanel;


    [Header("Demonstration UI")]
    public Image demonstration_npcImage;
    public GameObject dialoguePanel;
    public TMP_Text demonstration_dialogueText;
    public GameObject graphicsPanel;
    public Image graphicsImage;
    public GameObject graphicsImagePanel;
    public Image graphicsInstructorImage;
    public GameObject questionsPanel;
    public Button[] demonstration_answerButtons;
    public GameObject npcImagePanel;

    public static UIManager Instance { get; private set; } // Singleton instance

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


    // Method to change the player avatar
    public void ChangePlayerAvatar(int avatarIndex)
    {
       
        
        // Save the selected index
        currentAvatarIndex = avatarIndex;
        
        // If we're in a scene with a player, swap the avatar
        SwapPlayerAvatar();
        
    }

    public void setGameUIPanelsInactive(){
        characterSelectionPanel.SetActive(false);
        chatPanel.SetActive(false);
        objectivesPanel.SetActive(false);
        settingsPanel.SetActive(false);
        achievementsPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        skillsPanel.SetActive(false);
        levelsGamePanel.SetActive(false);
    }
    public void ShowCharacterSelection()
    {
        setGameUIPanelsInactive();
        characterSelectionPanel.SetActive(true);
    }

    public void ShowGameLevels(){
        setGameUIPanelsInactive();
        levelsGamePanel.SetActive(true);
    }

    public void ShowShop() {
        setGameUIPanelsInactive();
        shopPanel.SetActive(true);
        
        // Show loading while fetching shop items
        ShowLoading();
        
        // Get shop items from the database
        DatabaseManager.Instance.GetShopItems((shopItemsResponse) => {
            // Hide loading indicator when data is retrieved
            HideLoading();
            
            shop=shopItemsResponse;
            ShopManager.Instance.getBoughtItems(shop);
        });
    }

    public void ShowShopByCharacters(){
         ShopManager.Instance.PopulateCharacterItems(shop);
    }

    public void ShowShopByInstructors(){
         ShopManager.Instance.PopulateInstructorItems(shop);
    }

    public void ShowShopByBoosts(){
         ShopManager.Instance.PopulateBoostItems(shop);
    }
    
    public void ShowChat(){
        setGameUIPanelsInactive();
        chatPanel.SetActive(true);
    }

    public void ShowObjectives(){
        setGameUIPanelsInactive();
        objectivesPanel.SetActive(true);
        
        // Show loading indicator while fetching objectives
        ShowLoading();
        
        // Get objectives from DatabaseManager
        DatabaseManager.Instance.GetObjectives((objectives) => {
            // Hide loading indicator when data is retrieved
            HideLoading();
            
            // Clear existing objectives first
            foreach (Transform child in objectivesContentPanel) {
                Destroy(child.gameObject);
            }
            
            if (objectives != null && objectives.Count > 0) {
                // Populate the panel with objective prefabs
                foreach (Objective objective in objectives) {
                    GameObject objectiveObject = Instantiate(objectivePrefab, objectivesContentPanel);
                    ObjectiveUI objectiveUI = objectiveObject.GetComponent<ObjectiveUI>();
                    
                    if (objectiveUI != null) {
                        // Determine if objective is completed
                        bool isCompleted = objective.status.ToLower() == "completed";
                        
                        // Set the objective data in the UI component
                        objectiveUI.SetObjective(objective.objective_name, isCompleted, objective.description, objective.difficulty);
                    }

                }
            }
        });
    }

    public void ShowSettings(){
        setGameUIPanelsInactive();
        settingsPanel.SetActive(true);
    }

    public void ShowAchievements(){
        setGameUIPanelsInactive();
        settingsPanel.SetActive(true);
    }

    public void ShowLeaderboard()
    {
        setGameUIPanelsInactive();
        leaderboardPanel.SetActive(true);
        
        // Show loading while fetching leaderboard data
        ShowLoading();
        
        // Get leaderboard data from the database
        DatabaseManager.Instance.GetLeaderboard((leaderboardEntries) => {
            // Hide loading indicator when data is retrieved
            HideLoading();
            
            // Clear any existing entries first
            foreach (Transform child in leaderboardContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (leaderboardEntries != null && leaderboardEntries.Count > 0)
            {
                leaderboardEntries.Sort((a, b) => b.score.CompareTo(a.score));
                leaderboard = leaderboardEntries;
                int i=0;
                // Populate the leaderboard with entries
                foreach (LeaderboardEntry entry in leaderboardEntries)
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
        });
    }

    public void ShowLeaderboardbyBestTime(){
        // Clear any existing entries first
            foreach (Transform child in leaderboardContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            if (leaderboard != null && leaderboard.Count > 0)
            {
                leaderboard.Sort((a, b) => a.bestTime.CompareTo(b.bestTime));
                int i=0;
                // Populate the leaderboard with entries
                foreach (LeaderboardEntry entry in leaderboard)
                {
                    GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContentPanel);
                    LeaderboardEntryUI entryUI = entryObject.GetComponent<LeaderboardEntryUI>();
                    
                    if (entryUI != null)
                    {
                        entryUI.SetEntryData(entry.username, entry.bestTime, i);
                        i+=1;
                    }
                }
            }
    }

    public void ShowLeaderboardbyAchievements(){
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

    public void ShowSkills(){
        setGameUIPanelsInactive();
        skillsPanel.SetActive(true);
    }

    // Swap the current player avatar with the selected one
    public void SwapPlayerAvatar()
    {
        // Find the current player avatar if not already tracked
        if (currentPlayerAvatar == null)
        {
            currentPlayerAvatar = GameObject.FindWithTag("Player");
            
            // If still null, the player might not be in the scene yet
            if (currentPlayerAvatar == null)
            {
                Debug.Log("No player avatar found in the scene");
                return;
            }
        }
        
        // Store the current position and rotation
        Vector3 position = currentPlayerAvatar.transform.position;
        Quaternion rotation = currentPlayerAvatar.transform.rotation;
        
        // Get any important components/values from the current avatar that need to be preserved
        // For example, you might need to save the current health, inventory, etc.
        // PlayerController playerController = currentPlayerAvatar.GetComponent<PlayerController>();
        // float health = playerController != null ? playerController.health : 100f;
        
        // Destroy the current avatar
        Destroy(currentPlayerAvatar);
        
        // Instantiate the new avatar prefab at the same position
        currentPlayerAvatar = Instantiate(playerAvatarPrefabs[currentAvatarIndex], position, rotation);
        
        // Restore any important components/values to the new avatar
        // playerController = currentPlayerAvatar.GetComponent<PlayerController>();
        // if (playerController != null) playerController.health = health;
        
        // Make sure the new avatar has the "Player" tag for future reference
        currentPlayerAvatar.tag = "Player";
    
    }
    private void Start(){
        StartCoroutine(showLanding());
    }

    IEnumerator showLanding(){
        yield return new WaitForSeconds(1);

        if(landingPage!=null){
            landingPage.SetActive(true);
        }
    }

    public void startLevel(int level){
        setPanelsInactive();
        DatabaseManager.Instance.loggedInUser.currentLevel = level;
        DatabaseManager.Instance.StartLevel();
        StartCoroutine(TransitionManager.Instance.transition(5));
        landingPanel.SetActive(false);
    }

    public void ShowLogin()
    {
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
        setPanelsInactive();
        registrationSuccessMessage.SetActive(false);
        registrationFailedMessage.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void BackToLanding()
    {
        setPanelsInactive();
        landingPage.SetActive(true);
    }

    public void showCourseSelection()
    {
        setPanelsInactive();
        courseSelectionPanel.SetActive(true);
    }
    
    public void showLevels() {
        setPanelsInactive();
        ShowLoading();
        levelsPanel.SetActive(true);
        
        // Fetch course structure and display levels when ready
        DatabaseManager.Instance.GetCourseStructure((courseStructure) => {
            HideLoading();
            if (courseStructure != null) {
                Debug.Log("Course structure retrieved successfully");
                currentChapterIndex = 0; // Reset to first chapter
                DisplayChapterLevels(courseStructure);
            } else {
                Debug.LogError("Failed to retrieve course structure");
                // Consider showing an error message to the user
            }
        });
    }

    // Display the levels for the current chapter
    private void DisplayChapterLevels(CourseStructure courseStructure) {

        DatabaseManager.Instance.loggedInUser.currentChapter = currentChapterIndex;
        // Clear existing level prefabs
        foreach (Transform child in levelsContentPanel.transform) {
            Destroy(child.gameObject);
        }
        
        if (courseStructure.chapters.Count == 0 || currentChapterIndex >= courseStructure.chapters.Count) {
            Debug.LogError("No chapters available or invalid chapter index");
            return;
        }
        
        // Get current chapter
        Chapter currentChapter = courseStructure.chapters[currentChapterIndex];
        
        chapterNameText.text = currentChapter.chapter_name;
        // Create level prefabs for each level in the chapter
        foreach (Level level in currentChapter.levels) {
            GameObject levelObject;
            
            // Determine if level is locked
            bool isLocked = level.status.ToLower() == "locked";
            
            // Instantiate appropriate prefab
            if (isLocked) {
                levelObject = Instantiate(lockedLevelPrefab, levelsContentPanel.transform);
                
            } else {
                levelObject = Instantiate(levelPrefab, levelsContentPanel.transform);
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
            DisplayChapterLevels(courseStructure);
        }
    }

    // Navigate to the previous chapter with looping behavior
    public void PreviousChapter() {
        CourseStructure courseStructure = DatabaseManager.Instance.loggedInUser.courseStructure;
        if (courseStructure != null && courseStructure.chapters.Count > 0) {
            // Using modulo with addition to wrap around to last chapter when at first chapter
            currentChapterIndex = (currentChapterIndex - 1 + courseStructure.chapters.Count) % courseStructure.chapters.Count;
            DisplayChapterLevels(courseStructure);
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


    public void restartCourse(){
        string courseName = courseNameText.text;
        ShowLoading();
        DatabaseManager.Instance.restartCourse(courseName, (bool success) =>
        {
            HideLoading();
            if (success)
            {
                setPanelsInactive();
                
                ShowSavedGames();
            }
            else
            {
               Debug.Log("Failed to restart course");
            }
        });
    }

    public void ShowSavedGames()
    {
        setPanelsInactive();
        ShowLoading();
        savedGamesPanel.SetActive(true); // Show Saved Games UI


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

    public void registerUser()
    {
        string email = registerEmailField.text;
        string username = registerUsernameField.text;
        string password = registerPasswordField.text;

        DatabaseManager.Instance.Register(email, username, password, (bool success) =>
        {
            if (success)
            {
                setPanelsInactive();
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
