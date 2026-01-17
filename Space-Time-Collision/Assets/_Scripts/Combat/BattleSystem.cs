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
        End,
        Won,
        Lost
    }
    
    private enum DuplicationType
    {
        Cleave,
        Ricochet,
        Dualcast,
        None
    }

    [Header("Battle System")]
    [SerializeField] private BattleState state;

    [Header("Grid Locations")]
    [SerializeField] private List<GridTile> partyBattleGrid = new List<GridTile>();
    [SerializeField] private List<GridTile> enemyBattleGrid = new List<GridTile>();
    
    [Header("Combatants")]
    [SerializeField] private List<BattleEntity> allCombatants = new List<BattleEntity>();
    [SerializeField] private List<BattleEntity> enemyCombatants = new List<BattleEntity>();
    [SerializeField] private List<BattleEntity> partyCombatants = new List<BattleEntity>();
    [SerializeField] private List<BattleEntity> preparedCombatants = new List<BattleEntity>();
    [SerializeField] private List<BattleEntity> turnOrder = new  List<BattleEntity>();
    [SerializeField] private List<BattleEntity> targetList = new List<BattleEntity>();
    
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
    private BattleToken guardColumnToken; // Not implemented
    private BattleToken guardRowToken; // Not implemented
    private BattleToken hasteToken;
    private BattleToken pierceToken;
    private BattleToken precisionToken;
    private BattleToken quickToken;
    private BattleToken ricochetToken;
    private BattleToken riposteToken; // Not implemented
    private BattleToken rushToken;
    private BattleToken stealthToken;
    private BattleToken tauntToken;
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
    private BattleToken restrictToken;
    private BattleToken slowToken;
    private BattleToken staggerToken;
    private BattleToken stunToken;
    private BattleToken vulnerableToken;
    
    // Character Specific Tokens
    private BattleToken ascensionToken;
    private BattleToken killseekerToken;
    private BattleToken viceToken;
    
    // Ailment Counters
    private BattleToken bleedCounter;
    private BattleToken burnCounter;
    private BattleToken poisonCounter;
    
    [Header("UI")]
    [SerializeField] private GameObject combatStartUI;
    [SerializeField] private Animator combatStartUIAnimator;
    
    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private TokenManager tokenManager;
    private TurnOrderDisplay turnOrderDisplay;
    private CombatGrid combatGrid;
    
    // Character Specific Logic
    private RepentantBattleLogic repentantLogic;
    private CowboyBattleLogic cowboyLogic;
    private RicochetBattleLogic ricochetLogic;
    
    private int currentPlayer;
    private int extraCastCount = 0;
    private bool abilitySelected;
    private bool wentBack = false;
    private bool targetSelected;
    private bool targetIsEnemy;
    private bool usedAbility;
    private bool usedLightAction;
    private bool brokeRow;
    private bool abilityDuplicated = false;
    private bool targetBeingIndicated = false;
    private bool targetIndicatedGrid = false;
    
    private DuplicationType duplicationType = DuplicationType.None;
    
    // Grid Variables
    private const int BASE_PLAYER_X_MIN = 1;
    private const int BASE_PLAYER_X_MAX = 4;
    private const int BASE_ENEMY_X_MIN = 5;
    private const int BASE_ENEMY_X_MAX = 8;
    private const int BASE_Y_MIN = 1;
    private const int BASE_Y_MAX = 4;
    
    public int playerXMin = BASE_PLAYER_X_MIN;
    public int playerXMax = BASE_PLAYER_X_MAX;
    public int enemyXMin = BASE_ENEMY_X_MIN;
    public int enemyXMax = BASE_ENEMY_X_MAX;
    public int yMin = BASE_Y_MIN;
    public int yMax = BASE_Y_MAX;
    
    private int xChange = 0;
    private int yChange = 0;
    
    private const float COMBAT_BEGIN_DELAY = 1.75f;
    private const float TURN_ACTION_DELAY = 1.5f;
    private const float AILMENT_DAMAGE_DELAY = 0.5f;
    private const float DEATH_DELAY = 3f;
    private const float CRIT_DAMAGE_MODIFIER = 1.5f;
    private const int TURN_START_THRESHOLD = 200;
    private const int BASE_ACTION_GAIN = 20;
    private const int MAX_ACTION_START = 100;
    private const int MAX_INDIVIDUAL_DISPLAY = 5;
    private const int PREVIEW_RESIST_PIERCE = 500;
    private const int PLAYER_NONMOVE_ABILITIES = 4;
    private const float MOVE_SPEED = 1f;
    private const string MAP_SCENE = "BaseScene";
    private const string BASE_SCENE = "BaseScene";
    
    // Stat modifiers
    public const float SkillCritMod = 1.5f;
    public const float WitPierceMod = 2;
    public const float PowerStunMod = 2;
    public const float MindDebuffMod = 2;
    public const float LuckCritMod = 1;
    
    // Animator Constants
    private const string BATTLE_START_END = "EndTrigger";

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();
        tokenManager = FindFirstObjectByType<TokenManager>();
        turnOrderDisplay = FindFirstObjectByType<TurnOrderDisplay>();
        combatGrid = FindFirstObjectByType<CombatGrid>();
        
        combatGrid.GetGridInfo(ref partyBattleGrid, ref enemyBattleGrid);
        LinkCharacterLogics();
        CreatePartyEntities();
        CreateEnemyEntities();
        InitializeBattleTokens();
    }
    
    
    private void Start()
    {
        StartCoroutine(StartRoutine());
        battleStartUI.SetActive(true);
    }

    private void LinkCharacterLogics()
    {
        repentantLogic = FindFirstObjectByType<RepentantBattleLogic>();
        repentantLogic.RepentantBattleSystemLink(this);
        
        cowboyLogic = FindFirstObjectByType<CowboyBattleLogic>();
        cowboyLogic.CowboyBattleSystemLink(this);
        
        ricochetLogic = FindFirstObjectByType<RicochetBattleLogic>();
        ricochetLogic.RicochetBattleSystemLink(this);
    }

    public List<BattleEntity> GetPartyList()
    {
        return partyCombatants;
    }
    
    public List<BattleEntity> GetEnemyList()
    {
        return enemyCombatants;
    }

    public BattleEntity GetActiveEntity()
    {
        return allCombatants[currentPlayer];
    }

    public List<BattleToken> GetAllTokens()
    {
        return allTokens;
    }
    
    // Battle state routines

    private IEnumerator StartRoutine()
    {
        if (state == BattleState.Start) {
            SetAbilityBar();
            
            for (int i = 0; i < allCombatants.Count; i++) {
                allCombatants[i].actionPoints = Random.Range(1, MAX_ACTION_START + 1);
            }
            GetTurnOrder();
            
            yield return new WaitForSeconds(COMBAT_BEGIN_DELAY);
            Destroy(battleStartUI);
            state = BattleState.Battle;
            StartCoroutine(BattleRoutine());
            yield break;
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
        
        // Make sure display settings are correct TODO update this to only run for characters that could feasibly have new values here
        foreach (BattleEntity entity in allCombatants) {
            FindMyGridPosition(entity);
        }
        
        if (state == BattleState.Battle) {
            while (preparedCombatants.Count <= 0) {
                for (int i = 0; i < allCombatants.Count; i++) {
                    if (state == BattleState.Battle) {
                        if (allCombatants[i].activeTokens.Any(t => t.tokenName == "Haste")) {
                            allCombatants[i].actionPoints += (int)((BASE_ACTION_GAIN + allCombatants[i].speed) * 
                                                                   (1 + hasteToken.tokenValue));
                        } else if (allCombatants[i].activeTokens.Any(t => t.tokenName == "Slow")) {
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
            yield break;
        } else {
            print("Battle Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator OrderRoutine()
    {
        if (state == BattleState.Ordering) {
            if (preparedCombatants.Count > 0) {
                
                List<int> indexesToRemove = new List<int>();
                
                foreach (BattleEntity t in preparedCombatants) {
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

                GetTurnOrder();
                
                int characterIndex = allCombatants.IndexOf(preparedCombatants[0]);
                if (preparedCombatants[0].isPlayer) {
                    state = BattleState.PlayerTurn;
                    StartCoroutine(PlayerTurnRoutine(characterIndex));
                    yield break;
                } else if (!preparedCombatants[0].isPlayer) {
                    state = BattleState.EnemyTurn;
                    StartCoroutine(EnemyTurnRoutine(characterIndex));
                    yield break;
                }
            } else {
                state = BattleState.Battle;
                StartCoroutine(BattleRoutine());
                yield break;
            }
        } else {
            print("Order Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator PlayerTurnRoutine(int characterIndex)
    {
        if (state == BattleState.PlayerTurn) {
            currentPlayer = characterIndex;
            if (!wentBack && !usedLightAction) {

                TriggerTurnStartTokens(allCombatants[characterIndex]);
                RemoveSelfTurnStartTokens(allCombatants[characterIndex]);
                allCombatants[currentPlayer].actionPoints -= TURN_START_THRESHOLD;
                preparedCombatants.RemoveAt(preparedCombatants.IndexOf(allCombatants[currentPlayer]));
                
                // Run character specific Methods
                switch (allCombatants[currentPlayer].myName) {
                    case "Bune":
                        yield return StartCoroutine(cowboyLogic.CowboyTurnStartLogic(allCombatants[currentPlayer]));
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
            yield break;
        }  else {
            print("Player Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator TargetRoutine()
    {
        if (state == BattleState.Targeting) {
            
            BattleEntity activeCharacter = allCombatants[currentPlayer];
            BattleEntity targetCharacter = null;
            Ability abilityInUse = activeCharacter.myAbilities[activeCharacter.activeAbility];
            
            int tempTarget;
            SetAbilityValuesForDisplay();
            switch (abilityInUse.abilityType) {
                case Ability.AbilityType.Damage:
                    targetIsEnemy = true;
                    
                    combatGrid.DisplayValidTiles(activeCharacter, abilityInUse.abilityType, targetIsEnemy, abilityInUse.targetSelf);
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    combatGrid.HideTiles(targetIsEnemy);
                    
                    tempTarget = activeCharacter.target;
                    if (targetBeingIndicated) {
                        StopIndicatingTarget(enemyCombatants.IndexOf(allCombatants[tempTarget]));
                    } else if (targetIndicatedGrid) {
                        foreach (GridTile tile in enemyBattleGrid) {
                            if (tile.occupiedBy == targetCharacter) {
                                var tileIndex = enemyBattleGrid.IndexOf(tile);
                                StopIndicatingGridTarget(tileIndex);
                            }
                        }
                    }
                    
                    yield return StartCoroutine(DamageAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case Ability.AbilityType.Heal:
                    targetIsEnemy = false;
                    
                    combatGrid.DisplayValidTiles(activeCharacter, abilityInUse.abilityType, targetIsEnemy, abilityInUse.targetSelf);
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    combatGrid.HideTiles(targetIsEnemy);
                    
                    tempTarget = activeCharacter.target;
                    if (targetBeingIndicated) {
                        StopIndicatingTarget(partyCombatants.IndexOf(allCombatants[tempTarget]));
                    } else if (targetIndicatedGrid) {
                        foreach (GridTile tile in partyBattleGrid) {
                            if (tile.occupiedBy == targetCharacter) {
                                var tileIndex = partyBattleGrid.IndexOf(tile);
                                StopIndicatingGridTarget(tileIndex);
                            }
                        }
                    }
                    
                    yield return StartCoroutine(HealAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case Ability.AbilityType.Buff:
                    targetIsEnemy = false;
                    
                    combatGrid.DisplayValidTiles(activeCharacter, abilityInUse.abilityType, targetIsEnemy, abilityInUse.targetSelf);
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    combatGrid.HideTiles(targetIsEnemy);
                    
                    tempTarget = activeCharacter.target;
                    if (targetBeingIndicated) {
                        StopIndicatingTarget(partyCombatants.IndexOf(allCombatants[tempTarget]));
                    } else if (targetIndicatedGrid) {
                        foreach (GridTile tile in partyBattleGrid) {
                            if (tile.occupiedBy == targetCharacter) {
                                var tileIndex = partyBattleGrid.IndexOf(tile);
                                StopIndicatingGridTarget(tileIndex);
                            }
                        }
                    }
                    
                    yield return StartCoroutine(BuffAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case Ability.AbilityType.Debuff:
                    targetIsEnemy = true;
                    
                    combatGrid.DisplayValidTiles(activeCharacter, abilityInUse.abilityType, targetIsEnemy, abilityInUse.targetSelf);
                    ShowTargetMenu(currentPlayer);
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                    targetCharacter = allCombatants[activeCharacter.target];
                    
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    combatGrid.HideTiles(targetIsEnemy);
                    
                    tempTarget = activeCharacter.target;
                    if (targetBeingIndicated) {
                        StopIndicatingTarget(enemyCombatants.IndexOf(allCombatants[tempTarget]));
                    } else if (targetIndicatedGrid) {
                        foreach (GridTile tile in enemyBattleGrid) {
                            if (tile.occupiedBy == targetCharacter) {
                                var tileIndex = enemyBattleGrid.IndexOf(tile);
                                StopIndicatingGridTarget(tileIndex);
                            }
                        }
                    }
                    
                    yield return StartCoroutine(DebuffAction(activeCharacter, targetCharacter, activeCharacter.activeAbility));
                    break;
                case Ability.AbilityType.Movement:
                    targetIsEnemy = false;
                    
                    combatGrid.DisplayValidTiles(activeCharacter, abilityInUse.abilityType, targetIsEnemy, false);
                    combatGrid.SetGridMovementButtons(targetIsEnemy);
                    targetSelected = false;
                    
                    // Line break variables
                    bool[] lineBrokenStatus = combatGrid.GetLineBreakInfo();
                    bool frontRowEmpty;
                    int frontMostRow;
                    
                    // define front-most row and determine if it is occupied
                    if (lineBrokenStatus[5]) {
                        frontRowEmpty = combatGrid.IsRowEmpty(7);
                        frontMostRow = 7;
                    } else if (lineBrokenStatus[4]) {
                        frontRowEmpty = combatGrid.IsRowEmpty(6);
                        frontMostRow = 6;
                    } else if (!lineBrokenStatus[4]) {
                        frontRowEmpty = combatGrid.IsRowEmpty(5);
                        frontMostRow = 5;
                    } else {
                        frontRowEmpty = false;
                        frontMostRow = 5;
                    }
                    // if front-most row is empty enable movement to enter enemy line to "break" it
                    if (frontRowEmpty) {
                        // allow played to select tiles in the front-most row in order to line break the row
                        combatGrid.DisplayValidRowBreakTiles(activeCharacter, frontMostRow);
                    }
                    
                    activeCharacter.combatMenuVisuals.ChangeAbilitySelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangePassButtonVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(true);
                    SetNoTargetButtons(currentPlayer);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(true);
                    
                    targetSelected = false;
                    yield return new WaitUntil(() => targetSelected);
                
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    combatGrid.HideTiles(targetIsEnemy);
                    if (brokeRow) {
                        combatGrid.HideTiles(!targetIsEnemy);
                        brokeRow = false;
                    }
                    
                    yield return StartCoroutine(SetGridPosition(allCombatants[currentPlayer], xChange, yChange));
                    break;
                default:
                    print("Unsupported ability type of " + allCombatants[currentPlayer].activeAbilityType + " supplied.");
                    activeCharacter.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
                    activeCharacter.combatMenuVisuals.ChangeBackButtonVisibility(false);
                    break;
            }

            if (abilityInUse.abilityWeight == Ability.AbilityWeight.Light) {
                if (activeCharacter.activeTokens.Any(t => t.tokenName == "Rush") &&
                    abilityInUse.abilityName == "Step") {
                    int tokenPosition = activeCharacter.activeTokens.FindIndex(t => t.tokenName == "Rush");
                    activeCharacter.activeTokens.RemoveAt(tokenPosition);
                    activeCharacter.battleVisuals.UpdateTokens(activeCharacter.activeTokens);
                } else {
                    usedLightAction = true;
                    int abilityIndex = activeCharacter.myAbilities.IndexOf(abilityInUse);
                    activeCharacter.abilityCooldowns[abilityIndex] += abilityInUse.cooldown;
                }
                BackToAbilities();
            } else {
                state  = BattleState.End;
                StartCoroutine(EndRoutine(activeCharacter));
                yield break;
            }
        } else {
            print("Target Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator EnemyTurnRoutine(int characterIndex)
    {
        if (state == BattleState.EnemyTurn) {
            BattleEntity activeEnemy = allCombatants[characterIndex];
            EnemyBrain myBrain = activeEnemy.enemyBrain;
            List<EnemyAbility> validAbilities = new List<EnemyAbility>();
            
            TriggerTurnStartTokens(allCombatants[characterIndex]);
            RemoveSelfTurnStartTokens(activeEnemy);
            activeEnemy.battleVisuals.SetMyTurnAnimation(true);
            activeEnemy.actionPoints -= TURN_START_THRESHOLD;
            preparedCombatants.RemoveAt(preparedCombatants.IndexOf(activeEnemy));
            int enemyFrontRow = enemyXMin;
            int playerFrontRow = playerXMax;
            
            yield return new WaitForSeconds(TURN_ACTION_DELAY);

            // Check is abilities have valid targets
            for (int i = 0; i < myBrain.enemyAbilities.Count; i++) {
                if (activeEnemy.abilityCooldowns[i] <= 0) {
                    if (usedLightAction &&
                        myBrain.enemyAbilities[i].ability.abilityWeight == Ability.AbilityWeight.Heavy) { continue; }
                    switch (myBrain.enemyAbilities[i].ability.abilityType) {
                        case Ability.AbilityType.Damage:
                        case Ability.AbilityType.Debuff:
                            foreach (BattleEntity entity in partyCombatants) {
                                int distance = CalculateTargetDistance(entity, activeEnemy);
                                if (distance <= myBrain.enemyAbilities[i].ability.rangeMax &&
                                    distance >= myBrain.enemyAbilities[i].ability.rangeMin &&
                                    entity.activeTokens.All(t => t.tokenName != "Stealth"))  {
                                    
                                    int brokenPlayerColumns = 0;
                                    bool[] lineBrokenStatus = combatGrid.GetLineBreakInfo();
                                    foreach (bool isBroken in lineBrokenStatus) {
                                        if (isBroken) {
                                            brokenPlayerColumns++;
                                        }
                                    }
                                    if (myBrain.enemyAbilities[i].ability.bannedColumns.Length > 0 &&
                                        myBrain.enemyAbilities[i].ability.bannedColumns.Any(t => (t + brokenPlayerColumns) == entity.xPos)) {
                                        continue;
                                    }
                                    validAbilities.Add(myBrain.enemyAbilities[i]);
                                }
                            }
                            break;
                        case Ability.AbilityType.Heal:
                        case Ability.AbilityType.Buff:
                            foreach (BattleEntity entity in enemyCombatants) {
                                int distance = CalculateTargetDistance(entity, activeEnemy);
                                if (distance <= myBrain.enemyAbilities[i].ability.rangeMax &&
                                    distance >= myBrain.enemyAbilities[i].ability.rangeMin) {
                                    validAbilities.Add(myBrain.enemyAbilities[i]);
                                }
                            }
                            break;
                        case Ability.AbilityType.Other: // Only the enemy LINE BREAK ability should be mapped to .Other for enemies
                            if (activeEnemy.xPos == enemyFrontRow && combatGrid.IsRowEmpty(playerFrontRow)) {
                                validAbilities.Add(myBrain.enemyAbilities[i]);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
            }
            
            // See if any moves have valid targets. If not, align with an opponent and move closer
            int priorityTotal = 0;
            int prevDistance = 0;
            BattleEntity closestOpponent = null;
            if (validAbilities.Count <= 0) {
                foreach (BattleEntity entity in partyCombatants) {
                    int distance = CalculateTargetDistance(entity, activeEnemy);
                    if (distance > prevDistance) {
                        closestOpponent = entity;
                        prevDistance = distance;
                    }
                }
                if (closestOpponent.yPos == activeEnemy.yPos) {
                    StartCoroutine(SetGridPosition(activeEnemy, -1, 0));
                } else if (closestOpponent.yPos > activeEnemy.yPos) {
                    StartCoroutine(SetGridPosition(activeEnemy, 0, 1));
                } else if (closestOpponent.yPos < activeEnemy.yPos) {
                    StartCoroutine(SetGridPosition(activeEnemy, 0, -1));
                }
                usedAbility = false;
            } else {
                for (int i = 0; i < validAbilities.Count; i++) {
                    priorityTotal += validAbilities[i].abilityPriority;
                }
                int enemyDecision = Random.Range(0, priorityTotal + 1);
                int abilityThreshold = 0;
                EnemyAbility abilityUsed = null;

                for (int i = 0; i < validAbilities.Count; i++) {
                    abilityThreshold += validAbilities[i].abilityPriority;
                    if (enemyDecision <= abilityThreshold) {
                        abilityUsed = validAbilities[i];
                        activeEnemy.activeAbility = myBrain.enemyAbilities.IndexOf(abilityUsed);
                        break;
                    }
                }
                if (abilityUsed is null) {
                    print("No ability selected");
                    yield break;
                }

                bool targetingFoes = true;
                targetList.Clear();
                switch (abilityUsed.ability.abilityType) {
                    case Ability.AbilityType.Damage:
                    case Ability.AbilityType.Debuff:
                        foreach (BattleEntity entity in partyCombatants) {
                            int distance = CalculateTargetDistance(entity, activeEnemy);
                            if (distance <= abilityUsed.ability.rangeMax && distance >= abilityUsed.ability.rangeMin &&
                                entity.activeTokens.All(t => t.tokenName != "Stealth")) {
                                targetList.Add(entity);
                            }
                        }
                        targetingFoes = true;
                        break;
                    case Ability.AbilityType.Heal:
                    case Ability.AbilityType.Buff:
                        foreach (BattleEntity t in enemyCombatants) {
                            int distance = CalculateTargetDistance(t, activeEnemy);
                            if (distance <= abilityUsed.ability.rangeMax && distance >= abilityUsed.ability.rangeMin) {
                                targetList.Add(t);
                            }
                        }
                        targetingFoes = false;
                        break;
                    case Ability.AbilityType.Other:
                        combatGrid.LineBreak(playerFrontRow);
                        for (int i = 0; i < abilityUsed.ability.selfTokensApplied.Length; i++) {
                            AddTokens(activeEnemy, activeEnemy, abilityUsed.ability.selfTokensApplied[i].ToString(), 
                                abilityUsed.ability.selfTokenCountApplied[i], 0);
                        }
                        state  = BattleState.End;
                        StartCoroutine(EndRoutine(activeEnemy));
                        yield break;
                        
                }

                BattleEntity abilityTarget = null;
                bool hasTaunt = false;
                if (targetingFoes) {
                    foreach (BattleEntity entity in targetList.Where(entity => entity.activeTokens.Any(t => t.tokenName == "Taunt"))) {
                        abilityTarget = entity;
                        hasTaunt = true;
                        break;
                    }
                }
                if (!hasTaunt) {
                    if (Random.Range(0, 21) < abilityUsed.randomChance ||
                        abilityUsed.targetMethod == EnemyBrain.TargetMethod.Random) {
                    
                        int abilityTargetIndex = Random.Range(0, targetList.Count);
                        abilityTarget = targetList[abilityTargetIndex];
                    } else {
                        bool targetLowest;
                        switch (abilityUsed.targetMethod) {
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

                        switch (abilityUsed.targetQualifier) {
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
                                print("Qualifier of " + abilityUsed.targetQualifier +
                                      " supplied");
                                abilityTarget = targetList[Random.Range(0, targetList.Count)];
                                // The below is sort lowest to highest
                                turnOrder.Sort((bi1, bi2) => bi1.ticksToTurn.CompareTo(bi2.ticksToTurn));
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            
                yield return new WaitForSeconds(TURN_ACTION_DELAY);
                switch (abilityUsed.ability.abilityType) {
                    case Ability.AbilityType.Damage:
                        yield return StartCoroutine(DamageAction(activeEnemy, abilityTarget, myBrain.enemyAbilities.IndexOf(abilityUsed)));
                        break;
                    case Ability.AbilityType.Debuff:
                        yield return StartCoroutine(DebuffAction(activeEnemy, abilityTarget, myBrain.enemyAbilities.IndexOf(abilityUsed)));
                        break;
                    case Ability.AbilityType.Heal:
                        yield return StartCoroutine(HealAction(activeEnemy, abilityTarget, myBrain.enemyAbilities.IndexOf(abilityUsed)));
                        break;
                    case Ability.AbilityType.Buff:
                        yield return StartCoroutine(BuffAction(activeEnemy, abilityTarget, myBrain.enemyAbilities.IndexOf(abilityUsed)));
                        break;
                    default:
                        print("Invalid ability type of " + abilityUsed.ability.abilityType +
                              " called.");
                        yield break;
                }
            }
            
            state  = BattleState.End;
            StartCoroutine(EndRoutine(activeEnemy));
            yield break;
        } else {
            print("Enemy Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator EndRoutine(BattleEntity activeEntity)
    {
        // Call character turn end logic
        if (activeEntity.myName == "Renée") {
            repentantLogic.AscensionMax(activeEntity);
        }
        
        // If a move action was used, turn off move buttons
        if (allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility].abilityType == Ability.AbilityType.Movement
            && allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility].abilityWeight != Ability.AbilityWeight.Light) {
            combatGrid.DisableGridButtons(targetIsEnemy);
        }
        
        // Trigger Ailments
        if (activeEntity.activeTokens.Any(t => t.tokenType == Token.TokenType.Ailments)) {
            yield return StartCoroutine(TriggerAilments(activeEntity));
        }
        
        // Reduce Cooldowns of all unused abilities by one
        for (int i = 0; i < activeEntity.abilityCooldowns.Count; i++) {
            if (activeEntity.abilityCooldowns[i] > 0) {
                activeEntity.abilityCooldowns[i] -= 1;
            }
        }
        // Start the cooldown of the used ability
        if (usedAbility) {
            activeEntity.abilityCooldowns[activeEntity.activeAbility] = activeEntity.myAbilities[activeEntity.activeAbility].cooldown;
        }
            
        // Reset turn-related values
        activeEntity.battleVisuals.SetMyTurnAnimation(false);
        wentBack = false;
        usedLightAction = false;
        usedAbility = true;
        abilityDuplicated = false;
        extraCastCount = 0;
        duplicationType = DuplicationType.None;
        activeEntity.wasDamagedLastTurn = false;
        activeEntity.damagedBy = 100;

        if (activeEntity.myFirstTurn) {
            activeEntity.myFirstTurn = false;
        }
            
        state = BattleState.Battle;
        StartCoroutine(BattleRoutine());
        yield break;
    }
    
    private void CreatePartyEntities()
    {
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetCurrentParty();
        
        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntity tempEntity = new BattleEntity();
            
            tempEntity.SetEntityValue(currentParty[i].memberName, currentParty[i].memberPortrait, currentParty[i].level,
                currentParty[i].xPos, currentParty[i].yPos, currentParty[i].maxHealth, currentParty[i].currentHealth,
                currentParty[i].maxSpirit, currentParty[i].currentSpirit, currentParty[i].maxDefense, currentParty[i].maxArmor,
                currentParty[i].power, currentParty[i].skill, currentParty[i].wit, currentParty[i].mind, currentParty[i].speed,
                currentParty[i].luck, currentParty[i].stunResist, currentParty[i].debuffResist, currentParty[i].ailmentResist, true);
            
            // Get grid starting position
            int gridSpawn = GetGridPosition(tempEntity);

            GameObject tempVisualGameObject = Instantiate(currentParty[i].allyBattleVisualPrefab,
                partyBattleGrid[gridSpawn].gridTransform.position, Quaternion.identity);
            partyBattleGrid[gridSpawn].isOccupied = true;
            partyBattleGrid[gridSpawn].occupiedBy = tempEntity;
            BattleVisuals tempBattleVisuals =  tempVisualGameObject.GetComponent<BattleVisuals>();
            CombatMenuVisuals tempCombatMenuVisuals = Instantiate(currentParty[i].allyMenuVisualPrefab, Vector2.zero,
                Quaternion.identity).GetComponent<CombatMenuVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentParty[i].maxHealth, currentParty[i].currentHealth, currentParty[i].maxDefense, currentParty[i].maxArmor);
            tempCombatMenuVisuals.SetMyEntity(tempEntity);
            tempCombatMenuVisuals.SetMenuStartingValues(currentParty[i].maxSpirit, currentParty[i].currentSpirit);
            // Assign said visuals to the battle entity
            tempEntity.myVisuals = tempVisualGameObject;
            tempEntity.battleVisuals = tempBattleVisuals;
            tempEntity.combatMenuVisuals = tempCombatMenuVisuals;
            tempEntity.targetButtons = tempEntity.combatMenuVisuals.GetTargetButtons();
            tempEntity.targetPortraits = tempEntity.combatMenuVisuals.GetTargetPortraits();
            tempEntity.targetBorders = tempEntity.combatMenuVisuals.GetTargetBorders();
            
            // Assign abilities to character TODO Make this also update visuals
            tempEntity.myAbilities = partyManager.GetActiveAbilities(i);
            tempEntity.abilityCooldowns = new List<int>();
            for (int j = 0; j < tempEntity.myAbilities.Count; j++) {
                tempEntity.abilityCooldowns.Add(0);
            }
            
            // Assign Line Break token
            tempEntity.lineBreakToken = currentParty[i].lineBreakToken;
            tempEntity.lineBreakTokenCount = currentParty[i].lineBreakTokenCount;

            FindMyGridPosition(tempEntity);
            
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
            BattleEntity tempEntity = new BattleEntity();
            
            tempEntity.SetEntityValue(currentEnemies[i].enemyName, currentEnemies[i].enemyPortrait, currentEnemies[i].level,
                currentEnemies[i].xPos, currentEnemies[i].yPos, currentEnemies[i].maxHealth, currentEnemies[i].currentHealth,
                currentEnemies[i].maxSpirit, currentEnemies[i].currentSpirit, currentEnemies[i].maxDefense, currentEnemies[i].maxArmor,
                currentEnemies[i].power, currentEnemies[i].skill, currentEnemies[i].wit, currentEnemies[i].mind, currentEnemies[i].speed,
                currentEnemies[i].luck, currentEnemies[i].stunResist, currentEnemies[i].debuffResist, currentEnemies[i].ailmentResist, false);

            tempEntity.SetEnemyBrain(currentEnemies[i].enemyBrain);
            
            // Get grid starting position
            int gridSpawn = GetGridPosition(tempEntity);
            
            // Spawn the visuals
            GameObject tempVisualGameObject = Instantiate(currentEnemies[i].enemyVisualPrefab,
                enemyBattleGrid[gridSpawn].gridTransform.position, Quaternion.identity);
            enemyBattleGrid[gridSpawn].isOccupied = true;
            enemyBattleGrid[gridSpawn].occupiedBy = tempEntity;
            BattleVisuals tempBattleVisuals =  tempVisualGameObject.GetComponent<BattleVisuals>();
            
            // Set the visuals' starting values
            tempBattleVisuals.SetStartingValues(currentEnemies[i].maxHealth, currentEnemies[i].currentHealth, currentEnemies[i].maxDefense, currentEnemies[i].maxArmor);
            // Assign said visuals to the battle entity
            tempEntity.myVisuals = tempVisualGameObject;
            tempEntity.battleVisuals = tempBattleVisuals;
            // Give the enemy their abilities
            tempEntity.myAbilities = enemyManager.GetAbilities(i);
            tempEntity.abilityCooldowns = new List<int>();
            for (int j = 0; j < tempEntity.myAbilities.Count; j++) {
                tempEntity.abilityCooldowns.Add(0);
            }
            
            FindMyGridPosition(tempEntity);
            
            // Add the allied combatant to the all combatants and party combatant lists
            allCombatants.Add(tempEntity);
            enemyCombatants.Add(tempEntity);
        }
    }

    private void InitializeBattleTokens()
    {
        List<Token> currentTokens = new List<Token>();
        currentTokens = tokenManager.GetTokenInfo();

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
        guardColumnToken = allTokens.SingleOrDefault(obj => obj.tokenName == "GuardColumn");
        guardRowToken = allTokens.SingleOrDefault(obj => obj.tokenName == "GuardRow");
        hasteToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Haste");
        pierceToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Pierce");
        precisionToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Precision");
        //quickToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Quick");
        ricochetToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Ricochet");
        riposteToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Riposte");
        rushToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Rush");
        stealthToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Stealth");
        tauntToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Taunt");
        wardToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Ward");
        
        // Set debuff tokens
        antiHealToken = allTokens.SingleOrDefault(obj => obj.tokenName == "AntiHeal");
        blindToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Blind");
        breakToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Break");
        //delayToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Delay");
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
        killseekerToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Killseeker");
        viceToken = allTokens.SingleOrDefault(obj => obj.tokenName == "Vice");
        
        // Set ailment counters
        bleedCounter = allTokens.SingleOrDefault(obj => obj.tokenName == "Bleed");
        burnCounter = allTokens.SingleOrDefault(obj => obj.tokenName == "Burn");
        poisonCounter = allTokens.SingleOrDefault(obj => obj.tokenName == "Poison");
    }

    public BattleToken GetTokenIdentity(string tokenName)
    {
        BattleToken tempToken = allTokens.SingleOrDefault(obj => obj.tokenName == tokenName);
        return tempToken;
    }

    private void GetTurnOrder()
    {
        turnOrder.Clear();
        for (int i = 0; i < MAX_INDIVIDUAL_DISPLAY; i++){
            foreach (BattleEntity t in allCombatants) {
                BattleEntity tempEntity = new BattleEntity();
                List<BattleToken> tempTokensList = new List<BattleToken>(t.activeTokens);
                tempEntity.SetEntityTurnDisplayValues(t.myName, t.myPortrait, t.isPlayer, t.speed, t.actionPoints, tempTokensList);
                tempEntity.actionPoints -= (200 * i);
                turnOrder.Add(tempEntity);
            }
        }

        // TODO Make this turn display change based on the number of Haste & Slow tokens you have, versus being a binary have/don't have
        foreach (BattleEntity entity in turnOrder) {
            float actionPointGain = 0f;
            if (entity.activeTokens.Any(t => t.tokenName == "Haste")) {
                actionPointGain = (BASE_ACTION_GAIN + entity.speed) * (1 + hasteToken.tokenValue);
            } else if (entity.activeTokens.Any(t => t.tokenName == "Slow")) {
                actionPointGain = (BASE_ACTION_GAIN + entity.speed) * (1 - slowToken.tokenValue);
            } else {
                actionPointGain = (BASE_ACTION_GAIN + entity.speed);
            }
            float tickDifference = (TURN_START_THRESHOLD - entity.actionPoints) / actionPointGain;
            entity.ticksToTurn = tickDifference;
        }
        turnOrder.Sort((bi1, bi2) => bi1.ticksToTurn.CompareTo(bi2.ticksToTurn));
        
        turnOrderDisplay.SetTurnDisplay(turnOrder);
    }

    private void PreviewTurnOrder()
    {
        // TODO Make this turn display change based on the number of Haste & Slow tokens you have, versus being a binary have/don't have
        for (int i = 1; i < turnOrder.Count; i++) // This for loop starts at 1 so the active player will not be affected
        {
            float actionPointGain = 0f;
            if (turnOrder[i].activeTokens.Any(t => t.tokenName == "Haste")) {
                actionPointGain = (BASE_ACTION_GAIN + turnOrder[i].speed) * (1 + hasteToken.tokenValue);
            } else if (turnOrder[i].activeTokens.Any(t => t.tokenName == "Slow")) {
                actionPointGain = (BASE_ACTION_GAIN + turnOrder[i].speed) * (1 - slowToken.tokenValue);
            } else {
                actionPointGain = (BASE_ACTION_GAIN + turnOrder[i].speed);
            }
            float tickDifference = (TURN_START_THRESHOLD - turnOrder[i].actionPoints) / actionPointGain;
            turnOrder[i].ticksToTurn = tickDifference;
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

    private int GetGridPosition(BattleEntity entity)
    {
        switch (entity.yPos) {
            case 1:
                switch (entity.xPos) {
                    case 1:
                        return 0;
                    case 2:
                        return 1;
                    case 3:
                        return 2;
                    case 4:
                        return 3;
                    case 5:
                        return 0;
                    case 6:
                        return 1;
                    case 7:
                        return 2;
                    case 8:
                        return 3;
                    default:
                        break;
                }
                break;
            case 2:
                switch (entity.xPos) {
                    case 1:
                        return 4;
                    case 2:
                        return 5;
                    case 3:
                        return 6;
                    case 4:
                        return 7;
                    case 5:
                        return 4;
                    case 6:
                        return 5;
                    case 7:
                        return 6;
                    case 8:
                        return 7;
                    default:
                        break;
                }
                break;
            case 3:
                switch (entity.xPos) {
                    case 1:
                        return 8;
                    case 2:
                        return 9;
                    case 3:
                        return 10;
                    case 4:
                        return 11;
                    case 5:
                        return 8;
                    case 6:
                        return 9;
                    case 7:
                        return 10;
                    case 8:
                        return 11;
                    default:
                        break;
                }
                break;
            case 4:
                switch (entity.xPos) {
                    case 1:
                        return 12;
                    case 2:
                        return 13;
                    case 3:
                        return 14;
                    case 4:
                        return 15;
                    case 5:
                        return 12;
                    case 6:
                        return 13;
                    case 7:
                        return 14;
                    case 8:
                        return 15;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        print("Invalid position: " + entity.xPos + ", " +  entity.yPos);
        return 0;
    }

    public void MoveOrTargetCheck(int positionIndex)
    {
        if (allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility].abilityType ==
            Ability.AbilityType.Movement) {
            if (positionIndex > 15) {
                int rowTarget = enemyBattleGrid[positionIndex - 16].xPos;
                combatGrid.LineBreak(rowTarget);
                brokeRow = true;
                FindMoveDistance(positionIndex);
                xChange -= 1;
                AddTokens(allCombatants[currentPlayer], allCombatants[currentPlayer], 
                    allCombatants[currentPlayer].lineBreakToken.tokenName, allCombatants[currentPlayer].lineBreakTokenCount, 
                    0);
                targetSelected = true;
            } else {
                FindMoveDistance(positionIndex);
            }
        } else {
            bool isTargetingEnemies = true;
            switch (allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility].abilityType) {
                case Ability.AbilityType.Damage:
                case Ability.AbilityType.Debuff:
                    isTargetingEnemies = true;
                    break;
                case Ability.AbilityType.Heal:
                case Ability.AbilityType.Buff:
                    isTargetingEnemies = false;
                    break;
            }

            if (isTargetingEnemies) {
                positionIndex -= 16;
            }
            SelectTargetWithGrid(positionIndex, isTargetingEnemies);
        }
    }

    private int CalculateTargetDistance(BattleEntity targetEntity, BattleEntity activeEntity)
    {
        int xDistance = Math.Abs(targetEntity.xPos - activeEntity.xPos);
        int yDistance = Math.Abs(targetEntity.yPos - activeEntity.yPos);
        
        bool[] lineBrokenStatus = combatGrid.GetLineBreakInfo();

        int linesToIgnore = 0;
        if (activeEntity.xPos < targetEntity.xPos) {
            for (int i = activeEntity.xPos; i < targetEntity.xPos; i++) {
                if (lineBrokenStatus[i]) {
                    linesToIgnore++;
                }
            }
        } else {
            for (int i = targetEntity.xPos; i < activeEntity.xPos; i++) {
                if (lineBrokenStatus[i]) {
                    linesToIgnore++;
                }
            }
        }
        xDistance -=  linesToIgnore;
        
        int distance = xDistance + yDistance;

        return distance;
    }

    public int CalculateTileDistance(GridTile targetTile, BattleEntity activeEntity)
    {
        int xDistance = Math.Abs(targetTile.xPos - activeEntity.xPos);
        int yDistance = Math.Abs(targetTile.yPos - activeEntity.yPos);
        
        bool[] lineBrokenStatus = combatGrid.GetLineBreakInfo();

        int linesToIgnore = 0;
        if (activeEntity.xPos < targetTile.xPos) {
            for (int i = activeEntity.xPos; i < targetTile.xPos; i++) {
                if (lineBrokenStatus[i]) {
                    linesToIgnore++;
                }
            }
        } else {
            for (int i = targetTile.xPos; i < activeEntity.xPos; i++) {
                if (lineBrokenStatus[i]) {
                    linesToIgnore++;
                }
            }
        }
        xDistance -=  linesToIgnore;
        
        int distance = xDistance + yDistance;

        return distance;
    }
    
    private void FindMoveDistance(int positionIndex)
    {
        if (positionIndex > 15) {
            GridTile chosenTile = enemyBattleGrid[positionIndex - 16];
            GridTile currentTile = null;
            for (int i = 0; i < partyBattleGrid.Count; i++) {
                if (!partyBattleGrid[i].isOccupied) continue;
                if (partyBattleGrid[i].occupiedBy.myName == allCombatants[currentPlayer].myName) {
                    currentTile = partyBattleGrid[i];
                    break;
                }
            }
            
            if (currentTile == null) {
                print("Current tile not found");
                return;
            }
            
            xChange = (chosenTile.xPos - currentTile.xPos);
            yChange = (chosenTile.yPos - currentTile.yPos);
            print(xChange + " " + yChange);
        } else {
            GridTile chosenTile = partyBattleGrid[positionIndex];
            GridTile currentTile = null;
            for (int i = 0; i < partyBattleGrid.Count; i++) {
                if (!partyBattleGrid[i].isOccupied) continue;
                if (partyBattleGrid[i].occupiedBy.myName == allCombatants[currentPlayer].myName) {
                    currentTile = partyBattleGrid[i];
                    break;
                }
            }

            if (currentTile == null) {
                print("Current tile not found");
                return;
            }

            xChange = (chosenTile.xPos - currentTile.xPos);
            yChange = (chosenTile.yPos - currentTile.yPos);

            targetSelected = true;
        }
        
    }

    private IEnumerator SetGridPosition(BattleEntity entity, int xMove, int yMove)
    {
        int oldGridPos = GetGridPosition(entity);
        int newGridPos;
        Vector3 oldPos;
        Vector3 newPos;
        
        if (entity.isPlayer) {
            oldPos = partyBattleGrid[oldGridPos].gridTransform.position;
            entity.xPos += xMove;
            entity.yPos += yMove;

            if (entity.xPos > playerXMax) {
                entity.xPos = playerXMax;
            } else if (entity.xPos < playerXMin) {
                entity.xPos = playerXMin;
            }
            if (entity.yPos > yMax) {
                entity.yPos = yMax;
            } else if (entity.yPos < yMin) {
                entity.yPos = yMin;
            }
                
            newGridPos = GetGridPosition(entity);

            if (partyBattleGrid[newGridPos].isOccupied && partyBattleGrid[newGridPos].occupiedBy != entity) {
                BattleEntity movePartner = partyBattleGrid[newGridPos].occupiedBy;
                
                // Swap spaces with the target if they are not immobilized
                if (movePartner.activeTokens.All(t => t.tokenName != "Restrict")) {
                    newPos = partyBattleGrid[newGridPos].gridTransform.position;
                    
                    if (xMove > 0) {
                        movePartner.xPos -= 1;
                    } else if (xMove < 0) {
                        movePartner.xPos += 1;
                    } else if (yMove > 0) {
                        movePartner.yPos -= 1;
                    } else if (yMove < 0) {
                        movePartner.yPos += 1;
                    }

                    int partnerNewGridPos = GetGridPosition(movePartner);
                    Vector3 partnerNewPos = partyBattleGrid[partnerNewGridPos].gridTransform.position;
                    
                    StartCoroutine(MoveToPosition(movePartner, newPos, partnerNewPos));

                    if (partnerNewPos != oldPos) {
                        partyBattleGrid[oldGridPos].isOccupied = false;
                        partyBattleGrid[oldGridPos].occupiedBy = null;
                        partyBattleGrid[partnerNewGridPos].isOccupied = true;
                        partyBattleGrid[partnerNewGridPos].occupiedBy = movePartner;
                    } else {
                        partyBattleGrid[oldGridPos].isOccupied = true;
                        partyBattleGrid[oldGridPos].occupiedBy = movePartner;
                    }
                    partyBattleGrid[newGridPos].isOccupied = true;
                    partyBattleGrid[newGridPos].occupiedBy = entity;
                } else {
                    newPos = oldPos;
                    newGridPos = oldGridPos;
                    partyBattleGrid[newGridPos].isOccupied = true;
                    partyBattleGrid[newGridPos].occupiedBy = entity;
                }
            } else {
                newPos = partyBattleGrid[newGridPos].gridTransform.position;
                partyBattleGrid[oldGridPos].isOccupied = false;
                partyBattleGrid[oldGridPos].occupiedBy = null;
                partyBattleGrid[newGridPos].isOccupied = true;
                partyBattleGrid[newGridPos].occupiedBy = entity;
            }
        } else {
            oldPos = enemyBattleGrid[oldGridPos].gridTransform.position;
            entity.xPos += xMove;
            entity.yPos += yMove;
            
            if (entity.xPos > enemyXMax) {
                entity.xPos = enemyXMax;
            } else if (entity.xPos < enemyXMin) {
                entity.xPos = enemyXMin;
            }
            if (entity.yPos > yMax) {
                entity.yPos = yMax;
            } else if (entity.yPos < yMin) {
                entity.yPos = yMin;
            }
                
            newGridPos = GetGridPosition(entity);

            if (enemyBattleGrid[newGridPos].isOccupied && enemyBattleGrid[newGridPos].occupiedBy != entity) {
                BattleEntity movePartner = enemyBattleGrid[newGridPos].occupiedBy;
                
                // Swap spaces with the target if they are not immobilized
                if (movePartner.activeTokens.All(t => t.tokenName != "Restrict")) {
                    newPos = enemyBattleGrid[newGridPos].gridTransform.position;
                    
                    if (xMove > 0) {
                        movePartner.xPos -= 1;
                    } else if (xMove < 0) {
                        movePartner.xPos += 1;
                    } else if (yMove > 0) {
                        movePartner.yPos -= 1;
                    } else if (yMove < 0) {
                        movePartner.yPos += 1;
                    }

                    int partnerNewGridPos = GetGridPosition(movePartner);
                    Vector3 partnerNewPos = enemyBattleGrid[partnerNewGridPos].gridTransform.position;
                    
                    StartCoroutine(MoveToPosition(movePartner, newPos, partnerNewPos));

                    if (partnerNewPos != oldPos) {
                        enemyBattleGrid[oldGridPos].isOccupied = false;
                        enemyBattleGrid[oldGridPos].occupiedBy = null;
                        enemyBattleGrid[partnerNewGridPos].isOccupied = true;
                        enemyBattleGrid[partnerNewGridPos].occupiedBy = movePartner;
                    } else {
                        enemyBattleGrid[oldGridPos].isOccupied = true;
                        enemyBattleGrid[oldGridPos].occupiedBy = movePartner;
                    }
                    enemyBattleGrid[newGridPos].isOccupied = true;
                    enemyBattleGrid[newGridPos].occupiedBy = entity;
                } else {
                    newPos = oldPos;
                    newGridPos = oldGridPos;
                    enemyBattleGrid[newGridPos].isOccupied = true;
                    enemyBattleGrid[newGridPos].occupiedBy = entity;
                }
            } else {
                newPos = enemyBattleGrid[newGridPos].gridTransform.position;
                enemyBattleGrid[oldGridPos].isOccupied = false;
                enemyBattleGrid[oldGridPos].occupiedBy = null;
                enemyBattleGrid[newGridPos].isOccupied = true;
                enemyBattleGrid[newGridPos].occupiedBy = entity;
            }
        }
        StartCoroutine(MoveToPosition(entity, oldPos, newPos));
        yield break;
    }

    public IEnumerator MoveToPosition(BattleEntity entity, Vector3 startPos, Vector3 endPos)
    {
        entity.myVisuals.transform.position = endPos;
        
        /*bool isMoving = true;
        while (isMoving) {
            entity.myVisuals.transform.position = Vector3.MoveTowards(startPos, endPos, MOVE_SPEED * Time.deltaTime);
            
            if (entity.myVisuals.transform.position == endPos) {
                isMoving = false;
            }
        }*/

        yield break;
    }

    public void FindMyGridPosition(BattleEntity entity)
    {
        switch (entity.yPos) {
            case 4:
                entity.battleVisuals.SetMyOrder(1);
                break;
            case 3:
                entity.battleVisuals.SetMyOrder(2);
                break;
            case 2:
                entity.battleVisuals.SetMyOrder(3);
                break;
            case 1:
                entity.battleVisuals.SetMyOrder(4);
                break;
        }
        
        if (entity.isPlayer) {
            foreach (GridTile tile in partyBattleGrid) {
                if (tile.xPos == entity.xPos && tile.yPos > entity.yPos && tile.isOccupied) {
                    entity.battleVisuals.SetSharedRowAnimation(true);
                    return;
                }
            }
            entity.battleVisuals.SetSharedRowAnimation(false);
            return;
        } else {
            // TODO add transparency to enemies with enemies above them
            foreach (GridTile tile in enemyBattleGrid) {
                if (tile.xPos == entity.xPos && tile.yPos > entity.yPos && tile.isOccupied) {
                    //entity.battleVisuals.SetSharedRowAnimation(true);
                    return;
                }
            }
            //entity.battleVisuals.SetSharedRowAnimation(false);
            return;
        }
    }
    
    public void ShowAbilitySelectMenu(int characterIndex)
    {
        // Set whose turn it is
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilitySelectUIVisibility(true);
        allCombatants[characterIndex].combatMenuVisuals.ChangePassButtonVisibility(true);
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilityEffectTextVisibility(true);
    }
    
    public void ShowTargetMenu(int characterIndex)
    {
        allCombatants[characterIndex].combatMenuVisuals.ChangeAbilitySelectUIVisibility(false);
        allCombatants[characterIndex].combatMenuVisuals.ChangePassButtonVisibility(false);
        SetTargetButtons(characterIndex);
        allCombatants[characterIndex].combatMenuVisuals.ChangeTargetSelectUIVisibility(true);
        allCombatants[characterIndex].combatMenuVisuals.ChangeBackButtonVisibility(true);
    }
    
    private void SetAbilityBar()
    {
        foreach (BattleEntity player in partyCombatants) {
            player.combatMenuVisuals.SetMyAbilityBar();
        }
    }

    private void UpdateAbilityBar(int characterIndex)
    {
        BattleEntity player = partyCombatants[characterIndex];
        List<Image> abilityImages = player.combatMenuVisuals.GetAbilityImages();
        List<Button> abilityButtons = player.combatMenuVisuals.GetAbilityButtons();
        
        for (int i = 0; i < player.myAbilities.Count; i++) {
            // Character ability blocking logic
            if (player.myName == "Renée") {
                if (repentantLogic.RepentantUseLogic(player, player.myAbilities[i])) {
                    abilityImages[i].color = new Color(40,40,40);
                    abilityButtons[i].interactable = false;
                    continue;
                }
            } else if (player.myName == "Bune") {
                if (cowboyLogic.CowboyUseLogic(player, player.myAbilities[i])) {
                    abilityImages[i].color = new Color(40,40,40);
                    abilityButtons[i].interactable = false;
                    continue;
                }
            }
            if (player.abilityCooldowns[i] > 0 || player.myAbilities[i].abilityWeight == Ability.AbilityWeight.Heavy && usedLightAction || 
                player.myAbilities[i].abilityWeight == Ability.AbilityWeight.Light && usedLightAction) {
                
                abilityImages[i].color = new Color(40,40,40);
                abilityButtons[i].interactable = false;
            } else {
                int brokenPlayerColumns = 0;
                bool[] lineBrokenStatus = combatGrid.GetLineBreakInfo();
                foreach (bool isBroken in lineBrokenStatus) {
                    if (isBroken) {
                        brokenPlayerColumns++;
                    }
                }
                if (player.myAbilities[i].bannedColumns.Length > 0 &&
                    player.myAbilities[i].bannedColumns.Any(t => (t - brokenPlayerColumns) == player.xPos)) {
                    abilityImages[i].color = new Color(40,40,40);
                    abilityButtons[i].interactable = false;
                    continue;
                }
                abilityImages[i].color = new Color(255,255,255);
                abilityButtons[i].interactable = true;
            }

            switch (player.myAbilities[i].costResource.ToString()) {
                case "Null":
                    break;
                case "Spirit":
                    if (player.currentSpirit < player.myAbilities[i].costAmount) {
                        abilityImages[i].color = new Color(40,40,40);
                        abilityButtons[i].interactable = false;
                    }
                    break;
                case "Health":
                    if (player.currentHealth < player.myAbilities[i].costAmount) {
                        abilityImages[i].color = new Color(40,40,40);
                        abilityButtons[i].interactable = false;
                    }
                    break;
                case "Defense":
                    if (player.currentDefense < player.myAbilities[i].costAmount) {
                        abilityImages[i].color = new Color(40,40,40);
                        abilityButtons[i].interactable = false;
                    }
                    break;
                case "SelfDmg":
                    break;
                case "Armor":
                    if (player.currentArmor < player.myAbilities[i].costAmount) {
                        abilityImages[i].color = new Color(40,40,40);
                        abilityButtons[i].interactable = false;
                    }
                    break;
                case "Special":
                    print("Special resource type was called but isn't programmed in yet.");
                    break;
                default:
                    print("Invalid resource of " +  player.myAbilities[i].costResource + " supplied");
                    break;
            }
            
            if (player.myAbilities[i].abilityType == Ability.AbilityType.Movement &&
                player.activeTokens.Any(t => t.tokenName == "Restrict"))
            {
                abilityImages[i].color = new Color(40,40,40);
                abilityButtons[i].interactable = false;
            }
        }
    }

    public string SetAbilityDescription(int abilityIndex)
    {
        BattleEntity currentPlayerEntity = allCombatants[currentPlayer];
        return currentPlayerEntity.myAbilities[abilityIndex].description;
    }

    public void PreviewResourceValue(int abilityIndex)
    {
        BattleEntity currentPlayerEntity = allCombatants[currentPlayer];
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
                if (tempInt > 0) { tempInt = 0;}
                currentPlayerEntity.battleVisuals.ChangeHealth(tempInt);
                break;
            case "Defense":
                tempInt = currentPlayerEntity.currentDefense - currentPlayerEntity.myAbilities[abilityIndex].costAmount;
                if (tempInt > 0) { tempInt = 0;}
                currentPlayerEntity.battleVisuals.ChangeDefense(tempInt);
                break;
            case "SelfDmg":
                break;
            case "Armor":
                tempInt = currentPlayerEntity.currentArmor - currentPlayerEntity.myAbilities[abilityIndex].costAmount;
                if (tempInt > 0) { tempInt = 0;}
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
        BattleEntity currentPlayerEntity = allCombatants[currentPlayer];
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

    public void PreviewSelfGain(BattleEntity playerEntity)
    {
        Ability activeAbility = playerEntity.myAbilities[playerEntity.activeAbility];

        if (activeAbility.selfMin != activeAbility.selfMax) { return; }
        
        int tempInt;
        switch (activeAbility.selfTarget) {
            case Ability.SelfTarget.Spirit:
                tempInt = playerEntity.currentSpirit + activeAbility.costAmount;
                playerEntity.combatMenuVisuals.ChangeSpirit(tempInt);
                break;
            case Ability.SelfTarget.Armor:
                tempInt = playerEntity.currentArmor + activeAbility.selfMax;
                if (tempInt > playerEntity.maxArmor) {
                    playerEntity.battleVisuals.ChangeArmor(tempInt);
                }
                break;
            case Ability.SelfTarget.ActionPoints:
                List<BattleEntity> tempList = new List<BattleEntity>();
                foreach (BattleEntity t in turnOrder) {
                    if (t.myName == playerEntity.myName) {
                        tempList.Add(t);
                    }
                }
                ChangeAP(tempList, -activeAbility.selfMax);
                break;
            case Ability.SelfTarget.Null:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void EndSelfGainPreview(BattleEntity activeEntity)
    {
        Ability activeAbility = allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility];
        
        if (activeAbility.selfMin != activeAbility.selfMax) { return; }
        
        switch (activeAbility.selfTarget) {
            case Ability.SelfTarget.Spirit:
                activeEntity.combatMenuVisuals.ChangeSpirit(activeEntity.currentSpirit);
                break;
            case Ability.SelfTarget.Armor:
                activeEntity.battleVisuals.ChangeArmor(activeEntity.currentArmor);
                break;
            case Ability.SelfTarget.ActionPoints:
                List<BattleEntity> tempList = new List<BattleEntity>();
                foreach (BattleEntity t in turnOrder) {
                    if (t.myName == activeEntity.myName) {
                        tempList.Add(t);
                    }
                }
                ChangeAP(tempList, activeAbility.selfMax);
                break;
            case Ability.SelfTarget.Null:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void PreviewTargetResourceValue(BattleEntity targetEntity)
    {
        BattleEntity playerEntity = allCombatants[currentPlayer];
        Ability activeAbility = playerEntity.myAbilities[playerEntity.activeAbility];
        int tempInt;
        switch (activeAbility.secondaryTarget) {
            case Ability.SecondaryTarget.Spirit:
                if (targetEntity.isPlayer) {
                    tempInt = targetEntity.currentSpirit - activeAbility.costAmount;
                    if (tempInt > 0) { tempInt = 0;}
                    targetEntity.combatMenuVisuals.ChangeSpirit(tempInt);
                }
                break;
            case Ability.SecondaryTarget.Armor:
                tempInt = targetEntity.currentArmor - activeAbility.secondaryValue;
                if (tempInt > 0) { tempInt = 0;}
                targetEntity.battleVisuals.ChangeArmor(tempInt);
                break;
            case Ability.SecondaryTarget.ActionPoints:
                List<BattleEntity> tempList = new List<BattleEntity>();
                foreach (BattleEntity t in turnOrder) {
                    if (t.myName == targetEntity.myName) {
                        tempList.Add(t);
                    }
                }
                switch (activeAbility.abilityType) {
                    case Ability.AbilityType.Damage:
                    case Ability.AbilityType.Debuff:
                        ChangeAP(tempList, activeAbility.secondaryValue);
                        break;
                    case Ability.AbilityType.Heal:
                    case Ability.AbilityType.Buff:
                        ChangeAP(tempList, -activeAbility.secondaryValue);
                        break;
                }
                break;
            case Ability.SecondaryTarget.Null:
            case Ability.SecondaryTarget.Bonus:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void EndTargetResourcePreview(BattleEntity targetEntity)
    {
        Ability activeAbility = allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility];
        switch (activeAbility.secondaryTarget) {
            case Ability.SecondaryTarget.Spirit:
                if (targetEntity.isPlayer) {
                    targetEntity.combatMenuVisuals.ChangeSpirit(targetEntity.currentSpirit);
                }
                break;
            case Ability.SecondaryTarget.Armor:
                targetEntity.battleVisuals.ChangeArmor(targetEntity.currentArmor);
                break;
            case Ability.SecondaryTarget.ActionPoints:
                List<BattleEntity> tempList = new List<BattleEntity>();
                foreach (BattleEntity t in turnOrder) {
                    if (t.myName == targetEntity.myName) {
                        tempList.Add(t);
                    }
                }
                switch (activeAbility.abilityType) {
                    case Ability.AbilityType.Damage:
                    case Ability.AbilityType.Debuff:
                        ChangeAP(tempList, -activeAbility.secondaryValue);
                        break;
                    case Ability.AbilityType.Heal:
                    case Ability.AbilityType.Buff:
                        ChangeAP(tempList, activeAbility.secondaryValue);
                        break;
                }
                break;
            case Ability.SecondaryTarget.Null:
            case Ability.SecondaryTarget.Bonus:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void ChangeAP(List<BattleEntity> entities, int apChange)
    {
        foreach (BattleEntity t in entities) {
            t.actionPoints -= apChange;
        }
        PreviewTurnOrder();
    }

    private void ChangeTurnSpeed(List<BattleEntity> entities, Ability.TokenOption tokenApplied, int tokenCount)
    {
        string tokenName = null;

        switch (tokenApplied) {
            case Ability.TokenOption.Haste:
                tokenName = "Haste";
                break;
            case Ability.TokenOption.Slow:
                tokenName = "Slow";
                break;
        }
        
        foreach (BattleEntity t in entities) {
            AddTokens(allCombatants[currentPlayer], t, tokenName, tokenCount, PREVIEW_RESIST_PIERCE);
        }
        
        PreviewTurnOrder();
    }
    
    private void RevertTurnSpeed(BattleEntity targetEntity)
    {
        foreach (BattleEntity t in turnOrder) {
            if (t.myName == targetEntity.myName) {
                List<BattleToken> tempList = new List<BattleToken>(targetEntity.activeTokens);
                t.activeTokens = tempList;
            }
        }
        PreviewTurnOrder();
    }
    
    public void SetCurrentAbilityType(int abilityIndex)
    {
        BattleEntity currentPlayerEntity = allCombatants[currentPlayer];
        currentPlayerEntity.activeAbilityType = currentPlayerEntity.myAbilities[abilityIndex].abilityType.ToString();
        currentPlayerEntity.activeAbility = abilityIndex;
        abilitySelected = true;
    }

    public void PassTurn()
    {
        if (state == BattleState.PlayerTurn) {
            usedAbility = false;
            StopAllCoroutines();
            allCombatants[currentPlayer].combatMenuVisuals.ChangeAbilitySelectUIVisibility(false);
            allCombatants[currentPlayer].combatMenuVisuals.ChangePassButtonVisibility(false);
            StartCoroutine(EndRoutine(allCombatants[currentPlayer]));
        }
    }
    
    public void BackToAbilities()
    {
        if (state == BattleState.Targeting) {
            EndResourcePreview(allCombatants[currentPlayer].activeAbility);
            abilitySelected = false;
            wentBack = true;
            
            allCombatants[currentPlayer].combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
            allCombatants[currentPlayer].combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
            allCombatants[currentPlayer].combatMenuVisuals.ChangeBackButtonVisibility(false);
            combatGrid.HideTiles(targetIsEnemy);
            if (allCombatants[currentPlayer].myAbilities[allCombatants[currentPlayer].activeAbility].abilityType == Ability.AbilityType.Movement) {
                combatGrid.DisableGridButtons(targetIsEnemy);
            }
            
            StopAllCoroutines();
            
            state = BattleState.PlayerTurn;
            StartCoroutine(PlayerTurnRoutine(currentPlayer));
        } else {
            print("Back button selected, but the user is in the " + state + " state.");
        }
    }
    
    private void SetTargetButtons(int characterIndex)
    {
        BattleEntity activeEntity = allCombatants[characterIndex];
        Ability activeAbility = activeEntity.myAbilities[activeEntity.activeAbility];
        targetList.Clear();
        
        // Disable all buttons
        for (int i = 0; i < activeEntity.targetButtons.Length; i++) {
            activeEntity.targetButtons[i].SetActive(false); 
        }

        if (targetIsEnemy) {
            List<int> stealthedTargets = new List<int>();
            
            // Enable buttons for each present enemy
            foreach (BattleEntity entity in enemyCombatants) {
                int distance = CalculateTargetDistance(entity, activeEntity);
                if (entity.activeTokens.Any(t => t.tokenName == "Stealth")) {
                    stealthedTargets.Add(enemyCombatants.IndexOf(entity));
                } else if (distance <= activeAbility.rangeMax && distance >= activeAbility.rangeMin) {
                    targetList.Add(entity);
                }
            }
            // Check if all enemies remaining are Stealthed, if so, ignore its effect.
            if (stealthedTargets.Count == enemyCombatants.Count) {
                foreach (int index in stealthedTargets) {
                    targetList.Add(enemyCombatants[index]);
                }
            }
            for (int i = 0; i < targetList.Count; i++) {
                activeEntity.targetButtons[i].SetActive(true);
                activeEntity.targetPortraits[i].GetComponent<Image>().sprite =
                    targetList[i].myPortrait;
                activeEntity.targetBorders[i].GetComponentInChildren<Image>().color =
                    new Color32(255, 0, 0, 255);
            }
        } else {
            // Enable buttons for each present ally
            foreach (BattleEntity entity in partyCombatants) {
                int distance = CalculateTargetDistance(entity, activeEntity);

                if (distance <= activeAbility.rangeMax && distance >= activeAbility.rangeMin) {
                    targetList.Add(entity);
                }
            }

            for (int i = 0; i < targetList.Count; i++) {
                activeEntity.targetButtons[i].SetActive(true);
                activeEntity.targetPortraits[i].GetComponent<Image>().sprite =
                    targetList[i].myPortrait;
                activeEntity.targetBorders[i].GetComponentInChildren<Image>().color =
                    new Color32(147, 229, 242, 255);
            }
        }
        
    }

    private void SetNoTargetButtons(int characterIndex)
    {
        BattleEntity activeEntity = allCombatants[characterIndex];
        
        // Disable all buttons
        for (int i = 0; i < activeEntity.targetButtons.Length; i++) {
            activeEntity.targetButtons[i].SetActive(false); 
        }
    }

    private void CheckTurnTarget(BattleEntity targetEntity)
    {
        List<int> targetIndexes =  new List<int>();
        for (int i = 0; i < turnOrder.Count; i++) {
            if (turnOrder[i].myName == targetEntity.myName) {
                targetIndexes.Add(i);
            }
        }

        turnOrderDisplay.ShiftTurnDisplay(targetIndexes);
    }

    private void SetTargetValuesForDisplay(int hoveredTarget)
    {
        BattleEntity activeEntity = allCombatants[currentPlayer];
        BattleEntity targetEntity;
        
        // Check if target is ally or enemy
        int target;
        target = allCombatants.IndexOf(targetList[hoveredTarget]);
        targetEntity = allCombatants[target];
        
        int abilityModifier = 0;
        bool singleValue = false;
        bool isCrit = false;
        float acc = 100f;
        int min = 0;
        int max = 0;
        int crit = 0;
        
        SetAbilityValuesAgainstTarget(activeEntity, targetEntity, ref abilityModifier, ref isCrit, ref acc, ref min,
            ref max, ref crit);
        
        // Check for damage abilities
        if (activeEntity.myAbilities[activeEntity.activeAbility].abilityType == Ability.AbilityType.Damage) {
            RunAbilityAgainstSelfTokens(activeEntity, ref abilityModifier, ref singleValue, ref acc, ref min, ref max, ref crit);
            RunAbilityAgainstTargetTokens(activeEntity, targetEntity, activeEntity.myAbilities[activeEntity.activeAbility],
                ref singleValue, ref acc, ref min, ref max, ref crit);
        }
        
        // Check against heal affecting tokens
        if (activeEntity.myAbilities[activeEntity.activeAbility].abilityType == Ability.AbilityType.Heal) {
            RunHealAgainstSelfTokens(activeEntity, ref abilityModifier, ref singleValue, ref acc, ref min, ref max, ref crit);
        }
        
        // Check against debuff affecting tokens
        if (activeEntity.myAbilities[activeEntity.activeAbility].abilityType == Ability.AbilityType.Debuff) {
            RunDebuffAgainstSelfTokens(activeEntity, ref isCrit, ref acc, ref crit);
            RunDebuffAgainstTargetTokens(activeEntity, targetEntity, ref isCrit, ref acc , ref crit);
        }

        switch (activeEntity.myAbilities[activeEntity.activeAbility].abilityType) {
            case Ability.AbilityType.Damage:
            case Ability.AbilityType.Debuff:
                crit -= allCombatants[target].critResist;
                break;
            case Ability.AbilityType.Heal:
            case Ability.AbilityType.Buff:
            case Ability.AbilityType.Movement:
            case Ability.AbilityType.Other:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
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
        
        // Methods for displaying the updated resources and turn order
        if (!abilityDuplicated) { PreviewSelfGain(activeEntity); }
        PreviewTargetResourceValue(targetEntity);

        bool hasSpeedToken = false;
        int speedTokenIndex = 10;

        for (int i = 0; i < activeEntity.myAbilities[activeEntity.activeAbility].targetTokensApplied.Length; i++) {
            if (activeEntity.myAbilities[activeEntity.activeAbility].targetTokensApplied[i] == Ability.TokenOption.Haste ||
                activeEntity.myAbilities[activeEntity.activeAbility].targetTokensApplied[i] == Ability.TokenOption.Slow) {
                hasSpeedToken = true;
                speedTokenIndex = i;
            }
        }

        if (hasSpeedToken) {
            List<BattleEntity> tempList = new List<BattleEntity>();
            foreach (BattleEntity t in turnOrder) {
                if (t.myName == targetEntity.myName) {
                    tempList.Add(t);
                }
            }
            
            ChangeTurnSpeed(tempList, activeEntity.myAbilities[activeEntity.activeAbility].targetTokensApplied[speedTokenIndex],
                activeEntity.myAbilities[activeEntity.activeAbility].targetTokenCountApplied[speedTokenIndex]);
        }
        
        CheckTurnTarget(targetEntity);
    }

    private void SetAbilityValuesForDisplay()
    {
        BattleEntity activeEntity = allCombatants[currentPlayer];
        
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

        turnOrderDisplay.ResetTurnDisplays();
    }

    public void IndicateTarget(int hoveredTarget)
    {
        int target;
        // Check if target is ally or enemy
        if (targetIsEnemy) {
            target = allCombatants.IndexOf(targetList[hoveredTarget]);
            allCombatants[target].battleVisuals.TargetEnemyActive();
        } else {
            target = allCombatants.IndexOf(targetList[hoveredTarget]);
            allCombatants[target].battleVisuals.TargetAllyActive();
        }
        
        targetBeingIndicated = true;
        SetTargetValuesForDisplay(hoveredTarget);
    }
    
    public void StopIndicatingTarget(int hoveredTarget)
    {
        targetBeingIndicated = false;
        SetAbilityValuesForDisplay();
        
        int target = allCombatants.IndexOf(targetList[hoveredTarget]);
        EndTargetResourcePreview(allCombatants[target]);
        if (!abilityDuplicated) { EndSelfGainPreview(allCombatants[currentPlayer]); }
        RevertTurnSpeed(allCombatants[target]);
        
        allCombatants[target].battleVisuals.TargetInactive();
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

    public void IndicateGridTarget(int positionIndex)
    {
        targetIndicatedGrid = true;
        int targetIndex;
        if (targetIsEnemy) {
            //targetIndex = enemyCombatants.IndexOf(enemyBattleGrid[positionIndex].occupiedBy);
            targetIndex = targetList.IndexOf(enemyBattleGrid[positionIndex].occupiedBy);
        } else {
            //targetIndex = partyCombatants.IndexOf(partyBattleGrid[positionIndex].occupiedBy);
            targetIndex = targetList.IndexOf(partyBattleGrid[positionIndex].occupiedBy);
        }
        SetTargetValuesForDisplay(targetIndex);
        
        if (targetIsEnemy) {
            targetIndex = allCombatants.IndexOf(enemyBattleGrid[positionIndex].occupiedBy);
            allCombatants[targetIndex].battleVisuals.TargetEnemyActive();
        } else {
            targetIndex = allCombatants.IndexOf(partyBattleGrid[positionIndex].occupiedBy);
            allCombatants[targetIndex].battleVisuals.TargetAllyActive();
        }
    }

    public void StopIndicatingGridTarget(int positionIndex)
    {
        targetIndicatedGrid = false;
        SetAbilityValuesForDisplay();
        int targetIndex;

        if (targetIsEnemy) {
            targetIndex = allCombatants.IndexOf(enemyBattleGrid[positionIndex].occupiedBy);
        } else {
            targetIndex = allCombatants.IndexOf(partyBattleGrid[positionIndex].occupiedBy);
        }
        
        EndTargetResourcePreview(allCombatants[targetIndex]);
        if (!abilityDuplicated) { EndSelfGainPreview(allCombatants[currentPlayer]); }
        RevertTurnSpeed(allCombatants[targetIndex]);
        allCombatants[targetIndex].battleVisuals.TargetInactive();
    }
    
    public void SelectTargetWithButtons(int currentTarget)
    {
        // Set the current member's target
        BattleEntity currentPlayerEntity = allCombatants[currentPlayer];
        
        if (targetIsEnemy) {
            currentPlayerEntity.SetTarget(allCombatants.IndexOf(enemyCombatants[currentTarget]));
        } else {
            currentPlayerEntity.SetTarget(allCombatants.IndexOf(partyCombatants[currentTarget]));
        }
        
        targetSelected = true;
    }
    
    private void SelectTargetWithGrid(int positionIndex, bool isTargetingEnemies)
    {
        GridTile chosenTile;
        if (isTargetingEnemies) {
            chosenTile = enemyBattleGrid[positionIndex];
        } else {
            chosenTile = partyBattleGrid[positionIndex];
        }

        // If somehow the player clicks the button for a tile with no enemy bounce them back
        if (!chosenTile.isOccupied) {
            print("Tile is not occupied, invalid target.");
            return;
        }
        
        allCombatants[currentPlayer].target = allCombatants.IndexOf(chosenTile.occupiedBy);
        targetSelected = true;
    }
    
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

    private int GetAbilityModifier(BattleEntity activeEntity, int activeAbilityIndex)
    {
        Ability.KeyStat abilityKey = activeEntity.myAbilities[activeAbilityIndex].keyStat;
        int abilityKeyMod = activeEntity.myAbilities[activeAbilityIndex].statModifier;
        int abilityModifier = 0;
        int tempInt = 0;
        
        switch (abilityKey)
        {
            case Ability.KeyStat.Null:
                abilityModifier = 0;
                break;
            case Ability.KeyStat.Power:
                abilityModifier = activeEntity.power * abilityKeyMod;
                break;
            case Ability.KeyStat.Skill:
                abilityModifier = activeEntity.skill * abilityKeyMod;
                break;
            case Ability.KeyStat.Wit:
                abilityModifier = activeEntity.wit * abilityKeyMod;
                break;
            case Ability.KeyStat.Mind:
                abilityModifier = activeEntity.mind * abilityKeyMod;
                break;
            case Ability.KeyStat.Speed:
                abilityModifier = activeEntity.speed * abilityKeyMod;
                break;
            case Ability.KeyStat.Luck:
                abilityModifier = activeEntity.luck * abilityKeyMod;
                break;
            case Ability.KeyStat.SelfBuffCount:
                foreach (BattleToken token in activeEntity.activeTokens) {
                    if (token.tokenType == Token.TokenType.Buff) {
                        tempInt += token.tokenCount;
                    }
                }
                abilityModifier = tempInt * abilityKeyMod;
                break;
            case Ability.KeyStat.SelfDebuffCount:
                foreach (BattleToken token in activeEntity.activeTokens) {
                    if (token.tokenType == Token.TokenType.Debuff) {
                        tempInt += token.tokenCount;
                    }
                }
                abilityModifier = tempInt * abilityKeyMod;
                break;
            case Ability.KeyStat.TargetBuffCount:
            case Ability.KeyStat.TargetDebuffCount:
                abilityModifier = 0;
                break;
            default:
                print("Invalid damage key of " +  abilityKey + " supplied");
                break;
        }
        return abilityModifier;
    }

    private int GetAbilityModifierAgainstTarget(BattleEntity activeEntity, BattleEntity targetEntity, int activeAbilityIndex)
    {
        Ability.KeyStat abilityKey = activeEntity.myAbilities[activeAbilityIndex].keyStat;
        int abilityKeyMod = activeEntity.myAbilities[activeAbilityIndex].statModifier;
        int abilityModifier = 0;
        int tempInt = 0;
        
        switch (abilityKey)
        {
            case Ability.KeyStat.Null:
                abilityModifier = 0;
                break;
            case Ability.KeyStat.Power:
                abilityModifier = activeEntity.power * abilityKeyMod;
                break;
            case Ability.KeyStat.Skill:
                abilityModifier = activeEntity.skill * abilityKeyMod;
                break;
            case Ability.KeyStat.Wit:
                abilityModifier = activeEntity.wit * abilityKeyMod;
                break;
            case Ability.KeyStat.Mind:
                abilityModifier = activeEntity.mind * abilityKeyMod;
                break;
            case Ability.KeyStat.Speed:
                abilityModifier = activeEntity.speed * abilityKeyMod;
                break;
            case Ability.KeyStat.Luck:
                abilityModifier = activeEntity.luck * abilityKeyMod;
                break;
            case Ability.KeyStat.SelfBuffCount:
                foreach (BattleToken token in activeEntity.activeTokens) {
                    if (token.tokenType == Token.TokenType.Buff) {
                        tempInt += token.tokenCount;
                    }
                }
                abilityModifier = tempInt * abilityKeyMod;
                break;
            case Ability.KeyStat.SelfDebuffCount:
                foreach (BattleToken token in activeEntity.activeTokens) {
                    if (token.tokenType == Token.TokenType.Debuff) {
                        tempInt += token.tokenCount;
                    }
                }
                abilityModifier = tempInt * abilityKeyMod;
                break;
            case Ability.KeyStat.TargetBuffCount:
                foreach (BattleToken token in targetEntity.activeTokens) {
                    if (token.tokenType == Token.TokenType.Buff) {
                        tempInt += token.tokenCount;
                    }
                }
                abilityModifier = tempInt * abilityKeyMod;
                break;
            case Ability.KeyStat.TargetDebuffCount:
                foreach (BattleToken token in targetEntity.activeTokens) {
                    if (token.tokenType == Token.TokenType.Debuff) {
                        tempInt += token.tokenCount;
                    }
                }
                abilityModifier = tempInt * abilityKeyMod;
                break;
            default:
                print("Invalid damage key of " +  abilityKey + " supplied");
                break;
        }
        return abilityModifier;
    }

    private int GetSecondaryAbilityModifier(BattleEntity activeEntity, int activeAbilityIndex)
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

    private void SetAbilityValues(BattleEntity activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        abilityModifier = GetAbilityModifier(activeEntity, activeEntity.activeAbility);
        
        isCrit = false;
        min = activeEntity.myAbilities[activeEntity.activeAbility].dmgMin + abilityModifier;
        max = activeEntity.myAbilities[activeEntity.activeAbility].dmgMax + abilityModifier;
        crit = activeEntity.critChance + activeEntity.myAbilities[activeEntity.activeAbility].critChance;
    }

    private void SetAbilityValuesAgainstTarget(BattleEntity activeEntity, BattleEntity targetEntity,
        ref int abilityModifier, ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        abilityModifier = GetAbilityModifierAgainstTarget(activeEntity, targetEntity, activeEntity.activeAbility);
        
        isCrit = false;
        min = activeEntity.myAbilities[activeEntity.activeAbility].dmgMin + abilityModifier;
        max = activeEntity.myAbilities[activeEntity.activeAbility].dmgMax + abilityModifier;
        crit = activeEntity.critChance + activeEntity.myAbilities[activeEntity.activeAbility].critChance;
    }

    private void SetSecondaryAbilityValues(BattleEntity activeEntity, ref int secondaryAbilityModifier, ref int secondaryValue)
    {
        secondaryAbilityModifier = GetSecondaryAbilityModifier(activeEntity, activeEntity.activeAbility);
        secondaryValue += secondaryAbilityModifier + activeEntity.myAbilities[activeEntity.activeAbility].secondaryValue;
    }

    private void RunAbilityAgainstSelfTokens(BattleEntity activeEntity, ref int abilityModifier,
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
            if (token.tokenName == "Blind" && activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }
    
    private void RunHealAgainstSelfTokens(BattleEntity activeEntity, ref int abilityModifier,
        ref bool isCrit, ref float acc, ref int min, ref int max, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Boost or Break tokens
            if (token.tokenName == "BoostPlus") {
                min = Mathf.FloorToInt(min * (1 + boostPlusToken.tokenValue));
                max = Mathf.FloorToInt(max * (1 + boostPlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Boost") {
                min = Mathf.FloorToInt(min * (1 + boostToken.tokenValue));
                max = Mathf.FloorToInt(max * (1 + boostToken.tokenValue));
                break;
            } else if (token.tokenName == "Break") {
                min = Mathf.FloorToInt(min * (1 - breakToken.tokenValue));
                max = Mathf.FloorToInt(max * (1 - breakToken.tokenValue));
                break;
            }
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                max = Mathf.FloorToInt(max * CRIT_DAMAGE_MODIFIER);
                min = max;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind" && activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }
    
    private void RunAbilityAgainstTargetTokens(BattleEntity activeEntity, BattleEntity targetEntity, Ability ability, ref bool isCrit, ref float acc,
        ref int min, ref int max, ref int crit)
    {
        bool hasPrecision = activeEntity.activeTokens.Any(t => t.tokenName == "Precision");
        
        foreach (BattleToken token in targetEntity.activeTokens) {
            // Check for Block or Vulnerable tokens
            if (token.tokenName == "BlockPlus" && !hasPrecision && !ability.ignoreBlock) {
                min = Mathf.FloorToInt(min * (1 - blockPlusToken.tokenValue));
                max = Mathf.FloorToInt(max * (1 - blockPlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Block" && !hasPrecision && !ability.ignoreBlock) {
                min = Mathf.FloorToInt(min * (1 - blockToken.tokenValue));
                max = Mathf.FloorToInt(max * (1 - blockToken.tokenValue));
                break;
            } else if (token.tokenName == "Vulnerable") {
                min = Mathf.FloorToInt(min * (1 + vulnerableToken.tokenValue));
                max = Mathf.FloorToInt(max * (1 + vulnerableToken.tokenValue));
                break;
            }

            // Check for Dodge tokens
            if (token.tokenName == "DodgePlus" && !hasPrecision  && !ability.ignoreDodge &&
                activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc = (acc * (1 - dodgePlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Dodge" && !hasPrecision  && !ability.ignoreDodge && 
                       activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc = (acc * (1 - dodgeToken.tokenValue));
                break;
            }
        }
    }

    private void RunBuffAgainstSelfTokens(BattleEntity activeEntity, ref bool isCrit, ref float acc, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind" && activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }
    
    private void RunDebuffAgainstSelfTokens(BattleEntity activeEntity, ref bool isCrit, ref float acc, ref int crit)
    {
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Critical tokens
            if (token.tokenName == "Critical") {
                crit = 100;
                isCrit = true;
                break;
            }
            // Check for Blind tokens
            if (token.tokenName == "Blind" && activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc *= (1 - blindToken.tokenValue);
                break;
            }
        }
    }

    private void RunDebuffAgainstTargetTokens(BattleEntity activeEntity, BattleEntity targetEntity, ref bool isCrit,
        ref float acc, ref int crit)
    {
        bool hasPrecision = activeEntity.activeTokens.Any(t => t.tokenName == "Precision");
        Ability activeAbility = activeEntity.myAbilities[activeEntity.activeAbility];
        
        foreach (BattleToken token in activeEntity.activeTokens) {
            // Check for Dodge tokens
            if (token.tokenName == "DodgePlus" && !hasPrecision  && !activeAbility.ignoreDodge &&
                activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc = (acc * (1 - dodgePlusToken.tokenValue));
                break;
            } else if (token.tokenName == "Dodge" && !hasPrecision  && !activeAbility.ignoreDodge && 
                       activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                acc = (acc * (1 - dodgeToken.tokenValue));
                break;
            }
        }
    }
    
    private IEnumerator TriggerAilments(BattleEntity activeEntity)
    {
        int ailmentIndex;
        int ailmentDamage = 0;
        bool ignoreDefense = false;
        if (activeEntity.activeTokens.Any(t => t.tokenName == "Burn")) {
            ailmentIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == "Burn");
            ailmentDamage = (int)(activeEntity.activeTokens[ailmentIndex].tokenCount * burnCounter.tokenValue);
            activeEntity.activeTokens[ailmentIndex].tokenCount =
                Mathf.FloorToInt(activeEntity.activeTokens[ailmentIndex].tokenCount * 0.5f);
            if (activeEntity.activeTokens[ailmentIndex].tokenCount <= 0) {
                activeEntity.activeTokens.RemoveAt(ailmentIndex);
                activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
            }
            
            int distance;
            if (activeEntity.isPlayer) {
                foreach (GridTile tile in partyBattleGrid) {
                    distance = CalculateTileDistance(tile, activeEntity);
                    if (distance <= 1 && tile.isOccupied && tile.occupiedBy != activeEntity) {
                        AddTokens(activeEntity, tile.occupiedBy, activeEntity.activeTokens[ailmentIndex].tokenName, 
                            2, 0);
                    }
                }
            } else {
                foreach (GridTile tile in enemyBattleGrid) {
                    distance = CalculateTileDistance(tile, activeEntity);
                    if (distance <= 1 && tile.isOccupied && tile.occupiedBy != activeEntity) {
                        AddTokens(activeEntity, tile.occupiedBy, activeEntity.activeTokens[ailmentIndex].tokenName, 
                            2, 0);
                    }
                }
            }
        }

        if (activeEntity.activeTokens.Any(t => t.tokenName == "Poison")) {
            ignoreDefense = true;
            ailmentIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == "Poison");
            ailmentDamage = (int)(activeEntity.activeTokens[ailmentIndex].tokenCount * poisonCounter.tokenValue);
            activeEntity.activeTokens[ailmentIndex].tokenCount =
                Mathf.FloorToInt(activeEntity.activeTokens[ailmentIndex].tokenCount * 0.5f);
            if (activeEntity.activeTokens[ailmentIndex].tokenCount <= 0) {
                activeEntity.activeTokens.RemoveAt(ailmentIndex);
                activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
            }
        }

        if (activeEntity.activeTokens.Any(t => t.tokenName == "Bleed")) {
            ignoreDefense = true;
            ailmentIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == "Bleed");
            ailmentDamage = (int)(activeEntity.activeTokens[ailmentIndex].tokenCount * bleedCounter.tokenValue);
            activeEntity.activeTokens[ailmentIndex].tokenCount -= 1;
            if (activeEntity.activeTokens[ailmentIndex].tokenCount <= 0) {
                activeEntity.activeTokens.RemoveAt(ailmentIndex);
                activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
            }
        }

        if (ignoreDefense) {
            activeEntity.currentHealth -= ailmentDamage;
        } else {
            // Deal the damage to defense, or if the target has none, to HP
            if (activeEntity.currentDefense > 0) {
                // If the damage dealt is greater than the target's defense, deal the rest to their HP
                if (ailmentDamage > activeEntity.currentDefense) {
                    int overflowDamage = ailmentDamage - activeEntity.currentDefense;
                    activeEntity.currentDefense = 0;
                    activeEntity.currentHealth -= overflowDamage;
                } else {
                    activeEntity.currentDefense -= ailmentDamage;
                }
            } else {
                activeEntity.currentHealth -= ailmentDamage;
            }
        }

        activeEntity.battleVisuals.PlayHitAnimation(ailmentDamage, false); // target plays on hit animation
        yield return new WaitForSeconds(AILMENT_DAMAGE_DELAY);
        if (activeEntity.isPlayer) {
            activeEntity.UpdatePlayerUI();
        } else if (!activeEntity.isPlayer) {
            activeEntity.UpdateEnemyUI();
        }
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }

    private BattleToken CreateBattleToken(BattleToken originalToken)
    {
        BattleToken battleToken = new BattleToken();
        battleToken.SetTokenValues(originalToken.tokenName, originalToken.displayName, originalToken.tokenIcon,
            originalToken.tokenType, originalToken.tokenValue, originalToken.tokenCap, originalToken.tokenInverses,
            originalToken.tokenDescription);
        return battleToken;
    }

    public void AddTokens(BattleEntity applyingEntity, BattleEntity recipientEntity, string tokenName, int tokenCount, int resistPierce)
    {
        int tIndex = 100;
        bool notPresent = true;
        string inverseOne;
        string inverseTwo;
        BattleToken tokenAdded = allTokens.FirstOrDefault(t => tokenName == t.tokenName);

        if (recipientEntity.currentDefense > 0 && tokenName == "Bleed") { return; }

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
                
                if (tokenAdded.tokenType == Token.TokenType.Ailments) {
                    int ailmentRoll = Random.Range(1, 101);
                    if (ailmentRoll + resistPierce < recipientEntity.ailmentResist) {
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

                if (tokenName == "Haste" || tokenName == "Slow") {
                    GetTurnOrder();
                }
            }
        }
        
        // Check and mark unique tokens added
        if (recipientEntity.myName == "Bune" && tokenAdded.tokenName == "Vice") {
            cowboyLogic.GainedVice(true);
        }

        if (resistPierce != PREVIEW_RESIST_PIERCE) {
            recipientEntity.battleVisuals.UpdateTokens(recipientEntity.activeTokens);
        }
    }

    private void ClearTokens(BattleEntity targetEntity, string tokenName)
    {
        BattleToken tokenForRemoval = allTokens.FirstOrDefault(t => tokenName == t.tokenName);

        if (targetEntity.activeTokens.Any(t => tokenForRemoval.tokenName == t.tokenName)) {
            int tokenIndex = targetEntity.activeTokens.FindIndex(t => tokenForRemoval.tokenName == t.tokenName);
            targetEntity.activeTokens.RemoveAt(tokenIndex);
        }
    }

    private void RemoveSelfTurnStartTokens(BattleEntity activeEntity)
    {
        // Run character specific end of turn Methods
        if (activeEntity.myName == "Bune") {
            cowboyLogic.CowboyRemoveVice(activeEntity);
        }
        
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in activeEntity.activeTokens) {
            
            // Check for Haste or Slow tokens
            if (t.tokenName == "Haste") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Slow") {
                tokensToRemove.Add(t.tokenName);
            }
        
            // Check for Off-Guard tokens
            if (t.tokenName == "OffGuard") {
                tokensToRemove.Add(t.tokenName);
            }
        
            // Check for Anti-Heal tokens
            if (t.tokenName == "AntiHeal") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Restrict tokens
            if (t.tokenName == "Restrict") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Stealth tokens
            if (t.tokenName == "Stealth") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Guard tokens
            if (t.tokenName == "GuardColumn") {
                tokensToRemove.Add(t.tokenName);
            }
            if (t.tokenName == "GuardRow") {
                tokensToRemove.Add(t.tokenName);
            }
        }

        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (activeEntity.activeTokens[tokenIndex].tokenCount > 1) {
                activeEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }

    private void RemoveSelfDamageTokens(BattleEntity activeEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in activeEntity.activeTokens) {
            
            // Check for Boost or Break tokens
            if (t.tokenName == "BoostPlus") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Boost") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Break") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Critical tokens
            if (t.tokenName == "Critical") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Blind tokens
            if (t.tokenName == "Blind") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Pierce tokens
            if (t.tokenName == "Pierce") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Precision tokens
            if (t.tokenName == "Precision") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Ricochet tokens
            if (t.tokenName == "Ricochet") {
                tokensToRemove.Add(t.tokenName);
            }
        }

        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (activeEntity.activeTokens[tokenIndex].tokenCount > 1) {
                activeEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }
    
    private void RemoveSelfHealTokens(BattleEntity activeEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in activeEntity.activeTokens) {
            
            // Check for Boost or Break tokens
            if (t.tokenName == "BoostPlus") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Boost") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Break") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Critical tokens
            if (t.tokenName == "Critical") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Blind tokens
            if (t.tokenName == "Blind") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Precision tokens
            if (t.tokenName == "Precision") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (activeEntity.activeTokens[tokenIndex].tokenCount > 1) {
                activeEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }
    
    private void RemoveSelfBuffTokens(BattleEntity activeEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in activeEntity.activeTokens) {

            // Check for Critical tokens
            if (t.tokenName == "Critical") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Blind tokens
            if (t.tokenName == "Blind" && activeEntity.myAbilities[activeEntity.activeAbility].hasAccuracy) {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Precision tokens
            if (t.tokenName == "Precision") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Ricochet tokens
            if (t.tokenName == "Ricochet") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (activeEntity.activeTokens[tokenIndex].tokenCount > 1) {
                activeEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }
    
    private void RemoveSelfDebuffTokens(BattleEntity activeEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in activeEntity.activeTokens) {

            // Check for Critical tokens
            if (t.tokenName == "Critical") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Precision tokens
            if (t.tokenName == "Precision") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = activeEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (activeEntity.activeTokens[tokenIndex].tokenCount > 1) {
                activeEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                activeEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        activeEntity.battleVisuals.UpdateTokens(activeEntity.activeTokens);
    }

    private void RemoveTargetDamageTokens(BattleEntity targetEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in  targetEntity.activeTokens) {
            
            // Check for Block or Vulnerable tokens
            if (t.tokenName == "BlockPlus") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Block") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Vulnerable") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Dodge tokens
            if (t.tokenName == "DodgePlus") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Dodge") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Ward or Isolation
            if (t.tokenName == "Ward") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Isolation") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Taunt
            if (t.tokenName == "Taunt") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Guard tokens
            if (t.tokenName == "GuardColumn") {
                tokensToRemove.Add(t.tokenName);
            }
            if (t.tokenName == "GuardRow") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = targetEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (targetEntity.activeTokens[tokenIndex].tokenCount > 1) {
                targetEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void RemoveTargetHealTokens(BattleEntity healTarget)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in healTarget.activeTokens) {
            
            // Check for Anti-Heal
            if (t.tokenName == "AntiHeal") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Ward or Isolation
            if (t.tokenName == "Ward") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Isolation") {
                tokensToRemove.Add(t.tokenName);
            }
            
            foreach (string tokenName in tokensToRemove) {
                int tokenIndex = healTarget.activeTokens.FindIndex(t => t.tokenName == tokenName);
                if (healTarget.activeTokens[tokenIndex].tokenCount > 1) {
                    healTarget.activeTokens[tokenIndex].tokenCount -= 1;
                } else {
                    healTarget.activeTokens.RemoveAt(tokenIndex);
                }
            }

            healTarget.battleVisuals.UpdateTokens(healTarget.activeTokens);
        }
    }
    
    private void RemoveTargetBuffTokens(BattleEntity targetEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in targetEntity.activeTokens) {
            
            // Check for Ward
            if (t.tokenName == "Isolation") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Precision tokens
            if (t.tokenName == "Precision") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = targetEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (targetEntity.activeTokens[tokenIndex].tokenCount > 1) {
                targetEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void RemoveTargetDebuffTokens(BattleEntity targetEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in targetEntity.activeTokens) {
            
            // Check for Dodge tokens
            if (t.tokenName == "DodgePlus") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Dodge") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Ward or Isolation
            if (t.tokenName == "Ward") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Taunt
            if (t.tokenName == "Taunt") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Guard tokens
            if (t.tokenName == "GuardColumn") {
                tokensToRemove.Add(t.tokenName);
            }
            if (t.tokenName == "GuardRow") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = targetEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (targetEntity.activeTokens[tokenIndex].tokenCount > 1) {
                targetEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void RemoveTokensOnMiss(BattleEntity targetEntity)
    {
        List<string> tokensToRemove = new List<string>();
        
        foreach (BattleToken t in targetEntity.activeTokens) {
            
            // Check for Dodge tokens
            if (t.tokenName == "DodgePlus") {
                tokensToRemove.Add(t.tokenName);
            } else if (t.tokenName == "Dodge") {
                tokensToRemove.Add(t.tokenName);
            }

            // Check for Taunt tokens
            if (t.tokenName == "Taunt") {
                tokensToRemove.Add(t.tokenName);
            }
            
            // Check for Guard tokens
            if (t.tokenName == "GuardColumn") {
                tokensToRemove.Add(t.tokenName);
            }
            if (t.tokenName == "GuardRow") {
                tokensToRemove.Add(t.tokenName);
            }
        }
        
        foreach (string tokenName in tokensToRemove) {
            int tokenIndex = targetEntity.activeTokens.FindIndex(t => t.tokenName == tokenName);
            if (targetEntity.activeTokens[tokenIndex].tokenCount > 1) {
                targetEntity.activeTokens[tokenIndex].tokenCount -= 1;
            } else {
                targetEntity.activeTokens.RemoveAt(tokenIndex);
            }
        }
        
        targetEntity.battleVisuals.UpdateTokens(targetEntity.activeTokens);
    }

    private void TriggerTurnStartTokens(BattleEntity entity)
    {
        if (entity.activeTokens.Any(t => t.tokenName == "Stagger")) {
            usedLightAction = true;
            int tokenPosition = entity.activeTokens.FindIndex(t => t.tokenName == "Stagger");
            entity.activeTokens.RemoveAt(tokenPosition);
        }
    }

    private void TriggerTurnSpeedTokens(BattleEntity entity)
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
            for (int i = 0; i < entity.abilityCooldowns.Count; i++) {
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
            state = BattleState.End;
        }
    }

    private bool IgnoreArmorWithTokens(BattleEntity attacker, BattleEntity attackTarget)
    {
        return attacker.activeTokens.Any(t => t.tokenName == "Pierce") ||
               attackTarget.activeTokens.Any(t => t.tokenName == "Pierce");
    }

    private bool IgnoreRestoreWithTokens(BattleEntity healer, BattleEntity healTarget)
    {
        return (healTarget.activeTokens.Any(t => t.tokenName == "AntiHeal") && 
                healer.activeTokens.All(t => t.tokenName != "Precision"));
    }

    private void SetAbilityTokens(ref List<BattleToken> targetTokens, ref List<int> targetTokensCount,
        ref List<BattleToken> selfTokens, ref List<int> selfTokensCount, Ability activeAbility)
    {
        for (int i = 0; i < activeAbility.targetTokensApplied.Length; i++) {
            var tokenType = activeAbility.targetTokensApplied[i];
            BattleToken tokenToAdd = allTokens.Find(t => t.tokenName == tokenType.ToString());
            targetTokens.Add(tokenToAdd);
            targetTokensCount.Add(activeAbility.targetTokenCountApplied[i]);
        }
        for (int i = 0; i < activeAbility.selfTokensApplied.Length; i++) {
            var tokenType = activeAbility.selfTokensApplied[i];
            BattleToken tokenToAdd = allTokens.Find(t => t.tokenName == tokenType.ToString());
            selfTokens.Add(tokenToAdd);
            selfTokensCount.Add(activeAbility.selfTokenCountApplied[i]);
        }
    }

    private void SetAbilityCritTokens(ref List<BattleToken> targetTokens, ref List<int> targetTokensCount,
        ref List<BattleToken> selfTokens, ref List<int> selfTokensCount, Ability activeAbility)
    {
        for (int i = 0; i < activeAbility.targetCritTokensApplied.Length; i++) {
            var tokenType = activeAbility.targetCritTokensApplied[i];
            BattleToken tokenToAdd = allTokens.Find(t => t.tokenName == tokenType.ToString());
            targetTokens.Add(tokenToAdd);
            targetTokensCount.Add(activeAbility.targetCritTokenCountApplied[i]);
        }
        for (int i = 0; i < activeAbility.selfCritTokensApplied.Length; i++) {
            var tokenType = activeAbility.selfCritTokensApplied[i];
            BattleToken tokenToAdd = allTokens.Find(t => t.tokenName == tokenType.ToString());
            selfTokens.Add(tokenToAdd);
            selfTokensCount.Add(activeAbility.selfCritTokenCountApplied[i]);
        }
    }

    private void SetYMovement(BattleEntity activeEntity, BattleEntity targetEntity, Ability activeAbility, 
        ref int selfYTravel, ref int targetYTravel)
    {
        if (activeAbility.selfYChangeToCenter) {
            if (activeEntity.yPos <= (BASE_Y_MAX / 2)) {
                selfYTravel = activeAbility.selfYChange;
            } else {
                selfYTravel = (activeAbility.selfYChange * -1);
            }
        } else {
            selfYTravel = activeAbility.selfYChange;
        }

        if (activeAbility.targetYChangeToCenter) {
            if (targetEntity.yPos <= (BASE_Y_MAX / 2)) {
                targetYTravel = activeAbility.selfYChange;
            } else {
                targetYTravel = (activeAbility.selfYChange * -1);
            }
        } else {
            targetYTravel = activeAbility.selfYChange;
        }
    }
    
    // Character specific methods
    public IEnumerator CowboyViceActOut(BattleEntity cowboy) // TODO make this reflect ability ranges
    {
        BattleEntity abilityTarget = null;
        int actOutAbility;
        targetList.Clear();

        int characterIndex = allCombatants.IndexOf(cowboy);

        StopCoroutine(PlayerTurnRoutine(characterIndex));
            
        // Bune uses a random action with a random target
        print("Bune acts out due to boredom!");
        yield return new WaitForSeconds(TURN_ACTION_DELAY);
            
        actOutAbility = Random.Range(1, PLAYER_NONMOVE_ABILITIES + 1);
        cowboy.activeAbility = actOutAbility;

        bool targetingFoes = true;
        switch (cowboy.myAbilities[actOutAbility].abilityType) {
            case Ability.AbilityType.Damage:
            case Ability.AbilityType.Debuff:
                print("Act-out is targeting enemies");
                targetList = enemyCombatants;
                targetingFoes = true;
                break;
            case  Ability.AbilityType.Heal:
            case  Ability.AbilityType.Buff:
                print("Act-out is targeting allies");
                targetList = partyCombatants;
                targetingFoes = false;
                break;
        }

        bool hasTaunt = false;
        if (targetingFoes) {
            foreach (BattleEntity entity in targetList.Where(entity => entity.activeTokens.Any(t => t.tokenName == "Taunt"))) {
                abilityTarget = entity;
                hasTaunt = true;
                break;
            }
        }
        if (!hasTaunt) {
            int abilityTargetIndex = Random.Range(0, targetList.Count);
            print(abilityTargetIndex);
            abilityTarget = targetList[abilityTargetIndex];
        }
            
        switch (cowboy.myAbilities[actOutAbility].abilityType) {
            case Ability.AbilityType.Damage:
                yield return StartCoroutine(DamageAction(cowboy, abilityTarget, actOutAbility));
                break;
            case Ability.AbilityType.Debuff:
                yield return StartCoroutine(DebuffAction(cowboy, abilityTarget, actOutAbility));
                break;
            case  Ability.AbilityType.Heal:
                yield return StartCoroutine(HealAction(cowboy, abilityTarget, actOutAbility));
                break;
            case  Ability.AbilityType.Buff:
                yield return StartCoroutine(BuffAction(cowboy, abilityTarget, actOutAbility));
                break;
            default:
                print("Invalid ability type of " + cowboy.myAbilities[actOutAbility].abilityType + " called.");
                yield break;
        }

        cowboy.combatMenuVisuals.ChangeTargetSelectUIVisibility(false);
        cowboy.combatMenuVisuals.ChangeAbilityEffectTextVisibility(false);
        cowboy.combatMenuVisuals.ChangeBackButtonVisibility(false);

        cowboyLogic.ResetCowboyActout();
        
        state = BattleState.End;
        StartCoroutine(EndRoutine(cowboy));
        yield break;
    }
    
    private IEnumerator DamageAction(BattleEntity attacker, BattleEntity attackTarget, int activeAbilityIndex)
    {
        Ability activeAbility = attacker.myAbilities[activeAbilityIndex];
        
        print(attacker.myName + " used " + activeAbility.abilityName + " against " + attackTarget.myName);
        
        // Declare damage values
        int damage;
        int damageModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        bool isLethal = false;
        
        // Declare secondary damage values, if any
        int secondaryDamage;
        int secondaryModifier = 0;
        int secondaryValue = 0;
        string secondaryTarget = activeAbility.secondaryTarget.ToString();
        
        // Get damage values
        SetAbilityValuesAgainstTarget(attacker, attackTarget, ref damageModifier, ref isCrit, ref acc, ref minDamageRange,
                ref maxDamageRange, ref critChance);
        
        // Run damage against tokens
        RunAbilityAgainstSelfTokens(attacker, ref damageModifier, ref isCrit, ref acc, ref minDamageRange, 
            ref maxDamageRange, ref critChance);
        RunAbilityAgainstTargetTokens(attacker, attackTarget, activeAbility, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        // Get secondary damage values
        SetSecondaryAbilityValues(attacker, ref secondaryModifier,  ref secondaryValue); 
        secondaryDamage = secondaryValue;
        
        // Determine move distance
        int selfXTravel = activeAbility.selfXChange;
        int selfYTravel = 0;
        int targetXTravel = activeAbility.targetXChange;
        int targetYTravel = 0;
        SetYMovement(attacker, attackTarget, activeAbility, ref selfYTravel, ref targetYTravel);
        
        // Determine which and how many tokens will be applied by the ability
        List<BattleToken> targetTokens = new List<BattleToken>();
        List<int> targetTokensCount = new List<int>();
        List<BattleToken> selfTokens = new List<BattleToken>();
        List<int> selfTokensCount = new List<int>();

        SetAbilityTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);

        if (abilityDuplicated) {
            switch (duplicationType) {
                case DuplicationType.Cleave:
                    break;
                case DuplicationType.Ricochet:
                    minDamageRange = Mathf.FloorToInt(minDamageRange * ricochetToken.tokenValue);
                    maxDamageRange = Mathf.FloorToInt(maxDamageRange * ricochetToken.tokenValue);
                    break;
                case DuplicationType.Dualcast:
                    break;
                case DuplicationType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        switch (attacker.myName) {
            // Check character logic
            case "Renée":
                repentantLogic.RepentantAbilityLogic(attacker, attackTarget, activeAbility, ref minDamageRange,
                    ref maxDamageRange, ref secondaryDamage, ref critChance, ref selfTokens, ref selfTokensCount,
                    ref targetTokens, ref targetTokensCount);
                break;
            case "Bune":
                cowboyLogic.CowboyGainVice(allCombatants[currentPlayer], attackTarget);
                break;
            case "Tre":
                ricochetLogic.RicochetAttackLogic(attacker, activeAbility, ref minDamageRange, ref maxDamageRange, ref critChance,
                    ref selfXTravel, ref selfYTravel, ref selfTokens, ref selfTokensCount, ref targetTokens, ref targetTokensCount);
                break;
        }
        
        // Reduce crit chance by target's crit resist
        critChance -= attackTarget.critResist;
        if (critChance < 0) {
            critChance = 0;
        }
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (attacker.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(attacker, token.ToString());
                }
            }
        }
        
        // Change self position
        if ((selfXTravel != 0 || selfYTravel != 0) && !abilityDuplicated) {
            if (attacker.activeTokens.All(t => t.tokenName != "Restrict")) {
                StartCoroutine(SetGridPosition(attacker, selfXTravel, selfYTravel));
            }
        }
        
        // Check for hit
        if (activeAbility.hasAccuracy) {
            int accRoll = Random.Range(1, 101);
            if (accRoll > (int)acc) {
                attackTarget.battleVisuals.AbilityMisses();
                RemoveSelfDamageTokens(attacker);
                RemoveTokensOnMiss(attackTarget);
                if (!abilityDuplicated) {
                    SelfGain(attacker, activeAbility, isCrit);
                    if (!abilityDuplicated) {
                        for (int i = 0; i < selfTokens.Count; i++) {
                            AddTokens(attacker, attacker, selfTokens[i].tokenName, selfTokensCount[i], 0);
                        }
                    }
                }
                
                // Check character logic on miss
                if (!abilityDuplicated) {
                    if (attacker.myName == "Tre") {
                        if (activeAbility.costResource == Ability.CostResource.Spirit) {
                            ricochetLogic.ReduceBulletCount(activeAbility);
                        }
                    }
                }
                
                yield return new WaitForSeconds(TURN_ACTION_DELAY);

                if (!abilityDuplicated) {
                    yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
                }
                SaveResources();
                
                // Check for extra casts
                if (extraCastCount < activeAbility.extraCasts) {
                    extraCastCount++;
                    yield return StartCoroutine(DamageAction(attacker, attackTarget, activeAbilityIndex));
                }
                
                yield break;
            }
        }
        
        // Check for crit and determine damage values for crit
        int critRoll = Random.Range(1, 101);
        if (critRoll <= critChance && attacker.activeTokens.All(t => t.tokenName != "Critical")) {
            isCrit = true;
            if (maxDamageRange < 0) {
                maxDamageRange = 0;
            }
            damage = (int)(maxDamageRange * CRIT_DAMAGE_MODIFIER);
            if (!IgnoreArmorWithTokens(attacker, attackTarget) || attacker.myAbilities[attacker.activeAbility].ignoreArmor) {
                damage -= attackTarget.currentArmor;
            }
            if (damage < 1) {
                damage = 1;
            }
        } else {
            if (minDamageRange < 0) {
                minDamageRange = 0;
            }
            if (maxDamageRange < 0) {
                maxDamageRange = 0;
            }
            damage = Random.Range(minDamageRange, maxDamageRange + 1);
            if (!IgnoreArmorWithTokens(attacker, attackTarget) || attacker.myAbilities[attacker.activeAbility].ignoreArmor) {
                damage -= attackTarget.currentArmor;
            }
            if (damage < 0) {
                damage = 0;
            }
        }

        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (attackTarget.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(attackTarget, token.ToString());
                }
            }
        }
        
        // Change target position
        if (targetXTravel != 0 || targetYTravel != 0) {
            StartCoroutine(SetGridPosition(attackTarget, targetXTravel, targetYTravel));
        }
        
        // Restore self values
        if (!abilityDuplicated) {
            SelfGain(attacker, activeAbility, isCrit);
        }
        
        // Apply on crit tokens if attack crit
        if (isCrit) {
            SetAbilityCritTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
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
        }
        
        // Play combat animations
        attackTarget.battleVisuals.PlayHitAnimation(damage, isCrit); // target plays on hit animation
        if (!abilityDuplicated && extraCastCount == 0) {
            attacker.battleVisuals.PlayAttackAnimation(); // play the attack animation
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        }
        
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
        if (attackTarget.currentHealth <= 0) {
            isLethal = true;
        }
        
        // Check character lethal logic
        if (isLethal) {
            if (attacker.myName == "Renée") {
                repentantLogic.RepentantLethalLogic(attacker, activeAbility, ref selfTokens, ref selfTokensCount);
            }

            if (attacker.activeTokens.Any(t => t.tokenName == "Killseeker")) {
                selfTokens.Add(GetTokenIdentity("Boost"));
                selfTokensCount.Add(1);
            }
        }
        
        // Apply tokens to self
        if (!abilityDuplicated) {
            for (int i = 0; i < selfTokens.Count; i++) {
                AddTokens(attacker, attacker, selfTokens[i].tokenName, selfTokensCount[i], 0);
            }
        }
        // Apply target tokens
        for (int i = 0; i < targetTokens.Count; i++) {
            AddTokens(attacker, attackTarget, targetTokens[i].tokenName, targetTokensCount[i],
                attacker.resistPierce);
        }

        attackTarget.wasDamagedLastTurn = true;
        attackTarget.damagedBy = allCombatants.IndexOf(attacker);

        if (!abilityDuplicated && extraCastCount == 0) {
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        }
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
            
            if (preparedCombatants.Any(t => t.myName == attackTarget.myName)) {
                preparedCombatants.Remove(attackTarget);
            }
            if (attackTarget.isPlayer) {
                partyCombatants.Remove(attackTarget);
                
            } else if (!attackTarget.isPlayer) {
                enemyCombatants.Remove(attackTarget);
            }
        }
        
        if (activeAbility.attackType == Ability.AttackType.Ranged &&
            attacker.activeTokens.Any(t => t.tokenName == "Ricochet") &&
            !abilityDuplicated) {
            List<BattleEntity> ricochetTargetList = new List<BattleEntity>();
            foreach (BattleEntity entity in enemyCombatants) {
                int distance = CalculateTargetDistance(entity, attackTarget);
                if (distance <= 2 && distance >= 1) {
                    ricochetTargetList.Add(entity);
                }
            }

            if (ricochetTargetList.Count > 0) {
                int ricochetTargetIndex = Random.Range(0, ricochetTargetList.Count);
                abilityDuplicated = true;
                duplicationType = DuplicationType.Ricochet;
                yield return StartCoroutine(DamageAction(attacker, ricochetTargetList[ricochetTargetIndex], activeAbilityIndex));
            }
        }

        // Check for extra casts
        if (extraCastCount < activeAbility.extraCasts) {
            extraCastCount++;
            yield return StartCoroutine(DamageAction(attacker, attackTarget, activeAbilityIndex));
        }
        
        // Check character logic post-attack
        if (!abilityDuplicated) {
            if (attacker.myName == "Tre") {
                if (activeAbility.costResource == Ability.CostResource.Spirit) {
                    ricochetLogic.ReduceBulletCount(activeAbility);
                }
            }
        }
    }
    
    private IEnumerator HealAction(BattleEntity healer, BattleEntity healTarget, int activeAbilityIndex)
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
        SetAbilityValuesAgainstTarget(healer, healTarget, ref restoreModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);

        // Check heal against tokens
        RunHealAgainstSelfTokens(healer, ref restoreModifier, ref isCrit, ref acc, ref minDamageRange, 
            ref maxDamageRange, ref critChance);
        
        // Get secondary heal values
        SetSecondaryAbilityValues(healer, ref secondaryModifier,  ref secondaryValue);
        secondaryRestore = secondaryValue;
        
        // Determine move distance
        int selfXTravel = activeAbility.selfXChange;
        int selfYTravel = 0;
        int targetXTravel = activeAbility.targetXChange;
        int targetYTravel = 0;
        SetYMovement(healer, healTarget, activeAbility, ref selfYTravel, ref targetYTravel);
        
        // Determine which and how many tokens will be applied by the ability
        List<BattleToken> targetTokens = new List<BattleToken>();
        List<int> targetTokensCount = new List<int>();
        List<BattleToken> selfTokens = new List<BattleToken>();
        List<int> selfTokensCount = new List<int>();

        SetAbilityTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
        
        if (abilityDuplicated) {
            switch (duplicationType) {
                case DuplicationType.Cleave:
                    break;
                case DuplicationType.Ricochet:
                    minDamageRange = Mathf.FloorToInt(minDamageRange * ricochetToken.tokenValue);
                    maxDamageRange = Mathf.FloorToInt(maxDamageRange * ricochetToken.tokenValue);
                    break;
                case DuplicationType.Dualcast:
                    break;
                case DuplicationType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Check character logic
        if (healer.myName == "Renée") {
            repentantLogic.RepentantAbilityLogic(healer, healTarget, activeAbility, ref minDamageRange,
                ref maxDamageRange, ref secondaryRestore, ref critChance, ref selfTokens, ref selfTokensCount,
                ref targetTokens, ref targetTokensCount);
        }
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (healer.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(healer, token.ToString());
                }
            }
        }
        
        // Change self position
        if ((selfXTravel != 0 || selfYTravel != 0) && !abilityDuplicated) {
            if (healer.activeTokens.All(t => t.tokenName != "Restrict")) {
                StartCoroutine(SetGridPosition(healer, selfXTravel, selfYTravel));
            }
        }
        
        // Check for hit
        if (activeAbility.hasAccuracy) {
            int accRoll = Random.Range(1, 101);
            if (accRoll > (int)acc) {
                healTarget.battleVisuals.AbilityMisses();
                RemoveSelfHealTokens(healer);
                if (!abilityDuplicated) {
                    SelfGain(healer, activeAbility, isCrit);
                    if (!abilityDuplicated) {
                        for (int i = 0; i < selfTokens.Count; i++) {
                            AddTokens(healer, healer, selfTokens[i].tokenName, selfTokensCount[i], 0);
                        }
                    }
                }
                yield return new WaitForSeconds(TURN_ACTION_DELAY);

                if (!abilityDuplicated) {
                    yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
                }
                SaveResources();
                
                // Check for extra casts
                if (extraCastCount < activeAbility.extraCasts) {
                    extraCastCount++;
                    yield return StartCoroutine(DamageAction(healer, healTarget, activeAbilityIndex));
                }
                
                yield break;
            }
        }
        
        // Check for crit then determine heal value either way
        int critRoll = Random.Range(1, 101);
        if (critRoll <= critChance && healer.activeTokens.All(t => t.tokenName != "Critical")) {
            isCrit = true;
            restore = (int)(maxDamageRange * CRIT_DAMAGE_MODIFIER);
        } else {
            restore = Random.Range(minDamageRange, maxDamageRange + 1);
        }
        
        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (healTarget.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(healTarget, token.ToString());
                }
            }
        }
        
        // Change target position
        if (targetXTravel != 0 || targetYTravel != 0) {
            StartCoroutine(SetGridPosition(healTarget, targetXTravel, targetYTravel));
        }
        
        if (!abilityDuplicated) {
            SelfGain(healer, activeAbility, isCrit);
        }
        
        // Check for Anti-Heal
        if (IgnoreRestoreWithTokens(healer, healTarget)) {
            restore *= (int)antiHealToken.tokenValue;
            secondaryRestore *= (int)antiHealToken.tokenValue;
        }
        
        // Apply on crit tokens if attack crit
        if (isCrit) {
            SetAbilityCritTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
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
        
        // Apply tokens to self
        if (!abilityDuplicated) {
            for (int i = 0; i < selfTokens.Count; i++) {
                AddTokens(healer, healer, selfTokens[i].tokenName, selfTokensCount[i], 0);
            }
        }
        // Apply target tokens
        for (int i = 0; i < targetTokens.Count; i++) {
            AddTokens(healer, healTarget, targetTokens[i].tokenName, targetTokensCount[i],
                healer.resistPierce);
        }
        
        healTarget.battleVisuals.PlayHealAnimation(restore, isCrit); // target plays on heal animation
        if (!abilityDuplicated && extraCastCount == 0) {
            // Eventual animation call
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        }
        
        // Heal the target
        healTarget.currentDefense += restore; // restore HP
        
        if (!abilityDuplicated) {
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        }
        SaveResources();
        // Update the UI
        if (healTarget.isPlayer) {
            healTarget.UpdatePlayerUI();
            healer.UpdatePlayerUI();
        } else if (!healTarget.isPlayer) {
            healTarget.UpdateEnemyUI();
            healer.UpdateEnemyUI();
        }
        
        if (activeAbility.attackType == Ability.AttackType.Ranged &&
            healer.activeTokens.Any(t => t.tokenName == "Ricochet") &&
            !abilityDuplicated) {
            List<BattleEntity> ricochetTargetList = new List<BattleEntity>();
            foreach (BattleEntity entity in enemyCombatants) {
                int distance = CalculateTargetDistance(entity, healTarget);
                if (distance <= 2 && distance >= 1) {
                    ricochetTargetList.Add(entity);
                }
            }
            
            if (ricochetTargetList.Count > 0) {
                int ricochetTargetIndex = Random.Range(0, ricochetTargetList.Count);
                abilityDuplicated = true;
                duplicationType = DuplicationType.Ricochet;
                yield return StartCoroutine(HealAction(healer, ricochetTargetList[ricochetTargetIndex], activeAbilityIndex));
            }
        }
        
        // Check for extra casts
        if (extraCastCount < activeAbility.extraCasts) {
            extraCastCount++;
            yield return StartCoroutine(DamageAction(healer, healTarget, activeAbilityIndex));
        }
    }

    private IEnumerator BuffAction(BattleEntity buffer, BattleEntity buffTarget, int activeAbilityIndex)
    {
        Ability activeAbility = buffer.myAbilities[activeAbilityIndex];
        
        int abilityModifier = 0;
        bool isCrit = false;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        int critChance = 0;
        
        // Get ability values
        SetAbilityValuesAgainstTarget(buffer, buffTarget, ref abilityModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        RunBuffAgainstSelfTokens(buffer, ref isCrit, ref acc, ref critChance);
        
        // Determine move distance
        int selfXTravel = activeAbility.selfXChange;
        int selfYTravel = 0;
        int targetXTravel = activeAbility.targetXChange;
        int targetYTravel = 0;
        SetYMovement(buffer, buffTarget, activeAbility, ref selfYTravel, ref targetYTravel);
        
        // Determine which and how many tokens will be applied by the ability
        List<BattleToken> targetTokens = new List<BattleToken>();
        List<int> targetTokensCount = new List<int>();
        List<BattleToken> selfTokens = new List<BattleToken>();
        List<int> selfTokensCount = new List<int>();

        SetAbilityTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
        
        // Check character logic
        if (buffer.myName == "Renée") {
            int secondaryValue = 0;
            repentantLogic.RepentantAbilityLogic(buffer, buffTarget, activeAbility, ref minDamageRange,
                ref maxDamageRange, ref secondaryValue, ref critChance, ref selfTokens, ref selfTokensCount,
                ref targetTokens, ref targetTokensCount);
        } else if (buffer.myName == "Tre") {
            ricochetLogic.RicochetBuffLogic(buffer, activeAbility, ref selfTokens, ref selfTokensCount);
        }
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0 && !abilityDuplicated) {
            foreach (Ability.TokenOption token in activeAbility.selfTokensCleared) {
                if (buffer.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(buffer, token.ToString());
                }
            }
        }
        
        // Change self position
        if ((selfXTravel != 0 || selfYTravel != 0) && !abilityDuplicated) {
            if (buffer.activeTokens.All(t => t.tokenName != "Restrict")) {
                StartCoroutine(SetGridPosition(buffer, selfXTravel, selfYTravel));
            }
        }
        
        // Check for hit
        if (activeAbility.hasAccuracy) {
            int accRoll = Random.Range(1, 101);
            if (accRoll > (int)acc) {
                buffTarget.battleVisuals.AbilityMisses();
                RemoveTokensOnMiss(buffTarget);
                if (!abilityDuplicated) {
                    SelfGain(buffer, activeAbility, isCrit);
                    // Apply tokens to self
                    if (!abilityDuplicated) {
                        for (int i = 0; i < selfTokens.Count; i++) {
                            AddTokens(buffer, buffer, selfTokens[i].tokenName, selfTokensCount[i], 0);
                        }
                    }
                }
                yield return new WaitForSeconds(TURN_ACTION_DELAY);
                
                if (!abilityDuplicated) {
                    yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
                }
                SaveResources();
                
                // Check for extra casts
                if (extraCastCount < activeAbility.extraCasts) {
                    extraCastCount++;
                    yield return StartCoroutine(DamageAction(buffer, buffTarget, activeAbilityIndex));
                }
                
                yield break;
            }
        }
        
        int critRoll = Random.Range(1, 101);
        if (critRoll <= critChance) {
            isCrit = true;
        }
        
        
        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (buffTarget.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(buffTarget, token.ToString());
                }
            }
        }
        
        // Change target position
        if (targetXTravel != 0 || targetYTravel != 0) {
            StartCoroutine(SetGridPosition(buffTarget, targetXTravel, targetYTravel));
        }
        
        RemoveSelfBuffTokens(buffer);
        RemoveTargetBuffTokens(buffTarget);
        
        if (!abilityDuplicated) {
            SelfGain(buffer, activeAbility, isCrit);
        }

        // Apply on crit tokens if attack crit
        if (isCrit) {
            SetAbilityCritTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
        }
        
        // Apply tokens to self
        if (!abilityDuplicated) {
            for (int i = 0; i < selfTokens.Count; i++) {
                AddTokens(buffer, buffer, selfTokens[i].tokenName, selfTokensCount[i], 0);
            }
        }
        // Apply target tokens
        for (int i = 0; i < targetTokens.Count; i++) {
            AddTokens(buffer, buffTarget, targetTokens[i].tokenName, targetTokensCount[i],
                buffer.resistPierce);
        }
        
        if (!abilityDuplicated && extraCastCount == 0) {
            // Eventual animation call
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        }
        
        if (!abilityDuplicated) {
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        }
        SaveResources();
        
        if (activeAbility.attackType == Ability.AttackType.Ranged &&
            buffer.activeTokens.Any(t => t.tokenName == "Ricochet") &&
            !abilityDuplicated) {
            List<BattleEntity> ricochetTargetList = new List<BattleEntity>();
            foreach (BattleEntity entity in enemyCombatants) {
                int distance = CalculateTargetDistance(entity, buffTarget);
                if (distance <= 2 && distance >= 1) {
                    ricochetTargetList.Add(entity);
                }
            }

            if (ricochetTargetList.Count > 0) {
                int ricochetTargetIndex = Random.Range(0, ricochetTargetList.Count);
                abilityDuplicated = true;
                duplicationType = DuplicationType.Ricochet;
                yield return StartCoroutine(BuffAction(buffer, ricochetTargetList[ricochetTargetIndex], activeAbilityIndex));
            }
        }
        
        // Check for extra casts
        if (extraCastCount < activeAbility.extraCasts) {
            extraCastCount++;
            yield return StartCoroutine(DamageAction(buffer, buffTarget, activeAbilityIndex));
        }
    }
    
    private IEnumerator DebuffAction(BattleEntity debuffer, BattleEntity debuffTarget, int activeAbilityIndex)
    {
        Ability activeAbility = debuffer.myAbilities[activeAbilityIndex];
        
        int abilityModifier = 0;
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
        
        // Get secondary damage values
        SetSecondaryAbilityValues(debuffer, ref secondaryModifier,  ref secondaryValue); 
        secondaryDamage = secondaryValue;
        
        // Get ability values
        SetAbilityValuesAgainstTarget(debuffer, debuffTarget, ref abilityModifier, ref isCrit, ref acc, ref minDamageRange,
            ref maxDamageRange, ref critChance);
        
        RunDebuffAgainstSelfTokens(debuffer, ref isCrit, ref acc, ref critChance);
        RunDebuffAgainstTargetTokens(debuffer, debuffTarget, ref isCrit, ref acc, ref critChance);
        
        // Determine move distance
        int selfXTravel = activeAbility.selfXChange;
        int selfYTravel = 0;
        int targetXTravel = activeAbility.targetXChange;
        int targetYTravel = 0;
        SetYMovement(debuffer, debuffTarget, activeAbility, ref selfYTravel, ref targetYTravel);
        
        // Determine which and how many tokens will be applied by the ability
        List<BattleToken> targetTokens = new List<BattleToken>();
        List<int> targetTokensCount = new List<int>();
        List<BattleToken> selfTokens = new List<BattleToken>();
        List<int> selfTokensCount = new List<int>();

        SetAbilityTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
        
        if (debuffer.myName == "Renée") {
            repentantLogic.RepentantAbilityLogic(debuffer, debuffTarget, activeAbility, ref minDamageRange,
                ref maxDamageRange, ref secondaryDamage, ref critChance, ref selfTokens, ref selfTokensCount,
                ref targetTokens, ref targetTokensCount);
        }
        
        // Reduce crit chance by target's crit resist
        critChance -= debuffTarget.critResist;
        if (critChance < 0) {
            critChance = 0;
        }
        
        // Clear tokens from self
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (debuffer.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(debuffer, token.ToString());
                }
            }
        }
        
        // Change self position
        if ((selfXTravel != 0 || selfYTravel != 0) && !abilityDuplicated) {
            if (debuffer.activeTokens.All(t => t.tokenName != "Restrict")) {
                StartCoroutine(SetGridPosition(debuffer, selfXTravel, selfYTravel));
            }
        }
        
        // Check for hit
        if (activeAbility.hasAccuracy) {
            int accRoll = Random.Range(1, 101);
            if (accRoll > (int)acc) {
                debuffTarget.battleVisuals.AbilityMisses();
                RemoveTokensOnMiss(debuffTarget);
                if (!abilityDuplicated) {
                    SelfGain(debuffer, activeAbility, isCrit);
                    // Apply tokens to self
                    if (!abilityDuplicated) {
                        for (int i = 0; i < selfTokens.Count; i++) {
                            AddTokens(debuffer, debuffer, selfTokens[i].tokenName, selfTokensCount[i], 0);
                        }
                    }
                }
                yield return new WaitForSeconds(TURN_ACTION_DELAY);
        
                if (!abilityDuplicated) {
                    yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
                }
                SaveResources();
                
                // Check for extra casts
                if (extraCastCount < activeAbility.extraCasts) {
                    extraCastCount++;
                    yield return StartCoroutine(DamageAction(debuffer, debuffTarget, activeAbilityIndex));
                }
                
                yield break;
            }
        }
        
        int critRoll = Random.Range(1, 101);
        if (critRoll <= critChance) {
            isCrit = true;
        }
        
        // Apply on crit tokens if attack crit
        if (isCrit) {
            SetAbilityCritTokens(ref targetTokens, ref targetTokensCount, ref selfTokens, ref selfTokensCount, activeAbility);
        }
        
        // Clear tokens from the target
        if (activeAbility.targetTokensCleared.Length > 0) {
            foreach (Ability.TokenOption token in activeAbility.targetTokensCleared) {
                if (debuffTarget.activeTokens.Any(t => t.tokenName == token.ToString())) {
                    ClearTokens(debuffTarget, token.ToString());
                }
            }
        }
        
        // Change target position
        if (targetXTravel != 0 || targetYTravel != 0) {
            StartCoroutine(SetGridPosition(debuffTarget, targetXTravel, targetYTravel));
        }
        
        RemoveSelfDebuffTokens(debuffTarget);
        RemoveTargetDebuffTokens(debuffTarget);
        
        if (!abilityDuplicated) {
            SelfGain(debuffer, activeAbility, isCrit);
        }
        
        // Apply tokens to self
        if (!abilityDuplicated) {
            for (int i = 0; i < selfTokens.Count; i++) {
                AddTokens(debuffer, debuffer, selfTokens[i].tokenName, selfTokensCount[i], 0);
            }
        }
        // Apply target tokens
        for (int i = 0; i < targetTokens.Count; i++) {
            AddTokens(debuffer, debuffTarget, targetTokens[i].tokenName, targetTokensCount[i],
                debuffer.resistPierce);
        }
        
        // Apply Secondary Damage
        if (secondaryTarget != "Null") {
            switch (secondaryTarget) {
                case "Bonus":
                    print("Bonus does not work for debuffs");
                    break;
                case "Spirit":
                    debuffTarget.currentSpirit -= secondaryDamage;
                    if (debuffTarget.currentSpirit < 0) {
                        debuffTarget.currentSpirit = 0;
                    }
                    break;
                case "Armor":
                    debuffTarget.currentArmor -= secondaryDamage;
                    if (debuffTarget.currentArmor < 0) {
                        debuffTarget.currentArmor = 0;
                    }
                    break;
                case "ActionPoints":
                    debuffTarget.actionPoints -= secondaryDamage;
                    if (debuffTarget.actionPoints < 0) {
                        debuffTarget.actionPoints = 0;
                    }
                    break;
                default:
                    print("Invalid secondary target of " +  secondaryTarget + " supplied");
                    break;
            }
        }
        
        if (!abilityDuplicated && extraCastCount == 0) {
            // Eventual animation call
            yield return new WaitForSeconds(TURN_ACTION_DELAY);
        }
        
        if (!abilityDuplicated) {
            yield return StartCoroutine(ConsumeResources(activeAbilityIndex));
        }
        SaveResources();
        
        if (activeAbility.attackType == Ability.AttackType.Ranged &&
            debuffer.activeTokens.Any(t => t.tokenName == "Ricochet") &&
            !abilityDuplicated) {
            List<BattleEntity> ricochetTargetList = new List<BattleEntity>();
            foreach (BattleEntity entity in enemyCombatants) {
                int distance = CalculateTargetDistance(entity, debuffTarget);
                if (distance <= 2 && distance >= 1) {
                    ricochetTargetList.Add(entity);
                }
            }

            if (ricochetTargetList.Count > 0) {
                int ricochetTargetIndex = Random.Range(0, ricochetTargetList.Count);
                abilityDuplicated = true;
                duplicationType = DuplicationType.Ricochet;
                yield return StartCoroutine(DebuffAction(debuffer, ricochetTargetList[ricochetTargetIndex], activeAbilityIndex));
            }
        }
        
        // Check for extra casts
        if (extraCastCount < activeAbility.extraCasts) {
            extraCastCount++;
            yield return StartCoroutine(DamageAction(debuffer, debuffTarget, activeAbilityIndex));
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
    
    private void SelfGain(BattleEntity self, Ability ability, bool isCrit)
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
        foreach (var t in allCombatants) {
            if (t.currentHealth < 0) {
                t.currentHealth = 0;
            } else if (t.currentHealth > t.maxHealth) {
                t.currentHealth = t.maxHealth;
            }
            if (t.currentSpirit < 0) {
                t.currentSpirit = 0;
            } else if (t.currentSpirit > t.maxSpirit) {
                t.currentSpirit = t.maxSpirit;
            }
            if (t.currentDefense < 0) {
                t.currentDefense = 0;
            } else if (t.currentDefense > t.maxDefense) {
                t.currentDefense = t.maxDefense;
            }
            if (t.currentArmor < 0) {
                t.currentArmor = 0;
            } else if (t.currentArmor > t.maxArmor) {
                t.currentArmor = t.maxArmor;
            }
        }

        yield break;
    }
}

[Serializable]
public class BattleEntity
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
    public bool myFirstTurn = true;
    public bool wasDamagedLastTurn;
    public int damagedBy;
    
    public string activeAbilityType;
    public int target;
    public int activeAbility;
    public int level;
    
    public int xPos;
    public int yPos;
    
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

    // Resistances
    public int critChance;
    public int resistPierce;
    public int stunResist;
    public int debuffResist;
    public int critResist;
    public int ailmentResist;
    
    // Enemy Only
    public EnemyBrain enemyBrain;
    
    // Player Only
    public Token lineBreakToken;
    public int lineBreakTokenCount;
    
    public BattleVisuals battleVisuals;
    public CombatMenuVisuals combatMenuVisuals;
    public GameObject myVisuals;

    public GameObject[] abilityButtons;
    public GameObject[] targetButtons;
    public GameObject[] targetPortraits;
    public GameObject[] targetBorders;
    public List<Ability> myAbilities;
    public List<int> abilityCooldowns;
    public List<BattleToken> activeTokens;

    public void SetEntityValue(string entityName, Sprite entityPortrait, int entityLevel, int entityXPos, int entityYPos, 
        int entityMaxHealth, int entityCurrentHealth, int entityMaxSpirit, int entityCurrentSpirit, int entityMaxDefense, 
        int entityMaxArmor, int entityPower, int entitySkill, int entityWit, int entityMind, int entitySpeed, int entityLuck,
        int entityStunResist, int entityDebuffResist, int entityAilmentResist, bool entityIsPlayer)
    {
        myName = entityName;
        myPortrait = entityPortrait;
        isPlayer = entityIsPlayer;
        level = entityLevel;

        xPos = entityXPos;
        yPos = entityYPos;

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