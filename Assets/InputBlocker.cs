using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InputBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        InputManager.Disable();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InputManager.Enable();
    }
}
