using System;

[System.Serializable]
public class Course
{
    public string courseName;  // Name of the course
    public int numChapaters;
    public DateTime timeStarted; // Timestamp when the user started the course

    public Course(string courseName, int numChapaters, DateTime timeStarted)
    {
        this.courseName = courseName;
        this.numChapaters = numChapaters;
        this.timeStarted = timeStarted;
    }
}
