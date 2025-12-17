using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    //private string currentAbilityType;
    
    private const float COMBAT_BEGIN_DELAY = 1f;
    private const float TURN_ACTION_DELAY = 1.5f;
    private const float DEATH_DELAY = 3f;
    private const float CRIT_DAMAGE_MODIFIER = 1.5f;
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
            yield return StartCoroutine(SetAbilityBar());
            
            for (int i = 0; i < allCombatants.Count; i++) {
                allCombatants[i].initiative = Random.Range(1, MAX_INITIATIVE_START + 1);
            }
            
            yield return new WaitForSeconds(COMBAT_BEGIN_DELAY);
            combatStartUIAnimator.SetTrigger(BATTLE_START_END);
            yield return new WaitForSeconds(0.2f);
            Destroy(combatStartUI);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        } else {
            print("Battle Routine called but the battle system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator BattleRoutine()
    {
        print("Battle state was called");
        
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
        yield return StartCoroutine(FixResources());
        RemoveDeadCombatants();
        
        if (state == BattleState.Battle) {
            
            
            while (preparedCombatants.Count <= 0) {
                for (int i = 0; i < allCombatants.Count; i++) {
                    if (state == BattleState.Battle) {
                        allCombatants[i].initiative += BASE_INITIATIVE_GAIN + allCombatants[i].speed;
                        if (allCombatants[i].initiative >= TURN_START_THRESHOLD) {
                            preparedCombatants.Add(allCombatants[i]);
                        }
                    }
                }
                
            }

            state = BattleState.Ordering;
            StartCoroutine(OrderRoutine());
        } else {
            print("Battle Routine called but the battle system is in the " + state + " state.");
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
            print("Battle Routine called but the battle system is in the " + state + " state.");
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
            print("Player turn has begun. " + allCombatants[characterIndex].myName + " Is the active character.");

            abilitySelected = false;
            UpdateAbilityBar(characterIndex);
            ShowAbilitySelectMenu(characterIndex);
            yield return new WaitUntil(() => abilitySelected);
            
            switch (allCombatants[characterIndex].currentAbilityType) {
                case "Damage":
                    targetIsEnemy = true;
                    ShowTargetMenu(characterIndex, targetIsEnemy);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    
                    allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    yield return StartCoroutine(DamageAction(allCombatants[characterIndex],
                        allCombatants[allCombatants[characterIndex].target], allCombatants[characterIndex].activeAbility));
                    break;
                case "Heal":
                    targetIsEnemy = false;
                    ShowTargetMenu(characterIndex, targetIsEnemy);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    
                    allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    yield return StartCoroutine(HealAction(allCombatants[characterIndex],
                        allCombatants[allCombatants[characterIndex].target], allCombatants[characterIndex].activeAbility));
                    break;
                default:
                    print("Unsupported ability type of " + allCombatants[characterIndex].currentAbilityType + " supplied.");
                    allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    break;
            }

            // Reduce Cooldowns of all unused abilities by one
            for (int i = 0; i < allCombatants[characterIndex].abilityCooldowns.Count; i++) {
                if (allCombatants[characterIndex].abilityCooldowns[i] > 0) {
                    allCombatants[characterIndex].abilityCooldowns[i] -= 1;
                }
            }
            // Start the cooldown of the used ability
            allCombatants[characterIndex].abilityCooldowns[allCombatants[characterIndex].activeAbility] +=
                allCombatants[characterIndex].myAbilities[allCombatants[characterIndex].activeAbility].cooldown;
            
            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(false);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        }  else {
            print("Battle Routine called but the battle system is in the " + state + " state.");
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
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
            
        
            allCombatants[characterIndex].target = GetRandomPartyMember();
            allCombatants[characterIndex].activeAbility = 0; // TODO replace this ability with enemy brain function
            yield return StartCoroutine(DamageAction(allCombatants[characterIndex], 
                allCombatants[allCombatants[characterIndex].target], allCombatants[characterIndex].activeAbility));

            allCombatants[characterIndex].battleVisuals.SetMyTurnAnimation(false);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        } else {
            print("Battle Routine called but the battle system is in the " + state + " state.");
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
            tempEntity.abilityButtons = tempEntity.combatMenuVisuals.GetAbilityButtons();
            
            // Assign abilities to character TODO Make this also update visuals
            tempEntity.myAbilities = partyManager.GetActiveAbilities(i);
            tempEntity.abilityCooldowns = new List<int>();
            for (int j = 0; j < tempEntity.myAbilities.Count; j++) {
                tempEntity.abilityCooldowns.Add(0);
            }
            
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
            // TODO have enemies spawned at a specific grid position
            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].enemyVisualPrefab, enemyGridTransform[i].position,
                Quaternion.identity).GetComponent<BattleVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentEnemies[i].maxHealth, currentEnemies[i].currentHealth, currentEnemies[i].maxDefense, currentEnemies[i].maxArmor);
            // Assign said visuals to the battle entity
            tempEntity.battleVisuals = tempBattleVisuals;
            // Give the enemy their abilities
            tempEntity.myAbilities = enemyManager.GetAbilities(i);
            
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
    
    private IEnumerator SetAbilityBar()
    {
        for (int i = 0; i < partyCombatants.Count; i++) {
            for (int j = 0; j < partyCombatants[i].myAbilities.Count; j++) {
                /*partyCombatants[i].abilityButtons[j].GetComponent<SpriteRenderer>().sprite =
                    partyCombatants[i].myAbilities[j].abilityIcon;*/
                partyCombatants[i].abilityButtons[j].GetComponentInChildren<TextMeshProUGUI>().text =
                    partyCombatants[i].myAbilities[j].abilityName;
                
                
            }
        }

        yield break;
    }

    private void UpdateAbilityBar(int characterIndex)
    {
        for (int i = 0; i < partyCombatants[characterIndex].myAbilities.Count; i++) {
            if (partyCombatants[characterIndex].abilityCooldowns[i] > 0) {
                partyCombatants[characterIndex].abilityButtons[i].GetComponent<Image>().color = new Color(40,40,40);
                partyCombatants[characterIndex].abilityButtons[i].GetComponent<Button>().interactable = false;
            }
            switch (partyCombatants[characterIndex].myAbilities[i].costResource.ToString()) {
                case "Null":
                    break;
                case "Spirit":
                    if (partyCombatants[characterIndex].currentSpirit <
                        partyCombatants[characterIndex].myAbilities[i].costAmount) {
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Image>().color = new Color(40,40,40);
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Button>().interactable = false;
                    }
                    break;
                case "Health":
                    if (partyCombatants[characterIndex].currentSpirit <
                        partyCombatants[characterIndex].myAbilities[i].costAmount) {
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Image>().color = new Color(40,40,40);
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Button>().interactable = false;
                    }
                    break;
                case "Defense":
                    if (partyCombatants[characterIndex].currentSpirit <
                        partyCombatants[characterIndex].myAbilities[i].costAmount) {
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Image>().color = new Color(40,40,40);
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Button>().interactable = false;
                    }
                    break;
                case "Selfdmg":
                    break;
                case "Armor":
                    if (partyCombatants[characterIndex].currentSpirit <
                        partyCombatants[characterIndex].myAbilities[i].costAmount) {
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Image>().color = new Color(40,40,40);
                        partyCombatants[characterIndex].abilityButtons[i].GetComponent<Button>().interactable = false;
                    }
                    break;
                case "Special":
                    print("Special resource type was called but isn't programmed in yet.");
                    break;
                default:
                    print("Invalid resource of " +  partyCombatants[characterIndex].myAbilities[i].costResource + " supplied");
                    break;
            }
            
        }
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
                allCombatants[characterIndex].targetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyCombatants[i].myName;
                
            }
        } else {
            // Enable buttons for each present ally
            for (int i = 0; i < partyCombatants.Count; i++) {
                allCombatants[characterIndex].targetButtons[i].SetActive(true);
                // Change the button's text
                allCombatants[characterIndex].targetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = partyCombatants[i].myName;
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
        allCombatants[currentPlayer].currentAbilityType = currentPlayerEntity.myAbilities[abilityIndex].abilityType.ToString();
        allCombatants[currentPlayer].activeAbility = abilityIndex;
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
    
    private IEnumerator DamageAction(BattleEntities attacker, BattleEntities attackTarget, int activeAbilityIndex)
    {
        // Calculate damage dealt
        int damage;
        
        string damageKey = attacker.myAbilities[activeAbilityIndex].keyStat.ToString();
        int damageKeyMod = attacker.myAbilities[activeAbilityIndex].statModifier;
        int damageModifier;
        
        switch (damageKey)
        {
            case "Power":
                damageModifier = attacker.power * damageKeyMod;
                break;
            case "Skill":
                damageModifier = attacker.skill * damageKeyMod;
                break;
            case "Wit":
                damageModifier = attacker.wit * damageKeyMod;
                break;
            case "Mind":
                damageModifier = attacker.mind * damageKeyMod;
                break;
            case "Speed":
                damageModifier = attacker.speed * damageKeyMod;
                break;
            case "Luck":
                damageModifier = attacker.luck * damageKeyMod;
                break;
            default:
                print("Invalid damage key of " +  damageKey + " supplied");
                yield break;
        }
        
        int minDamageRange = attacker.myAbilities[activeAbilityIndex].dmgMin;
        int maxDamageRange = attacker.myAbilities[activeAbilityIndex].dmgMax;
        int critChance =  attacker.myAbilities[activeAbilityIndex].critChance;

        if (Random.Range(1, 101) > critChance) {
            damage = Random.Range(minDamageRange, maxDamageRange + 1) + damageModifier;
        } else {
            print(attacker.myName + " scored a critical hit!");
            damage = (int)((maxDamageRange + damageModifier) * CRIT_DAMAGE_MODIFIER);
        }
        
        // Play combat animations
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
            } else {
                attackTarget.currentDefense -= damage;
            }
        } else {
            attackTarget.currentHealth -= damage;
        }
        print(string.Format("{0} attacks {1} dealing {2} damage.", attacker.myName, attackTarget.myName, damage));
        
        yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        SaveResources();
        if (attackTarget.isPlayer) {
            attackTarget.UpdatePlayerUI();
            attacker.UpdateEnemyUI();
        } else if (!attackTarget.isPlayer) {
            attackTarget.UpdateEnemyUI();
            attacker.UpdatePlayerUI();
        }
        if (attackTarget.currentHealth <= 0) {
            yield return new WaitForSeconds(DEATH_DELAY);
            
            if (attackTarget.isPlayer) {
                partyCombatants.Remove(attackTarget);
                
            } else if (!attackTarget.isPlayer) {
                enemyCombatants.Remove(attackTarget);
                
            }
        }
    }
    
    private IEnumerator HealAction(BattleEntities healer, BattleEntities healTarget, int activeAbilityIndex)
    {
        // Calculate damage dealt
        int restore;
        
        string restoreKey = healer.myAbilities[activeAbilityIndex].keyStat.ToString();
        int restoreKeyMod = healer.myAbilities[activeAbilityIndex].statModifier;
        int restoreModifier;
        
        switch (restoreKey)
        {
            case "Power":
                restoreModifier = healer.power * restoreKeyMod;
                break;
            case "Skill":
                restoreModifier = healer.skill * restoreKeyMod;
                break;
            case "Wit":
                restoreModifier = healer.wit * restoreKeyMod;
                break;
            case "Mind":
                restoreModifier = healer.mind * restoreKeyMod;
                break;
            case "Speed":
                restoreModifier = healer.speed * restoreKeyMod;
                break;
            case "Luck":
                restoreModifier = healer.luck * restoreKeyMod;
                break;
            default:
                print("Invalid restore key of " +  restoreKey + " supplied");
                yield break;
        }
        
        int minDamageRange = healer.myAbilities[activeAbilityIndex].dmgMin;
        int maxDamageRange = healer.myAbilities[activeAbilityIndex].dmgMax;
        int critChance =  healer.myAbilities[activeAbilityIndex].critChance;

        if (Random.Range(1, 101) > critChance) {
            restore = Random.Range(minDamageRange, maxDamageRange + 1) + restoreModifier;
        } else {
            print(healer.myName + " scored a critical heal!");
            restore = (int)((maxDamageRange + restoreModifier) * CRIT_DAMAGE_MODIFIER);
        }
        
        //healer.battleVisuals.PlayAttackAnimation(); // play the attack animation
        healTarget.currentDefense += restore; // restore HP
        healTarget.battleVisuals.PlayHealAnimation(); // target plays on hit animation
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
        print(string.Format("{0} heals {1} restoring {2} defense.", healer.myName, healTarget.myName, restore));
        yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        SaveResources();
        // Update the UI
        if (healTarget.isPlayer) {
            healTarget.UpdatePlayerUI();
            healer.UpdatePlayerUI();
        } else if (!healTarget.isPlayer) {
            healTarget.UpdateEnemyUI();
            healer.UpdateEnemyUI();
        }
    }

    private IEnumerator ConsumeResources(int activeAbilityIndex)
    {
        // Reduce the user's resource 
        string resourceConsumed = allCombatants[currentPlayer].myAbilities[activeAbilityIndex].costResource.ToString();
        int resourceCost = allCombatants[currentPlayer].myAbilities[activeAbilityIndex].costAmount;

        switch (resourceConsumed)
        {
            case "Null":
                break;
            case "Spirit":
                allCombatants[currentPlayer].currentSpirit -= resourceCost;
                break;
            case "Health":
                allCombatants[currentPlayer].currentHealth -= resourceCost;
                break;
            case "Defense":
                allCombatants[currentPlayer].currentHealth -= resourceCost;
                break;
            case "SelfDmg":
                if (allCombatants[currentPlayer].currentDefense > 0) {
                    if (resourceCost > allCombatants[currentPlayer].currentDefense) {
                        int overflowDamage = resourceCost - allCombatants[currentPlayer].currentDefense;
                        allCombatants[currentPlayer].currentDefense = 0;
                        allCombatants[currentPlayer].currentHealth -= overflowDamage;
                    } else {
                        allCombatants[currentPlayer].currentDefense -= resourceCost;
                    }
                } else {
                    allCombatants[currentPlayer].currentHealth -= resourceCost;
                }
                break;
            case "Armor":
                allCombatants[currentPlayer].currentArmor -= resourceCost;
                break;
            // TODO Implement special resource consumption
            case "Special":
                //attacker.currentHealth -= resourceCost;
                print("Special resource called for consumption.");
                break;
            default:
                print("Invalid resource " + resourceConsumed + " called for consumption.");
                break;
        }
        yield break;
    }

    private IEnumerator FixResources()
    {
        for (int i = 0; i < allCombatants.Count; i++) {
            if (allCombatants[i].currentHealth < 0) {
                allCombatants[i].currentHealth = 0;
            } else if (allCombatants[i].currentHealth > allCombatants[i].maxHealth) {
                allCombatants[i].currentHealth = allCombatants[i].maxHealth;
            }
            if (allCombatants[i].currentSpirit < 0) {
                allCombatants[i].currentSpirit = 0;
            } else if (allCombatants[i].currentSpirit > allCombatants[i].maxSpirit) {
                allCombatants[i].currentSpirit = allCombatants[i].maxSpirit;
            }
            if (allCombatants[i].currentDefense < 0) {
                allCombatants[i].currentDefense = 0;
            } else if (allCombatants[i].currentDefense > allCombatants[i].maxDefense) {
                allCombatants[i].currentDefense = allCombatants[i].maxDefense;
            }
            if (allCombatants[i].currentArmor < 0) {
                allCombatants[i].currentArmor = 0;
            } else if (allCombatants[i].currentArmor > allCombatants[i].maxArmor) {
                allCombatants[i].currentArmor = allCombatants[i].maxArmor;
            }
            
        }
        yield break;
    }
    /*
    public int maxHealth;
    public int currentHealth;
    public int maxSpirit;
    public int currentSpirit;
    public int maxDefense;
    public int currentDefense;
    public int maxArmor;
    public int currentArmor;
    */
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

    public string myName;
    public string currentAbilityType;
    public bool isPlayer;
    public int target;
    public int activeAbility;
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

    public GameObject[] abilityButtons;
    public GameObject[] targetButtons;
    public List<Ability> myAbilities;
    public List<int> abilityCooldowns;

    public void SetEntityValue(string entityName, int entityLevel, int entityMaxHealth, int entityCurrentHealth,
        int entityMaxSpirit, int entityCurrentSpirit, int entityMaxDefense, int entityMaxArmor, int entityPower,
        int entitySkill, int entityWit, int entityMind, int entitySpeed, int entityLuck, bool entityIsPlayer)
    {
        myName = entityName;
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