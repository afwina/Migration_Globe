using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WPM;

public class GlobeManager : InputHandler
{
    public static Action<float> OnReset;
    public Action<string> OnCountryHoveredChanged;
    public Action<string> OnGlobeClicked;

    [SerializeField]
    private Vector3 DefaultRotation;
    [SerializeField]
    private float ResetSpeed = 0.1f;
    [SerializeField]
    private float RotateSensitivityMax;
    [SerializeField]
    private float RotateSensitivityMin;

    private bool Dragging = false;
    private bool OverGlobe = false;
    private float RotateSensitivity;
    private WorldMapGlobe Globe;
    private int HoveredCountryIndex = -1;
    public void Initialize()
    {
        Globe = WorldMapGlobe.instance;
        transform.rotation = Quaternion.Euler(DefaultRotation);
        RotateSensitivity = RotateSensitivityMax;
        CameraManager.OnZoomChanged += AdjustRotateSensitivity;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CameraManager.OnZoomChanged -= AdjustRotateSensitivity;
    }

    public override void HandleInput(InputInfo input)
    {
        if (input.Reset_KeyDown)
        {
            ResetGlobe();
        }
        else if (Dragging && input.MouseLPressed)
        {
            Vector3 mouse = new Vector3(input.MousePosDelta.x, input.MousePosDelta.y) * RotateSensitivity;
            if(Vector3.Dot(transform.up, Vector3.up) >= 0)
            {
                transform.Rotate(transform.up, -Vector3.Dot(mouse, Camera.main.transform.right), Space.World);
            }
            else
            {
                transform.Rotate(transform.up, Vector3.Dot(mouse, Camera.main.transform.right), Space.World);
            }

            transform.Rotate(Camera.main.transform.right, Vector3.Dot(mouse, Camera.main.transform.up), Space.World);
        }

        if (Globe != null)
        {
            int prev = HoveredCountryIndex;
            HoveredCountryIndex = Globe.CheckCountryHover(input.MousePos);
            if (prev != HoveredCountryIndex)
            {
                if (HoveredCountryIndex == -1 || !OverGlobe)
                {
                    OnCountryHoveredChanged?.Invoke(null);
                }
                else
                {
                    OnCountryHoveredChanged?.Invoke(Globe.countries[HoveredCountryIndex].name);
                }
            }

            if (input.MouseLClicked)
            {
                if (HoveredCountryIndex == -1 || !OverGlobe)
                {
                    OnGlobeClicked?.Invoke(null);
                }
                else
                {
                    OnGlobeClicked?.Invoke(Globe.countries[HoveredCountryIndex].name);
                }
            }

            if (input.MouseLHold)
            {
                Debug.Log("Pressed!");
            }
        }
    }

    public void ResetGlobe()
    {
        var totalDiff = Mathf.Abs(transform.position.x - DefaultRotation.x) + Mathf.Abs(transform.position.y - DefaultRotation.y) + Mathf.Abs(transform.position.z - DefaultRotation.z);
        var timeToReset = totalDiff / ResetSpeed;

        OnReset?.Invoke(timeToReset);
        InputManager.Disable();
        StartCoroutine(ResetRotation(timeToReset, () => InputManager.Enable()));
    }

    public void ColorGlobe(Color color)
    {
        Globe.ColorAllRegionsInstant(color);
    }
    
    public void UpdateGlobe(uint[] data, float max, List<string> countries, Gradient colorGradient, float duration = 1)
    {
        Globe.StopFading();
        for (int i = 0; i < data.Length; i++)
        {
            float time = data[i] / max;
            var color = colorGradient.Evaluate(time);
            Globe.FadeCountryIntoColor(countries[i], color, duration);
        }
    }

    public void HighlightCountry(string country)
    {
        Globe.EnlargeCountry(country);
    }

    private IEnumerator ResetRotation(float timeToReset, Action onComplete)
    {
        var start = transform.rotation;
        var end = Quaternion.Euler(DefaultRotation);
        float time = 0;

        while(time < timeToReset)
        {
            transform.rotation = Quaternion.Lerp(start, end, time/timeToReset);
            time += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke();
    }

    private void AdjustRotateSensitivity(float percent)
    {
        RotateSensitivity = RotateSensitivityMin + ((RotateSensitivityMax - RotateSensitivityMin) * percent);
    }

    private void OnMouseDown()
    {
        Dragging = true;
    }

    private void OnMouseUp()
    {
        Dragging = false;
    }

    private void OnMouseOver()
    {
        OverGlobe = true;
    }

    private void OnMouseExit()
    {
        OverGlobe = false;
    }
}
