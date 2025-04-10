using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelMessage : MonoBehaviour
{
    // UI Components
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI pointsText;
    public void SetLevelUpdateUI(float score, int bestScore, int points, bool isFailed)
    {
        scoreText.text = score.ToString();
        bestScoreText.text = " Best Score: " + bestScore.ToString();
        if(isFailed)
        {
            pointsText.text = "X0";
            return;
        }
        else{
            pointsText.text = "X" + points.ToString();
        }
    }

}