using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GlobalEnums;
using HutongGames.PlayMaker;
using Steamworks;
using UnityEngine;

namespace SilksongMultiplayer
{
    internal class NetworkDataReceiver : MonoBehaviour
    {

        void Update()
        {
            while (SteamNetworking.IsP2PPacketAvailable(out uint packetSize))
            {
                byte[] data = new byte[packetSize];
                if (SteamNetworking.ReadP2PPacket(data, packetSize, out uint bytesRead, out CSteamID senderID))
                {
                    ProcessPacket(data, senderID);
                }
            }
        }

        private void ProcessPacket(byte[] data, CSteamID senderID)
        {
            if (!SilksongMultiplayerAPI.startGame)
                return;

            int offset = 0;
            NetworkMessageType messageType = (NetworkMessageType)PacketDeserializer.ReadByte(data, ref offset);

            switch (messageType)
            {
                case NetworkMessageType.PlayerPosition:
                    HandlePositionMessage(data, senderID, ref offset);
                    break;
                case NetworkMessageType.PlayerAnimation:
                    HandleAnimationMessage(data, senderID, ref offset);
                    break;
                case NetworkMessageType.MapChange:
                    HandleMapChange(data, senderID, ref offset);
                    break;
                case NetworkMessageType.PlayerTakeDamage:
                    HandlePlayerTakeDamageMessage(data, senderID, ref offset);
                    break;
                case NetworkMessageType.MapPosition:
                    HandleMapPositionMessage(data, senderID, ref offset);
                    break;
                case NetworkMessageType.HeroAttackAnimation:
                    HandleHeroAttackAnimation(data, senderID, ref offset);
                    break;
                case NetworkMessageType.EnemyFsmState:
                    HandleEnemyFsmState(data, senderID, ref offset);
                    break;
                case NetworkMessageType.EnemyTakeDamage:
                    HandleEnemyTakeDamage(data, senderID, ref offset);
                    break;
                case NetworkMessageType.PlayerKnockDown:
                    HandlePlayerKnockDown(data, senderID, ref offset);
                break;
                case NetworkMessageType.CocoonTakeDamage:
                    HandleCocoonTakeDamage(data, senderID, ref offset);
                    break;
                case NetworkMessageType.EnemyPosition:
                    HandleEnemyPosition(data, senderID, ref offset);
                    break;
                case NetworkMessageType.EnemyTarget:
                    HandleEnemyTarget(data, senderID, ref offset);
                    break;
                case NetworkMessageType.EnemyHp:
                    HandleEnemyHp(data, senderID, ref offset);
                    break;
                case NetworkMessageType.SceneOwner:
                    HandleSceneOwner(data, senderID, ref offset);
                break;
                case NetworkMessageType.AllKnockDown:
                    HandleAllKnockDown(data, senderID, ref offset);
                    break;
                case NetworkMessageType.Skin:
                    HandleSkinData(data, senderID, ref offset);
                    break;

            }
        }

        private void HandlePositionMessage(byte[] data, CSteamID senderID, ref int offset)
        {
            float x = PacketDeserializer.ReadFloat(data, ref offset);
            float y = PacketDeserializer.ReadFloat(data, ref offset);
            float z = PacketDeserializer.ReadFloat(data, ref offset);
            float facing = PacketDeserializer.ReadFloat(data, ref offset);

            Vector3 newPosition = new Vector3(x, y, z);

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                player.UpdatePosition(newPosition, facing);
            }
            else if (SilksongMultiplayerAPI.startGame)
            {
                CreateNewPlayer(senderID, newPosition);
            }
        }

        private void HandleAnimationMessage(byte[] data, CSteamID senderID, ref int offset)
        {
            string animationName = PacketDeserializer.ReadString(data, ref offset);
            int extraValue = PacketDeserializer.ReadInt(data, ref offset);

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                //Debug.Log("动作ID：" + animationName);

                ToolCrest toolCrest = null;
                if (extraValue == -1)
                {
                    player.GetComponent<tk2dSpriteAnimator>().Play(animationName);
                }
                else
                {
                    toolCrest = extraValue switch
                    {
                        0 => SilksongMultiplayerAPI.Hunter_v3,
                        1 => SilksongMultiplayerAPI.Reaper,
                        2 => SilksongMultiplayerAPI.Wanderer,
                        3 => SilksongMultiplayerAPI.Warrior,
                        4 => SilksongMultiplayerAPI.Witch,
                        5 => SilksongMultiplayerAPI.Toolmaster,
                        6 => SilksongMultiplayerAPI.Spell,
                        _ => null
                    };

                    if (toolCrest != null)
                        player.GetComponent<tk2dSpriteAnimator>().Play(toolCrest.HeroConfig.GetAnimationClip(animationName));
                }
            }
        }

        private void HandleMapChange(byte[] data, CSteamID senderID, ref int offset)
        {
            string mapName = PacketDeserializer.ReadString(data, ref offset);
            Debug.Log($"收到地图切换: {mapName} (来自 {senderID})");

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                player.UpdateMap(mapName);
            }
        }

        private void HandlePlayerTakeDamageMessage(byte[] data, CSteamID senderID, ref int offset)
        {
            ulong targetSteamId = PacketDeserializer.ReadULong(data, ref offset);
            int damage = PacketDeserializer.ReadInt(data, ref offset);
            int direction = PacketDeserializer.ReadInt(data, ref offset);
            int hazardType = PacketDeserializer.ReadInt(data, ref offset);
            int attackTypes = PacketDeserializer.ReadInt(data, ref offset);

            if (SilksongMultiplayerAPI.enablePvP)
            {
                Debug.Log($"{targetSteamId} 和 {SteamUser.GetSteamID().m_SteamID}");
                if (targetSteamId == SteamUser.GetSteamID().m_SteamID)
                    HeroController.instance.TakeDamage(gameObject, (CollisionSide)direction, damage, (HazardType)hazardType);
            }

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                player.HitEffect((CollisionSide)direction, damage, (AttackTypes)attackTypes);
            }
        }

        private void HandleMapPositionMessage(byte[] data, CSteamID senderID, ref int offset)
        {
            float velX = PacketDeserializer.ReadFloat(data, ref offset);
            float velY = PacketDeserializer.ReadFloat(data, ref offset);
            float inputX = PacketDeserializer.ReadFloat(data, ref offset);
            float inputY = PacketDeserializer.ReadFloat(data, ref offset);

            Vector2 compass = new Vector2(velX, velY);
            Vector2 wideCompass = new Vector2(inputX, inputY);

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                player.UpdateCompassPosition(compass, wideCompass);
            }
        }

        private void HandleHeroAttackAnimation(byte[] data, CSteamID senderID, ref int offset)
        {
            string parentName = PacketDeserializer.ReadString(data, ref offset);
            string name = PacketDeserializer.ReadString(data, ref offset);
            string animationName = PacketDeserializer.ReadString(data, ref offset);

            if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                var attackRoot = player.transform.Find("Attacks(Clone)");
                if (attackRoot && attackRoot.Find(parentName + "(Clone)") && attackRoot.Find(parentName + "(Clone)").Find(name + "(Clone)"))
                {
                    var atkAnimator = attackRoot.Find(parentName + "(Clone)").Find(name + "(Clone)").GetComponent<tk2dSpriteAnimator>();
                    Debug.Log("获取动画：" + animationName);
                    atkAnimator.gameObject.GetComponent<AttackAnimTimeCounter>().SetRemainDuration(
                        atkAnimator.Library.GetClipByName(animationName).Duration);
                    atkAnimator.Play(animationName.TrimStart());
                }
            }
        }

        private void HandleEnemyFsmState(byte[] data, CSteamID senderID, ref int offset)
        {
            string bossName = PacketDeserializer.ReadString(data, ref offset);
            string stateName = PacketDeserializer.ReadString(data, ref offset);

            string sceneName = PacketDeserializer.ReadString(data, ref offset);
            if (SilksongMultiplayerAPI.currentScene != sceneName)
                return;

            Debug.Log($"收到Boss状态: Boss={bossName}, Event={stateName}, 来自={senderID}");



            var go = GameObject.Find(bossName);
            if (!go)
            {
                Debug.LogWarning($"找不到Boss对象: {bossName}");
                return;
            }

            if (go.GetComponent<EnemyAvatar>() && go.GetComponent<EnemyAvatar>().isOwner == false)
            {
                Debug.Log("已找到boss");
                PlayMakerFSM bossFsm = go.GetComponent<PlayMakerFSM>();
                SilksongMultiplayerAPI.SyncState = true;
                bossFsm.Fsm.SetState(stateName);
                SilksongMultiplayerAPI.SyncState = false;
            }
        }

        private void HandleEnemyTakeDamage(byte[] data, CSteamID senderID, ref int offset)
        {
            /*
             *  PacketSerializer.SerializeString(enemyName),
                PacketSerializer.SerializeInt(hitInstance.DamageDealt),
                PacketSerializer.SerializeFloat(hitInstance.Direction),
                PacketSerializer.SerializeInt((int)hitInstance.AttackType),
                PacketSerializer.SerializeFloat(hitInstance.Multiplier),
                PacketSerializer.SerializeFloat(hitInstance.MagnitudeMultiplier),
                PacketSerializer.SerializeInt((int)hitInstance.NailElement),
                PacketSerializer.SerializeBool(hitInstance.NonLethal),
                PacketSerializer.SerializeBool(hitInstance.CriticalHit),
                PacketSerializer.SerializeBool(hitInstance.CanWeakHit),
                PacketSerializer.SerializeInt(hitInstance.DamageScalingLevel),
                PacketSerializer.SerializeInt((int)hitInstance.SpecialType),
                PacketSerializer.SerializeString(sceneName)
            */

            string enemyName = PacketDeserializer.ReadString(data, ref offset);
            int DamageDealt = PacketDeserializer.ReadInt(data, ref offset);
            float Direction = PacketDeserializer.ReadFloat(data, ref offset);
            int AttackType = PacketDeserializer.ReadInt(data, ref offset);
            float Multiplier = PacketDeserializer.ReadFloat(data, ref offset);
            float MagnitudeMultiplier = PacketDeserializer.ReadFloat(data, ref offset);
            int NailElement = PacketDeserializer.ReadInt(data, ref offset);
            bool NonLethal = PacketDeserializer.ReadBool(data, ref offset);
            bool CriticalHit = PacketDeserializer.ReadBool(data, ref offset);
            bool CanWeakHit = PacketDeserializer.ReadBool(data,ref offset);
            int DamageScalingLevel = PacketDeserializer.ReadInt(data,ref offset);
            int SpecialType = PacketDeserializer.ReadInt(data,ref offset);

            string sceneName = PacketDeserializer.ReadString(data, ref offset);
            if (SilksongMultiplayerAPI.currentScene != sceneName)
                return;

            if(SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar playerAvatar))
            {
                HitInstance hitInstance = new HitInstance
                {
                    Source = playerAvatar.gameObject,
                    DamageDealt = DamageDealt,
                    Direction = Direction,
                    AttackType = (AttackTypes)AttackType,
                    Multiplier = Multiplier,
                    MagnitudeMultiplier = MagnitudeMultiplier,
                    NailElement = (NailElements)NailElement,
                    NonLethal = NonLethal,
                    CriticalHit = CriticalHit,
                    CanWeakHit = CanWeakHit,
                    DamageScalingLevel = DamageScalingLevel,
                    SpecialType = (SpecialTypes)SpecialType,
                    IsHeroDamage = true,
                    SilkGeneration = HitSilkGeneration.None
                };

                if (GameObject.Find(enemyName))
                {

                    GameObject.Find(enemyName).GetComponent<Dummy>().DoNotSend = true;
                    GameObject.Find(enemyName).SendMessage("Hit", hitInstance, SendMessageOptions.DontRequireReceiver);

                    if(GameObject.Find(enemyName).GetComponent<EnemyAvatar>().isOwner)
                    {
                        NetworkDataSender.SendTargetEnemyHpData(enemyName, GameObject.Find(enemyName).GetComponent<HealthManager>().hp, SilksongMultiplayerAPI.currentScene);
                    }

                }
            }
        }

        private void HandleEnemyHp(byte[] data, CSteamID senderID, ref int offset)
        {
            string enemyName = PacketDeserializer.ReadString(data, ref offset);
            int hp = PacketDeserializer.ReadInt(data, ref offset);

            string sceneName = PacketDeserializer.ReadString(data, ref offset);
            if (SilksongMultiplayerAPI.currentScene != sceneName)
                return;


            if (GameObject.Find(enemyName) && GameObject.Find(enemyName).GetComponent<EnemyAvatar>().isOwner == false)
            {
                GameObject.Find(enemyName).GetComponent<HealthManager>().hp = hp;
            }
        }

        public static void CallHitResponders(GameObject target, HitInstance hitInstance, Component exclude = null)
        {
            foreach (var responder in target.GetComponents<IHitResponder>())
            {
                var comp = responder as Component;
                if (comp == null || comp == exclude)
                    continue;

                responder.Hit(hitInstance);
            }
        }


        private void CreateNewPlayer(CSteamID steamID, Vector3 position)
        {
            GameObject playerObj = Instantiate(new GameObject("Player_Clone"), position, Quaternion.identity);
            playerObj.AddComponent<PlayerAvatar>();
            GameObject.DontDestroyOnLoad(playerObj);

            tk2dSprite spriteRenderer = playerObj.AddComponent<tk2dSprite>();
            tk2dSpriteAnimator animator = playerObj.AddComponent<tk2dSpriteAnimator>();
            animator.Library = SilksongMultiplayerAPI.Hero_Hornet.GetComponent<tk2dSpriteAnimator>().Library;
            animator.Play(SilksongMultiplayerAPI.Hero_Hornet.GetComponent<tk2dSpriteAnimator>().CurrentClip);

            spriteRenderer.SetSprite(
                SilksongMultiplayerAPI.Hero_Hornet.GetComponent<tk2dSprite>().Collection,
                SilksongMultiplayerAPI.Hero_Hornet.GetComponent<tk2dSprite>().spriteId);

            PlayerAvatar avatar = playerObj.GetComponent<PlayerAvatar>();
            avatar.Initialize(steamID);
            SilksongMultiplayerAPI.remotePlayers.Add(steamID, avatar);
            SilksongMultiplayerAPI.remotePlayersTransformList.Add(playerObj.transform);
        }

        private void HandlePlayerKnockDown(byte[] data, CSteamID senderID, ref int offset)
        {
            bool isKnockedDown = PacketDeserializer.ReadBool(data, ref offset);

            if(SilksongMultiplayerAPI.remotePlayers.TryGetValue(senderID, out PlayerAvatar player))
            {
                player.KnockedDown = isKnockedDown;
                if(isKnockedDown)
                {
                    player.cocoon.transform.localPosition = new Vector3(0, 0, -0.004f);
                }
                else
                {
                    player.cocoon.transform.localPosition = new Vector3(0,1000,0);
                }
            }

            if(SilksongMultiplayerAPI.roomOwner && isKnockedDown)
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
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = true;
                    SilksongMultiplayerAPI.KnockedDown = false;

                    NetworkDataSender.AllKnockDown();

                    HeroController.instance.TakeDamage(base.gameObject, CollisionSide.other, 999, HazardType.ENEMY);

                    foreach (KeyValuePair<CSteamID, PlayerAvatar> kvp in SilksongMultiplayerAPI.remotePlayers)
                    {
                        kvp.Value.KnockedDown = false;
                        kvp.Value.cocoon.transform.localPosition = new Vector3(0, 1000, 0);
                    }

                    SilksongMultiplayerAPI.AllPlayerKnockedDown = false;
                }
            }
        }

        private void HandleCocoonTakeDamage(byte[] data, CSteamID senderID, ref int offset)
        {
            ulong targetSteamId = PacketDeserializer.ReadULong(data, ref offset);
            int damage = PacketDeserializer.ReadInt(data, ref offset);
            int direction = PacketDeserializer.ReadInt(data, ref offset);
            int hazardType = PacketDeserializer.ReadInt(data, ref offset);
            int attackTypes = PacketDeserializer.ReadInt(data, ref offset);

            if (targetSteamId == SteamUser.GetSteamID().m_SteamID)
            {
                SilksongMultiplayerAPI.playerNetworkSync.cocoonHP -= damage;

                if(SilksongMultiplayerAPI.playerNetworkSync.cocoonHP <= 0)
                {
                    SilksongMultiplayerAPI.playerNetworkSync.cocoonHP = 25;

                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = true;

                    SilksongMultiplayerAPI.KnockedDown = false;
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
                    NetworkDataSender.PlayerKnockDown(false);
                }
            }
                
        }

        private void HandleEnemyPosition(byte[] data, CSteamID senderID, ref int offset)
        {
            string enemyName = PacketDeserializer.ReadString(data, ref offset);
            Vector2 position = Vector2.zero;
            position.x = PacketDeserializer.ReadFloat(data, ref offset);
            position.y = PacketDeserializer.ReadFloat(data, ref offset);
            int direction = PacketDeserializer.ReadInt(data, ref offset);

            string sceneName = PacketDeserializer.ReadString(data, ref offset);
            if (SilksongMultiplayerAPI.currentScene != sceneName)
                return;

            if (GameObject.Find(enemyName) && GameObject.Find(enemyName).GetComponent<EnemyAvatar>().isOwner == false)
            {
                GameObject.Find(enemyName).GetComponent<EnemyAvatar>().UpdatePosition(position);
                GameObject.Find(enemyName).transform.localScale = new Vector2(direction,1);
            }

        }

        private void HandleEnemyTarget(byte[] data, CSteamID senderID, ref int offset)
        {
            ulong bossTargetID = PacketDeserializer.ReadULong(data, ref offset);

            string enemyName = PacketDeserializer.ReadString(data,ref offset);

            string sceneName = PacketDeserializer.ReadString(data, ref offset);
            if (SilksongMultiplayerAPI.currentScene != sceneName)
                return;

            if(GameObject.Find(enemyName) && GameObject.Find(enemyName).GetComponent<EnemyAvatar>() && GameObject.Find(enemyName).GetComponent<EnemyAvatar>().isOwner == false)
            {
                SilksongMultiplayerAPI.ChangeEnemyTarget(bossTargetID, enemyName);
            }
        }

        private void HandleSceneOwner(byte[] data, CSteamID senderID, ref int offset)
        {
            ulong targetID = PacketDeserializer.ReadULong(data,ref offset);

            string sceneName = PacketDeserializer.ReadString(data, ref offset);

            if (SilksongMultiplayerAPI.roomOwner == false)
            {
                if (targetID == SteamUser.GetSteamID().m_SteamID)
                {
                    SilksongMultiplayerAPI.ChangeCurrentOwnedScene(sceneName);
                }
                else if (SilksongMultiplayerAPI.currentScene == sceneName)
                {
                    foreach (HealthManager enemyHealthManager in HealthManager.EnumerateActiveEnemies())
                    {
                        if (enemyHealthManager.gameObject.GetComponent<EnemyAvatar>())
                        {
                            enemyHealthManager.gameObject.GetComponent<EnemyAvatar>().isOwner = false;
                        }
                    }
                }
            }
        }

        private void HandleAllKnockDown(byte[] data, CSteamID senderID, ref int offset)
        {
            SilksongMultiplayerAPI.AllPlayerKnockedDown = true;
            SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = true;
            SilksongMultiplayerAPI.Hero_Hornet.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            SilksongMultiplayerAPI.Hero_Hornet.transform.Find("HeroBox").GetComponent<Collider2D>().enabled = true;
            SilksongMultiplayerAPI.KnockedDown = false;
            Debug.Log("全员倒地");

            HeroController.instance.TakeDamage(base.gameObject, CollisionSide.other, 999, HazardType.ENEMY);

            foreach (KeyValuePair<CSteamID, PlayerAvatar> kvp in SilksongMultiplayerAPI.remotePlayers)
            {
                kvp.Value.KnockedDown = false;
                kvp.Value.cocoon.transform.localPosition = new Vector3(0, 1000, 0);
            }
        }

        private void HandleSkinData(byte[] data, CSteamID senderID, ref int offset)
        {
            string skinName = PacketDeserializer.ReadString(data, ref offset);
            string link1 = PacketDeserializer.ReadString(data, ref offset);
            string link2 = PacketDeserializer.ReadString(data, ref offset);
            string link3 = PacketDeserializer.ReadString(data, ref offset);
            string link4 = PacketDeserializer.ReadString(data, ref offset);
            print("受到皮肤链接信息" + skinName);
            if(skinName != "default")
            {
                SilksongMultiplayerAPI.remotePlayers[senderID].skinName = skinName;

                string savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", skinName, "Knight", "atlas0.png");
                DownloadImageAsync(link1, savePath);

                savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", skinName, "Knight", "atlas1.png");
                DownloadImageAsync(link2, savePath);

                savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", skinName, "Knight", "atlas2.png");
                DownloadImageAsync(link3, savePath);

                savePath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "plugins", "XvX", "skin", skinName,"Knight", "atlas3.png");
                DownloadImageAsync(link4, savePath);
            }
        }

        private static readonly HttpClient _http = new HttpClient();

        public static async Task<string> DownloadImageAsync(string url, string saveFullPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(saveFullPath)!);

            // ✅ 已存在就直接返回
            if (File.Exists(saveFullPath))
                return saveFullPath;

            byte[] data = await _http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(saveFullPath, data);

            return saveFullPath;
        }
    }

    
}
