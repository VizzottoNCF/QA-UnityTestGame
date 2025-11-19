using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CustomButtonBase : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Pointer Click");
        
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Pointer Exit");
        
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Pointer Enter");
    }
}
