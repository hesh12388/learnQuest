using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; } // Singleton instance

    private string serverUrl = "http://localhost:5000"; // Change to your deployed server URL

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 📌 Register a new user (returns success/failure via callback)
    public void Register(string email, string username, string password, Action<bool> callback)
    {
        StartCoroutine(RegisterRequest(email, username, password, callback));
    }

    private IEnumerator RegisterRequest(string email, string username, string password, Action<bool> callback)
    {
        string json = "{\"email\":\"" + email + "\", \"username\":\"" + username + "\", \"password\":\"" + password + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/register", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("User registered successfully: " + request.downloadHandler.text);
                callback?.Invoke(true); // Success
            }
            else
            {
                Debug.LogError("Registration failed: " + request.error);
                callback?.Invoke(false); // Failure
            }
        }
    }

    // 📌 Login a user (returns success/failure via callback)
    public void Login(string email, string password, Action<bool> callback)
    {
        StartCoroutine(LoginRequest(email, password, callback));
    }

    private IEnumerator LoginRequest(string email, string password, Action<bool> callback)
    {
        string json = "{\"email\":\"" + email + "\", \"password\":\"" + password + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login successful: " + request.downloadHandler.text);
                callback?.Invoke(true); // Success
            }
            else
            {
                Debug.LogError("Login failed: " + request.error);
                callback?.Invoke(false); // Failure
            }
        }
    }
}
