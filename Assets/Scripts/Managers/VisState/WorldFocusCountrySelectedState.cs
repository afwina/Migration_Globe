using System.Collections;
using UnityEngine;


public class WorldFocusCountrySelectedState : AbstarctWorldFocusState
{
    public override void Enter(VisManager vis)
    {
        base.Enter(vis);

        vis.BackButton.Show();
        vis.HighlightCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryTotal(vis.CurrentCountry, vis.CurrentYear, vis.CurrentMode);
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        if (!string.IsNullOrEmpty(country) && country != vis.CurrentCountry)
        {
            vis.StopCountryAnimation(vis.CurrentCountry);
            vis.HighlightCountry(country);
            vis.InfoPanel.DisplayCountryTotal(country, vis.CurrentYear, vis.CurrentMode);
            vis.CurrentCountry = country;
        }

        return this;
    }

    public override void HandleYearChange(VisManager vis, string year)
    {
        vis.StopCountryAnimation(vis.CurrentCountry);
        base.HandleYearChange(vis, year);
        vis.HighlightCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryTotal(vis.CurrentCountry, year, vis.CurrentMode);
    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.StopCountryAnimation(vis.CurrentCountry);
        base.HandleFlowChange(vis, mode);
        vis.HighlightCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryTotal(vis.CurrentCountry, vis.CurrentYear, mode);
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold, float duration)
    {
        if (staticHold && !string.IsNullOrEmpty(country) && country == HeldCountry && duration > vis.Config.FocusCountryHoldMin)
        {
            vis.StopCountryAnimation(vis.CurrentCountry);
            vis.CurrentCountry = country;
            return VisStates.CountryFocusState;
        }


        vis.StopCountryAnimation(country);
        if (country == vis.CurrentCountry)
        {
            vis.HighlightCountry(vis.CurrentCountry);
        }
        HeldCountry = null;
        return this;
    }

    public override VisState HandleBack(VisManager vis)
    {
        vis.StopCountryAnimation(vis.CurrentCountry);
        vis.CurrentCountry = null;
        return VisStates.WorldFocusState;
    }

    public override void HandleScaleModeChanged(VisManager vis, ScaleMode mode)
    {
        vis.StopCountryAnimation(vis.CurrentCountry);
        base.HandleScaleModeChanged(vis, mode);
        vis.HighlightCountry(vis.CurrentCountry);
    }
}
