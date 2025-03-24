[System.Serializable]
public class Achievement
{
    public string achievement_name;
    public string description;
    public int gems;
    public string status;
    
    public Achievement(string name, string description, int gems, string status)
    {
        this.achievement_name = name;
        this.description = description;
        this.gems = gems;
        this.status = status;
    }
}