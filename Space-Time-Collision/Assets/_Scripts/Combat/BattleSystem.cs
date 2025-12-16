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
    private bool abilitySelected;
    private bool targetSelected;
    private bool targetIsEnemy;
    private string currentAbilityType;
    
    private const float COMBAT_BEGIN_DELAY = 1f;
    private const float TURN_ACTION_DELAY = 1.5f;
    private const float DEATH_DELAY = 3f;
    private const int TURN_START_THRESHOLD = 200;
    private const int BASE_INITIATIVE_GAIN = 20;
    private const int MAX_INITIATIVE_START = 100;
    private const string MAP_SCENE = "BaseScene";
    private const string BASE_SCENE = "BaseScene";
    
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
                allCombatants[i].initiative = Random.Range(100, MAX_INITIATIVE_START + 1);
                print(allCombatants[i].name + "'s starting initiative is " +  allCombatants[i].initiative);
            }

            yield return new WaitForSeconds(COMBAT_BEGIN_DELAY);
            combatStartUIAnimator.SetTrigger(BATTLE_START_END);
            yield return new WaitForSeconds(0.2f);
            Destroy(combatStartUI);
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
            // if no party members remain -> battle is lost
            if (partyCombatants.Count <= 0) {
                state = BattleState.Lost;
                yield return new WaitForSeconds(TURN_ACTION_DELAY);  // wait a few seconds
                print("GAME OVER! \n Go to game over screen.");
                SceneManager.LoadScene(BASE_SCENE);
            }
            // if no enemies remain -> battle is won
            if (enemyCombatants.Count <= 0) {
                state = BattleState.Won;
                yield return new WaitForSeconds(TURN_ACTION_DELAY);  // wait a few seconds
                print("Your party prevailed!");
                SceneManager.LoadScene(MAP_SCENE);
            }
            // Remove any dead combatants from the combat
            RemoveDeadCombatants();
            
            while (preparedCombatants.Count <= 0) {
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
            }

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
            currentPlayer = characterIndex;
            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(true);
            allCombatants[characterIndex].initiative -= TURN_START_THRESHOLD;
            preparedCombatants.RemoveAt(preparedCombatants.IndexOf(allCombatants[characterIndex]));
            
            if (preparedCombatants.Count > 0) {
                for (int i = 0; i < preparedCombatants.Count; i++) {
                    print(preparedCombatants[i].name + " is still prepared and their initiative is " +
                          preparedCombatants[i].initiative);
                }
            } else {
                print("No other characters are prepared.");
            }
            
            print("Player turn has begun. " + allCombatants[characterIndex].name + " Is the active character.");

            abilitySelected = false;
            ShowAbilitySelectMenu(characterIndex);
            yield return new WaitUntil(() => abilitySelected);
            
            switch (currentAbilityType) {
                case "Damage":
                    targetIsEnemy = true;
                    ShowTargetMenu(characterIndex, targetIsEnemy);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    
                    allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    yield return StartCoroutine(DamageAction(allCombatants[characterIndex],
                        allCombatants[allCombatants[characterIndex].target]));
                    break;
                case "Heal":
                    targetIsEnemy = false;
                    ShowTargetMenu(characterIndex, targetIsEnemy);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    
                    allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    yield return StartCoroutine(HealAction(allCombatants[characterIndex],
                        allCombatants[allCombatants[characterIndex].target]));
                    break;
                default:
                    print("Unsupported ability type of " + currentAbilityType + " supplied.");
                    allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    break;
            }

            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(false);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
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
            preparedCombatants.RemoveAt(preparedCombatants.IndexOf(allCombatants[characterIndex]));
            
            print("Enemy turn has begun. " + allCombatants[characterIndex].name + " Is the active enemy.");
            if (preparedCombatants.Count > 0) {
                for (int i = 0; i < preparedCombatants.Count; i++) {
                    print(preparedCombatants[i].name + " is still prepared and their initiative is " +
                          preparedCombatants[i].initiative);
                } 
            } else {
                print("No other characters are prepared.");
            }
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
            
        
            allCombatants[characterIndex].target = GetRandomPartyMember();
            yield return StartCoroutine(DamageAction(allCombatants[characterIndex], 
                allCombatants[allCombatants[characterIndex].target]));

            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(false);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
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
            // TODO Right now it sets to a set position based on instatiate order, this will eventually need to be updated to place on the selected grid position
            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].allyBattleVisualPrefab, partyGridTransform[i].position,
                Quaternion.identity).GetComponent<BattleVisuals>();
            CombatMenuVisuals tempCombatMenuVisuals = Instantiate(currentParty[i].allyMapVisualPrefab, Vector2.zero,
                Quaternion.identity).GetComponent<CombatMenuVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentParty[i].maxHealth, currentParty[i].currentHealth, currentParty[i].maxDefense, currentParty[i].maxArmor);
            tempCombatMenuVisuals.SetMenuStartingValues(currentParty[i].maxSpirit, currentParty[i].currentSpirit);
            // Assign said visuals to the battle entity
            tempEntity.battleVisuals = tempBattleVisuals;
            tempEntity.combatMenuVisuals = tempCombatMenuVisuals;
            tempEntity.targetButtons = tempEntity.combatMenuVisuals.GetTargetButtons();
            
            // Assign abilities to character TODO Make this also update visuals
            tempEntity.myAbilities = partyManager.GetActiveAbilities(i);
            
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
            tempBattleVisuals.SetStartingValues(currentEnemies[i].maxHealth, currentEnemies[i].currentHealth, currentEnemies[i].maxDefense, currentEnemies[i].maxArmor);
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
    
    public void ShowAbilitySelectMenu(int characterIndex)
    {
        // Set whose turn it is
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilitySelectUIVisibility(true);
        //allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(true);
    }
    
    public void ShowTargetMenu(int characterIndex, bool targetEnemy)
    {
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(true);
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilitySelectUIVisibility(false);
        SetTargetButtons(characterIndex, targetEnemy);
        allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(true);
    }
    
    // TODO Enemy selection functions needs to reference and defer to grid range
    
    private void SetTargetButtons(int characterIndex, bool targetEnemy)
    {
        // Disable all buttons
        for (int i = 0; i < allCombatants[characterIndex].targetButtons.Length; i++) {
            allCombatants[characterIndex].targetButtons[i].SetActive(false); 
        }
        if (targetEnemy) {
            // Enable buttons for each present enemy
            for (int i = 0; i < enemyCombatants.Count; i++) {
                allCombatants[characterIndex].targetButtons[i].SetActive(true);
                // Change the button's text
                allCombatants[characterIndex].targetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyCombatants[i].name;
                
            }
        } else {
            // Enable buttons for each present ally
            for (int i = 0; i < partyCombatants.Count; i++) {
                allCombatants[characterIndex].targetButtons[i].SetActive(true);
                // Change the button's text
                allCombatants[characterIndex].targetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = partyCombatants[i].name;
            }
        }
        
    }
    
    public void SelectTarget(int currentTarget)
    {
        // Set the current member's target
        BattleEntities currentPlayerEntity = allCombatants[currentPlayer];
        
        if (targetIsEnemy) {
            currentPlayerEntity.SetTarget(allCombatants.IndexOf(enemyCombatants[currentTarget]));
        } else {
            currentPlayerEntity.SetTarget(allCombatants.IndexOf(partyCombatants[currentTarget]));
        }
        
        targetSelected = true;
    }

    public void SetCurrentAbilityType(int abilityIndex)
    {
        BattleEntities currentPlayerEntity = allCombatants[currentPlayer];
        currentAbilityType = currentPlayerEntity.myAbilities[abilityIndex].abilityType.ToString();
        abilitySelected = true;
    }
    
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
    
    // Unsure if this function will be needed as characters take their turn when it happens, not upfront like in FF
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
    
    private IEnumerator DamageAction(BattleEntities attacker, BattleEntities attackTarget)
    {
        int damage = attacker.power; // get damage (can use a formula) TODO make this read and use ability damage ranges and modifiers
        attacker.battleVisuals.PlayAttackAnimation(); // play the attack animation
        attackTarget.battleVisuals.PlayHitAnimation(); // target plays on hit animation
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        // Deal the damage to defense, or if the target has none, to HP
        if (attackTarget.currentDefense > 0) {
            // If the damage dealt is greater than the target's defense, deal the rest to their HP
            if (damage > attackTarget.currentDefense) {
                int overflowDamage = damage - attackTarget.currentDefense;
                attackTarget.currentDefense = 0;
                attackTarget.currentHealth -= overflowDamage;
            }
            attackTarget.currentDefense -= damage;
        } else {
            attackTarget.currentHealth -= damage;
        }

        switch (attackTarget.isPlayer) {
            // Update the UI
            case true:
                attackTarget.UpdatePlayerUI();
                break;
            case false:
                attackTarget.UpdateEnemyUI();
                break;
        }
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
    
    private IEnumerator HealAction(BattleEntities healer, BattleEntities healTarget)
    {
        int restore = healer.mind; // get damage (can use a formula)
        //healer.battleVisuals.PlayAttackAnimation(); // play the attack animation
        healTarget.currentDefense += restore; // deal the damage
        healTarget.battleVisuals.PlayHealAnimation(); // target plays on hit animation
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        // Update the UI
        if (healTarget.isPlayer) {
            healTarget.UpdatePlayerUI();
        } else if (!healTarget.isPlayer) {
            healTarget.UpdateEnemyUI();
        }
        
        print(string.Format("{0} heals {1} restoring {2} defense.", healer.name, healTarget.name, restore));
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
    public int target;
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
    public CombatMenuVisuals combatMenuVisuals;

    public GameObject[] targetButtons;
    public List<Ability> myAbilities;

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

    public void UpdatePlayerUI()
    {
        battleVisuals.ChangeHealth(currentHealth);
        battleVisuals.ChangeDefense(currentDefense);
        battleVisuals.ChangeArmor(currentArmor);
        combatMenuVisuals.ChangeSpirit(currentSpirit);
    }

    public void UpdateEnemyUI()
    {
        battleVisuals.ChangeHealth(currentHealth);
        battleVisuals.ChangeDefense(currentDefense);
        battleVisuals.ChangeArmor(currentArmor);
    }
    
    
}