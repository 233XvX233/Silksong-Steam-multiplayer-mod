using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using GlobalEnums;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongMultiplayer
{
    internal class Dummy : MonoBehaviour, IHitResponder
    {
        HealthManager hm;
        public EnemyHitEffectsRegular hitEffectsRegular;
        public bool hero = false;
        public bool boss = false;
        public bool cocoon = false;
        public bool noExtraComponent = false;
        public string bossName;
        public string imageName = "myImage.png";
        public Rect spriteSize = new Rect(0, 0, 0.5f, 1f);
        public Vector2Int textureSize = new Vector2Int(1,1);
        public bool DoNotSend = false;

        public float damageMultiplier = 1;
        public void Init()
        {
            if(noExtraComponent == false)
            {
                SpriteRenderer spriteRenderer = this.gameObject.AddComponent<SpriteRenderer>();

                string folder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "BepInEx", "plugins", "XvX");
                string imagePath = Path.Combine(folder, imageName);

                byte[] fileData = File.ReadAllBytes(imagePath);

                Texture2D tex = new Texture2D(textureSize.x, textureSize.y);
                tex.LoadImage(fileData); // 现在可以识别了

                Sprite sprite = Sprite.Create(
                    tex,
                    spriteSize,
                    new Vector2(0.5f, 0.5f)
                );


                spriteRenderer.sprite = sprite;

                BoxCollider2D collider = this.gameObject.AddComponent<BoxCollider2D>();
                this.gameObject.layer = (int)PhysLayers.ENEMIES;
                collider.isTrigger = true;
                this.gameObject.AddComponent<PersonalObjectPool>();
                
                hm = this.gameObject.AddComponent<HealthManager>();
                SilksongMultiplayerAPI.SetDamageScalingToCustom(hm);
                SilksongMultiplayerAPI.ReplaceItemDropGroups(hm);

                hitEffectsRegular = this.gameObject.AddComponent<EnemyHitEffectsRegular>();
                hitEffectsRegular.Profile = SilksongMultiplayerAPI.sampleEnemyHitEffectsProfile;

                hm.hp = 1000;
            }
        }

        public IHitResponder.HitResponse Hit(HitInstance hitInstance)
        {

            Debug.Log("attack type: " + hitInstance.AttackType + " Damage: " + hitInstance.DamageDealt);
            
            if (noExtraComponent == false)
            {
                hm.hp = 1000;
            }

            if(hero && hitInstance.IsHeroDamage)
            {
                this.transform.parent.GetComponent<PlayerAvatar>().HitByHero(hitInstance);
            }

            if(DoNotSend == false)
            {
                if (cocoon)
                {
                    if(hitInstance.IsHeroDamage)
                    {
                        this.transform.parent.GetComponent<PlayerAvatar>().CocoonHitByHero(hitInstance);

                        if (this.GetComponent<EnemyHitEffectsRegular>())
                            this.GetComponent<EnemyHitEffectsRegular>().ReceiveHitEffect(hitInstance);
                    }
                }
                else
                {
                    if (boss && cocoon == false)
                    {
                        NetworkDataSender.SendTargetEnemyTakeDamageData(bossName, hitInstance, SilksongMultiplayerAPI.currentScene);
                    }

                    if (this.gameObject.GetComponent<EnemyAvatar>().isOwner && boss)
                    {
                        NetworkDataSender.SendTargetEnemyHpData(bossName, this.GetComponent<HealthManager>().hp, SilksongMultiplayerAPI.currentScene);
                    }
                }
            }
            else
            {
                DoNotSend = false;
            }
                return IHitResponder.Response.None;
        }
    }

    [HarmonyPatch(typeof(HealthManager))]
    [HarmonyPatch("TakeDamage")]
    internal static class TakeDamage_Patch
    {
        // Token: 0x06000013 RID: 19 RVA: 0x00002230 File Offset: 0x00000430
        private static bool Prefix(HealthManager __instance, ref HitInstance hitInstance)
        {
            if (__instance == null || __instance.gameObject == null)
            {
                return true;
            }

            if(__instance.gameObject.GetComponent<Dummy>())
            {
                    hitInstance.Multiplier *= __instance.gameObject.GetComponent<Dummy>().damageMultiplier;
            }
                

            return true;
        }
    }


}
