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
        {
            // Parse the UTC time string
            DateTime startDateTime;
            string format = "M/d/yyyy h:mm:ss tt";  // Same format as used later
            
            if (DateTime.TryParseExact(startTime, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal, out startDateTime))
            {
                // Convert UTC time to local time
                DateTime localDateTime = startDateTime.ToLocalTime();
                
                // Format the local time for display
                timeStarted.text = localDateTime.ToString("M/d/yyyy h:mm:ss tt");
            }
            else
            {
                // Fallback if parsing fails
                timeStarted.text = startTime;
            }
        }
        
        if (timePlayed != null)
        {
            DateTime startDateTime;
            // Make sure the format string matches EXACTLY what you're receiving
            string format = "M/d/yyyy h:mm:ss tt";  // Matches "3/24/2025 11:42:03 AM"
            
            // Debug both the input and the parsed result
            Debug.Log($"Input time string: {startTime}");
            
            if (DateTime.TryParseExact(startTime, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal, out startDateTime))
            {
                // Ensure we're comparing UTC time to UTC time
                DateTime utcNow = DateTime.UtcNow;
                TimeSpan playTimeSpan = utcNow - startDateTime;
                
                // Debug to verify the calculation
                Debug.Log($"Start time (UTC): {startDateTime:u}, Current time (UTC): {utcNow:u}, Difference: {playTimeSpan}");
                
                // Format time played to include days if applicable
                string formattedTime;
                if (playTimeSpan.Days > 0)
                {
                    formattedTime = $"{playTimeSpan.Days}d {playTimeSpan.Hours:D2}:{playTimeSpan.Minutes:D2}:{playTimeSpan.Seconds:D2}";
                }
                else
                {
                    formattedTime = playTimeSpan.ToString(@"hh\:mm\:ss");
                }
                
                timePlayed.text = formattedTime;
            }
            else
            {
                Debug.LogWarning($"Failed to parse date: '{startTime}' using format '{format}'");
                timePlayed.text = "Unknown";
            }
        }
        
    }

    public void deleteSavedGame(){
        AudioController.Instance.PlayButtonClick();
        string courseName = this.courseName.text;
        UIManager.Instance.deleteGame(courseName);
    }

    public void selectCourse(){
        AudioController.Instance.PlayButtonClick();
        string courseName = this.courseName.text;
        DatabaseManager.Instance.setUserCourse(courseName);
        UIManager.Instance.showLevels();
    }
}