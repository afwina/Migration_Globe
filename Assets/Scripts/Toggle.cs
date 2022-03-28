using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class Toggle : MonoBehaviour
{
    [SerializeField]
    private UnityEvent Option1Selected;
    [SerializeField]
    private UnityEvent Option2Selected;

    [SerializeField]
    private Animator Animator;
    [SerializeField]
    private Slider Slider;

    private static readonly int Showing = Animator.StringToHash("Showing");

    public void UI_ToggleChanged(float choice)
    {
        if (choice == 0)
        {
            Option1Selected?.Invoke();
        }
        else
        {
            Option2Selected?.Invoke();
        }
    }

    public void Disable()
    {
        Animator?.SetBool(Showing, false);
        Slider.interactable = false;
    }

    public void Enable()
    {
        Animator?.SetBool(Showing, true);
        Slider.interactable = true;
    }
}
