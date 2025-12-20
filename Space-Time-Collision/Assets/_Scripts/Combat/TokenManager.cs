using UnityEngine;
using System.Collections.Generic;

public class TokenManager : MonoBehaviour
{
    [SerializeField] private Token[] allTokens;
    [SerializeField] private List<Token> allTokensList = new List<Token>();
    
    public List<Token> GetAllTokens()
    {
        allTokensList.AddRange(allTokens);
        return allTokensList;
    }
}
