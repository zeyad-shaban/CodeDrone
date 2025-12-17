using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;


// The eyes of this body
public class FlagDetector : MonoBehaviour {
    public event Action<DetectionResult> OnFlagDetected;

    [Header("Capture")]
    [SerializeField] Camera droneCamera;
    [SerializeField] LayerMask groundLayer;
    public const int IMG_WIDTH = 704;
    public const int IMG_HEIGHT = 704;

    [Header("Networking")]
    private bool waitingResponse = false;

    [SerializeField] string flagDetectorEndpoint = "http://127.0.0.1:8000/detect_flags";
    [SerializeField] float confidenceThreshold = 0.5f;
    private const string FILE_KEY = "img_file";
    private const string CONF_KEY = "min_conf";


    private RenderTexture rt;

    async private void Start() {
        rt = new(IMG_WIDTH, IMG_HEIGHT, 16, RenderTextureFormat.ARGB32);
    }


    async private void Update() {
        if (waitingResponse)
            return;

        droneCamera.targetTexture = rt;
        RenderTexture.active = rt;

        droneCamera.Render();

        Texture2D img = new Texture2D(IMG_WIDTH, IMG_HEIGHT, TextureFormat.RGB24, false);
        img.ReadPixels(new Rect(0, 0, IMG_WIDTH, IMG_HEIGHT), 0, 0);
        img.Apply();

        waitingResponse = true;
        try {
            await SendFile(img);
        }
        catch (Exception ex) {
            Debug.LogError($"SendFile exception: {ex}");
        }
        finally {
            waitingResponse = false;
            DestroyImmediate(img);
        }
    }

    async private Task<bool> SendFile(Texture2D img) {
        byte[] imgBytes = img.EncodeToPNG();

        WWWForm form = new();
        form.AddField(CONF_KEY, "0.7");
        form.AddBinaryData(FILE_KEY, imgBytes, "image.png", "image/png");

        using UnityWebRequest req = UnityWebRequest.Post(flagDetectorEndpoint, form);
        await req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success) {
            Debug.Log($"API call to {flagDetectorEndpoint} Failed: {req.error}");
            Debug.Log($"Err msg: {req.downloadHandler.text}");
            return false;
        }

        DetectionResult res = JsonUtility.FromJson<DetectionResult>(req.downloadHandler.text);
        OnFlagDetected?.Invoke(res);

        if (res.conf.Length != 0)
            Debug.Log("A flag was detected");


        return true;
    }

    public Camera GetDroneCamera() {
        return droneCamera;
    }

    public Vector3 GetWorldPos(Vector3 pos, float noiseMean = 0.0f, float noiseStd = 0.0f) {
        droneCamera.targetTexture = rt;

        Ray ray = droneCamera.ScreenPointToRay(pos);
        RaycastHit hit;

        Vector3 worldPos = Vector3.zero;
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer)) {
            Vector3 noise = NoiseGenerator.GetGaussianVector3(noiseMean, noiseStd);
            worldPos = hit.point + noise;
        }

        droneCamera.targetTexture = null;
        return worldPos;
    }
}