using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class InnFunctionality : MonoBehaviour
{
    [Header("Ally Roster UI")]
    [SerializeField] private GameObject allySelection;
    [SerializeField] private GameObject allyPrefab;
    
    [Header("Party UI")]
    [SerializeField] private GameObject[] partyDisplays;

    [Header("Ability Select UI")]
    [SerializeField] private GameObject abilitySelection;
    [FormerlySerializedAs("activeAbilityDisplay")] [SerializeField] private GameObject activeEquippedDisplay;
    [FormerlySerializedAs("activeAbilitySlots")] [SerializeField] private GameObject[] activeEquippedSlots;
    [SerializeField] private GameObject abilityPrefab;
    [SerializeField] private Sprite lightBorder;
    [SerializeField] private Sprite mediumBorder;
    [SerializeField] private Sprite heavyBorder;
    
    [Header("Ally Preview")]
    [SerializeField] private GameObject allyPreview;
    [SerializeField] private GameObject statDisplay;
    [SerializeField] private GameObject resistDisplay;
    [SerializeField] private Image allySprite;
    [SerializeField] private Image allyShadowPortrait;
    [SerializeField] private Image allyName;
    
    private const string BASE_SCENE = "BaseScene";
    private const float FULL_ROSTER = 6;
    private const float ALLY_ROW_MAX = 1;
    private const float ALLY_Y_SPACE = 0;
    private const float ABILITY_ROW_MAX = 3;
    private const float ABILITY_Y_SPACE = 10;
    
    private readonly Color32 levelOneColor = new Color32(206,137,70, 255);
    
    private List<AllyInfo> allyList;
    private List<AllyInfo> preppedMembers = new List<AllyInfo>();
    private AllyInfo displayedMember;
    private RectTransform allySelectionRect;
    private float allyYSize;
    private RectTransform abilitySelectionRect;
    private float abilityYSize;
    
    private PartyManager partyManager;

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
        
        CreateAllyList();
        SetPreppedMembers();
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

    private void SetPreppedMembers()
    {
        preppedMembers.Clear();
        List<PartyMember> activeParty = partyManager.GetCurrentParty();
        foreach (PartyMember member in activeParty) {
            AllyInfo ally = allyList.Find(t => t.allyName == member.memberName);
            preppedMembers.Add(ally);
        }
    }

    private void UpdatePartyDisplay()
    {
        for (int i = 0; i < partyDisplays.Length; i++) {
            var display = partyDisplays[i];
            GameObject tempDisplay = display.transform.GetChild(1).gameObject;
            if (i < preppedMembers.Count) {
                AllyInfo tempAlly = allyList.Find(t => t.allyName == preppedMembers[i].allyName);
                display.GetComponent<AllySelectButton>().SetMyAlly(tempAlly);
                tempDisplay.SetActive(true);
                tempDisplay.GetComponent<Image>().sprite = preppedMembers[i].allySquarePortrait;
            } else {
                tempDisplay.SetActive(false);
            }
        }
    }

    public void AddPartyMember(AllyInfo ally)
    {
        if (preppedMembers.Any(t => t == ally)) {
            preppedMembers.Remove(ally);
        } else {
            preppedMembers.Add(ally);
        }
        UpdatePartyDisplay();
    }

    public void AddAbility(Ability ability)
    {
        if (displayedMember.equippedAbilities.Any(t => t == ability)) {
            print("Remove ability");
            displayedMember.equippedAbilities.Remove(ability);
        } else if (displayedMember.equippedAbilities.Count >= 5) {
            print("Too many abilities");
            // Do nothing
        } else {
            print("Add ability");
            displayedMember.equippedAbilities.Add(ability);
        }
        UpdateEquippedAbilities(displayedMember);
    }

    public void DisplayAllyInfo(AllyInfo ally)
    {
        displayedMember = ally;
        
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

        if (!statDisplay.activeSelf) {
            statDisplay.SetActive(true);
        }
        if (!resistDisplay.activeSelf) {
            resistDisplay.SetActive(true);
        }
        
        statDisplay.transform.Find("Health").GetComponent<TextMeshProUGUI>().text = "<u>Health</u>: " + ally.baseHealth;
        statDisplay.transform.Find("Defense").GetComponent<TextMeshProUGUI>().text = "<u>Defense</u>: " + ally.baseDefense;
        statDisplay.transform.Find("Armor").GetComponent<TextMeshProUGUI>().text = "<u>Armor</u>: " + ally.baseArmor;
        statDisplay.transform.Find("Spirit").GetComponent<TextMeshProUGUI>().text = "<u>Spirit</u>: " + ally.baseSpirit;
        statDisplay.transform.Find("Power").GetComponent<TextMeshProUGUI>().text = "<u>Power</u>: " + ally.basePower;
        statDisplay.transform.Find("Skill").GetComponent<TextMeshProUGUI>().text = "<u>Skill</u>: " + ally.baseSkill;
        statDisplay.transform.Find("Wit").GetComponent<TextMeshProUGUI>().text = "<u>Wit</u>: " + ally.baseWit;
        statDisplay.transform.Find("Mind").GetComponent<TextMeshProUGUI>().text = "<u>Mind</u>: " + ally.baseMind;
        statDisplay.transform.Find("Speed").GetComponent<TextMeshProUGUI>().text = "<u>Speed</u>: " + ally.baseSpeed;
        statDisplay.transform.Find("Luck").GetComponent<TextMeshProUGUI>().text = "<u>Luck</u>: " + ally.baseLuck;

        resistDisplay.transform.Find("Stun Resist").GetComponent<TextMeshProUGUI>().text = "<u>Stun</u>:\n" + ally.baseStunResist + "%";
        resistDisplay.transform.Find("Debuff Resist").GetComponent<TextMeshProUGUI>().text = "<u>Debuff</u>:\n" + ally.baseDebuffResist + "%";
        resistDisplay.transform.Find("Ailment Resist").GetComponent<TextMeshProUGUI>().text = "<u>Ailment</u>:\n" + ally.baseAilmentResist + "%";

        foreach (Transform child in abilitySelection.transform) {
            Destroy(child.gameObject);
        }
        foreach (Ability ability in ally.abilities) {
            GameObject tempAbility = Instantiate(abilityPrefab, abilitySelection.transform);
            AbilitySelectButton tempButton = tempAbility.GetComponent<AbilitySelectButton>();
            tempButton.SetMyAbility(ability);
            
            //tempAbility.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = ability.abilityIcon;
            tempAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
            
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

            Image abilityBorder = tempAbility.transform.GetChild(1).gameObject.GetComponent<Image>();
            abilityBorder.sprite = tempSprite;
            abilityBorder.color = levelOneColor;
            
            tempAbility.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = ability.abilityName;
        }
        
        Rect rect = abilitySelectionRect.rect;
        float width = rect.width;
        int rowCount = Mathf.CeilToInt(ally.abilities.Length / ABILITY_ROW_MAX); 
        float height = (abilityYSize * 2 + ABILITY_Y_SPACE) + ((abilityYSize + ABILITY_Y_SPACE) * (rowCount - 2)) + 5;
        
        abilitySelectionRect.sizeDelta = new Vector2(width, height);

        UpdateEquippedAbilities(ally);
    }

    private void UpdateEquippedAbilities(AllyInfo ally)
    {
        if (!activeEquippedDisplay.activeSelf) {
            activeEquippedDisplay.SetActive(true);
        }
        
        for (int i = 0; i < activeEquippedSlots.Length; i++) {
            GameObject abilitySlot = activeEquippedSlots[i];

            if (i < ally.equippedAbilities.Count) {
                Ability ability = ally.equippedAbilities[i];

                AbilitySelectButton abilitySlotButton = abilitySlot.GetComponent<AbilitySelectButton>();
                abilitySlotButton.SetMyAbility(ability);
            
                //abilitySlot.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = ability.abilityIcon;
                abilitySlot.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
            
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
                Image abilityBorder = abilitySlot.transform.GetChild(1).gameObject.GetComponent<Image>();
                abilityBorder.sprite = tempSprite;
                abilityBorder.color = levelOneColor;
                
                abilitySlot.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = ability.abilityName;
            } else {
                Image abilityIcon = activeEquippedSlots[i].transform.GetChild(0).GetComponent<Image>();
                abilityIcon.sprite = null;
                abilityIcon.color = Color.black;
                
                Image abilityBorder =  activeEquippedSlots[i].transform.GetChild(1).GetComponent<Image>();
                abilityBorder.sprite = lightBorder;
                abilityBorder.color = Color.gray3;
                
                abilitySlot.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }
    
    // OnClick Methods
    
    public void ConfirmParty()
    {
        SceneManager.LoadScene(BASE_SCENE);
        foreach (AllyInfo ally in preppedMembers) {
            partyManager.AddMemberToPartyByName(ally.allyName, 1); // TODO add in party positioning
        }
    }
}
