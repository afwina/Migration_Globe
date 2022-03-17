using System.Collections;
using UnityEngine;


public class CountryFocusState : VisState
{
    public string SecondaryCountry = null;
    public string HeldCountry = null;
    public override void Enter(VisManager vis) 
    {
        base.Enter(vis);
        vis.BackButton.Show();
        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, vis.CurrentCountry);
        vis.FocusCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, vis.CurrentMode);
        HeldCountry = null;
    }

    public override void Exit()
    {
        SecondaryCountry = null;
        HeldCountry = null;
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        if (vis.CurrentCountry.Equals(country))
        {
            country = null;
        }

        SecondaryCountry = country;
        if (country != null)
        {
            vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, country, vis.CurrentYear, vis.CurrentMode);
        }
        else
        {
            vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, vis.CurrentMode);
        }
        return this;
    }

    public override VisState HandleGlobeHover(VisManager vis, string country)
    {
        return this;
    }

    public override VisState HandleYearChange(VisManager vis, string year)
    {
        vis.VisualizeCountryMigration(vis.CurrentMode, year, vis.CurrentCountry);
        if (SecondaryCountry != null)
        {
            vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, SecondaryCountry, year, vis.CurrentMode);
        }
        else
        {
            vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, year, vis.CurrentMode);
        }

        return this;
    }

    public override VisState HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.VisualizeCountryMigration(mode, vis.CurrentYear, vis.CurrentCountry);
        if (SecondaryCountry != null)
        {
            vis.InfoPanel.DisplayCountry2CountryFocus(vis.CurrentCountry, SecondaryCountry, vis.CurrentYear, mode);
        }
        else
        {
            vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, mode);
        }
        return this;
    }

    public override VisState HandleGlobeHold(VisManager vis, string country, bool staticHold)
    {
        if (string.IsNullOrEmpty(HeldCountry) && staticHold && country != null)
        {
            HeldCountry = country;
            vis.StartChargeUpAnimation(country);
        }
        else if (!string.IsNullOrEmpty(HeldCountry))
        {
            if (HeldCountry != country || !staticHold)
            {
                vis.StopChargeUpAnimation(HeldCountry);
                HeldCountry = null;
            }
        }
        return this;
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold, float duration)
    {
        if (country == null || !staticHold || duration < vis.Config.FocusCountryHoldMin || HeldCountry != country)
        {
            vis.StopChargeUpAnimation(HeldCountry);
            HeldCountry = null;
            return this;
        }

        vis.CurrentCountry = country;
        SecondaryCountry = null;

        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, country);
        vis.FocusCountry(country);

        vis.InfoPanel.DisplayCountryFocus(country, vis.CurrentYear, vis.CurrentMode);
        vis.PulseCountry(country);
        return this;
    }

    public override VisState HandleBack(VisManager vis)
    {
        if (SecondaryCountry == null)
        {
            vis.CurrentCountry = null;
            return VisStates.WorldFocusState;
        }

        SecondaryCountry = null;
        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, vis.CurrentCountry);
        vis.FocusCountry(vis.CurrentCountry);

        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, vis.CurrentMode);
        vis.PulseCountry(vis.CurrentCountry);
        return this;
    }
}
