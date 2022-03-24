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
    private GameObject TickPrefab;

    protected override void SetupUI(object custom = null)
    {
        for (int i = 0; i < Options.Count; i++)
        {
            Slider.value = i;
            GameObject tick = Instantiate(TickPrefab, Background.transform);
            tick.GetComponent<SliderTick>().SetTick(Options[i]);
            tick.transform.position = new Vector3(Handle.transform.position.x, tick.transform.position.y, Handle.transform.position.z);
        }

        Slider.value = CurrentIndex;
    }
}
