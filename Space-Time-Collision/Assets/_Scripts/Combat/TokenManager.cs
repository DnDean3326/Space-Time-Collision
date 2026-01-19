using UnityEngine;
using System.Collections.Generic;

public class TokenManager : MonoBehaviour
{
    [SerializeField] private Token[] allTokens;
    [SerializeField] private List<Token> allTokensList = new List<Token>();
    
    private static GameObject _instance;
    
    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
    }
    
    public List<Token> GetTokenInfo()
    {
        if (allTokensList.Count == 0) {
            allTokensList.AddRange(allTokens);
        }
        return allTokensList;
    }
}
