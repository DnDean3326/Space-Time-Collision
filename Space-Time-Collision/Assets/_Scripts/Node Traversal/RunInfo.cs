using System.Collections.Generic;
using UnityEngine;

public class RunInfo : MonoBehaviour
{
    private int encounterCount;
    private Encounter previousEncounter;
    private int eventCount;
    private int funds;
    private int currentNode;
    
    private NodeManager nodeManager;
    
    private static GameObject _instance;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
        
        nodeManager = FindFirstObjectByType<NodeManager>();
    }

    public void SetCurrentNode(int noteIndex)
    {
        currentNode = noteIndex;
    }

    public int GetCurrentNode()
    {
        return currentNode;
    }

    public void SetPreviousEncounter(Encounter currentEncounter)
    {
        previousEncounter = currentEncounter;
    }
    
    public Encounter GetPreviousEncounter()
    {
        if (previousEncounter != null) {
            return previousEncounter;
        }

        return null;
    }
    
    public int GetEncounterCount()
    {
        return encounterCount;
    }

    public void IncreaseEncounterCount()
    {
        encounterCount++;
    }

    public void EndRun()
    {
        Destroy(gameObject);
    }
}
