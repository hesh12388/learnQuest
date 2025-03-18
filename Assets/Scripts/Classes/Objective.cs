// Define the Objective class first
[System.Serializable]
public class Objective
{
    public string objective_name;
    public string status;
    public string description;
    public int difficulty;
    public Objective(string name, string status, string description, int difficulty)
    {
        this.objective_name = name;
        this.status = status;
        this.description = description;
        this.difficulty = difficulty;
    }
}