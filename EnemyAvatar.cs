using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using TMProOld;
using UnityEngine;

namespace SilksongMultiplayer
{
    internal class EnemyAvatar : MonoBehaviour
    {
        private Vector3 targetPosition;
        private Vector3 savedPosition;
        private float movingProgress = 0;
        private float lastUpdateTime;
        private float interpolationDelay = 0.1f; // 与发送间隔匹配

        private float lastSendTime;
        private const float sendInterval = 0.03f; // 30ms 发送一次

        public GameObject TargetPlayer = SilksongMultiplayerAPI.Hero_Hornet;
        private float BossTargetChangeCounter = 0;
        private float BossTargetChangeCounterDefault = 15;

        private float TargetChangeCounter = 0;
        private float TargetChangeCounterDefault = 3;

        public bool isOwner = true;

        public bool isBoss = false;

        public bool died = false;
        public float startCounter = 0.5f;

        void Start()
        {
            if(this.GetComponent<HealthManager>() && this.GetComponent<HealthManager>().hp > 100)
            {
                isBoss = true;
            }

            if (this.GetComponent<Dummy>())
            {
                //this.GetComponent<Dummy>().boss = isBoss;
                float Multiplier;
                if (isBoss)
                    Multiplier = SilksongMultiplayerAPI.BossHPmultiplier;
                else
                    Multiplier = SilksongMultiplayerAPI.EnemyHPmultiplier;
                this.GetComponent<Dummy>().damageMultiplier = 1f / (1 + (SteamMatchmaking.GetNumLobbyMembers(SilksongMultiplayerAPI.RoomManager.currentRoomID) - 1) * Multiplier);
            }

            savedPosition   =   this.transform.position;
            targetPosition = this.transform.position;
            TargetPlayer = SilksongMultiplayerAPI.Hero_Hornet;
        }

        void Update()
        {
            if (this.GetComponent<HealthManager>() && this.GetComponent<HealthManager>().hp < 0)
            {
                died = true;
            }
            else
            {
                died = false;
            }

            if(startCounter > 0)
            {
                startCounter -= Time.deltaTime;
            }
            else
            {
                if (SilksongMultiplayerAPI.currentScene == SilksongMultiplayerAPI.currentOwnedScene)
                {
                    isOwner = true;
                }
                else
                {
                    isOwner = false;
                }
            }

            if (isOwner == false)
            {
                this.transform.position = Vector3.Lerp(savedPosition, targetPosition, movingProgress);
                movingProgress += Time.deltaTime / 0.03f;
            }
            else
            {
                // 定时发送
                if (Time.time - lastSendTime >= sendInterval)
                {
                    if (died == false)
                    {
                        NetworkDataSender.SendEnemyPositionToAll(this.gameObject.name, this.transform.position, (int)this.transform.localScale.x, SilksongMultiplayerAPI.currentScene);
                        lastSendTime = Time.time;
                    }
                    else
                    {
                        NetworkDataSender.SendEnemyPositionToAll(this.gameObject.name, new Vector3(0, 1000, 0), (int)this.transform.localScale.x, SilksongMultiplayerAPI.currentScene);
                        lastSendTime = Time.time;
                    }
                }

                if (isBoss)
                {
                    if (BossTargetChangeCounter > 0)
                    {
                        BossTargetChangeCounter -= Time.deltaTime;
                    }
                    else
                    {
                        BossTargetChangeCounter = BossTargetChangeCounterDefault;
                        int memberCount = SteamMatchmaking.GetNumLobbyMembers(SilksongMultiplayerAPI.RoomManager.currentRoomID);
                        CSteamID memberID;
                        int randomIndex = UnityEngine.Random.Range(-1, memberCount);

                        memberID = SteamMatchmaking.GetLobbyMemberByIndex(SilksongMultiplayerAPI.RoomManager.currentRoomID, randomIndex);
                        if (SilksongMultiplayerAPI.remotePlayers.TryGetValue(memberID, out PlayerAvatar playerAvatar))
                        {

                        }
                        else
                        {
                            memberID = SteamUser.GetSteamID();
                        }


                        Debug.Log("切换boss目标为：" + memberID.m_SteamID);

                        SilksongMultiplayerAPI.ChangeEnemyTarget(memberID.m_SteamID, this.gameObject.name);
                        NetworkDataSender.SendEnemyTargetData(memberID.m_SteamID, this.gameObject.name, SilksongMultiplayerAPI.currentScene);

                    }
                }
                else
                {
                    if (TargetChangeCounter > 0)
                    {
                        TargetChangeCounter -= Time.deltaTime;
                    }
                    else
                    {
                        TargetChangeCounter = TargetChangeCounterDefault;

                        GameObject nearestObject = SilksongMultiplayerAPI.Hero_Hornet;
                        float nearestDistance = Vector2.Distance(SilksongMultiplayerAPI.Hero_Hornet.transform.position, this.transform.position);

                        CSteamID memberID = SteamUser.GetSteamID();
                        foreach (var kvp in SilksongMultiplayerAPI.remotePlayers)
                        {
                            if (Vector2.Distance(kvp.Value.gameObject.transform.position, this.transform.position) < nearestDistance)
                            {
                                nearestObject = kvp.Value.gameObject;
                                nearestDistance = Vector2.Distance(kvp.Value.gameObject.transform.position, this.transform.position);
                                memberID = kvp.Value.steamID;
                            }
                        }

                        Debug.Log("最近目标为：" + nearestObject.name);
                        if (TargetPlayer != nearestObject)
                        {
                            SilksongMultiplayerAPI.ChangeEnemyTarget(memberID.m_SteamID, this.gameObject.name);
                            NetworkDataSender.SendEnemyTargetData(memberID.m_SteamID, this.gameObject.name, SilksongMultiplayerAPI.currentScene);
                        }
                    }
                }
            }
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            targetPosition = newPosition;
            savedPosition = this.transform.position;
            movingProgress = 0;
        }
    }
}
