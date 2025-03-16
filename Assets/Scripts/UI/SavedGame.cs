using System;
using System.Globalization;
using UnityEngine;
using TMPro;

public class SavedGame : MonoBehaviour
{
    public TextMeshProUGUI courseName;
    public TextMeshProUGUI timeStarted;
    public TextMeshProUGUI timePlayed;

    public void SetSavedGameData(string course, string startTime)
    {
        if (courseName != null)
            courseName.text = course;
        
        if (timeStarted != null)
            timeStarted.text = startTime;
        

        if (timePlayed != null)
        {
            DateTime startDateTime;
            string format = "M/d/yyyy h:mm:ss tt";  // Matches "3/15/2025 12:53:12 AM"

            if (DateTime.TryParseExact(startTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDateTime))
            {
                TimeSpan playTimeSpan = DateTime.UtcNow - startDateTime.ToUniversalTime();
                timePlayed.text = playTimeSpan.ToString(@"hh\:mm\:ss");
            }
            else
            {
                timePlayed.text = "Invalid Start Time";
                Debug.LogError($"Failed to parse start time: {startTime}");
            }
        }
        
    }

    public void selectCourse(){
        string courseName = this.courseName.text;
        DatabaseManager.Instance.setUserCourse(courseName);
        UIManager.Instance.showLevels();
    }
}
