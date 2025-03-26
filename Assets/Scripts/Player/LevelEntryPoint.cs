using UnityEngine;

public class LevelEntryPoint : MonoBehaviour
{
    // Visual representation in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}