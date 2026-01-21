using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TokenDisplay : MonoBehaviour
{
    [FormerlySerializedAs("tokenShadow")] [SerializeField] private GameObject tokenDisplay;
    private GameObject tokenCountShadow;
    private TextMeshProUGUI tokenCountText;
    private Image tokenShadow;
    private Image tokenSlot;

    private BattleToken myToken;

    private void Awake()
    {
        tokenDisplay = gameObject;
        tokenCountShadow = tokenDisplay.transform.GetChild(0).gameObject;
        tokenCountText = tokenDisplay.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        tokenShadow = tokenDisplay.transform.GetChild(2).GetComponent<Image>();
        tokenSlot = tokenDisplay.transform.GetChild(3).GetComponent<Image>();
        
        tokenDisplay.SetActive(false);
        tokenCountShadow.SetActive(false);
        tokenCountText.enabled = false;
        tokenShadow.enabled = false;
        tokenSlot.enabled = false;
    }

    public void SetMyToken(BattleToken token)
    {
        myToken = token;
    }

    public void DisplayToken()
    {
        if (myToken.tokenCount > 0) {
            tokenDisplay.SetActive(true);
            tokenSlot.sprite = myToken.tokenIcon;
            tokenShadow.enabled = true;
            tokenSlot.enabled = true;
        } else {
            tokenDisplay.SetActive(false);
            tokenCountShadow.SetActive(false);
            tokenCountText.enabled = false;
            tokenShadow.enabled = false;
            tokenSlot.enabled = false;
        }
        
        if (myToken.tokenCount > 1) {
            tokenCountShadow.SetActive(true);
            tokenCountText.text = myToken.tokenCount.ToString();
            tokenCountText.enabled = true;
        } else {
            tokenCountShadow.SetActive(false);
            tokenCountText.enabled = false;
        }
    }
    
    // OnPointerEnter Methods
    
    public void DisplayTokenInfo()
    {
        string tokenName = myToken.displayName;
        string tokenDescription = myToken.tokenDescription;
        
        Tooltip.ShowTooltip_Static(tokenName + ":" ," " + tokenDescription);
    }
    
    // OnPointerExit Methods
    
    public void HideTokenInfo()
    {
        Tooltip.HideTooltip_Static();
    }
    
}
