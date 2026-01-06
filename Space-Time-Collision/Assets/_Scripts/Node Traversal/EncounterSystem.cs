using UnityEngine;

public class EncounterSystem : MonoBehaviour
{

    [SerializeField] private Encounter[] availableEncounters;
    [SerializeField] private int? lastEncounterUsed = null;

    private EnemyManager enemyManager;
    
    private void Awake()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
    }

    
    void Start()
    {
        int encounterToUse = Random.Range(0, availableEncounters.Length);
        if (encounterToUse == lastEncounterUsed) {
            encounterToUse += 1;
            if (encounterToUse >= availableEncounters.Length) {
                encounterToUse = 0;
            }
        }
        enemyManager.GenerateEnemiesByEncounter(availableEncounters[encounterToUse]);
        lastEncounterUsed = encounterToUse;
    }
}