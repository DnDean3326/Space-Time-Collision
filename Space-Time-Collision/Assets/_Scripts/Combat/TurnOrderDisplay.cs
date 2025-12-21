using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderDisplay : MonoBehaviour
{
    [SerializeField] private GameObject turnOrderUI;
    [SerializeField] private GameObject[] turnDisplays;
    [SerializeField] private GameObject[] turnBorders;
    
    private BattleSystem battleSystem;

    public void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }
    
    public void SetTurnDisplay(List<BattleEntities> turnOrder)
    {
        for (int i = 0; i < turnDisplays.Length; i++) {
            turnDisplays[i].GetComponent<Image>().sprite = turnOrder[i].myPortrait;
            if (turnOrder[i].isPlayer) {
                turnBorders[i].GetComponent<Image>().color = new Color32(147, 229, 242, 255);
            } else {
                turnBorders[i].GetComponent<Image>().color = new Color32(255, 0, 0, 255);
            }
        }
    }
    
    // Button OnHover methods
    
    public void TargetIndicate(int hoveredTarget)
    {
        battleSystem.IndicateTurnTarget(hoveredTarget);
    }
    
    public void TargetIndicateRemove(int hoveredTarget)
    {
        battleSystem.StopIndicatingTurnTarget(hoveredTarget);
    }
}
