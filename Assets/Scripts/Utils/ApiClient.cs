using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiClient
{
    private const string BaseUrl = "http://127.0.0.1:8000";

    // This method automates ALL the manual UnityWebRequest setup and JSON conversion.
    // TRequest: The C# object you are sending (e.g., DummyRequestData)
    // TResponse: The C# object you expect to receive (e.g., DummyResponse)
    public static async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        string url = $"{BaseUrl}/{endpoint}";

        // 1. Serialization (Object to JSON string)
        string jsonPayload = JsonUtility.ToJson(data);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        using UnityWebRequest req = new(url);
        req.method = UnityWebRequest.kHttpVerbPOST;

        // 2. AUTOMATION: Setup all handlers and headers automatically
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.SetRequestHeader("Content-Type", "application/json");
        req.downloadHandler = new DownloadHandlerBuffer(); // CRITICAL: Ensures we can read the response body

        // 3. Send and await (UniTask handles the heavy lifting here)
        await req.SendWebRequest();

        // 4. Handle Errors
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"API FAILED [{url}]: {req.error}");
            throw new System.Exception(req.downloadHandler.text); // Throw server body as exception text
        }

        TResponse responseObject = JsonUtility.FromJson<TResponse>(req.downloadHandler.text);
        return responseObject;
    }
}
