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
    protected int PreviousIndex = 0;
    protected bool SetupDone = false;
    public void Setup(List<T> options, int defaultIndex = 0, object custom = null)
    {
        Options = options;

        Slider.minValue = 0;
        Slider.maxValue = Options.Count-1;
        Slider.wholeNumbers = true;

        CurrentIndex = defaultIndex >= 0 ? defaultIndex : 0;

        SetupSlider(custom);
        SetupDone = true;

        Slider.value = CurrentIndex;
        UpdateSlider(CurrentIndex);
    }

    public void UI_OnSliderChanged(float index)
    {
        PreviousIndex = CurrentIndex;
        CurrentIndex = (int)index;
        if (SetupDone)
        {
            UpdateSlider(CurrentIndex);
        }
        OnSliderChange?.Invoke(Options[CurrentIndex]);
    }

    protected abstract void UpdateSlider(int index);

    protected abstract void SetupSlider(object custom = null);
}
