using TMPro;
using UnityEngine;

public class AbilityNameDisplay : MonoBehaviour
{
    [SerializeField] private GameObject abilityDisplay;
    private TextMeshProUGUI abilityText;

    private void Awake()
    {
        abilityText =  abilityDisplay.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void DisplayAbilityInfo(BattleEntity user, BattleEntity target, Ability ability)
    {
        if (user != target) {
            abilityText.text = user.myName + " used " + ability.abilityName + " against " + target.myName;
        } else {
            abilityText.text = user.myName + " used " + ability.abilityName;
        }
    }
    
    public void HideAbilityInfo()
    {
        abilityText.text = "";
    }
}
