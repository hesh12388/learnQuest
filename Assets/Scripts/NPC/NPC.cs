using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    
    private Image npcImage;
    private GameObject dialoguePanel;
    private TMP_Text dialogueText;
    private GameObject graphicsPanel;
    private Image graphicsImage;
    private GameObject graphicsImagePanel;
    private Image graphicsInstructorImage;
    private GameObject npcImagePanel;
    private Button closeDialogue;
    private Button continueDialogue;
    private Button exitDialogue;
    private Image enterKey;
    public bool isInstructing;
    public bool isEvaluation;
    public bool isAssistant;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isOnPreRequisite = false;
    private Coroutine flashingCoroutine;
    private bool isPaused = false;

    private void Start(){
        npcImage=UIManager.Instance.demonstration_npcImage;
        dialoguePanel= UIManager.Instance.dialoguePanel;
        dialogueText= UIManager.Instance.demonstration_dialogueText;
        graphicsPanel= UIManager.Instance.graphicsPanel;
        graphicsImage= UIManager.Instance.graphicsImage;
        graphicsImagePanel= UIManager.Instance.graphicsImagePanel;
        graphicsInstructorImage= UIManager.Instance.graphicsInstructorImage;
        npcImagePanel= UIManager.Instance.npcImagePanel;
        closeDialogue= UIManager.Instance.closeDialogue;
        continueDialogue= UIManager.Instance.continueDialogue;
        exitDialogue= UIManager.Instance.exitDialogue;
        enterKey=UIManager.Instance.enterKey;
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if(!UIManager.Instance.isMenuOpen())
        {
            UIManager.Instance.setMenuOpen(false);
        }
        if( EvaluationManager.Instance.isEvaluating || isOnPreRequisite || isPaused || RagChatManager.Instance.isUsingAssistant)
        {
            return;
        }
        
        if(dialogueData == null)
        {
            Debug.LogWarning("No dialogue data assigned to this NPC");
            return;
        }

        isInstructing=true;
        NPCManager.Instance.isInstructing=true;
        Player.Instance.pausePlayer();
        // Start the interaction coroutine instead of doing everything immediately
        StartCoroutine(InteractSequence());
    }

    private IEnumerator InteractSequence()
    {
        if(isEvaluation)
        {
            if(ObjectiveManager.Instance.AreAllObjectivesCompleted()){
                EvaluationManager.Instance.StartEvaluation();
                isInstructing=false;
                NPCManager.Instance.isInstructing=false;
                yield break;
            }
            else{
                yield return StartCoroutine(EvaluationManager.Instance.NotReady());
                Player.Instance.resumePlayer();
                isInstructing=false;
                NPCManager.Instance.isInstructing=false;
                yield break;
            }
        }
        
        if(isAssistant)
        {
           RagChatManager.Instance.ShowRagPanel();
           Player.Instance.pausePlayer();
           Player.Instance.stopInteraction();
           UIManager.Instance.disablePlayerHUD();
           NPCManager.Instance.isInstructing=false;
           isInstructing=false;
           yield break;
        }
        if(!isDialogueActive){
            // Now load the dialogue (after transition is complete)
            dialogueData.LoadDialogue();
            // Wait for the transition to complete
            yield return StartCoroutine(TransitionManager.Instance.contentTransition());
            closeDialogue.onClick.AddListener(pauseDemonstration);
            continueDialogue.onClick.AddListener(resumeDemonstration);
            exitDialogue.onClick.AddListener(EndDialogue);
            AudioController.Instance.PlayMenuOpen();
        }

        
        for(int i=0; i<dialogueData.requiredPreviousDialogues.Length; i++)
        {
            if(!ObjectiveManager.Instance.IsObjectiveCompleted(dialogueData.requiredPreviousDialogues[i]))
            {
                isOnPreRequisite = true;
                DisplayPrerequisiteMessage("Hello there! You are not quite ready for this lesson yet. You need to " + dialogueData.requiredPreviousDialogues[i] + " first.", dialogueData.requiredPreviousDialogues[i]);
                yield break;
            }
        }

      
        if(isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue(){
        UIManager.Instance.disablePlayerHUD();
        AudioController.Instance.PlayDemonstrationMusic();
        isDialogueActive=true;
        dialogueIndex=0;

        npcImage.sprite=dialogueData.npcSprite;

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeDialogue());
    }

    void NextLine(){
        AudioController.Instance.PlayButtonClick();
        if(isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogue[dialogueIndex]);
            isTyping=false;
            if (flashingCoroutine != null)
            {
                StopCoroutine(flashingCoroutine);
            }
            flashingCoroutine = StartCoroutine(FlashEnterKey());
        }
        else if(++dialogueIndex<dialogueData.dialogue.Length)
        {
          
            StartCoroutine(TypeDialogue());
            
        }
        else
        {
            EndDialogue();
        }
    }

    // Display a message when prerequisites aren't met
    public void DisplayPrerequisiteMessage(string message, string requiredNPC)
    {
        // Stop any ongoing dialogue
        StopAllCoroutines();
        UIManager.Instance.disablePlayerHUD();
        AudioController.Instance.PlayDemonstrationMusic();
        // Show the dialogue panel
        AudioController.Instance.PlayMenuOpen();
        dialoguePanel.SetActive(true);
        npcImage.sprite=dialogueData.npcSprite;
        // Display only the NPC image, not graphics
        npcImagePanel.SetActive(true);
        graphicsPanel.SetActive(false);
        
        // Start typing the prerequisite message
        StartCoroutine(TypePrerequisiteMessage(message, requiredNPC));
    }

    // Similar to TypeDialogue but for prerequisite messages
    private IEnumerator TypePrerequisiteMessage(string message, string requiredNPC)
    {
        // Set typing state
        isTyping = true;
        dialogueText.SetText("");
        
        // Type out the message character by character
        foreach(char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        
        isTyping = false;
        
        // Auto-close the message after a delay
        yield return new WaitForSeconds(3f);

    
        // Close dialogue and resume player
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        AudioController.Instance.PlayMenuOpen();
        dialoguePanel.SetActive(false);
        isInstructing = false;
        NPCManager.Instance.isInstructing=false;
        Player.Instance.resumePlayer();
        isOnPreRequisite = false;
        AudioController.Instance.PlayBackgroundMusic();
        UIManager.Instance.enablePlayerHUD();
    }

    IEnumerator TypeDialogue(){
        // Hide the enter key
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            enterKey.gameObject.SetActive(false);
        }
        
        if(dialogueData.dialogueImages!=null && dialogueData.dialogueImages.Length>dialogueIndex && dialogueData.dialogueImages[dialogueIndex]!=null)
        {
            graphicsInstructorImage.sprite=dialogueData.npcSprite;
            graphicsPanel.SetActive(true);
            graphicsImagePanel.SetActive(true);
            graphicsImage.sprite=dialogueData.dialogueImages[dialogueIndex];
            graphicsImage.SetNativeSize();
            // Get the current size after setting native size
            RectTransform rt = graphicsImage.rectTransform;
            float currentWidth = rt.rect.width;
            float currentHeight = rt.rect.height;
             
            // Set maximum allowed dimensions
            float maxWidth = 500f;
            float maxHeight = 450f;
            
            // Check if image exceeds maximum dimensions
            if (currentWidth > maxWidth || currentHeight > maxHeight)
            {
                // Calculate scale factor to fit within constraints while preserving aspect ratio
                float widthScale = maxWidth / currentWidth;
                float heightScale = maxHeight / currentHeight;
                
                // Use the smaller scale to ensure both dimensions fit
                float scale = Mathf.Min(widthScale, heightScale);
                
                // Apply the scaled size
                rt.sizeDelta = new Vector2(currentWidth * scale, currentHeight * scale);
            }
            npcImagePanel.SetActive(false);
        }
        else
        {
            npcImagePanel.SetActive(true);
            graphicsPanel.SetActive(false);
        }

        isTyping=true;
        dialogueText.SetText("");

        string fullText = dialogueData.dialogue[dialogueIndex];
        
        foreach(char letter in dialogueData.dialogue[dialogueIndex].ToCharArray())
        {
            
            dialogueText.text+=letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        // Show the enter key
        enterKey.gameObject.SetActive(true);
        
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
        }
        flashingCoroutine = StartCoroutine(FlashEnterKey());

        isTyping=false;

        if(dialogueData.autoProgressLines.Length>dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
       
    }
    public IEnumerator FlashEnterKey(float flashRate = 0.5f)
    {
        // Make sure the enter key starts visible
        if (enterKey != null && enterKey.gameObject != null)
        {
            enterKey.gameObject.SetActive(true);
            
            while (true) // Loop until stopped externally
            {
                yield return new WaitForSeconds(flashRate);
                enterKey.gameObject.SetActive(!enterKey.gameObject.activeSelf);
            }
        }
    }
    public void pauseDemonstration(){
        isPaused=true;
    }
    
    public void resumeDemonstration(){
        isPaused=false;
    }

    public void EndDialogue(){
        StopAllCoroutines();
        // Hide the enter key
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            enterKey.gameObject.SetActive(false);
        }
        isDialogueActive=false;
        dialogueText.SetText("");
        AudioController.Instance.PlayMenuOpen();
        AudioController.Instance.PlayBackgroundMusic();
        dialoguePanel.SetActive(false);

        if(dialogueIndex >=dialogueData.dialogue.Length){
             ObjectiveManager.Instance.MarkObjectiveCompleted(dialogueData.npcName);
        }
         // Remove button listeners
        if (closeDialogue != null)
            closeDialogue.onClick.RemoveListener(pauseDemonstration);
        
        if (continueDialogue != null)
            continueDialogue.onClick.RemoveListener(resumeDemonstration);
        
        if (exitDialogue != null)
            exitDialogue.onClick.RemoveListener(EndDialogue);

        isInstructing=false;
        NPCManager.Instance.isInstructing=false;
        UIManager.Instance.enablePlayerHUD();
        Player.Instance.resumePlayer();
    }


}
