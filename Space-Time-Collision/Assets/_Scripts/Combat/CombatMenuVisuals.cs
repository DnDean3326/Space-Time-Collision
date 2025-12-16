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
        this.maxSpirit = maxSpirit;
        this.currentSpirit = currentSpirit;
        
        UpdateSpiritBar();
    }
    
    public void UpdateSpiritBar()
    {
        spiritBar.maxValue = maxSpirit;
        spiritBar.value = currentSpirit;
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

    public GameObject[] GetAbilityButtons()
    {
        return abilityButtons;
    }
    
    public GameObject[] GetTargetButtons()
    {
        return targetButtons;
    }

    public void ChooseAbilityButton(int selectedAbility)
    {
        battleSystem.SetCurrentAbilityType(selectedAbility);
    }
    
    public void ChooseTargetButton(int currentTarget)
    {
        battleSystem.SelectTarget(currentTarget);
    }
}
