using UnityEngine;

public class AspectController : MonoBehaviour
{
    public static AspectController Instance { get; private set; }

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
        }
    }

    public void SetBattleAspect()
    {
        SetAspectRatio(3f, 2f);
    }

    public void SetNormalAspect()
    {
        SetAspectRatio(1f, 1f);
    }

    private void SetAspectRatio(float targetWidth, float targetHeight)
    {
        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = Camera.main;
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}
