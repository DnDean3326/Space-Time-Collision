using UnityEngine;
using UnityEngine.EventSystems;

public class AbilitySelectButton : MonoBehaviour, IPointerEnterHandler
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
        print("I was clicked");
        innFunctionality.AddAbility(myAbility);
    }

    // OnHover Enter Methods

    public void OnPointerEnter(PointerEventData eventData)
    {
        print("OnPointerEnter");
    }
}
