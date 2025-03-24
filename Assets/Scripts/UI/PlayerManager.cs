using UnityEngine;
using UnityEngine.U2D.Animation; // Required for Sprite Library
using System.Collections.Generic;

[System.Serializable]
public class CharacterSpriteLibrary
{
    public string characterName; // The name of the character
    public SpriteLibraryAsset spriteLibraryAsset; // The corresponding Sprite Library Asset
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; } // Singleton instance

    public SpriteLibrary spriteLibrary; // Reference to the Sprite Library component on the active player

    // List of character names and their corresponding sprite libraries (assignable in the Inspector)
    public List<CharacterSpriteLibrary> characterSpriteLibraryList = new List<CharacterSpriteLibrary>();

    // Dictionary to map character names to their Sprite Library Assets (populated at runtime)
    private Dictionary<string, SpriteLibraryAsset> characterSpriteLibraries = new Dictionary<string, SpriteLibraryAsset>();

    private void Awake()
    {
        // Ensure only one instance of PlayerManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }

        // Populate the dictionary from the list
        InitializeCharacterSpriteLibraries();
    }

    // Method to populate the dictionary from the list
    private void InitializeCharacterSpriteLibraries()
    {
        characterSpriteLibraries.Clear();

        foreach (var entry in characterSpriteLibraryList)
        {
            if (!characterSpriteLibraries.ContainsKey(entry.characterName) && entry.spriteLibraryAsset != null)
            {
                characterSpriteLibraries.Add(entry.characterName, entry.spriteLibraryAsset);
            }
        }
    }

    // Method to set the active player's appearance by character name
    public void SetActivePlayerAppearance(string characterName)
    {
        if (spriteLibrary == null)
        {
            Debug.LogError("Sprite Library is not assigned to the PlayerManager!");
            return;
        }

        if (!characterSpriteLibraries.ContainsKey(characterName))
        {
            Debug.LogError($"Character name '{characterName}' not found in the sprite library dictionary!");
            return;
        }

        SpriteLibraryAsset newLibraryAsset = characterSpriteLibraries[characterName];

        if (newLibraryAsset == null)
        {
            Debug.LogError($"Sprite Library Asset for character '{characterName}' is null!");
            return;
        }

        // Swap the Sprite Library Asset
        spriteLibrary.spriteLibraryAsset = newLibraryAsset;
        spriteLibrary.RefreshSpriteResolvers(); // Refresh to apply changes
        Debug.Log($"Switched to new Sprite Library for character: {characterName}");
    }
}