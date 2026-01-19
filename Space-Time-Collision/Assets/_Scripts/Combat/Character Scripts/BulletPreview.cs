using UnityEngine;
using UnityEngine.UI;

public class BulletPreview : MonoBehaviour
{
    [SerializeField] private BulletDisplay myBulletDisplay;
    private Image myImage;
    
    private void Awake()
    {
        myImage = GetComponent<Image>();
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
