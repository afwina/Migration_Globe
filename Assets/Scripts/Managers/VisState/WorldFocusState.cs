using System.Collections;
using UnityEngine;


public class WorldFocusState : VisState
{
    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        vis.CurrentCountry = country;
        if (country == null)
        {
            vis.InfoPanel.DisplayTotalTitle(vis.CurrentMode, vis.CurrentYear);
        }
        else
        {
            vis.HighlightCountry(country);
            vis.InfoPanel.DisplayCountryTotal(country, vis.CurrentYear);
        }

        return this;
    }

    public override VisState HandleGlobeHover(VisManager vis, string country)
    {
        return this;
    }

    public override VisState HandleYearChange(VisManager vis, string year)
    {
        vis.VisualizeTotal(vis.CurrentMode, year);
        if (vis.CurrentCountry != null)
        {
            vis.InfoPanel.DisplayCountryTotal(vis.CurrentCountry, year);
        }
        else
        {
            vis.InfoPanel.DisplayTotalTitle(vis.CurrentMode, year);
        }

        return this;
    }

    public override VisState HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.VisualizeTotal(mode, vis.CurrentYear);
        return this;
    }

    public override VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold)
    {
        if (staticHold && country != null)
        {
            vis.CurrentCountry = country;
            return VisStates.CountryFocusState;
        }

        return this;
    }
}
