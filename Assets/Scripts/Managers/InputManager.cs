using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputInfo
{
    public float ScrollDelta;
    public bool Reset_KeyDown;
    public bool MouseLPressed;  // The left click held down 
    public bool MouseLDown;     // The left click began being held down 
    public bool MouseLClicked;  // On release of the left click after less than ClickDownMax seconds
    public bool MouseLHold;     // Left click had been held down for longer than HoldDownMin seconds 
    public bool MouseLHoldReleased; // Left click released after a hold
    public bool MouseLHoldStatic;   // Has the mouse remained mostly stationary while held down?
    public float MouseLHoldDuration;// How long the left mouse has been pressed down
    public Vector2 MousePos;
    public Vector2 MousePosPrev;
    public Vector2 MousePosDelta;
}
public class InputManager : MonoBehaviour
{
    [SerializeField]
    private float ClickDownMax;
    [SerializeField]
    private float HoldDownMin;
    [SerializeField]
    private float StaticHoldThreshold;

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
            CurrentInput.MouseLClicked = false;
            CurrentInput.MouseLHold = false;
            CurrentInput.MouseLHoldReleased = false;
            CurrentInput.MouseLDown = Input.GetMouseButtonDown(0);

            if (CurrentInput.MouseLDown)
            {
                CurrentInput.MouseLHoldDuration = 0;
                CurrentInput.MouseLHoldStatic = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                CurrentInput.MouseLHoldDuration += Time.deltaTime;
                if (CurrentInput.MouseLHoldDuration < ClickDownMax)
                {
                    CurrentInput.MouseLClicked = true;
                }
                if (CurrentInput.MouseLHoldDuration > HoldDownMin)
                {
                    CurrentInput.MouseLHoldReleased = true;
                }
                CurrentInput.MouseLHoldDuration = 0;
            }
            else if (CurrentInput.MouseLPressed)
            {
                CurrentInput.MouseLHoldDuration += Time.deltaTime;
                if (CurrentInput.MouseLHoldDuration > HoldDownMin)
                {
                    CurrentInput.MouseLHold = true;
                    if (Mathf.Abs(CurrentInput.MousePosDelta.x) > StaticHoldThreshold || 
                        Mathf.Abs(CurrentInput.MousePosDelta.y) > StaticHoldThreshold)
                    {
                        CurrentInput.MouseLHoldStatic = false;
                    }
                }
            }
            else
            {
                CurrentInput.MouseLHoldStatic = false;
            }

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
