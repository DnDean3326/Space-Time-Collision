using UnityEngine;

public class TokenGlossaryButton : MonoBehaviour
{
    private TokenGlossary tokenGlossary;
    
    private BattleToken myToken;
    
    private void Awake()
    {
        tokenGlossary = FindFirstObjectByType<TokenGlossary>();
    }

    public void SetMyToken(BattleToken token)
    {
        myToken = token;
    }
    
    // OnHover Enter Methods

    public void CallDisplayTokenEffect()
    {
        tokenGlossary.DisplayTokenEffect(myToken);
    }
    
    // OnHover Exit Methods
    
    public void CallRemoveTokenEffect()
    {
        tokenGlossary.RemoveTokenEffect(myToken);
    }
}
