using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    public static CameraManager Instance { get; private set; } // Singleton instance
    public static CinemachineVirtualCamera ActiveCamera = null;

    private void Awake()
    {
        // Ensure only one instance of CameraManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    public static bool IsActiveCamera(CinemachineVirtualCamera camera)
    {
        return ActiveCamera == camera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCamera)
    {
        newCamera.Priority = 10;
        ActiveCamera = newCamera;
        foreach (CinemachineVirtualCamera c in cameras)
        {
            if (c != newCamera)
            {
                c.Priority = 0;
            }
        }
    }

    public static void RegisterCamera(CinemachineVirtualCamera camera)
    {
        cameras.Add(camera);
    }

    public static void UnregisterCamera(CinemachineVirtualCamera camera)
    {
        cameras.Remove(camera);
    }
}