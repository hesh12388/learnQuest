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
    public User(string email, string username, string createdAt, int score)
    {
        this.email = email;
        this.username = username; 
        this.createdAt = createdAt;
        this.score= score;
    }
}
