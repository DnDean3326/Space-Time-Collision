using UnityEngine;
using UnityEngine.Serialization;

public class EncounterSystem : MonoBehaviour
{

    [SerializeField] private Encounter[] easyEncounters;
    [SerializeField] private Encounter[] normalEncounters;
    [SerializeField] private Encounter[] minibossEncounters;
    
    private EnemyManager enemyManager;
    private RunInfo runInfo;
    
    private void Awake()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
        runInfo = FindFirstObjectByType<RunInfo>();
    }
    

    public void GenerateStandardEncounter()
    {
        int encounterToUse;
        if (runInfo.GetEncounterCount() > 2) {
            encounterToUse = Random.Range(0, normalEncounters.Length + 1);
            enemyManager.GenerateEnemiesByEncounter(normalEncounters[encounterToUse]);
        } else {
            encounterToUse = Random.Range(0, easyEncounters.Length + 1);
            enemyManager.GenerateEnemiesByEncounter(easyEncounters[encounterToUse]);
        }
        runInfo.IncreaseEncounterCount();
    }

    public void GenerateMinibossEncounter()
    {
        int encounterToUse = Random.Range(0, minibossEncounters.Length + 1);
        enemyManager.GenerateEnemiesByEncounter(minibossEncounters[encounterToUse]);
    }
}