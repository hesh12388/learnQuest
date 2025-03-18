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

    private string serverUrl = "http://localhost:5001"; // Change to your deployed server URL

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
    public void StartLevel()
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot start level: No user is logged in");
            return;
        }

        StartCoroutine(StartLevelRequest());
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
        string level_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel].level_name;
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
                    JArray objectivesArray = (JArray)responseObj["objectives"];
                    
                    List<Objective> objectives = new List<Objective>();
                    
                    foreach (JObject objectiveObj in objectivesArray)
                    {
                        string objectiveName = objectiveObj["objective_name"].ToString();
                        string status = objectiveObj["status"].ToString();
                        string description = objectiveObj["description"].ToString();
                        int difficulty = objectiveObj["difficulty"].ToObject<int>();
                        
                        objectives.Add(new Objective(objectiveName, status, description, difficulty));
                    }
                    
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

    private IEnumerator StartLevelRequest()
    {
        string level_name = loggedInUser.courseStructure.chapters[loggedInUser.currentChapter].levels[loggedInUser.currentLevel].level_name;
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
            }
            else
            {
                Debug.LogError("Failed to start level: " + request.error);
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
                        int bestTime = entryObj["bestTime"].ToObject<int>();
                        int numAchievements = entryObj["numAchievements"].ToObject<int>();
                        
                        leaderboardEntries.Add(new LeaderboardEntry(username, score, numAchievements, bestTime));
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

    public void restartCourse(string courseName, Action<bool> callback)
    {
        if (loggedInUser == null)
        {
            Debug.LogError("Cannot restart course: No user is logged in");
            callback?.Invoke(false);
            return;
        }

        StartCoroutine(RestartCourseRequest(courseName, callback));
    }

    private IEnumerator RestartCourseRequest(string courseName, Action<bool> callback)
    {
        string json = "{\"course_name\":\"" + courseName + "\", \"username\":\"" + loggedInUser.username + "\"}";
        byte[] jsonData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/restart-course", "POST"))
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


                    // Create and store the User object
                    loggedInUser = new User(emailAddress, username, createdAt);
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
                        
                        levels.Add(new Level(levelName, score, level_status, levelNumber));
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
