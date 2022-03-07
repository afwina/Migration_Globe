using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class SliderController<T> : MonoBehaviour
{
    public Action<T> OnSliderChange;

    [SerializeField]
    protected Slider Slider;

    protected List<T> Options;
    protected int CurrentIndex = 0;

    public void Setup(List<T> options, int defaultIndex = 0, object custom = null)
    {
        Options = options;

        Slider.minValue = 0;
        Slider.maxValue = Options.Count-1;
        Slider.wholeNumbers = true;

        CurrentIndex = defaultIndex >= 0 ? defaultIndex : 0;
        Slider.value = CurrentIndex;

        SetupUI(custom);
    }

    public void UI_OnSliderChanged(float index)
    {
        SliderChanged((int)index);
    }

    protected virtual void SliderChanged(int index)
    {
        CurrentIndex = index;
        OnSliderChange?.Invoke(Options[CurrentIndex]);
    }

    protected abstract void SetupUI(object custom = null);
}
