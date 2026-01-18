using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InnFunctionality : MonoBehaviour
{
    [Header("Ally Roster UI")]
    [SerializeField] private GameObject allySelection;
    [SerializeField] private GameObject allyPrefab;
    
    [Header("Party UI")]
    [SerializeField] private GameObject[] partyDisplays;

    [Header("Ability Select UI")]
    [SerializeField] private GameObject abilitySelection;
    [SerializeField] private GameObject abilityPrefab;
    [SerializeField] private Sprite lightBorder;
    [SerializeField] private Sprite mediumBorder;
    [SerializeField] private Sprite heavyBorder;
    
    [Header("Ally Preview")]
    [SerializeField] private GameObject allyPreview;
    [SerializeField] private Image allySprite;
    [SerializeField] private Image allyShadowPortrait;
    [SerializeField] private Image allyName;
    
    private const string BASE_SCENE = "BaseScene";
    private const float FULL_ROSTER = 6;
    private const float ALLY_ROW_MAX = 1;
    private const float ALLY_Y_SPACE = 0;
    private const float ABILITY_ROW_MAX = 3;
    private const float ABILITY_Y_SPACE = 10;
    
    private List<AllyInfo> allyList;
    private PartyManager partyManager;
    private RectTransform allySelectionRect;
    private float allyYSize;
    private RectTransform abilitySelectionRect;
    private float abilityYSize;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
    }

    private void Start()
    {
        allyList = partyManager.GetAllAllies();
        
        RectTransform allyRect = allyPrefab.GetComponent<RectTransform>();
        allyYSize = allyRect.rect.height;
        RectTransform abilityRect = abilityPrefab.GetComponent<RectTransform>();
        abilityYSize = abilityRect.rect.height;
        
        allySelectionRect = allySelection.GetComponent<RectTransform>();
        abilitySelectionRect = abilitySelection.GetComponent<RectTransform>();

        foreach (GameObject display in partyDisplays) {
            
        }
        
        CreateAllyList();
        UpdatePartyDisplay();
    }

    private void CreateAllyList()
    {
        int unlockedAllyCount = 0;
        
        foreach (AllyInfo allyInfo in allyList) {
            GameObject newAlly = Instantiate(allyPrefab, allySelection.transform);
            newAlly.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = allyInfo.allySquarePortrait;
            AllySelectButton tempButton = newAlly.GetComponent<AllySelectButton>();
            tempButton.SetMyAlly(allyInfo);
            
            unlockedAllyCount++;
        }

        if (unlockedAllyCount < FULL_ROSTER) {
            for (int i = unlockedAllyCount; i < FULL_ROSTER; i++) {
                GameObject newAlly = Instantiate(allyPrefab, allySelection.transform);
                newAlly.transform.GetChild(1).gameObject.SetActive(false);
                newAlly.GetComponent<Button>().enabled = false;
            }
        }
        
        Rect rect = allySelectionRect.rect;
        float width = rect.width;
        int rowCount = Mathf.CeilToInt(FULL_ROSTER / ALLY_ROW_MAX); 
        float height = (allyYSize * 2 + ALLY_Y_SPACE) + ((allyYSize + ALLY_Y_SPACE) * (rowCount - 2));
        
        allySelectionRect.sizeDelta = new Vector2(width, height);
    }

    private void UpdatePartyDisplay()
    {
        List<PartyMember> activeParty = partyManager.GetCurrentParty();
        for (var i = 0; i < partyDisplays.Length; i++) {
            var display = partyDisplays[i];
            GameObject tempDisplay = display.transform.GetChild(1).gameObject;
            if (i < activeParty.Count) {
                AllyInfo tempAlly = allyList.Find(t => t.allyName == activeParty[i].memberName);
                display.GetComponent<AllySelectButton>().SetMyAlly(tempAlly);
                tempDisplay.SetActive(true);
                tempDisplay.GetComponent<Image>().sprite = activeParty[i].memberSquarePortrait;
            } else {
                tempDisplay.SetActive(false);
            }
        }
    }

    public void AddPartyMember(AllyInfo ally)
    {
        List<PartyMember> activeParty = partyManager.GetCurrentParty();
        if (activeParty.Any(t => t.memberName == ally.allyName)) {
            int partyIndex = activeParty.FindIndex(t => t.memberName == ally.allyName);
            activeParty.RemoveAt(partyIndex);
        } else {
            partyManager.AddMemberToPartyByName(ally.allyName, 1);
        }
        UpdatePartyDisplay();
    }

    public void DisplayAllyInfo(AllyInfo ally)
    {
        if (!allyPreview.activeSelf) {
            allyPreview.SetActive(true);
        }
        allySprite.sprite = ally.allyCombatSprite;
        allyShadowPortrait.sprite = ally.allyShadowPortrait;
        if (ally.allySignature != null) {
            allyName.gameObject.SetActive(true);
            allyName.sprite = ally.allySignature;
        } else {
            allyName.gameObject.SetActive(false);
        }

        foreach (Transform child in abilitySelection.transform) {
            Destroy(child.gameObject);
        }
        foreach (Ability ability in ally.abilities) {
            GameObject tempAbility = Instantiate(abilityPrefab, abilitySelection.transform);
            //tempAbility.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = ability.abilityIcon;
            
            Sprite tempSprite;
            switch (ability.abilityWeight) {
                case Ability.AbilityWeight.Heavy:
                    tempSprite = heavyBorder;
                    break;
                case Ability.AbilityWeight.Medium:
                    tempSprite = mediumBorder;
                    break;
                case Ability.AbilityWeight.Light:
                    tempSprite = lightBorder;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            tempAbility.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = tempSprite;
            
            tempAbility.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = ability.abilityName;
        }
        
        Rect rect = abilitySelectionRect.rect;
        float width = rect.width;
        int rowCount = Mathf.CeilToInt(ally.abilities.Length / ABILITY_ROW_MAX); 
        float height = (abilityYSize * 2 + ABILITY_Y_SPACE) + ((abilityYSize + ABILITY_Y_SPACE) * (rowCount - 2)) + 5;
        
        abilitySelectionRect.sizeDelta = new Vector2(width, height);
    }
    
    // OnClick Methods
    
    public void ConfirmParty()
    {
        SceneManager.LoadScene(BASE_SCENE);
    }
}
