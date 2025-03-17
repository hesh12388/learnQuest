using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSelector : MonoBehaviour
{
    public Sprite[] playerAvatars; // Array of player avatar images
    public string[] playerNames; // Array of player names/types

    public Image playerImageUI; // Reference to the UI Image component
    public TextMeshProUGUI playerNameText; // Reference to the Player Name text

    public Button leftButton, rightButton, selectButton; // UI Buttons

    private int currentIndex = 0; // Track the selected player

    void Start()
    {
        UpdatePlayerDisplay();

        // Add button listeners
        leftButton.onClick.AddListener(PreviousPlayer);
        rightButton.onClick.AddListener(NextPlayer);
        selectButton.onClick.AddListener(SelectPlayer);
        DatabaseManager.Instance.loggedInUser.playerAvatar = currentIndex;
    }

    void UpdatePlayerDisplay()
    {
        // Update UI with new player avatar & name
        playerImageUI.sprite = playerAvatars[currentIndex];
        playerNameText.text = playerNames[currentIndex];
    }

    public void NextPlayer()
    {
        currentIndex++;
        if (currentIndex >= playerAvatars.Length)
            currentIndex = 0; // Loop back to first player

        DatabaseManager.Instance.loggedInUser.playerAvatar = currentIndex;
        UpdatePlayerDisplay();
    }

    public void PreviousPlayer()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = playerAvatars.Length - 1; // Loop back to last player
        DatabaseManager.Instance.loggedInUser.playerAvatar = currentIndex;
        UpdatePlayerDisplay();
    }

    public void SelectPlayer()
    {
        UIManager.Instance.ChangePlayerAvatar(currentIndex);
    }

  
    private void OnDestroy()
    {
        // Remove listeners to prevent memory leaks
        if (leftButton != null)
            leftButton.onClick.RemoveListener(PreviousPlayer);
        
        if (rightButton != null)
            rightButton.onClick.RemoveListener(NextPlayer);
        
    }
}