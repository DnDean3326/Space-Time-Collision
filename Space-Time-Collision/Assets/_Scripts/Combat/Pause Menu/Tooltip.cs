using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private static Tooltip instance;
    
    [SerializeField] private RectTransform canvasRectTransform;
    
    private TextMeshProUGUI tooltipText;
    private RectTransform backgroundRectTransform;
    private RectTransform rectTransform;

    private void Awake()
    {
        instance = this;
        
        canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        rectTransform = transform.GetComponent<RectTransform>();
        tooltipText = transform.Find("Tooltip Text").GetComponent<TextMeshProUGUI>();
        backgroundRectTransform = transform.Find("Tooltip BG").GetComponent<RectTransform>();
    }

    private void Start()
    {
        HideTooltip();
    }

    private void Update()
    {
        Vector2 anchoredPosition = Mouse.current.position.value / canvasRectTransform.localScale.x;

        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width) {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height) {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }
        
        rectTransform.anchoredPosition = anchoredPosition;
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
