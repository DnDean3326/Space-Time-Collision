using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
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
    [SerializeField] private List<BattleEntities> turnOrder = new  List<BattleEntities>();
    
    [Header("Tokens")]
    [SerializeField] private List<BattleToken> allTokens = new List<BattleToken>();

    [Header("Battle Start UI")]
    [SerializeField]
    private GameObject battleStartUI;

    // Buff Tokens
    private BattleToken blockToken;
    private BattleToken blockPlusToken;
    private BattleToken boostToken;
    private BattleToken boostPlusToken;
    private BattleToken criticalToken;
    private BattleToken drainToken; // Not implemented
    private BattleToken dodgeToken;
    private BattleToken dodgePlusToken;
    private BattleToken goadToken; // Not implemented
    private BattleToken guardToken;
    private BattleToken hasteToken;
    private BattleToken pierceToken;
    private BattleToken precisionToken;
    private BattleToken quickToken;
    private BattleToken riposteToken; // Not implemented
    private BattleToken rushToken; // Not implemented
    private BattleToken stealthToken; // Not implemented
    private BattleToken tauntToken; // Not implemented
    private BattleToken wardToken;
    
    // Debuff Tokens
    private BattleToken antiHealToken;
    private BattleToken blindToken;
    private BattleToken breakToken;
    private BattleToken delayToken;
    private BattleToken goadedToken; // Not implemented
    private BattleToken isolationToken;
    private BattleToken linkToken; // Not implemented
    private BattleToken offGuardToken;
    private BattleToken restrictToken; // Not implemented
    private BattleToken slowToken;
    private BattleToken staggerToken; // Not implemented
    private BattleToken stunToken; // Not implemented
    private BattleToken vulnerableToken;
    
    // Character Specific Tokens
    private BattleToken ascensionToken; // Not implemented
    private BattleToken viceToken; // Not implemented
    
    // Ailment Counters
    private BattleToken burnCounter; // Not implemented
    
    [Header("UI")]
    [SerializeField] private GameObject combatStartUI;
    [SerializeField] private Animator combatStartUIAnimator;
    
    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private TokenManager tokenManager;
    private TurnOrderDisplay turnOrderDisplay;
    
    private int currentPlayer;
    private bool abilitySelected;
    private bool wentBack;
    private bool targetSelected;
    private bool targetIsEnemy;
    
    private const float COMBAT_BEGIN_DELAY = 2.25f;
    private const float TURN_ACTION_DELAY = 1.5f;
    private const float DEATH_DELAY = 3f;
    private const float CRIT_DAMAGE_MODIFIER = 1.5f;
    private const int TURN_START_THRESHOLD = 200;
    private const int BASE_ACTION_GAIN = 20;
    private const int MAX_ACTION_START = 100;
    private const int MAX_INDIVIDUAL_DISPLAY = 5;
    private const string MAP_SCENE = "BaseScene";
    private const string BASE_SCENE = "BaseScene";
    
    public const float SkillCritMod = 1.5f;
    public const float WitPierceMod = 2;
    public const float PowerStunMod = 2;
    public const float MindDebuffMod = 2;
    public const float LuckCritMod = 1;
    
    // Character Specific Conts
    private const float BUNE_BASE_ACTOUT = 10f;
    private const float BUNE_MAX_ACTOUT = 70f;
    private const float BUNE_ACTOUT_INCREASE = 30f;
    
    // Animator Constants
    private const string BATTLE_START_END = "EndTrigger";

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();
        tokenManager = FindFirstObjectByType<TokenManager>();
        turnOrderDisplay = FindFirstObjectByType<TurnOrderDisplay>();
    }
    
    
    void Start()
    {
        CreatePartyEntities();
        CreateEnemyEntities();
        InitializeBattleTokens();
        StartCoroutine(StartRoutine());
        battleStartUI.SetActive(true);
    }
    
    // Battle state routines

    private IEnumerator StartRoutine()
    {
        if (state == BattleState.Start) {
            yield return StartCoroutine(SetAbilityBar());
            
            for (int i = 0; i < allCombatants.Count; i++) {
                allCombatants[i].actionPoints = Random.Range(1, MAX_ACTION_START + 1);
            }
            GetTurnOrder();
            
            yield return new WaitForSeconds(COMBAT_BEGIN_DELAY);
            Destroy(battleStartUI);
            state = BattleState.Battle;
            yield return StartCoroutine(BattleRoutine());
        } else {
            print("Start Routine called but the battle system is in the " + state + " state.");
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
        GetTurnOrder();
        
        if (state == BattleState.Battle) {
            while (preparedCombatants.Count <= 0) {
                for (int i = 0; i < allCombatants.Count; i++) {
                    if (state == BattleState.Battle) {
                        if (allCombatants[i].activeTokens.All(t => t.tokenName != "Haste")) {
                            allCombatants[i].actionPoints += (int)((BASE_ACTION_GAIN + allCombatants[i].speed) * 
                                                                   (1 + hasteToken.tokenValue));
                        } else if (allCombatants[i].activeTokens.All(t => t.tokenName != "Slow")) {
                            allCombatants[i].actionPoints += (int)((BASE_ACTION_GAIN + allCombatants[i].speed) * 
                                                                   (1 - slowToken.tokenValue));
                        } else {
                            allCombatants[i].actionPoints += BASE_ACTION_GAIN + allCombatants[i].speed;
                        }
                        if (allCombatants[i].actionPoints >= TURN_START_THRESHOLD) {
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
                
                List<int> indexesToRemove = new List<int>();
                
                foreach (BattleEntities t in preparedCombatants) {
                    if (t.activeTokens.Any(t => t.tokenName == "Stun") ||
                        t.activeTokens.Any(t => t.tokenName == "Quick") ||
                        t.activeTokens.Any(t => t.tokenName == "Delay")) {
                        TriggerTurnSpeedTokens(t);
                    }
                    if (t.actionPoints < TURN_START_THRESHOLD) {
                        indexesToRemove.Add(preparedCombatants.IndexOf(t));
                    }
                }
                foreach (int t in indexesToRemove) {
                    preparedCombatants.RemoveAt(t);
                }
                if (preparedCombatants.Count <= 0) {
                    state = BattleState.Battle;
                    StartCoroutine(BattleRoutine());
                    yield break;
                }
                
                // Sorts prepared combatants by initiative from highest to lowest
                preparedCombatants.Sort((bi1, bi2) => -bi1.actionPoints.CompareTo(bi2.actionPoints));
                
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
        }
    }

    private IEnumerator PlayerTurnRoutine(int characterIndex)
    {
        if (state == BattleState.PlayerTurn) {
            currentPlayer = characterIndex;
            if (!wentBack) {
                RemoveSelfTurnStartTokens(allCombatants[characterIndex]);
                allCombatants[currentPlayer].actionPoints -= TURN_START_THRESHOLD;
                preparedCombatants.RemoveAt(preparedCombatants.IndexOf(allCombatants[currentPlayer]));
                
                // Run character specific Methods
                switch (allCombatants[characterIndex].myName) {
                    case "Bune":
                        if (allCombatants[characterIndex].activeTokens.Any(t => t.tokenName != "Vice")) {
                            BuneViceActOut();
                        } else {
                            allCombatants[characterIndex].specialResourceFloat = BUNE_BASE_ACTOUT;
                        }
                        break;
                    default:
                        break;
                }
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
            activeCharacter.abilityCooldowns[activeCharacter.activeAbility] = activeCharacter.myAbilities[activeCharacter.activeAbility].cooldown;
            
            // Reset turn-related values
            activeCharacter.battleVisuals.SetMyTurnAnimation(false);
            wentBack = false;
            activeCharacter.gainedUniqueTokenLastTurn = false;
            activeCharacter.wasDamagedLastTurn = false;
            activeCharacter.damagedBy = 100;
            
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        } else {
            print("Target Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator EnemyTurnRoutine(int characterIndex)
    {
        if (state == BattleState.EnemyTurn) {
            BattleEntities activeEnemy = allCombatants[characterIndex];
            EnemyBrain myBrain = activeEnemy.enemyBrain;
            
            RemoveSelfTurnStartTokens(activeEnemy);
            activeEnemy.battleVisuals.SetMyTurnAnimation(true);
            activeEnemy.actionPoints -= TURN_START_THRESHOLD;
            preparedCombatants.RemoveAt(preparedCombatants.IndexOf(activeEnemy));
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);

            int priorityTotal = 0;
            for (int i = 0; i < myBrain.enemyAbilities.Length; i++) {
                if (activeEnemy.abilityCooldowns[i] <= 0) {
                    priorityTotal += myBrain.enemyAbilities[i].abilityPriority;
                }
            }

            int enemyDecision = Random.Range(0, priorityTotal + 1);
            int abilityThreshold = 0;
            int? abilityUsed = null;

            for (int i = 0; i < myBrain.enemyAbilities.Length; i++) {
                abilityThreshold += myBrain.enemyAbilities[i].abilityPriority;
                if (enemyDecision <= abilityThreshold) {
                    abilityUsed = i;
                    activeEnemy.activeAbility = (int)abilityUsed;
                    break;
                }
            }
            if (abilityUsed == null) {
                print("No ability selected");
                yield break;
            }

            bool targetingFoes = true;
            List<BattleEntities> targetList = new List<BattleEntities>();
            switch (activeEnemy.myAbilities[(int)abilityUsed].abilityType) {
                case Ability.AbilityType.Damage:
                case Ability.AbilityType.Debuff:
                    targetList = partyCombatants;
                    targetingFoes = true;
                    break;
                case  Ability.AbilityType.Heal:
                case  Ability.AbilityType.Buff:
                    targetList = enemyCombatants;
                    targetingFoes = false;
                    break;
            }

            BattleEntities abilityTarget = null;
            bool hasTaunt = false;

            if (targetingFoes) {
                foreach (BattleEntities entity in targetList.Where(entity => entity.activeTokens.Any(t => t.tokenName == "Taunt"))) {
                    abilityTarget = entity;
                    hasTaunt = true;
                    break;
                }
            }
            if (!hasTaunt) {
                if (Random.Range(0, 21) < myBrain.enemyAbilities[(int)abilityUsed].randomChance ||
                    myBrain.enemyAbilities[(int)abilityUsed].targetMethod == EnemyBrain.TargetMethod.Random) {
                    abilityTarget = targetList[Random.Range(0, targetList.Count)];
                } else {
                    bool targetLowest;
                    switch (myBrain.enemyAbilities[(int)abilityUsed].targetMethod) {
                        case EnemyBrain.TargetMethod.Lowest:
                            targetLowest = true;
                            break;
                        case EnemyBrain.TargetMethod.Highest:
                            targetLowest = false;
                            break;
                        default:
                            print("Default target method called.");
                            targetLowest = true;
                            break;
                    }

                    switch (myBrain.enemyAbilities[(int)abilityUsed].targetQualifier) {
                        case EnemyBrain.TargetQualifier.Null:
                            print("Invalid Qualifier of Null");
                            abilityTarget = targetList[Random.Range(0, targetList.Count)];
                            break;
                        case EnemyBrain.TargetQualifier.Health:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.currentHealth.CompareTo(bi2.currentHealth));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.currentHealth.CompareTo(bi2.currentHealth));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Defense:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.currentDefense.CompareTo(bi2.currentDefense));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.currentDefense.CompareTo(bi2.currentDefense));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Armor:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.currentArmor.CompareTo(bi2.currentArmor));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.currentArmor.CompareTo(bi2.currentArmor));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Spirit:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.currentSpirit.CompareTo(bi2.currentSpirit));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.currentSpirit.CompareTo(bi2.currentSpirit));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.ActionPoints:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.actionPoints.CompareTo(bi2.actionPoints));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.actionPoints.CompareTo(bi2.actionPoints));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Power:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.power.CompareTo(bi2.power));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.power.CompareTo(bi2.power));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Skill:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.skill.CompareTo(bi2.skill));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.skill.CompareTo(bi2.skill));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Wit:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.wit.CompareTo(bi2.wit));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.wit.CompareTo(bi2.wit));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Mind:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.mind.CompareTo(bi2.mind));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.mind.CompareTo(bi2.mind));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Speed:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.speed.CompareTo(bi2.speed));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.speed.CompareTo(bi2.speed));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Luck:
                            if (targetLowest) {
                                targetList.Sort((bi1, bi2) => bi1.luck.CompareTo(bi2.luck));
                            } else {
                                targetList.Sort((bi1, bi2) => -bi1.luck.CompareTo(bi2.luck));
                            }

                            abilityTarget = targetList[0];
                            break;
                        case EnemyBrain.TargetQualifier.Proximity:
                            print("Qualifier of Proximity not currently functional");
                            abilityTarget = targetList[Random.Range(0, targetList.Count)];
                            break;
                        default:
                            print("Qualifier of " + myBrain.enemyAbilities[(int)abilityUsed].targetQualifier +
                                  " supplied");
                            abilityTarget = targetList[Random.Range(0, targetList.Count)];
                            // The below is sort lowest to highest
                            turnOrder.Sort((bi1, bi2) => bi1.ticksToTurn.CompareTo(bi2.ticksToTurn));
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
            switch (activeEnemy.myAbilities[(int)abilityUsed].abilityType) {
                case Ability.AbilityType.Damage:
                    yield return StartCoroutine(DamageAction(activeEnemy, abilityTarget, (int)abilityUsed));
                    break;
                case Ability.AbilityType.Debuff:
                    yield return StartCoroutine(DebuffAction(activeEnemy, abilityTarget, (int)abilityUsed));
                    break;
                case  Ability.AbilityType.Heal:
                    yield return StartCoroutine(HealAction(activeEnemy, abilityTarget, (int)abilityUsed));
                    break;
                case  Ability.AbilityType.Buff:
                    yield return StartCoroutine(BuffAction(activeEnemy, abilityTarget, (int)abilityUsed));
                    break;
                default:
                    print("Invalid ability type of " + activeEnemy.myAbilities[(int)abilityUsed].abilityType +
                          " called.");
                    yield break;
            }
            
            // Reduce Cooldowns of all unused abilities by one
            activeEnemy.abilityCooldowns = new List<int>();
            for (int j = 0; j < activeEnemy.myAbilities.Count; j++) {
                activeEnemy.abilityCooldowns.Add(0);
            }
            // Start the cooldown of the used ability
            activeEnemy.abilityCooldowns[activeEnemy.activeAbility] = myBrain.enemyAbilities[(int)abilityUsed].ability.cooldown;
            
            activeEnemy.battleVisuals.SetMyTurnAnimation(false);
            activeEnemy.wasDamagedLastTurn = false;
            activeEnemy.damagedBy = 100;
            
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
            
            tempEntity.SetEntityValue(currentParty[i].memberName, currentParty[i].memberPortait, currentParty[i].level,
                currentParty[i].maxHealth, currentParty[i].currentHealth, currentParty[i].maxSpirit, currentParty[i].currentSpirit,
                currentParty[i].maxDefense, currentParty[i].maxArmor, currentParty[i].power, currentParty[i].skill,
                currentParty[i].wit, currentParty[i].mind, currentParty[i].speed, currentParty[i].luck, currentParty[i].stunResist, 
                currentParty[i].debuffResist, currentParty[i].ailmentResist, true);
            
            // Spawn the visuals
            // TODO Right now it sets to a set position based on instantiate order, this will eventually need to be updated to place on the selected grid position
            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].allyBattleVisualPrefab, partyGridTransform[i].position,
                Quaternion.identity).GetComponent<BattleVisuals>();
            CombatMenuVisuals tempCombatMenuVisuals = Instantiate(currentParty[i].allyMenuVisualPrefab, Vector2.zero,
                Quaternion.identity).GetComponent<CombatMenuVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentParty[i].maxHealth, currentParty[i].currentHealth, currentParty[i].maxDefense, currentParty[i].maxArmor);
            tempCombatMenuVisuals.SetMenuStartingValues(currentParty[i].maxSpirit, currentParty[i].currentSpirit);
            // Assign said visuals to the battle entity
            tempEntity.battleVisuals = tempBattleVisuals;
            tempEntity.combatMenuVisuals = tempCombatMenuVisuals;
            tempEntity.targetButtons = tempEntity.combatMenuVisuals.GetTargetButtons();
            tempEntity.targetPortraits = tempEntity.combatMenuVisuals.GetTargetPortraits();
            tempEntity.targetBorders = tempEntity.combatMenuVisuals.GetTargetBorders();
            tempEntity.abilityButtons = tempEntity.combatMenuVisuals.GetAbilityButtons();
            
            // Assign abilities to character TODO Make this also update visuals
            tempEntity.myAbilities = partyManager.GetActiveAbilities(i);
            tempEntity.abilityCooldowns = new List<int>();
            for (int j = 0; j < tempEntity.myAbilities.Count; j++) {
                tempEntity.abilityCooldowns.Add(0);
            }
            
            // Assign character specific values
            if (tempEntity.myName == "Bune") {
                tempEntity.specialResourceFloat = BUNE_BASE_ACTOUT;
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
            
            tempEntity.SetEntityValue(currentEnemies[i].enemyName, currentEnemies[i].enemyPortrait, currentEnemies[i].level,
                currentEnemies[i].maxHealth, currentEnemies[i].currentHealth, currentEnemies[i].maxSpirit, currentEnemies[i].currentSpirit,
                currentEnemies[i].maxDefense, currentEnemies[i].maxArmor, currentEnemies[i].power, currentEnemies[i].skill,
                currentEnemies[i].wit, currentEnemies[i].mind, currentEnemies[i].speed, currentEnemies[i].luck, currentEnemies[i].stunResist, 
                currentEnemies[i].debuffResist, currentEnemies[i].ailmentResist, false);

            tempEntity.SetEnemyBrain(currentEnemies[i].enemyBrain);
            
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
            tempEntity.abilityCooldowns = new List<int>();
            for (int j = 0; j < tempEntity.myAbilities.Count; j++) {
                tempEntity.abilityCooldowns.Add(0);
            }
            
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
            
            battleToken.SetTokenValues(currentTokens[i].tokenName, currentTokens[i].displayName, currentTokens[i].tokenIcon, currentTokens[i].tokenType, currentTokens[i].tokenValue,
                currentTokens[i].tokenCap, currentTokens[i].tokenInverses, currentTokens[i].tokenDescription);
            
            allTokens.Add(battleToken);
        }
        
        // Set buff tokens
        blockToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Block");
        blockPlusToken = allTokens.SingleOrDefault(obj => obj.tokenName == "BlockPlus");
        boostToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Boost");
        boostPlusToken = allTokens.SingleOrDefault(obj => obj.tokenName == "BoostPlus");
        criticalToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Critical");
        dodgeToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Dodge");
        dodgePlusToken = allTokens.SingleOrDefault(obj => obj.tokenName == "DodgePlus");
        drainToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Drain");
        goadToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Goad");
        guardToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Guard");
        hasteToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Haste");
        pierceToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Pierce");
        precisionToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Precision");
        quickToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Quick");
        riposteToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Riposte");
        rushToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Rush");
        stealthToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Stealth");
        tauntToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Taunt");
        wardToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Ward");
        
        // Set debuff tokens
        antiHealToken = allTokens.SingleOrDefault(obj => obj.tokenName == "AntiHeal");
        blindToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Blind");
        breakToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Break");
        delayToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Delay");
        goadedToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Goaded");
        isolationToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Isolation");
        linkToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Link");
        offGuardToken = allTokens.SingleOrDefault(obj => obj.tokenName == "OffGuard");
        restrictToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Restrict");
        slowToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Slow");
        staggerToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Stagger");
        stunToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Stun");
        vulnerableToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Vulnerable");
        
        // Set character specific tokens
        ascensionToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Ascension");
        viceToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Vice");
        
        // Set ailment counters
        burnCounter = allTokens.SingleOrDefault(obj => obj.tokenName == "Burn");
    }

    private void GetTurnOrder()
    {
        turnOrder.Clear();
        for (int i = 0; i < MAX_INDIVIDUAL_DISPLAY; i++){
            foreach (BattleEntities t in allCombatants) {
                BattleEntities tempEntity = new BattleEntities();
                tempEntity.SetEntityTurnDisplayValues(t.myName, t.myPortrait, t.isPlayer, t.speed, t.actionPoints, t.activeTokens);
                tempEntity.actionPoints -= (200 * i);
                turnOrder.Add(tempEntity);
            }
        }

        foreach (BattleEntities entity in turnOrder) {
            float actionPointGain = 0f;
            int speedTokenIndex;
            if (entity.activeTokens.Any(t => t.tokenName == "Haste")) {
                speedTokenIndex = entity.activeTokens.FindIndex(t => t.tokenName == "Haste");
                if (entity.activeTokens[speedTokenIndex].tokenCount > turnOrder.Count(t => t.myName == entity.myName)) {
                    actionPointGain = (BASE_ACTION_GAIN + entity.speed) * (1 + hasteToken.tokenValue);
                } else {
                    actionPointGain = (BASE_ACTION_GAIN + entity.speed);
                }
            } else if (entity.activeTokens.Any(t => t.tokenName == "Slow")) {
                speedTokenIndex = entity.activeTokens.FindIndex(t => t.tokenName == "Slow");
                if (entity.activeTokens[speedTokenIndex].tokenCount > turnOrder.Count(t => t.myName == entity.myName)) {
                    actionPointGain = (BASE_ACTION_GAIN + entity.speed) * (1 - slowToken.tokenValue);
                } else {
                    actionPointGain = (BASE_ACTION_GAIN + entity.speed);
                }
            } else {
                actionPointGain = (BASE_ACTION_GAIN + entity.speed);
            }
            float tickDifference = (TURN_START_THRESHOLD - entity.actionPoints) / actionPointGain;
            entity.ticksToTurn = tickDifference;
        }
        turnOrder.Sort((bi1, bi2) => bi1.ticksToTurn.CompareTo(bi2.ticksToTurn));
        
        turnOrderDisplay.SetTurnDisplay(turnOrder);
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
                allCombatants[characterIndex].targetPortraits[i].GetComponent<Image>().sprite =
                    enemyCombatants[i].myPortrait;
                allCombatants[characterIndex].targetBorders[i].GetComponentInChildren<Image>().color =
                    new Color32(255, 0, 0, 255);
                // Change the button's text
                //allCombatants[characterIndex].targetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyCombatants[i].myName;
            }
        } else {
            // Enable buttons for each present ally
            for (int i = 0; i < partyCombatants.Count; i++) {
                allCombatants[characterIndex].targetButtons[i].SetActive(true);
                allCombatants[characterIndex].targetPortraits[i].GetComponent<Image>().sprite =
                    partyCombatants[i].myPortrait;
                allCombatants[characterIndex].targetBorders[i].GetComponentInChildren<Image>().color =
                    new Color32(147, 229, 242, 255);
                // Change the button's text
                //allCombatants[characterIndex].targetButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = partyCombatants[i].myName;
            }
        }
        
    }

    private void SetTargetValuesForDisplay(int hoveredTarget)
    {
        BattleEntities activeEntity = allCombatants[currentPlayer];
        BattleEntities targetEntity;
        // Check if target is ally or enemy
        int target;
        if (targetIsEnemy) {
            target = allCombatants.IndexOf(enemyCombatants[hoveredTarget]);
            targetEntity = allCombatants[target];
        } else {
            target = allCombatants.IndexOf(partyCombatants[hoveredTarget]);
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
        
        // Check for damage abilities
        if (activeEntity.myAbilities[activeEntity.activeAbility].abilityType == Ability.AbilityType.Damage) {
            RunAbilityAgainstSelfTokens(activeEntity, ref abilityModifier, ref singleValue, ref acc, ref min, ref max, ref crit);
            RunAbilityAgainstTargetTokens(activeEntity, targetEntity, activeEntity.myAbilities[activeEntity.activeAbility],
                ref singleValue, ref acc, ref min, ref max, ref crit);
        }
        
        // Check heal against tokens
        if (activeEntity.myAbilities[activeEntity.activeAbility].abilityType == Ability.AbilityType.Heal) {
            RunHealAgainstSelfTokens(activeEntity, ref abilityModifier, ref singleValue, ref acc, ref min, ref max, ref crit);
        }

        crit -= allCombatants[target].critResist;
        if (crit < 0) {
            crit = 0;
        }
        
        bool isDamage;
        if (activeEntity.activeAbilityType == "Damage") {
            isDamage = true;
            if (activeEntity.activeTokens.All(t => t.tokenName != "Pierce") ||
                activeEntity.activeTokens.All(t => t.tokenName != "OffGuard")) {
                min -= targetEntity.currentArmor;
                max -= targetEntity.currentArmor;
            }
        } else if (activeEntity.activeAbilityType == "Heal") {
            isDamage = false;
            if (IgnoreRestoreWithTokens(activeEntity, allCombatants[hoveredTarget])) {
                singleValue = true;
                min = 0;
                max = 0;
            }
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

    public void IndicateTurnTarget(int hoveredTarget)
    {
        string targetName = turnOrder[hoveredTarget].myName;
        int targetIndex = allCombatants.FindIndex(t => t.myName == targetName);
        if (allCombatants[targetIndex].isPlayer) {
            allCombatants[targetIndex].battleVisuals.TargetAllyActive();
        } else {
            allCombatants[targetIndex].battleVisuals.TargetEnemyActive();
        }
    }

    public void StopIndicatingTurnTarget(int hoveredTarget)
    {
        string targetName = turnOrder[hoveredTarget].myName;
        int targetIndex = allCombatants.FindIndex(t => t.myName == targetName);
        allCombatants[targetIndex].battleVisuals.TargetInactive();
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

    private int GetAbilityModifier(BattleEntities activeEntity, int activeAbilityIndex)
    {
        string abilityKey = activeEntity.myAbilities[activeAbilityIndex].keyStat.ToString();
        int abilityKeyMod = activeEntity.myAbilities[activeAbilityIndex].statModifier;
        int abilityModifier = 0;
        
        switch (abilityKey)
        {
            case "Null":
                abilityModifier = 0;
                break;
            case "Power":
                abilityModifier = activeEntity.power * abilityKeyMod;
                break;
            case "Skill":
                abilityModifier = activeEntity.skill * abilityKeyMod;
                break;
            case "Wit":
                abilityModifier = activeEntity.wit * abilityKeyMod;
                break;
            case "Mind":
                abilityModifier = activeEntity.mind * abilityKeyMod;
                break;
            case "Speed":
                abilityModifier = activeEntity.speed * abilityKeyMod;
                break;
            case "Luck":
                abilityModifier = activeEntity.luck * abilityKeyMod;
                break;
            default:
                print("Invalid damage key of " +  abilityKey + " supplied");
                break;
        }
        return abilityModifier;
    }

    private int GetSecondaryAbilityModifier(BattleEntities activeEntity, int activeAbilityIndex)
    {
        string secondaryKey = activeEntity.myAbilities[activeAbilityIndex].secondaryStat.ToString();
        int secondaryKeyMod = activeEntity.myAbilities[activeAbilityIndex].secondaryStatModifier;
        int secondaryModifier = 0;
        
        switch (secondaryKey)
        {
            case "Null":
                break;
            case "Power":
                secondaryModifier = activeEntity.power * secondaryKeyMod;
                break;
            case "Skill":
                secondaryModifier = activeEntity.skill * secondaryKeyMod;
                break;
            case "Wit":
                secondaryModifier = activeEntity.wit * secondaryKeyMod;
                break;
            case "Mind":
                secondaryModifier = activeEntity.mind * secondaryKeyMod;
                break;
            case "Speed":
                secondaryModifier = activeEntity.speed * secondaryKeyMod;
                break;
            case "Luck":
                secondaryModifier = activeEntity.luck * secondaryKeyMod;
                break;
            default:
                print("Invalid secondary damage key of " +  secondaryKey + " supplied");
                break;
        }
        return secondaryModifier;
    }

    private void SetAbilityValues(BattleEntities activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        abilityModifier = GetAbilityModifier(activeEntity, activeEntity.activeAbility);
        
        isCrit = false;
        min = activeEntity.myAbilities[activeEntity.activeAbility].dmgMin + abilityModifier;
        max = activeEntity.myAbilities[activeEntity.activeAbility].dmgMax + abilityModifier;
        crit = activeEntity.critChance + activeEntity.myAbilities[activeEntity.activeAbility].critChance;
    }

    private void SetSecondaryAbilityValues(BattleEntities activeEntity, ref int secondaryAbilityModifier, ref int secondaryValue)
    {
        secondaryAbilityModifier = GetSecondaryAbilityModifier(activeEntity, activeEntity.activeAbility);
        secondaryValue += secondaryAbilityModifier + activeEntity.myAbilities[activeEntity.activeAbility].secondaryValue;
    }

    private void RunAbilityAgainstSelfTokens(BattleEntities activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        if (activeEntity.myName == "Bune") {
            if (activeEntity.activeTokens.Any(t => t.tokenName == "Vice")) {
                int vicePosition = activeEntity.activeTokens.FindIndex(t => t.tokenName == "Vice");
                crit += (int)(viceToken.tokenValue * activeEntity.activeTokens[vicePosition].tokenCount);
            }
        }
        
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            if (token.tokenName == "BoostPlus") {
                min = (int)(min * (1 + boostPlusToken.tokenValue));
                max = (int)(max * (1 + boostPlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Boost") {
                min = (int)(min * (1 + boostToken.tokenValue));
                max = (int)(max * (1 + boostToken.tokenValue));
                break;
            } else if (token.tokenName == "Break") {
                min = (int)(min * (1 - breakToken.tokenValue));
                max = (int)(max * (1 - breakToken.tokenValue));
                break;
            }
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                max = (int)(max * CRIT_DAMAGE_MODIFIER);
                min = max;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind") {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }
    
    private void RunHealAgainstSelfTokens(BattleEntities activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            if (token.tokenName == "BoostPlus") {
                min = (int)(min * (1 + boostPlusToken.tokenValue));
                max = (int)(max * (1 + boostPlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Boost") {
                min = (int)(min * (1 + boostToken.tokenValue));
                max = (int)(max * (1 + boostToken.tokenValue));
                break;
            } else if (token.tokenName == "Break") {
                min = (int)(min * (1 - breakToken.tokenValue));
                max = (int)(max * (1 - breakToken.tokenValue));
                break;
            }
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                max = (int)(max * CRIT_DAMAGE_MODIFIER);
                min = max;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind") {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }
    
    private void RunAbilityAgainstTargetTokens(BattleEntities entity, BattleEntities targetEntity, Ability ability, ref bool isCrit, ref float acc,
        ref int min, ref int max, ref int crit)
    {
        bool hasPrecision = entity.activeTokens.Any(t => t.tokenName == "Precision");
        
        foreach (BattleToken token in targetEntity.activeTokens) {
            // Check for Block or Vulnerable tokens
            if (token.tokenName == "BlockPlus" && !hasPrecision && !ability.ignoreBlock) {
                min = (int)(min * (1 - blockPlusToken.tokenValue));
                max = (int)(max * (1 - blockPlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Block" && !hasPrecision && !ability.ignoreBlock) {
                min = (int)(min * (1 - blockToken.tokenValue));
                max = (int)(max * (1 - blockToken.tokenValue));
                break;
            } else if (token.tokenName == "Vulnerable") {
                min = (int)(min * (1 + vulnerableToken.tokenValue));
                max = (int)(max * (1 + vulnerableToken.tokenValue));
                break;
            }

            // Check for Dodge tokens
            if (token.tokenName == "DodgePlus" && !hasPrecision  && !ability.ignoreDodge) {
                acc = (int)(acc * (1 - dodgePlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Dodge" && !hasPrecision  && !ability.ignoreDodge) {
                acc = (int)(acc * (1 - dodgeToken.tokenValue));
                break;
            }
        }
    }

    private void RunBuffAgainstSelfTokens(BattleEntities activeEntity, ref bool isCrit, ref float acc, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind") {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }
    
    private void RunDebuffAgainstSelfTokens(BattleEntities activeEntity, ref bool isCrit, ref float acc, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind") {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }

    private BattleToken CreateBattleToken(BattleToken originalToken)
    {
        BattleToken battleToken = new BattleToken();
        battleToken.SetTokenValues(originalToken.tokenName, originalToken.displayName, originalToken.tokenIcon,
            originalToken.tokenType, originalToken.tokenValue, originalToken.tokenCap, originalToken.tokenInverses,
            originalToken.tokenDescription);
        return battleToken;
    }
    
    private void AddTokens(BattleEntities applyingEntity, BattleEntities recipientEntity, string tokenName, int tokenCount, int resistPierce)
    {
        int tIndex = 100;
        bool notPresent = true;
        string inverseOne;
        string inverseTwo;
        BattleToken tokenAdded = allTokens.FirstOrDefault(t => tokenName == t.tokenName);

        if (tokenAdded != null) {
            switch (tokenAdded.tokenType) {
                case Token.TokenType.Buff when recipientEntity.activeTokens.Any(t => "Isolation" == t.tokenName) &&
                                               applyingEntity.activeTokens.All(t => "Precision" != t.tokenName):
                case Token.TokenType.Debuff when recipientEntity.activeTokens.Any(t => "Ward" == t.tokenName) &&
                                                 applyingEntity.activeTokens.All(t => "Precision" != t.tokenName) && 
                                                 !applyingEntity.myAbilities[recipientEntity.activeAbility].ignoreWard:
                case Token.TokenType.Ailments when recipientEntity.activeTokens.Any(t => "Ward" == t.tokenName) &&
                                                   applyingEntity.activeTokens.All(t => "Precision" != t.tokenName) && 
                                                   !applyingEntity.myAbilities[recipientEntity.activeAbility].ignoreWard:
                    return;
            }

            if (tokenAdded != null) {
                switch (tokenAdded.tokenInverses.Count) {
                    case 1:
                        inverseOne = tokenAdded.tokenInverses[0];
                        inverseTwo = "N/A";
                        break;
                    case 2:
                        inverseOne = tokenAdded.tokenInverses[0];
                        inverseTwo = tokenAdded.tokenInverses[1];
                        break;
                    default:
                        inverseOne = "N/A";
                        inverseTwo = "N/A";
                        break;
                }

                for (int i = 0; i < recipientEntity.activeTokens.Count; i++) {
                    if (recipientEntity.activeTokens[i].tokenName == inverseOne ||
                        recipientEntity.activeTokens[i].tokenName == inverseTwo) {
                        if (tokenCount > recipientEntity.activeTokens[i].tokenCount) {
                            recipientEntity.activeTokens.Remove(recipientEntity.activeTokens[i]);
                            BattleToken tempToken = CreateBattleToken(tokenAdded);
                            recipientEntity.activeTokens.Add(tempToken);
                            tIndex = recipientEntity.activeTokens.IndexOf(tempToken);
                            recipientEntity.activeTokens[tIndex].tokenCount = tokenCount - recipientEntity.activeTokens[i].tokenCount;
                        } else {
                            recipientEntity.activeTokens[i].tokenCount -= tokenCount;
                            if (recipientEntity.activeTokens[i].tokenCount <= 0) {
                                recipientEntity.activeTokens.Remove(recipientEntity.activeTokens[i]);
                            }
                        }

                        notPresent = false;
                        break;
                    }
                }

                if (tokenAdded.tokenType == Token.TokenType.Debuff) {
                    int debuffRoll = Random.Range(1, 101);
                    if (debuffRoll + resistPierce < recipientEntity.debuffResist) {
                        return;
                    }
                }

                foreach (BattleToken t in recipientEntity.activeTokens) {
                    if (t.tokenName == tokenAdded.tokenName) {
                        t.tokenCount += tokenCount;
                        tIndex = recipientEntity.activeTokens.IndexOf(t);
                        notPresent = false;
                    }
                }

                if (notPresent) {
                    BattleToken tempToken = CreateBattleToken(tokenAdded);
                    recipientEntity.activeTokens.Add(tempToken);
                    tIndex = recipientEntity.activeTokens.IndexOf(tempToken);
                    recipientEntity.activeTokens[tIndex].tokenCount = tokenCount;
                }

                if (tIndex != 100) {
                    if (recipientEntity.activeTokens[tIndex].tokenCount > recipientEntity.activeTokens[tIndex].tokenCap) {
                        recipientEntity.activeTokens[tIndex].tokenCount = recipientEntity.activeTokens[tIndex].tokenCap;
                    }
                }
            }
        }
        
        // Check and mark unique tokens added
        if (recipientEntity.myName == "Bune" && tokenAdded.tokenName == "Vice") {
            recipientEntity.gainedUniqueTokenLastTurn = true;
        }
        
        recipientEntity.battleVisuals.UpdateTokens(recipientEntity.activeTokens);
    }

    private void ClearTokens(BattleEntities targetEntity, string tokenName)
    {
        BattleToken tokenForRemoval = allTokens.FirstOrDefault(t => tokenName == t.tokenName);

        if (targetEntity.activeTokens.Any(t => tokenForRemoval.tokenName == t.tokenName)) {
            int tokenIndex = targetEntity.activeTokens.FindIndex(t => tokenForRemoval.tokenName == t.tokenName);
            targetEntity.activeTokens.RemoveAt(tokenIndex);
        }
    }

    private void RemoveSelfTurnStartTokens(BattleEntities activeEntity)
    {
        // Run character specific end of turn Methods
        if (activeEntity.myName == "Bune") {
            BuneRemoveVice();
        } else
        
        foreach (BattleToken t in activeEntity.activeTokens) {
            // Check for Haste or Slow tokens
            int tokenPosition;
            if (t.tokenName == "Haste") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Slow") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        
            // Check for Off-Guard tokens
            if (t.tokenName == "OffGuard") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        
            // Check for Anti-Heal tokens
            if (t.tokenName == "AntiHeal") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        }
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }

    private void RemoveSelfDamageTokens(BattleEntities activeEntity)
    {
        foreach (BattleToken t in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            int tokenPosition;
            if (t.tokenName == "BoostPlus") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Boost") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Break") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Critical tokens
            if (t.tokenName == "Critical") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Blind tokens
            if (t.tokenName == "Blind") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
            
            // Check for Pierce tokens
            if (t.tokenName == "Pierce") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        }

        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }
    
    private void RemoveSelfHealTokens(BattleEntities activeEntity)
    {
        foreach (BattleToken t in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            int tokenPosition;
            if (t.tokenName == "BoostPlus") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Boost") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break; 
            } else if (t.tokenName == "Break") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Critical tokens
            if (t.tokenName == "Critical") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Blind tokens
            if (t.tokenName == "Blind") {
                tokenPosition = activeEntity.activeTokens.IndexOf(t);
                if (activeEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    activeEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    activeEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }

    private void RemoveTargetDamageTokens(BattleEntities targetEntity)
    {
        foreach (BattleToken t in  targetEntity.activeTokens) {
            // Check for Block or Vulnerable tokens
            int tokenPosition;
            if (t.tokenName == "BlockPlus") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Block") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Vulnerable") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Dodge tokens
            if (t.tokenName == "DodgePlus") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Dodge") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
            
            // Check for Ward or Isolation
            if (t.tokenName == "Ward") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Isolation") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Taunt
            if (t.tokenName == "Taunt") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void RemoveTargetHealTokens(BattleEntities healTarget)
    {
        foreach (BattleToken t in healTarget.activeTokens) {
            int tokenPosition;
            if (t.tokenName == "AntiHeal") {
                tokenPosition = healTarget.activeTokens.IndexOf(t);
                if (healTarget.activeTokens[tokenPosition].tokenCount > 1) {
                    healTarget.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    healTarget.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
            
            // Check for Ward or Isolation
            if (t.tokenName == "Ward") {
                tokenPosition = healTarget.activeTokens.IndexOf(t);
                if (healTarget.activeTokens[tokenPosition].tokenCount > 1) {
                    healTarget.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    healTarget.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Isolation") {
                tokenPosition = healTarget.activeTokens.IndexOf(t);
                if (healTarget.activeTokens[tokenPosition].tokenCount > 1) {
                    healTarget.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    healTarget.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            healTarget.battleVisuals.UpdateTokens(healTarget.activeTokens);
        }
    }
    
    private void RemoveTargetBuffTokens(BattleEntities targetEntity)
    {
        foreach (BattleToken t in targetEntity.activeTokens) {
            int tokenPosition;
            // Check for Ward
            if (t.tokenName == "Isolation") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void RemoveTargetDebuffTokens(BattleEntities targetEntity)
    {
        foreach (BattleToken t in targetEntity.activeTokens) {
            int tokenPosition;
            // Check for Ward or Isolation
            if (t.tokenName == "Ward") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }

            // Check for Taunt
            if (t.tokenName == "Taunt") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void RemoveTokensOnMiss(BattleEntities targetEntity)
    {
        foreach (BattleToken t in targetEntity.activeTokens) {
            // Check for Dodge tokens
            int tokenPosition;
            if (t.tokenName == "DodgePlus") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break;
            } else if (t.tokenName == "Dodge") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
                break; 
            }

            // Check for Taunt tokens
            if (t.tokenName == "Ward") {
                tokenPosition = targetEntity.activeTokens.IndexOf(t);
                if (targetEntity.activeTokens[tokenPosition].tokenCount > 1) {
                    targetEntity.activeTokens[tokenPosition].tokenCount -= 1;
                } else {
                    targetEntity.activeTokens.RemoveAt(tokenPosition);
                }
            }
        }
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void TriggerTurnSpeedTokens(BattleEntities entity)
    {
        // TODO update this to make the target immune to Quick/Delay until the start of their next turn.
        int tokenPosition;
        bool isStunned = false;
        if (entity.activeTokens.Any(t => t.tokenName == "Stun")) {
            print(entity.myName + " was stunned!"); // TODO replace with stun animation
            entity.actionPoints -= TURN_START_THRESHOLD;
            RemoveSelfTurnStartTokens(entity);
            tokenPosition = entity.activeTokens.FindIndex(t => t.tokenName == "Stun");
            entity.activeTokens.RemoveAt(tokenPosition);
            isStunned = true;
            // Reduce Cooldowns of all unused abilities by one
            for (int i = 0; i < allCombatants[currentPlayer].abilityCooldowns.Count; i++) {
                if (entity.abilityCooldowns[i] > 0) {
                    entity.abilityCooldowns[i] -= 1;
                }
            }
            entity.battleVisuals.SetMyTurnAnimation(false);
            wentBack = false;
            
        } else if (entity.activeTokens.Any(t => t.tokenName == "Quick")) {
            print(entity.myName + " was quickened!"); // TODO replace with quicken animation
            entity.actionPoints += (int)quickToken.tokenValue;
            tokenPosition = entity.activeTokens.FindIndex(t => t.tokenName == "Quick");
            entity.activeTokens.RemoveAt(tokenPosition);
        } else if (entity.activeTokens.Any(t => t.tokenName == "Delay")) {
            print(entity.myName + " was delayed!"); // TODO replace with delay animation
            entity.actionPoints -= (int)delayToken.tokenValue;
            tokenPosition = entity.activeTokens.FindIndex(t => t.tokenName == "Delay");
            entity.activeTokens.RemoveAt(tokenPosition);
        }
        
        entity.battleVisuals.UpdateTokens(entity.activeTokens);
        if (isStunned) {
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        }
    }

    private bool IgnoreArmorWithTokens(BattleEntities attacker, BattleEntities attackTarget)
    {
        return attacker.activeTokens.Any(t => t.tokenName == "Pierce") ||
               attackTarget.activeTokens.Any(t => t.tokenName == "Pierce");
    }

    private bool IgnoreRestoreWithTokens(BattleEntities healer, BattleEntities healTarget)
    {
        return (healTarget.activeTokens.Any(t => t.tokenName == "AntiHeal") && 
                healer.activeTokens.All(t => t.tokenName != "Precision"));
    }
    
    // Character specific methods
    private void BuneViceActOut()
    {
        BattleEntities Bune = allCombatants[currentPlayer];
        BattleEntities abilityTarget;
        int actOutAbility;
        List<BattleEntities> targetList = new List<BattleEntities>();
        
        float viceRoll = Random.Range(1, 101);
        if (viceRoll < Bune.specialResourceFloat) {
            // Bune uses a random action with a random target
            print("Bune acts out due to boredom!");
            
            actOutAbility = Random.Range(1, Bune.myAbilities.Count + 1);

            bool targetingFoes = true;
            switch (Bune.myAbilities[actOutAbility].abilityType) {
                case Ability.AbilityType.Damage:
                case Ability.AbilityType.Debuff:
                    targetList = enemyCombatants;
                    targetingFoes = true;
                    break;
                case  Ability.AbilityType.Heal:
                case  Ability.AbilityType.Buff:
                    targetList = partyCombatants;
                    targetingFoes = false;
                    break;
            }

            bool hasTaunt = false;
            if (targetingFoes) {
                foreach (BattleEntities entity in targetList.Where(entity => entity.activeTokens.Any(t => t.tokenName == "Taunt"))) {
                    abilityTarget = entity;
                    hasTaunt = true;
                    break;
                }
            }
            if (!hasTaunt) {
                abilityTarget = targetList[Random.Range(0, targetList.Count)];
            }

            Bune.specialResourceFloat = BUNE_BASE_ACTOUT;
            
            // Reduce Cooldowns of all unused abilities by one
            for (int i = 0; i < allCombatants[currentPlayer].abilityCooldowns.Count; i++) {
                if (Bune.abilityCooldowns[i] > 0) {
                    Bune.abilityCooldowns[i] -= 1;
                }
            }
            // Start the cooldown of the used ability
            Bune.abilityCooldowns[Bune.activeAbility] = Bune.myAbilities[Bune.activeAbility].cooldown;
            
            // Reset turn-related values
            Bune.battleVisuals.SetMyTurnAnimation(false);
            wentBack = false;
            Bune.gainedUniqueTokenLastTurn = false;
            Bune.wasDamagedLastTurn = false;
            Bune.damagedBy = 100;
            
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
        } else {
            Bune.specialResourceFloat += BUNE_MAX_ACTOUT;
            if (Bune.specialResourceFloat > BUNE_MAX_ACTOUT) {
                Bune.specialResourceFloat = BUNE_MAX_ACTOUT;
            }
        }
    }
    
    private void BuneRemoveVice()
    {
        BattleEntities Bune = allCombatants[currentPlayer];
        if (Bune.activeTokens.Any(t => t.tokenName == "Vice")) {
            if (!Bune.wasDamagedLastTurn && !Bune.gainedUniqueTokenLastTurn) {
                Bune.activeTokens.RemoveAll(t => t.tokenName == "Vice");
            }
        }
    }
    
    
    private IEnumerator DamageAction(BattleEntities attacker, BattleEntities attackTarget, int activeAbilityIndex)
    {
        Ability activeAbility = attacker.myAbilities[activeAbilityIndex];
        
        // Declare damage values
        int damage;
        int damageModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        
        // Declare secondary damage values, if any
        int secondaryDamage;
        int secondaryModifier = 0;
        int secondaryValue = 0;
        string secondaryTarget = activeAbility.secondaryTarget.ToString();
        
        // Get damage values
        SetAbilityValues(attacker, ref damageModifier, ref isCrit, ref acc, ref minDamageRange,
                ref maxDamageRange, ref critChance);
        
        // Run damage against tokens
        RunAbilityAgainstSelfTokens(attacker, ref damageModifier, ref isCrit, ref acc, ref minDamageRange, 
            ref maxDamageRange, ref critChance);
        RunAbilityAgainstTargetTokens(attacker, attackTarget, activeAbility, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        // Reduce crit chance by target's crit resist
        critChance -= attackTarget.critResist;
        if (critChance < 0) {
            critChance = 0;
        }
        
        // Get secondary damage values
        SetSecondaryAbilityValues(attacker, ref secondaryModifier,  ref secondaryValue); secondaryDamage = secondaryValue;
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.selfTokensCleared) {
                ClearTokens(attacker, token.ToString());
            }
        }
        
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(attacker, attacker, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i], 0);
        }
        
        // Check for hit
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            attackTarget.battleVisuals.AbilityMisses();
            RemoveSelfDamageTokens(attacker);
            RemoveTokensOnMiss(attackTarget);
            SelfGain(attacker, activeAbility, isCrit);
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }
        
        // Check for crit and determine damage values for crit
        int critRoll = Random.Range(1, 101);
        if (critRoll < critChance && attacker.activeTokens.All(t => t.tokenName != "Critical")) {
            isCrit = true;
            damage = (int)(maxDamageRange * CRIT_DAMAGE_MODIFIER);
            if (!IgnoreArmorWithTokens(attacker, attackTarget) || attacker.myAbilities[attacker.activeAbility].ignoreArmor) {
                damage -= attackTarget.currentArmor;
            }
        } else {
            damage = Random.Range(minDamageRange, maxDamageRange + 1);
            if (!IgnoreArmorWithTokens(attacker, attackTarget) || attacker.myAbilities[attacker.activeAbility].ignoreArmor) {
                damage -= attackTarget.currentArmor;
            }
        }

        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                ClearTokens(attackTarget, token.ToString());
            }
        }
        
        // Restore self values
        SelfGain(attacker, activeAbility, isCrit);
        
        // Apply on crit tokens if attack crit
        if (isCrit) {
            for (int i = 0; i < activeAbility.selfCritTokensApplied.Length; i++) {
                AddTokens(attacker, attacker, activeAbility.selfCritTokensApplied[i].ToString(),
                    activeAbility.selfCritTokenCountApplied[i], 0);
            }
            for (int i = 0; i < activeAbility.targetCritTokensApplied.Length; i++) {
                AddTokens(attacker, attackTarget, activeAbility.targetCritTokensApplied[i].ToString(),
                    activeAbility.targetCritTokenCountApplied[i], attackTarget.resistPierce);
            }
        }
        
        // Remove appropriate tokens
        RemoveSelfDamageTokens(attacker);
        RemoveTargetDamageTokens(attackTarget);
        
        // Apply Secondary Damage
        if (secondaryTarget != "Null") {
            switch (secondaryTarget) {
                case "Bonus":
                    damage += secondaryDamage;
                    break;
                case "Spirit":
                    attackTarget.currentSpirit -= secondaryDamage;
                    if (attackTarget.currentSpirit < 0) {
                        attackTarget.currentSpirit = 0;
                    }
                    break;
                case "Armor":
                    attackTarget.currentArmor -= secondaryDamage;
                    if (attackTarget.currentArmor < 0) {
                        attackTarget.currentArmor = 0;
                    }
                    break;
                case "ActionPoints":
                    attackTarget.actionPoints -= secondaryDamage;
                    if (attackTarget.actionPoints < 0) {
                        attackTarget.actionPoints = 0;
                    }
                    break;
                default:
                    print("Invalid secondary target of " +  secondaryTarget + " supplied");
                    break;
            }
            print(attackTarget.myName + " " + secondaryTarget + " damaged by " + secondaryDamage + ".");
        }
        
        // Apply target tokens
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(attacker, attackTarget, activeAbility.targetTokensApplied[i].ToString(),
                activeAbility.targetTokenCountApplied[i], attacker.resistPierce);
        }
        
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

        attackTarget.wasDamagedLastTurn = true;
        attackTarget.damagedBy = allCombatants.IndexOf(attacker);
        
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
        
        // Declare heal values
        int restore;
        int restoreModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        
        // Declare secondary heal values, if any
        int secondaryRestore;
        int secondaryModifier = 0;
        int secondaryValue = 0;
        string secondaryTarget = activeAbility.secondaryTarget.ToString();
        
        // Get heal values
        SetAbilityValues(healer, ref restoreModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);

        // Check heal against tokens
        RunHealAgainstSelfTokens(healer, ref restoreModifier, ref isCrit, ref acc, ref minDamageRange, 
            ref maxDamageRange, ref critChance);
        
        // Get secondary heal values
        SetSecondaryAbilityValues(healer, ref secondaryModifier,  ref secondaryValue);
        secondaryRestore = secondaryValue;
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.selfTokensCleared) {
                ClearTokens(healer, token.ToString());
            }
        }
        
        // Apply self tokens
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(healer, healer, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i], 0);
        }
        
        // Check for hit
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            healTarget.battleVisuals.AbilityMisses();
            RemoveSelfHealTokens(healer);
            SelfGain(healer, activeAbility, isCrit);
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }
        
        // Check for crit then determine heal value either way
        int critRoll = Random.Range(1, 101);
        if (critRoll + healTarget.critResist < critChance && healer.activeTokens.All(t => t.tokenName != "Critical")) {
            isCrit = true;
            restore = (int)(maxDamageRange * CRIT_DAMAGE_MODIFIER);
        } else {
            restore = Random.Range(minDamageRange, maxDamageRange + 1);
        }
        
        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                ClearTokens(healTarget, token.ToString());
            }
        }
        
        SelfGain(healer, activeAbility, isCrit);
        
        // Check for Anti-Heal
        if (IgnoreRestoreWithTokens(healer, healTarget)) {
            restore *= (int)antiHealToken.tokenValue;
            secondaryRestore *= (int)antiHealToken.tokenValue;
        }
        
        // Add on crit tokens, if applicable
        if (isCrit) {
            for (int i = 0; i < activeAbility.selfCritTokensApplied.Length; i++) {
                AddTokens(healer, healer, activeAbility.selfCritTokensApplied[i].ToString(), activeAbility.selfCritTokenCountApplied[i], 0);
            }
            for (int i = 0; i < activeAbility.targetCritTokensApplied.Length; i++) {
                AddTokens(healer, healTarget, activeAbility.targetCritTokensApplied[i].ToString(),
                    activeAbility.targetCritTokenCountApplied[i], 0);
            }
        }
        
        // Remove appropriate tokens
        RemoveSelfHealTokens(healer);
        RemoveTargetHealTokens(healTarget);
        
        // Apply Secondary Heal
        if (secondaryTarget != "Null") {
            switch (secondaryTarget) {
                case "Bonus":
                    restore += secondaryRestore;
                    break;
                case "Spirit":
                    healTarget.currentSpirit += secondaryRestore;
                    if (healTarget.currentSpirit > healTarget.maxSpirit) {
                        healTarget.currentSpirit = healTarget.maxSpirit;
                    }
                    break;
                case "Armor":
                    healTarget.currentArmor += secondaryRestore;
                    if (healTarget.currentArmor > healTarget.maxArmor) {
                        healTarget.currentArmor = healTarget.maxArmor;
                    }
                    break;
                case "ActionPoints":
                    healTarget.actionPoints += secondaryRestore;
                    break;
                default:
                    print("Invalid secondary target of " +  secondaryTarget + " supplied");
                    break;
            }
        }
        
        // Heal the target
        //healer.battleVisuals.PlayAttackAnimation(); // play the attack animation
        healTarget.currentDefense += restore; // restore HP
        healTarget.battleVisuals.PlayHealAnimation(restore, isCrit); // target plays on hit animation
        
        // Apply target tokens
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(healer, healTarget, activeAbility.targetTokensApplied[i].ToString(),
                activeAbility.targetTokenCountApplied[i], 0);
        }
        
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
        
        int abilityModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        
        // Get ability values
        SetAbilityValues(buffer, ref abilityModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        RunBuffAgainstSelfTokens(buffer, ref isCrit, ref acc, ref critChance);
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.selfTokensCleared) {
                ClearTokens(buffer, token.ToString());
            }
        }
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(buffer, buffer, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i], 0);
        }
        
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            buffTarget.battleVisuals.AbilityMisses();
            RemoveTokensOnMiss(buffTarget);
            SelfGain(buffer, activeAbility, isCrit);
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }
        
        int critRoll = Random.Range(1, 101);
        if (critRoll + buffTarget.critResist < critChance) {
            isCrit = true;
        }
        
        
        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                ClearTokens(buffTarget, token.ToString());
            }
        }

        RemoveTargetBuffTokens(buffTarget);
        
        SelfGain(buffer, activeAbility, isCrit);

        if (isCrit) {
            for (int i = 0; i < activeAbility.selfCritTokensApplied.Length; i++) {
                AddTokens(buffer, buffer, activeAbility.selfCritTokensApplied[i].ToString(), activeAbility.selfCritTokenCountApplied[i], 0);
            }
            for (int i = 0; i < activeAbility.targetCritTokensApplied.Length; i++) {
                AddTokens(buffer, buffTarget, activeAbility.targetCritTokensApplied[i].ToString(), activeAbility.targetCritTokenCountApplied[i], 0);
            }
        }
        
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(buffer,buffTarget, activeAbility.targetTokensApplied[i].ToString(), activeAbility.targetTokenCountApplied[i], 0);
        }
        
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
        yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        SaveResources();
    }
    
    private IEnumerator DebuffAction(BattleEntities debuffer, BattleEntities debuffTarget, int activeAbilityIndex)
    {
        Ability activeAbility = debuffer.myAbilities[activeAbilityIndex];
        
        int abilityModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        
        // Get ability values
        SetAbilityValues(debuffer, ref abilityModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        RunDebuffAgainstSelfTokens(debuffer, ref isCrit, ref acc, ref critChance);
        
        // Reduce crit chance by target's crit resist
        critChance -= debuffTarget.critResist;
        if (critChance < 0) {
            critChance = 0;
        }

        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.selfTokensCleared) {
                ClearTokens(debuffer, token.ToString());
            }
        }
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            AddTokens(debuffer, debuffer, activeAbility.selfTokensApplied[i].ToString(), activeAbility.selfTokenCountApplied[i], 0);
        }
        
        int accRoll = Random.Range(1, 101);
        if (accRoll > (int)acc) {
            print("The ability missed!");
            debuffTarget.battleVisuals.AbilityMisses();
            RemoveTokensOnMiss(debuffTarget);
            SelfGain(debuffer, activeAbility, isCrit);
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
            SaveResources();
            yield break;
        }
        
        int critRoll = Random.Range(1, 101);
        if (critRoll + debuffTarget.critResist < critChance) {
            isCrit = true;
        }
        
        if (isCrit) {
            for (int i = 0; i < activeAbility.selfCritTokensApplied.Length; i++) {
                AddTokens(debuffer, debuffer, activeAbility.selfCritTokensApplied[i].ToString(),
                    activeAbility.selfCritTokenCountApplied[i], 0);
            }
            for (int i = 0; i < activeAbility.targetCritTokensApplied.Length; i++) {
                AddTokens(debuffer, debuffTarget, activeAbility.targetCritTokensApplied[i].ToString(),
                    activeAbility.targetCritTokenCountApplied[i], debuffer.resistPierce);
            }
        }
        
        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                ClearTokens(debuffTarget, token.ToString());
            }
        }
        RemoveTargetDebuffTokens(debuffTarget);
        
        SelfGain(debuffer, activeAbility, isCrit);
        
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            AddTokens(debuffer, debuffTarget, activeAbility.targetTokensApplied[i].ToString(),
                activeAbility.targetTokenCountApplied[i], debuffer.resistPierce);
        }
        
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
        yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        SaveResources();
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
                allCombatants[currentPlayer].currentDefense -= resourceCost;
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
                //allCombatants[currentPlayer].currentHealth -= resourceCost;
                print("Special resource called for consumption.");
                break;
            default:
                print("Invalid resource " + resourceConsumed + " called for consumption.");
                break;
        }
        yield break;
    }
    
    private void SelfGain(BattleEntities self, Ability ability, bool isCrit)
    {
        if (ability.selfTarget == Ability.SelfTarget.Null ||
            self.activeTokens.Any(t => t.tokenName == "AntiHeal")) return;
        
        int gainValue;
        int abilityMod = GetAbilityModifier(self, self.myAbilities.IndexOf(ability));
        if (isCrit) {
            gainValue = (int)((ability.selfMax + abilityMod) * 1.5f);
        } else {
            gainValue = Random.Range(ability.selfMin, ability.selfMax + 1);
        }
        switch (ability.selfTarget) {
            case Ability.SelfTarget.Health:
                self.currentHealth += gainValue;
                if (self.currentHealth > self.maxHealth) {
                    self.currentHealth = self.maxHealth;
                }
                break;
            case Ability.SelfTarget.Defense:
                self.currentHealth += gainValue;
                if (self.currentDefense > self.maxDefense) {
                    self.currentDefense = self.maxDefense;
                }
                break;
            case Ability.SelfTarget.Spirit:
                self.currentHealth += gainValue;
                if (self.currentSpirit > self.maxSpirit) {
                    self.currentSpirit = self.maxSpirit;
                }
                break;
            case Ability.SelfTarget.Armor:
                self.currentArmor += gainValue;
                if (self.currentArmor > self.maxArmor) {
                    self.currentArmor = self.maxArmor;
                }
                break;
            case Ability.SelfTarget.ActionPoints:
                self.actionPoints += gainValue;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
    public Sprite myPortrait;
    public bool isPlayer;
    public bool wasDamagedLastTurn;
    public int damagedBy;
    
    public string activeAbilityType;
    public int target;
    public int activeAbility;
    public int level;
    
    public int actionPoints;
    public float ticksToTurn;

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

    public int critChance;
    public int resistPierce;
    public int stunResist;
    public int debuffResist;
    public int critResist;
    public int ailmentResist;
    
    // Variables for use with character-specific mechanics
    public bool gainedUniqueTokenLastTurn;
    public float specialResourceFloat;
    
    // Enemy Only
    public EnemyBrain enemyBrain;
    
    public BattleVisuals battleVisuals;
    public CombatMenuVisuals combatMenuVisuals;

    public GameObject[] abilityButtons;
    public GameObject[] targetButtons;
    public GameObject[] targetPortraits;
    public GameObject[] targetBorders;
    public List<Ability> myAbilities;
    public List<int> abilityCooldowns;
    public List<BattleToken> activeTokens;

    public void SetEntityValue(string entityName, Sprite entityPortrait, int entityLevel, int entityMaxHealth,
        int entityCurrentHealth, int entityMaxSpirit, int entityCurrentSpirit, int entityMaxDefense, int entityMaxArmor,
        int entityPower, int entitySkill, int entityWit, int entityMind, int entitySpeed, int entityLuck,
        int entityStunResist, int entityDebuffResist, int entityAilmentResist, bool entityIsPlayer)
    {
        myName = entityName;
        myPortrait = entityPortrait;
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

        critChance = (int)(skill * BattleSystem.SkillCritMod);
        resistPierce = (int)(wit * BattleSystem.WitPierceMod);
        stunResist = entityStunResist + (int)(power * BattleSystem.PowerStunMod);
        debuffResist = entityDebuffResist + (int)(mind * BattleSystem.MindDebuffMod);
        ailmentResist = entityAilmentResist;
        critResist = (int)(luck * BattleSystem.LuckCritMod);
        if (!isPlayer) {
            critResist -= 5;
            if (critResist < 0) {
                critResist = 0;
            }
        }
        
        activeTokens = new List<BattleToken>();
    }

    public void SetEntityTurnDisplayValues(string entityName, Sprite entityPortrait, bool entityIsPlayer, int entitySpeed,
        int entityActionPoints, List<BattleToken> entityTokens)
    {
        myName = entityName;
        isPlayer = entityIsPlayer;
        speed =  entitySpeed;
        myPortrait = entityPortrait;
        actionPoints = entityActionPoints;
        activeTokens = entityTokens;
    }

    public void SetEnemyBrain(EnemyBrain entityBrain)
    {
        enemyBrain = entityBrain;
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
    public string displayName;
    public Sprite tokenIcon;
    public Token.TokenType tokenType;
    public float tokenValue;
    public int tokenCap;
    public int tokenCount;

    [FormerlySerializedAs("inverseTokens")] public List<String> tokenInverses = new List<String>();

    public string tokenDescription;

    public void SetTokenValues(string storedName, string storedDisplayName, Sprite storedIcon, Token.TokenType storedType,
        float storedValue, int storedCap, List<String> storedInverses, string storedDescription)
    {
        tokenName  = storedName;
        displayName = storedDisplayName;
        tokenIcon = storedIcon;
        tokenType = storedType;
        tokenValue = storedValue;
        tokenCap = storedCap;
        tokenCount = 0;
        tokenInverses = storedInverses;
        
        tokenDescription = storedDescription;
    }
}