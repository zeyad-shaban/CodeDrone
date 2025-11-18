using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum AgentState
{
    Searching,
    Dropping,
    HoverWaitDrop,
    ReturnToLaunch,
}

// THE MAIN GOD OF THE DRONE
public class Brain : MonoBehaviour
{
    // Events
    public event Action OnCutRope;
    // End Events

    // Parameters
    private static float flagMergingThresh = 5.0f;
    private static int minConfirmationScore = 20;
    private static float mowOverlap = 0.2f; // 20% overlap between Mow lines
    private static float minLastTimeDetected = 2.0f;
    private static float maxDropWaitTime = 2;
    // End parameters
    private float currDropWaitTime = 0;

    // Calculated Parameters
    private float coveredHeight = 0;
    private float coveredWidth = 0;
    private float effectiveMowWidth = 0;
    private float effectiveMowHeight = 0;
    // End calculated parameters

    private FlagDetector flagDetector;
    private AgentState state;

    private TargetsList targets = new TargetsList(mergingThresh: flagMergingThresh, minConfirmationScore: minConfirmationScore, minLastTimeDetected: minLastTimeDetected);
    private Target dropAtTarget;

    [Header("Search Algorithm")]
    [SerializeField] Vector3[] searchCorners = new Vector3[4];
    private List<Vector2> searchGrid = new();
    private int currSearchTargetPnt = 0;
    DroneController droneController;

    void Awake()
    {
        flagDetector = transform.GetComponent<FlagDetector>();
        droneController = transform.GetComponent<DroneController>();
        state = AgentState.Searching;
    }

    void Start()
    {
        float fovRad = flagDetector.GetDroneCamera().fieldOfView * Mathf.PI / 180;
        coveredHeight = 2 * transform.position.y * Mathf.Tan(fovRad / 2.0f);
        coveredWidth = flagDetector.GetDroneCamera().aspect * coveredHeight;
        effectiveMowWidth = coveredWidth * (1 - mowOverlap);
        effectiveMowHeight = coveredHeight * (1 - mowOverlap);

        searchGrid = SearchGridPlanner.GenerateSearchGrid(searchCorners, effectiveMowHeight, effectiveMowWidth, transform.position);

        // DEBUGGING
        // foreach (Vector2 pnt in searchGrid)
        // {
        //     Vector3 pos = new Vector3(pnt.x, 9, pnt.y);
        //     GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     cube.transform.position = pos;
        //     cube.transform.localScale = new Vector3(2, 100, 2);
        //     cube.GetComponent<Renderer>().material.color = Color.red;
        // }
    }

    void OnEnable()
    {
        flagDetector.OnFlagDetected += OnFlagDetectedHandler;
    }

    void OnDisable()
    {
        flagDetector.OnFlagDetected -= OnFlagDetectedHandler;
    }

    void Update()
    {
        if (state == AgentState.Searching)
        {
            Searching();
            targets.ClearOld();
        }
        else if (state == AgentState.Dropping)
        {
            Dropping();
        }
        else if (state == AgentState.HoverWaitDrop)
        {
            HoverWaitDrop();
        }
        else if (state == AgentState.ReturnToLaunch)
        {
            ReturnToLaunch();
        }

        // DEBUGGING
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector3 mousePos = Input.mousePosition;
        //     Vector3 worldPos = flagDetector.GetWorldPos(mousePos, 0, 0.9f);
        //     worldPos.x = worldPos.x * FlagDetector.IMG_WIDTH / Screen.width;
        //     worldPos.y = worldPos.y * FlagDetector.IMG_HEIGHT / Screen.height;
        //     Debug.Log($"[DEBUG] Mouse Pos {mousePos} -> {worldPos}");
        // }
    }

    public void Searching()
    {
        if (droneController.MoveToPoint(searchGrid[currSearchTargetPnt]))
            ++currSearchTargetPnt;
    }

    public void Dropping()
    {
        Debug.Log($"IT'S HERO TIME {dropAtTarget.worldPos}");
        if (droneController.MoveToPoint(dropAtTarget.worldPos))
        {
            state = AgentState.HoverWaitDrop;
        }
    }

    public void HoverWaitDrop()
    {
        currDropWaitTime += Time.deltaTime;
        if (currDropWaitTime >= maxDropWaitTime)
        {
            currDropWaitTime = 0;
            OnCutRope?.Invoke();
            state = AgentState.ReturnToLaunch;
        }
    }
    public void ReturnToLaunch()
    {
        Debug.Log("WE FUCKN DID IT LETS GOO!");
    }

    private void OnFlagDetectedHandler(DetectionResult detections)
    {
        if (detections.conf.Length == 0)
            return;

        for (int i = 0; i < detections.conf.Length; ++i)
        {
            float x1 = detections.boxes[i * 4];
            float y1 = detections.boxes[i * 4 + 1];
            float x2 = detections.boxes[i * 4 + 2];
            float y2 = detections.boxes[i * 4 + 3];

            float sx = (x1 + x2) / 2.0f;
            float sy = FlagDetector.IMG_HEIGHT - (y1 + y2) / 2.0f;

            Vector3 pos = new(sx, sy, 0);
            Vector3 worldPos = flagDetector.GetWorldPos(pos, 0, 0.3f);
            // Debug.Log($"Predicted {pos} -> {worldPos}");

            Target target = new Target(worldPos);
            targets.Add(target, detections.conf[i]);
            // Debug.Log(targets.Count);
        }
        Debug.Log($"#Detected: {detections.conf.Length}, #targets: {targets.Count}, #Confirmed {targets.GetConfirmedTargets().Count}");

        if (targets.GetConfirmedTargets().Count > 0)
        {
            dropAtTarget = targets.ArchivedPopConfirmed();

            if (state == AgentState.Searching)
                state = AgentState.Dropping;
        }
    }

    public void DestroySelf()
    {
        Destroy(this);
    }
}