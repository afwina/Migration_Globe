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
    public void Initialize(Color noDataColor)
    {
        NoDataSwatch.color = noDataColor;
    }

    public void SetScale(float min, float max, Gradient gradient)
    {
        MinText.text = NumberFormatter.Format(min);
        MaxText.text = NumberFormatter.Format(max);
    }
}
