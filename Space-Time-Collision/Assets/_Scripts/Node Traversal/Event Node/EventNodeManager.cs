using System.Collections.Generic;
using UnityEngine;

public class EventNodeManager : MonoBehaviour
{
    [SerializeField] private List<EventInfo> allEvents;
    private RunInfo runInfo;
    
    private static GameObject _instance;
    
    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
        runInfo = FindFirstObjectByType<RunInfo>();
    }

    public EventInfo GenerateNodeEvent()
    {
        List<EventInfo> eventPool = new List<EventInfo>();
        foreach (EventInfo eventInfo in allEvents) {
            eventPool.Add(eventInfo);
        }
        if (runInfo.GetEventCompletedCount() > 0) {
            List<EventInfo> completedEvents = runInfo.GetCompletedEvents();
            foreach (EventInfo completedEvent in completedEvents) {
                if (eventPool.Contains(completedEvent)) {
                    eventPool.Remove(completedEvent);
                }
            }
        }
        // If requirements for event are not met, remove it from the pool
        EventInfo eventForUse = eventPool[Random.Range(0, eventPool.Count)];
        return eventForUse;
    }
}
