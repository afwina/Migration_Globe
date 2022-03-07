using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobeManager : InputHandler
{
    public static Action<float> OnReset;

    [SerializeField]
    private Vector3 DefaultRotation;
    [SerializeField]
    private float ResetSpeed = 0.1f;
    [SerializeField]
    private float RotateSensitivityMax;
    [SerializeField]
    private float RotateSensitivityMin;

    private bool Dragging = false;
    private float RotateSensitivity;

    private void Start()
    {
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
    }

    public void ResetGlobe()
    {
        var totalDiff = Mathf.Abs(transform.position.x - DefaultRotation.x) + Mathf.Abs(transform.position.y - DefaultRotation.y) + Mathf.Abs(transform.position.z - DefaultRotation.z);
        var timeToReset = totalDiff / ResetSpeed;

        OnReset?.Invoke(timeToReset);
        InputManager.Disable();
        StartCoroutine(ResetRotation(timeToReset, () => InputManager.Enable()));
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
}
