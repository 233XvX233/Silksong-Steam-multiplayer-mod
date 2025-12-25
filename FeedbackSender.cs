using System;
using System.Collections;
using System.Text;
using SilksongMultiplayer;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[Serializable]
public class FeedbackPayload
{
    public string sceneName;
    public float x;
    public float y;
    public string message;
    public string platform;
}

public class FeedbackSender : MonoBehaviour
{
    private const string Endpoint = "https://comment.silk-echo.com/feedback";


    public void SendFeedback(Vector2 pos, string message)
    {
        StartCoroutine(SendFeedbackCo(pos, message));
    }

    private IEnumerator SendFeedbackCo(Vector2 pos, string message)
    {
        FeedbackPayload payload = new FeedbackPayload
        {
            sceneName = SceneManager.GetActiveScene().name,
            x = pos.x,
            y = pos.y,
            message = message,
            platform = SilksongMultiplayerAPI.RoomManager.playerID.m_SteamID.ToString(),
        };

        payload.message =
    $"sceneName: {payload.sceneName}\n" +
    $"x: {payload.x}\n" +
    $"y: {payload.y}\n" +
    $"message: {message}";

        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req =
            new UnityWebRequest(Endpoint, "POST");

        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"Feedback failed: {req.responseCode}\n{req.downloadHandler.text}"
            );
        }
        else
        {
            Debug.Log("Feedback sent successfully!");
            Debug.Log(req.downloadHandler.text); // issueUrl 在这里
        }
    }
}

