using System.Collections;
using UnityEngine;


public class WorldFocusCountrySelectedState : AbstarctWorldFocusState
{
    public override void Enter(VisManager vis)
    {
        base.Enter(vis);

        vis.BackButton.Show();
        vis.HighlightCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryTotal(vis.CurrentCountry, vis.CurrentYear);
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        if (!string.IsNullOrEmpty(country) && country != vis.CurrentCountry)
        {
            vis.StopCountryAnimation(vis.CurrentCountry);
            vis.HighlightCountry(country);
            vis.InfoPanel.DisplayCountryTotal(country, vis.CurrentYear);
            vis.CurrentCountry = country;
        }

        return this;
    }

    public override void HandleYearChange(VisManager vis, string year)
    {
        base.HandleYearChange(vis, year);
        vis.HighlightCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryTotal(vis.CurrentCountry, year);
    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        base.HandleFlowChange(vis, mode);
        vis.HighlightCountry(vis.CurrentCountry);
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
        HeldCountry = null;
        return this;
    }

    public override VisState HandleBack(VisManager vis)
    {
        vis.StopCountryAnimation(vis.CurrentCountry);
        vis.CurrentCountry = null;
        return VisStates.WorldFocusState;
    }
}
