using System.Collections;
using UnityEngine;


public class CountryFocusState : VisState
{
    public string SecondaryCountry = null;
    public override void Enter(VisManager vis) 
    {
        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, vis.CurrentCountry);
        vis.FocusCountry(vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, vis.CurrentMode);
    }

    public override void Exit()
    {
        SecondaryCountry = null;
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
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
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, year, vis.CurrentMode);

        return this;
    }

    public override VisState HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.VisualizeCountryMigration(mode, vis.CurrentYear, vis.CurrentCountry);
        vis.InfoPanel.DisplayCountryFocus(vis.CurrentCountry, vis.CurrentYear, mode);
        return this;
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold)
    {
        if (country == null || !staticHold)
        {
            return this;
        }
        vis.CurrentCountry = country;
        vis.VisualizeCountryMigration(vis.CurrentMode, vis.CurrentYear, country);
        vis.FocusCountry(country);
        vis.InfoPanel.DisplayCountryFocus(country, vis.CurrentYear, vis.CurrentMode);
        return this;
    }
}
