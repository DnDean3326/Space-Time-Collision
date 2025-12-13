using UnityEngine;

public class EncounterSystem : MonoBehaviour
{

    [SerializeField] private Encounter[] enemiesInScene;
    [SerializeField] private int maxNumEnemies;

    private EnemyManager enemyManager;
    
    private void Awake()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
    }

    
    void Start()
    {
        enemyManager.GenerateEnemiesByEncounter(enemiesInScene, maxNumEnemies);
    }
}

[System.Serializable]
public class Encounter
{
    public EnemyInfo enemy;
    public int levelMin;
    public int levelMax;
}
