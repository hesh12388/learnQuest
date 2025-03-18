using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI scoreText;
    public Image medalImage; 
    // Colors for different ranks
    private Color goldColor = new Color(1f, 0.84f, 0f); // Gold (#FFD700)
    private Color silverColor = new Color(0.75f, 0.75f, 0.75f); // Silver (#C0C0C0)
    private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f); // Bronze (#CD7F32)
    private Color defaultColor = new Color(0.5f, 0.35f, 0.05f); // Brown (#806020)

    public void SetEntryData(string username, int score, int index)
    {
        rankText.text = (index + 1).ToString();
        usernameText.text = username;
        scoreText.text = score.ToString();

        switch (index)
            {
                case 0: // First place (index 0)
                    medalImage.color = goldColor;
                    scoreText.color = goldColor;
                    break;
                case 1: // Second place (index 1)
                    medalImage.color = silverColor;
                    scoreText.color = silverColor;
                    break;
                case 2: // Third place (index 2)
                    medalImage.color = bronzeColor;
                    scoreText.color = bronzeColor;
                    break;
                default: // All other places
                    medalImage.color = defaultColor;
                    break;
            }
    }
}