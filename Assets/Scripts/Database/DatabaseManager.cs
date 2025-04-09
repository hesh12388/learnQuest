using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using Newtonsoft.Json.Linq; 

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; } // Singleton instance

    private string serverUrl = "https://learnquest-expressserver.onrender.com";

    public User loggedInUser; // Currently logged in user
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

    // Add this method to your DatabaseManager class
    public void UpdateUserScore()
    {
        if (loggedInUser == null)
        {
            return;
        }

        StartCoroutine(UpdateUserScoreRequest());
    }

    public string GetServerUrl()
    {
        return serverUrl;
    }

    // The coroutine to handle the API request
    private IEnumerator UpdateUserScoreRequest()
    {
        string json = "{\"username\":\"" + loggedInUser.username + "\", \"score\":" + loggedInUser.score + "}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/update-score", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Score updated successfully: " + responseText);
            }
            else
            {
                Debug.LogError("Failed to update score: " + request.error);
            }
        }
    }

    // 📌 Register a new user (returns success/failure via callback)
    public void Register(string email, string username, string password, Action<bool> callback)
    {
        StartCoroutine(RegisterRequest(email, username, password, callback));
    }

    public void setUserCourse(string coursename){
        loggedInUser.selectecCourse = coursename;
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

    // 📌 Start a specific level for the user
    public void StartLevel(Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot start level: No user is logged in");
            return;
        }

        StartCoroutine(StartLevelRequest(callback));
    }

    // 📌 Get Objectives for the current level
    public void GetObjectives(Action<List<Objective>> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot get objectives: No user is logged in or no current level set");
            callback?.Invoke(null);
            return;
        }

        StartCoroutine(GetObjectivesRequest(callback));
    }

    private IEnumerator GetObjectivesRequest(Action<List<Objective>> callback)
    {
        string username = loggedInUser.username;
        Debug.Log(loggedInUser.currentChapter);
        Debug.Log(loggedInUser.currentLevel);
        string level_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1].level_name;
        string url = $"{serverUrl}/get-objectives/{username}/{level_name}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Objectives retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON using JObject
                    JObject responseObj = JObject.Parse(jsonResponse);
                    Debug.Log(responseObj);
                    JArray objectivesArray = (JArray)responseObj["objectives"];
                    
                    List<Objective> objectives = new List<Objective>();
                    
                    foreach (JObject objectiveObj in objectivesArray)
                    {
                        string objectiveName = objectiveObj["objective_name"].ToString();
                        string status = objectiveObj["status"].ToString();
                        string description = objectiveObj["description"].ToString();
                        int difficulty = objectiveObj["difficulty"].ToObject<int>();
                        int points = objectiveObj["points"].ToObject<int>();
                        objectives.Add(new Objective(objectiveName, status, description, difficulty, points));
                    }
                    Debug.Log(objectives);
                    callback?.Invoke(objectives);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing objectives JSON: " + ex.Message);
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve objectives: {request.error}");
                callback?.Invoke(null);
            }
        }
    }

    private IEnumerator StartLevelRequest(Action<bool> callback)
    {
        string level_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1].level_name;
        string json = "{\"username\":\"" + loggedInUser.username + "\", \"level_name\":\"" + level_name + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/start-level", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Level started successfully: " + responseText);
                callback?.Invoke(true); // Success
            }
            else
            {
                Debug.LogError("Failed to start level: " + request.error);
                callback?.Invoke(false); // Failure
            }
        }
    }

    // Public method to remove an item from a user's inventory
    public void RemoveUserItem(string itemName, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot remove item: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(RemoveUserItemRequest(itemName, callback));
    }

    private IEnumerator RemoveUserItemRequest(string itemName, Action<bool> callback)
    {
        string username = loggedInUser.username;
        string url = $"{serverUrl}/user-items/{username}/{itemName}";
        
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer(); // Required to read response

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"Item '{itemName}' removed successfully: {responseText}");
                
                // Also remove the item from the local purchasedItems list if it exists
                if (loggedInUser.purchasedItems != null)
                {
                    loggedInUser.purchasedItems.RemoveAll(item => item.item_name == itemName);
                }
                
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"Failed to remove item '{itemName}': {request.error}");
                callback?.Invoke(false);
            }
        }
    }

    // 📌 Get all available shop items
    public void GetShopItems(Action<ShopItemsResponse> callback)
    {
        StartCoroutine(GetShopItemsRequest(callback));
    }

    private IEnumerator GetShopItemsRequest(Action<ShopItemsResponse> callback)
    {
        string url = $"{serverUrl}/items";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Shop items retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON using JObject
                    JObject responseObj = JObject.Parse(jsonResponse);
                    JObject itemsObj = (JObject)responseObj["items"];
                    
                    ShopItemsResponse shopItems = new ShopItemsResponse();
                    
                    // Loop through each category (weapon, potion, armor)
                    foreach (var category in itemsObj.Properties())
                    {
                        string categoryName = category.Name; // "weapon", "potion", "armor"
                        Debug.Log("Parsing category: " + categoryName);
                        JArray itemsArray = (JArray)category.Value;
                        
                        List<ShopItem> categoryItems = new List<ShopItem>();
                        
                        // Process each item in this category
                        foreach (JObject itemObj in itemsArray)
                        {
                            string itemName = itemObj["item_name"].ToString();
                            int cost = itemObj["cost"].ToObject<int>();
                            Debug.Log("Parsing item: " + itemName + " for " + categoryName);
                            categoryItems.Add(new ShopItem(itemName, cost));
                        }
                        
                        // Add this category to our response
                        shopItems.categories.Add(categoryName, categoryItems);
                    }
                    
                    Debug.Log(shopItems);
                    callback?.Invoke(shopItems);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing shop items JSON: " + ex.Message);
                    callback?.Invoke(new ShopItemsResponse());
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve shop items: {request.error}");
                callback?.Invoke(new ShopItemsResponse());
            }
        }
    }

    // 📌 Get user achievements
    public void GetUserAchievements(Action<List<Achievement>> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot get achievements: No user is logged in");
            callback?.Invoke(null);
            return;
        }

        StartCoroutine(GetUserAchievementsRequest(callback));
    }

    private IEnumerator GetUserAchievementsRequest(Action<List<Achievement>> callback)
    {
        string url = $"{serverUrl}/get-achievements/{loggedInUser.username}/{loggedInUser.selectecCourse}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("User achievements retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON using JObject
                    JObject responseObj = JObject.Parse(jsonResponse);
                    JArray achievementsArray = (JArray)responseObj["achievements"];
                    
                    List<Achievement> achievements = new List<Achievement>();
                    
                    foreach (JObject achievementObj in achievementsArray)
                    {
                        string name = achievementObj["achievement_name"].ToString();
                        string description = achievementObj["description"].ToString();
                        int gems = achievementObj["gems"].ToObject<int>();
                        string status = achievementObj["status"].ToString();
                        
                        achievements.Add(new Achievement(name, description, gems, status));
                    }
                    
                    callback?.Invoke(achievements);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing achievements JSON: " + ex.Message);
                    callback?.Invoke(new List<Achievement>());
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve achievements: {request.error}");
                callback?.Invoke(new List<Achievement>());
            }
        }
    }
    
    // 📌 Get Leaderboard Rankings
    public void GetLeaderboard(Action<List<LeaderboardEntry>> callback)
    {
        StartCoroutine(GetLeaderboardRequest(callback));
    }

    private IEnumerator GetLeaderboardRequest(Action<List<LeaderboardEntry>> callback)
    {
        string url = $"{serverUrl}/leaderboard";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Leaderboard retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON array
                    JArray leaderboardArray = JArray.Parse(jsonResponse);
                    List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
                    
                    foreach (JObject entryObj in leaderboardArray)
                    {
                        string username = entryObj["username"].ToString();
                        int score = entryObj["score"].ToObject<int>();
                        int numGems = entryObj["numGems"].ToObject<int>();
                        int numAchievements = entryObj["numAchievements"].ToObject<int>();
                        
                        leaderboardEntries.Add(new LeaderboardEntry(username, score, numAchievements, numGems));
                    }
                    
                    callback?.Invoke(leaderboardEntries);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing leaderboard JSON: " + ex.Message);
                    callback?.Invoke(new List<LeaderboardEntry>());
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve leaderboard: {request.error}");
                callback?.Invoke(new List<LeaderboardEntry>());
            }
        }
    }

    public void deleteSavedGame(string courseName, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot restart course: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(deleteSavedGameRequest(courseName, callback));
    }

    private IEnumerator deleteSavedGameRequest(string courseName, Action<bool> callback)
    {
        string json = "{\"course_name\":\"" + courseName + "\", \"username\":\"" + loggedInUser.username + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/delete-course", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Course restarted successfully: " + responseText);
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError("Failed to restart course: " + request.error);
                callback?.Invoke(false);
            }
        }
    }

    // 📌 Login a user (returns success/failure via callback)
    public void Login(string email, string password, Action<bool> callback)
    {
        StartCoroutine(LoginRequest(email, password, callback));
    }

     // 📌 Get All Courses Started by a User (Using Arrays)
    public void GetUserCourses(Action<Course[]> callback)
    {
    
        StartCoroutine(GetUserCoursesRequest(callback));
    }

    // 📌 Start a new course for the user or continue an existing one
    public void startCourse(string courseName, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot start course: No user is logged in");
            return;
        }

        StartCoroutine(StartCourseRequest(courseName, callback));
    }

    private IEnumerator StartCourseRequest(string courseName, Action<bool> callback)
    {
        string json = "{\"course_name\":\"" + courseName + "\", \"username\":\"" + loggedInUser.username + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/start-course", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                callback?.Invoke(true);
                
            }
            else
            {
                Debug.LogError("Failed to start course: " + request.error);
                callback?.Invoke(false);
            }
        }
    }


    private IEnumerator GetUserCoursesRequest(Action<Course[]> callback)
    {
        string url = serverUrl + "/user-courses/" + loggedInUser.username;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Courses retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON manually
                    JArray jsonArray = JArray.Parse(jsonResponse);
                    Course[] courses = new Course[jsonArray.Count];

                    for (int i = 0; i < jsonArray.Count; i++)
                    {
                        JObject obj = (JObject)jsonArray[i];

                        string courseName = obj["course_name"].ToString();
                        int numChapaters = obj["numChapters"].ToObject<int>();
                        string timeStarted = obj["time_started"].ToString();

                        courses[i] = new Course(courseName, numChapaters, timeStarted);
                    }

                    callback?.Invoke(courses);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing courses JSON: " + ex.Message);
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("No courses found");
                callback?.Invoke(new Course [0]);
            }
        }
    }

    // 📌 Purchase an item from the shop
    public void BuyItem(string itemName, string itemType, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot buy item: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(BuyItemRequest(itemName, itemType, callback));
    }

    private IEnumerator BuyItemRequest(string itemName, string itemType, Action<bool> callback)
    {
        string json = "{\"username\":\"" + loggedInUser.username + "\", \"item_name\":\"" + itemName + "\", \"item_type\":\"" + itemType + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/buy-item", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Item purchased successfully: " + responseText);
                
                // Parse response to check purchase result and update user data if needed
                try
                {
                    JObject response = JObject.Parse(responseText);
                    
                    // If response includes updated user score/coins, update the local user object
                    if (response.ContainsKey("remaining_credits"))
                    {
                        int newBalance = response["remaining_credits"].ToObject<int>();
                        loggedInUser.score = newBalance;  // Assuming score is used as currency
                        
                        Debug.Log($"User balance updated to: {newBalance}");
                    }
                    // Add the newly purchased item to the user's purchased items list
                    if (loggedInUser.purchasedItems != null)
                    {
                        UserItem newItem = new UserItem(itemName, itemType);
                        loggedInUser.purchasedItems.Add(newItem);
                        Debug.Log($"Added {itemName} to user's purchased items");
                    }

                    callback?.Invoke(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing purchase response: " + ex.Message);
                    callback?.Invoke(true);  // Purchase likely succeeded even if parsing failed
                }
            }
            else
            {
                string errorMessage = request.error;
                
                // Try to get more detailed error from response if available
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    try
                    {
                        JObject errorResponse = JObject.Parse(request.downloadHandler.text);
                        if (errorResponse.ContainsKey("error"))
                        {
                            errorMessage = errorResponse["error"].ToString();
                        }
                    }
                    catch { /* Use default error message if parsing fails */ }
                }
                
                Debug.LogError($"Failed to purchase item: {errorMessage}");
                callback?.Invoke(false);
            }
        }
    }

    // 📌 Helper function to wrap JSON array for Unity's JsonUtility
    private string WrapJsonArray(string json)
    {
        return "{\"courses\":" + json + "}"; // Wraps in a JSON object for proper parsing
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
                string responseText = request.downloadHandler.text;
                Debug.Log("Login successful: " + responseText);

                // Parse JSON to extract user details
                JObject jsonResponse = JObject.Parse(responseText);
                if (jsonResponse.ContainsKey("username") && jsonResponse.ContainsKey("email") && jsonResponse.ContainsKey("createdAt"))
                {
                    string username = jsonResponse["username"].ToString();
                    string emailAddress = jsonResponse["email"].ToString();
                    string createdAt = jsonResponse["createdAt"].ToString();
                    int score = jsonResponse["score"].ToObject<int>();
                    int numGems= jsonResponse["numGems"].ToObject<int>();
                    int streak_counter = jsonResponse["streak_counter"].ToObject<int>();


                    // Create and store the User object
                    loggedInUser = new User(emailAddress, username, createdAt, score, numGems, streak_counter);
                    StartCoroutine(LoadUserItemsAfterLogin(callback));
                    yield break;
                }

                callback?.Invoke(true); // Success
            }
            else
            {
                Debug.LogError("Login failed: " + request.error);
                callback?.Invoke(false); // Failure
            }
        }
    }

    private IEnumerator LoadUserItemsAfterLogin(Action<bool> loginCallback)
    {
        // Wait for the coroutine to complete and store items in the user object
        bool itemsLoaded = false;
        GetUserItems((items) => {
            if (items != null)
            {
                loggedInUser.purchasedItems = items;
                Debug.Log($"Loaded {items.Count} purchased items for user {loggedInUser.username}");

                // Extract player moves from purchased items
                List<string> movesList = new List<string>();
                foreach (UserItem item in items)
                {
                    if (item.item_type == "Move" && movesList.Count < 4)
                    {
                        movesList.Add(item.item_name);
                    }
                    else if(item.item_type == "character" && item.item_name=="Jessica"){
                        loggedInUser.equippedCharacter = item.item_name;
                    }
                }
                
                // Set the player moves
                loggedInUser.playerMoves = movesList.ToArray();
                Debug.Log($"Set {loggedInUser.playerMoves.Length} player moves");
            }
            itemsLoaded = true;
        });
        
        // Wait until items are loaded
        while (!itemsLoaded)
        {
            yield return null;
        }
        
        // Now finalize the login process
        loginCallback?.Invoke(true);
    }

    // 📌 Get Course Structure
    public void GetCourseStructure(Action<CourseStructure> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot get course structure: No user is logged in");
            callback?.Invoke(null);
            return;
        }

        StartCoroutine(GetCourseStructureRequest(callback));
    }

    // 📌 Get all items owned by the user
    public void GetUserItems(Action<List<UserItem>> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot get user items: No user is logged in");
            callback?.Invoke(null);
            return;
        }

        StartCoroutine(GetUserItemsRequest(callback));
    }

    private IEnumerator GetUserItemsRequest(Action<List<UserItem>> callback)
    {
        string url = $"{serverUrl}/user-items/{loggedInUser.username}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("User items retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON using JObject
                    JObject responseObj = JObject.Parse(jsonResponse);
                    JArray itemsArray = (JArray)responseObj["items"];
                    
                    List<UserItem> userItems = new List<UserItem>();
                    
                    foreach (JObject itemObj in itemsArray)
                    {
                        string itemName = itemObj["item_name"].ToString();
                        string itemType = itemObj["item_type"].ToString();
                        
                        userItems.Add(new UserItem(itemName, itemType));
                    }
                    
                    callback?.Invoke(userItems);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing user items JSON: " + ex.Message);
                    callback?.Invoke(new List<UserItem>());
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve user items: {request.error}");
                callback?.Invoke(new List<UserItem>());
            }
        }
    }

    // Completes the current level with time and score data
    public void CompleteLevel(bool isFailed, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot complete level: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        // Get the current level name from the course structure
        string levelName = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1].level_name;
        string chapter_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].chapter_name;
        
        // Get time and score from the user object
        float timeTaken = loggedInUser.getLevelTime();
        float score = loggedInUser.getLevelScore();
        
        StartCoroutine(CompleteLevelRequest(levelName, chapter_name, loggedInUser.currentLevel, timeTaken, score, isFailed, callback));
    }

    private IEnumerator CompleteLevelRequest(string levelName, string chapter_name, int level_number, float timeTaken, float score, bool isFailed, Action<bool> callback)
    {
        // Create JSON data with all required parameters
        string json = "{" +
        "\"username\":\"" + loggedInUser.username + "\"," +
        "\"level_name\":\"" + levelName + "\"," + 
        "\"chapter_name\":\"" + chapter_name + "\"," +
        "\"level_number\":" + level_number + "," +
        "\"time_taken\":" + timeTaken.ToString("F2") + "," +
        "\"score\":" + score + "," +
        "\"isFailed\":" + isFailed.ToString().ToLower() + "," +
        "\"streak_counter\":" + loggedInUser.consecutiveLevelsWithoutFailure +
    "}";;
        
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/update-level", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Level completed successfully: " + responseText);
                
                try
                {
                    // Parse response to check for any updates (e.g., achievements unlocked)
                    JObject response = JObject.Parse(responseText);
                    
                    if (!isFailed && loggedInUser.courseStructure != null)
                    {
                        // Update current level
                        Chapter currentChapter = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter];
                        Level currentLevel = currentChapter.levels[loggedInUser.currentLevel - 1];
                        
                        // Update level properties
                        currentLevel.isCompleted = true;
        
                        if ((int)score > currentLevel.score)
                        {
                            currentLevel.score = (int)score;
                        }
                        
                        // Find and unlock the next level
                        if (loggedInUser.currentLevel < currentChapter.levels.Count)
                        {
                            // Unlock next level in same chapter
                            Level nextLevel = currentChapter.levels[loggedInUser.currentLevel];
                            nextLevel.status = "unlocked";
                        }
                        
                        Debug.Log("Updated local course structure after level completion");
                    }
                    else{
                        Chapter currentChapter = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter];
                        Level currentLevel = currentChapter.levels[loggedInUser.currentLevel - 1];
                        if ((int)score > currentLevel.score)
                        {
                            currentLevel.score = (int)score;
                        }
                    }
                    callback?.Invoke(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing level completion response: " + ex.Message);
                    callback?.Invoke(true); // Level likely completed even if parsing failed
                }
            }
            else
            {
                Debug.LogError("Failed to complete level: " + request.error);
                callback?.Invoke(false);
            }
        }
    }

        // Unlock an achievement for the current user
    public void UnlockAchievement(string achievementName, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot unlock achievement: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(UnlockAchievementRequest(achievementName, callback));
    }

    private IEnumerator UnlockAchievementRequest(string achievementName, Action<bool> callback)
    {
        // Create JSON data with username and achievement name
        string json = "{" +
            "\"username\":\"" + loggedInUser.username + "\"," +
            "\"achievement_name\":\"" + achievementName + "\"" +
        "}";
        
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/complete-achievement", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Achievement unlocked successfully: " + responseText);
                
                try
                {
                    // Parse response to update user data if needed
                    JObject response = JObject.Parse(responseText);
                    
                    // Update user data if gems or score were updated
                    if (response.ContainsKey("numGems"))
                    {
                        loggedInUser.numGems = response["numGems"].ToObject<int>();
                    }
                    
                    if (response.ContainsKey("score"))
                    {
                        loggedInUser.score = response["score"].ToObject<int>();
                    }
                    
                    callback?.Invoke(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing achievement unlock response: " + ex.Message);
                    callback?.Invoke(true); // Achievement likely unlocked even if parsing failed
                }
            }
            else
            {
                Debug.LogError("Failed to unlock achievement: " + request.error);
                callback?.Invoke(false);
            }
        }
    }

    public void CompleteObjective(string objectiveName, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot complete objective: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(CompleteObjectiveRequest(objectiveName, callback));
    }

    private IEnumerator CompleteObjectiveRequest(string objectiveName, Action<bool> callback)
    {
        string json = "{\"username\":\"" + loggedInUser.username + "\", \"objective_name\":\"" + objectiveName + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/complete-objective", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Objective completed successfully: " + responseText);
                
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError("Failed to complete objective: " + request.error);
                callback?.Invoke(false);
            }
        }
    }

   
    // Start the level timer on the server
    public void StartLevelTime()
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot start level timer: No user is logged in");
          
            return;
        }

        // Get the current level name and chapter name from the course structure
        string levelName = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel-1].level_name;
        string chapterName = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].chapter_name;

        StartCoroutine(StartLevelTimeRequest(loggedInUser.username, levelName, chapterName));
    }

    private IEnumerator StartLevelTimeRequest(string username, string levelName, string chapterName)
    {
        string json = "{" +
            "\"username\":\"" + username + "\"," +
            "\"level_name\":\"" + levelName + "\"," +
            "\"chapter_name\":\"" + chapterName + "\"" +
        "}";
        
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/start-level-time", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Level timer started successfully: " + responseText);
                
                try
                {
                    // Parse the response to get the start time
                    JObject response = JObject.Parse(responseText);
                    
                    if (response.ContainsKey("start_level_time"))
                    {
                        string startTime = response["start_level_time"].ToString();

                        Debug.Log($"Level start time: {startTime}");
                        loggedInUser.currentLevelStartTime = startTime;
                    }
                    else
                    {
                        Debug.LogWarning("Response doesn't contain start_time field");
                        
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing start time response: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError("Failed to start level timer: " + request.error);
              
            }
        }
    }

    private IEnumerator GetCourseStructureRequest(Action<CourseStructure> callback)
    {
        string username = loggedInUser.username;
        string courseName = loggedInUser.selectecCourse;
        string url = $"{serverUrl}/course-structure/{username}/{courseName}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Course structure retrieved: " + jsonResponse);

                try
                {
                    // Parse JSON using JObject
                    JObject courseObj = JObject.Parse(jsonResponse);
                    
                    string name = courseObj["course_name"].ToString();
                    int id = courseObj["course_id"].ToObject<int>();
                    int numChapters = courseObj["numChapters"].ToObject<int>();
                    
                    List<Chapter> chapters = new List<Chapter>();
                    JArray chaptersArray = (JArray)courseObj["chapters"];
                    
                    Debug.Log("Parsing chapters");
                    foreach (JObject chapterObj in chaptersArray)
                    {
                        string chapterName = chapterObj["chapter_name"].ToString();
                        string status = chapterObj["status"].ToString();
                        
                        List<Level> levels = new List<Level>();
                        JArray levelsArray = (JArray)chapterObj["levels"];
                        
                        Debug.Log("Parsing levels");
                        foreach (JObject levelObj in levelsArray)
                        {
                            string levelName = levelObj["level_name"].ToString();
                            int score = levelObj["score"].ToObject<int>();
                            string level_status = levelObj["status"].ToString();
                            int levelNumber = levelObj["levelNumber"].ToObject<int>();
                            int points = levelObj["points"].ToObject<int>();
                            bool isCompleted = levelObj["isCompleted"].ToObject<bool>();
                            
                            levels.Add(new Level(levelName, score, level_status, levelNumber, points, isCompleted));
                        }
                        
                        chapters.Add(new Chapter(chapterName, status, levels));
                    }
                    
                    Debug.Log("Setting course structure");
                    CourseStructure courseStructure = new CourseStructure(name, id, numChapters, chapters);
                    loggedInUser.courseStructure=courseStructure;
                    callback?.Invoke(courseStructure);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing course structure JSON: " + ex.Message);
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve course structure: {request.error}");
                callback?.Invoke(null);
            }
        }
    }
}
