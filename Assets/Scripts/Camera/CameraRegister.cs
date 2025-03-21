using UnityEngine;
using Cinemachine;

public class CameraRegister : MonoBehaviour{

    private void OnEnable(){
        CameraManager.RegisterCamera(GetComponent<CinemachineVirtualCamera>());
    }

    private void OnDisable(){
        CameraManager.UnregisterCamera(GetComponent<CinemachineVirtualCamera>());
    }
}