using UnityEngine;
using TMPro;  // Import TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject landingPage;
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;
    public GameObject loginSuccessPanel;
    public GameObject loginFailedMessage;

    [Header("Register Fields")]
    public TMP_InputField registerEmailField;
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public GameObject registrationSuccessPanel;
    public GameObject registrationFailedMessage;

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

    public void ShowLogin()
    {
        setPanelsInactive();
        loginPanel.SetActive(true);
    }

    public void setPanelsInactive(){
        landingPage.SetActive(false);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        registrationSuccessPanel.SetActive(false);
        loginSuccessPanel.SetActive(false);
        courseSelectionPanel.SetActive(false);
        savedGamesPanel.SetActive(false);
        courseSelectionErrorPanel.SetActive(false);
        blurPanel.SetActive(false);
        levelsPanel.SetActive(false);
    }

    public void ShowRegister()
    {
        setPanelsInactive();
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
        levelsPanel.SetActive(true);
        
        // Fetch course structure and display levels when ready
        DatabaseManager.Instance.GetCourseStructure((courseStructure) => {
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
        DatabaseManager.Instance.startCourse(courseName, (bool success) =>
        {
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
                    loginSuccessPanel.SetActive(true);
                }
                else
                {
                    loginFailedMessage.SetActive(true);
                }
            });
    }


    public void restartCourse(){
        string courseName = courseNameText.text;
        DatabaseManager.Instance.restartCourse(courseName, (bool success) =>
        {
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
        savedGamesPanel.SetActive(true); // Show Saved Games UI
       

        DatabaseManager.Instance.GetUserCourses((Course[] courses) =>
        {

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
                registrationSuccessPanel.SetActive(true);

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
