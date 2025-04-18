using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();

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
    /// Fetch leaderboard data from the database
    /// </summary>
    public void LoadLeaderboard(Action<List<LeaderboardEntry>> onComplete)
    {
        DatabaseManager.Instance.GetLeaderboard((entries) =>
        {
            if (entries != null)
            {
                leaderboardEntries = entries;
                Debug.Log($"Loaded {entries.Count} leaderboard entries.");
            }
            else
            {
                Debug.LogWarning("Failed to load leaderboard data.");
                leaderboardEntries = new List<LeaderboardEntry>();
            }
            onComplete?.Invoke(leaderboardEntries);
        });
    }

    /// <summary>
    /// Get the cached leaderboard entries
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardEntries()
    {
        return leaderboardEntries;
    }

    /// <summary>
    /// Filter leaderboard by category
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardByCategory(string category)
    {
        switch (category.ToLower())
        {
            case "score":
                return new List<LeaderboardEntry>(leaderboardEntries.OrderByDescending(e => e.score));
            case "achievements":
                return new List<LeaderboardEntry>(leaderboardEntries.OrderByDescending(e => e.numAchievements));
            case "gems":
                return new List<LeaderboardEntry>(leaderboardEntries.OrderByDescending(e => e.numGems));
            default:
                Debug.LogWarning($"Unknown leaderboard category: {category}");
                return leaderboardEntries;
        }
    }
}