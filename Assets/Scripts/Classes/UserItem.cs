// User-owned item class
[System.Serializable]
public class UserItem
{
    public string item_name;
    public string item_type;
    
    public UserItem(string name, string type)
    {
        this.item_name = name;
        this.item_type = type;
    }
}