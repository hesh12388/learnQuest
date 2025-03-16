using System.Collections.Generic;

[System.Serializable]
public class Chapter
{
    public string chapter_name;
    public string status;
    public List<Level> levels;

    public Chapter(string name, string status, List<Level> levels)
    {
        this.chapter_name = name;
        this.status = status;
        this.levels = levels;
    }
}
