using System;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public enum ActionSpace
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward,
        // DropPayload,
        Nothing,
    }

    // Flight required vars
    [SerializeField] private Transform targetObj;

    // Drone Params
    [SerializeField] private float flySpeed = 6;
    [SerializeField] private float hoverSpeed = 7;
    [SerializeField] bool isAuto = false;

    // Helper params
    private float reachedThresh = 0.5f;

    private DroneInputActions droneInputActions;

    private void Start()
    {
        droneInputActions = GetComponent<DroneInputActions>();
        droneInputActions.OnMoveEvent += MoveInDirection;
        droneInputActions.OnHoverEvent += DoHover;
    }

    private void Update()
    {
        if (isAuto)
        {
            Vector3 targetPnt3d = targetObj.transform.position;
            bool reached = MoveToPoint(new Vector2(targetPnt3d.x, targetPnt3d.z));
        }
    }

    public bool MoveToPoint(Vector2 targetPnt)
    {
        Vector3 pnt = new(targetPnt.x, 0, targetPnt.y);
        return MoveToPoint(pnt);
    }
    public bool MoveToPoint(Vector3 targetPnt)
    {
        // Returns true if reached
        Vector3 dir = targetPnt - transform.position;
        dir.y = 0;

        if (dir.magnitude <= reachedThresh)
            return true;

        MoveInDirection(dir.normalized);
        transform.rotation = Quaternion.LookRotation(dir);
        return false;
    }

    public void MoveInDirection(Vector2 direction2D) =>
        MoveInDirection(new Vector3(direction2D.x, 0, direction2D.y));

    public void MoveInDirection(Vector3 direction)
    {
        direction = direction * Time.deltaTime * flySpeed;
        transform.position += direction;
    }
    public void DoHover(float hoverVal)
    {
        hoverVal = hoverVal * Time.deltaTime * hoverSpeed;
        transform.position += new Vector3(0, hoverVal, 0);
    }
}
