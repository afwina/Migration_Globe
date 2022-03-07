using System.Collections;
using UnityEngine;

public class FlowModeSlider : SliderController<FlowMode>
{
    [SerializeField]
    private UIGradient Gradient;
    protected override void SetupUI(object custom)
    {
        VisConfig config = custom as VisConfig;
        Gradient.m_color1 = config.EmigrationColor;
        Gradient.m_color2 = config.ImmigrationColor;
    }
}
