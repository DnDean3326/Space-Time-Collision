using System.Collections.Generic;
using UnityEngine;

public class RunInfo : MonoBehaviour
{
    private int encounterCount;
    private bool easyEncountersOver;
    private Encounter previousEncounter;
    private int eventCount;
    private List<EventInfo> completedEvents;
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
    
    public bool GetEncounterStatus()
    {
        if (!easyEncountersOver && encounterCount > 2) {
            easyEncountersOver = true;
        } 
        return easyEncountersOver;
    }

    public void ChangeEncounterStatus(bool status)
    {
        easyEncountersOver = status;
    }

    public void IncreaseEncounterCount()
    {
        encounterCount++;
    }

    public List<EventInfo> GetCompletedEvents()
    {
        return completedEvents;
    }

    public int GetEventCompletedCount()
    {
        return completedEvents.Count;
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

    public List<Consumable> GetConsumables()
    {
        return consumableInventory;
    }

    public int GetConsumableCount()
    {
        return consumableInventory.Count;
    }

    public void AddTalisman(Talisman talisman)
    {
        talismanInventory.Add(talisman);
    }
    
    public List<Talisman> GetTalismans()
    {
        return talismanInventory;
    }
    
    public int GetTalismanCount()
    {
        return talismanInventory.Count;
    }

    public void RemoveConsumable(Consumable consumable)
    {
        consumableInventory.Remove(consumable);
    }

    public void EndRun()
    {
        Destroy(gameObject);
    }
}
