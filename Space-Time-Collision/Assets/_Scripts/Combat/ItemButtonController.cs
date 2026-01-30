using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButtonController : MonoBehaviour
{
    [SerializeField] private List<Button> consumableButtons;
    [SerializeField] private List<Image> consumableBackgrounds;
    [SerializeField] private List<GameObject> consumableIconObjects;
    [SerializeField] private List<Image> consumableIcons;
    private List<Consumable> consumableInventory;
    private List<Talisman> talismanInventory;
    private RunInfo runInfo;
    private BattleSystem battleSystem;
    private int consumableSelected;

    private readonly Color32 damageBGColor = new Color32(177, 2, 37, 255);
    private readonly Color32 healBGColor = new Color32(83, 183, 122, 255);
    private readonly Color32 buffBGColor = new Color32(28, 113, 162, 255);
    private readonly Color32 debuffBGColor = new Color32(193, 93, 37, 255);
    private readonly Color32 emptyBGColor = new Color32(40, 40, 40, 200);

    private void Awake()
    {
        runInfo = FindFirstObjectByType<RunInfo>();
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }

    private void Start()
    {
        consumableInventory = runInfo.GetConsumables();
        talismanInventory = runInfo.GetTalismans();
        UpdateItemButtons();
    }

    private void UpdateItemButtons()
    {
        for (int i = 0; i < consumableInventory.Count; i++) {
            consumableButtons[i].interactable = true;
            consumableIconObjects[i].SetActive(true);
            consumableIcons[i].sprite = consumableInventory[i].GetIcon();
            switch (consumableInventory[i].GetLinkedAbility().abilityType) {
                case Ability.AbilityType.Damage:
                    consumableBackgrounds[i].color = damageBGColor;
                    break;
                case Ability.AbilityType.Heal:
                    consumableBackgrounds[i].color = healBGColor;
                    break;
                case Ability.AbilityType.Buff:
                    consumableBackgrounds[i].color = buffBGColor;
                    break;
                case Ability.AbilityType.Debuff:
                    consumableBackgrounds[i].color = debuffBGColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        for (int i = consumableInventory.Count; i < consumableButtons.Count; i++) {
            consumableButtons[i].interactable = false;
            consumableIconObjects[i].SetActive(false);
            consumableBackgrounds[i].color = emptyBGColor;
        }
    }

    public void DisplayConsumable(int index)
    {
        if (consumableInventory[index] == null) {
            return;
        }
        consumableSelected = index;
        battleSystem.SetConsumableAbility(consumableInventory[index].GetLinkedAbility());
    }

    public void UseConsumable()
    {
        runInfo.RemoveConsumable(consumableInventory[consumableSelected]);
        UpdateItemButtons();
    }

    public void DisplayConsumableEffect(int index)
    {
        if (consumableInventory.Count >= index) {
            return;
        }
        
        string itemDescription = consumableInventory[index].GetItemDescription();
        
        Tooltip.ShowTooltip_Static("",itemDescription);
    }

    public void DisplayTalismanEffect(int index)
    {
        string itemDescription = talismanInventory[index].GetItemDescription();
        
        Tooltip.ShowTooltip_Static("",itemDescription);
    }

    public void EndDisplay()
    {
        Tooltip.HideTooltip_Static();
    }
}
