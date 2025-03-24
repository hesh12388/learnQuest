[System.Serializable]
public class Level
{
    public string level_name;
    public int score;
    public string status;
    public int levelNumber;
    public int points;
    public bool isCompleted;

    public Level(string name, int score, string status, int levelNumbe, int points, bool isCompeleted)
    {
        this.level_name = name;
        this.score = score;
        this.status = status;
        this.levelNumber = levelNumber;
        this.points = points;
        this.isCompleted = isCompleted;
    }
}