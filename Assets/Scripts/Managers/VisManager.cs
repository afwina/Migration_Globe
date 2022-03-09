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
    [SerializeField]
    private GlobeManager Globe;
    [SerializeField]
    private CameraManager Camera;
    [SerializeField]
    private InfoPanel m_InfoPanel;
    public InfoPanel InfoPanel => m_InfoPanel;

    [HideInInspector]
    public FlowMode CurrentMode;
    [HideInInspector]
    public string CurrentYear;
    [HideInInspector]
    public string CurrentCountry = null;

    private VisState VisState;

    public void Initialize()
    {
        ScaleLegend.Initialize(Config.NoDataColor);

        YearController.Setup(DataManager.GetYears(), DataManager.GetYears().IndexOf(Config.DefaultYear));
        YearController.OnSliderChange += SetYear;
        FlowModeController.Setup(new List<FlowMode> {FlowMode.Immigration, FlowMode.Emigration}, (int)Config.DefaultFlowMode, Config);
        FlowModeController.OnSliderChange += SetFlow;

        Globe.Initialize();
        Globe.ColorGlobe(Config.NoDataColor);
        Globe.OnCountryHoveredChanged += HandleGlobeHover;
        Globe.OnGlobeClicked += HandleGlobeClick;
        Globe.OnReset += Camera.ResetZoom;
        Globe.OnHoldReleased += HandleGlobeHoldReleased;

        Camera.OnZoomChanged += Globe.AdjustRotateSensitivity;

        SetState(VisStates.WorldFocusState);
        VisualizeTotal(Config.DefaultFlowMode, Config.DefaultYear);
    }

    private void OnDestroy()
    {
        YearController.OnSliderChange -= SetYear;
        FlowModeController.OnSliderChange -= SetFlow;
        Globe.OnCountryHoveredChanged -= HandleGlobeHover;
        Globe.OnGlobeClicked -= HandleGlobeClick;
        Globe.OnReset -= Camera.ResetZoom;
        Globe.OnHoldReleased -= HandleGlobeHoldReleased;
        Camera.OnZoomChanged -= Globe.AdjustRotateSensitivity;
    }

    private void SetState(VisState newState)
    {
        if (newState != VisState)
        {
            VisState?.Exit();
            VisState = newState;
            VisState.Enter(this);
        }
    }

    public void VisualizeTotal(FlowMode mode, string year)
    {
        uint[] startingData = mode == FlowMode.Emigration ? DataManager.GetTotalEmigrants(year) : DataManager.GetTotalImmigrants(year);
        List<string> countries = mode == FlowMode.Emigration ? DataManager.Origins : DataManager.Destinations;
        Gradient colorGradient = mode == FlowMode.Immigration ? Config.ImmigrationGradient : Config.EmigrationGradient;
        float duration = mode != CurrentMode ? Config.FlowModeSwitchDuration : Config.YearSwitchDuration;
        float max = Mathf.Round(DataManager.GetMaxTotal(mode) / 100000f) *100000;
        UpdateVis(startingData, max, countries, colorGradient, duration);

        CurrentMode = mode;
        CurrentYear = year;
    }

    public void VisualizeCountryMigration(FlowMode mode, string year, string country)
    {
        uint[] data = mode == FlowMode.Emigration ? DataManager.GetEmigrantsFrom(country, year) : DataManager.GetImmigrationTo(country, year);
        List<string> countries = mode == FlowMode.Emigration ? DataManager.Destinations : DataManager.Origins;
        Gradient colorGradient = mode == FlowMode.Immigration ? Config.ImmigrationGradient : Config.EmigrationGradient;
        float duration = mode != CurrentMode ? Config.FlowModeSwitchDuration : Config.YearSwitchDuration;
        float max = Mathf.Round(data.Max() / 100f) * 100;
        
        UpdateVis(data, max, countries, colorGradient, duration);

        CurrentMode = mode;
        CurrentYear = year;
        CurrentCountry = country;
    }

    public void SetYear(string year)
    {
        VisState.HandleYearChange(this, year);
    }

    public void SetFlow(FlowMode mode)
    {
        VisState.HandleFlowChange(this, mode);
    }

    private void UpdateVis(uint[] data, float max, List<string> countries, Gradient colorGradient, float duration = 1)
    {
        ScaleLegend.SetScale(0, max, colorGradient);
        Globe.UpdateGlobe(data, max, countries, colorGradient, duration);
    }

    private void HandleGlobeHover(string country)
    {
        VisState.HandleGlobeHover(this, country);
    }

    private void HandleGlobeClick(string country)
    {
        VisState.HandleGlobeClick(this, country);
    }

    private void HandleGlobeHoldReleased(string country, bool staticHold)
    {
        SetState(VisState.HandleGlobeHoldReleased(this, country, staticHold));
    }

    public void HighlightCountry(string country)
    {
        Globe.HighlightCountry(country);
    }

    public void FocusCountry(string country)
    {
        Globe.RotateToCountry(country);

    }
}
