using System.Collections.Generic;
using UnityEngine;
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

    private List<Image> turnPortraitImages = new List<Image>();
    private List<Image> turnBorderImages = new List<Image>();
    private BattleSystem battleSystem;
    
    private const float TURN_START_THRESHOLD = 200f;
    private float yChange;
    private float xChange;

    public void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
        Image tempImage;
        foreach (var turnPortrait in turnPortraits) {
            tempImage = turnPortrait.GetComponent<Image>();
            turnPortraitImages.Add(tempImage);
        }
        foreach (var turnBorder in turnBorders) {
            tempImage = turnBorder.GetComponent<Image>();
            turnBorderImages.Add(tempImage);
        }
    }

    public void Start()
    {
        RectTransform imageRect = turnDisplays[1].GetComponent<RectTransform>();
        yChange = imageRect.rect.height / 2;
        xChange = imageRect.rect.width / 10;
    }
    
    public void SetTurnDisplay(List<BattleEntity> turnOrder)
    {
        for (int i = 0; i < turnPortraits.Length; i++) {
            turnPortraitImages[i].sprite = turnOrder[i].myPortrait;
            if (turnOrder[i].isPlayer) {
                turnBorderImages[i].color = new Color32(147, 229, 242, 255);
            } else {
                turnBorderImages[i].color = new Color32(255, 0, 0, 255);
            }

            if (turnOrder[i].actionPoints < (-1 * TURN_START_THRESHOLD)) {
                darkApMeters[i].fillAmount = (turnOrder[i].actionPoints + (TURN_START_THRESHOLD * 2)) / TURN_START_THRESHOLD;
                mediumApMeters[i].fillAmount = 0;
                lightApMeters[i].fillAmount = 0;
            } else if (turnOrder[i].actionPoints < 0) {
                darkApMeters[i].fillAmount = 1;
                mediumApMeters[i].fillAmount = (turnOrder[i].actionPoints + TURN_START_THRESHOLD) / TURN_START_THRESHOLD;
                lightApMeters[i].fillAmount = 0;
            } else {
                darkApMeters[i].fillAmount = 1;
                mediumApMeters[i].fillAmount = 1;
                lightApMeters[i].fillAmount = turnOrder[i].actionPoints / TURN_START_THRESHOLD;
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
            transformPosition.y -= yChange;
            transformPosition.x -= xChange;
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
