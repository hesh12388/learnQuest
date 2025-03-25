using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

[System.Serializable]
public class User
{
    public string email;
    public string username;
    public string createdAt;
    public string selectecCourse;
    public CourseStructure courseStructure;
    public int currentChapter;
    public int currentLevel;
    public int playerAvatar;
    public int score;
    public int numGems;
    public List<UserItem> purchasedItems; // New field for purchased items
    public string [] playerMoves;
    public string equippedCharacter;
    public string currentLevelStartTime;
    private float currentLevelScore;
    public int consecutiveLevelsWithoutFailure;
    public User(string email, string username, string createdAt, int score, int numGems, int consecutiveLevelsWithoutFailure)
    {
        this.email = email;
        this.username = username; 
        this.createdAt = createdAt;
        this.score = score;
        this.purchasedItems = new List<UserItem>(); // Initialize the list
        this.numGems = numGems;
        this.consecutiveLevelsWithoutFailure = consecutiveLevelsWithoutFailure;
    }
    
    // Helper method to check if an item is purchased
    public bool HasPurchasedItem(string itemName)
    {
        if (purchasedItems == null)
            return false;
            
        foreach(UserItem item in purchasedItems)
        {
            if(item.item_name == itemName)
                return true;
        }
        return false;
    }

    public void setLevelScore(float score)
    {
        currentLevelScore = score;
    }

    public float getLevelScore()
    {
        return currentLevelScore;
    }
    
    public float getLevelTime()
    {
        if (string.IsNullOrEmpty(currentLevelStartTime))
        {
            Debug.LogWarning("Level start time not set");
            return 0f;
        }
        
        try
        {
            // Parse the ISO 8601 format timestamp (e.g., "2023-05-15T14:30:45.123Z")
            DateTime startDateTime = DateTime.Parse(currentLevelStartTime, null, DateTimeStyles.RoundtripKind);
            
            // Get current time in UTC
            DateTime currentTime = DateTime.UtcNow;
            
            // Calculate the time span between now and when the level started
            TimeSpan timePlayed = currentTime - startDateTime;
            
            // Return seconds as float
            return (float)timePlayed.TotalSeconds;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calculating level time: {ex.Message}");
            return 0f;
        }
    }
}