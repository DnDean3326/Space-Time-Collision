using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class CombatGrid : MonoBehaviour
{
    [Header("Grid Locations")]
    [SerializeField] private Transform[] partyGridTransform;
    [SerializeField] private GameObject[] partyGridObjects;
    [SerializeField] private GridTile[] partyGrid;
    [SerializeField] private Transform[] enemyGridTransforms;
    [SerializeField] private GameObject[] enemyGridObjects;
    [SerializeField] private GridTile[] enemyGrid;

    private readonly Color damageColor = new Color32(177, 2, 37, 255);
    private readonly Color healColor = new Color32(83, 183, 122, 255);
    private readonly Color buffColor = new Color32(36, 173, 204, 255);
    private readonly Color debuffColor = new Color32(193, 93, 37, 255);
    private readonly Color movementColor = new Color32(255,255,255, 255);
    
    private BattleSystem battleSystem;

    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
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
            partyGrid[i].xPos = (i % 4) + 1;
            partyGrid[i].yPos = (int)Math.Floor((decimal)(i / 4)) + 1;
            partyBattleGrid.Add(partyGrid[i]);
            
            enemyGrid[i].isOccupied = false;
            enemyGrid[i].occupiedBy = null;
            enemyGrid[i].gridVisual = enemyGridObjects[i];
            enemyGrid[i].gridVisual.GetComponent<Button>().enabled = false;
            enemyGrid[i].gridVisual.GetComponent<EventTrigger>().enabled = false;
            enemyGrid[i].gridVisual.SetActive(false);
            enemyGrid[i].gridTransform = enemyGridTransforms[i];
            enemyGrid[i].xPos = (i % 4) + 5;
            enemyGrid[i].yPos = (int)Math.Floor((decimal)(i / 4)) + 1;
            enemyBattleGrid.Add(enemyGrid[i]);
        }
        SetGridImages(Ability.AbilityType.Heal, false);
        SetGridImages(Ability.AbilityType.Damage, true);
    }

    public void DisplayValidTiles(BattleEntities user, Ability.AbilityType abilityType,
        bool targetingEnemy, bool canTargetSelf)
    {
        int userPosition;
        int abilityRange = user.myAbilities[user.activeAbility].range;
        int distance;
        
        for (int i = 0; i < partyGrid.Length; i++) {
            if (partyGrid[i].isOccupied) { continue; }
            if (partyGrid[i].occupiedBy == user) { userPosition = i; break; }
        }
        
        SetGridImages(abilityType, targetingEnemy);

        if (targetingEnemy) {
            foreach (GridTile tile in enemyGrid) {
                distance = Math.Abs(user.xPos - tile.xPos) + Math.Abs(user.yPos - tile.yPos);
                if (abilityRange >= distance) {
                    tile.gridVisual.SetActive(true);
                    if (tile.isOccupied) {
                        Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                        tempColor.a = 1f;
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                        
                        tile.gridVisual.GetComponent<Button>().enabled = true;
                        tile.gridVisual.GetComponent<EventTrigger>().enabled = true;
                    } else {
                        Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                        tempColor.a = 0.7f;
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                    }
                }
            }
        } else {
            foreach (GridTile tile in partyGrid) {
                distance = Math.Abs(user.xPos - tile.xPos) + Math.Abs(user.yPos - tile.yPos);
                if (abilityRange >= distance) {
                    tile.gridVisual.SetActive(true);
                    if (tile.isOccupied) {
                        Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                        if (tile.occupiedBy == user && !canTargetSelf) {
                            tempColor.a = 0.7f;
                            tile.gridVisual.GetComponent<Image>().color = tempColor;
                            tile.gridVisual.SetActive(false);
                        } else {
                            tempColor.a = 1f;
                            tile.gridVisual.GetComponent<Image>().color = tempColor;
                        
                            tile.gridVisual.GetComponent<Button>().enabled = true;
                            tile.gridVisual.GetComponent<EventTrigger>().enabled = true;
                        }
                    } else {
                        Color tempColor = tile.gridVisual.GetComponent<Image>().color;
                        tempColor.a = 0.7f;
                        tile.gridVisual.GetComponent<Image>().color = tempColor;
                    }
                }
            }
        }
    }

    public void HideTiles(bool targetingEnemy)
    {
        if (targetingEnemy) {
            foreach (GridTile tile in enemyGrid) {
                tile.gridVisual.SetActive(false);
            }
        } else {
            foreach (GridTile tile in partyGrid) {
                tile.gridVisual.SetActive(false);
            }
        }
    }

    public void SetGridMovementButtons(bool targetingEnemy)
    {
        if (targetingEnemy) {
            foreach (GridTile tile in enemyGrid) {
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
                if (tile.gridVisual.GetComponent<Button>().enabled) {
                    tile.gridVisual.GetComponent<Button>().enabled = false;
                    tile.gridVisual.GetComponent<EventTrigger>().enabled = false;
                }
            }
        } else {
            foreach (GridTile tile in partyGrid) {
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
                        partyGrid[i].gridVisual.GetComponent<Image>().color = damageColor;
                    }
                    break;
                case Ability.AbilityType.Heal:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        partyGrid[i].gridVisual.GetComponent<Image>().color = healColor;
                    }
                    break;
                case Ability.AbilityType.Buff:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        partyGrid[i].gridVisual.GetComponent<Image>().color = buffColor;
                    }
                    break;
                case Ability.AbilityType.Debuff:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        partyGrid[i].gridVisual.GetComponent<Image>().color = debuffColor;
                    }
                    break;
                case Ability.AbilityType.Movement:
                    for (int i = 0; i < partyGrid.Length; i++) {
                        partyGrid[i].gridVisual.GetComponent<Image>().color = movementColor;
                    }
                    break;
            }
        } else {
            switch (abilityType)
            {
                case Ability.AbilityType.Damage:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = damageColor;
                    }
                    break;
                case Ability.AbilityType.Heal:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = healColor;
                    }
                    break;
                case Ability.AbilityType.Buff:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = buffColor;
                    }
                    break;
                case Ability.AbilityType.Debuff:
                    for (int i = 0; i < enemyGrid.Length; i++) {
                        enemyGrid[i].gridVisual.GetComponent<Image>().color = debuffColor;
                    }
                    break;
                case Ability.AbilityType.Movement:
                    for (int i = 0; i < enemyGrid.Length; i++) {
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
    public bool isOccupied;
    public BattleEntities occupiedBy;
    public GameObject gridVisual;
    public Transform gridTransform;
    public int xPos;
    public int yPos;
}
