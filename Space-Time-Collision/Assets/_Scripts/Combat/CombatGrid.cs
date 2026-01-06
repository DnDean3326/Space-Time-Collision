using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatGrid : MonoBehaviour
{
    [Header("Grid Arrays")]
    [SerializeField] private GridTile[] partyGrid;
    [SerializeField] private GridTile[] enemyGrid;
    [SerializeField] private bool[] lineBroken;
    
    [Header("Grid Set-Up")]
    [SerializeField] private Transform[] partyGridTransform;
    [SerializeField] private GameObject[] partyGridObjects;
    [SerializeField] private Transform[] enemyGridTransforms;
    [SerializeField] private GameObject[] enemyGridObjects;
    [SerializeField] private Sprite[] destroyedGridSprites;

    private readonly Color damageColor = new Color32(177, 2, 37, 255);
    private readonly Color healColor = new Color32(83, 183, 122, 255);
    private readonly Color buffColor = new Color32(28, 113, 162, 255);
    private readonly Color debuffColor = new Color32(193, 93, 37, 255);
    private readonly Color movementColor = new Color32(255,255,255, 255);
    
    private BattleSystem battleSystem;
    private List<BattleEntities> targetList = new List<BattleEntities>();
    private const int GRID_Y_MAX = 4;
    private const int GRID_X_MAX = 4;
    private const int GRID_COUNT = 16;

    private const float UNUSABLE_TRANSPARENCY = 0.6f;

    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }
    
    private void Start()
    {
        lineBroken = new bool[8];
        for (int i = 0; i < lineBroken.Length; i++) {
            lineBroken[i] = false;
        }
    }

    public bool[] GetLineBreakInfo()
    {
        return lineBroken;
    }
    
    public void GetGridInfo(ref List<GridTile> partyBattleGrid, ref List<GridTile> enemyBattleGrid)
    {
        for (int i = 0; i < partyGrid.Length; i++) {
            partyGrid[i].isOccupied = false;
            partyGrid[i].occupiedBy = null;
            partyGrid[i].gridVisual = partyGridObjects[i];
            partyGrid[i].gridVisual.GetComponent<Button>().enabled = false;
            partyGrid[i].gridVisual.GetComponent<EventTrigger>().enabled = false;
            partyGrid[i].gridVisual.SetActive(false);
            partyGrid[i].gridTransform = partyGridTransform[i];
            partyGrid[i].xPos = (i % GRID_X_MAX) + 1;
            partyGrid[i].yPos = (int)Math.Floor((decimal)(i / GRID_Y_MAX)) + 1;
            partyBattleGrid.Add(partyGrid[i]);
            
            enemyGrid[i].isOccupied = false;
            enemyGrid[i].occupiedBy = null;
            enemyGrid[i].gridVisual = enemyGridObjects[i];
            enemyGrid[i].gridVisual.GetComponent<Button>().enabled = false;
            enemyGrid[i].gridVisual.GetComponent<EventTrigger>().enabled = false;
            enemyGrid[i].gridVisual.SetActive(false);
            enemyGrid[i].gridTransform = enemyGridTransforms[i];
            enemyGrid[i].xPos = (i % GRID_X_MAX) + 5;
            enemyGrid[i].yPos = (int)Math.Floor((decimal)(i / GRID_Y_MAX)) + 1;
            enemyBattleGrid.Add(enemyGrid[i]);
        }
        SetGridImages(Ability.AbilityType.Heal, false);
        SetGridImages(Ability.AbilityType.Damage, true);
    }

    public bool IsRowEmpty(int row)
    {
        if (row <= GRID_X_MAX) {
            foreach (GridTile tile in partyGrid) {
                if (tile.xPos == row && tile.isOccupied) {
                    return false;
                }
            }
            return true;
        } else {
            foreach (GridTile tile in enemyGrid) {
                if (tile.xPos == row && tile.isOccupied) {
                    return false;
                }
            }
            return true;
        }
    }

    public void LineBreak(int row)
    {
        print("Row go boom");
        if (IsRowEmpty(row)) {
            lineBroken[row - 1] = true;
            if (row <= GRID_X_MAX) {
                // Affecting player side
                battleSystem.playerXMax = (GRID_X_MAX - (GRID_X_MAX - row + 1));
                print("playerXMax = " + battleSystem.playerXMax);
                Color tempColor = movementColor;
                for (int i = 0; i < partyGrid.Length; i++) {
                    GridTile tile = partyGrid[i];
                    if (tile.xPos == row) {
                        tile.isDestroyed = true;

                        tile.gridVisual.GetComponent<Image>().sprite = destroyedGridSprites[i];
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                        tile.gridVisual.GetComponent<Button>().enabled = false;
                        tile.gridVisual.GetComponent<EventTrigger>().enabled = false;

                        tile.gridVisual.SetActive(true);
                    }
                }
            } else {
                // Affecting enemy side
                battleSystem.enemyXMin = (row - GRID_X_MAX) + GRID_X_MAX;
                Color tempColor = movementColor;
                for (int i = 0; i < enemyGrid.Length; i++) {
                    GridTile tile = enemyGrid[i];
                    if (tile.xPos == row) {
                        tile.isDestroyed = true;

                        tile.gridVisual.GetComponent<Image>().sprite = destroyedGridSprites[i + GRID_COUNT];
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                        tile.gridVisual.GetComponent<Button>().enabled = false;
                        tile.gridVisual.GetComponent<EventTrigger>().enabled = false;

                        tile.gridVisual.SetActive(true);
                    }
                }
            }
        }
    }

    public void DisplayValidRowBreakTiles(BattleEntities user, int frontRow)
    {
        int moveRange = user.myAbilities[user.activeAbility].range;
        int distance;
        
        foreach (GridTile tile in enemyGrid) {
            if (tile.xPos != frontRow) { continue; }
            distance = battleSystem.CalculateTileDistance(tile, user);
            Color tempColor = tile.gridVisual.GetComponent<Image>().color;
            if (moveRange >= distance) {
                tile.gridVisual.SetActive(true);
                tempColor.a = 1f;
                tile.gridVisual.GetComponent<Image>().color = tempColor;
                tile.gridVisual.GetComponent<Button>().enabled = true;
                tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
            }
        }
    }

    public void DisplayValidTiles(BattleEntities user, Ability.AbilityType abilityType,
        bool targetingEnemy, bool canTargetSelf)
    {
        int abilityRange = user.myAbilities[user.activeAbility].range;
        int distance;
        
        SetGridImages(abilityType, targetingEnemy);

        if (targetingEnemy) {
            List<int> stealthedTargets = new List<int>();

            for (int i = 0; i < enemyGrid.Length; i++) {
                if (enemyGrid[i].isDestroyed) { continue; }
                GridTile tile = enemyGrid[i];
                distance = battleSystem.CalculateTileDistance(tile, user);
                if (abilityRange >= distance) {
                    if (tile.isOccupied && tile.occupiedBy.activeTokens.Any(t => t.tokenName == "Stealth")) {
                        stealthedTargets.Add(i);
                    } else {
                        tile.gridVisual.SetActive(true);
                    }

                    Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                    if (tile.isOccupied) {
                        if (abilityType == Ability.AbilityType.Movement &&
                            tile.occupiedBy.activeTokens.Any(t => t.tokenName == "Restrict")) {
                            tempColor.a = UNUSABLE_TRANSPARENCY;
                            tile.gridVisual.GetComponent<Image>().color = tempColor;
                            continue;
                        }
                        tempColor.a = 1f;
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                        tile.gridVisual.GetComponent<Button>().enabled = true;
                        tile.gridVisual.GetComponent<EventTrigger>().enabled = true;
                    } else {
                        tempColor.a = UNUSABLE_TRANSPARENCY;
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                        tile.gridVisual.GetComponent<Button>().enabled = false;
                        tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                    }
                }
            }
            targetList = battleSystem.GetEnemyList();
            if (stealthedTargets.Count == targetList.Count) {
                foreach (var t in stealthedTargets) {
                    Color tempColor = enemyGrid[t].gridVisual.GetComponent<Image>().color;

                    if (enemyGrid[t].isOccupied) {
                        enemyGrid[t].gridVisual.SetActive(true);
                        tempColor.a = 1f;
                        enemyGrid[t].gridVisual.GetComponent<Image>().color = tempColor;
                        enemyGrid[t].gridVisual.GetComponent<Button>().enabled = true;
                        enemyGrid[t].gridVisual.GetComponent<EventTrigger>().enabled = true;
                    }
                }
            }
        } else {
            foreach (GridTile tile in partyGrid) {
                if (tile.isDestroyed) { continue; }
                distance = battleSystem.CalculateTileDistance(tile, user);
                if (abilityRange >= distance) {
                    tile.gridVisual.SetActive(true);
                    Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                    if (tile.isOccupied) {
                        if (tile.occupiedBy == user && !canTargetSelf) {
                            tempColor.a = UNUSABLE_TRANSPARENCY;
                            tile.gridVisual.GetComponent<Image>().color = tempColor;
                            tile.gridVisual.SetActive(false);
                        } else if (abilityType == Ability.AbilityType.Movement && 
                                   tile.occupiedBy.activeTokens.Any(t => t.tokenName == "Restrict")) {
                            tempColor.a = UNUSABLE_TRANSPARENCY;
                            tile.gridVisual.GetComponent<Image>().color = tempColor;
                            tile.gridVisual.SetActive(false);
                        } else {
                            tempColor.a = 1f;
                            tile.gridVisual.GetComponent<Image>().color = tempColor;
                        
                            tile.gridVisual.GetComponent<Button>().enabled = true;
                            tile.gridVisual.GetComponent<EventTrigger>().enabled = true;
                        }
                    } else {
                        tempColor.a = UNUSABLE_TRANSPARENCY;
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                        tile.gridVisual.GetComponent<Button>().enabled = false;
                        tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                    }
                }
            }
        }
    }

    public void HideTiles(bool targetingEnemy)
    {
        if (targetingEnemy) {
            foreach (GridTile tile in enemyGrid) {
                if (tile.isDestroyed) { continue; }
                tile.gridVisual.SetActive(false);
            }
        } else {
            foreach (GridTile tile in partyGrid) {
                if (tile.isDestroyed) { continue; }
                tile.gridVisual.SetActive(false);
            }
        }
    }

    public void SetGridMovementButtons(bool targetingEnemy)
    {
        if (targetingEnemy) {
            foreach (GridTile tile in enemyGrid) {
                if (tile.isDestroyed) { continue; }
                if (tile.gridVisual.activeSelf) {
                    Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                    tempColor.a = 1f;
                    tile.gridVisual.GetComponent<Image>().color = tempColor;
                    
                    tile.gridVisual.GetComponent<Button>().enabled = true;
                    tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                }
            }
        } else {
            foreach (GridTile tile in partyGrid) {
                if (tile.isDestroyed) { continue; }
                if (tile.gridVisual.activeSelf) {
                    Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                    tempColor.a = 1f;
                    tile.gridVisual.GetComponent<Image>().color = tempColor;
                    
                    tile.gridVisual.GetComponent<Button>().enabled = true;
                    tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                }
            }
        }
    }

    public void DisableGridButtons(bool targetingEnemy)
    {
        if (targetingEnemy) {
            foreach (GridTile tile in enemyGrid) {
                if (tile.isDestroyed) { continue; }
                if (tile.gridVisual.GetComponent<Button>().enabled) {
                    tile.gridVisual.GetComponent<Button>().enabled = false;
                    tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                }
            }
        } else {
            foreach (GridTile tile in partyGrid) {
                if (tile.isDestroyed) { continue; }
                if (tile.gridVisual.GetComponent<Button>().enabled) {
                    tile.gridVisual.GetComponent<Button>().enabled = false;
                    tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                }
            }
        }
    }

    private void SetGridImages(Ability.AbilityType abilityType, bool targetingEnemy)
    {
        if (!targetingEnemy) {
            switch (abilityType)
            {
                case Ability.AbilityType.Damage:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        if (partyGrid[i].isDestroyed) { continue; }
                        partyGrid[i].gridVisual.GetComponent<Image>().color = damageColor;
                    }
                    break;
                case Ability.AbilityType.Heal:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        if (partyGrid[i].isDestroyed) { continue; }
                        partyGrid[i].gridVisual.GetComponent<Image>().color = healColor;
                    }
                    break;
                case Ability.AbilityType.Buff:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        if (partyGrid[i].isDestroyed) { continue; }
                        partyGrid[i].gridVisual.GetComponent<Image>().color = buffColor;
                    }
                    break;
                case Ability.AbilityType.Debuff:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        if (partyGrid[i].isDestroyed) { continue; }
                        partyGrid[i].gridVisual.GetComponent<Image>().color = debuffColor;
                    }
                    break;
                case Ability.AbilityType.Movement:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        if (partyGrid[i].isDestroyed) { continue; }
                        partyGrid[i].gridVisual.GetComponent<Image>().color = movementColor;
                    }
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        if (enemyGrid[i].isDestroyed) { continue; }
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = damageColor;
                    }
                    break;
            }
        } else {
            switch (abilityType)
            {
                case Ability.AbilityType.Damage:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        if (enemyGrid[i].isDestroyed) { continue; }
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = damageColor;
                    }
                    break;
                case Ability.AbilityType.Heal:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        if (enemyGrid[i].isDestroyed) { continue; }
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = healColor;
                    }
                    break;
                case Ability.AbilityType.Buff:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        if (enemyGrid[i].isDestroyed) { continue; }
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = buffColor;
                    }
                    break;
                case Ability.AbilityType.Debuff:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        if (enemyGrid[i].isDestroyed) { continue; }
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = debuffColor;
                    }
                    break;
                case Ability.AbilityType.Movement:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        if (enemyGrid[i].isDestroyed) { continue; }
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = movementColor;
                    }
                    break;
            }
        }
    }
    
    // OnClick Method

    public void GetTileID(int positionIndex)
    {
        battleSystem.MoveOrTargetCheck(positionIndex);
    }
    
    // OnHover Methods

    public void IndicateTarget(int positionIndex)
    {
        battleSystem.IndicateGridTarget(positionIndex);
    }

    public void StopIndicatingTarget(int positionIndex)
    {
        battleSystem.StopIndicatingGridTarget(positionIndex);
    }
}

[Serializable]
public class GridTile
{
    public bool isDestroyed = false;
    public bool isOccupied;
    public BattleEntities occupiedBy;
    public GameObject gridVisual;
    public Transform gridTransform;
    public int xPos;
    public int yPos;
}
