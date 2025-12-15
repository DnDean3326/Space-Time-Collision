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
    private const float TURN_ACTION_DELAY = 1.5f;
    private const float DEATH_DELAY = 3f;
    private const int TURN_START_THRESHOLD = 200;
    private const int BASE_INITIATIVE_GAIN = 20;
    private const int MAX_INITIATIVE_START = 100;
    private const string MAP_SCENE = "BaseScene";
    
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
                allCombatants[i].initiative = Random.Range(0, MAX_INITIATIVE_START + 1);
                print(allCombatants[i].name + "'s starting initiative is " +  allCombatants[i].initiative);
            }

            yield return new WaitForSeconds(COMBAT_BEGIN_DELAY);
            combatStartUIAnimator.SetTrigger(BATTLE_START_END);
            yield return new WaitForSeconds(0.2f);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        } else {
            print("Start Routine called but the battle system is not in the start state.");
            yield break;
        }
    }

    private IEnumerator BattleRoutine()
    {
        if (state == BattleState.Battle) {
            RemoveDeadCombatants();
            print("Battle Routine called");
            print("Battle Routine called");
            do {
                for (int i = 0; i < allCombatants.Count; i++) {
                    if (state == BattleState.Battle) {
                        allCombatants[i].initiative += BASE_INITIATIVE_GAIN + allCombatants[i].speed;
                        print(allCombatants[i].name + "'s new initiative is " + allCombatants[i].initiative);
                        if (allCombatants[i].initiative >= TURN_START_THRESHOLD) {
                            preparedCombatants.Add(allCombatants[i]);
                            print(allCombatants[i].name + " has been prepared.");
                        }
                    }
                }

                yield return new WaitForSeconds(0.5f);
            } while (preparedCombatants.Count <= 0);

            state = BattleState.Ordering;
            StartCoroutine(OrderRoutine());
        } else {
            print("Battle Routine called but the battle system is not in the Battle state.");
            yield break;
        }
    }

    private IEnumerator OrderRoutine()
    {
        if (state == BattleState.Ordering) {
            // if no party members remain -> battle is lost
            if (partyCombatants.Count <= 0) {
                state = BattleState.Lost;
                yield return new WaitForSeconds(TURN_ACTION_DELAY);  // wait a few seconds
                print("GAME OVER! \n Go to game over screen.");
            }
            // if no enemies remain -> battle is won
            if (enemyCombatants.Count <= 0) {
                state = BattleState.Won;
                yield return new WaitForSeconds(TURN_ACTION_DELAY);  // wait a few seconds
                SceneManager.LoadScene(MAP_SCENE);
            }
            
            RemoveDeadCombatants();
            if (preparedCombatants.Count > 0) {
                // Sorts prepared combatants by initiative from highest to lowest
                preparedCombatants.Sort((bi1, bi2) => -bi1.initiative.CompareTo(bi2.initiative));
                
                int characterIndex = allCombatants.IndexOf(preparedCombatants[0]);
                if (preparedCombatants[0].isPlayer) {
                    state = BattleState.PlayerTurn;
                    StartCoroutine(PlayerTurnRoutine(characterIndex));
                } else if (!preparedCombatants[0].isPlayer) {
                    state = BattleState.EnemyTurn;
                    StartCoroutine(EnemyTurnRoutine(characterIndex));
                }
            } else {
                state = BattleState.Battle;
                StartCoroutine(BattleRoutine());
            }
                
        } else {
            print("Start Order called but the battle system is not in the Order state.");
            yield break;
        }
    }

    private IEnumerator PlayerTurnRoutine(int characterIndex)
    {
        if (state == BattleState.PlayerTurn) {
            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(true);
            allCombatants[characterIndex].initiative -= TURN_START_THRESHOLD;
            preparedCombatants.RemoveAt(0);
            print("Player turn has begun. " + allCombatants[characterIndex].name + " Is the active character.");
            
            
            
            /*allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(false);
            state = BattleState.Ordering;
            StartCoroutine(OrderRoutine());*/
        }  else {
            print("Player Turn Routine called but the battle system is not in the Player Turn state.");
            yield break;
        }

        yield break;
    }

    private IEnumerator EnemyTurnRoutine(int characterIndex)
    {
        if (state == BattleState.EnemyTurn) {
            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(true);
            allCombatants[characterIndex].initiative -= TURN_START_THRESHOLD;
            preparedCombatants.RemoveAt(0);
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
            print("Enemy turn has begun. " + allCombatants[characterIndex].name + " Is the active enemy.");
        
            allCombatants[characterIndex].target = GetRandomPartyMember();
            yield return StartCoroutine(DamageAction(allCombatants[characterIndex], 
                allCombatants[allCombatants[characterIndex].target]));

            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(false);
            state = BattleState.Ordering;
            StartCoroutine(OrderRoutine());
        } else {
            print("Enemy Turn Routine called but the battle system is not in the Enemy Turn state.");
            yield break;
        }
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
    
    private void SaveResources()
    {
        for (int i = 0; i < partyCombatants.Count; i++) {
            partyManager.SaveHealth(i, partyCombatants[i].currentHealth);
            partyManager.SaveSpirit(i, partyCombatants[i].currentSpirit);
        }
            
    }
    
    // TODO Method to make battle UI visible and functional needed here
    
    // TODO Select enemy function needed here
    
    // TODO Replace the random targeting methods here with ones that accommodates the grid
    
    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>(); // create a temporary list of type int (index)
        // find all the party members -> add them to list
        for (int i = 0; i < allCombatants.Count; i++) {
            if (allCombatants[i].isPlayer) {
                partyMembers.Add(i);
            }
        }
        return partyMembers[Random.Range(0, partyMembers.Count)]; // return a random party member
    }
    
    private int GetRandomEnemy()
    {
        List<int> enemies = new List<int>(); // create a temporary list of type int (index)
        // find all the party members -> add them to list
        for (int i = 0; i < allCombatants.Count; i++) {
            if (!allCombatants[i].isPlayer) {
                enemies.Add(i);
            }
        }
        return enemies[Random.Range(0, enemies.Count)]; // return a random party member
    }
    
    // TODO Damage function needed here
    
    private IEnumerator DamageAction(BattleEntities attacker, BattleEntities attackTarget)
    {
        int damage = attacker.power; // get damage (can use a formula)
        attacker.battleVisuals.PlayAttackAnimation(); // play the attack animation
        attackTarget.battleVisuals.PlayHitAnimation(); // target plays on hit animation
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        attackTarget.currentHealth -= damage; // deal the damage
        attackTarget.UpdateUI(); // update the UI
        print(string.Format("{0} attacks {1} dealing {2} damage.", attacker.name, attackTarget.name, damage));
        
        if (attackTarget.currentHealth <= 0) {
            yield return new WaitForSeconds(DEATH_DELAY);
            
            // For some reason in the tutorial allies and enemies are never removed from allCombatants on death. Unsure if that's intentional...
            if (attackTarget.isPlayer) {
                partyCombatants.Remove(attackTarget);
                
            } else if (!attackTarget.isPlayer) {
                enemyCombatants.Remove(attackTarget);
                
            }
            
        }
        
        SaveResources();
    }
    
    // TODO Heal function needed here
    private void HealAction(BattleEntities healer, BattleEntities healTarget)
    {
        int restore = healer.power; // get damage (can use a formula)
        //healer.battleVisuals.PlayAttackAnimation(); // play the attack animation
        healTarget.currentHealth += restore; // deal the damage
        healTarget.battleVisuals.PlayHitAnimation(); // target plays on hit animation
        healTarget.UpdateUI(); // update the UI
        print(string.Format("{0} heals {1} restoring {2} toughness.", healer.name, healTarget.name, restore));
        SaveResources();
    }
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