using System.Collections;
using UnityEngine;

public class WorldFocusState : AbstarctWorldFocusState
{
    public override void Enter(VisManager vis)
    {
        base.Enter(vis);

        vis.BackButton.Hide();
        vis.VisualizeTotal(vis.CurrentMode, vis.CurrentYear, vis.CurrentScaleMode);
        vis.InfoPanel.DisplayTotalTitle(vis.CurrentMode, vis.CurrentYear);
        vis.ScaleToggle.Enable();
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        if (!string.IsNullOrEmpty(country))
        {
            vis.CurrentCountry = country;
            return VisStates.WorldFocusCountrySelectState;
        }

        return this;
    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        base.HandleFlowChange(vis, mode);
        vis.InfoPanel.DisplayTotalTitle(mode, vis.CurrentYear, false);
    }

    public override void HandleYearChange(VisManager vis, string year)
    {
        base.HandleYearChange(vis, year);
        vis.InfoPanel.DisplayTotalTitle(vis.CurrentMode, year);
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold, float duration)
    {
        if (staticHold && !string.IsNullOrEmpty(country) && country == HeldCountry && duration > vis.Config.FocusCountryHoldMin)
        {
            vis.CurrentCountry = country;
            return VisStates.CountryFocusState;
        }

        vis.StopCountryAnimation(country);
        HeldCountry = null;
        return this;
    }

    public override VisState HandleBack(VisManager vis)
    {
        return this;
    }
}