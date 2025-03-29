using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text timestampText;
    [SerializeField] private Button deleteButton; // Add this field
    
    private ChatMessage chatMessage;
    private Action<string> onDeleteRequested; // Callback for delete requests
    
    public void SetMessage(ChatMessage message, Action<string> deleteCallback = null)
    {
        chatMessage = message;
        
        // Set username if the field exists
        if (usernameText != null)
            usernameText.text = message.username;
            
        // Set message text
        if (messageText != null)
            messageText.text = message.message;
        
        // Format timestamp
        if (timestampText != null)
        {
            DateTime messageTime = message.timestamp;
            
            if (messageTime.Date == DateTime.Now.Date)
            {
                // Today, just show time
                timestampText.text = messageTime.ToString("HH:mm");
            }
            else
            {
                // Not today, show date and time
                timestampText.text = messageTime.ToString("MMM d, HH:mm");
            }
        }
        
        // Setup delete button if it exists and a callback was provided
        if (deleteButton != null && deleteCallback != null)
        {
            onDeleteRequested = deleteCallback;
            
            // Remove any existing listeners
            deleteButton.onClick.RemoveAllListeners();
            
            // Add new listener
            deleteButton.onClick.AddListener(OnDeleteClicked);
            
            // Only show the delete button in sent messages (user's own messages)
            deleteButton.gameObject.SetActive(true);
        }
        else if (deleteButton != null)
        {
            // Hide delete button for received messages
            deleteButton.gameObject.SetActive(false);
        }
    }
    
    private void OnDeleteClicked()
    {
        // Call the delete callback with this message's ID
        onDeleteRequested?.Invoke(chatMessage.id);
    }
    
    public ChatMessage GetMessage()
    {
        return chatMessage;
    }
    
    public DateTime GetMessageTimestamp()
    {
        return chatMessage.timestamp;
    }
    
    public string GetMessageId()
    {
        return chatMessage.id;
    }
}