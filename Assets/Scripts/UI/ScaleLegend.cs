using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ScaleLegend : MonoBehaviour
{
    [SerializeField]
    private Image NoDataSwatch;
    [SerializeField]
    private TextMeshProUGUI MinText;
    [SerializeField]
    private TextMeshProUGUI MaxText;
    [SerializeField]
    private MultiStepGradient Gradient;
    public void Initialize(Color noDataColor)
    {
        NoDataSwatch.color = noDataColor;
    }

    public void SetScale(float min, float max, Gradient gradient, ScaleMode mode)
    {
        if (mode == ScaleMode.RawValue)
        {
            MinText.text = NumberFormatter.Format(min);
            MaxText.text = NumberFormatter.Format(max);
        }
        else
        {
            MinText.text = NumberFormatter.RoundPercent(min);
            MaxText.text = NumberFormatter.RoundPercent(max);
        }

        Gradient.SetGradient(gradient);
    }
}
