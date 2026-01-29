using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyInfo[] allEnemies;
    [SerializeField] private List<Enemy> currentEnemies;
    
    private static GameObject _instance;

    private const float LEVEL_MODIFIER = 0.5f;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    public void GenerateEnemiesByEncounter(Encounter tempEncounter)
    {
        currentEnemies.Clear();
        for (int i = 0; i < tempEncounter.encounterEnemies.Length; i++) {
            // TODO add level scaling so future worlds get tougher
            GenerateEnemyByName(tempEncounter.encounterEnemies[i].enemy.enemyName, 1,
                tempEncounter.encounterEnemies[i].xPos, tempEncounter.encounterEnemies[i].yPos); 
        }
    }

    private void GenerateEnemyByName(string enemyName, int level, int encounterXPos, int encounterYPos)
    {
        for (int i = 0; i < allEnemies.Length; i++) {
            if (allEnemies[i].enemyName == enemyName) {
                Enemy newEnemy = new Enemy();
                
                newEnemy.enemyBaseName = allEnemies[i].enemyName;
                if (currentEnemies.Any(t => t.enemyName == newEnemy.enemyBaseName)) {
                    newEnemy.enemyName = (newEnemy.enemyBaseName + " " + 
                                          (currentEnemies.Count(t => t.enemyBaseName == newEnemy.enemyBaseName) + 1));
                } else {
                    newEnemy.enemyName = newEnemy.enemyBaseName;
                }
                
                newEnemy.enemyPortrait = allEnemies[i].enemyPortrait;
                newEnemy.level = level;
                float levelModifier = (LEVEL_MODIFIER * (newEnemy.level - 1));
                
                // TODO Make this pull from encounter sets
                newEnemy.xPos = encounterXPos;
                newEnemy.yPos = encounterYPos;
                
                newEnemy.maxHealth = Mathf.RoundToInt(allEnemies[i].baseHealth + (allEnemies[i].baseHealth * levelModifier));
                newEnemy.currentHealth = newEnemy.maxHealth;
                newEnemy.maxDefense = Mathf.RoundToInt(allEnemies[i].baseDefense + (allEnemies[i].baseDefense * levelModifier));
                newEnemy.currentDefense = newEnemy.maxDefense;
                newEnemy.maxArmor = Mathf.RoundToInt(allEnemies[i].baseArmor + (allEnemies[i].baseArmor * levelModifier));
                newEnemy.currentArmor = newEnemy.maxArmor;
                newEnemy.maxSpirit = Mathf.RoundToInt(allEnemies[i].baseSpirit + (allEnemies[i].baseSpirit * levelModifier));
                newEnemy.currentSpirit = newEnemy.maxSpirit;
                
                newEnemy.power = Mathf.RoundToInt(allEnemies[i].basePower + (allEnemies[i].basePower * levelModifier));
                newEnemy.skill = Mathf.RoundToInt(allEnemies[i].baseSkill + (allEnemies[i].baseSkill * levelModifier));
                newEnemy.wit = Mathf.RoundToInt(allEnemies[i].baseWit + (allEnemies[i].baseWit * levelModifier));
                newEnemy.mind = Mathf.RoundToInt(allEnemies[i].baseMind + (allEnemies[i].baseMind * levelModifier));
                newEnemy.speed = Mathf.RoundToInt(allEnemies[i].baseSpeed + (allEnemies[i].baseSpeed * levelModifier));
                newEnemy.luck = Mathf.RoundToInt(allEnemies[i].baseLuck + (allEnemies[i].baseLuck * levelModifier));
                
                newEnemy.stunResist = allEnemies[i].baseStunResist;
                newEnemy.debuffResist = allEnemies[i].baseDebuffResist;
                newEnemy.ailmentResist = allEnemies[i].baseAilmentResist;
                
                newEnemy.fundDrop = Random.Range(allEnemies[i].minFund, allEnemies[i].maxFund + 1);
                
                newEnemy.enemyVisualPrefab = allEnemies[i].enemyBattleVisualPrefab;
                
                newEnemy.enemyBrain = allEnemies[i].baseEnemyBrain;
                newEnemy.abilities = new List<Ability>();
                foreach (EnemyAbility t in newEnemy.enemyBrain.enemyAbilities)
                {
                    newEnemy.abilities.Add(t.ability);
                }
                
                currentEnemies.Add(newEnemy);
            }
        }
        
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
    }
    
    public List<Ability> GetAbilities(int enemyIndex)
    {
        List<Ability> abilities = currentEnemies[enemyIndex].abilities;
        return abilities;
    }
}

[System.Serializable]
public class Enemy
{
    public string enemyBaseName;
    public string enemyName;
    public Sprite enemyPortrait;
    public int level;
    
    public int xPos;
    public int yPos;
    
    public int maxHealth;
    public int currentHealth;
    public int maxDefense;
    public int currentDefense;
    public int maxArmor;
    public int currentArmor;
    // Unsure if the enemies will need the two below
    public int maxSpirit;
    public int currentSpirit;
    
    public int power;
    public int skill;
    public int wit;
    public int mind;
    public int speed;
    public int luck;
    
    public int stunResist;
    public int debuffResist;
    public int ailmentResist;

    public int fundDrop;
    
    public GameObject enemyVisualPrefab; // what will be displayed in the battle scene
    public EnemyBrain enemyBrain;
    
    public List<Ability> abilities;
}
