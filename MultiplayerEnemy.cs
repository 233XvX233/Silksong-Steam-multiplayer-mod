using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace SilksongMultiplayer
{
    static class MultiplayerEnemy
    {

    }

    [HarmonyPatch(typeof(Fsm))]
    [HarmonyPatch("Event", new[] { typeof(FsmEventTarget), typeof(FsmEvent) })]
    public class BossHook
    {
        static bool Prefix(Fsm __instance, FsmEventTarget eventTarget, FsmEvent fsmEvent)
        {

            if (eventTarget != null && eventTarget.target.ToString() != "self" && eventTarget.target.ToString() != "Self")
            {
                return true;
            }

            if (__instance.GameObject != null && __instance.GameObject.GetComponent<EnemyAvatar>() && __instance.GameObject.GetComponent<EnemyAvatar>().isOwner == false && __instance.GameObject.GetComponent<EnemyAvatar>().disConnect == false)
            {
                UnityEngine.Debug.Log($"[HOOK] 拦截试图事件: {fsmEvent.Name}");
                return false;
            }

            return true;
        }

        // Postfix 在原方法运行后调用
        static void Postfix(FsmEventTarget eventTarget, FsmEvent fsmEvent)
        {
            // 可以在这里做后处理，比如记录哪些事件实际成功派发
        }
    }

    [HarmonyPatch(typeof(Fsm), nameof(Fsm.SwitchState))]
    class FsmSwitchStatePatch
    {
        static void Prefix(Fsm __instance, FsmState toState)
        {
            if (toState != null && __instance.GameObject != null)
            {
                //UnityEngine.Debug.Log($"[HOOK] 试图切换到: {toState.Name}");
                if (__instance.GameObject.GetComponent<EnemyAvatar>())
                {
                    if(__instance.GameObject.GetComponent<EnemyAvatar>().isOwner == true)
                    {
                        NetworkDataSender.SendEnemyFsmStateData(__instance.GameObjectName, toState.Name, SilksongMultiplayerAPI.currentScene);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GetPosition), "DoGetPosition")]
    class Patch_GetPosition
    {
        static void Postfix(GetPosition __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>())
            {
                if (__instance.cachedTransform.gameObject.name == "Hero_Hornet(Clone)" || __instance.cachedTransform.gameObject.name == "Player_Clone(Clone)")
                {
                    Vector3 vector = (__instance.space == Space.World) ? __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.position : __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.localPosition;
                    __instance.vector.Value = vector;
                    __instance.x.Value = vector.x;
                    __instance.y.Value = vector.y;
                    __instance.z.Value = vector.z;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GetDistance), "DoGetDistance")]
    class Patch_GetDistance
    {
        static void Prefix(GetDistance __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(ChaseObjectGround), "DoChase")]
    class Patch_ChaseObjectGround
    {
        static void Prefix(ChaseObjectGround __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }


    [HarmonyPatch(typeof(ChaseObjectVertical), "DoChase")]
    class Patch_ChaseObjectVertical
    {
        static void Prefix(ChaseObjectVertical __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(ChaseObject), "DoBuzz")]
    class Patch_ChaseObject
    {
        static void Prefix(ChaseObject __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(ChaseObjectV2), "DoChase")]
    class Patch_ChaseObjectV2
    {
        static void Prefix(ChaseObjectV2 __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(ChaseObjectV3), "DoChase")]
    class Patch_ChaseObjectV3
    {
        static void Prefix(ChaseObjectV3 __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(CheckTargetDirection), "DoCheckDirection")]
    class Patch_CheckTargetDirection
    {
        static void Prefix(CheckTargetDirection __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(FaceObject), "DoFace")]
    class Patch_FaceObject
    {
        static void Prefix(FaceObject __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.objectB.Value.name == "Hero_Hornet(Clone)" || __instance.objectB.Value.name == "Player_Clone(Clone)")
                    __instance.objectB = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(FaceObjectV2), "DoFace")]
    class Patch_FaceObjectV2
    {
        static void Prefix(FaceObjectV2 __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.objectB.Value.name == "Hero_Hornet(Clone)" || __instance.objectB.Value.name == "Player_Clone(Clone)")
                    __instance.objectB = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(FaceObjectV3), "DoFace")]
    class Patch_FaceObjectV3
    {
        static void Prefix(FaceObjectV3 __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.ObjectB.Value.name == "Hero_Hornet(Clone)" || __instance.ObjectB.Value.name == "Player_Clone(Clone)")
                    __instance.ObjectB = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(FaceObjectV4), "DoFace")]
    class Patch_FaceObjectV4
    {
        static void Prefix(FaceObjectV4 __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if (__instance.ObjectB.Value.name == "Hero_Hornet(Clone)" || __instance.ObjectB.Value.name == "Player_Clone(Clone)")
                    __instance.ObjectB = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }


    [HarmonyPatch(typeof(GetXDistance), "DoGetDistance")]
    class Patch_GetXDistance
    {
        static void Prefix(GetXDistance __instance)
        {
            if (__instance.Fsm.GameObject.GetComponent<EnemyAvatar>() )
            {
                if(__instance.target.Value.name == "Hero_Hornet(Clone)" || __instance.target.Value.name == "Player_Clone(Clone)")
                    __instance.target = __instance.Fsm.GameObject.GetComponent<EnemyAvatar>().TargetPlayer.transform.gameObject;
            }
        }
    }
}
