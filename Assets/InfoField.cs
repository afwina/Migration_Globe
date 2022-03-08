using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoField : MonoBehaviour
{
    [TextArea]
    public string Format;

    private TextMeshProUGUI text;
    public void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Display(params string[] input)
    {
        gameObject.SetActive(true);
        text.text = string.Format(Format, input);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
