using System.Collections.Generic;


[System.Serializable]
public class CourseStructure
{
    public string course_name;
    public int course_id;
    public int numChapters;
    public List<Chapter> chapters;

    public CourseStructure(string name, int id, int numChapters, List<Chapter> chapters)
    {
        this.course_name = name;
        this.course_id = id;
        this.numChapters = numChapters;
        this.chapters = chapters;
    }
}
