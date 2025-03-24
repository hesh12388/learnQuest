using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveCompleteUI : MonoBehaviour
{
    // UI Components
    public TextMeshProUGUI objectiveNameText;
    public TextMeshProUGUI pointsText;
    public void SetObjectiveCompleteData(string objective_name, int points)
    {
        objectiveNameText.text = objective_name;
        pointsText.text = "+" + points.ToString();
    }
    
   
}