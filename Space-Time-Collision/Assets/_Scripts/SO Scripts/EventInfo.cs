using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventInfo", menuName = "Scriptable Objects/EventInfo")]
public class EventInfo : ScriptableObject
{
    public string eventName;
    [TextArea(5,20)] public string eventDescription;
    [TextArea(3,20)] public List<string> eventOptions;
    [TextArea(10,20)] public List<string> eventResults;
}
