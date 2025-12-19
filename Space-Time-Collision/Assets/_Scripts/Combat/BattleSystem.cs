using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleSystem : MonoBehaviour
{

    private enum BattleState
    {
        Start,
        Battle,
        Ordering,
        PlayerTurn,
        EnemyTurn,
        Targeting,
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
    
    [Header("Tokens")]
    [SerializeField] private List<BattleToken> allTokens = new List<BattleToken>();

    // Buff Tokens
    private BattleToken blockToken;
    private BattleToken blockPlusToken;
    private BattleToken boostToken;
    private BattleToken boostPlusToken;
    private BattleToken criticalToken;
    private BattleToken dodgeToken;
    private BattleToken dodgePlusToken;
    
    // Debuff Tokens
    private BattleToken blindToken;
    private BattleToken breakToken;
    private BattleToken vulnerableToken;
    
    [Header("UI")]
    [SerializeField] private GameObject combatStartUI;
    [SerializeField] private Animator combatStartUIAnimator;
    
    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private TokenManager tokenManager;
    
    private int currentPlayer;
    private bool abilitySelected;
    private bool wentBack;
    private bool targetSelected;
    private bool targetIsEnemy;
    
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
        tokenManager = FindFirstObjectByType<TokenManager>();
    }
    
    
    void Start()
    {
        CreatePartyEntities();
        CreateEnemyEntities();
        InitializeBattleTokens();
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
            print("Start Routine called but the battle system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator BattleRoutine()
    {
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
            print("Order Routine called but the battle system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator PlayerTurnRoutine(int characterIndex)
    {
        if (state == BattleState.PlayerTurn) {
            currentPlayer = characterIndex;
            if (!wentBack) {
                allCombatants[currentPlayer].initiative -= TURN_START_THRESHOLD;
                preparedCombatants.RemoveAt(preparedCombatants.IndexOf(allCombatants[currentPlayer]));
            }
            
            allCombatants[currentPlayer].battleVisuals.SetMyTurnAnimation(true);
            abilitySelected = false;
            UpdateAbilityBar(currentPlayer);
            ShowAbilitySelectMenu(currentPlayer);
            yield return new WaitUntil(() => abilitySelected);
            state = BattleState.Targeting;
            StartCoroutine(TargetRoutine());
        }  else {
            print("Player Routine called but the battle system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator TargetRoutine()
    {
        if (state == BattleState.Targeting) {
            
            BattleEntities activeCharacter = allCombatants[currentPlayer];
            BattleEntities targetCharacter;
            
            int tempTarget;
            SetAbilityValuesForDisplay();
            switch (allCombatants[currentPlayer].activeAbilityType) {
                case "Damage":
                    targetIsEnemy = true;
                    
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    
                    tempTarget = activeCharacter.target;
                    StopIndicatingTarget(enemyCombatants.IndexOf(allCombatants[tempTarget]));
                    
                    yield return StartCoroutine(DamageAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case "Heal":
                    targetIsEnemy = false;
                    
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    
                    tempTarget = activeCharacter.target;
                    StopIndicatingTarget(partyCombatants.IndexOf(allCombatants[tempTarget]));
                    
                    yield return StartCoroutine(HealAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case "Buff":
                    targetIsEnemy = false;
                    
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    
                    tempTarget = activeCharacter.target;
                    StopIndicatingTarget(partyCombatants.IndexOf(allCombatants[tempTarget]));
                    
                    yield return StartCoroutine(BuffAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case "Debuff":
                    targetIsEnemy = true;
                    
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    
                    tempTarget = activeCharacter.target;
                    StopIndicatingTarget(enemyCombatants.IndexOf(allCombatants[tempTarget]));
                    
                    yield return StartCoroutine(DebuffAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                default:
                    print("Unsupported ability type of " + allCombatants[currentPlayer].activeAbilityType + " supplied.");
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    break;
            }

            // Reduce Cooldowns of all unused abilities by one
            for (int i = 0; i < allCombatants[currentPlayer].abilityCooldowns.Count; i++) {
                if (activeCharacter.abilityCooldowns[i] > 0) {
                    activeCharacter.abilityCooldowns[i] -= 1;
                }
            }
            // Start the cooldown of the used ability
            activeCharacter.abilityCooldowns[activeCharacter.activeAbility] += activeCharacter.myAbilities[activeCharacter.activeAbility].cooldown;
            
            activeCharacter.battleVisuals.SetMyTurnAnimation(false);
            wentBack = false;
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
            
        } else {
            print("Target Routine called but the battle system is in the " + state + " state.");
            yield break;
        }
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
            print("Enemy Routine called but the battle system is in the " + state + " state.");
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

    private void InitializeBattleTokens()
    {
        List<Token> currentTokens = new List<Token>();
        currentTokens = tokenManager.GetAllTokens();

        for (int i = 0; i < currentTokens.Count; i++) {
            BattleToken battleToken = new BattleToken();
            
            battleToken.SetTokenValues(currentTokens[i].TokenName, currentTokens[i].TokenIcon, currentTokens[i].TokenType, currentTokens[i].TokenValue,
                currentTokens[i].TokenCap, currentTokens[i].TokenDescription);
            
            allTokens.Add(battleToken);
        }
        
        // Set buff tokens
        blockToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Block");
        blockPlusToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Block+");
        boostToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Boost");
        boostPlusToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Boost+");
        criticalToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Critical");
        dodgeToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Dodge");
        dodgePlusToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Dodge+");
        
        // Set debuff tokens
        blindToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Blind");
        breakToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Break");
        vulnerableToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Vulnerable");
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
    
    public void ShowAbilitySelectMenu(int characterIndex)
    {
        // Set whose turn it is
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilitySelectUIVisibility(true);
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(true);
    }
    
    public void ShowTargetMenu(int characterIndex)
    {
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilitySelectUIVisibility(false);
        SetTargetButtons(characterIndex);
        allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(true);
        allCombatants[characterIndex].combatMenuVisuals.ChangeBackButtonVisibility(true);
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
                case "SelfDmg":
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

    public string SetAbilityDescription(int abilityIndex)
    {
        BattleEntities currentPlayerEntity = allCombatants[currentPlayer];
        return currentPlayerEntity.myAbilities[abilityIndex].description;
    }

    public void PreviewResourceValue(int abilityIndex)
    {
        BattleEntities currentPlayerEntity = allCombatants[currentPlayer];
        int tempInt;
        switch (currentPlayerEntity.myAbilities[abilityIndex].costResource.ToString()) {
            case "Null":
                break;
            case "Spirit":
                tempInt = currentPlayerEntity.currentSpirit - currentPlayerEntity.myAbilities[abilityIndex].costAmount;
                currentPlayerEntity.combatMenuVisuals.ChangeSpirit(tempInt);
                break;
            case "Health":
                tempInt = currentPlayerEntity.currentHealth - currentPlayerEntity.myAbilities[abilityIndex].costAmount;
                currentPlayerEntity.battleVisuals.ChangeHealth(tempInt);
                break;
            case "Defense":
                tempInt = currentPlayerEntity.currentDefense - currentPlayerEntity.myAbilities[abilityIndex].costAmount;
                currentPlayerEntity.battleVisuals.ChangeDefense(tempInt);
                break;
            case "SelfDmg":
                break;
            case "Armor":
                tempInt = currentPlayerEntity.currentArmor - currentPlayerEntity.myAbilities[abilityIndex].costAmount;
                currentPlayerEntity.battleVisuals.ChangeArmor(tempInt);
                break;
            case "Special":
                print("Special resource type was called but isn't programmed in yet.");
                break;
            default: 
                print("Invalid resource of " +  currentPlayerEntity.myAbilities[abilityIndex].costResource + " supplied");
                break;
        }
    }

    public void EndResourcePreview(int abilityIndex)
    {
        BattleEntities currentPlayerEntity = allCombatants[currentPlayer];
        switch (currentPlayerEntity.myAbilities[abilityIndex].costResource.ToString()) {
            case "Null":
                break;
            case "Spirit":
                currentPlayerEntity.combatMenuVisuals.ChangeSpirit(currentPlayerEntity.currentSpirit);
                break;
            case "Health":
                currentPlayerEntity.battleVisuals.ChangeHealth(currentPlayerEntity.currentHealth);
                break;
            case "Defense":
                currentPlayerEntity.battleVisuals.ChangeDefense(currentPlayerEntity.currentDefense);
                break;
            case "SelfDmg":
                currentPlayerEntity.battleVisuals.ChangeHealth(currentPlayerEntity.currentHealth);
                currentPlayerEntity.battleVisuals.ChangeDefense(currentPlayerEntity.currentDefense);
                break;
            case "Armor":
                currentPlayerEntity.battleVisuals.ChangeArmor(currentPlayerEntity.currentArmor);
                break;
            case "Special":
                print("Special resource type was called but isn't programmed in yet.");
                break;
            default: 
                print("Invalid resource of " +  currentPlayerEntity.myAbilities[abilityIndex].costResource + " supplied");
                break;
        }
    }
    
    public void SetCurrentAbilityType(int abilityIndex)
    {
        BattleEntities currentPlayerEntity = allCombatants[currentPlayer];
        currentPlayerEntity.activeAbilityType = currentPlayerEntity.myAbilities[abilityIndex].abilityType.ToString();
        currentPlayerEntity.activeAbility = abilityIndex;
        abilitySelected = true;
    }

    public void BackToAbilities()
    {
        if (state == BattleState.Targeting) {
            abilitySelected = false;
            wentBack = true;
            allCombatants[currentPlayer].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
            allCombatants[currentPlayer].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
            allCombatants[currentPlayer].combatMenuVisuals.ChangeBackButtonVisibility(false);
            
            state = BattleState.PlayerTurn;
            StartCoroutine(PlayerTurnRoutine(currentPlayer));
        } else {
            print("Back button selected, but the user is in the " + state + " state.");
        }
    }
    
    // TODO Enemy selection functions needs to reference and defer to grid range
    
    private void SetTargetButtons(int characterIndex)
    {
        // Disable all buttons
        for (int i = 0; i < allCombatants[characterIndex].targetButtons.Length; i++) {
            allCombatants[characterIndex].targetButtons[i].SetActive(false); 
        }
        if (targetIsEnemy) {
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

    private void SetTargetValuesForDisplay(int hoveredTarget)
    {
        BattleEntities activeEntity = allCombatants[currentPlayer];
        BattleEntities targetEntity;
        // Check if target is ally or enemy
        if (targetIsEnemy) {
            int target = allCombatants.IndexOf(enemyCombatants[hoveredTarget]);
            targetEntity = allCombatants[target];
        } else {
            int target = allCombatants.IndexOf(partyCombatants[hoveredTarget]);
            targetEntity = allCombatants[target];
        }
        
        int abilityModifier = 0;
        bool singleValue = false;
        bool isCrit = false;
        float acc = 100f;
        int min = 0;
        int max = 0;
        int crit = 0;
        
        SetAbilityValues(activeEntity, ref abilityModifier, ref isCrit, ref acc, ref min,
            ref max, ref crit);
        
        RunAbilityAgainstSelfTokens(activeEntity, ref abilityModifier, ref singleValue, ref acc, ref min, ref max, ref crit);
        RunAbilityAgainstTargetTokens(targetEntity, ref singleValue, ref acc, ref min, ref max, ref crit);
        
        bool isDamage;
        if (activeEntity.activeAbilityType == "Damage") {
            isDamage = true;
            min -= targetEntity.currentArmor;
            max -= targetEntity.currentArmor;
        } else if (activeEntity.activeAbilityType == "Heal") {
            isDamage = false;
        } else {
            isDamage = true;
        }
        
        activeEntity.combatMenuVisuals.SetAbilityValues(acc, min, max, crit, isDamage, singleValue);
    }

    private void SetAbilityValuesForDisplay()
    {
        BattleEntities activeEntity = allCombatants[currentPlayer];
        
        int abilityModifier = 0;
        bool singleValue = false;
        bool isCrit = false;
        float acc = 100f;
        int min = 0;
        int max = 0;
        int crit = 0;
        
        SetAbilityValues(activeEntity, ref abilityModifier, ref isCrit, ref acc, ref min,
            ref max, ref crit);
        
        RunAbilityAgainstSelfTokens(activeEntity, ref abilityModifier, ref singleValue, ref acc, ref min, ref max, ref crit);
        
        bool isDamage;
        if (activeEntity.activeAbilityType == "Damage") {
            isDamage = true;
        } else if (activeEntity.activeAbilityType == "Heal") {
            isDamage = false;
        } else {
            isDamage = true;
        }
        
        activeEntity.combatMenuVisuals.SetAbilityValues(acc, min, max, crit, isDamage, singleValue);
    }

    public void IndicateTarget(int hoveredTarget)
    {
        // Check if target is ally or enemy
        if (targetIsEnemy) {
            int target = allCombatants.IndexOf(enemyCombatants[hoveredTarget]);
            allCombatants[target].battleVisuals.TargetEnemyActive();
        } else {
            int target = allCombatants.IndexOf(partyCombatants[hoveredTarget]);
            allCombatants[target].battleVisuals.TargetAllyActive();
        }
        SetTargetValuesForDisplay(hoveredTarget);
    }
    
    public void StopIndicatingTarget(int hoveredTarget)
    {
        // Check if target is ally or enemy
        if (targetIsEnemy) {
            int target = allCombatants.IndexOf(enemyCombatants[hoveredTarget]);
            allCombatants[target].battleVisuals.TargetInactive();
        } else {
            int target = allCombatants.IndexOf(partyCombatants[hoveredTarget]);
            allCombatants[target].battleVisuals.TargetInactive();
        }
        SetAbilityValuesForDisplay();
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

    private int GetAbilityModifier(int activeAbilityIndex)
    {
        string abilityKey = allCombatants[currentPlayer].myAbilities[activeAbilityIndex].keyStat.ToString();
        int abilityKeyMod = allCombatants[currentPlayer].myAbilities[activeAbilityIndex].statModifier;
        int abilityModifier;
        
        switch (abilityKey)
        {
            case "Power":
                abilityModifier = allCombatants[currentPlayer].power * abilityKeyMod;
                break;
            case "Skill":
                abilityModifier = allCombatants[currentPlayer].skill * abilityKeyMod;
                break;
            case "Wit":
                abilityModifier = allCombatants[currentPlayer].wit * abilityKeyMod;
                break;
            case "Mind":
                abilityModifier = allCombatants[currentPlayer].mind * abilityKeyMod;
                break;
            case "Speed":
                abilityModifier = allCombatants[currentPlayer].speed * abilityKeyMod;
                break;
            case "Luck":
                abilityModifier = allCombatants[currentPlayer].luck * abilityKeyMod;
                break;
            default:
                abilityModifier = 0;
                print("Invalid damage key of " +  abilityKey + " supplied");
                break;
        }
        return abilityModifier;
    }

    private void SetAbilityValues(BattleEntities activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        abilityModifier = GetAbilityModifier(allCombatants[currentPlayer].activeAbility);
        
        isCrit = false;
        min = activeEntity.myAbilities[activeEntity.activeAbility].dmgMin + abilityModifier;
        max = activeEntity.myAbilities[activeEntity.activeAbility].dmgMax + abilityModifier;
        crit = activeEntity.myAbilities[activeEntity.activeAbility].critChance;
    }

    private void RunAbilityAgainstSelfTokens(BattleEntities activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            if (activeEntity.activeTokens.Contains(boostPlusToken)) {
                min = (int)(min * (1 + boostPlusToken.tokenValue));
                max = (int)(max * (1 + boostPlusToken.tokenValue));
            } else if (activeEntity.activeTokens.Contains(boostToken)) {
                min = (int)(min * (1 + boostToken.tokenValue));
                max = (int)(max * (1 + boostToken.tokenValue));
            } else if (activeEntity.activeTokens.Contains(breakToken)) {
                min = (int)(min * (1 - breakToken.tokenValue));
                max = (int)(max * (1 - breakToken.tokenValue));
            }
            // Check for Critical tokens
            if (activeEntity.activeTokens.Contains(criticalToken)) {
                crit = 100;
                max = (int)(max * CRIT_DAMAGE_MODIFIER);
                min = max;
                isCrit = true;
            }
            // Check for Blind tokens
            if (activeEntity.activeTokens.Contains(blindToken)) {
                acc *= (1 - blindToken.tokenValue);
            }
        }
    }
    
    private void RunHealAgainstSelfTokens(BattleEntities activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            if (activeEntity.activeTokens.Contains(boostPlusToken)) {
                min = (int)(min * (1 + boostPlusToken.tokenValue));
                max = (int)(max * (1 + boostPlusToken.tokenValue));
            } else if (activeEntity.activeTokens.Contains(boostToken)) {
                min = (int)(min * (1 + boostToken.tokenValue));
                max = (int)(max * (1 + boostToken.tokenValue));
            } else if (activeEntity.activeTokens.Contains(breakToken)) {
                min = (int)(min * (1 - breakToken.tokenValue));
                max = (int)(max * (1 - breakToken.tokenValue));
            }
            // Check for Critical tokens
            if (activeEntity.activeTokens.Contains(criticalToken)) {
                crit = 100;
                max = (int)(max * CRIT_DAMAGE_MODIFIER);
                min = max;
                isCrit = true;
            }
            // Check for Blind tokens
            if (activeEntity.activeTokens.Contains(blindToken)) {
                acc *= (1 - blindToken.tokenValue);
            }
        }
    }
    
    private void RunAbilityAgainstTargetTokens(BattleEntities targetEntity, ref bool isCrit, ref float acc,
        ref int min, ref int max, ref int crit)
    {
        // Check for Block or Vulnerable tokens
        if (targetEntity.activeTokens.Contains(blockPlusToken)) {
            min = (int)(min * (1 - blockPlusToken.tokenValue));
            max = (int)(max * (1 - blockPlusToken.tokenValue));
        } else if (targetEntity.activeTokens.Contains(blockToken)) {
            min = (int)(min * (1 - blockToken.tokenValue));
            max = (int)(max * (1 - blockToken.tokenValue));
        } else if (targetEntity.activeTokens.Contains(vulnerableToken)) {
            min = (int)(min * (1 + vulnerableToken.tokenValue));
            max = (int)(max * (1 + vulnerableToken.tokenValue));
        }
        // Check for Dodge tokens
        if (targetEntity.activeTokens.Contains(dodgePlusToken)) {
            acc = (int)(acc * (1 - dodgePlusToken.tokenValue));
        } else if (targetEntity.activeTokens.Contains(dodgeToken)) {
            acc = (int)(acc * (1 - dodgeToken.tokenValue));
        }
    }

    private BattleToken CreateBattleToken(BattleToken originalToken)
    {
        BattleToken battleToken = new BattleToken();
        battleToken = originalToken;
        return originalToken;
    }
    
    private void AddTokens(BattleEntities entity, string tokenName, int tokenCount)
    {
        foreach (var t in allTokens) {
            if (tokenName == t.tokenName) {
                int tIndex;
                if (entity.activeTokens.Contains(t)) {
                    tIndex = entity.activeTokens.IndexOf(t);
                    entity.activeTokens[tIndex].tokenCount += tokenCount;
                } else {
                    BattleToken tempToken = CreateBattleToken(t);
                    entity.activeTokens.Add(tempToken);
                    tIndex = entity.activeTokens.IndexOf(tempToken);
                    entity.activeTokens[tIndex].tokenCount = tokenCount;
                }
                
                if (entity.activeTokens[tIndex].tokenCount > entity.activeTokens[tIndex].tokenCap) {
                    entity.activeTokens[tIndex].tokenCount =  entity.activeTokens[tIndex].tokenCap;
                }
            }
        }
    }

    private void RemoveSelfDamageTokens(BattleEntities activeEntity)
    {
        int tokenPosition;
        
        // Check for Boost or Break tokens
        if (activeEntity.activeTokens.Contains(boostPlusToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(boostPlusToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (activeEntity.activeTokens.Contains(boostToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(boostToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (activeEntity.activeTokens.Contains(breakToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(breakToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        
        // Check for Critical tokens
        if (activeEntity.activeTokens.Contains(criticalToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(criticalToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        
        // Check for Blind tokens
        if (activeEntity.activeTokens.Contains(blindToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(blindToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }

        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }
    
    private void RemoveSelfHealTokens(BattleEntities activeEntity)
    {
        int tokenPosition;
        
        // Check for Boost or Break tokens
        if (activeEntity.activeTokens.Contains(boostPlusToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(boostPlusToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (activeEntity.activeTokens.Contains(boostToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(boostToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (activeEntity.activeTokens.Contains(breakToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(breakToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        
        // Check for Critical tokens
        if (activeEntity.activeTokens.Contains(criticalToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(criticalToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        
        // Check for Blind tokens
        if (activeEntity.activeTokens.Contains(blindToken)) {
            tokenPosition = activeEntity.activeTokens.IndexOf(blindToken);
            if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }

    private void RemoveTargetDamageTokens(BattleEntities targetEntity)
    {
        int tokenPosition;
        
        // Check for Block or Vulnerable tokens
        if (targetEntity.activeTokens.Contains(blockPlusToken)) {
            tokenPosition = targetEntity.activeTokens.IndexOf(blockPlusToken);
            if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (targetEntity.activeTokens.Contains(blockToken)) {
            tokenPosition = targetEntity.activeTokens.IndexOf(blockToken);
            if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (targetEntity.activeTokens.Contains(vulnerableToken)) {
            tokenPosition = targetEntity.activeTokens.IndexOf(vulnerableToken);
            if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        // Check for Dodge tokens
        if (targetEntity.activeTokens.Contains(dodgePlusToken)) {
            tokenPosition = targetEntity.activeTokens.IndexOf(dodgePlusToken);
            if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenPosition);
            }
        } else if (targetEntity.activeTokens.Contains(dodgeToken)) {
            tokenPosition = targetEntity.activeTokens.IndexOf(dodgeToken);
            if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenPosition);
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }
    
    private IEnumerator DamageAction(BattleEntities attacker, BattleEntities attackTarget, int activeAbilityIndex)
    {
        Ability activeAbility = attacker.myAbilities[activeAbilityIndex];
        int damage;
        
        int damageModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;

        SetAbilityValues(attacker, ref damageModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);

        RunAbilityAgainstSelfTokens(attacker, ref damageModifier, ref isCrit, ref acc, ref minDamageRange, 
            ref maxDamageRange, ref critChance);
        RunAbilityAgainstTargetTokens(attackTarget, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        // TODO check for Isolation/Ward tokens
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(attacker, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i]);
        }
        attacker.battleVisuals.UpdateTokens(attacker.activeTokens);
        
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            attackTarget.battleVisuals.AbilityMisses();
            RemoveSelfDamageTokens(attacker);
            // Check for Dodge tokens
            if (attackTarget.activeTokens.Contains(dodgePlusToken)) {
                attackTarget.activeTokens.Remove(dodgePlusToken);
            } else if (attackTarget.activeTokens.Contains(dodgeToken)) {
                attackTarget.activeTokens.Remove(dodgeToken);
            }
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }
        int critRoll = Random.Range(1, 101);
        if (critRoll < critChance && !attacker.activeTokens.Contains(criticalToken)) {
            damage = (int)(maxDamageRange * CRIT_DAMAGE_MODIFIER) - attackTarget.currentArmor;
        } else {
            damage = Random.Range(minDamageRange, maxDamageRange + 1) - attackTarget.currentArmor;
        }
        
        RemoveSelfDamageTokens(attacker);
        RemoveTargetDamageTokens(attackTarget);
        
        // Apply target tokens
        /// TODO Check for Ward tokens
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(attackTarget, activeAbility.targetTokensApplied[i].ToString(), activeAbility.targetTokenCountApplied[i]);
        }
        attackTarget.battleVisuals.UpdateTokens(attackTarget.activeTokens);
        
        // Play combat animations
        attacker.battleVisuals.PlayAttackAnimation(); // play the attack animation
        attackTarget.battleVisuals.PlayHitAnimation(damage, isCrit); // target plays on hit animation
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
        Ability activeAbility = healer.myAbilities[activeAbilityIndex];
        int restore;
        
        int restoreModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        
        SetAbilityValues(healer, ref restoreModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);

        RunHealAgainstSelfTokens(healer, ref restoreModifier, ref isCrit, ref acc, ref minDamageRange, 
            ref maxDamageRange, ref critChance);
        // TODO Add a method that checks only for tokens on the target that affect healing
        
        // Apply self tokens
        // TODO check for Isolation/Ward tokens
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(healer, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i]);
        }
        healer.battleVisuals.UpdateTokens(healer.activeTokens);
        
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            healTarget.battleVisuals.AbilityMisses();
            RemoveSelfHealTokens(healer);
            // TODO Check for Healblock tokens
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }

        int critRoll = Random.Range(1, 101);
        if (critRoll < critChance && !healer.activeTokens.Contains(criticalToken)) {
            restore = (int)(maxDamageRange * CRIT_DAMAGE_MODIFIER);
        } else {
            restore = Random.Range(minDamageRange, maxDamageRange + 1);
        }
        
        // Apply target tokens
        // TODO Check for Isloation/Ward tokens
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(healTarget, activeAbility.targetTokensApplied[i].ToString(), activeAbility.targetTokenCountApplied[i]);
        }
        healTarget.battleVisuals.UpdateTokens(healTarget.activeTokens);
        
        //healer.battleVisuals.PlayAttackAnimation(); // play the attack animation
        healTarget.currentDefense += restore; // restore HP
        healTarget.battleVisuals.PlayHealAnimation(restore, isCrit); // target plays on hit animation
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
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

    private IEnumerator BuffAction(BattleEntities buffer, BattleEntities buffTarget, int activeAbilityIndex)
    {
        Ability activeAbility = buffer.myAbilities[activeAbilityIndex];
        float acc = 100;

        // TODO Check for Isolation/Ward tokens
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(buffer, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i]);
        }
        buffer.battleVisuals.UpdateTokens(buffer.activeTokens);
        
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            buffTarget.battleVisuals.AbilityMisses();
            
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }
        
        // TODO Check for Isolation tokens
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(buffTarget, activeAbility.targetTokensApplied[i].ToString(), activeAbility.targetTokenCountApplied[i]);
        }
        buffTarget.battleVisuals.UpdateTokens(buffTarget.activeTokens);
        
        yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
    }
    
    private IEnumerator DebuffAction(BattleEntities debuffer, BattleEntities debuffTarget, int activeAbilityIndex)
    {
        Ability activeAbility = debuffer.myAbilities[activeAbilityIndex];
        float acc = 100;

        // TODO Check for Isolation/Ward tokens
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(debuffer, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i]);
        }
        debuffer.battleVisuals.UpdateTokens(debuffer.activeTokens);
        
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            debuffTarget.battleVisuals.AbilityMisses();
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        } 
        
        // TODO Check for Ward tokens
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(debuffTarget, activeAbility.targetTokensApplied[i].ToString(), activeAbility.targetTokenCountApplied[i]);
        }
        debuffTarget.battleVisuals.UpdateTokens(debuffTarget.activeTokens);
        
        yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
    }

    private IEnumerator ConsumeResources(int activeAbilityIndex)
    {
        print("Consume resources was called for " + allCombatants[currentPlayer].myAbilities[activeAbilityIndex].name);
        // Reduce the user's resource 
        string resourceConsumed = allCombatants[currentPlayer].myAbilities[activeAbilityIndex].costResource.ToString();
        int resourceCost = allCombatants[currentPlayer].myAbilities[activeAbilityIndex].costAmount;
        print("Resource consumed is: " +  resourceConsumed + "Cost amount is: " + resourceCost);

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
                print(allCombatants[currentPlayer] + "'s new armor value is " +  allCombatants[currentPlayer].currentArmor);
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
}

[Serializable]
public class BattleEntities
{
    public enum Action
    {
        Attack,
        Defend,
    }

    public Action battleAction;

    public string myName;
    public string activeAbilityType;
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
    public List<BattleToken> activeTokens;

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
        
        activeTokens = new List<BattleToken>();
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

[Serializable]
public class BattleToken
{
    public string tokenName;
    public Sprite tokenIcon;
    public TokenInfo.TokenType tokenType;
    public float tokenValue;
    public int tokenCap;
    public int tokenCount;

    public string tokenDescription;

    public void SetTokenValues(string storedName, Sprite storedIcon, TokenInfo.TokenType storedType, float storedValue, int storedCap, string storedDescription)
    {
        tokenName  = storedName;
        tokenIcon = storedIcon;
        tokenType = storedType;
        tokenValue = storedValue;
        tokenCap = storedCap;
        tokenCount = 0;
        
        tokenDescription = storedDescription;
    }
}