using System.Collections;
using UnityEngine;

public class FlowModeSlider : SliderController<FlowMode>
{
    [SerializeField]
    private UIGradient Gradient;

    [SerializeField]
    private Animator ImmIconAnimator;
    [SerializeField]
    private Animator EmIconAnimator;

    private static readonly int Select = Animator.StringToHash("Select");

    protected override void SetupSlider(object custom)
    {
        VisConfig config = custom as VisConfig;
        Gradient.m_color1 = config.EmigrationColor;
        Gradient.m_color2 = config.ImmigrationColor;
    }

    protected override void UpdateSlider(int index)
    {
        if (Options[index] == FlowMode.Immigration)
        {
            ImmIconAnimator.SetTrigger(Select);
        }
        else
        {
            EmIconAnimator.SetTrigger(Select);
        }
    }
}
