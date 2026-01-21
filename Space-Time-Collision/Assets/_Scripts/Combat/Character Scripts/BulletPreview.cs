using UnityEngine;
using UnityEngine.UI;

public class BulletPreview : MonoBehaviour
{
    private BulletDisplay myBulletDisplay;
    private Image myImage;

    private void Start()
    {
        myImage = gameObject.GetComponent<Image>();
    }

    public void SetMyBulletDisplay(BulletDisplay bulletDisplay)
    {
        myBulletDisplay = bulletDisplay;
        if (myImage == null) {
            myImage = gameObject.GetComponent<Image>();;
        }
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
