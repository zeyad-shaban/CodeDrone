using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
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

    void Awake()
    {
        flagDetector = transform.GetComponent<FlagDetector>();
        droneController = transform.GetComponent<DroneController>();
        state = AgentState.Searching;
    }

    void Update()
    {
        if (state == AgentState.Searching)
        {
            Searching();
        }
    }

    void Start()
    {
        Transform[] searchingTransforms = searchObjsParent.GetComponentsInChildren<Transform>();
        if (searchingTransforms.Length != 5)
        {
            Debug.LogError("Incomplete Search Grid");
            return;
        }

        Vector3[] searchCorners = new Vector3[4];
        for (int i = 0; i < 4; ++i)
        {
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

    public void Searching()
    {
        if (currSearchTargetPnt >= searchGrid.Count)
        {
            state = AgentState.ReturnToLaunch;
            return;
        }

        if (droneController.MoveToPoint(searchGrid[currSearchTargetPnt]))
            ++currSearchTargetPnt;
    }
}