[System.Serializable]
public class Level
{
    public string level_name;
    public int score;
    public string status;
    public int levelNumber;

    public Level(string name, int score, string status, int levelNumber)
    {
        this.level_name = name;
        this.score = score;
        this.status = status;
        this.levelNumber = levelNumber;
    }
}