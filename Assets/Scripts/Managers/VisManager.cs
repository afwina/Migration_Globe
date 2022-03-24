using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPM;
using System.Linq;
public class VisManager : MonoBehaviour
{
    public const string World = "World";

    [SerializeField]
    public VisConfig Config;
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
    [SerializeField]
    private BackButton m_BackButton;
    public BackButton BackButton => m_BackButton;

    [HideInInspector]
    public FlowMode CurrentMode;
    [HideInInspector]
    public string CurrentYear;
    [HideInInspector]
    public string CurrentCountry = null;
    [HideInInspector]
    public string SecondaryCountry = null;

    private VisState VisState;

    public void Initialize()
    {
        ScaleLegend.Initialize(Config.NoDataColor);

        YearController.Setup(DataManager.GetYears(), DataManager.GetYears().IndexOf(Config.DefaultYear));
        YearController.OnSliderChange += SetYear;
        FlowModeController.Setup(new List<FlowMode> {FlowMode.Immigration, FlowMode.Emigration}, (int)Config.DefaultFlowMode, Config);
        FlowModeController.OnSliderChange += SetFlow;

        Globe.Initialize(Config.ZeroColor);
        Globe.ColorGlobe(Config.NoDataColor);
        Globe.OnCountryHoveredChanged += HandleGlobeHover;
        Globe.OnGlobeClicked += HandleGlobeClick;
        Globe.OnReset += Camera.ResetZoom;
        Globe.OnHoldReleased += HandleGlobeHoldReleased;
        Globe.OnHold += HandleGlobeHold;

        Camera.OnZoomChanged += Globe.AdjustRotateSensitivity;

        BackButton.OnClick += HandleBack;

        CurrentMode = Config.DefaultFlowMode;
        CurrentYear = Config.DefaultYear;
        SetState(VisStates.WorldFocusState);
    }

    private void OnDestroy()
    {
        YearController.OnSliderChange -= SetYear;
        FlowModeController.OnSliderChange -= SetFlow;
        Globe.OnCountryHoveredChanged -= HandleGlobeHover;
        Globe.OnGlobeClicked -= HandleGlobeClick;
        Globe.OnReset -= Camera.ResetZoom;
        Globe.OnHoldReleased -= HandleGlobeHoldReleased;
        Globe.OnHold -= HandleGlobeHold;
        Camera.OnZoomChanged -= Globe.AdjustRotateSensitivity;
        BackButton.OnClick -= HandleBack;
    }

    private void SetState(VisState newState)
    {
        if (newState != VisState)
        {
            VisState?.Exit(this);
            VisState = newState;
            VisState.Enter(this);
            Debug.Log("State changed: " + VisState.GetType().ToString());

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

        if (data.Length > 0)
        {
            Gradient colorGradient = mode == FlowMode.Immigration ? Config.ImmigrationGradient : Config.EmigrationGradient;
            float duration = mode != CurrentMode ? Config.FlowModeSwitchDuration : Config.YearSwitchDuration;
            float max = Mathf.Round(data.Max() / 100f) * 100;
            UpdateVis(data, max, countries, colorGradient, duration);

        }
        else
        {
            Globe.WPMGlobe.StopFading();
            foreach (string c in countries)
            {
                Globe.WPMGlobe.UpdateCountry(c, Config.NoDataColor, Config.YearSwitchDuration);
            }
        }

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
        SetState(VisState.HandleGlobeClick(this, country));
    }

    private void HandleGlobeHold(string country, bool staticHold)
    {
        VisState.HandleGlobeHold(this, country, staticHold);
    }

    private void HandleGlobeHoldReleased(string country, bool staticHold, float duration)
    {
        SetState(VisState.HandleGlobeHoldReleased(this, country, staticHold, duration));
    }

    private void HandleBack()
    {
        SetState(VisState.HandleBack(this));
    }

    public void HighlightCountry(string country)
    {
        if (string.IsNullOrEmpty(country))
        {
            return;
        }

        if (CurrentMode == FlowMode.Immigration)
        {
            Globe.WPMGlobe.OutlineCountry(country, Config.HighlightDuration, Config.HighlightImmGradient);
        }
        else
        {
            Globe.WPMGlobe.OutlineCountry(country, Config.HighlightDuration, Config.HighlightEmGradient);
        }
    }

    public void StartChargeUpAnimation(string country)
    {
        if (string.IsNullOrEmpty(country))
        {
            return;
        }

        Color color = CurrentMode == FlowMode.Immigration ? Config.FocusImmColor : Config.FocusEmColor;
        Globe.WPMGlobe.GlowCountry(country, Config.FocusCountryHoldMin, color);
    }

    public void StopCountryAnimation(string country)
    {
        if (country != null)
        {
            Globe.WPMGlobe.StopCountryCoroutine(country);
        }
    }

    public void FocusCountry(string country)
    {
        Globe.WPMGlobe.FlyToCountry(country, Config.RotateToCountryDuration);
    }

    public void PulseCountry(string country)
    {
        Gradient grad = CurrentMode == FlowMode.Immigration ? Config.PulseImmGradient : Config.PulseEmGradient;
        Globe.WPMGlobe.PulseCountry(country, grad, Config.PulseCountryPeriod);
    }
}
