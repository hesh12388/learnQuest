using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUnlocked : MonoBehaviour
{
    // UI Components
    public TextMeshProUGUI achievementNameText;
    public TextMeshProUGUI achievementDescriptionText;
    public TextMeshProUGUI gemsText;
    public void SetAchievementUnlocked(string achievement_name, int gems, string description)
    {
        achievementNameText.text = achievement_name;
        achievementDescriptionText.text = description;
        gemsText.text = "+" + gems.ToString();
    }
    
   
}