using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoField : MonoBehaviour
{
    [TextArea(3,10)]
    public string Format;

    [SerializeField]
    private TextMeshProUGUI text;

    [SerializeField]
    private Animator Animator;

    private static readonly int Update = Animator.StringToHash("Update");

    public void Display (params string[] input)
    {
        Display(true, input);
    }

    public void Display(bool animate, params string[] input)
    {
        gameObject.SetActive(true);
        if (animate)
        {
            Animator.SetTrigger(Update);
        }
        text.text = string.Format(Format, input);
    }

    public void Display(float delay, params string[] input)
    {
        gameObject.SetActive(true);
        StartCoroutine(WaitAndDisplay(delay, input));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator WaitAndDisplay(float delay, string[] input)
    {
        yield return new WaitForSeconds(delay);
        Display(input);
    }
}
