using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    
    public bool isInstructing;
    public bool isEvaluation;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isOnPreRequisite = false;
    private Coroutine flashingCoroutine;
    private bool isPaused = false;

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
        
        if(EvaluationManager.Instance.isEvaluating || isOnPreRequisite || isPaused || RagChatManager.Instance.isUsingAssistant || NPCManager.Instance.isGuiding)
        {
            // Prevent interaction if already in a dialogue or if prerequisites are not met
            Debug.Log("Cannot interact with NPC at this time.");
            return;
        }
        
        if(dialogueData == null)
        {
            Debug.LogWarning("No dialogue data assigned to this NPC");
            return;
        }

        isInstructing = true;
        NPCManager.Instance.isInstructing = true;
        Player.Instance.pausePlayer();
        
        // Start the interaction coroutine
        StartCoroutine(InteractSequence());
    }

    private IEnumerator InteractSequence()
    {
        if(isEvaluation)
        {
            if(ObjectiveManager.Instance.AreAllObjectivesCompleted()){
                EvaluationManager.Instance.StartEvaluation();
                isInstructing = false;
                NPCManager.Instance.isInstructing = false;
                yield break;
            }
            else{
                yield return StartCoroutine(EvaluationManager.Instance.NotReady());
                Player.Instance.resumePlayer();
                isInstructing = false;
                NPCManager.Instance.isInstructing = false;
                yield break;
            }
        }
          
        if(!isDialogueActive){
            // Load the dialogue data
            dialogueData.LoadDialogue();
            
            // Wait for transition to complete
            yield return StartCoroutine(TransitionManager.Instance.contentTransition());
            
            // Set up dialogue button listeners through UIManager
            UIManager.Instance.SetupDialogueButtons(pauseDemonstration, resumeDemonstration, EndDialogue);
            AudioController.Instance.PlayMenuOpen();
            DatabaseManager.Instance.UpdateMetric("npc_revisit", dialogueData.npcName);
        }

        // Check for prerequisites
        for(int i = 0; i < dialogueData.requiredPreviousDialogues.Length; i++)
        {
            if(!ObjectiveManager.Instance.IsObjectiveCompleted(dialogueData.requiredPreviousDialogues[i]))
            {
                isOnPreRequisite = true;
                DisplayPrerequisiteMessage("Hello there! You are not quite ready for this lesson yet. You need to " + 
                                          dialogueData.requiredPreviousDialogues[i] + " first.", 
                                          dialogueData.requiredPreviousDialogues[i]);
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
        // Disable player controls and HUD
        UIManager.Instance.disablePlayerHUD();

        // Play the demonstration music
        AudioController.Instance.PlayDemonstrationMusic();

        // Set the dialogue state
        isDialogueActive = true;
        dialogueIndex = 0;
        
        // Show dialogue panel through UIManager
        UIManager.Instance.ShowDialoguePanel(dialogueData, dialogueIndex);
        
        // Start typing the dialogue
        StartCoroutine(TypeDialogue());
    }

    void NextLine(){
        AudioController.Instance.PlayButtonClick();
        
        if(isTyping)
        {
            StopAllCoroutines();
            
            // Set text directly through UIManager
            UIManager.Instance.SetDialogueText(dialogueData.dialogue[dialogueIndex]);
            
            isTyping = false;
            
            // Stop any existing flashing coroutine
            if (flashingCoroutine != null)
            {
                UIManager.Instance.StopFlashingEnterKey(flashingCoroutine);
            }
            
            // Start new flashing coroutine
            flashingCoroutine = UIManager.Instance.StartFlashingEnterKey();
        }
        else if(++dialogueIndex < dialogueData.dialogue.Length)
        {
            // Show the next dialogue if there is more dialogue available
            StartCoroutine(TypeDialogue());
        }
        else
        {
            // End the dialogue if no more lines are available
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
        AudioController.Instance.PlayMenuOpen();
        
        // Show dialogue panel with NPC
        UIManager.Instance.ShowDialoguePanel(dialogueData, 0);
        UIManager.Instance.ShowNPCImageOnly(dialogueData.npcSprite);
        
        // Start typing the prerequisite message
        StartCoroutine(TypePrerequisiteMessage(message, requiredNPC));
    }

    // Similar to TypeDialogue but for prerequisite messages
    private IEnumerator TypePrerequisiteMessage(string message, string requiredNPC)
    {
        // Start typing through UIManager
        isTyping = true;
        yield return UIManager.Instance.TypeDialogueText(message, dialogueData.typingSpeed);
        isTyping = false;
        
        // Auto-close the message after a delay
        yield return new WaitForSeconds(3f);

        // Close dialogue and resume player controls
        isDialogueActive = false;

        // Update the UI
        UIManager.Instance.SetDialogueText("");
        AudioController.Instance.PlayMenuOpen();
        UIManager.Instance.HideDialoguePanel();
        isInstructing = false;
        
        // Show the NPC indicator
        yield return StartCoroutine(NPCManager.Instance.showNpcIndicator(requiredNPC));
        
        // Resume player controls
        NPCManager.Instance.isInstructing = false;
        Player.Instance.resumePlayer();
        Player.Instance.resumeInteraction();
        isOnPreRequisite = false;
        AudioController.Instance.PlayBackgroundMusic();
        UIManager.Instance.enablePlayerHUD();
        Debug.Log(Player.Instance.stop_interaction);
        Debug.Log(Player.Instance.isPaused);
    }

    IEnumerator TypeDialogue(){
        // Stop any flashing enter key effect
        if (flashingCoroutine != null)
        {
            UIManager.Instance.StopFlashingEnterKey(flashingCoroutine);
        }
        
        // Show either dialogue image or NPC image
        if(dialogueData.dialogueImages != null && dialogueData.dialogueImages.Length > dialogueIndex && 
           dialogueData.dialogueImages[dialogueIndex] != null)
        {
            UIManager.Instance.ShowDialogueImage(dialogueData.npcSprite, dialogueData.dialogueImages[dialogueIndex]);
        }
        else
        {
            UIManager.Instance.ShowNPCImageOnly(dialogueData.npcSprite);
        }

        isTyping = true;
        
        // Use UIManager to handle text typing
        yield return UIManager.Instance.TypeDialogueText(dialogueData.dialogue[dialogueIndex], dialogueData.typingSpeed);
        
        // Start flashing enter key
        flashingCoroutine = UIManager.Instance.StartFlashingEnterKey();
        
        isTyping = false;

        // Handle auto-progress if configured
        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void pauseDemonstration(){
        isPaused = true;
    }
    
    public void resumeDemonstration(){
        isPaused = false;
    }

    public void EndDialogue(){
        StopAllCoroutines();
        NPCManager.Instance.isGuiding=true;
        // Stop flashing enter key
        if (flashingCoroutine != null)
        {
            UIManager.Instance.StopFlashingEnterKey(flashingCoroutine);
        }
        
        isDialogueActive = false;
        UIManager.Instance.SetDialogueText("");
        AudioController.Instance.PlayMenuOpen();
        AudioController.Instance.PlayBackgroundMusic();
        UIManager.Instance.HideDialoguePanel();

        if(dialogueIndex >= dialogueData.dialogue.Length){
             ObjectiveManager.Instance.MarkObjectiveCompleted(dialogueData.npcName);
        }
        
        // Remove button listeners
        UIManager.Instance.ClearDialogueButtonListeners();
        
        isInstructing = false;
        NPCManager.Instance.isInstructing = false;
        UIManager.Instance.enablePlayerHUD();
        Player.Instance.resumePlayer();
    }
}