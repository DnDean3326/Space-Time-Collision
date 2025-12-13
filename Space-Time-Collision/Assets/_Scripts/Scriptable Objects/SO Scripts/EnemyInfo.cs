using UnityEngine;

[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public string enemyName;
    
    public int baseHealth;
    public int baseDefense;
    public int baseArmor;
    public int baseSpirit; // Unsure if enemies need this one
    
    public int basePower;
    public int baseSkill;
    public int baseWit;
    public int baseMind;
    public int baseSpeed;
    public int baseLuck;
    
    public GameObject allyBattleVisualPrefab; // what will be displayed in the battle scene
}
