using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform chatMessagesContainer;
    [SerializeField] private GameObject chatMessageSentPrefab;     // Prefab for messages sent by the user
    [SerializeField] private GameObject chatMessageReceivedPrefab; // Prefab for messages received from others
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject chatPanel;
    
    [Header("Settings")]
    [SerializeField] private int maxMessages = 100;
    
    private string currentUsername;
    private Dictionary<string, ChatMessageUI> messageUIComponents = new Dictionary<string, ChatMessageUI>();
    
    private void OnEnable()
    {
        // Initialize UI
        connectingPanel.SetActive(true);
        chatPanel.SetActive(false);
        
        // Make sure we have the current username
        if (DatabaseManager.Instance != null && DatabaseManager.Instance.loggedInUser != null)
        {
            currentUsername = DatabaseManager.Instance.loggedInUser.username;
        }
        
        // Subscribe to chat manager events
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.OnConnected += HandleConnected;
            ChatManager.Instance.OnDisconnected += HandleDisconnected;
            ChatManager.Instance.OnConnectionError += HandleConnectionError;
            ChatManager.Instance.OnMessageReceived += HandleMessageReceived;
            ChatManager.Instance.OnHistoryReceived += HandleHistoryReceived;
            ChatManager.Instance.OnModerationResult += HandleModerationResult;
            
            // Setup UI events
            sendButton.onClick.AddListener(SendMessage);
            messageInputField.onSubmit.AddListener(_ => SendMessage());
            
            // Just connect - don't try to join course yet
            if (!ChatManager.Instance.IsConnected())
            {
                Debug.Log("Connecting to chat server...");
                StartCoroutine(ConnectAsync());
            }
            else
            {
                // Already connected, handle UI update and join course
                HandleConnected();
            }
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from chat manager events
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.OnConnected -= HandleConnected;
            ChatManager.Instance.OnDisconnected -= HandleDisconnected;
            ChatManager.Instance.OnConnectionError -= HandleConnectionError;
            ChatManager.Instance.OnMessageReceived -= HandleMessageReceived;
            ChatManager.Instance.OnHistoryReceived -= HandleHistoryReceived;
            ChatManager.Instance.OnModerationResult -= HandleModerationResult;
    
        }
        
        // Clean up UI events
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendMessage);
            
        if (messageInputField != null)
            messageInputField.onSubmit.RemoveAllListeners();
    }

    private IEnumerator ConnectAsync()
    {
        var task = ChatManager.Instance.Connect();
        while (!task.IsCompleted)
            yield return null;
        
        if (task.Exception != null)
            Debug.LogError($"Error connecting: {task.Exception.Message}");
    }

    private IEnumerator JoinCourseAsync(string courseId)
    {
        var task = ChatManager.Instance.JoinCourse(courseId);
        while (!task.IsCompleted)
            yield return null;
        
        if (task.Exception != null)
            Debug.LogError($"Error joining course: {task.Exception.Message}");
    }
    
    // Now update HandleConnected to handle course joining:
    private void HandleConnected()
    {
         // Check if this object is still alive
        if (this == null || !this.gameObject || !this.isActiveAndEnabled)
            return;
            
        Debug.Log("Chat connected callback received");
        
        // Update UI with null checks
        if (connectingPanel != null) connectingPanel.SetActive(false);
        if (chatPanel != null) chatPanel.SetActive(true);
        if (messageInputField != null) messageInputField.interactable = true;
        if (sendButton != null) sendButton.interactable = true;
        
        // Update UI
        connectingPanel.SetActive(false);
        chatPanel.SetActive(true);
        messageInputField.interactable = true;
        sendButton.interactable = true;
        
        // Check if we need to join a course
        if (DatabaseManager.Instance != null && 
            DatabaseManager.Instance.loggedInUser != null && 
            !string.IsNullOrEmpty(DatabaseManager.Instance.loggedInUser.selectecCourse))
        {
            string courseId = DatabaseManager.Instance.loggedInUser.selectecCourse;
            string currentCourse = ChatManager.Instance.GetCurrentCourse();
            
            // Only join if we're not already in this course
            if (currentCourse != courseId)
            {
                Debug.Log($"Now joining course: {courseId}");
                StartCoroutine(JoinCourseAsync(courseId));
            }
            else
            {
                Debug.Log($"Already in course: {courseId}");
            }
        }
        else
        {
            Debug.LogWarning("No course selected or user not logged in");
        }
    }
    
    private void HandleDisconnected()
    {
        // Check if this object is still alive
        if (this == null || !this.gameObject || !this.isActiveAndEnabled)
            return;
            
        if (connectingPanel != null) connectingPanel.SetActive(true);
        if (chatPanel != null) chatPanel.SetActive(false);
        if (messageInputField != null) messageInputField.interactable = false;
        if (sendButton != null) sendButton.interactable = false;
        connectingPanel.SetActive(true);
        chatPanel.SetActive(false);
        messageInputField.interactable = false;
        sendButton.interactable = false;
    }
    
    private void HandleConnectionError(string errorMessage)
    {
        // Check if this object is still alive before logging
        if (this == null || !this.gameObject || !this.isActiveAndEnabled)
            return;
            
        Debug.LogError($"Chat error: {errorMessage}");
        
        // Check if UI components exist before using them
        if (connectingPanel != null && chatPanel != null)
        {
            connectingPanel.SetActive(true);
            chatPanel.SetActive(false);
        }
        
        if (messageInputField != null && messageInputField.gameObject)
            messageInputField.interactable = false;
            
        if (sendButton != null && sendButton.gameObject)
            sendButton.interactable = false;
    }
    
    private void HandleMessageReceived(ChatMessage message)
    {
        if (this == null || !this.gameObject || !this.isActiveAndEnabled)
            return;
        Debug.Log($"UI handling message: {message.username}: {message.message}");
        AddMessageToUI(message);
        
        // Auto-scroll to bottom
        StartCoroutine(ScrollToBottom());
    }
    
    private void HandleHistoryReceived(List<ChatMessage> history)
    {
        // Clear existing messages
        ClearMessages();
        
        // Add all messages from history
        foreach (var message in history)
        {
            AddMessageToUI(message);
        }
        
        // Scroll to bottom
        StartCoroutine(ScrollToBottom());
    }
    
    // Update the HandleModerationResult method
    private void HandleModerationResult(string result)
    {
        // Log moderation result
        Debug.Log($"Chat moderation: {result}");
        
        // Check if this is a rejection result
        if (result.StartsWith("Rejected") || result.Contains("rejected"))
        {
            // Change the input field placeholder to indicate the message was rejected
            StartCoroutine(ShowModerationRejection());
        }
    }

    // Add this new coroutine to handle moderation rejection feedback
    private IEnumerator ShowModerationRejection()
    {
        // Save original placeholder and colors
        string originalPlaceholder = messageInputField.placeholder.GetComponent<TextMeshProUGUI>().text;
        Color originalColor = messageInputField.placeholder.GetComponent<TextMeshProUGUI>().color;
        
        // Update placeholder with rejection message
        TextMeshProUGUI placeholderText = messageInputField.placeholder.GetComponent<TextMeshProUGUI>();
        placeholderText.text = "Message rejected - not relevant to course";
        placeholderText.color = Color.red;
        
        // Keep the rejection message visible for a few seconds
        yield return new WaitForSeconds(3.5f);
        
        // Restore original placeholder and color
        placeholderText.text = originalPlaceholder;
        placeholderText.color = originalColor;
    }
    
    private void SendMessage()
    {
        string messageText = messageInputField.text.Trim();
        
        if (string.IsNullOrEmpty(messageText))
        {
            return;
        }
        
        // Clear input field
        messageInputField.text = string.Empty;
        messageInputField.ActivateInputField();
        
        // Send message through chat manager
        StartCoroutine(SendMessageAsync(messageText));
    }

    private IEnumerator SendMessageAsync(string messageText)
    {
        var task = ChatManager.Instance.SendMessage(messageText);
        while (!task.IsCompleted)
            yield return null;
        
        if (task.Exception != null)
            Debug.LogError($"Error sending message: {task.Exception.Message}");
    }
    
    private void AddMessageToUI(ChatMessage message)
    {
        // Check if we already have this message (by ID)
        if (messageUIComponents.ContainsKey(message.id))
        {
            return;
        }
        
        // Choose the appropriate prefab based on whether the message is from the current user
        GameObject prefabToUse = message.username == currentUsername 
            ? chatMessageSentPrefab 
            : chatMessageReceivedPrefab;
        
        // Instantiate message prefab
        GameObject messageGO = Instantiate(prefabToUse, chatMessagesContainer);
        ChatMessageUI messageUI = messageGO.GetComponent<ChatMessageUI>();
        
        if (messageUI == null)
        {
            Debug.LogError("ChatMessageUI component not found on chat message prefab");
            return;
        }
        
        // Set message content - no need to pass color since we're using different prefabs
        messageUI.SetMessage(message);
        
        // Store reference to the UI component
        messageUIComponents[message.id] = messageUI;
        
        // Limit number of messages
        if (messageUIComponents.Count > maxMessages)
        {
            // Find the oldest message
            DateTime oldestTime = DateTime.MaxValue;
            string oldestId = null;
            
            foreach (var pair in messageUIComponents)
            {
                if (pair.Value.GetMessageTimestamp() < oldestTime)
                {
                    oldestTime = pair.Value.GetMessageTimestamp();
                    oldestId = pair.Key;
                }
            }
            
            if (oldestId != null)
            {
                // Destroy the oldest message GameObject
                Destroy(messageUIComponents[oldestId].gameObject);
                messageUIComponents.Remove(oldestId);
            }
        }
    }
    
    private void ClearMessages()
    {
        // Destroy all message GameObjects
        foreach (var pair in messageUIComponents)
        {
            Destroy(pair.Value.gameObject);
        }
        
        messageUIComponents.Clear();
    }
    
    private IEnumerator ScrollToBottom()
    {
        // Wait for the end of the frame to ensure UI has updated
        yield return new WaitForEndOfFrame();
        
        // Scroll to bottom
        chatScrollRect.normalizedPosition = new Vector2(0, 0);
    }
}