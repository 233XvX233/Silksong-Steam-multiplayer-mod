using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SilksongMultiplayer
{
    internal class PlayerNetworkSync : MonoBehaviour
    {
        public Steamworks.CSteamID currentRoomID;


        private float lastSendTime;
        private const float sendInterval = 0.03f; // 30ms 发送一次
        private string mapName;
        private float createColliderCounter = 0.3f;
        Canvas canva;
        GameObject nameText;
        GameObject cocoon;

        private float compassLastSendTime;
        private const float compassSendInterval = 1f; // 1000ms 发送一次

        private float mapNameSendCounter = 0f;

        public int cocoonHP = 25;

        public Canvas DebugCanvaComponent;
        public GameObject DebugText;

        void Start()
        {
            //GameManager.instance.playerData.hasBrolly = true;

            string savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", SilksongMultiplayerAPI.skinName, "Knight", "atlas0.png");
            NetworkDataReceiver.DownloadImageAsync(SilksongMultiplayerAPI.skinLink1, savePath);

            savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", SilksongMultiplayerAPI.skinName, "Knight", "atlas1.png");
            NetworkDataReceiver.DownloadImageAsync(SilksongMultiplayerAPI.skinLink2, savePath);

            savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", SilksongMultiplayerAPI.skinName, "Knight", "atlas2.png");
            NetworkDataReceiver.DownloadImageAsync(SilksongMultiplayerAPI.skinLink3, savePath);

            savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", SilksongMultiplayerAPI.skinName, "Knight", "atlas3.png");
            NetworkDataReceiver.DownloadImageAsync(SilksongMultiplayerAPI.skinLink4, savePath);


            this.gameObject.AddComponent<SkinLock>();
            Skin.ChangeSkinOnObject(this.gameObject,SilksongMultiplayerAPI.skinName);

            SilksongMultiplayerAPI.playerNetworkSync = this;

            GameObject nameCanva = new GameObject("nameCanva");
            nameCanva.transform.SetPositionAndRotation(this.transform.position, Quaternion.identity);
            nameCanva.transform.SetParent(this.transform);

            if (SilksongMultiplayerAPI.enterRoom)
            {
                if(SteamMatchmaking.GetNumLobbyMembers(SilksongMultiplayerAPI.RoomManager.currentRoomID) > 1)
                {
                    canva = nameCanva.AddComponent<Canvas>();
                    canva.renderMode = RenderMode.WorldSpace;
                    canva.sortingLayerName = "HUD";
                    canva.sortingLayerID = 629535577;
                    canva.sortingOrder = 50;


                    canva.renderMode = RenderMode.ScreenSpaceCamera;

                    RectTransform rect = nameCanva.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(2560, 1440);

                    nameText = new GameObject("nameText");
                    nameText.transform.SetParent(nameCanva.transform);

                    // 必须有 CanvasRenderer
                    nameText.AddComponent<CanvasRenderer>();
                    nameText.transform.localScale = Vector3.one * 0.01f;
                    Text text = nameText.AddComponent<Text>();
                    text.text = SteamFriends.GetPersonaName(); // 或 SteamFriends.GetPersonaName()
                    text.font = SilksongMultiplayerAPI.savedFont;
                    text.fontSize = 50;
                    text.alignment = TextAnchor.MiddleCenter;

                    ulong XvXSteamId64 = 76561198929282998UL;
                    ulong truthSteamId64 = 76561199835946204UL;

                    if (SteamUser.GetSteamID().m_SteamID == XvXSteamId64 || SteamUser.GetSteamID().m_SteamID == truthSteamId64)
                    {
                        text.color = Color.yellow;
                    }
                }


                if(SilksongMultiplayerAPI.debug)
                {
                    GameObject DebugCanva = new GameObject("DebugCanva");
                    DebugCanva.transform.SetPositionAndRotation(this.transform.position, Quaternion.identity);
                    DebugCanva.transform.SetParent(this.transform);

                    
                    DebugCanvaComponent = DebugCanva.AddComponent<Canvas>();
                    DebugCanvaComponent.renderMode = RenderMode.WorldSpace;
                    DebugCanvaComponent.sortingLayerName = "HUD";
                    DebugCanvaComponent.sortingLayerID = 629535577;
                    DebugCanvaComponent.sortingOrder = 50;


                    DebugCanvaComponent.renderMode = RenderMode.ScreenSpaceCamera;

                    RectTransform rect = DebugCanva.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(256000, 144000);

                    DebugText = new GameObject("DebugText");
                    DebugText.transform.SetParent(DebugCanva.transform);

                    // 必须有 CanvasRenderer
                    DebugText.AddComponent<CanvasRenderer>();
                    DebugText.transform.localScale = Vector3.one * 0.01f;
                    Text text = DebugText.AddComponent<Text>();
                    text.text = SteamFriends.GetPersonaName(); // 或 SteamFriends.GetPersonaName()
                    text.font = SilksongMultiplayerAPI.savedFont;
                    text.fontSize = 30;
                    text.alignment = TextAnchor.MiddleCenter;

                    SilksongMultiplayerAPI.DebugText = text;
                }

                cocoon = new GameObject("cocoon");
                cocoon.transform.SetParent(this.transform);

                SpriteRenderer spriteRenderer = cocoon.AddComponent<SpriteRenderer>();

                string folder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "BepInEx", "plugins", "XvX");
                string imagePath = Path.Combine(folder, "Hornet_death.png");

                byte[] fileData = File.ReadAllBytes(imagePath);

                Texture2D tex = new Texture2D(4, 4);
                tex.LoadImage(fileData); // 现在可以识别了

                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, 105, 216),
                    new Vector2(0.5f, 0.5f)
                );

                spriteRenderer.sprite = sprite;

                cocoon.transform.localPosition = new Vector3(0, 1000, 0);
                cocoon.transform.localScale = new Vector3(2, 2, 2);
            }
        }
        tk2dSpriteCollectionData savedtk2dSpriteCollectionData = new tk2dSpriteCollectionData();
        void Update()
        {
            if(SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox"))
                SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").transform.localPosition = Vector3.zero;




            if (SilksongMultiplayerAPI.DebugText != null)
            {
                string text = "玩家当前场景" + SilksongMultiplayerAPI.currentScene;

                foreach(KeyValuePair<CSteamID,string> pair in SilksongMultiplayerAPI.playerSceneMap)
                {
                    text += Environment.NewLine + SteamFriends.GetFriendPersonaName(pair.Key) + " = " + pair.Value;
                }

                text += Environment.NewLine + "场景所有权" + SilksongMultiplayerAPI.currentOwnedScene;

                foreach (KeyValuePair<string, CSteamID> pair in SilksongMultiplayerAPI.sceneOwnersList)
                {
                    text += Environment.NewLine + SteamFriends.GetFriendPersonaName(pair.Value) + " = " + pair.Key;
                }

                text += Environment.NewLine + "已死亡小怪";

                foreach (KeyValuePair<string, SceneEnemyData> pair in SilksongMultiplayerAPI.sceneEnemyData)
                {
                    foreach(string enemie in pair.Value.diedEnemy)
                        text += Environment.NewLine + pair.Key + " " +  enemie;
                }

                text += Environment.NewLine + "连战房数据";
                foreach (KeyValuePair<string, SceneEnemyData> pair in SilksongMultiplayerAPI.sceneEnemyData)
                {
                    foreach (KeyValuePair<string, int> enemie in pair.Value.battleSceneData)
                        text += Environment.NewLine + pair.Key + " " + enemie.Key + " " + enemie.Value;
                }



                SilksongMultiplayerAPI.DebugText.text = text;

                DebugCanvaComponent.transform.localPosition = Vector3.zero + new Vector3(0, 5f, 0);
                DebugText.transform.localPosition = Vector3.zero;
                DebugText.GetComponent<RectTransform>().sizeDelta = new Vector2(2560, 1440);

                if (this.transform.localScale.x < 0)
                    DebugCanvaComponent.transform.localScale = new Vector2(-1, 1);
                else
                    DebugCanvaComponent.transform.localScale = new Vector2(1, 1);
            }





            if (this.GetComponent<tk2dSprite>().Collection != savedtk2dSpriteCollectionData)
            {
                Skin.ChangeSkinOnObject(this.gameObject, SilksongMultiplayerAPI.skinName);
                savedtk2dSpriteCollectionData = this.GetComponent<tk2dSprite>().Collection;
            }

            if (cocoon != null)
            {
                if(SilksongMultiplayerAPI.KnockedDown)
                {
                    cocoon.transform.localPosition = new Vector3(0, 0, -0.004f);
                }
                else
                {
                    cocoon.transform.localPosition = new Vector3(0, 1000, 0);

                }
            }

            if(SilksongMultiplayerAPI.AllPlayerKnockedDown)
            {
                HeroController.instance.TakeDamage(base.gameObject, CollisionSide.other, 999, HazardType.ENEMY);
            }


            if (SilksongMultiplayerAPI.compassIcon == null && DDOLFinder.FindInDDOLByName("Game_Map_Hornet(Clone)", exact: true).Count > 0)
                SilksongMultiplayerAPI.compassIcon = DDOLFinder.FindChildByName(DDOLFinder.FindInDDOLByName("Game_Map_Hornet(Clone)", exact: true)[0], "Compass Icon");

            if (SilksongMultiplayerAPI.wideCompassIcon == null && DDOLFinder.FindInDDOLByName("Wide Map(Clone)", exact: true).Count > 0)
                SilksongMultiplayerAPI.wideCompassIcon = DDOLFinder.FindChildByName(DDOLFinder.FindInDDOLByName("Wide Map(Clone)", exact: true)[0], "Compass Icon");

            // 定时发送
            if (Time.time - lastSendTime >= sendInterval)
            {
                SendPositionToAll();
                lastSendTime = Time.time;
            }

            // 定时发送地图位置
            if (Time.time - compassLastSendTime >= compassSendInterval)
            {
                SendMapPositionToAll();
                compassLastSendTime = Time.time;

                NetworkDataSender.SendSkinData(SilksongMultiplayerAPI.skinName, SilksongMultiplayerAPI.skinLink1,
                    SilksongMultiplayerAPI.skinLink2, SilksongMultiplayerAPI.skinLink3, SilksongMultiplayerAPI.skinLink4);
            }



            mapNameSendCounter -= Time.deltaTime;
            if (mapNameSendCounter <= 0 || SceneManager.GetActiveScene().name != mapName)
            {
                if(SceneManager.GetActiveScene().name != mapName && SilksongMultiplayerAPI.showComments)
                {
                    List<FeedbackEntry> entries = SilksongMultiplayerAPI.RoomManager.githubTextReader.entries;


                    List<FeedbackEntry> list = entries.FindAll(e => e.sceneName == SceneManager.GetActiveScene().name);

                    foreach (FeedbackEntry entry in list)
                    {
                        float f = UnityEngine.Random.value;
                        if (f < 15f / list.Count)
                        {
                            GameObject comment = GameObject.Instantiate(new GameObject(), new Vector3(entry.x, entry.y, 0), Quaternion.identity);
                            PlayerComment playerComment = comment.AddComponent<PlayerComment>();
                            playerComment.Init(entry.message, new Vector3(entry.x, entry.y));
                        }
                    }
                }

                mapName = SceneManager.GetActiveScene().name;

                //SilksongMultiplayerAPI.currentScene = mapName;

                NetworkDataSender.SendMapChangeNotification(mapName);

                SilksongMultiplayerAPI.OnChangeScene(mapName);

                mapNameSendCounter = 3;
            }

            if (SilksongMultiplayerAPI.enterRoom)
            {
                /*
                bool findBoss = false;
                foreach(string bossName in SilksongMultiplayerAPI.fsmObject)
                {
                    if(GameObject.Find(bossName))
                    {
                        GameObject bossObject = GameObject.Find(bossName);
                        findBoss = true;

                        PlayMakerFSM fsmComp = bossObject.GetComponent<PlayMakerFSM>();

                        // 访问底层 FSM
                        Fsm fsm = fsmComp.Fsm;

                        // 遍历它的 Events 数组
                        foreach (FsmEvent ev in fsm.Events)
                        {
                            Debug.Log($"[FSM EVENT] {ev.Name}");
                        }

                        foreach (var state in fsm.States)
                            Debug.Log($"FSM状态: {state.Name}");
                    }
                }
                */

                foreach (HealthManager enemyHealthManager in HealthManager.EnumerateActiveEnemies())
                {
                    if (enemyHealthManager.gameObject.GetComponent<Dummy>() == false)
                    {
                        Dummy dummy = enemyHealthManager.gameObject.AddComponent<Dummy>();
                        dummy.boss = true;
                        dummy.noExtraComponent = true;
                        dummy.bossName = enemyHealthManager.gameObject.name;
                        dummy.damageMultiplier = 1f;
                        dummy.Init();

                        enemyHealthManager.gameObject.AddComponent<EnemyAvatar>();
                    }
                }
            }

            if (SilksongMultiplayerAPI.enterRoom && canva != null)
            {
                canva.transform.localPosition = Vector3.zero + new Vector3(0, 2.5f, 0);
                nameText.transform.localPosition = Vector3.zero;
                nameText.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);

                if (this.transform.localScale.x < 0)
                    canva.transform.localScale = new Vector2(-1, 1);
                else
                    canva.transform.localScale = new Vector2(1, 1);
            }

            if (createColliderCounter < 0 && createColliderCounter > -100)
            {
                if (SilksongMultiplayerAPI.enterRoom && canva != null)
                {
                    canva.renderMode = RenderMode.WorldSpace;
                    canva.transform.localPosition = Vector3.zero;
                    nameText.transform.localPosition = Vector3.zero;
                }

                if (SilksongMultiplayerAPI.enterRoom && DebugCanvaComponent != null)
                {
                    DebugCanvaComponent.renderMode = RenderMode.WorldSpace;
                    DebugCanvaComponent.transform.localPosition = Vector3.zero;
                    DebugText.transform.localPosition = Vector3.zero;
                }

                createColliderCounter = -100;
            }
            else
            {
                createColliderCounter -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.F2) && SilksongMultiplayerAPI.roomOwner)
            {
                foreach(BattleScene battleScene in GameObject.FindObjectsByType<BattleScene>(FindObjectsSortMode.None))
                {
                    battleScene.WaveEnd();
                }
            }

            if (Input.GetKeyDown(KeyCode.F3) && SilksongMultiplayerAPI.cheat)
            {
                NetworkDataSender.SendTeleportData(SceneManager.GetActiveScene().name, GameManager.instance.entryGateName, SilksongMultiplayerAPI.Hero_Hornet.transform.position);
            }
            
            if (Input.GetKeyDown(KeyCode.F5) && SilksongMultiplayerAPI.KnockedDown == false)
            {
                HeroController.instance.RegainControl();
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                SilksongMultiplayerAPI.hideOuther = !SilksongMultiplayerAPI.hideOuther;
            }

            if (Input.GetKeyDown(KeyCode.F8) && SilksongMultiplayerAPI.KnockedDown == true)
            {
                SilksongMultiplayerAPI.playerNetworkSync.cocoonHP = 25;

                SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
                SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = true;
                SilksongMultiplayerAPI.KnockedDown = false;

                SilksongMultiplayerAPI.Suicide = true;

                HeroController.instance.TakeDamage(new GameObject(), CollisionSide.other, 100, HazardType.ENEMY);
                
                SilksongMultiplayerAPI.Suicide = false;

                NetworkDataSender.PlayerKnockDown(false);
            }
        }

        public static GameObject FindInactiveChildByName(GameObject parent, string childName)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            // 注意这里传 true，表示包含 inactive 的对象

            foreach (Transform child in children)
            {
                if (child.name == childName)
                {
                    return child.gameObject;
                }
            }
            return null; // 没找到
        }

        public GameObject[] FindInActiveObjectsByName(string name)
        {
            var list = new List<GameObject>();
            var all = Resources.FindObjectsOfTypeAll<GameObject>(); // 直接找GameObject更直接

            foreach (var go in all)
            {
                // 排除资产（Project 里的Prefab/资源），只要场景对象
                if (!go || !go.scene.IsValid()) continue;

                // 只按名字匹配；不要限制 hideFlags
                if (go.name == name)
                    list.Add(go);
            }
            return list.ToArray();
        }

        public void SendMapPositionToAll()
        {
            if (SilksongMultiplayerAPI.compassIcon == null || SilksongMultiplayerAPI.wideCompassIcon == null)
                return;

            Vector2 compass = SilksongMultiplayerAPI.compassIcon.transform.localPosition;
            Vector2 wideCompass = SilksongMultiplayerAPI.wideCompassIcon.transform.localPosition;

            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.MapPosition),
                PacketSerializer.SerializeFloat(compass.x),
                PacketSerializer.SerializeFloat(compass.y),
                PacketSerializer.SerializeFloat(wideCompass.x),
                PacketSerializer.SerializeFloat(wideCompass.y)
            );

            NetworkDataSender.Broadcast(data, EP2PSend.k_EP2PSendUnreliable); // 不可靠传输
        }

        private void SendPositionToAll()
        {
            Vector3 currentPosition = transform.position;

            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.PlayerPosition),
                PacketSerializer.SerializeFloat(currentPosition.x),
                PacketSerializer.SerializeFloat(currentPosition.y),
                PacketSerializer.SerializeFloat(currentPosition.z),
                PacketSerializer.SerializeFloat(transform.localScale.x)
            );

            NetworkDataSender.Broadcast(data, EP2PSend.k_EP2PSendUnreliable); // 不可靠传输
        }

        [HarmonyPatch(typeof(PlayerData))] // 改成定义 TakeDamage 的类
        [HarmonyPatch("TakeHealth")]
        public static class Patch_TakeHealth
        {
            // Prefix 返回 bool，返回 false 就会阻止原函数
            static bool Prefix(int amount, bool hasBlueHealth, bool allowFracturedMaskBreak)
            {
                if(SteamMatchmaking.GetNumLobbyMembers(SilksongMultiplayerAPI.RoomManager.currentRoomID) <= 1)
                    return true;


                // 返回 false = 阻止原始 TakeDamage 执行
                if (SilksongMultiplayerAPI.AllPlayerKnockedDown || SilksongMultiplayerAPI.Suicide)
                {
                    SilksongMultiplayerAPI.AllPlayerKnockedDown = false;
                    SilksongMultiplayerAPI.Suicide = false;
                    NetworkDataSender.PlayerKnockDown(false);
                    return true;
                }
                else if(HeroController.instance.playerData.health - amount <= 0)
                {
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = false;
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = false;
                    SilksongMultiplayerAPI.KnockedDown = true;
                    HeroController.instance.playerData.health = amount + 1;

                    NetworkDataSender.PlayerKnockDown(true);

                    if(SilksongMultiplayerAPI.roomOwner)
                    {
                        bool allKnockedDown = true;
                        foreach (KeyValuePair<CSteamID, PlayerAvatar> kvp in SilksongMultiplayerAPI.remotePlayers)
                        {
                            if (kvp.Value.KnockedDown == false)
                                allKnockedDown = false;
                        }

                        if (SilksongMultiplayerAPI.KnockedDown == false)
                            allKnockedDown = false;

                        if (allKnockedDown)
                        {
                            SilksongMultiplayerAPI.AllPlayerKnockedDown = true;
                            NetworkDataSender.AllKnockDown();

                            SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
                            SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                            SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = true;
                            SilksongMultiplayerAPI.KnockedDown = false;

                            HeroController.instance.TakeDamage(new GameObject(), CollisionSide.other, 1, HazardType.ENEMY);

                            foreach (KeyValuePair<CSteamID, PlayerAvatar> kvp in SilksongMultiplayerAPI.remotePlayers)
                            {
                                kvp.Value.KnockedDown = false;
                                kvp.Value.cocoon.transform.localPosition = new Vector3(0, 1000, 0);
                            }

                            SilksongMultiplayerAPI.AllPlayerKnockedDown = false;
                            SilksongMultiplayerAPI.Suicide = false;
                        }
                    }
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeQuickDamage), new[] { typeof(int), typeof(bool), typeof(bool) })]
        public static class HeroController_TakeQuickDamage_Patch
        {
            static bool Prefix(HeroController __instance)
            {
                return true;



                if (SilksongMultiplayerAPI.Hero_Hornet.gameObject.GetComponent<tk2dSpriteAnimator>().CurrentClip.name != "Wound")
                {
                    UnityEngine.Debug.Log("拦截TakeQuickDamage");
                    return false;       // 跳过原方法
                }
                return true;
            }
        }
    }
}
