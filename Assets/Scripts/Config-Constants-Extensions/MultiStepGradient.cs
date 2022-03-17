using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/MultiStepGradient")]
public class MultiStepGradient : MonoBehaviour
{
    [SerializeField]
    private UIGradient BaseGradient;

    private List<UIGradient> GradientSteps;
    private float Height;

    private void Awake()
    {
        Height = BaseGradient.GetComponent<RectTransform>().rect.height;
    }

    public void SetGradient(Gradient gradient)
    {
        int stepsNeeded = gradient.colorKeys.Length - 1;
        if (GradientSteps == null)
        {
            GradientSteps = new List<UIGradient>();
            GradientSteps.Add(BaseGradient);
            while (GradientSteps.Count < stepsNeeded)
            {
                UIGradient g = Instantiate(BaseGradient, transform);
                GradientSteps.Add(g);
            }
        }
        else if(GradientSteps.Count < stepsNeeded)
        {
            while (GradientSteps.Count < stepsNeeded)
            {
                UIGradient g = Instantiate(BaseGradient, transform);
                GradientSteps.Add(g);
            }
        }
        else if (GradientSteps.Count > stepsNeeded)
        {
            while (GradientSteps.Count > stepsNeeded)
            {
                GradientSteps.RemoveAt(0);
            }
        }

        for (int i = 0; i < stepsNeeded; i++)
        {
            float start = gradient.colorKeys[i].time;
            float end = gradient.colorKeys[i + 1].time;
            var rect = GradientSteps[i].GetComponent<RectTransform>();
            rect.localScale = new Vector3(1, end - start, 1);
            rect.anchoredPosition = new Vector3(0, (end -start)* Height/2 + start *Height, 0);
            GradientSteps[i].m_color2 = gradient.colorKeys[i].color;
            GradientSteps[i].m_color1 = gradient.colorKeys[i + 1].color;
        }
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}
