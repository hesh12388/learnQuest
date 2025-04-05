using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RagChatUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform chatMessagesContainer;
    [SerializeField] private GameObject userMessagePrefab;     // Prefab for messages sent by the user
    [SerializeField] private GameObject assistantMessagePrefab; // Prefab for messages from the AI
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject loadingIndicator; 
    [Header("Settings")]
    [SerializeField] private int maxMessages = 100;
    
    private Dictionary<string, ChatMessageUI> messageUIComponents = new Dictionary<string, ChatMessageUI>();
    private bool isWaitingForResponse = false;
    
    private void OnEnable()
    {
        connectingPanel.SetActive(true);
        chatPanel.SetActive(false);
        
        loadingIndicator.SetActive(false);
        
        // Subscribe to RAG chat manager events
        if (RagChatManager.Instance != null)
        {
            RagChatManager.Instance.OnResponseReceived += HandleResponseReceived;
            RagChatManager.Instance.OnHistoryReceived += HandleHistoryReceived;
            RagChatManager.Instance.OnError += HandleError;
            RagChatManager.Instance.OnConnected += HandleConnected;
            RagChatManager.Instance.OnDisconnected += HandleDisconnected;
            
            // Setup UI events
            sendButton.onClick.AddListener(SendMessage);
            messageInputField.onSubmit.AddListener(_ => SendMessage());

            if(!RagChatManager.Instance.IsConnected())
            {
                // Attempt to connect if not already connected
                StartCoroutine(ConnectAsync());
            }
            else
            {
                // Already connected, show chat panel
                HandleConnected();
            }
            
            // Load chat history
            StartCoroutine(RagChatManager.Instance.LoadRagChatHistory());
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from RAG chat manager events
        if (RagChatManager.Instance != null)
        {
            RagChatManager.Instance.OnResponseReceived -= HandleResponseReceived;
            RagChatManager.Instance.OnHistoryReceived -= HandleHistoryReceived;
            RagChatManager.Instance.OnError -= HandleError;
            RagChatManager.Instance.OnConnected -= HandleConnected;
            RagChatManager.Instance.OnDisconnected -= HandleDisconnected;
        }
        
        // Clean up UI events
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendMessage);
            
        if (messageInputField != null)
            messageInputField.onSubmit.RemoveAllListeners();
    }

    private IEnumerator ConnectAsync()
    {
        var task = RagChatManager.Instance.Connect();
        while (!task.IsCompleted)
            yield return null;
        
        if (task.Exception != null)
            Debug.LogError($"Error connecting: {task.Exception.Message}");
    }
    
    private void HandleConnected()
    {
        // Update UI
        connectingPanel.SetActive(false);
        chatPanel.SetActive(true);
        messageInputField.interactable = true;
        sendButton.interactable = true;
    }
    
    private void HandleDisconnected()
    {
        // Update UI
        connectingPanel.SetActive(true);
        chatPanel.SetActive(false);
        messageInputField.interactable = false;
        sendButton.interactable = false;
    }
    
    private void HandleResponseReceived(RagChatMessage message)
    {
        // If this is a user message, show it and show loading indicator
        if (message.isUserMessage)
        {
            AddUserMessageToUI(message);
            loadingIndicator.SetActive(true);
            chatPanel.SetActive(false);
            isWaitingForResponse = true;
            // Disable input until response arrives
            messageInputField.interactable = false;
            sendButton.interactable = false;
        }
        // If this is an AI response, show it and hide loading indicator
        else if (!string.IsNullOrEmpty(message.assistantResponse))
        {
            AddAssistantMessageToUI(message);
            loadingIndicator.SetActive(false);
            chatPanel.SetActive(true);
            isWaitingForResponse = false;
            // Re-enable input
            messageInputField.interactable = true;
            sendButton.interactable = true;
        }
        
        // Auto-scroll to bottom
        StartCoroutine(ScrollToBottom());
    }
    
    private void HandleHistoryReceived(List<RagChatMessage> history)
    {
        // Clear existing messages
        ClearMessages();
        
        // Debug log to help diagnose
        Debug.Log($"Received {history.Count} history messages");
        
        // Add all messages from history
        foreach (var message in history)
        {
            Debug.Log($"Processing history message: ID={message.id}, IsUserMsg={message.isUserMessage}, " +
                    $"UserMsg={message.userMessage?.Substring(0, Math.Min(20, message.userMessage?.Length ?? 0))}, " +
                    $"AssistantMsg={message.assistantResponse?.Substring(0, Math.Min(20, message.assistantResponse?.Length ?? 0))}");
            
            if (message.isUserMessage && !string.IsNullOrEmpty(message.userMessage))
            {
                // This is a user message
                AddUserMessageToUI(message);
            }
            else if (!message.isUserMessage && !string.IsNullOrEmpty(message.assistantResponse))
            {
                // This is an assistant response
                AddAssistantMessageToUI(message);
            }
            else
            {
                Debug.LogWarning($"Skipping invalid history message: ID={message.id}, IsUserMsg={message.isUserMessage}");
            }
        }
        
        // Scroll to bottom
        StartCoroutine(ScrollToBottom());
        connectingPanel.SetActive(false);
    }
    
    private void HandleError(string errorMessage)
    {
        Debug.LogError($"RAG Chat error: {errorMessage}");
        
        // Show error in chat
        RagChatMessage errorMsg = new RagChatMessage
        {
            id = "error_" + System.Guid.NewGuid().ToString(),
            assistantResponse = "Error: " + errorMessage,
            timestamp = System.DateTime.Now,
            isUserMessage = false
        };
        
        AddAssistantMessageToUI(errorMsg);
        
        // Hide loading indicator and re-enable input
        loadingIndicator.SetActive(false);
        isWaitingForResponse = false;
        messageInputField.interactable = true;
        sendButton.interactable = true;
    }
    
    private void SendMessage()
    {
        // Don't send if waiting for response
        if (isWaitingForResponse)
            return;
            
        string messageText = messageInputField.text.Trim();
        
        if (string.IsNullOrEmpty(messageText))
            return;
        
        // Clear input field
        messageInputField.text = string.Empty;
        messageInputField.ActivateInputField();
        
        // Send message through RAG chat manager
        StartCoroutine(SendMessageAsync(messageText));
    }
    
    private IEnumerator SendMessageAsync(string messageText)
    {
        var task = RagChatManager.Instance.SendRagMessage(messageText);
        while (!task.IsCompleted)
            yield return null;
        
        if (task.Exception != null)
            Debug.LogError($"Error sending RAG message: {task.Exception.Message}");
    }
    
    private void AddUserMessageToUI(RagChatMessage message)
    {
        // Check if we already have this message (by ID)
        if (messageUIComponents.ContainsKey(message.id))
            return;
            
        // Instantiate message prefab
        GameObject messageGO = Instantiate(userMessagePrefab, chatMessagesContainer);
        ChatMessageUI messageUI = messageGO.GetComponent<ChatMessageUI>();
        
        if (messageUI == null)
        {
            Debug.LogError("ChatMessageUI component not found on chat message prefab");
            return;
        }
        
        // Create a chat message compatible with existing UI component
        ChatMessage uiMessage = new ChatMessage
        {
            id = message.id,
            username = "You",
            message = message.userMessage,
            timestamp = message.timestamp
        };
        
        // Set message content
        messageUI.SetMessage(uiMessage);
        
        // Store reference to the UI component
        messageUIComponents[message.id] = messageUI;
        
        // Enforce message limit
        EnforceMessageLimit();
    }
    
    private void AddAssistantMessageToUI(RagChatMessage message)
    {
        // Check if we already have this message (by ID)
        if (messageUIComponents.ContainsKey(message.id))
            return;
            
        // Instantiate message prefab
        GameObject messageGO = Instantiate(assistantMessagePrefab, chatMessagesContainer);
        ChatMessageUI messageUI = messageGO.GetComponent<ChatMessageUI>();
        
        if (messageUI == null)
        {
            Debug.LogError("ChatMessageUI component not found on chat message prefab");
            return;
        }
        
        // Create a chat message compatible with existing UI component
        ChatMessage uiMessage = new ChatMessage
        {
            id = message.id,
            username = "Assistant",
            message = message.assistantResponse,
            timestamp = message.timestamp
        };
        
        // Set message content
        messageUI.SetMessage(uiMessage);
        
        // Store reference to the UI component
        messageUIComponents[message.id] = messageUI;
        
        // Enforce message limit
        EnforceMessageLimit();
    }
    
    private void EnforceMessageLimit()
    {
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