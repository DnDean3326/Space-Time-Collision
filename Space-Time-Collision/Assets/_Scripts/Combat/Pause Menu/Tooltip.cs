using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private static Tooltip instance;
    
    [SerializeField] private Camera uiCamera;
    
    private TextMeshProUGUI tooltipText;
    private RectTransform backgroundRectTransform;
    private RectTransform parentRectTransform;

    private void Awake()
    {
        instance = this;
        
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
        tooltipText = transform.Find("Tooltip Text").GetComponent<TextMeshProUGUI>();
        backgroundRectTransform = transform.Find("Tooltip BG").GetComponent<RectTransform>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, Mouse.current.position.ReadValue(), null, out localPoint);
        transform.localPosition = localPoint;
    }
    
    private void ShowTooltip(string boldTooltipString, string tooltipString)
    {
        gameObject.SetActive(true);
        
        tooltipText.text = "<b><u>" + boldTooltipString + "</b></u>" + tooltipString;
        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + textPaddingSize * 2,
            tooltipText.preferredHeight + textPaddingSize * 2);
        backgroundRectTransform.sizeDelta = backgroundSize;
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip_Static(string boldTooltipString, string tooltipString)
    {
        instance.ShowTooltip(boldTooltipString, tooltipString);
    }
    
    public static void HideTooltip_Static()
    {
        instance.HideTooltip();
    }
}
