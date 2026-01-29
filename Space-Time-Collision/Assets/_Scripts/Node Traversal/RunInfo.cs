using System.Collections.Generic;
using UnityEngine;

public class RunInfo : MonoBehaviour
{
    private int encounterCount;
    private Encounter previousEncounter;
    private int eventCount;
    [SerializeField] private int funds;
    [SerializeField] private int currentNode;
    [SerializeField] private List<Consumable> consumableInventory = new List<Consumable>();
    [SerializeField] private List<Talisman> talismanInventory = new List<Talisman>();
    
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

    public void SetCurrentNode(int nodeIndex)
    {
        currentNode = nodeIndex;
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

    public int GetFunds()
    {
        return funds;
    }

    public void ChangeFunds(int fundChange)
    {
        funds += fundChange;
    }

    public void AddConsumable(Consumable consumable)
    {
        consumableInventory.Add(consumable);
    }

    public void AddTalisman(Talisman talisman)
    {
        talismanInventory.Add(talisman);
    }

    public void RemoveConsumable(int itemIndex)
    {
        consumableInventory.RemoveAt(itemIndex);
    }

    public void EndRun()
    {
        Destroy(gameObject);
    }
}
