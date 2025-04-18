using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }
    
    // Track consecutive correct answers in the current level
    private int consecutiveCorrectAnswers = 0;
    // Track levels where all objectives were completed
    private HashSet<string> levelsWithAllObjectivesCompleted = new HashSet<string>();
    
    private bool combo_coder_unlocked = false;
    
    // Queue for achievement processing
    private Queue<string> achievementQueue = new Queue<string>();
    public bool isProcessingAchievements = false;
    
    public List<Achievement> Achievements { get; private set; } = new List<Achievement>();
    
    private void Awake()
    {
        // Singleton pattern
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
    
    /// <summary>
    /// Load achievements from the database
    /// </summary>
    public void LoadAchievements(Action onComplete = null)
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager is not available. Cannot load achievements.");
            onComplete?.Invoke();
            return;
        }

        DatabaseManager.Instance.GetUserAchievements((achievements) =>
        {
            if (achievements != null)
            {
                Achievements = achievements;
                Debug.Log($"Loaded {achievements.Count} achievements.");
            }
            else
            {
                Debug.LogWarning("Failed to load achievements.");
                Achievements = new List<Achievement>();
            }

            onComplete?.Invoke(); // Notify that achievements have been loaded
        });
    }

    // Call this when an answer is submitted
    public void TrackAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            consecutiveCorrectAnswers++;
            if (consecutiveCorrectAnswers >= 3)
            {
                combo_coder_unlocked = true;
            }
        }
        else
        {
            consecutiveCorrectAnswers = 0;
        }
    }
    
    // Call this when all objectives in a level are completed
    public void CheckObjectiveCompletion()
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.loggedInUser == null)
            return;
            
        User user = DatabaseManager.Instance.loggedInUser;
        CourseStructure courseStructure = user.courseStructure;
        
        if (courseStructure == null)
            return;
        
        // Count completed levels across all chapters
        int completedLevels = 0;
        
        foreach (Chapter chapter in courseStructure.chapters)
        {
            foreach (Level level in chapter.levels)
            {
                // Check if level is marked as complete
                if (level.isCompleted)
                {
                    completedLevels++;
                    
                    // Check for Objective Overlord achievement once we have 3+ completed levels
                    if (completedLevels >= 3)
                    {
                        EnqueueAchievement("Objective Overlord");
                        return;
                    }
                }
            }
        }
       
    }
    
    // Reset tracking at the start of a new level
    public void StartLevel()
    {
        consecutiveCorrectAnswers = 0;
    }
    
    // Check achievements when a level is completed
    public void CheckAchievements(string levelName, float timeTaken, int totalQuestions, int correctAnswers, bool isFailed)
    {
        // Your existing implementation, but replace all direct calls to UnlockAchievement with EnqueueAchievement
        
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.loggedInUser == null)
            return;
            
        User user = DatabaseManager.Instance.loggedInUser;
        CourseStructure courseStructure = user.courseStructure;
        if (courseStructure == null)
            return;
        
        if(!isFailed)
        {
            // First Steps - Complete your first level
            EnqueueAchievement("First Steps");
        }
        
        CheckObjectiveCompletion();

        // Calculate accuracy
        float accuracy = totalQuestions > 0 ? (float)correctAnswers / totalQuestions : 0f;
        int wrongAnswers = totalQuestions - correctAnswers;
        
        if(combo_coder_unlocked)
        {
            EnqueueAchievement("Combo Coder");
            combo_coder_unlocked = false;
        }

        // Sharp Thinker - Finish with at most 1 wrong answer
        if (!isFailed && wrongAnswers <= 1)
        {
            EnqueueAchievement("Sharp Thinker");
        }
        
        // Syntax Sorcerer - Finish with no incorrect answers
        if (!isFailed && wrongAnswers == 0)
        {
            EnqueueAchievement("Syntax Sorcerer");
        }
        
        // Quick Commit - Complete a level within 3 minutes
        if (!isFailed && timeTaken <= 180f)
        {
            EnqueueAchievement("Quick Commit");
        }
        
        // Time Bender - 100% accuracy in under 2 minutes
        if (!isFailed && wrongAnswers == 0 && timeTaken <= 120f)
        {
            EnqueueAchievement("Time Bender");
        }
        
        if (!isFailed)
        {
            user.consecutiveLevelsWithoutFailure++;
            
            if (user.consecutiveLevelsWithoutFailure >= 3)
            {
                EnqueueAchievement("Code Streak");
            }
        }
        else
        {
            user.consecutiveLevelsWithoutFailure = 0;
        }
        
        if (user.currentLevel >= 5)
        {
            EnqueueAchievement("Knowledge Climber");
        }
        
        CheckFinalLevel(levelName, courseStructure, isFailed);
        
        // After queueing all potential achievements, start processing them
        ProcessAchievementQueue();
    }
    
    private void CheckFinalLevel(string levelName, CourseStructure courseStructure, bool isFailed)
    {
        if (isFailed)
            return;
            
        if (courseStructure.chapters.Count > 0)
        {
            Chapter lastChapter = courseStructure.chapters[courseStructure.chapters.Count - 1];
            if (lastChapter.levels.Count > 0)
            {
                Level lastLevel = lastChapter.levels[lastChapter.levels.Count - 1];
                if (lastLevel.level_name == levelName)
                {
                    EnqueueAchievement("The Final Debugger");
                }
            }
        }
    }
    
    // Enqueue an achievement to be processed
    private void EnqueueAchievement(string achievementName)
    {
        // Only add if not already in the queue
        if (!achievementQueue.Contains(achievementName))
        {
            achievementQueue.Enqueue(achievementName);
            Debug.Log($"Enqueued achievement: {achievementName}");
        }
    }
    
    // Start processing the achievement queue if not already processing
    private void ProcessAchievementQueue()
    {
        if (!isProcessingAchievements && achievementQueue.Count > 0)
        {
            isProcessingAchievements = true;
            StartCoroutine(ProcessAchievementQueueCoroutine());
        }
    }
    
    // Process achievements one by one
    private IEnumerator ProcessAchievementQueueCoroutine()
    {
        Player.Instance.pausePlayer();
        while (achievementQueue.Count > 0)
        {
            string achievementName = achievementQueue.Dequeue();
            
            // Call the API to unlock the achievement
            bool unlockCompleted = false;
            bool isAchievementSuccess = false;
            DatabaseManager.Instance.UnlockAchievement(achievementName, (success) => {
                unlockCompleted = true;
                if (success)
                {
                    Debug.Log($"Achievement '{achievementName}' unlocked successfully!");
                    isAchievementSuccess = true;
                    updateUserGems(achievementName);
                }
                else
                {
                    Debug.LogError($"Failed to unlock achievement: {achievementName}");
                }
            });
            
            // Wait for the API call to complete
            yield return new WaitUntil(() => unlockCompleted);

            if (isAchievementSuccess)
            {
                yield return StartCoroutine(UIManager.Instance.ShowAchievementUnlocked(achievementName));
            }
            
            
        }
        
        isProcessingAchievements = false;
        Player.Instance.resumePlayer();
    }

    public void updateUserGems(string achievement_name){
        foreach(Achievement achievement in Achievements){
            if(achievement.achievement_name==achievement_name){
                DatabaseManager.Instance.loggedInUser.numGems+=achievement.gems;
                return;
            }
        }
    }
    
    // Unlock an achievement directly (for backward compatibility)
    private void UnlockAchievement(string achievementName)
    {
        EnqueueAchievement(achievementName);
        ProcessAchievementQueue();
    }
}