using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class EncounterSystem : MonoBehaviour
{

    [SerializeField] private List<Encounter> easyEncounters;
    [SerializeField] private List<Encounter> normalEncounters;
    [SerializeField] private List<Encounter> minibossEncounters;
    [SerializeField] private List<Encounter> demoEncounters;
    
    private EnemyManager enemyManager;
    private RunInfo runInfo;
    
    private void Awake()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
        runInfo = FindFirstObjectByType<RunInfo>();
    }

    public void GenerateDemoEncounter()
    {
        int encounterToUse = runInfo.GetEncounterCount();
        enemyManager.GenerateEnemiesByEncounter(demoEncounters[encounterToUse]);
        runInfo.IncreaseEncounterCount();
    }

    public void GenerateStandardEncounter()
    {
        int encounterToUse;
        Encounter previousEncounter;
        if (runInfo.GetEncounterCount() > 2) {
            previousEncounter = runInfo.GetPreviousEncounter();
            if (previousEncounter != null && normalEncounters.Any(t => t == previousEncounter)) {
                normalEncounters.Remove(previousEncounter);
            }
            encounterToUse = Random.Range(0, normalEncounters.Count);
            runInfo.SetPreviousEncounter(normalEncounters[encounterToUse]);
            enemyManager.GenerateEnemiesByEncounter(normalEncounters[encounterToUse]);
        } else {
            previousEncounter = runInfo.GetPreviousEncounter();
            if (previousEncounter != null && easyEncounters.Any(t => t == previousEncounter)) {
                easyEncounters.Remove(previousEncounter);
            }
            encounterToUse = Random.Range(0, easyEncounters.Count);
            runInfo.SetPreviousEncounter(easyEncounters[encounterToUse]);
            enemyManager.GenerateEnemiesByEncounter(easyEncounters[encounterToUse]);
        }
        runInfo.IncreaseEncounterCount();
    }

    public void GenerateMinibossEncounter()
    {
        Encounter previousEncounter = runInfo.GetPreviousEncounter();
        if (previousEncounter != null && minibossEncounters.Any(t => t == previousEncounter)) {
            minibossEncounters.Remove(previousEncounter);
        }
        int encounterToUse = Random.Range(0, minibossEncounters.Count);
        runInfo.SetPreviousEncounter(minibossEncounters[encounterToUse]);
        enemyManager.GenerateEnemiesByEncounter(minibossEncounters[encounterToUse]);
    }
}