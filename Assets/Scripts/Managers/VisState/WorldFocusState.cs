using System.Collections;
using UnityEngine;


public class WorldFocusState : VisState
{
    private string HeldCountry = null;

    public override void Enter(VisManager vis)
    {
        base.Enter(vis);
        vis.BackButton.Hide();
        HeldCountry = null;
        vis.VisualizeTotal(vis.CurrentMode, vis.CurrentYear);
        vis.InfoPanel.DisplayTotalTitle(vis.CurrentMode, vis.CurrentYear);
    }

    public override void Exit()
    {
        base.Exit();
        HeldCountry = null;
    }

    public override VisState HandleGlobeClick(VisManager vis, string country)
    {
        if (country == null)
        {
            vis.InfoPanel.DisplayTotalTitle(vis.CurrentMode, vis.CurrentYear);
            vis.StopHighlightCountry(vis.CurrentCountry);
        }
        else
        {
            vis.StopHighlightCountry(vis.CurrentCountry);
            vis.HighlightCountry(country);
            vis.InfoPanel.DisplayCountryTotal(country, vis.CurrentYear);
        }
        vis.CurrentCountry = country;

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
        if (staticHold && country != null && country == HeldCountry && duration > vis.Config.FocusCountryHoldMin)
        {
            vis.CurrentCountry = country;
            return VisStates.CountryFocusState;
        }

        vis.StopChargeUpAnimation(country);
        HeldCountry = null;
        return this;
    }

    public override VisState HandleBack(VisManager vis)
    {
        return this;
    }
}
