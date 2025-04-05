using System;
using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json.Linq;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance { get; private set; }

    // WebSocket instance
    private WebSocket webSocket;
    private string serverUrl ="ws://localhost:8080";
    private bool isConnecting = false;
    private bool isConnected = false;
    
    // Events that both chat systems can subscribe to
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;
    public event Action<string> OnMessage;
    
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    
    }
    
    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
        #endif
    }
    
    public WebSocket GetWebSocket()
    {
        return webSocket;
    }
    
    public bool IsConnected()
    {
        return isConnected && webSocket != null && webSocket.State == WebSocketState.Open;
    }
    
    public async Task<bool> ConnectWebSocket()
    {
        if (isConnecting || IsConnected())
            return true;
    
        string wsUrl = serverUrl;
        Debug.Log($"Connecting WebSocket to: {wsUrl}");
        
        try
        {
            isConnecting = true;
            
            // Close any existing connection
            await DisconnectWebSocket();
            
            // Create new WebSocket
            webSocket = new WebSocket(wsUrl);
            
            // Set up event handlers
            webSocket.OnOpen += HandleWebSocketOpen;
            webSocket.OnClose += HandleWebSocketClose;
            webSocket.OnError += HandleWebSocketError;
            webSocket.OnMessage += HandleWebSocketMessage;
            
            // Connect
            await webSocket.Connect();
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket connection error: {e.Message}");
            OnError?.Invoke($"Connection error: {e.Message}");
            isConnecting = false;
            isConnected = false;
            return false;
        }
    }
    
    public async Task DisconnectWebSocket()
    {
        if (webSocket != null)
        {
            // Remove event handlers
            webSocket.OnOpen -= HandleWebSocketOpen;
            webSocket.OnClose -= HandleWebSocketClose;
            webSocket.OnError -= HandleWebSocketError;
            webSocket.OnMessage -= HandleWebSocketMessage;
            
            // Close connection
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.Close();
            }
            
            // Clean up
            webSocket = null;
            isConnected = false;
        }
    }
    
    private void HandleWebSocketOpen()
    {
        Debug.Log("WebSocket connection opened");
        isConnected = true;
        isConnecting = false;
        OnConnected?.Invoke();
    }
    
    private void HandleWebSocketClose(WebSocketCloseCode closeCode)
    {
        Debug.Log($"WebSocket connection closed: {closeCode}");
        isConnected = false;
        isConnecting = false;
        OnDisconnected?.Invoke();
    }
    
    private void HandleWebSocketError(string errorMsg)
    {
        Debug.LogError($"WebSocket error: {errorMsg}");
        OnError?.Invoke(errorMsg);
    }
    
    private void HandleWebSocketMessage(byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        //Debug.Log($"WebSocket message received: {json}");
        OnMessage?.Invoke(json);
    }
    
    private void OnApplicationQuit()
    {
        DisconnectWebSocket().ContinueWith(task => {
            if (task.Exception != null)
            {
                Debug.LogError($"Error disconnecting WebSocket: {task.Exception.Message}");
            }
        });
    }
    
    // Helper method to check if a message is of a specific type
    public static bool IsMessageOfType(string json, string type)
    {
        try
        {
            JObject messageObj = JObject.Parse(json);
            return messageObj["type"]?.ToString() == type;
        }
        catch
        {
            return false;
        }
    }
}