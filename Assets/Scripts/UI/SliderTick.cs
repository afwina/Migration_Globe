using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderTick : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI Tick;

    [SerializeField]
    private Animator Animator;

    private static readonly int Selected = Animator.StringToHash("Selected");

    public void SetTick(string tickText)
    {
        Tick.text = tickText;
    }

    public void Select()
    {
        Animator.SetBool(Selected, true);
    }

    public void Deselect()
    {
        Animator.SetBool(Selected, false);
    }
}
