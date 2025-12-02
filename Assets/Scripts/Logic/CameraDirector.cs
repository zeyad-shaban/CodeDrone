using Unity.Cinemachine;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    [SerializeField] CinemachineCamera followCam;
    [SerializeField] CinemachineCamera droneCam;
    [SerializeField] CinemachineCamera topViewCam;
    [SerializeField] CinemachineCamera payloadCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchToCamera(topViewCam);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchToCamera(followCam);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchToCamera(droneCam);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchToCamera(topViewCam);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SwitchToCamera(payloadCam);

    }

    private void SwitchToCamera(CinemachineCamera cam)
    {
        followCam.Priority = 0;
        droneCam.Priority = 0;
        topViewCam.Priority = 0;
        payloadCam.Priority = 0;

        cam.Priority = 10;
    }
}
