using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;

[System.Serializable]
public class NPCIndicatorMapping
{
    public string npcName;
    public GameObject indicator;
}

public class NPCManager : MonoBehaviour
{
   
    private HashSet<string> completedNPCs = new HashSet<string>();

    public int numNpcs;
    public static NPCManager Instance;
    public CinemachineVirtualCamera shallowCamera;
    public CinemachineVirtualCamera wideCamera;

    private Dictionary<string, GameObject> npcIndicators = new Dictionary<string, GameObject>();

    [SerializeField]
    private List<NPCIndicatorMapping> npcIndicatorsList = new List<NPCIndicatorMapping>();

    private void Awake()
    {
        // Setup singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Convert the list to a dictionary at runtime
        InitializeDictionary();
    }

    // Initialize the dictionary from inspector-assigned list
    private void InitializeDictionary()
    {
        npcIndicators.Clear();
        
        foreach (var mapping in npcIndicatorsList)
        {
            if (!string.IsNullOrEmpty(mapping.npcName) && mapping.indicator != null)
            {
                npcIndicators[mapping.npcName] = mapping.indicator;
                // Hide all indicators initially
                mapping.indicator.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Invalid NPC indicator mapping detected. Check your Inspector settings.");
            }
        }
    }


    public void showIndicator(string npcName){
        StartCoroutine(showNpcIndicator(npcName));
    }

    private IEnumerator showNpcIndicator(string npcName){
        CameraManager.SwitchCamera(wideCamera);
        npcIndicators[npcName].SetActive(true);
        yield return new WaitForSeconds(4f);
        CameraManager.SwitchCamera(shallowCamera);
        npcIndicators[npcName].SetActive(false);
    }


    public bool HasCompletedNPC(string npcName)
    {
        return completedNPCs.Contains(npcName);
    }

    public  void MarkNPCCompleted(string npcName)
    {
        if (!completedNPCs.Contains(npcName))
        {
            completedNPCs.Add(npcName);
        }

        if (AreAllNPCsCompleted())
        {
            Debug.Log("All NPCs completed!");
            AspectController.Instance.SetBattleAspect();
            EvaluationManager.Instance.StartEvaluation();
        }
    }

    public bool AreAllNPCsCompleted()
    {
        return completedNPCs.Count == numNpcs;
    }
}
