using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float currentLevelStartTime;
    private float currentLevelScore;
    public User(string email, string username, string createdAt, int score, int numGems)
    {
        this.email = email;
        this.username = username; 
        this.createdAt = createdAt;
        this.score = score;
        this.purchasedItems = new List<UserItem>(); // Initialize the list
        this.numGems = numGems;
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

    public void startLevelTimer()
    {
        currentLevelStartTime = Time.time;
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
        float levelTime = Time.time - currentLevelStartTime;
        return levelTime;
    }
}