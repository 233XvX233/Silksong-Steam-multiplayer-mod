using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class FeedbackEntry
{
    public string sceneName;
    public float x;
    public float y;
    public string message;
}

public class GithubTextReader : MonoBehaviour
{
    // 换成你自己的 raw 链接
    private const string RawUrl =
        "https://raw.githubusercontent.com/233XvX233/Silksong-Steam-multiplayer-mod-data/refs/heads/main/comments.txt";

    public List<FeedbackEntry> entries = new List<FeedbackEntry>();
    public void Load()
    {
        StartCoroutine(LoadCo());
    }

    private IEnumerator LoadCo()
    {
        using var req = UnityWebRequest.Get(RawUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Load failed: {req.responseCode}\n{req.error}");
            yield break;
        }

        string text = req.downloadHandler.text;
        entries = ParseEntries(text);

        Debug.Log($"Loaded entries: {entries.Count}");
        if (entries.Count > 0)
        {
            Debug.Log($"First: {entries[0].sceneName} ({entries[0].x},{entries[0].y}) {entries[0].message}");
        }
    }

    // 解析你给的格式：key: value，每条记录之间用空行分隔
    private static List<FeedbackEntry> ParseEntries(string raw)
    {
        var results = new List<FeedbackEntry>();
        if (string.IsNullOrWhiteSpace(raw)) return results;

        // 统一换行符
        raw = raw.Replace("\r\n", "\n").Replace("\r", "\n");

        // 用空行切块（允许多个空行）
        string[] blocks = raw.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string b in blocks)
        {
            string block = b.Trim();
            if (block.Length == 0) continue;

            var entry = new FeedbackEntry();

            string[] lines = block.Split('\n');
            foreach (string line0 in lines)
            {
                string line = line0.Trim();
                if (line.Length == 0) continue;

                int idx = line.IndexOf(':');
                if (idx < 0) continue;

                string key = line.Substring(0, idx).Trim();
                string val = line.Substring(idx + 1).Trim();

                switch (key)
                {
                    case "sceneName":
                        entry.sceneName = val;
                        break;
                    case "x":
                        float.TryParse(val, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out entry.x);
                        break;
                    case "y":
                        float.TryParse(val, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out entry.y);
                        break;
                    case "message":
                        entry.message = val;
                        break;
                }
            }

            // 简单校验：至少要有 sceneName 和 message 才算一条
            if (!string.IsNullOrEmpty(entry.sceneName) && entry.message != null)
                results.Add(entry);
        }

        return results;
    }
}
