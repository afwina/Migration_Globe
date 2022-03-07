using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraManager : InputHandler
{
    public static Action<float> OnZoomChanged;

    [SerializeField]
    private float MinZ;
    [SerializeField]
    private float MaxZ;
    [SerializeField]
    private float Sensitivity;

    private Transform Camera;
   
    private void Start()
    {
        Camera = GetComponent<Camera>()?.transform;
        if (Camera == null)
        {
            Debug.LogError("No camera attached to CameraManager!");
        }

        GlobeManager.OnReset += ResetZoom;
    }

    private void OnDestroy()
    {
        GlobeManager.OnReset -= ResetZoom;
    }

    public override void HandleInput(InputInfo input)
    {
        if (input.ScrollDelta != 0)
        {
            var target = Mathf.Abs(Camera.position.z) - Sensitivity * input.ScrollDelta;
            var clamped = -Mathf.Clamp(target, MinZ, MaxZ);
            Camera.position = new Vector3(Camera.position.x, Camera.position.y, clamped);
            OnZoomChanged?.Invoke(((-clamped) - MinZ) / (MaxZ - MinZ));
        }
    }

    public void ResetZoom(float time)
    {
        StartCoroutine(ZoomToTarget(-MaxZ, time));
    }

    public IEnumerator ZoomToTarget(float target, float duration)
    {
        float start = transform.position.z;
        float time = 0;

        while (time < duration)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(start, target, time / duration));
            time += Time.deltaTime;
            yield return null;
        }
    }
}
