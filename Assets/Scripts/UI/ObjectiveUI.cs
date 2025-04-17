using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveUI : MonoBehaviour
{
    // UI Components
    public TextMeshProUGUI objectiveNameText;
    public GameObject starsPanel;
    public GameObject starPrefab;
    public GameObject emptyStarPrefab;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statusText;
    public GameObject completedIcon;
    public GameObject failedIcon;
    public GameObject inProgressIcon;
    public GameObject statusPanel;
    public TextMeshProUGUI pointsText;
    public void SetObjective(string objective_name, bool isCompleted, string description, int difficulty, int points)
    {
      objectiveNameText.text = objective_name;
      descriptionText.text = description;
      pointsText.text = "X" + points.ToString();
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

    public void showObjectiveInMap(){
        UIManager.Instance.showObjectiveInMap(objectiveNameText.text);
    }
   
}