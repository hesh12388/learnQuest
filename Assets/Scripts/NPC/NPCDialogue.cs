using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "NPC Dialogue")] 
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcSprite;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;
    public float autoProgressDelay = 1.5f;
    public string dialogueImagesFolder = "DialogueImages";

    [System.NonSerialized] public string[] dialogue;
    [System.NonSerialized] public bool[] autoProgressLines;
    [System.NonSerialized] public Sprite[] dialogueImages;
    [System.NonSerialized] public string[] requiredPreviousDialogues;

    private static Dictionary<string, NPCDialogueData> dialogueDictionary;

    

    public void LoadDialogue()
    {
        if (dialogueDictionary == null)
        {
            LoadDialogueFromJSON();
        }

        if (dialogueDictionary == null)
        {
            Debug.LogError("Dialogue dictionary is still null after loading JSON!");
            return;
        }

        if (!dialogueDictionary.TryGetValue(npcName, out NPCDialogueData data))
        {
            Debug.LogError($"Dialogue not found for NPC: {npcName}. Check if the JSON contains the correct name.");
            return;
        }

        dialogue = data.dialogue;
        autoProgressLines = data.autoProgressLines;
        LoadDialogueImages(data.images);
        requiredPreviousDialogues = data.requiredPreviousDialogues;
    }
    
    private void LoadDialogueImages(string[] imageFileNames)
    {
        if (imageFileNames == null)
        {
            Debug.LogError($"No image data found for NPC: {npcName}");
            return;
        }
        
        dialogueImages = new Sprite[imageFileNames.Length];
        
        for (int i = 0; i < imageFileNames.Length; i++)
        {
            string imageName = imageFileNames[i];
            
            if (string.IsNullOrEmpty(imageName))
            {
                dialogueImages[i] = null;
                continue;
            }
            
            Sprite loadedSprite = Resources.Load<Sprite>(Path.Combine(dialogueImagesFolder, Path.GetFileNameWithoutExtension(imageName)));
            
            if (loadedSprite != null)
            {
                dialogueImages[i] = loadedSprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load image: {imageName} for dialogue line {i} of NPC: {npcName}");
                dialogueImages[i] = null;
            }
        }
    }

    private static void LoadDialogueFromJSON()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "NPCDialogues.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError("NPC dialogue JSON file not found at: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);

        try
        {
            NPCDialogueWrapper wrapper = JsonUtility.FromJson<NPCDialogueWrapper>(json);
            
            if (wrapper == null || wrapper.dialogues == null || wrapper.dialogues.Count == 0)
            {
                Debug.LogError("Dialogue data is empty! Check your JSON structure.");
                return;
            }

            dialogueDictionary = new Dictionary<string, NPCDialogueData>();

            foreach (var npcData in wrapper.dialogues)
            {
                dialogueDictionary[npcData.npcName] = npcData;
            }

            Debug.Log($"Successfully loaded {dialogueDictionary.Count} NPC dialogues.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error parsing JSON: " + ex.Message);
        }
    }

    [System.Serializable]
    private class NPCDialogueData
    {
        public string npcName;
        public string[] dialogue;
        public string[] images;
        public bool[] autoProgressLines;
        public string[] requiredPreviousDialogues;
    }

    [System.Serializable]
    private class NPCDialogueWrapper
    {
        public List<NPCDialogueData> dialogues;
    }

}