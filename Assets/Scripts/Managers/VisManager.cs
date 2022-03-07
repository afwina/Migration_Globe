using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPM;
using System.Linq;
public class VisManager : MonoBehaviour
{
    public const string World = "World";

    [SerializeField]
    private VisConfig Config;
    [SerializeField]
    private ScaleLegend ScaleLegend;
    [SerializeField]
    private YearSlider YearController;
    [SerializeField]
    private FlowModeSlider FlowModeController;

    private WorldMapGlobe Globe;
    private FlowMode CurrentMode;
    private string CurrentYear;
    private string Focus;

    public void Initialize()
    {
        ScaleLegend.Initialize(Config.NoDataColor);

        YearController.Setup(DataManager.GetYears(), DataManager.GetYears().IndexOf(Config.DefaultYear));
        YearController.OnSliderChange += SetYear;
        FlowModeController.Setup(new List<FlowMode> {FlowMode.Immigration, FlowMode.Emigration}, (int)Config.DefaultFlowMode, Config);
        FlowModeController.OnSliderChange += SetFlow;

        Globe = WorldMapGlobe.instance;
        Globe.ColorAllRegionsInstant(Config.NoDataColor);


        VisualizeTotal(Config.DefaultFlowMode, Config.DefaultYear);
    }

    private void OnDestroy()
    {
        YearController.OnSliderChange -= SetYear;
        FlowModeController.OnSliderChange -= SetFlow;
    }

    public void VisualizeTotal(FlowMode mode, string year)
    {
        uint[] startingData = mode == FlowMode.Emigration ? DataManager.GetTotalEmigrants(year) : startingData = DataManager.GetTotalImmigrants(year);
        List<string> countries = mode == FlowMode.Emigration ? DataManager.Origins : DataManager.Destinations;
        Gradient colorGradient = mode == FlowMode.Immigration ? Config.ImmigrationGradient : Config.EmigrationGradient;
        float duration = mode != CurrentMode ? Config.FlowModeSwitchDuration : Config.YearSwitchDuration;
        float max = Mathf.Round(DataManager.GetMaxTotal(mode) / 100000f) *100000;
        UpdateGlobe(startingData, max, countries, colorGradient, duration);

        CurrentMode = mode;
        CurrentYear = year;
        Focus = World;
    }

    public void UpdateGlobe(uint[] data, float max, List<string> countries, Gradient colorGradient, float duration = 1)
    {
        Globe.StopFading();
        for (int i = 0; i < data.Length; i++)
        {
            float time = data[i] / max;
            var color = colorGradient.Evaluate(time);
            Globe.FadeCountryIntoColor(countries[i], color, duration);
        }

        ScaleLegend.SetScale(0, max, colorGradient);
    }

    public void SetYear(string year)
    {
        if (Focus == World)
        {
            VisualizeTotal(CurrentMode, year);
        }
    }

    public void SetFlow(FlowMode mode)
    {
        if (Focus == World)
        {
            VisualizeTotal(mode, CurrentYear);
        }
    }
}
