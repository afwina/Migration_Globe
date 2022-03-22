using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VisState
{
    public abstract VisState HandleGlobeHover(VisManager vis, string country);
    public abstract VisState HandleGlobeClick(VisManager vis, string country);
    public abstract void HandleYearChange(VisManager vis, string year);
    public abstract void HandleFlowChange(VisManager vis, FlowMode mode);
    public abstract VisState HandleGlobeHold(VisManager vis, string country, bool staticHold);
    public abstract VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold, float duration);
    public abstract VisState HandleBack(VisManager vis);
    public virtual void Enter(VisManager vis) {}
    public virtual void Exit(VisManager vis) { }
}

public class VisStates
{
    public static WorldFocusState WorldFocusState = new WorldFocusState();
    public static WorldFocusCountrySelectedState WorldFocusCountrySelectState = new WorldFocusCountrySelectedState();
    public static CountryFocusState CountryFocusState = new CountryFocusState();
    public static Country2CountryFocusState Country2CountryFocusState = new Country2CountryFocusState();
}