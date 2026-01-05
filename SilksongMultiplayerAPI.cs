using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using HutongGames.PlayMaker.Actions;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongMultiplayer
{
    static class SilksongMultiplayerAPI
    {
        public static bool startGame = false;
        public static bool enterRoom = false;
        public static bool roomOwner = false;
        public static bool ownTheScene = false;
        public static GameObject RoomManagerObject;
        public static RoomManager RoomManager;
        public static PlayerNetworkSync playerNetworkSync;
        public static EnemyHitEffectsProfile sampleEnemyHitEffectsProfile;
        public static Font savedFont;
        public static bool enablePvP;
        public static float BossHPmultiplier = 1f;
        public static float EnemyHPmultiplier = 0f;

        public static GameObject compassIcon;
        public static GameObject wideCompassIcon;
        public static GameObject Hero_Hornet;

        public static GameObject createLobbyButton;
        public static GameObject inviteLobbyButton;

        public static ManualLogSource Logger;

        public static ToolCrest Hunter_v3;
        public static ToolCrest Reaper;
        public static ToolCrest Wanderer;
        public static ToolCrest Warrior;
        public static ToolCrest Witch;
        public static ToolCrest Toolmaster;
        public static ToolCrest Spell;

        public static bool SyncState = false;
        public static bool AllPlayerKnockedDown = false;
        public static bool KnockedDown = false;
        public static bool Suicide = false;
        public static bool pushWaveByOuther = false;

        public static Text PlayerListText;
        public static Text DebugText;

        public static Dictionary<CSteamID, PlayerAvatar> remotePlayers = new Dictionary<CSteamID, PlayerAvatar>();
        public static List<Transform> remotePlayersTransformList = new List<Transform>();

        public static string currentScene = "";

        public static Dictionary<string, CSteamID> sceneOwnersList = new Dictionary<string, CSteamID>();
        public static Dictionary<CSteamID, string> playerSceneMap = new Dictionary<CSteamID, string>();
        public static Dictionary<string, SceneEnemyData> sceneEnemyData = new Dictionary<string, SceneEnemyData>();

        public static string currentOwnedScene = "";

        public static string skinName;
        public static string skinLink1;
        public static string skinLink2;
        public static string skinLink3;
        public static string skinLink4;

        public static bool debug = false;
        public static bool hideOuther = false;
        public static bool showComments = true;
        public static bool cheat = true;

        public static List<CSteamID> GetRoomMembers()
        {
            return RoomManager.GetRoomMembers();
        }

        public static HashSet<string> fsmObject = new HashSet<string>
        {
            "Mossbone Mother",
            "Bone Beast",
            "Lace Boss1",
            "song_golem",
            "Skull King",
            "Bone Flyer Giant",
            "Vampire Gnat",
            "Splinter Queen",
            "Spinner Boss",
            "Driller A",
            "Driller B",
        };

        public static HashSet<string> hpObject = new HashSet<string>
        {
            "Mossbone Mother",
            "Bone Beast",
            "Lace Boss1",
            "SG_head",
            "Skull King",
            "Bone Flyer Giant",
            "Vampire Gnat",
            "Splinter Queen",
            "Spinner Boss",
            "Driller A",
            "Driller B",
        };

        public static void SetDamageScalingToCustom(this HealthManager hm)
        {
            // 获取 private nested class 类型
            Type nestedType = typeof(HealthManager).GetNestedType("DamageScalingConfig", BindingFlags.NonPublic);
            if (nestedType == null)
            {
                Debug.LogError("找不到 DamageScalingConfig 类型");
                return;
            }

            // 创建一个实例（默认构造函数）
            object newConfig = Activator.CreateInstance(nestedType);
            if (newConfig == null)
            {
                Debug.LogError("无法创建 DamageScalingConfig 实例");
                return;
            }

            // 可以用反射修改 newConfig 内部字段，如果你知道字段名
            FieldInfo multField = nestedType.GetField("someMultiplierField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (multField != null)
            {
                multField.SetValue(newConfig, 2.0f); // 举例：把倍率改为 2
            }

            // 设置到 hm 实例的 private 字段
            FieldInfo damageScalingField = typeof(HealthManager).GetField("damageScaling", BindingFlags.NonPublic | BindingFlags.Instance);
            if (damageScalingField != null)
            {
                damageScalingField.SetValue(hm, newConfig);
                Debug.Log("damageScaling 已替换");
            }
        }

        public static void ReplaceItemDropGroups(HealthManager hm)
        {
            if (hm == null)
            {
                Debug.LogError("HealthManager 实例为空");
                return;
            }

            // 找到 ItemDropGroup 类型
            var itemDropGroupType = typeof(HealthManager).GetNestedType(
                "ItemDropGroup", BindingFlags.NonPublic);

            if (itemDropGroupType == null)
            {
                Debug.LogError("找不到 HealthManager.ItemDropGroup 类型");
                return;
            }

            // 构造一个新的 List<ItemDropGroup>
            var listType = typeof(List<>).MakeGenericType(itemDropGroupType);
            var newList = Activator.CreateInstance(listType);

            // 这里 newList 还是空的，你也可以通过反射往里面加元素
            // 比如 listType.GetMethod("Add") 调用来添加 ItemDropGroup 实例

            // 找到 itemDropGroups 字段
            var field = typeof(HealthManager).GetField("itemDropGroups",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogError("找不到 itemDropGroups 字段");
                return;
            }

            // 替换掉原有的掉落组
            field.SetValue(hm, newList);

            Debug.Log("成功替换 itemDropGroups 列表");
        }

        public static void CloneAnimatorOfObject(GameObject gameObject, GameObject cloneTarget)
        {
            if (cloneTarget.GetComponent<tk2dSpriteAnimator>() && cloneTarget.GetComponent<tk2dSprite>())
            {
                tk2dSprite spriteRenderer = gameObject.AddComponent<tk2dSprite>();
                tk2dSpriteAnimator animator = gameObject.AddComponent<tk2dSpriteAnimator>();
                animator.Library = cloneTarget.GetComponent<tk2dSpriteAnimator>().Library;
                animator.Play(cloneTarget.GetComponent<tk2dSpriteAnimator>().CurrentClip);

                //spriteRenderer.SetSprite(cloneTarget.GetComponent<tk2dSprite>().Collection, cloneTarget.GetComponent<tk2dSprite>().spriteId);
            }
        }

        public static void ResetPlayer()
        {
            SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
            SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = true;
            SilksongMultiplayerAPI.KnockedDown = false;
            SilksongMultiplayerAPI.AllPlayerKnockedDown = false;
        }

        public static void ChangeEnemyTarget(ulong targetID, string enemyName)
        {

            CSteamID memberID = new CSteamID(targetID);

            Debug.Log("切换boss目标为：" + memberID.m_SteamID);

            if (GameObject.Find(enemyName) == false || GameObject.Find(enemyName).GetComponent<EnemyAvatar>() == false)
                return;

            EnemyAvatar enemy = GameObject.Find(enemyName).GetComponent<EnemyAvatar>();

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(memberID, out PlayerAvatar playerAvatar))
            {
                Debug.Log("切换" + enemy.name + "目标为其他玩家");
                enemy.TargetPlayer = playerAvatar.gameObject;
            }
            else
            {
                Debug.Log("切换" + enemy.name + "目标为自身");
                enemy.TargetPlayer = SilksongMultiplayerAPI.Hero_Hornet;
            }
        }

        public static void ChangeCurrentOwnedScene(string sceneName)
        {
            currentOwnedScene = sceneName;

            if (currentScene == currentOwnedScene)
            {
                foreach (HealthManager enemyHealthManager in HealthManager.EnumerateActiveEnemies())
                {
                    if (enemyHealthManager.gameObject.GetComponent<EnemyAvatar>())
                    {
                        enemyHealthManager.gameObject.GetComponent<EnemyAvatar>().isOwner = true;
                    }
                }
            }
        }

        public static void OnChangeScene(string sceneName)
        {
            if (!roomOwner)
            {
                currentScene = sceneName;

                if (currentScene == currentOwnedScene)
                {
                    foreach (HealthManager hm in HealthManager.EnumerateActiveEnemies())
                    {
                        if (hm.gameObject.TryGetComponent<EnemyAvatar>(out var ea))
                            ea.isOwner = true;
                    }
                }

                return;
            }

            // ✅ 用 currentScene 作为旧场景来源，最可靠
            string oldScene = currentScene;
            currentScene = sceneName;

            // ✅ 先更新自己所在场景，避免旧场景判断把自己算进去
            playerSceneMap[SteamUser.GetSteamID()] = sceneName;

            // ===== 进入新场景：如果新场景没人，则我成为 owner =====
            if (!sceneOwnersList.TryGetValue(sceneName, out CSteamID ownerId))
            {
                ownerId = SteamUser.GetSteamID();
                sceneOwnersList[sceneName] = ownerId;

                ChangeCurrentOwnedScene(sceneName);
                NetworkDataSender.SendSceneOwner(ownerId.m_SteamID, sceneName);
            }
            else
            {
                NetworkDataSender.SendSceneOwner(ownerId.m_SteamID, sceneName);

                if (sceneEnemyData.TryGetValue(sceneName, out SceneEnemyData a))
                {
                    foreach (string diedEnemieName in a.diedEnemy)
                    {
                        var go = GameObject.Find(diedEnemieName);
                        if (go && go.TryGetComponent<EnemyAvatar>(out var av) && av.isOwner == false)
                        {
                            av.NoRespondCounter = -1;
                        }
                    }
                }
            }

            // ===== 离开旧场景：如果旧场景 owner 是我 且旧场景没人了 => 清理 =====
            if (!string.IsNullOrEmpty(oldScene) && oldScene != sceneName)
            {
                if (sceneOwnersList.TryGetValue(oldScene, out var oldOwner) && oldOwner == SteamUser.GetSteamID())
                {
                    bool anyoneLeftInOldScene = false;
                    foreach (var kv in playerSceneMap)
                    {
                        if (kv.Key == SteamUser.GetSteamID()) continue; // 我已经算在新场景
                        if (kv.Value == oldScene)
                        {
                            anyoneLeftInOldScene = true;
                            break;
                        }
                    }

                    if (!anyoneLeftInOldScene)
                    {
                        sceneEnemyData.Remove(oldScene);
                        sceneOwnersList.Remove(oldScene);

                        // ✅ 只在当前 ownedScene 还是旧场景时才清空，避免误伤新场景
                        if (currentOwnedScene == oldScene)
                            ChangeCurrentOwnedScene("");
                    }
                }
            }

            // 以下逻辑保持不变
            if (currentScene == currentOwnedScene)
            {
                foreach (HealthManager hm in HealthManager.EnumerateActiveEnemies())
                {
                    if (hm.gameObject.TryGetComponent<EnemyAvatar>(out var ea))
                        ea.isOwner = true;
                }
            }

            CrearEmptySceneOwner();
        }



        public static void OnOutherChangeScene(string sceneName, CSteamID steamID)
        {
            if (!roomOwner) return;

            // 更新玩家所在场景
            playerSceneMap[steamID] = sceneName;

            if (!sceneOwnersList.TryGetValue(sceneName, out CSteamID existing))//如果进入空场景
            {
                // 这个玩家以前拥有的场景
                string prev = GetSceneNameBySceneOwnersSteamID(steamID);

                if (prev != null && prev != sceneName)
                {
                    HandleOwnerLeavingScene(prev, steamID);
                }

                sceneOwnersList[sceneName] = steamID;
                NetworkDataSender.SendSceneOwner(steamID.m_SteamID, sceneName);
            }
            else//进入有人场景
            {
                if (GetSceneNameBySceneOwnersSteamID(steamID) != null && GetSceneNameBySceneOwnersSteamID(steamID) != sceneName)
                {
                    sceneEnemyData.Remove(GetSceneNameBySceneOwnersSteamID(steamID));
                    sceneOwnersList.Remove(GetSceneNameBySceneOwnersSteamID(steamID));
                }

                NetworkDataSender.SendSceneOwner(existing.m_SteamID, sceneName);

                if (sceneEnemyData.TryGetValue(sceneName, out SceneEnemyData a))
                {
                    foreach (string diedEnemieName in sceneEnemyData[sceneName].diedEnemy)
                    {
                        NetworkDataSender.SendEnemieDieData(diedEnemieName, sceneName);
                    }
                }
            }

            CrearEmptySceneOwner();
        }


        private static bool TryTransferSceneOwner(string sceneName, CSteamID leavingOwner)
        {
            if (!sceneOwnersList.TryGetValue(sceneName, out var currentOwner)) return false;
            if (currentOwner != leavingOwner) return false;

            foreach (var kv in playerSceneMap)
            {
                var playerId = kv.Key;
                var playerScene = kv.Value;

                if (playerId == leavingOwner) continue;
                if (playerScene != sceneName) continue;

                sceneOwnersList[sceneName] = playerId;
                NetworkDataSender.SendSceneOwner(playerId.m_SteamID, sceneName);
                return true;
            }

            return false; // 没人接管
        }

        private static int CountPlayersInScene(string sceneName, CSteamID? excluding = null)
        {
            int count = 0;
            foreach (var kv in playerSceneMap)
            {
                if (excluding.HasValue && kv.Key == excluding.Value) continue;
                if (kv.Value == sceneName) count++;
            }
            return count;
        }

        private static bool IsSceneEmpty(string sceneName, CSteamID? excluding = null)
        {
            return CountPlayersInScene(sceneName, excluding) == 0;
        }

        private static void HandleOwnerLeavingScene(string sceneName, CSteamID leavingOwner)
        {
            if (!sceneOwnersList.TryGetValue(sceneName, out var currentOwner)) return;
            if (currentOwner != leavingOwner) return;

            // 先尝试转交
            bool transferred = TryTransferSceneOwner(sceneName, leavingOwner);

            if (transferred) return;

            // 转交失败：如果场景已经空了，就清掉 owner
            // 注意：排除 leavingOwner（把他当作已离开）
            if (IsSceneEmpty(sceneName, excluding: leavingOwner))
            {
                sceneOwnersList.Remove(sceneName);
                sceneEnemyData.Remove(sceneName);
                // 可选：广播“无 owner”（取决于你协议是否支持）
                // NetworkDataSender.SendSceneOwner(0, sceneName);
            }
        }



        public static string GetSceneNameBySceneOwnersSteamID(CSteamID steamID)
        {
            foreach (var kvp in sceneOwnersList)
            {
                if (kvp.Value == steamID)
                {
                    return kvp.Key;
                }
            }
            return null; // 没找到
        }
    

        public static void CrearEmptySceneOwner()
        {
            foreach (var kvp in sceneOwnersList)
            {
                bool findPlayer = false;
                foreach (var player in playerSceneMap)
                {
                    if(player.Value == kvp.Key)
                    {
                        findPlayer = true;
                    }
                }

                if (!findPlayer)
                {
                    sceneOwnersList.Remove(kvp.Key);
                }
            }
        }
    }
}
