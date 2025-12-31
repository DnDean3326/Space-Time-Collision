using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBrain", menuName = "Scriptable Objects/EnemyBrain")]
public class EnemyBrain : ScriptableObject
{
    public enum TargetFaction
    {
        Party,
        Enemies
    }

    public enum TargetMethod
    {
        Random,
        Lowest,
        Highest
    }

    public enum TargetQualifier
    {
        Null,
        
        Health,
        Defense,
        Armor,
        Spirit,
        ActionPoints,
        
        Power,
        Skill,
        Wit,
        Mind,
        Speed,
        Luck,
        
        Proximity
    }

    public List<EnemyAbility> enemyAbilities;
}

[System.Serializable]
public class EnemyAbility
{
    public Ability ability;
    public int abilityPriority;
    public EnemyBrain.TargetMethod targetMethod;
    public EnemyBrain.TargetQualifier targetQualifier;
    public int randomChance;
}
