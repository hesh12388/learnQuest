using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager: MonoBehaviour{
    static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    public static CinemachineVirtualCamera ActiveCamera = null;

    public static bool IsActiveCamera(CinemachineVirtualCamera camera){
        return ActiveCamera == camera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCamera){
       newCamera.Priority=10;
       ActiveCamera = newCamera;

       foreach(CinemachineVirtualCamera c in cameras){
           if(c != newCamera){
               c.Priority = 0;
           }
       }
    }

    public static void RegisterCamera(CinemachineVirtualCamera camera){
        cameras.Add(camera);
    }

    public static void UnregisterCamera(CinemachineVirtualCamera camera){
        cameras.Remove(camera);
    }

}