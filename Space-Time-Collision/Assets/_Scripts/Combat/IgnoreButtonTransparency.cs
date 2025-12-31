using UnityEngine;
using UnityEngine.UI;

public class IgnoreButtonTransparency : MonoBehaviour
{
    private Image buttonImage;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.alphaHitTestMinimumThreshold = 0.5f;
    }
}
