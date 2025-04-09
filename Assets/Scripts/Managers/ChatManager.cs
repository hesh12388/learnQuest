using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    // Use websocket manager instance
    private WebSocket websocket => WebSocketManager.Instance.GetWebSocket();
    private bool isConnected => WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected();
    
    private bool isJoiningCourse = false;
    private string currentCourseId = null;
    
    // Chat history
    private List<ChatMessage> chatMessages = new List<ChatMessage>();
    
    [Serializable]
    public class DeleteMessageRequest
    {
        public string type = "delete";
        public string message_id;
    }

    // Events
    public event Action<string> OnMessageDeleted;
    public event Action<string> OnDeleteConfirmed;
    public event Action<ChatMessage> OnMessageReceived;
    public event Action<List<ChatMessage>> OnHistoryReceived;
    public event Action<string> OnConnectionError;
    public event Action<string> OnModerationResult;
    public event Action OnConnected;
    public event Action OnDisconnected;
        
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe to WebSocketManager events
        if (WebSocketManager.Instance != null)
        {
            Debug.Log("ChatManager: Subscribing to WebSocketManager events");
            WebSocketManager.Instance.OnMessage += HandleWebSocketMessage;
            WebSocketManager.Instance.OnConnected += HandleWebSocketConnected;
            WebSocketManager.Instance.OnDisconnected += HandleWebSocketDisconnected;
            WebSocketManager.Instance.OnError += HandleWebSocketError;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from WebSocketManager events
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnMessage -= HandleWebSocketMessage;
            WebSocketManager.Instance.OnConnected -= HandleWebSocketConnected;
            WebSocketManager.Instance.OnDisconnected -= HandleWebSocketDisconnected;
            WebSocketManager.Instance.OnError -= HandleWebSocketError;
        }
    }
    
    private void HandleWebSocketConnected()
    {
        Debug.Log("WebSocket connected, in chat manager");
        OnConnected?.Invoke();
    }
    
    private void HandleWebSocketDisconnected()
    {
        OnDisconnected?.Invoke();
    }
    
    private void HandleWebSocketError(string errorMsg)
    {
        OnConnectionError?.Invoke($"WebSocket error: {errorMsg}");
    }
    
    private void HandleWebSocketMessage(string jsonMessage)
    {
        ProcessIncomingMessage(jsonMessage);
    }
    
    /// <summary>
    /// Connect to the chat WebSocket server
    /// </summary>
    public async System.Threading.Tasks.Task<bool> Connect()
    {
        if (isConnected)
            return true;
            
        if (WebSocketManager.Instance == null)
        {
            Debug.LogError("WebSocketManager.Instance is null");
            OnConnectionError?.Invoke("WebSocket manager not available");
            return false;
        }
        
        try
        {
            bool success = await WebSocketManager.Instance.ConnectWebSocket();
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to WebSocket: {e.Message}");
            OnConnectionError?.Invoke(e.Message);
            return false;
        }
    }
    
    /// <summary>
    /// Disconnect from the chat WebSocket server
    /// </summary>
    public async System.Threading.Tasks.Task Disconnect()
    {
        if (!isConnected)
            return;
        
        if (currentCourseId != null)
        {
            await LeaveCourse();
        }
        
        // WebSocketManager will handle the actual disconnection
        await WebSocketManager.Instance.DisconnectWebSocket();
    }
    
    /// <summary>
    /// Join a course chat room
    /// </summary>
    public async System.Threading.Tasks.Task JoinCourse(string courseId)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot join course: WebSocket not connected");
            await Connect();
            if (!isConnected) return;
        }
        
        if (isJoiningCourse)
        {
            Debug.LogWarning("Already joining a course, please wait");
            return;
        }
        
        isJoiningCourse = true;
        
        try
        {
            // Create join message
            JoinMessage joinMsg = new JoinMessage
            {
                type = "join",
                username = DatabaseManager.Instance.loggedInUser.username,
                course_id = courseId
            };
            
            string jsonMessage = JsonConvert.SerializeObject(joinMsg);
            
            // Send join request
            await websocket.SendText(jsonMessage);
            
            // Current course will be set when we receive confirmation
        }
        catch (Exception e)
        {
            Debug.LogError($"Error joining course chat: {e.Message}");
            OnConnectionError?.Invoke("Failed to join course chat: " + e.Message);
        }
        finally
        {
            isJoiningCourse = false;
        }
    }
    
    /// <summary>
    /// Leave the current course chat room
    /// </summary>
    public async System.Threading.Tasks.Task LeaveCourse()
    {
        if (!isConnected || currentCourseId == null)
        {
            return;
        }
        
        try
        {
            // Create leave message
            LeaveMessage leaveMsg = new LeaveMessage
            {
                type = "leave"
            };
            
            string jsonMessage = JsonConvert.SerializeObject(leaveMsg);
            
            // Send leave request
            await websocket.SendText(jsonMessage);
            
            // Clear current course
            currentCourseId = null;
            chatMessages.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error leaving course chat: {e.Message}");
        }
    }
    
    /// <summary>
    /// Send a chat message to the current course
    /// </summary>
    public async System.Threading.Tasks.Task SendMessage(string messageText)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot send message: WebSocket not connected");
            bool connected = await Connect();
            if (!connected)
            {
                OnConnectionError?.Invoke("Not connected to chat server");
                return;
            }
        }
        
        if (string.IsNullOrEmpty(currentCourseId))
        {
            Debug.LogWarning("Cannot send message: Not joined to any course");
            OnConnectionError?.Invoke("Join a course first before sending messages");
            return;
        }
        
        
        try
        {
            // Create chat message
            ChatMessageRequest chatMsg = new ChatMessageRequest
            {
                type = "chat",
                message = messageText
            };
            
            string jsonMessage = JsonConvert.SerializeObject(chatMsg);
            
            // Send message
            await websocket.SendText(jsonMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending chat message: {e.Message}");
            OnConnectionError?.Invoke("Failed to send message: " + e.Message);
        }
    }

    // Add a new method to delete messages
    public async System.Threading.Tasks.Task DeleteMessage(string messageId)
    {
        if (!isConnected)
        {
            Debug.LogWarning("Cannot delete message: WebSocket not connected");
            OnConnectionError?.Invoke("Not connected to chat server");
            return;
        }
        
        if (string.IsNullOrEmpty(currentCourseId))
        {
            Debug.LogWarning("Cannot delete message: Not joined to any course");
            OnConnectionError?.Invoke("Join a course first before deleting messages");
            return;
        }
        
        try
        {
            // Create delete message request
            DeleteMessageRequest deleteMsg = new DeleteMessageRequest
            {
                type = "delete",
                message_id = messageId
            };
            
            string jsonMessage = JsonConvert.SerializeObject(deleteMsg);
            
            // Send delete request
            await websocket.SendText(jsonMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting chat message: {e.Message}");
            OnConnectionError?.Invoke("Failed to delete message: " + e.Message);
        }
    }

    /// <summary>
    /// Process incoming WebSocket messages
    /// </summary>
    private void ProcessIncomingMessage(string jsonMessage)
    {
        try
        {
            JObject messageObj = JObject.Parse(jsonMessage);
            string messageType = messageObj["type"].ToString();
            
            switch (messageType)
            {
                // Add these new cases
                case "delete-confirmed":
                    // Our message was deleted successfully
                    string confirmedId = messageObj["message_id"].ToString();
                    Debug.Log($"Delete confirmed for message: {confirmedId}");
                    OnDeleteConfirmed?.Invoke(confirmedId);
                    break;
                    
                case "message-deleted":
                    // Someone deleted a message
                    string deletedId = messageObj["message_id"].ToString();
                    Debug.Log($"Message deleted: {deletedId}");
                    
                    // Remove the message from our local collection
                    chatMessages.RemoveAll(m => m.id == deletedId);
                    
                    // Notify listeners
                    OnMessageDeleted?.Invoke(deletedId);
                    break;
                case "joined":
                    // Successfully joined a course
                    currentCourseId = messageObj["course_id"].ToString();
                    Debug.Log($"Joined course chat: {currentCourseId}");
                    break;
                    
                case "chat":
                    // Received a chat message
                    ChatMessage chatMessage = new ChatMessage
                    {
                        id = messageObj["id"].ToString(),
                        username = messageObj["username"].ToString(),
                        message = messageObj["message"].ToString(),
                        timestamp = DateTime.Parse(messageObj["timestamp"].ToString())
                    };
                    
                    chatMessages.Add(chatMessage);
                    OnMessageReceived?.Invoke(chatMessage);
                    break;
                    
                case "history":
                    // Received chat history
                    JArray messagesArray = (JArray)messageObj["messages"];
                    List<ChatMessage> history = new List<ChatMessage>();
                    
                    foreach (JObject msgObj in messagesArray)
                    {
                        ChatMessage historyMsg = new ChatMessage
                        {
                            id = msgObj["id"].ToString(),
                            username = msgObj["username"].ToString(),
                            message = msgObj["message"].ToString(),
                            timestamp = DateTime.Parse(msgObj["timestamp"].ToString())
                        };
                        
                        history.Add(historyMsg);
                    }
                    
                    chatMessages = history;
                    OnHistoryReceived?.Invoke(history);
                    break;
                    
                case "moderation-start":
                    // Message is being moderated
                    string moderationStartMsg = messageObj["message"].ToString();
                    OnModerationResult?.Invoke("Moderating: " + moderationStartMsg);
                    break;
                    
                case "moderation-approved":
                    // Message was approved
                    string approvedMsg = messageObj["message"].ToString();
                    OnModerationResult?.Invoke("Approved: " + approvedMsg);
                    break;
                    
                case "moderation-rejected":
                    // Message was rejected
                    string rejectedMsg = messageObj["message"].ToString();
                    OnModerationResult?.Invoke("Rejected: " + rejectedMsg);
                    break;
                    
                case "error":
                    // Error message
                    string errorMsg = messageObj["message"].ToString();
                    Debug.LogError($"WebSocket Error: {errorMsg}");
                    OnConnectionError?.Invoke(errorMsg);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown message type: {messageType}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing WebSocket message: {e.Message}\nMessage: {jsonMessage}");
        }
    }
    
    /// <summary>
    /// Get all chat messages
    /// </summary>
    public List<ChatMessage> GetAllMessages()
    {
        return new List<ChatMessage>(chatMessages);
    }
    
    /// <summary>
    /// Get the current connection status
    /// </summary>
    public bool IsConnected()
    {
        return isConnected;
    }
    
    /// <summary>
    /// Get the currently joined course ID
    /// </summary>
    public string GetCurrentCourse()
    {
        return currentCourseId;
    }
}

// Message class definitions
[Serializable]
public class ChatMessage
{
    public string id;
    public string username;
    public string message;
    public DateTime timestamp;
}

[Serializable]
public class JoinMessage
{
    public string type = "join";
    public string username;
    public string course_id;
}

[Serializable]
public class LeaveMessage
{
    public string type = "leave";
}

[Serializable]
public class ChatMessageRequest
{
    public string type = "chat";
    public string message;
}