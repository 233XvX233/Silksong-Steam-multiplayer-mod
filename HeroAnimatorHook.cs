using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace SilksongMultiplayer
{
    [HarmonyPatch(typeof(tk2dSpriteAnimator))]
    [HarmonyPatch("Play", new System.Type[] { typeof(tk2dSpriteAnimationClip), typeof(float), typeof(float) })]
    static class HeroAnimatorHook
    {
        static void Prefix(tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps, tk2dSpriteAnimator __instance)
        {
            if(__instance.gameObject == SilksongMultiplayerAPI.Hero_Hornet)
            {
                int CrestID = -1;

                switch (PlayerData.instance.CurrentCrestID)
                {
                    case "Hunter_v3":
                        CrestID = 0;
                        break;
                    case "Reaper":
                        CrestID = 1;
                        break;
                    case "Wanderer":
                        CrestID = 2;
                        break;
                    case "Warrior":
                        CrestID = 3;
                        break;
                    case "Witch":
                        CrestID = 4;
                        break;
                    case "Toolmaster":
                        CrestID = 5;
                        break;
                    case "Spell":
                        CrestID = 6;
                        break;

                }

                if(CrestID != -1)
                {
                    ToolCrest toolCrest = null;
                    switch (CrestID)
                    {
                        case 0:
                            toolCrest = SilksongMultiplayerAPI.Hunter_v3;
                            break;
                        case 1:
                            toolCrest = SilksongMultiplayerAPI.Reaper;
                            break;
                        case 2:
                            toolCrest = SilksongMultiplayerAPI.Wanderer;
                            break;
                        case 3:
                            toolCrest = SilksongMultiplayerAPI.Warrior;
                            break;
                        case 4:
                            toolCrest = SilksongMultiplayerAPI.Witch;
                            break;
                        case 5:
                            toolCrest = SilksongMultiplayerAPI.Toolmaster;
                            break;
                        case 6:
                            toolCrest = SilksongMultiplayerAPI.Spell;
                            break;

                    }

                    if (toolCrest != null)
                    {
                        if(toolCrest.HeroConfig == null || toolCrest.HeroConfig != null && toolCrest.HeroConfig.GetAnimationClip(clip.name) == null)
                        {
                            CrestID = -1;
                        }
                    }
                    else
                    {
                        CrestID = -1;
                    }
                }

                NetworkDataSender.SendAnimationData(clip.name, CrestID);
            }

            if (__instance.transform.parent != null && __instance.transform.parent.parent != null && __instance.transform.parent.parent.parent != null && __instance.transform.parent.parent.parent.gameObject == SilksongMultiplayerAPI.Hero_Hornet && __instance.transform.parent.parent.gameObject.name == "Attacks")
                NetworkDataSender.SendHeroAttackAnimationData(__instance.transform.parent.name, __instance.name, clip.name);
        }
    }




    


}
