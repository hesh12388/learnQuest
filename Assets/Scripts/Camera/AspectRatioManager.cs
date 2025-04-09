using UnityEngine;
using UnityEngine.SceneManagement;

public class AspectRatioManager : MonoBehaviour
{
    public float targetAspectRatio = 1f; // 1:1

    private static AspectRatioManager instance;

    private void Awake()
    {
        // Singleton to avoid duplicates
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Apply the aspect ratio when a new scene is loaded
        Debug.Log("Applying aspect ratio to main camera.");
        ApplyToMainCamera();
    }

    private void ApplyToMainCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspectRatio;

        if (scaleHeight < 1.0f)
        {
            cam.rect = new Rect(0, (1.0f - scaleHeight) / 2.0f, 1, scaleHeight);
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            cam.rect = new Rect((1.0f - scaleWidth) / 2.0f, 0, scaleWidth, 1);
        }
    }
}
