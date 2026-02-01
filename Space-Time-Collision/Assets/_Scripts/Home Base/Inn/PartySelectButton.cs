using UnityEngine;
using UnityEngine.EventSystems;

public class PartySelectButton : MonoBehaviour
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

    public void DisplayPartyMember()
    {
        innFunctionality.DisplayAllyInfo(myAlly);
    }
}
