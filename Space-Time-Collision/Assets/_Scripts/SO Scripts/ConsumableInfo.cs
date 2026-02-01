using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ConsumableInfo", menuName = "Scriptable Objects/ConsumableInfo")]
public class ConsumableInfo : ScriptableObject
{
    public string consumableName;
    public Sprite consumableIcon;
    public int consumablePrice;
    public Ability consumableAbility;

    [Header("Description")]
    [TextArea(10,20)]
    public string itemDescription;
}
