using System;
using HarmonyLib;
using UnityEngine;
using SilksongMultiplayer;

public class BattleSceneSync
{
    public class BattleSceneDataComponent : MonoBehaviour
    {
        public bool started = false;
    }

    [HarmonyPatch(typeof(BattleScene), nameof(BattleScene.StartBattle))]
    public static class BattleScene_StartBattle_Patch
    {
        static bool Prefix(BattleScene __instance)
        {
            __instance.gameObject.AddComponent<BattleSceneDataComponent>();
            __instance.gameObject.GetComponent<BattleSceneDataComponent>().started = true;
            Debug.Log($"[WaveEnd Hook] currentWave={__instance.currentWave}");

            if (SilksongMultiplayerAPI.roomOwner == false)//不是房主
            {
                NetworkDataSender.SendBattleSceneWave(SilksongMultiplayerAPI.currentScene, __instance.gameObject.name, -1, false);
            }
            else if (SilksongMultiplayerAPI.roomOwner == true)//是房主
            {

                if (SilksongMultiplayerAPI.sceneEnemyData.TryGetValue(SilksongMultiplayerAPI.currentScene, out SceneEnemyData sceneData) == false)
                {
                    SilksongMultiplayerAPI.sceneEnemyData.Add(SilksongMultiplayerAPI.currentScene, new SceneEnemyData());
                }

                if (SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData.TryGetValue(__instance.gameObject.name, out int value) == false)
                    SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData.Add(__instance.gameObject.name, -1);

                if (-1 < SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData[__instance.gameObject.name])
                {
                    __instance.currentWave = SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData[__instance.gameObject.name];
                    SilksongMultiplayerAPI.pushWaveByOuther = true;
                    __instance.WaveEnd();
                }


            }


            return true; // 继续执行原 WaveEnd()
        }
    }
    [HarmonyPatch(typeof(BattleScene), nameof(BattleScene.WaveEnd))]
    public static class BattleScene_WaveEnd_Patch
    {
        // 返回 false = 阻止原 WaveEnd 执行
        static bool Prefix(BattleScene __instance)
        {
            Debug.Log($"[WaveEnd Hook] currentWave={__instance.currentWave}");
            if (__instance.gameObject.GetComponent<BattleSceneDataComponent>() == false)
            {
                SilksongMultiplayerAPI.pushWaveByOuther = false;
                return false;
            }
                

            if (SilksongMultiplayerAPI.pushWaveByOuther)
            {
                SilksongMultiplayerAPI.pushWaveByOuther = false;
            }
            else if(SilksongMultiplayerAPI.roomOwner == false)//不是房主
            {
                NetworkDataSender.SendBattleSceneWave(SilksongMultiplayerAPI.currentScene, __instance.gameObject.name, __instance.currentWave, false);
            }
            else if (SilksongMultiplayerAPI.roomOwner == true)//是房主
            {
                if (SilksongMultiplayerAPI.sceneEnemyData.TryGetValue(SilksongMultiplayerAPI.currentScene, out SceneEnemyData sceneData) == false)
                {
                    SilksongMultiplayerAPI.sceneEnemyData.Add(SilksongMultiplayerAPI.currentScene, new SceneEnemyData());
                }

                if (SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData.TryGetValue(__instance.gameObject.name, out int value) == false)
                    SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData.Add(__instance.gameObject.name, __instance.currentWave);

                if (__instance.currentWave > SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData[__instance.gameObject.name])
                {
                    SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData[__instance.gameObject.name] = __instance.currentWave;
                    NetworkDataSender.SendBattleSceneWave(SilksongMultiplayerAPI.currentScene, __instance.gameObject.name, __instance.currentWave, true);
                }
                else if (__instance.currentWave < SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData[__instance.gameObject.name])
                {
                    __instance.currentWave = SilksongMultiplayerAPI.sceneEnemyData[SilksongMultiplayerAPI.currentScene].battleSceneData[__instance.gameObject.name];
                }
            }


            return true; // 继续执行原 WaveEnd()
        }
    }
}
