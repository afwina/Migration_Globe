using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCountryFocusState : VisState
{
    protected string HeldCountry = null;

    public override void HandleYearChange(VisManager vis, string year)
    {
        vis.VisualizeCountryMigration(vis.CurrentMode, year, vis.CurrentCountry);
    }

    public override void HandleFlowChange(VisManager vis, FlowMode mode)
    {
        vis.VisualizeCountryMigration(mode, vis.CurrentYear, vis.CurrentCountry);
        vis.PulseCountry(vis.CurrentCountry);
    }

    public override VisState HandleGlobeHold(VisManager vis, string country, bool staticHold)
    {
        if (string.IsNullOrEmpty(HeldCountry) && staticHold && !string.IsNullOrEmpty(country) && !country.Equals(vis.CurrentCountry))
        {
            HeldCountry = country;
            vis.StartChargeUpAnimation(country);
        }
        else if (!string.IsNullOrEmpty(HeldCountry))
        {
            if (HeldCountry != country || !staticHold)
            {
                if (HeldCountry != vis.CurrentCountry)
                vis.StopCountryAnimation(HeldCountry);
                HeldCountry = null;
            }
        }
        return this;
    }

    public override void HandleScaleModeChanged(VisManager vis, ScaleMode mode)
    {
        return;
    }
}
