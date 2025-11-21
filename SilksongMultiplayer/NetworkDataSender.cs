using System;
using System.Text;
using Steamworks;
using UnityEngine;

namespace SilksongMultiplayer
{
    public static class NetworkDataSender
    {
        /// <summary>
        /// 统一的发送函数，减少重复
        /// </summary>
        public static void Broadcast(byte[] data, EP2PSend sendType)
        {
            foreach (CSteamID member in SilksongMultiplayerAPI.GetRoomMembers())
            {
                if (member != SteamUser.GetSteamID())
                {
                    SteamNetworking.SendP2PPacket(
                        member,
                        data,
                        (uint)data.Length,
                        sendType
                    );
                }
            }
        }

        public static void SendAnimationData(string animationName, int extraValue)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.PlayerAnimation),
                PacketSerializer.SerializeString(animationName),
                PacketSerializer.SerializeInt(extraValue)
            );

            Broadcast(data, EP2PSend.k_EP2PSendUnreliable); // 动画可丢包
        }

        public static void SendMapChangeNotification(string mapName)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.MapChange),
                PacketSerializer.SerializeString(mapName)
            );

            Broadcast(data, EP2PSend.k_EP2PSendReliable); // 必须可靠
        }

        public static void SendTargetHeroTakeDamageData(ulong targetSteamId, int damage, int direction, int hazardType, int attackTypes)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.PlayerTakeDamage),
                PacketSerializer.SerializeULong(targetSteamId),
                PacketSerializer.SerializeInt(damage),
                PacketSerializer.SerializeInt(direction),
                PacketSerializer.SerializeInt(hazardType),
                PacketSerializer.SerializeInt(attackTypes)
            );

            Broadcast(data, EP2PSend.k_EP2PSendReliable); // 伤害必须可靠
        }

        public static void SendHeroAttackAnimationData(string parentName, string name, string animationName)
        {
            byte[] parentBytes = Encoding.UTF8.GetBytes(parentName);
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] animBytes = Encoding.UTF8.GetBytes(animationName);

            // 检查长度
            if (parentBytes.Length > 255 || nameBytes.Length > 255 || animBytes.Length > 255)
            {
                Debug.LogError("[Net] 字符串太长，无法发送！");
                return;
            }

            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.HeroAttackAnimation),
                PacketSerializer.SerializeString(parentName),
                PacketSerializer.SerializeString(name),
                PacketSerializer.SerializeString(animationName)
            );

            Broadcast(data, EP2PSend.k_EP2PSendUnreliable);
        }

        public static void SendTargetCocoonTakeDamageData(ulong targetSteamId, int damage, int direction, int hazardType, int attackTypes)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.CocoonTakeDamage),
                PacketSerializer.SerializeULong(targetSteamId),
                PacketSerializer.SerializeInt(damage),
                PacketSerializer.SerializeInt(direction),
                PacketSerializer.SerializeInt(hazardType),
                PacketSerializer.SerializeInt(attackTypes)
            );

            Broadcast(data, EP2PSend.k_EP2PSendReliable); // 伤害必须可靠
        }

        public static void SendEnemyFsmStateData(string bossName, string stateName, string sceneName)
        {
            // 1. 构造数据包
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.EnemyFsmState), // 新增枚举类型
                PacketSerializer.SerializeString(bossName),
                PacketSerializer.SerializeString(stateName),
                PacketSerializer.SerializeString(sceneName)
            );

            // 2. 广播给所有玩家
            foreach (CSteamID member in SilksongMultiplayerAPI.GetRoomMembers())
            {
                if (member != SteamUser.GetSteamID())
                {
                    SteamNetworking.SendP2PPacket(
                        member,
                        data,
                        (uint)data.Length,
                        EP2PSend.k_EP2PSendReliable // Boss 事件建议可靠传输，避免丢失
                    );
                }
            }
        }


        public static void SendTargetEnemyTakeDamageData(string enemyName, HitInstance hitInstance, string sceneName)
        {
            /*
            	damageDealt = hitInstance.DamageDealt,+
				direction = hitInstance.Direction,+
				magnitudeMult = hitInstance.MagnitudeMultiplier,+
				attackType = (int)hitInstance.AttackType,+
				nailElement = (int)hitInstance.NailElement,+
				nonLethal = false,+
				critical = hitInstance.CriticalHit,+
				canWeakHit = hitInstance.CanWeakHit,+
				multiplier = hitInstance.Multiplier,+
				damageScalingLevel = hitInstance.DamageScalingLevel,+
				specialType = (int)hitInstance.SpecialType,
				isHeroDamage = true
            */

            // 1. 打包数据
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.EnemyTakeDamage), // 新枚举类型
                PacketSerializer.SerializeString(enemyName),
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
            );


            Broadcast(data, EP2PSend.k_EP2PSendReliable);

        }

        public static void SendTargetEnemyHpData(string enemyName, int hp , string sceneName)
        {
            // 1. 打包数据
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.EnemyHp), // 新枚举类型
                PacketSerializer.SerializeString(enemyName),
                PacketSerializer.SerializeInt(hp),
                PacketSerializer.SerializeString(sceneName)
            );


            Broadcast(data, EP2PSend.k_EP2PSendReliable);

        }

        public static void PlayerKnockDown(bool isKnockedDown)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.PlayerKnockDown),
                PacketSerializer.SerializeBool(isKnockedDown)
           );

            Broadcast(data, EP2PSend.k_EP2PSendReliable);
        }

        public static void AllKnockDown()
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.AllKnockDown)
            );

            Broadcast(data, EP2PSend.k_EP2PSendReliable);

        }

        public static void SendEnemyPositionToAll(string enemyName, Vector2 vector2,int direction , string sceneName)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.EnemyPosition),
                PacketSerializer.SerializeString(enemyName),
                PacketSerializer.SerializeFloat(vector2.x),
                PacketSerializer.SerializeFloat(vector2.y),
                PacketSerializer.SerializeInt(direction),
                PacketSerializer.SerializeString(sceneName)
                );

            Broadcast(data, EP2PSend.k_EP2PSendUnreliable);
        }

        public static void SendEnemyTargetData(ulong targetID, string enemyName, string sceneName)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.EnemyTarget),
                PacketSerializer.SerializeULong(targetID),
                PacketSerializer.SerializeString(enemyName),
                PacketSerializer.SerializeString(sceneName)
                );

            Broadcast(data, EP2PSend.k_EP2PSendReliable);
        }

        public static void SendSceneOwner(ulong targetID, string sceneName)
        {
            byte[] data = PacketSerializer.Combine(
                PacketSerializer.SerializeByte((byte)NetworkMessageType.SceneOwner),
                PacketSerializer.SerializeULong(targetID),
                PacketSerializer.SerializeString(sceneName)
                );

            Broadcast(data, EP2PSend.k_EP2PSendReliable);

        }
    }
}
