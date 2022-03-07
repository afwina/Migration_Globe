using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderTick : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI Tick;
    
    public void SetTick(string tickText)
    {
        Tick.text = tickText;
    }
}
