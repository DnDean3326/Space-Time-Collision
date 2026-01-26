using UnityEngine;
using UnityEngine.Serialization;

public class EncounterSystem : MonoBehaviour
{

    [FormerlySerializedAs("availableEncounters")] [SerializeField] private Encounter[] easyEncounters;
    [SerializeField] private Encounter[] normalEncounters;
    [SerializeField] private Encounter[] minibossEncounters;
    
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
        enemyManager.GenerateEnemiesByEncounter(easyEncounters[encounterToUse]);
    }
}