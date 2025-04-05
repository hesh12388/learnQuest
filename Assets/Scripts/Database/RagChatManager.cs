using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
public class RagChatManager : MonoBehaviour
{
    public static RagChatManager Instance { get; private set; }

    // Use WebSocketManager directly
    private WebSocket websocket => WebSocketManager.Instance.GetWebSocket();
    private bool isConnected => WebSocketManager.Instance != null && WebSocketManager.Instance.IsConnected();
    
    
    // Chat history
    private List<RagChatMessage> chatHistory = new List<RagChatMessage>();
    
    // Events
    public event Action<RagChatMessage> OnResponseReceived;
    public event Action<List<RagChatMessage>> OnHistoryReceived;
    public event Action<string> OnError;
    public event Action OnConnected;
    public event Action OnDisconnected;

    [Serializable]
    public class RAGMessage
    {
        public string type = "rag-chat";
        public string message;
        public string username;
    }
    
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
        // Subscribe to WebSocketManager events directly
        if (WebSocketManager.Instance != null)
        {
            Debug.Log("RagChatManager: Subscribing to WebSocketManager events");
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


    /// <summary>
    /// Ensure the WebSocket is connected before using it
    /// </summary>
    public async Task<bool> Connect()
    {
        if (isConnected)
            return true;
            
        Debug.Log("RagChatManager: Attempting to connect WebSocket...");
        
        if (WebSocketManager.Instance != null)
        {
            bool success = await WebSocketManager.Instance.ConnectWebSocket();
            
            if (success)
            {
                Debug.Log("RagChatManager: WebSocket connected successfully");
                return true;
            }
            else
            {
                Debug.LogError("RagChatManager: Failed to connect WebSocket");
                OnError?.Invoke("Failed to connect to chat server");
                return false;
            }
        }
        
        Debug.LogError("RagChatManager: WebSocketManager instance is null");
        OnError?.Invoke("WebSocket manager not available");
        return false;
    }

    private void HandleWebSocketConnected()
    {
        OnConnected?.Invoke();
    }

    private void HandleWebSocketDisconnected()
    {
        OnDisconnected?.Invoke();
    }

    private void HandleWebSocketError(string errorMsg)
    {
        OnError?.Invoke($"WebSocket error: {errorMsg}");
    }

    // Handle messages coming through the websocket
    private void HandleWebSocketMessage(string jsonMessage)
    {
        Debug.Log($"WebSocket message received in ragchat: {jsonMessage}");
        try
        {
            // Only process RAG response messages
            if (WebSocketManager.IsMessageOfType(jsonMessage, "rag-response"))
            {
                JObject messageObj = JObject.Parse(jsonMessage);
                string id = messageObj["id"].ToString();
                string responseText = messageObj["message"].ToString();
                string timestamp = messageObj["timestamp"].ToString();

                // Create a RAG chat message from the response
                RagChatMessage ragChatMessage = new RagChatMessage
                {
                    id = id,
                    assistantResponse = responseText,
                    timestamp = DateTime.Parse(timestamp),
                    isUserMessage = false
                };
                
                // Match this response with its question if possible
                foreach (var msg in chatHistory)
                {
                    if (msg.isUserMessage && msg.userMessageResponseId == null)
                    {
                        // This is an unanswered user message, link it with the response
                        msg.userMessageResponseId = id;
                        break;
                    }
                }
                
                // Add to history
                chatHistory.Add(ragChatMessage);
                
                // Notify listeners
                OnResponseReceived?.Invoke(ragChatMessage);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling WebSocket message: {ex.Message}");
        }
    }
  

    /// <summary>
    /// Send a RAG chat message
    /// </summary>
    public async System.Threading.Tasks.Task SendRagMessage(string message)
    {
       // Try to connect first if not already connected
        if (!isConnected)
        {
            Debug.Log("RagChatManager: Not connected, attempting to connect...");
            bool connected = await Connect();
            
            if (!connected)
            {
                Debug.LogWarning("RagChatManager: Connection attempt failed");
                // Error already reported by Connect()
                return;
            }
        }
    
        try
        {
            // Create RAG message request
        
            RAGMessage ragRequest= new RAGMessage{
                type = "rag-chat",
                username = DatabaseManager.Instance.loggedInUser.username,
                message = message,
            };
            
            string jsonMessage = JsonConvert.SerializeObject(ragRequest);
            
            // Send message through the websocket
            await websocket.SendText(jsonMessage);
            
            // Create and store user message
            RagChatMessage userMessage = new RagChatMessage
            {
                id = Guid.NewGuid().ToString(),
                userMessage = message,
                assistantResponse = null, // Will be filled when response is received
                timestamp = DateTime.Now,
                isUserMessage = true
            };
            
            chatHistory.Add(userMessage);
            
            // Notify listeners about the user message
            OnResponseReceived?.Invoke(userMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending RAG chat message: {e.Message}");
            OnError?.Invoke("Failed to send message: " + e.Message);
        }
    }

    /// <summary>
    /// Load RAG chat history for the current user
    /// </summary>
    public IEnumerator LoadRagChatHistory()
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.loggedInUser == null)
        {
            OnError?.Invoke("User not logged in");
            yield break;
        }
        
        string username = DatabaseManager.Instance.loggedInUser.username;
        string url = $"{DatabaseManager.Instance.GetServerUrl()}/rag-chat-history/{username}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JObject responseObj = JObject.Parse(jsonResponse);
                JArray messagesArray = (JArray)responseObj["messages"];
                
                List<RagChatMessage> history = new List<RagChatMessage>();
                
                foreach (JObject msgObj in messagesArray)
                {
                    // For each history item, create a pair of messages (user question + AI response)
                    
                    // User question message
                    RagChatMessage userMsg = new RagChatMessage
                    {
                        id = msgObj["id"].ToString() + "_user",
                        userMessage = msgObj["user_message"].ToString(),
                        userMessageResponseId = msgObj["id"].ToString(),
                        timestamp = DateTime.Parse(msgObj["timestamp"].ToString()),
                        isUserMessage = true
                    };
                    
                    // AI response message
                    RagChatMessage assistantMsg = new RagChatMessage
                    {
                        id = msgObj["id"].ToString(),
                        assistantResponse = msgObj["assistant_response"].ToString(),
                        timestamp = DateTime.Parse(msgObj["timestamp"].ToString()),
                        isUserMessage = false
                    };
                    
                    history.Add(userMsg);
                    history.Add(assistantMsg);
                }
                
                chatHistory = history;
                OnHistoryReceived?.Invoke(history);
            }
            else
            {
                Debug.LogError($"Error loading RAG chat history: {request.error}");
                OnError?.Invoke("Failed to load chat history: " + request.error);
            }
        }
    }

    /// <summary>
    /// Get all RAG chat messages
    /// </summary>
    public List<RagChatMessage> GetAllMessages()
    {
        return new List<RagChatMessage>(chatHistory);
    }
    
    /// <summary>
    /// Check if the websocket is connected
    /// </summary>
    public bool IsConnected()
    {
        return isConnected;
    }
}

// Message class definitions
[Serializable]
public class RagChatMessage
{
    public string id;
    public string userMessage;
    public string assistantResponse;
    public string userMessageResponseId; // To link user messages with AI responses
    public DateTime timestamp;
    public bool isUserMessage = false; // True if this is a message from the user (no response yet)
}