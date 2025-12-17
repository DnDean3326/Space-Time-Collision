using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatMenuVisuals : MonoBehaviour
{
    [Header("Resource Displays")]
    [SerializeField] private Slider spiritBar;
    [SerializeField] private TextMeshProUGUI spText;
    
    [Header("UI Menus")]
    [SerializeField] private GameObject abilitySelectUI;
    [SerializeField] private GameObject targetSelectUI;
    
    [Header("UI Buttons")]
    [SerializeField] private GameObject[] abilityButtons;
    [SerializeField] private GameObject[] targetButtons;
    [SerializeField] private GameObject backButton;
    
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI abilityEffectText;
    [SerializeField] private TextMeshProUGUI hitChanceText;
    [SerializeField] private TextMeshProUGUI dmgRangeText;
    [SerializeField] private TextMeshProUGUI critChanceText;

    private BattleSystem battleSystem;
    
    private int maxSpirit;
    private int currentSpirit;
    
    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }
    
    public void SetMenuStartingValues(int maxSpirit, int currentSpirit)
    {
        abilityEffectText.text = "";
        this.maxSpirit = maxSpirit;
        this.currentSpirit = currentSpirit;
        
        UpdateSpiritBar();
    }
    
    private void UpdateSpiritBar()
    {
        spiritBar.maxValue = maxSpirit;
        spiritBar.value = currentSpirit;
        
        spText.text = "SP: " + currentSpirit + " / " + maxSpirit;
    }
    
    public void ChangeSpirit(int newSpirit)
    {
        currentSpirit = newSpirit;
        UpdateSpiritBar();
    }

    public void ChangeAbilitySelectUIVisibility(bool visible)
    {
        abilitySelectUI.SetActive(visible);
    }

    public void ChangeTargetSelectUIVisibility(bool visible)
    {
        targetSelectUI.SetActive(visible);
    }
    
    public void ChangeAbilityEffectTextVisibility(bool visible)
    {
        abilityEffectText.gameObject.SetActive(visible);
    }

    public void ChangeBackButtonVisibility(bool visible)
    {
        backButton.SetActive(visible);
    }

    public void SetAbilityValues(float hitChance, int dmgMin, int dmgMax, int critChance, bool isDamage)
    {
        string type;
        if (isDamage) {
            type = "DMG";
        } else {
            type = "Heal";
        }
        hitChanceText.text = hitChance + "%" + '\n' + "Hit"; 
        dmgRangeText.text = dmgMin + "-" + dmgMax + '\n'+ type;
        critChanceText.text = critChance + "%" + '\n' + "Crit";
    }

    public GameObject[] GetAbilityButtons()
    {
        return abilityButtons;
    }
    
    public GameObject[] GetTargetButtons()
    {
        return targetButtons;
    }

    // Button OnClick methods
    
    public void ChooseAbilityButton(int selectedAbility)
    {
        battleSystem.SetCurrentAbilityType(selectedAbility);
    }
    
    public void ChooseTargetButton(int currentTarget)
    {
        battleSystem.SelectTarget(currentTarget);
    }

    public void BackButton()
    {
        battleSystem.BackToAbilities();
    }
    
    // Button OnHover methods

    public void AbilityEffect(int selectedAbility)
    {
        abilityEffectText.text = battleSystem.SetAbilityDescription(selectedAbility);
        battleSystem.PreviewResourceValue(selectedAbility);
    }

    public void AbilityEffectRemove(int selectedAbility)
    {
        abilityEffectText.text = "";
        battleSystem.EndResourcePreview(selectedAbility);
    }

    public void TargetIndicate(int hoveredTarget)
    {
        battleSystem.IndicateTarget(hoveredTarget);
    }
    
    public void TargetIndicateRemove(int hoveredTarget)
    {
        battleSystem.StopIndicatingTarget(hoveredTarget);
    }
    
    // TODO create OnHover methods for targeted enemy damage ranges
}
