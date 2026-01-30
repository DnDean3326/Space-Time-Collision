using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TalismanDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image myImage;
    private Talisman myTalisman;

    private void Awake()
    {
        myImage = GetComponent<Image>();
    }

    public void SetTalisman(Talisman talisman)
    {
        myTalisman = talisman;
        myImage.sprite = talisman.GetIcon(); }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string itemDescription = myTalisman.GetItemDescription();
        
        Tooltip.ShowTooltip_Static("",itemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.HideTooltip_Static();
    }
}


