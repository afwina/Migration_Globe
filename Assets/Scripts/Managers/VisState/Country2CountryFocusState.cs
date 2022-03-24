using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Country2CountryFocusState : AbstractCountryFocusState
{
    public override void Enter(VisManager vis)
    {
        base.Enter(vis);
        vis.HighlightCountry(vis.SecondaryCountry);
        vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, vis.SecondaryCountry, vis.CurrentYear, vis.CurrentMode);
    }

    public override void Exit(VisManager vis)
    {
        base.Exit(vis);
        vis.StopCountryAnimation(vis.SecondaryCountry);
        vis.SecondaryCountry = null;
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        if(!string.IsNullOrEmpty(country) && !country.Equals(vis.CurrentCountry) && !country.Equals(vis.SecondaryCountry))
        {
            vis.StopCountryAnimation(vis.SecondaryCountry);
            vis.HighlightCountry(country);
            vis.SecondaryCountry = country;
            vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, country, vis.CurrentYear, vis.CurrentMode, false);
        }

        return this;
    }

    public override void HandleYearChange(VisManager vis, string year)
    {
        vis.StopCountryAnimation(vis.SecondaryCountry);
        base.HandleYearChange(vis, year);
        vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, vis.SecondaryCountry, year, vis.CurrentMode);
        vis.HighlightCountry(vis.SecondaryCountry);

    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.StopCountryAnimation(vis.SecondaryCountry);
        base.HandleFlowChange(vis, mode);
        vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, vis.SecondaryCountry, vis.CurrentYear, mode);
        vis.HighlightCountry(vis.SecondaryCountry);
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold, float duration)
    {
        if (string.IsNullOrEmpty(country) || !staticHold || duration < vis.Config.FocusCountryHoldMin || HeldCountry != country)
        {
            vis.StopCountryAnimation(HeldCountry);
            if (HeldCountry == vis.SecondaryCountry)
            {
                vis.HighlightCountry(vis.SecondaryCountry);
            }
            else if (HeldCountry == vis.CurrentCountry)
            {
                vis.PulseCountry(vis.CurrentCountry);
            }
            HeldCountry = null;
            return this;
        }

        vis.StopCountryAnimation(vis.CurrentCountry);
        vis.CurrentCountry = country;
        return VisStates.CountryFocusState;
    }

    public override VisState HandleBack(VisManager vis)
    {
        return VisStates.CountryFocusState;
    }
}
