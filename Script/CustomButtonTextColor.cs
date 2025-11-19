using TMPro;
using UnityEngine;

public class CustomButtonTextColor : CustomButtonBase
{
    public TMP_Text buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (buttonText != null)
        {
            buttonText.color = hoverColor;
        }
    }

    public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
    }
}
