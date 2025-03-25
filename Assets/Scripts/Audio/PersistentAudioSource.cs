using UnityEngine;

/// <summary>
/// Makes an AudioSource persist between scene loads.
/// Attach this to a GameObject that has an AudioSource component.
/// </summary>
public class PersistentAudioSource : MonoBehaviour
{
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}