using Unity.Cinemachine;
using UnityEngine;

public class CameraDirector : MonoBehaviour {
    [SerializeField] CinemachineCamera followCam;
    [SerializeField] CinemachineCamera droneCam;
    [SerializeField] CinemachineCamera topViewCam;
    [SerializeField] CinemachineCamera payloadCam;
    [SerializeField] Brain brain;
    [SerializeField] GridVisualizer gridVisualizer;
    bool visualized = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        SwitchToCamera(followCam);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchToCamera(droneCam);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchToCamera(followCam);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchToCamera(payloadCam);
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SwitchToCamera(topViewCam);
            if (!visualized) {
                visualized = true;
                gridVisualizer.VisualizePath(brain.searchGrid);
            }
        }

    }

    private void SwitchToCamera(CinemachineCamera cam) {
        followCam.Priority = 0;
        droneCam.Priority = 0;
        topViewCam.Priority = 0;
        payloadCam.Priority = 0;

        cam.Priority = 10;
    }
}
