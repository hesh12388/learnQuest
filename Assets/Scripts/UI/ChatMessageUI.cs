using System;
using UnityEngine;
using TMPro;

public class ChatMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text timestampText;
    
    private ChatMessage chatMessage;
    
    public void SetMessage(ChatMessage message)
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
    }
    
    public ChatMessage GetMessage()
    {
        return chatMessage;
    }
    
    public DateTime GetMessageTimestamp()
    {
        return chatMessage.timestamp;
    }
}