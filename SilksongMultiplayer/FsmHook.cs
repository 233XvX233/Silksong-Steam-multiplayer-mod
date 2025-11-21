using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker;

namespace SilksongMultiplayer
{
    [HarmonyPatch(typeof(Fsm))]
    [HarmonyPatch("Event", new[] { typeof(FsmEventTarget), typeof(FsmEvent) })]
    public class FsmHook
    {
        private static readonly HashSet<string> LoggedEvents = new HashSet<string>();
        // Prefix 在原方法运行前调用
        static void Prefix(Fsm __instance, FsmEventTarget eventTarget, FsmEvent fsmEvent)
        {

        }

        // Postfix 在原方法运行后调用
        static void Postfix(FsmEventTarget eventTarget, FsmEvent fsmEvent)
        {
            // 可以在这里做后处理，比如记录哪些事件实际成功派发
        }
    }
}
