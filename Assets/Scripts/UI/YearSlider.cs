using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class YearSlider : SliderController<string>
{
    [SerializeField]
    private Image Handle;
    [SerializeField]
    private GameObject Background;
    [SerializeField]
    private SliderTick TickPrefab;

    private SliderTick[] Ticks;
    protected override void SetupSlider(object custom = null)
    {
        Ticks = new SliderTick[Options.Count];
        for (int i = 0; i < Options.Count; i++)
        {
            Slider.value = i;
            SliderTick tick = Instantiate(TickPrefab, Background.transform);
            tick.SetTick(Options[i]);
            tick.transform.position = new Vector3(Handle.transform.position.x, tick.transform.position.y, Handle.transform.position.z);
            Ticks[i] = tick;
        }
    }

    protected override void UpdateSlider(int index)
    {
        Ticks[PreviousIndex].Deselect();
        Ticks[index].Select();
    }
}
