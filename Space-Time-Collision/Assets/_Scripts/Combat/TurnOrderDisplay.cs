using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TurnOrderDisplay : MonoBehaviour
{
    [SerializeField] private GameObject turnOrderUI;
    [SerializeField] private GameObject[] turnDisplays;
    [SerializeField] private Transform[] turnDisplaysBasePosition;
    [SerializeField] private GameObject[] turnPortraits;
    [SerializeField] private GameObject[] turnBorders;
    [SerializeField] private Image[] darkApMeters;
    [SerializeField] private Image[] mediumApMeters;
    [SerializeField] private Image[] lightApMeters;
    
    private BattleSystem battleSystem;
    
    private const float TURN_START_THRESHOLD = 200f;
    private const float Y_CHANGE = -105.6f;

    public void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }
    
    public void SetTurnDisplay(List<BattleEntities> turnOrder)
    {
        for (int i = 0; i < turnPortraits.Length; i++) {
            turnPortraits[i].GetComponent<Image>().sprite = turnOrder[i].myPortrait;
            if (turnOrder[i].isPlayer) {
                turnBorders[i].GetComponent<Image>().color = new Color32(147, 229, 242, 255);
            } else {
                turnBorders[i].GetComponent<Image>().color = new Color32(255, 0, 0, 255);
            }

            if (turnOrder[i].actionPoints < (-1 * TURN_START_THRESHOLD)) {
                darkApMeters[i].GetComponent<Image>().fillAmount =
                    (turnOrder[i].actionPoints + (TURN_START_THRESHOLD * 2)) / TURN_START_THRESHOLD;
                mediumApMeters[i].GetComponent<Image>().fillAmount = 0;
                lightApMeters[i].GetComponent<Image>().fillAmount = 0;
            } else if (turnOrder[i].actionPoints < 0) {
                darkApMeters[i].GetComponent<Image>().fillAmount = 1;
                mediumApMeters[i].GetComponent<Image>().fillAmount =
                    (turnOrder[i].actionPoints + TURN_START_THRESHOLD) / TURN_START_THRESHOLD;
                lightApMeters[i].GetComponent<Image>().fillAmount = 0;
            } else {
                darkApMeters[i].GetComponent<Image>().fillAmount = 1;
                mediumApMeters[i].GetComponent<Image>().fillAmount = 1;
                lightApMeters[i].GetComponent<Image>().fillAmount =
                    turnOrder[i].actionPoints / TURN_START_THRESHOLD;
            }
        }
    }

    public void ShiftTurnDisplay(List<int> targetTurns)
    {
        List<int> targetsBeingDisplayed = new List<int>();
        
        foreach (int index in targetTurns) {
            if (index < 5) {
                targetsBeingDisplayed.Add(index);
            } 
        }

        foreach (int index in targetsBeingDisplayed) {
            var transformPosition = turnDisplays[index].transform.position;
            transformPosition.y += Y_CHANGE;
            turnDisplays[index].transform.position = transformPosition;
        }
    }

    public void ResetTurnDisplays()
    {
        for (int i = 0; i < turnDisplays.Length; i++) {
            var transformPosition = turnDisplaysBasePosition[i].transform.position;
            turnDisplays[i].transform.position = transformPosition;
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
