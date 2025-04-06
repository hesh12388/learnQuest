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

    public bool isInstructing;
    public bool isEvaluation;
    private int dialogueIndex ;
    private bool isTyping, isDialogueActive;

    private bool isOnPreRequisite = false;

    private string currentMenu;
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
    
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {

        if( EvaluationManager.Instance.isEvaluating || isOnPreRequisite || isPaused)
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
            if(NPCManager.Instance.AreAllNPCsCompleted()){
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
            if(!NPCManager.Instance.HasCompletedNPC(dialogueData.requiredPreviousDialogues[i]))
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

        
        if(dialogueData.dialogueImages!=null && dialogueData.dialogueImages.Length>dialogueIndex && dialogueData.dialogueImages[dialogueIndex]!=null)
        {
            graphicsInstructorImage.sprite=dialogueData.npcSprite;
            graphicsPanel.SetActive(true);
            graphicsImagePanel.SetActive(true);
            graphicsImage.sprite=dialogueData.dialogueImages[dialogueIndex];
            graphicsImage.SetNativeSize();
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

        isTyping=false;

        if(dialogueData.autoProgressLines.Length>dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
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
        isDialogueActive=false;
        dialogueText.SetText("");
        AudioController.Instance.PlayMenuOpen();
        AudioController.Instance.PlayBackgroundMusic();
        dialoguePanel.SetActive(false);

        if(dialogueIndex >=dialogueData.dialogue.Length){
             NPCManager.Instance.MarkNPCCompleted(dialogueData.npcName);
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
