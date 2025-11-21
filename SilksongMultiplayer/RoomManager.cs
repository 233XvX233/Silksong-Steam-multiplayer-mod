using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GlobalEnums;
using HutongGames.PlayMaker;
using InControl;
using Steamworks;
using TMProOld;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Device;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CutsceneHelper;

namespace SilksongMultiplayer
{
    class RoomManager : MonoBehaviour
    {
        public Steamworks.CSteamID currentRoomID;
        public Steamworks.CSteamID playerID;
        public bool host = false;
        public bool enterRoom = false;

        private bool findAgent = false;
        private bool createButton = false;

        private bool debugBossFinder = true;

        public void CreateRoom()
        {
            // 创建大厅（最多200人）
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 200);
        }

        public void Start()
        {
            Init();
        }

        public void Invite()
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentRoomID);
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.F9))
            {
                // 创建个沙包
                GameObject newObj = Instantiate(new GameObject("banana_Clone"), SilksongMultiplayerAPI.Hero_Hornet.transform.position, Quaternion.identity);
                newObj.AddComponent<Dummy>();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                HeroController.instance.TakeDamage(base.gameObject, CollisionSide.other, 999, HazardType.ENEMY);
            }

            if (SilksongMultiplayerAPI.enablePvP)
            {
                if(GameObject.Find("Mapper NPC") && GameObject.Find("Mapper NPC").transform.Find("Enemy Range"))
                {
                    GameObject.Find("Mapper NPC").transform.Find("Enemy Range").gameObject.SetActive(false);
                }
            }

            if (SilksongMultiplayerAPI.Hero_Hornet != null)
            {
                if (SilksongMultiplayerAPI.KnockedDown)
                {
                    SilksongMultiplayerAPI.Hero_Hornet.GetComponent<HeroController>().acceptingInput = false;
                }
            }
        }

        public void Init()
        {
            // 注册玩家加入大厅的回调
            Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            // 注册邀请接受事件回调
            Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            // 注册大厅创建成功回调
            Callback<LobbyCreated_t>.Create(OnLobbyCreated);

            // 注册玩家变动回调
            Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

            //获取自己的steamID
            playerID = SteamUser.GetSteamID();

            SilksongMultiplayerAPI.RoomManager = this;

        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (enterRoom && SilksongMultiplayerAPI.PlayerListText != null)
            {
                string playerList = "玩家列表";
                int memberCount = SteamMatchmaking.GetNumLobbyMembers(currentRoomID);
                for (int i = 0; i < memberCount; i++)
                {
                    CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(currentRoomID, i);
                    playerList += Environment.NewLine + SteamFriends.GetFriendPersonaName(memberID);
                }

                SilksongMultiplayerAPI.PlayerListText.text = playerList;
            }
        }

        public static GameObject FindObjectInScene(Scene scene, string objectName, bool clone = true)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("Scene 无效或未加载");
                return null;
            }

            GameObject target = null;

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    target = root;
                    break;
                }

                var t = root.transform.Find(objectName);
                if (t != null)
                {
                    target = t.gameObject;
                    break;
                }
            }

            if (target == null)
            {
                Debug.LogWarning($"在场景 {scene.name} 中未找到对象: {objectName}");
                return null;
            }

            // 是否克隆一份（避免卸载场景后丢失）
            if (clone)
            {
                var copy = UnityEngine.Object.Instantiate(target);
                copy.name = target.name + "_Copy";
                return copy;
            }

            return target;
        }



        public void OnLobbyCreated(LobbyCreated_t result)
        {
            if (result.m_eResult == EResult.k_EResultOK)
            {
                currentRoomID = new CSteamID(result.m_ulSteamIDLobby);

                SteamMatchmaking.SetLobbyData(currentRoomID, "name", SteamFriends.GetPersonaName().ToString());

                Debug.Log("大厅创建成功，ID: " + currentRoomID);
                enterRoom = true;
            }
        }

        // 当玩家通过邀请链接加入时触发
        public void OnJoinRequested(GameLobbyJoinRequested_t callback)
        {
            CSteamID inviterSteamID = callback.m_steamIDFriend;
            Debug.Log("收到来自玩家 " + SteamFriends.GetFriendPersonaName(inviterSteamID) + " 的邀请");

            if (SilksongMultiplayerAPI.createLobbyButton != null)
                Destroy(SilksongMultiplayerAPI.createLobbyButton);

            if (SilksongMultiplayerAPI.inviteLobbyButton != null)
                Destroy(SilksongMultiplayerAPI.inviteLobbyButton);


            // 加入邀请者的大厅
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
            host = false;
        }

        // 当玩家成功进入大厅时触发
        public void OnLobbyEntered(LobbyEnter_t callback)
        {
            //所有人
            currentRoomID = new CSteamID(callback.m_ulSteamIDLobby);

            enterRoom = true;
            SilksongMultiplayerAPI.enterRoom = true;
            // 获取大厅内所有成员的 SteamID
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(currentRoomID);
            for (int i = 0; i < memberCount; i++)
            {
                CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(currentRoomID, i);
                Debug.Log("大厅成员 " + SteamFriends.GetFriendPersonaName(memberID) + " SteamID: " + memberID);
            }


            if (enterRoom && SilksongMultiplayerAPI.PlayerListText != null)
            {
                string playerList = "玩家列表";
                for (int i = 0; i < memberCount; i++)
                {
                    CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(currentRoomID, i);
                    playerList += Environment.NewLine + SteamFriends.GetFriendPersonaName(memberID);
                }

                SilksongMultiplayerAPI.PlayerListText.text = playerList;
            }
        }

        void FixedUpdate()
        {
            SteamAPI.RunCallbacks(); // 关键！处理 Steam 事件队列

            if (SilksongMultiplayerAPI.startGame == false && GameObject.Find("Hero_Hornet(Clone)"))
            {
                SilksongMultiplayerAPI.startGame = true;

                CSteamID owner = SteamMatchmaking.GetLobbyOwner(currentRoomID);
                SilksongMultiplayerAPI.roomOwner = (owner.m_SteamID == SteamUser.GetSteamID().m_SteamID);

                SilksongMultiplayerAPI.Hero_Hornet = GameObject.Find("Hero_Hornet(Clone)");
                SilksongMultiplayerAPI.Hero_Hornet.AddComponent<PlayerNetworkSync>();
                SilksongMultiplayerAPI.Hero_Hornet.GetComponent<PlayerNetworkSync>().currentRoomID = currentRoomID;

                //加载场景读取完物体信息后删除场景
                AsyncOperationHandle<SceneInstance> fromOperationHandle = Addressables.LoadSceneAsync("Scenes/" + "Tut_02", LoadSceneMode.Additive, true, 100, SceneReleaseMode.ReleaseSceneWhenSceneUnloaded);
                Scene unityScene = new Scene();
                fromOperationHandle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        SceneInstance sceneInstance = op.Result;     // 这是 SceneInstance
                        unityScene = sceneInstance.Scene;      // 这是 UnityEngine.SceneManagement.Scene
                    }
                };

                fromOperationHandle.Completed += (op) =>
                {
                    GameObject crawler = FindObjectInScene(unityScene, "MossBone Crawler");

                    Debug.Log("已找到");
                    SilksongMultiplayerAPI.sampleEnemyHitEffectsProfile = crawler.GetComponent<EnemyHitEffectsRegular>().Profile;

                    SceneManager.UnloadSceneAsync(unityScene);
                };


                //Hunter_v3
                //Reaper
                //Wanderer
                //Warrior
                //Witch
                //Toolmaster
                //Spell
                //PlayerData.instance.CurrentCrestID
                //toolCrest.HeroConfig.GetAnimationClip
                SilksongMultiplayerAPI.Hunter_v3 = ToolItemManager.GetCrestByName("Hunter_v3");
                SilksongMultiplayerAPI.Reaper = ToolItemManager.GetCrestByName("Reaper");
                SilksongMultiplayerAPI.Wanderer = ToolItemManager.GetCrestByName("Wanderer");
                SilksongMultiplayerAPI.Warrior = ToolItemManager.GetCrestByName("Warrior");
                SilksongMultiplayerAPI.Witch = ToolItemManager.GetCrestByName("Witch");
                SilksongMultiplayerAPI.Toolmaster = ToolItemManager.GetCrestByName("Toolmaster");
                SilksongMultiplayerAPI.Spell = ToolItemManager.GetCrestByName("Spell");
            }


            if (GameObject.Find("OptionsButton") && createButton == false)
            {
                createButton = true;
                GameObject button = GameObject.Instantiate(GameObject.Find("OptionsButton"), GameObject.Find("OptionsButton").transform.parent);
                button.transform.GetChild(0).GetComponent<Text>().text = "创建大厅";
                button.GetComponent<EventTrigger>().enabled = false;
                button.AddComponent<CreateLobbyButton>();
                SilksongMultiplayerAPI.createLobbyButton = button;

                GameObject PlayerList = GameObject.Instantiate(GameObject.Find("OptionsButton").transform.GetChild(0).gameObject);
                Destroy(PlayerList.GetComponent<ChangeTextFontScaleOnHandHeld>());
                Destroy(PlayerList.GetComponent<FixVerticalAlign>());
                Destroy(PlayerList.GetComponent<ContentSizeFitter>());
                PlayerList.transform.parent = GameObject.Find("MainMenuScreen").transform;
                PlayerList.transform.position = new Vector3(-9, 2, -18);
                PlayerList.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 1000);
                PlayerList.transform.localScale = Vector3.one;
                PlayerList.GetComponent<Text>().lineSpacing = 1;
                PlayerList.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
                PlayerList.GetComponent<Text>().text = "";
                SilksongMultiplayerAPI.PlayerListText = PlayerList.GetComponent<Text>();
            }
        }

        public List<CSteamID> GetRoomMembers()
        {
            // 获取大厅内所有成员的 SteamID
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(SilksongMultiplayerAPI.RoomManager.currentRoomID);
            List<CSteamID> members = new List<CSteamID>();
            for (int i = 0; i < memberCount; i++)
            {
                CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(SilksongMultiplayerAPI.RoomManager.currentRoomID, i);
                members.Add(memberID);
            }

            return members;
        }
    }
}
