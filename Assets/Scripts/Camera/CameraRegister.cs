using UnityEngine;
using Cinemachine;

public class CameraRegister : MonoBehaviour
{
    private void OnEnable()
    {
        // Get the Cinemachine Virtual Camera component
        CinemachineVirtualCamera virtualCamera = GetComponent<CinemachineVirtualCamera>();

        // Register the camera with the CameraManager
        CameraManager.RegisterCamera(virtualCamera);

        // Set the camera to follow and look at the PlayerTransform
        if (Player.Instance != null && Player.Instance.PlayerTransform != null)
        {
            virtualCamera.Follow = Player.Instance.PlayerTransform; // Set Follow target
        }
        else
        {
            Debug.LogWarning("Player or PlayerTransform is not available!");
        }
    }

    private void OnDisable()
    {
        // Unregister the camera from the CameraManager
        CameraManager.UnregisterCamera(GetComponent<CinemachineVirtualCamera>());
    }
}