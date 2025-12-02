using System;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    [SerializeField] private float hoverSpeed = 7;
    [SerializeField] private float flySpeed = 6;
    [SerializeField] float rotSpeed = 5;
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
    }


    public bool MoveToPoint(Vector2 targetPnt)
    {
        return MoveToPoint(new Vector3(targetPnt.x, 0, targetPnt.y));
    }

    public bool MoveToPoint(Vector3 targetPnt)
    {
        Vector3 dir = targetPnt - transform.position;
        dir.y = 0;

        if (dir.magnitude <= reachedThresh)
            return true;

        MoveInDirection(dir.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * Time.deltaTime);
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
