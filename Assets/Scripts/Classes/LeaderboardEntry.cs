[System.Serializable]
public class LeaderboardEntry
{
    public int numAchievements;
    public int numGems;
    public string username;
    public int score;
    
    public LeaderboardEntry(string username, int score, int numAchievements, int numGems)
    {
        this.username = username;
        this.score = score;
        this.numAchievements = numAchievements;
        this.numGems = numGems;
    }

}