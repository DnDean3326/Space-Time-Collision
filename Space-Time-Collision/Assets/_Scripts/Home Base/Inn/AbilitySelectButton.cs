using UnityEngine;
using UnityEngine.EventSystems;

public class AbilitySelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InnFunctionality innFunctionality;
    private Ability myAbility;
    
    private void Awake()
    {
        innFunctionality = FindFirstObjectByType<InnFunctionality>();
    }
    
    public void SetMyAbility(Ability ability)
    {
        myAbility = ability;
    }
    
    // OnClick Methods

    public void CallAddAbility()
    {
        innFunctionality.AddAbility(myAbility);
    }

    // OnHover Enter Methods

    public void OnPointerEnter(PointerEventData eventData)
    {
        innFunctionality.DisplayAbilityEffect(myAbility);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        innFunctionality.HideAbilityEffect();
    }
}
