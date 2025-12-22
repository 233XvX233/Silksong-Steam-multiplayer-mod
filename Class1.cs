namespace SilksongMultiplayer
{
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using BepInEx;
    using BepInEx.Configuration;
    using BepInEx.Logging;
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
        private ConfigEntry<float> BossHPmultiplier;
        private ConfigEntry<float> EnemyHPmultiplier;

        private ConfigEntry<string> SkinName;
        private ConfigEntry<string> SkinLink1;
        private ConfigEntry<string> SkinLink2;
        private ConfigEntry<string> SkinLink3;
        private ConfigEntry<string> SkinLink4;


        void Awake()
        {
            // 初始化配置（如果配置文件里没有，就会写入默认值）
            enablePvP = Config.Bind("General", "enablePvP", false, "是否开启pvp");

            SilksongMultiplayerAPI.enablePvP = enablePvP.Value;

            BossHPmultiplier = Config.Bind("General", "BossHPmultiplier", 1f, "boss血量倍率(对于每个额外玩家，设置为0不加血量，设置为1则每多一个玩家血量加一倍)");

            SilksongMultiplayerAPI.BossHPmultiplier = BossHPmultiplier.Value;


            EnemyHPmultiplier = Config.Bind("General", "EnemyHPmultiplier", 0f, "普通敌人血量倍率(对于每个额外玩家，设置为0不加血量，设置为1则每多一个玩家血量加一倍)");
            SilksongMultiplayerAPI.EnemyHPmultiplier = EnemyHPmultiplier.Value;

            SkinName = Config.Bind("Skin", "SkinName", "default", "皮肤名称，对应皮肤文件夹名称，尽量不要和其他皮肤重复然后加上皮肤版本号，名称模板(abcd123)");
            SkinLink1 = Config.Bind("Skin", "SkinLink1", "", "皮肤图片链接，对应图片knight atlas0");
            SkinLink2 = Config.Bind("Skin", "SkinLink2", "", "皮肤图片链接，对应图片knight atlas1");
            SkinLink3 = Config.Bind("Skin", "SkinLink3", "", "皮肤图片链接，对应图片knight atlas2");
            SkinLink4 = Config.Bind("Skin", "SkinLink4", "", "皮肤图片链接，对应图片knight atlas3");

            SilksongMultiplayerAPI.skinName = SkinName.Value;
            SilksongMultiplayerAPI.skinLink1 = SkinLink1.Value;
            SilksongMultiplayerAPI.skinLink2 = SkinLink2.Value;
            SilksongMultiplayerAPI.skinLink3 = SkinLink3.Value;
            SilksongMultiplayerAPI.skinLink4 = SkinLink4.Value;


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
