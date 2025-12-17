using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public enum AgentState {
    Searching,
    GoToFlag,
    HoverWaitDrop,
    ReturnToLaunch,
}

// THE MAIN GOD OF THE DRONE
public class Brain : MonoBehaviour {
    #region params
    // Events
    public event Action OnCutRope;
    // End Events

    // Parameters
    private static float flagMergingThresh = 5.0f;
    private static int minConfirmationScore = 20;
    private static float mowOverlap = 0.2f; // 20% overlap between Mow lines
    private static float minLastTimeDetected = 2.0f;
    private static float maxDropWaitTime = 2;
    private static float angleErr = 20;
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
    [SerializeField] GameObject searchObjsParent;

    private List<Vector2> searchGrid = new();
    private int currSearchTargetPnt = 0;
    DroneController droneController;
    #endregion params

    void Awake() {
        flagDetector = transform.GetComponent<FlagDetector>();
        droneController = transform.GetComponent<DroneController>();
        state = AgentState.Searching;
    }

    void Start() {
        Transform[] searchingTransforms = searchObjsParent.GetComponentsInChildren<Transform>();
        if (searchingTransforms.Length != 5) {
            Debug.LogError("Incomplete Search Grid");
            return;
        }

        Vector3[] searchCorners = new Vector3[4];
        for (int i = 0; i < 4; ++i) {
            searchCorners[i] = searchingTransforms[i + 1].position;
        }

        float fovRad = (flagDetector.GetDroneCamera().fieldOfView - angleErr) * Mathf.PI / 180;
        coveredHeight = 2 * transform.position.y * Mathf.Tan(fovRad / 2.0f);
        coveredWidth = flagDetector.GetDroneCamera().aspect * coveredHeight;
        effectiveMowWidth = coveredWidth * (1 - mowOverlap);
        effectiveMowHeight = coveredHeight * (1 - mowOverlap);

        searchGrid = SearchGridPlanner.GenerateSearchGrid(searchCorners, effectiveMowHeight, effectiveMowWidth, transform.position);

        gameObject.GetComponent<GridVisualizer>().VisualizePath(searchGrid);
    }

    void OnEnable() {
        flagDetector.OnFlagDetected += OnFlagDetectedHandler;
    }

    void OnDisable() {
        flagDetector.OnFlagDetected -= OnFlagDetectedHandler;
    }


    void Update() {
        if (state == AgentState.Searching) {
            Searching();
        }
        if (state == AgentState.GoToFlag) {
            GoToFlag();
        }
        if (state == AgentState.HoverWaitDrop) {
            HoverWaitDrop();
        }
    }

    public void Searching() {
        if (currSearchTargetPnt >= searchGrid.Count) {
            state = AgentState.ReturnToLaunch;
            return;
        }

        if (droneController.MoveToPoint(searchGrid[currSearchTargetPnt]))
            ++currSearchTargetPnt;
    }

    public void GoToFlag() {
        Debug.Log($"IT'S HERO TIME {dropAtTarget.worldPos}");
        if (droneController.MoveToPoint(dropAtTarget.worldPos)) {
            state = AgentState.HoverWaitDrop;
        }
        gameObject.GetComponent<GridVisualizer>().VisualizeTargetRay(dropAtTarget.worldPos, transform.forward * 0.3f);
    }

    public void HoverWaitDrop() {
        currDropWaitTime += Time.deltaTime;
        if (currDropWaitTime >= maxDropWaitTime) {
            currDropWaitTime = 0;
            OnCutRope?.Invoke();
            state = AgentState.ReturnToLaunch;
        }
    }

    private void OnFlagDetectedHandler(DetectionResult detections) {
        if (detections.conf.Length == 0 || state != AgentState.Searching)
            return;

        for (int i = 0; i < detections.conf.Length; ++i) {
            float x1 = detections.boxes[i * 4];
            float y1 = detections.boxes[i * 4 + 1];
            float x2 = detections.boxes[i * 4 + 2];
            float y2 = detections.boxes[i * 4 + 3];

            float sx = (x1 + x2) / 2.0f;
            float sy = FlagDetector.IMG_HEIGHT - (y1 + y2) / 2.0f;

            Vector3 pos = new(sx, sy, 0);
            Vector3 worldPos = flagDetector.GetWorldPos(pos, 0, 0.3f);

            dropAtTarget = new Target(worldPos);
        }

        if (state == AgentState.Searching) {
            state = AgentState.GoToFlag;
        }
    }
}
