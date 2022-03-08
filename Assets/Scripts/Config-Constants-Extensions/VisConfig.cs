using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "ScriptableObjects/VisConfig", order = 1)]
public class VisConfig : ScriptableObject
{
    public string DefaultYear;
    public FlowMode DefaultFlowMode;
    public Color ImmigrationColor;
    public Color EmigrationColor;
    public Color NoDataColor;
    public Gradient ImmigrationGradient;
    public Gradient EmigrationGradient;
    public float YearSwitchDuration;
    public float FlowModeSwitchDuration;
}