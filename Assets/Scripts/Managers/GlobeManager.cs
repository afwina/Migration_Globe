using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WPM;

public class GlobeManager : InputHandler
{
    public Action<float> OnReset;
    public Action<string> OnCountryHoveredChanged;
    public Action<string> OnGlobeClicked;
    public Action<string, bool> OnHold;
    public Action<string, bool, float> OnHoldReleased;
    public WorldMapGlobe WPMGlobe;


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
    private int HoveredCountryIndex = -1;

    private Color ZeroColor;
    public void Initialize(Color zeroColor)
    {
        WPMGlobe = WorldMapGlobe.instance;
        transform.rotation = Quaternion.Euler(DefaultRotation);
        RotateSensitivity = RotateSensitivityMax;
        ZeroColor = zeroColor;
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
            RotateGlobe(mouse);
        }

        if (WPMGlobe != null)
        {
            int prev = HoveredCountryIndex;
            HoveredCountryIndex = WPMGlobe.CheckCountryHover(input.MousePos);
            
            string country = null;
            if (HoveredCountryIndex != -1 && OverGlobe)
            {
                country = WPMGlobe.countries[HoveredCountryIndex].name;
            }
            
            if (prev != HoveredCountryIndex)
            {
                OnCountryHoveredChanged?.Invoke(country);
            }

            if (input.MouseLClicked)
            {
                OnGlobeClicked?.Invoke(country);
            }
            else if (input.MouseLHold)
            {
                OnHold?.Invoke(country, input.MouseLHoldStatic);
            }
            else if (input.MouseLHoldReleased)
            {
                OnHoldReleased?.Invoke(country, input.MouseLHoldStatic, input.MouseLHoldDuration);
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

    public void RotateGlobe(Vector3 mouse)
    {
        if (Vector3.Dot(transform.up, Vector3.up) >= 0)
        {
            transform.Rotate(transform.up, -Vector3.Dot(mouse, Camera.main.transform.right), Space.World);
        }
        else
        {
            transform.Rotate(transform.up, Vector3.Dot(mouse, Camera.main.transform.right), Space.World);
        }

        transform.Rotate(Camera.main.transform.right, Vector3.Dot(mouse, Camera.main.transform.up), Space.World);
    }

    public void ColorGlobe(Color color)
    {
        WPMGlobe.ColorAllRegionsInstant(color);
    }
    
    public void UpdateGlobe(uint[] data, float max, List<string> countries, Gradient colorGradient, float duration = 1)
    {
        WPMGlobe.StopFading();
        for (int i = 0; i < data.Length; i++)
        {
            float time = data[i] / max;
            Color color;
            if (time == 0)
            {
                color = ZeroColor;
            }
            else
            {
                color = colorGradient.Evaluate(time);
            }
            WPMGlobe.FadeCountryIntoColor(countries[i], color, duration);
        }
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

    public void AdjustRotateSensitivity(float percent)
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
