using SilksongMultiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RuntimeFeedbackUI : MonoBehaviour
{
    // 你要禁用的“游戏操作脚本”（玩家移动、相机、攻击等）
    [Header("Disable these while UI is open")]
    public MonoBehaviour[] disableWhileOpen = new MonoBehaviour[0];

    private GameObject canvasGO;
    private GameObject panel;
    private InputField inputField;

    private bool isOpen;

    void Update()
    {
        // 打开/关闭 UI（你可以改键）
        if (Input.GetKeyDown(KeyCode.F6))
        {
            if (canvasGO == null) CreateUI();
            if (!isOpen) Open();
            else Close();
        }

        // UI 打开时：Enter 提交，Esc 取消
        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Submit();
            }
        }
    }

    void CreateUI()
    {
        // ===== Canvas =====
        canvasGO = new GameObject("FeedbackCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem（必须）
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // ===== Panel =====
        panel = CreateUIObject("Panel", canvasGO.transform);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.6f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(560, 220);
        panelRT.anchoredPosition = Vector2.zero;

        // ===== InputField =====
        var inputGO = CreateUIObject("InputField", panel.transform);
        var inputImg = inputGO.AddComponent<Image>();
        inputImg.color = Color.white;

        inputField = inputGO.AddComponent<InputField>();
        var inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(520, 80);
        inputRT.anchoredPosition = new Vector2(0, 10);

        // Text
        Text text = CreateText("Text", inputGO.transform, "");
        text.alignment = TextAnchor.UpperLeft;
        inputField.textComponent = text;

        // Placeholder
        Text placeholder = CreateText("Placeholder", inputGO.transform, "留下线痕…（Enter提交 / Esc取消）");
        placeholder.color = Color.gray;
        inputField.placeholder = placeholder;

        // 默认先隐藏
        panel.SetActive(false);
        isOpen = false;
    }

    void Open()
    {
        isOpen = true;
        panel.SetActive(true);

        // 清空并聚焦输入框
        inputField.text = "";
        inputField.ActivateInputField();

        // 解锁鼠标（如果你游戏锁鼠标）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 禁用游戏操作脚本
        foreach (var c in disableWhileOpen)
        {
            if (c != null) c.enabled = false;
        }
    }

    void Close()
    {
        isOpen = false;
        panel.SetActive(false);

        // 恢复鼠标（按你的游戏需求决定是否锁回去）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 恢复游戏操作脚本
        foreach (var c in disableWhileOpen)
        {
            if (c != null) c.enabled = true;
        }
    }

    void Submit()
    {
        string msg = inputField.text.Trim();
        if (msg.Length < 2) return; // 你后端要求 >=2

        Debug.Log("Feedback: " + msg);

        // TODO: 接你现有发送器（示例）
        Vector2 playerPos = SilksongMultiplayerAPI.Hero_Hornet.transform.position;

        SilksongMultiplayerAPI.RoomManager.feedbackSender.SendFeedback(playerPos, msg);

        GameObject comment = GameObject.Instantiate(new GameObject(), SilksongMultiplayerAPI.Hero_Hornet.transform.position + new Vector3(0,0,0),Quaternion.identity);
        PlayerComment playerComment = comment.AddComponent<PlayerComment>();
        playerComment.Init(msg, playerPos);
        Close();
    }

    // ===== 工具方法 =====
    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    Text CreateText(string name, Transform parent, string content)
    {
        GameObject go = CreateUIObject(name, parent);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = content;
        t.color = Color.black;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(12, 10);
        rt.offsetMax = new Vector2(-12, -10);

        return t;
    }
}
