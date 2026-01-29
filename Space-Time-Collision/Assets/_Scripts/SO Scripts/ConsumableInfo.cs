using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableInfo", menuName = "Scriptable Objects/ConsumableInfo")]
public class ConsumableInfo : ScriptableObject
{
    public string consumableName;
    public Sprite consumableIcon;
    public int consumablePrice;

    public Ability linkedAbility;
}
