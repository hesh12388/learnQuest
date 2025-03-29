using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerSelector : MonoBehaviour
{
    public Image playerImageUI; // Reference to the UI Image component
    public TextMeshProUGUI playerNameText; // Reference to the Player Name text


    private int currentIndex = 0; // Track the selected player

    // Use two lists to assign character names and images in the inspector
    public List<string> characterNamesList = new List<string>();
    public List<Sprite> characterSpritesList = new List<Sprite>();

    // Dictionary to store all characters (populated from the lists)
    private Dictionary<string, Sprite> allCharacters = new Dictionary<string, Sprite>();

    // Dictionary to store characters the user can select (based on purchased items)
    private Dictionary<string, Sprite> availableCharacters = new Dictionary<string, Sprite>();

    private List<string> characterNames = new List<string>(); // List of character names for navigation
    private List<Sprite> characterSprites = new List<Sprite>(); // List of character sprites for navigation

  

    void PopulateAllCharacters()
    {
        allCharacters.Clear();

        // Ensure the lists are of the same length
        if (characterNamesList.Count != characterSpritesList.Count)
        {
            Debug.LogError("Character names and sprites lists must have the same length!");
            return;
        }

        // Populate the dictionary
        for (int i = 0; i < characterNamesList.Count; i++)
        {
            allCharacters[characterNamesList[i]] = characterSpritesList[i];
        }
    }
    public void ShowCharacters()
    {
         // Populate the allCharacters dictionary from the lists
        PopulateAllCharacters();

        // Populate availableCharacters based on purchased items
        PopulateAvailableCharacters();

        // Initialize character names and sprites for navigation
        InitializeCharacterLists();
        // Update the player display whenever the component is enabled
        UpdatePlayerDisplay();
    }

    void PopulateAvailableCharacters()
    {
        // Ensure loggedInUser and purchasedItems are valid
        if (DatabaseManager.Instance.loggedInUser == null || 
            DatabaseManager.Instance.loggedInUser.purchasedItems == null)
        {
            Debug.LogWarning("No user logged in or no purchased items available");
            return;
        }

        // Clear the availableCharacters dictionary
        availableCharacters.Clear();

        // Iterate through purchased items and add characters to availableCharacters
        foreach (UserItem item in DatabaseManager.Instance.loggedInUser.purchasedItems)
        {
            if (item.item_type == "character" && allCharacters.ContainsKey(item.item_name))
            {
                Debug.Log($"Adding {item.item_name} to available characters");
                availableCharacters[item.item_name] = allCharacters[item.item_name];
            }
        }
    }

    void InitializeCharacterLists()
    {
        // Clear existing lists
        characterNames.Clear();
        characterSprites.Clear();

        // Populate the lists from availableCharacters
        foreach (var character in availableCharacters)
        {
            characterNames.Add(character.Key);
            characterSprites.Add(character.Value);
        }

        // Reset currentIndex to 0 if there are characters available
        if (characterNames.Count > 0)
        {
            currentIndex = 0;
        }
        else
        {
            Debug.LogWarning("No available characters to display");
        }
    }

    void UpdatePlayerDisplay()
    {
        // Check if there are characters to display
        if (characterNames.Count == 0 || characterSprites.Count == 0)
        {
            playerImageUI.sprite = null;
            playerNameText.text = "No Characters Available";
            return;
        }

        // Update UI with the current character's avatar and name
        playerImageUI.sprite = characterSprites[currentIndex];

        if(DatabaseManager.Instance.loggedInUser.equippedCharacter == characterNames[currentIndex])
        {
            playerNameText.text = "Equipped";
        }
        else
        {
            playerNameText.text = characterNames[currentIndex];
        }
    }

    public void NextPlayer()
    {
        currentIndex++;
        if (currentIndex >= characterNames.Count)
            currentIndex = 0; // Loop back to the first character

        UpdatePlayerDisplay();
    }

    public void PreviousPlayer()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = characterNames.Count - 1; // Loop back to the last character

        UpdatePlayerDisplay();
    }

    public void SelectPlayer()
    {
        if (characterNames.Count == 0)
        {
            Debug.LogWarning("No character selected because no characters are available");
            return;
        }

        if(DatabaseManager.Instance.loggedInUser.equippedCharacter == characterNames[currentIndex])
        {
            Debug.LogWarning("Character already equipped");
            return;
        }

        // Set the selected character in the loggedInUser
        DatabaseManager.Instance.loggedInUser.equippedCharacter = characterNames[currentIndex];
        playerNameText.text = "Equipped";

        PlayerManager.Instance.SetActivePlayerAppearance(characterNames[currentIndex]);
        Debug.Log($"Selected character: {characterNames[currentIndex]}");
    }
}