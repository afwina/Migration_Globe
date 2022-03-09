using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VisState
{
    public abstract VisState HandleGlobeHover(VisManager vis, string country);
    public abstract VisState HandleGlobeClick(VisManager vis, string country);
    public abstract VisState HandleYearChange(VisManager vis, string year);
    public abstract VisState HandleFlowChange(VisManager vis, FlowMode mode);
    public abstract VisState HandleGlobeHoldReleased(VisManager vis, string country, bool staticHold);
    public virtual void Enter(VisManager vis) {}
    public virtual void Exit() {}
}

public class VisStates
{
    public static WorldFocusState WorldFocusState = new WorldFocusState();
    public static CountryFocusState CountryFocusState = new CountryFocusState();
}