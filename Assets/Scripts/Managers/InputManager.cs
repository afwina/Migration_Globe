using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputInfo
{
    public float ScrollDelta;
    public bool Reset_KeyDown;
    public bool MouseLPressed;
    public Vector2 MousePos;
    public Vector2 MousePosPrev;
    public Vector2 MousePosDelta;
}
public class InputManager : MonoBehaviour
{
    private static List<InputHandler> InputHandlers = new List<InputHandler>();
    private InputInfo CurrentInput = new InputInfo();
    private static bool EnableInput = true;

    public static void RegisterInputHandler(InputHandler handler)
    {
        InputHandlers.Add(handler);
    }

    public static void RemoveInputHandler(InputHandler handler)
    {
        InputHandlers.Remove(handler);
    }

    public static void Enable()
    {
        EnableInput = true;
    } 

    public static void Disable()
    {
        EnableInput = false;
    }

    void Update()
    {
        if (EnableInput)
        {
            CurrentInput.ScrollDelta = Input.mouseScrollDelta.y;
            CurrentInput.Reset_KeyDown = Input.GetKeyDown(KeyCode.R);
            CurrentInput.MouseLPressed = Input.GetMouseButton(0);
            CurrentInput.MousePosPrev = CurrentInput.MousePos;
            CurrentInput.MousePos = Input.mousePosition;
            CurrentInput.MousePosDelta = CurrentInput.MousePos - CurrentInput.MousePosPrev;

            foreach (var handler in InputHandlers)
            {
                handler.HandleInput(CurrentInput);
            }
        }
    }
}

public abstract class InputHandler : MonoBehaviour
{
    private void Awake()
    {
        InputManager.RegisterInputHandler(this);
    }

    public virtual void OnDestroy()
    {
        InputManager.RemoveInputHandler(this);
    }

    public abstract void HandleInput(InputInfo input);
}
