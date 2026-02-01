using UnityEngine;
using UnityEngine.EventSystems;

public class AllySelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InnFunctionality innFunctionality;
    private AllyInfo myAlly;
    
    private void Awake()
    {
        innFunctionality = FindFirstObjectByType<InnFunctionality>();
    }
    
    public void SetMyAlly(AllyInfo ally)
    {
        myAlly = ally;
    }
    
    // OnClick Methods

    public void CallChangePartyMember()
    {
        innFunctionality.ChangePartyMember(myAlly);
    }

    // OnPointerEnter Methods

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myAlly == null) { return; }
        innFunctionality.DisplayAllyDescription(myAlly);
    }
    
    // OnPointerExit Methods

    public void OnPointerExit(PointerEventData eventData)
    {
        innFunctionality.HideAllyDescription();
    }
    
}
