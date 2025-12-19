using UnityEngine;
using System.Collections.Generic;

public class TokenManager : MonoBehaviour
{
    [SerializeField] private TokenInfo[] allTokens;
    [SerializeField] private List<Token> allTokensList;

    private void CreateTokens()
    {
        allTokensList  = new List<Token>();
        foreach (TokenInfo token in allTokens) {
            Token newToken = new Token();

            newToken.TokenName = token.tokenName;
            newToken.TokenIcon = token.tokenIcon;
            newToken.TokenType = token.tokenType;
            newToken.TokenValue = token.tokenValue;
            newToken.TokenCap = token.tokenCap;
            newToken.TokenDescription = token.tokenDescription;

            allTokensList.Add(newToken);
        }
    }
    
    public List<Token> GetAllTokens()
    {
        CreateTokens();
        return allTokensList;
    }
}

public class Token
{
    public string TokenName;
    public Sprite TokenIcon;
    public TokenInfo.TokenType TokenType;
    public float TokenValue;
    public int TokenCap;

    public string TokenDescription;
}
