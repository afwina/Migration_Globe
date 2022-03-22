using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstarctWorldFocusState : VisState
{
    protected string HeldCountry = null;

    public override void Enter(VisManager vis)
    {
        base.Enter(vis);

        HeldCountry = null;
    }

    public override void Exit(VisManager vis)
    {
        base.Exit(vis);
        HeldCountry = null;
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
                vis.StopCountryAnimation(HeldCountry);
                HeldCountry = null;
            }
        }

        return this;
    }

    public override VisState HandleGlobeHover(VisManager vis, string country)
    {
        return this;
    }

    public override void HandleYearChange(VisManager vis, string year)
    {
        vis.VisualizeTotal(vis.CurrentMode, year);
    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.VisualizeTotal(mode, vis.CurrentYear);
    }
}
