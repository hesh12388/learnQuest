using System;
using System.Globalization;
using UnityEngine;
using TMPro;

public class SavedGame : MonoBehaviour
{
    public TextMeshProUGUI courseName;
    public TextMeshProUGUI timeStarted;
    public TextMeshProUGUI timePlayed;

    public void SetSavedGameData(string course, DateTime startTime)
    {
        if (courseName != null)
            courseName.text = course;

        if (timeStarted != null)
        {
            // Convert UTC time to local time and display
            DateTime localDateTime = startTime.ToLocalTime();
            timeStarted.text = localDateTime.ToString("M/d/yyyy h:mm:ss tt");
        }

        if (timePlayed != null)
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan playTimeSpan = utcNow - startTime;

            Debug.Log($"Start time (UTC): {startTime:u}, Current time (UTC): {utcNow:u}, Difference: {playTimeSpan}");

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