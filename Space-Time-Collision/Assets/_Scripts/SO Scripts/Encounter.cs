using UnityEngine;

[CreateAssetMenu(fileName = "Encounter", menuName = "Scriptable Objects/Encounter")]
public class Encounter : ScriptableObject
{
    public EnemyEncounterInfo[] encounterEnemies;
}

[System.Serializable]
public class EnemyEncounterInfo
{
    public EnemyInfo enemy;
    public int xPos;
    public int yPos;
}
