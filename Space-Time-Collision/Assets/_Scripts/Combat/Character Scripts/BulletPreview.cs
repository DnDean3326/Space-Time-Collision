using UnityEngine;
using UnityEngine.UI;

public class BulletPreview : MonoBehaviour
{
    private RicochetBattleLogic ricochetLogic;
    private BulletDisplay myBulletDisplay;
    private Image myImage;
    
    private void Awake()
    {
        ricochetLogic = FindFirstObjectByType<RicochetBattleLogic>();
        myImage = GetComponent<Image>();
    }

    public void SetMyBulletDisplay(BulletDisplay bulletDisplay)
    {
        myBulletDisplay = bulletDisplay;
        myImage.color = bulletDisplay.bulletColor;
    }
    
    public void CallDisplayBulletInfo()
    {
        string displayString = myBulletDisplay.hoverText;
        Tooltip.ShowTooltip_Static("", displayString);
    }
    
    public void CallHideDisplayBulletInfo()
    {
        Tooltip.HideTooltip_Static();
    }
}
