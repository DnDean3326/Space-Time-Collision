using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleSystem : MonoBehaviour
{

    private enum BattleState
    {
        Start,
        Battle,
        Ordering,
        PlayerTurn,
        EnemyTurn,
        Ability,
        Won,
        Lost,
    }

    [Header("Battle System")]
    [SerializeField] private BattleState state;

    [Header("Grid Locations")]
    [SerializeField] private Transform[] partyGridTransform;
    [SerializeField] private Transform[] enemyGridTransform;
    
    [Header("Combatants")]
    [SerializeField] private List<BattleEntities> allCombatants = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyCombatants = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> partyCombatants = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> preparedCombatants = new List<BattleEntities>();
    
    [Header("UI")]
    [SerializeField] private GameObject combatStartUI;
    [SerializeField] private Animator combatStartUIAnimator;
    
    private PartyManager partyManager;
    
    private EnemyManager enemyManager;
    private int currentPlayer;
    
    private const float COMBAT_BEGIN_DELAY = 1f;
    private const float TURN_ACTION_DELAY = 2f;
    private const int TURN_START_THRESHOLD = 200;
    private const int BASE_INITIATIVE_GAIN = 20;
    private const int MAX_INITIATIVE_START = 100;
    
    // Animator Constants
    private const string BATTLE_START_END = "EndTrigger";

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();
    }
    
    
    void Start()
    {
        CreatePartyEntities();
        CreateEnemyEntities();
        state = BattleState.Start;
        StartCoroutine(StartRoutine());
    }

    // Battle state IENumerators go here

    private IEnumerator StartRoutine()
    {
        if (state == BattleState.Start) {
            for (int i = 0; i < allCombatants.Count; i++) {
                allCombatants[i].initiative = Random.Range(90, MAX_INITIATIVE_START + 1);
                print(allCombatants[i].name + "'s starting initiative is " +  allCombatants[i].initiative);
            }

            yield return new WaitForSeconds(COMBAT_BEGIN_DELAY);
            combatStartUIAnimator.SetTrigger(BATTLE_START_END);
            yield return new WaitForSeconds(0.2f);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
            yield return null;
        } else {
            print("Start Routine called but the battle system is not in the start state.");
        }
        yield return null;
    }

    private IEnumerator BattleRoutine()
    {
        print("Battle Routine called");
        do {
            for (int i = 0; i < allCombatants.Count; i++) {
                if (state == BattleState.Battle) {
                    allCombatants[i].initiative += BASE_INITIATIVE_GAIN + allCombatants[i].speed;
                    print(allCombatants[i].name + "'s new initiative is " +  allCombatants[i].initiative);
                    if (allCombatants[i].initiative >= TURN_START_THRESHOLD) {
                        preparedCombatants.Add(allCombatants[i]);
                        //allCombatants[i].initiative -= TURN_START_THRESHOLD;
                        print(allCombatants[i].name + " has been prepared.");
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        } while (preparedCombatants.Count <= 0);

        state = BattleState.Ordering;
        StartCoroutine(OrderRoutine());
        yield return null;
    }

    private IEnumerator OrderRoutine()
    {
        if (state == BattleState.Ordering) {
            // Sorts prepared combatants by initiative from highest to lowest
            preparedCombatants.Sort((bi1, bi2) => -bi1.initiative.CompareTo(bi2.initiative));
            int characterIndex = allCombatants.IndexOf(preparedCombatants[0]);
            if (preparedCombatants[0].isPlayer) {
                state = BattleState.PlayerTurn;
                StartCoroutine(PartyTurnRoutine(characterIndex));
            } else if (!preparedCombatants[0].isPlayer) {
                state = BattleState.EnemyTurn;
                StartCoroutine(EnemyTurnRoutine(characterIndex));
            }
                
        }
        yield return null;
    }

    private IEnumerator PartyTurnRoutine(int characterIndex)
    {
        print("Player turn has begun. " + allCombatants[characterIndex].name + " Is the active character.");
        yield return null;
    }

    private IEnumerator EnemyTurnRoutine(int characterIndex)
    {
        print("Enemy turn has begun. " + allCombatants[characterIndex].name + " Is the active enemy.");
        yield return null;
    }

    private void CreatePartyEntities()
    {
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetCurrentParty();
        
        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            
            tempEntity.SetEntityValue(currentParty[i].memberName, currentParty[i].level, currentParty[i].maxHealth, currentParty[i].currentHealth,
                currentParty[i].maxSpirit, currentParty[i].currentSpirit, currentParty[i].maxDefense, currentParty[i].maxArmor, currentParty[i].power,
                currentParty[i].skill, currentParty[i].wit, currentParty[i].mind, currentParty[i].speed, currentParty[i].luck, true);
            
            // Spawn the visuals
            // Right now it sets to a set position based on instatiate order, this will eventually need to be updated to place on the selected grid position
            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].allyBattleVisualPrefab, partyGridTransform[i].position,
                Quaternion.identity).GetComponent<BattleVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentParty[i].maxHealth, currentParty[i].currentHealth, currentParty[i].maxSpirit,
                currentParty[i].currentSpirit, currentParty[i].maxArmor);
            // Assign said visuals to the battle entity
            tempEntity.battleVisuals = tempBattleVisuals;
            
            // Add the allied combatant to the all combatants and party combatant lists
            allCombatants.Add(tempEntity);
            partyCombatants.Add(tempEntity);
        }
    }
    
    private void CreateEnemyEntities()
    {
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        for (int i = 0; i < currentEnemies.Count; i++) {
            BattleEntities tempEntity = new BattleEntities();
            
            tempEntity.SetEntityValue(currentEnemies[i].enemyName, currentEnemies[i].level, currentEnemies[i].maxHealth, currentEnemies[i].currentHealth,
                currentEnemies[i].maxSpirit, currentEnemies[i].currentSpirit, currentEnemies[i].maxDefense, currentEnemies[i].maxArmor, currentEnemies[i].power,
                currentEnemies[i].skill, currentEnemies[i].wit, currentEnemies[i].mind, currentEnemies[i].speed, currentEnemies[i].luck, false);
            
            // Spawn the visuals
            // Right now it sets to a set position based on instatiate order, this will eventually need to be updated to place on the selected grid position
            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].enemyVisualPrefab, enemyGridTransform[i].position,
                Quaternion.identity).GetComponent<BattleVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentEnemies[i].maxHealth, currentEnemies[i].currentHealth, currentEnemies[i].maxSpirit,
                currentEnemies[i].currentSpirit, currentEnemies[i].maxArmor);
            // Assign said visuals to the battle entity
            tempEntity.battleVisuals = tempBattleVisuals;
            
            // Add the allied combatant to the all combatants and party combatant lists
            allCombatants.Add(tempEntity);
            enemyCombatants.Add(tempEntity);
        }
    }
    
    private void RemoveDeadCombatants()
    {
        for (int i = 0; allCombatants.Count > i; i++) {
            if (allCombatants[i].currentHealth <= 0) {
                allCombatants.RemoveAt(i);
            }
        }
    }
    
    private void SaveHealth()
    {
        for (int i = 0; i < partyCombatants.Count; i++) {
            partyManager.SaveHealth(i, partyCombatants[i].currentHealth);
        }
            
    }
    
    // TODO Method to make battle UI visible and functional needed here
    
    // TODO Select enemy function needed here
    
    // TODO Random targeting needed here
    
    // TODO Damage function needed here
    
    // TODO Heal function needed here
}

[System.Serializable]
public class BattleEntities
{
    public enum Action
    {
        Attack,
        Defend,
    }

    public Action battleAction;

    public string name;
    public bool isPlayer;
    public int level;
    public int initiative;

    public int maxHealth;
    public int currentHealth;
    public int maxSpirit;
    public int currentSpirit;
    public int maxDefense;
    public int currentDefense;
    public int maxArmor;
    public int currentArmor;

    public int power;
    public int skill;
    public int wit;
    public int mind;
    public int speed;
    public int luck;
    
    public BattleVisuals battleVisuals;

    public int target;

    public void SetEntityValue(string entityName, int entityLevel, int entityMaxHealth, int entityCurrentHealth,
        int entityMaxSpirit, int entityCurrentSpirit, int entityMaxDefense, int entityMaxArmor, int entityPower,
        int entitySkill, int entityWit, int entityMind, int entitySpeed, int entityLuck, bool entityIsPlayer)
    {
        name = entityName;
        isPlayer = entityIsPlayer;
        level = entityLevel;

        maxHealth = entityMaxHealth;
        currentHealth = entityCurrentHealth;
        maxSpirit = entityMaxSpirit;
        currentSpirit = maxSpirit;
        maxDefense = entityMaxDefense;
        currentDefense = maxDefense;
        maxArmor = entityMaxArmor;
        currentArmor = maxArmor;

        power = entityPower;
        skill = entitySkill;
        wit = entityWit;
        mind = entityMind;
        speed = entitySpeed;
        luck = entityLuck;
    }

    public void SetTarget(int entityTarget)
    {
        target = entityTarget;
    }

    public void UpdateUI()
    {
        battleVisuals.ChangeHealth(currentHealth);
        battleVisuals.ChangeSpirit(currentSpirit);
        battleVisuals.ChangeArmor(currentArmor);
    }
}