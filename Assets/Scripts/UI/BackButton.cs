using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BackButton : MonoBehaviour
{
    public Action OnClick;

    [SerializeField]
    private Animator m_Animator;

    private static readonly int Showing = Animator.StringToHash("Showing");

    public void Show()
    {
        m_Animator.SetBool(Showing, true);
    }

    public void Hide()
    {
        m_Animator.SetBool(Showing, false);
    }

    public void UI_OnClick()
    {
        OnClick?.Invoke();
    }
}
