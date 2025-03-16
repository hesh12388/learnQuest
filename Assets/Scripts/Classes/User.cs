[System.Serializable]
public class User
{
    public string email;
    public string username;
    public string createdAt;
    public string selectecCourse;
    public CourseStructure courseStructure;

    public User(string email, string username, string createdAt)
    {
        this.email = email;
        this.username = username; 
        this.createdAt = createdAt;
    }
}
