using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TokenGlossary : MonoBehaviour
{
    [SerializeField] private GameObject glossaryPrefab;
    [SerializeField] private GameObject buffGlossary;
    [SerializeField] private GameObject debuffGlossary;
    [SerializeField] private GameObject ailmentGlossary;
    
    private const float ROW_MAX = 10f;
    private const float GLOSSARY_SPACE = 10f;
    private static float glossarySize;
    
    private BattleSystem battleSystem;
    private List<BattleToken> allTokens;
    private RectTransform buffRect;
    private RectTransform debuffRect;
    private RectTransform ailmentRect;

    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }

    private void Start()
    {
        allTokens = battleSystem.GetAllTokens();
        
        buffRect  = buffGlossary.GetComponent<RectTransform>();
        debuffRect = debuffGlossary.GetComponent<RectTransform>();
        ailmentRect = ailmentGlossary.GetComponent<RectTransform>();
        
        RectTransform glossaryRect = glossaryPrefab.GetComponent<RectTransform>();
        glossarySize = glossaryRect.rect.height;

        SetBuffTokens();
        SetDebuffTokens();
        SetAilmentTokens();
    }

    private void SetBuffTokens()
    {
        int buffsAdded = 0;
        foreach (BattleToken token in allTokens) {
            if (token.tokenType == Token.TokenType.Buff) {
                GameObject newGlossary = Instantiate(glossaryPrefab, buffGlossary.transform);
                newGlossary.GetComponent<Image>().sprite = token.tokenIcon;
                buffsAdded++;
                
                TokenGlossaryButton tempButton = newGlossary.GetComponent<TokenGlossaryButton>();
                tempButton.SetMyToken(token);
            }
        }

        Rect rect = buffRect.rect;
        float width = rect.width;
        float height;
        
        if (buffsAdded <= ROW_MAX) {
            height = glossarySize;
        } else if (buffsAdded <= ROW_MAX * 2) {
            height = glossarySize * 2 + GLOSSARY_SPACE;
        } else {
            int rowCount = Mathf.CeilToInt(buffsAdded / ROW_MAX);
            height = (glossarySize * 2 + GLOSSARY_SPACE) + ((glossarySize + GLOSSARY_SPACE) * (rowCount - 2));
        }
        
        buffRect.sizeDelta = new Vector2(width, height);
    }
    
    private void SetDebuffTokens()
    {
        int debuffsAdded = 0;
        foreach (BattleToken token in allTokens) {
            if (token.tokenType == Token.TokenType.Debuff) {
                GameObject newGlossary = Instantiate(glossaryPrefab, debuffGlossary.transform);
                newGlossary.GetComponent<Image>().sprite = token.tokenIcon;
                debuffsAdded++;
                
                TokenGlossaryButton tempButton = newGlossary.GetComponent<TokenGlossaryButton>();
                tempButton.SetMyToken(token);
            }
        }

        Rect rect = debuffRect.rect;
        float width = rect.width;
        float height;
        
        if (debuffsAdded <= ROW_MAX) {
            height = glossarySize;
        } else if (debuffsAdded <= ROW_MAX * 2) {
            height = glossarySize * 2 + GLOSSARY_SPACE;
        } else {
            int rowCount = Mathf.CeilToInt(debuffsAdded / ROW_MAX);
            height = (glossarySize * 2 + GLOSSARY_SPACE) + ((glossarySize + GLOSSARY_SPACE) * (rowCount - 2));
        }
        
        debuffRect.sizeDelta = new Vector2(width, height);
    }
    
    private void SetAilmentTokens()
    {
        int ailmentsAdded = 0;
        foreach (BattleToken token in allTokens) {
            if (token.tokenType == Token.TokenType.Ailments) {
                GameObject newGlossary = Instantiate(glossaryPrefab, ailmentGlossary.transform);
                newGlossary.GetComponent<Image>().sprite = token.tokenIcon;
                ailmentsAdded++;
                
                TokenGlossaryButton tempButton = newGlossary.GetComponent<TokenGlossaryButton>();
                tempButton.SetMyToken(token);
            }
        }

        Rect rect = buffRect.rect;
        float width = rect.width;
        float height;
        
        if (ailmentsAdded <= ROW_MAX) {
            height = glossarySize;
        } else if (ailmentsAdded <= ROW_MAX * 2) {
            height = glossarySize * 2 + GLOSSARY_SPACE;
        } else {
            int rowCount = Mathf.CeilToInt(ailmentsAdded / ROW_MAX);
            height = (glossarySize * 2 + GLOSSARY_SPACE) + ((glossarySize + GLOSSARY_SPACE) * (rowCount - 2));
        }
        
        ailmentRect.sizeDelta = new Vector2(width, height);
    }
    
    // OnHover Enter Methods

    public void DisplayTokenEffect(BattleToken token)
    {
        string tokenName = token.tokenName;
        string tokenDescription = token.tokenDescription;
        
        Tooltip.ShowTooltip_Static(tokenName + ":" ," " + tokenDescription);
    }
    
    // OnHover Exit Methods

    public void RemoveTokenEffect(BattleToken token)
    {
        Tooltip.HideTooltip_Static();
    }
}
