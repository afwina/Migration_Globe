using System.Collections;
using UnityEngine;


public class CountryFocusState : AbstractCountryFocusState
{
    public override void Enter(VisManager vis) 
    {
        base.Enter(vis);

        vis.BackButton.Show();
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, vis.CurrentMode);
        vis.ScaleToggle.Disable();

        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, vis.CurrentCountry);
        vis.FocusCountry(vis.CurrentCountry);
        vis.PulseCountry(vis.CurrentCountry);

        HeldCountry = null;
    }

    public override void Exit(VisManager vis)
    {
        HeldCountry = null;
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        vis.SecondaryCountry = country;
        if (!string.IsNullOrEmpty(country) && !vis.CurrentCountry.Equals(country))
        {
            return VisStates.Country2CountryFocusState;
        }

        return this;
    }

    public override void HandleYearChange(VisManager vis, string year)
    {
        base.HandleYearChange(vis, year);
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, year, vis.CurrentMode);
    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        base.HandleFlowChange(vis, mode);
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, mode);
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold, float duration)
    {
        if (string.IsNullOrEmpty(country) || !staticHold || duration < vis.Config.FocusCountryHoldMin || HeldCountry != country)
        {
            if (HeldCountry != vis.CurrentCountry)
            {
                vis.StopCountryAnimation(HeldCountry);
            }
            HeldCountry = null;
            return this;
        }

        vis.StopCountryAnimation(vis.CurrentCountry);
        vis.CurrentCountry = country;

        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, country);
        vis.FocusCountry(country);
        vis.PulseCountry(country);

        vis.InfoPanel.DisplayCountryFocus(country, vis.CurrentYear, vis.CurrentMode);
        return this;
    }

    public override VisState HandleBack(VisManager vis)
    {
        vis.StopCountryAnimation(vis.CurrentCountry);
        vis.CurrentCountry = null;
        return VisStates.WorldFocusState;
    }
}
