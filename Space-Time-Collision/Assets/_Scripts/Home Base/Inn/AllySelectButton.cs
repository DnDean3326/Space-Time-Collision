using UnityEngine;
using UnityEngine.EventSystems;

public class AllySelectButton : MonoBehaviour, IPointerEnterHandler
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

    public void CallAddPartyMember()
    {
        innFunctionality.AddPartyMember(myAlly);
    }

    // OnHover Enter Methods

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myAlly == null) { return; }
        innFunctionality.DisplayAllyInfo(myAlly);
    }
}
