using UnityEngine;
using UnityEngine.EventSystems;

public class InnAnimationScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int IsHovered = Animator.StringToHash("IsHovered");
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool(IsHovered, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool(IsHovered, false);
    }
}
