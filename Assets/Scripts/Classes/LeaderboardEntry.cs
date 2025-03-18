[System.Serializable]
public class LeaderboardEntry
{
    public int numAchievements;
    public int bestTime;
    public string username;
    public int score;
    
    public LeaderboardEntry(string username, int score, int numAchievements, int bestTime)
    {
        this.username = username;
        this.score = score;
        this.numAchievements = numAchievements;
        this.bestTime = bestTime;
    }

}