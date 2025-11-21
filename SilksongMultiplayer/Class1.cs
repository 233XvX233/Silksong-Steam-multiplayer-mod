namespace SilksongMultiplayer
{
    using System.Reflection;
    using BepInEx;
    using BepInEx.Logging;
    using BepInEx.Configuration;
    using GlobalSettings;
    using HarmonyLib;
    using HutongGames.PlayMaker.Actions;
    using Steamworks;
    using TeamCherry.Localization;
    using UnityEngine;

    //[HarmonyPatch(typeof(SomeGameClass), "TargetMethod")]
    public static class PatchClass
    {
        static void Prefix()
        {
            Debug.Log("目标方法被调用了！");
        }
    }

    [BepInPlugin("com.XvX", "XvX", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        // 定义配置项
        private ConfigEntry<bool> enablePvP;
        private ConfigEntry<bool> enableBossSync;

        void Awake()
        {
            // 初始化配置（如果配置文件里没有，就会写入默认值）
            enablePvP = Config.Bind("General", "enablePvP", false, "是否开启pvp");

            SilksongMultiplayerAPI.enablePvP = enablePvP.Value;


            // Plugin startup logic
            SilksongMultiplayerAPI.Logger = base.Logger;
            Logger.LogInfo($"Plugin XvX is loaded!");

            Harmony harmony = new Harmony("com.XvX");
            harmony.PatchAll();

            Debug.Log("初始化大厅系统");

            SilksongMultiplayerAPI.RoomManagerObject = GameObject.Instantiate(new GameObject("LobbyManager"));
            SilksongMultiplayerAPI.RoomManagerObject.AddComponent<RoomManager>();
            SilksongMultiplayerAPI.RoomManagerObject.AddComponent<NetworkDataReceiver>();

            SilksongMultiplayerAPI.RoomManagerObject = GameObject.Find("LobbyManager(Clone)");

            GameObject.DontDestroyOnLoad(SilksongMultiplayerAPI.RoomManagerObject);
        }
    }

    [HarmonyPatch(typeof(StartManager), nameof(StartManager.SwitchToMenuScene))]
    static class RestoreLanguagePatch
    {
        // 前置方法
        static void Prefix()
        {
            Debug.Log("[Harmony] RestoreLanguageSelection 被调用");

            SilksongMultiplayerAPI.RoomManagerObject = GameObject.Instantiate(new GameObject("LobbyManager"));
            SilksongMultiplayerAPI.RoomManagerObject.AddComponent<RoomManager>();
            SilksongMultiplayerAPI.RoomManagerObject.AddComponent<NetworkDataReceiver>();

            SilksongMultiplayerAPI.RoomManagerObject = GameObject.Find("LobbyManager(Clone)");

            GameObject.DontDestroyOnLoad(SilksongMultiplayerAPI.RoomManagerObject);

            //Logger.LogInfo($"LobbyManager is start");


            // 如果想直接替换返回值，可以：
            // __result = "en";
            // return false; // 阻止原方法执行
        }
    }
}
