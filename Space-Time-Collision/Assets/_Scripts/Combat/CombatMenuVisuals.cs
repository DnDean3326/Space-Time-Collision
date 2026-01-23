using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class CombatMenuVisuals : MonoBehaviour
{
    [Header("Resource Displays")]
    [SerializeField] private Slider spiritBar;
    [SerializeField] private TextMeshProUGUI spText;
    [SerializeField] private List<GameObject> bulletDisplays;
    
    [Header("UI Menus")]
    [SerializeField] private GameObject abilitySelectUI;
    [SerializeField] private GameObject resourcePreviewUI;
    [SerializeField] private GameObject abilityPreviewUI;
    
    [Header("UI Buttons")]
    [SerializeField] private GameObject[] abilityButtons;
    [SerializeField] private GameObject passButton;
    [SerializeField] private GameObject backButton;
    
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI abilityEffectText;
    [SerializeField] private TextMeshProUGUI hitChanceText;
    [SerializeField] private TextMeshProUGUI dmgRangeText;
    [SerializeField] private TextMeshProUGUI critChanceText;
    [SerializeField] private TextMeshProUGUI stunResText;
    [SerializeField] private TextMeshProUGUI debuffResText;
    [SerializeField] private TextMeshProUGUI ailmentResText;

    [Header("Ability Borders")]
    [SerializeField] private List<Image> abilityBorders = new List<Image>();
    [SerializeField] private Sprite lightBorder;
    [SerializeField] private Sprite mediumBorder;
    [SerializeField] private Sprite heavyBorder;

    private readonly Color32 levelOneColor = new Color32(206,137,70, 255);
    private readonly Color32 levelTwoColor = new Color32(196,196,196, 255);
    private readonly Color32 levelThreeColor = new Color32(239,191,4, 255);
    private readonly Color32 levelFourColor = new Color32(155,178,203, 255);

    private BattleEntity me;
    private BattleSystem battleSystem;
    private List<Image> abilityImages = new List<Image>();
    private List<Button> myAbilityButtons = new List<Button>();
    private List<EventTrigger> myAbilityTriggers = new List<EventTrigger>();
    private List<TextMeshProUGUI> abilityTexts = new List<TextMeshProUGUI>();
    private List<bool> abilityActive = new List<bool>();

    private RicochetBattleLogic ricochetLogic;
    private bool isTargeting = false;
    private int abilitySelected = 10;
    
    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
        
        foreach (GameObject abilityButton in abilityButtons) {
            Image tempImage = abilityButton.GetComponent<Image>();
            abilityImages.Add(tempImage);
            Button tempButton = abilityButton.GetComponent<Button>();
            myAbilityButtons.Add(tempButton);
            EventTrigger tempTrigger = abilityButton.GetComponent<EventTrigger>();
            myAbilityTriggers.Add(tempTrigger);
            TextMeshProUGUI tempText = abilityButton.GetComponentInChildren<TextMeshProUGUI>();
            abilityTexts.Add(tempText);
        }
    }

    private void Start()
    {
        abilitySelectUI.SetActive(false);
        abilityPreviewUI.SetActive(false);
    }

    public void SetMyAbilityBar()
    {
        for (int i = 0; i < abilityButtons.Length; i++) {
            Sprite tempSprite;
            switch (me.myAbilities[i].abilityWeight) {
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
            abilityBorders[i].sprite = tempSprite;

            Color32 tempColor;
            switch (me.myAbilities[i].abilityLevel) {
                case 1:
                    tempColor = levelOneColor;
                    break;
                case 2:
                    tempColor = levelTwoColor;
                    break;
                case 3:
                    tempColor = levelThreeColor;
                    break;
                case 4:
                    tempColor = levelFourColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            abilityBorders[i].color = tempColor;
            
            //abilityImages[i].sprite = me.myAbilities[i].abilityIcon; // TODO: Once ability icons are in, uncomment this line
            abilityTexts[i].text = me.myAbilities[i].abilityName;
        }
    }

    public void UpdateAbilityStatus(List<bool> abilityStatuses)
    {
        abilityActive = abilityStatuses;
    }

    public List<Image> GetAbilityImages()
    {
        return abilityImages;
    }
    
    public List<Button> GetAbilityButtons()
    {
        return myAbilityButtons;
    }

    public List<EventTrigger> GetAbilityTriggers()
    {
        return myAbilityTriggers;
    }
    
    public void SetMenuStartingValues(BattleEntity entity)
    {
        me = entity;
        if (me.myName == "Tre") {
            ricochetLogic = FindFirstObjectByType<RicochetBattleLogic>();
            ricochetLogic.SetupBulletDisplays(bulletDisplays);
        }

        abilityEffectText.text = "";
        
        UpdateSpiritBar();
    }

    public void UpdateSpiritBar()
    {
        if (me.myName == "Tre") {
            spText.text = ricochetLogic.CheckBulletCount() + " / " + me.maxSpirit;
            return;
        }
        
        spiritBar.maxValue = me.maxSpirit;
        spiritBar.value = me.currentSpirit;
        
        spText.text = "SP: " + me.currentSpirit + " / " + me.maxSpirit;
    }
    
    public void PreviewSpirit(int reduction)
    {
        float preview = me.currentSpirit - reduction;
        
        if (me.myName == "Tre") {
            spText.text = preview + " / " + me.maxSpirit;
            return;
        }
        
        spiritBar.maxValue = me.maxSpirit;
        spiritBar.value = preview;
        
        spText.text = "SP: " + preview + " / " + me.maxSpirit;
    }

    public void ChangeAbilitySelectUIVisibility(bool visible)
    {
        isTargeting = false;
        abilitySelected = 10;
        abilitySelectUI.SetActive(visible);
    }

    public void ChangeResourcePreviewUIVisibility(bool visible)
    {
        resourcePreviewUI.SetActive(visible);
    }

    public void ChangeAbilityPreviewUIVisibility(bool visible)
    {
        abilityPreviewUI.SetActive(visible);
    }
    
    public void ChangeAbilityEffectTextVisibility(bool visible)
    {
        abilityEffectText.gameObject.SetActive(visible);
    }
    
    public void ChangePassButtonVisibility(bool visible)
    {
        passButton.SetActive(visible);
    }

    public void ChangeBackButtonVisibility(bool visible)
    {
        backButton.SetActive(visible);
    }

    public void SetAbilityValues(float hitChance, int dmgMin, int dmgMax, int critChance, bool isDamage,
        bool singleValue)
    {
        string type;
        if (isDamage) {
            type = "DMG";
        } else {
            type = "Heal";
        }

        hitChanceText.text = hitChance + "%" + '\n' + "Hit";
        critChanceText.text = critChance + "%" + '\n' + "Crit";
        if (!singleValue) {
            if (dmgMin < 0) {
                dmgMin = 0;
            }

            if (dmgMax < 0) {
                dmgMax = 0;
            }

            dmgRangeText.text = dmgMin + "-" + dmgMax + '\n' + type;
        } else {
            if (dmgMax < 0) {
                dmgMax = 0;
            }

            dmgRangeText.text = dmgMax + "\n " + type;
        }
    }

    public void SetTargetResists(BattleEntity targetEntity)
    {
        stunResText.text = targetEntity.stunResist + "%";
        debuffResText.text = targetEntity.debuffResist + "%";
        ailmentResText.text = targetEntity.ailmentResist + "%";
    }

    // Button OnClick methods
    
    public void ChooseAbilityButton(int selectedAbility)
    {
        if (isTargeting) {
            battleSystem.BackToAbilities();
            abilitySelected = 10;
        }
        isTargeting = true;
        abilitySelected = selectedAbility;
        battleSystem.SetCurrentAbilityType(selectedAbility);
    }

    public void PassButton()
    {
        isTargeting = false;
        battleSystem.PassTurn();
    }
    
    public void BackButton()
    {
        isTargeting = false;
        battleSystem.BackToAbilities();
    }
    
    // Button OnHover methods

    public void AbilityEffect(int selectedAbility)
    {
        abilityEffectText.text = battleSystem.SetAbilityDescription(selectedAbility);
    }
    
    // Button OnExit methods
    
    public void AbilityEffectRemove(int selectedAbility)
    {
        if (!isTargeting) {
            abilityEffectText.text = "";
        } else {
            abilityEffectText.text = battleSystem.SetAbilityDescription(abilitySelected);
        }
    }
}
