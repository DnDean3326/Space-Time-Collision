using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderDisplay : MonoBehaviour
{
    [SerializeField] private GameObject[] turnDisplays;
    
    private BattleSystem battleSystem;

    public void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }
    
    public void SetTurnDisplay(List<BattleEntities> turnOrder)
    {
        for (int i = 0; i < turnDisplays.Length; i++) {
            turnDisplays[i].GetComponent<Image>().sprite = turnOrder[i].myPortrait;
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
