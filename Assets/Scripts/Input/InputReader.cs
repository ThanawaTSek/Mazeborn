using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using static Control;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> PrimaryFireEvent;
    public event Action<bool> SprintEvent;

    private Control controls;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Control();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
    }
    
    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }
    
    private void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            SprintEvent?.Invoke(false);
        }
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PrimaryFireEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            PrimaryFireEvent?.Invoke(false);
        }
    }
}