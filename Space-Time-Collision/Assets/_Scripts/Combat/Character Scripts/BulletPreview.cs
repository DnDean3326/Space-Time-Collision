using UnityEngine;
using UnityEngine.UI;

public class BulletPreview : MonoBehaviour
{
    private BulletDisplay myBulletDisplay;
    private Image myImage;

    private void Awake()
    {
        print("Bullet Display start called");
        myImage = gameObject.GetComponent<Image>();
    }

    public void SetMyBulletDisplay(BulletDisplay bulletDisplay)
    {
        myBulletDisplay = bulletDisplay;
        myImage.color = bulletDisplay.bulletColor;
    }
    
    public void DisplayBulletInfo()
    {
        string displayString = myBulletDisplay.hoverText;
        Tooltip.ShowTooltip_Static("", displayString);
    }
    
    public void HideDisplayBulletInfo()
    {
        Tooltip.HideTooltip_Static();
    }
}
