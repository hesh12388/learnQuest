using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : MonoBehaviour
{
    // UI Components
    public TextMeshProUGUI achievementNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI statusText;
    public GameObject completedIcon;
    public GameObject inProgressIcon;
    public GameObject statusPanel;
    public void SetAchievement(string achievement_name, bool isCompleted, string description, int gems)
    {
      achievementNameText.text = achievement_name;
      descriptionText.text = description;
      gemsText.text = "X" + gems.ToString();
      
      if (isCompleted)
      {
          statusText.text = "Completed";
          statusText.color = new Color(0.1f, 0.8f, 0.1f);
          GameObject completed = Instantiate(completedIcon, statusPanel.transform);
      }
      else
      {
          statusText.text = "In Progress";
          statusText.color= new Color(0.8f, 0.8f, 0.1f);
          GameObject inProgress = Instantiate(inProgressIcon, statusPanel.transform);
      }
    }
 
   
}