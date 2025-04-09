using System;
using System.Collections.Generic;
using UnityEngine;

public class CourseManager : MonoBehaviour
{
    public static CourseManager Instance { get; private set; }

    public CourseStructure CurrentCourseStructure { get; private set; }
    public List<Course> UserCourses { get; private set; } = new List<Course>();

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

    /// <summary>
    /// Get all courses started by the user
    /// </summary>
    public void GetUserCourses(Action<List<Course>> onComplete)
    {
        DatabaseManager.Instance.GetUserCourses((courses) =>
        {
            if (courses != null)
            {
                UserCourses = new List<Course>(courses);
                Debug.Log($"Loaded {courses.Length} user courses.");
            }
            else
            {
                Debug.LogWarning("Failed to load user courses.");
                UserCourses.Clear();
            }

            onComplete?.Invoke(UserCourses);
        });
    }

    /// <summary>
    /// Get the structure of the selected course
    /// </summary>
    public void GetCourseStructure(Action<CourseStructure> onComplete)
    {
        DatabaseManager.Instance.GetCourseStructure((courseStructure) =>
        {
            if (courseStructure != null)
            {
                CurrentCourseStructure = courseStructure;
                Debug.Log($"Loaded course structure for {courseStructure.course_name}.");
            }
            else
            {
                Debug.LogWarning("Failed to load course structure.");
                CurrentCourseStructure = null;
            }

            onComplete?.Invoke(CurrentCourseStructure);
        });
    }

    /// <summary>
    /// Start a new course or continue an existing one
    /// </summary>
    public void StartCourse(string courseName, Action<bool> onComplete)
    {
        DatabaseManager.Instance.startCourse(courseName, (success) =>
        {
            if (success)
            {
                Debug.Log($"Successfully started course: {courseName}");
            }
            else
            {
                Debug.LogError($"Failed to start course: {courseName}");
            }

            onComplete?.Invoke(success);
        });
    }

    /// <summary>
    /// Start a specific level in the current course
    /// </summary>
    public void StartLevel(int level, Action<bool> onComplete)
    {
        DatabaseManager.Instance.loggedInUser.currentLevel = level;
        DatabaseManager.Instance.StartLevel((bool success) =>
        {
            if (success)
            {
                Debug.Log($"Successfully started level {level}.");
                ObjectiveManager.Instance.LoadObjectives();
                ShopManager.Instance.LoadShopItems();
                AchievementManager.Instance.LoadAchievements();
                StartCoroutine(TransitionManager.Instance.transition(level));
                EvaluationManager.Instance.LoadQuestionsForLevel();
                DatabaseManager.Instance.loggedInUser.currentLevel = level;
                DatabaseManager.Instance.StartLevelTime();
            }
            else
            {
                Debug.LogError($"Failed to start level {level}.");
            }

            onComplete?.Invoke(success);
        });
    }

    /// <summary>
    /// Delete a saved game for a specific course
    /// </summary>
    public void DeleteSavedGame(string courseName, Action<bool> onComplete)
    {
        DatabaseManager.Instance.deleteSavedGame(courseName, (success) =>
        {
            if (success)
            {
                Debug.Log($"Successfully deleted saved game for course: {courseName}");
            }
            else
            {
                Debug.LogError($"Failed to delete saved game for course: {courseName}");
            }

            onComplete?.Invoke(success);
        });
    }
}