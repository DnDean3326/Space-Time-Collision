using UnityEngine;
using UnityEngine.EventSystems;

public class TokenGlossaryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TokenGlossary tokenGlossary;
    private Token myToken;
    
    private void Awake()
    {
        tokenGlossary = FindFirstObjectByType<TokenGlossary>();
    }

    public void SetMyToken(Token token)
    {
        myToken = token;
    }
    
    // OnHover Enter Methods

    public void OnPointerEnter(PointerEventData eventData)
    {
        tokenGlossary.DisplayTokenEffect(myToken);
    }
    
    // OnHover Exit Methods

    public void OnPointerExit(PointerEventData eventData)
    {
        tokenGlossary.RemoveTokenEffect();
    }
}
