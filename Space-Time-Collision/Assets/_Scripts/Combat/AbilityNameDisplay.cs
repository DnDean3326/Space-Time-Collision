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
            string userName;
            string targetName;
            if (user.myName == "Tre") {
                userName = "Ricochet";
            } else {
                userName = user.myName;
            }
            if (target.myName == "Tre") {
                targetName = "Ricochet";
            } else {
                targetName = user.myName;
            }
            abilityText.text = userName + " used " + ability.abilityName + " against " + targetName;
        } else {
            string userName;
            if (user.myName == "Tre") {
                userName = "Ricochet";
            } else {
                userName = user.myName;
            }
            abilityText.text = userName + " used " + ability.abilityName;
        }
    }
    
    public void HideAbilityInfo()
    {
        abilityText.text = "";
    }
}
