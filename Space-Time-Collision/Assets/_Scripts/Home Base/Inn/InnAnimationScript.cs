using UnityEngine;
using UnityEngine.EventSystems;

public class InnAnimationScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int IsHovered = Animator.StringToHash("IsHovered");
    private Animator animator;
    private bool isInactive = false;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    public void SetInactive(bool status)
    {
        isInactive = status;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isInactive == false) {
            animator.SetBool(IsHovered, true);
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInactive == false) {
            animator.SetBool(IsHovered, false);
        }
    }
}
