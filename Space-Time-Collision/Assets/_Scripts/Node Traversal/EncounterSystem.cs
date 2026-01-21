using UnityEngine;

public class EncounterSystem : MonoBehaviour
{

    [SerializeField] private Encounter[] availableEncounters;

    private EnemyManager enemyManager;
    private PlayerPrefs playerPrefs;
    
    private void Awake()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
        playerPrefs = FindFirstObjectByType<PlayerPrefs>();
    }

    
    void Start()
    {
        int encounterToUse = playerPrefs.GetRunStatus();
        enemyManager.GenerateEnemiesByEncounter(availableEncounters[encounterToUse]);
    }
}