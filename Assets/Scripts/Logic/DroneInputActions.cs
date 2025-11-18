using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class DroneInputActions : MonoBehaviour {
    [SerializeField] InputSystem_Actions inputActions;

    public event Action<Vector2> OnMoveEvent;
    public event Action<float> OnHoverEvent;

    private Vector2 movementDir;
    private float hoverVal;

    private void Awake() {
        inputActions = new();
        inputActions.Player.Enable();
    }

    private void Update() {
        movementDir = inputActions.Player.Move.ReadValue<Vector2>();
        hoverVal = inputActions.Player.Hover.ReadValue<float>();

        OnMoveEvent?.Invoke(movementDir);
        OnHoverEvent?.Invoke(hoverVal);
    }
}