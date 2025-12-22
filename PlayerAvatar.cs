using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalEnums;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;


namespace SilksongMultiplayer
{
    internal class PlayerAvatar : MonoBehaviour
    {
        private Vector3 positionBias = Vector3.zero;
        private Vector3 targetPosition;
        private Vector3 savedPosition;
        private float movingProgress = 0;
        private float lastUpdateTime;
        private float interpolationDelay = 0.1f; // 与发送间隔匹配


        private Vector3 compassTargetPosition;
        private Vector3 compassSavedPosition;
        private Vector3 wideCompassTargetPosition;
        private Vector3 wideCompassSavedPosition;
        private float compassMovingProgress = 0;


        public CSteamID steamID;
        public string mapName;
        private float createColliderCounter = 3;
        Canvas canva;
        GameObject nameText;
        public GameObject compassIcon;
        public GameObject wideCompassIcon;
        Dummy dummy;
        public bool KnockedDown = false;
        public Dummy cocoon;

        public string skinName = "default";

        public void UpdatePosition(Vector3 newPosition, float facing)
        {
            targetPosition = newPosition + new Vector3(0 ,0,0.001f);
            savedPosition = this.transform.position;
            movingProgress = 0;

            this.transform.localScale = new Vector3(facing, 1, 1);
        }

        public void UpdateCompassPosition(Vector2 compass, Vector2 wideCompass)
        {
            compassTargetPosition = compass;
            compassSavedPosition = compassIcon.transform.localPosition;

            wideCompassTargetPosition = wideCompass;
            wideCompassSavedPosition = wideCompassIcon.transform.localPosition;

            compassMovingProgress = 0;
        }

        public void UpdateMap(string mapName_get)
        {
            mapName = mapName_get;
            SilksongMultiplayerAPI.OnOutherChangeScene(mapName,steamID);
        }

        public void Initialize(CSteamID steamID)
        {
            this.gameObject.AddComponent<SkinLock>();
            Skin.ChangeSkinOnObject(this.gameObject, skinName);

            this.steamID = steamID;
            Debug.Log($"玩家对象已初始化，SteamID: {steamID}");

            GameObject nameCanva = new GameObject("nameCanva");
            nameCanva.transform.SetPositionAndRotation(this.transform.position, Quaternion.identity);
            nameCanva.transform.SetParent(this.transform);

            canva = nameCanva.AddComponent<Canvas>();
            canva.renderMode = RenderMode.ScreenSpaceCamera;
            canva.sortingLayerName = "HUD";
            canva.sortingLayerID = 629535577;
            canva.sortingOrder = 50;

            RectTransform rect = nameCanva.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(2560, 1440);

            nameText = new GameObject("nameText");
            nameText.transform.SetParent(nameCanva.transform);

            // 必须有 CanvasRenderer
            nameText.AddComponent<CanvasRenderer>();
            nameText.transform.localScale = Vector3.one * 0.01f;
            Text text = nameText.AddComponent<Text>();
            text.text = SteamFriends.GetFriendPersonaName(steamID); // 或 SteamFriends.GetPersonaName()
            text.font = SilksongMultiplayerAPI.savedFont;
            text.fontSize = 50;
            text.alignment = TextAnchor.MiddleCenter;


            ulong XvXSteamId64 = 76561198929282998UL;
            ulong truthSteamId64 = 76561199835946204UL;

            if (steamID.m_SteamID == XvXSteamId64 || steamID.m_SteamID == truthSteamId64)
            {
                text.color = Color.yellow;
            }


            //GameObject compassIcon = new GameObject("CompassIconClone");
            //compassIcon = new GameObject("WideCompassIconClone");Game_Map_Hornet(Clone)
            if (SilksongMultiplayerAPI.compassIcon != null)
            {
                compassIcon = GameObject.Instantiate(SilksongMultiplayerAPI.compassIcon, DDOLFinder.FindInDDOLByName("Game_Map_Hornet(Clone)")[0].transform);
                compassIcon.name = "compassIconClone";
            }

            if (SilksongMultiplayerAPI.wideCompassIcon != null)
            {
                wideCompassIcon = GameObject.Instantiate(SilksongMultiplayerAPI.wideCompassIcon, DDOLFinder.FindInDDOLByName("Wide Map(Clone)")[0].transform);
                wideCompassIcon.name = "wideCompassIconClone";
            }

            if(GameObject.Find("SceneBorder(Clone)"))
                NetworkDataSender.SendMapChangeNotification(mapName = GameObject.Find("SceneBorder(Clone)").scene.name);



            //攻击特效
            Transform attacks = SilksongMultiplayerAPI.Hero_Hornet.transform.Find("Attacks");


            GameObject AttacksClone = GameObject.Instantiate(new GameObject("Attacks"), this.transform.position, Quaternion.identity, this.transform);

            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Default"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Cloakless"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Scythe"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Warrior"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Wanderer"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Toolmaster"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Witch"), AttacksClone);
            CreateAttackEffectForAllChild(FindInactiveChildByName(attacks.gameObject, "Shaman"), AttacksClone);

        }

        public void CreateAttackEffectForAllChild(GameObject collection,GameObject parent)
        {
            if(collection.transform.parent.name == "Attacks")
            {
                GameObject attackGroup = GameObject.Instantiate(new GameObject(collection.name), this.transform.position, Quaternion.identity, parent.transform);
                Transform[] children = collection.transform.GetComponentsInChildren<Transform>(true);
                GameObject cloneCollection = null;
                foreach (Transform child in children)
                {
                    if(child.parent.parent.name == "Attacks")
                    {
                        GameObject AttackEffect = GameObject.Instantiate(new GameObject(child.name), this.transform.position, Quaternion.identity, attackGroup.transform);
                        AttackEffect.transform.localPosition = child.localPosition;
                        AttackEffect.AddComponent<AttackAnimTimeCounter>();
                        SilksongMultiplayerAPI.CloneAnimatorOfObject(AttackEffect, child.gameObject);
                    }
                }
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

        tk2dSpriteCollectionData savedtk2dSpriteCollectionData = new tk2dSpriteCollectionData();
        void Update()
        {
            if (this.GetComponent<tk2dSprite>().Collection != savedtk2dSpriteCollectionData)
            {
                Skin.ChangeSkinOnObject(this.gameObject, skinName);
                savedtk2dSpriteCollectionData = this.GetComponent<tk2dSprite>().Collection;
            }

            this.transform.position = Vector3.Lerp(savedPosition, targetPosition + positionBias, movingProgress);
            movingProgress += Time.deltaTime / 0.03f;

            if(SilksongMultiplayerAPI.compassIcon != null && compassIcon != null)
            {
                compassIcon.SetActive(SilksongMultiplayerAPI.compassIcon.activeSelf);
                compassIcon.transform.localPosition = (Vector3)Vector2.Lerp(compassSavedPosition, compassTargetPosition, compassMovingProgress) + new Vector3(0, 0, -5);

                wideCompassIcon.SetActive(SilksongMultiplayerAPI.wideCompassIcon.activeSelf);
                wideCompassIcon.transform.localPosition = (Vector3)Vector2.Lerp(wideCompassSavedPosition, wideCompassTargetPosition, compassMovingProgress) + new Vector3(0, 0, -2);

                compassMovingProgress += Time.deltaTime / 0.03f;
            }



            if(canva != null)
            {
                canva.transform.localPosition = Vector3.zero + new Vector3(0, 2.5f, 0);
                nameText.transform.localPosition = Vector3.zero;
                nameText.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);

                if (this.transform.localScale.x < 0)
                    canva.transform.localScale = new Vector2(-1, 1);
                else
                    canva.transform.localScale = new Vector2(1, 1);
            }


            if (Vector3.Distance(savedPosition, targetPosition) > 5)
            {
                movingProgress = 1;
            }

            if (GameObject.Find("SceneBorder(Clone)"))
            {
                if (GameObject.Find("SceneBorder(Clone)").scene.name == mapName)
                    Hide(false);
                else
                    Hide(true);
            }


            if (createColliderCounter < 0 && createColliderCounter > -100)
            {
                if(SilksongMultiplayerAPI.enablePvP)
                {
                    GameObject newObj = new GameObject("hitbox");
                    newObj.transform.SetPositionAndRotation(this.transform.position, Quaternion.identity);
                    newObj.transform.SetParent(this.transform);
                    dummy = newObj.AddComponent<Dummy>();
                    dummy.hero = true;

                    dummy.Init();
                }

                GameObject newCocoon = new GameObject("cocoon");
                newCocoon.transform.SetPositionAndRotation(this.transform.position, Quaternion.identity);
                newCocoon.transform.SetParent(this.transform);
                cocoon = newCocoon.AddComponent<Dummy>();
                cocoon.cocoon = true;
                cocoon.spriteSize = new Rect(0, 0, 105, 216);
                cocoon.textureSize = new Vector2Int(4,4);
                cocoon.imageName = "Hornet_death.png";
                cocoon.Init();
                
                cocoon.transform.localPosition = new Vector3(0, 1000, 0);
                cocoon.transform.localScale = new Vector3(2, 2, 2);

                if (canva != null)
                {
                    canva.renderMode = RenderMode.WorldSpace;
                    canva.transform.localPosition = Vector3.zero;
                    nameText.transform.localPosition = Vector3.zero;
                }


                createColliderCounter = -100;
            }
            else
            {
                createColliderCounter -= Time.deltaTime;
            }
        }

        void Hide(bool hide)
        {
            if (hide)
            {
                positionBias = new Vector3(0,1000,0);
            }
            else
            {
                positionBias = new Vector3(0, 0, 0);
            }
        }

        public void HitByHero(HitInstance hitInstance)
        {
            int direction = 4;

            if(hitInstance.Direction < 45)
            {
                direction = 1;
            }
            else if (hitInstance.Direction < 135)
            {
                direction = 3;
            }
            else if (hitInstance.Direction < 225)
            {
                direction = 2;
            }
            else if (hitInstance.Direction < 315)
            {
                direction = 0;
            }
            else
            {
                direction = 1;
            }
            float damage = hitInstance.DamageDealt;
            NetworkDataSender.SendTargetHeroTakeDamageData(steamID.m_SteamID, Mathf.FloorToInt(damage / 5), direction, (int)HazardType.ENEMY, (int)hitInstance.AttackType);
        }

        public void CocoonHitByHero(HitInstance hitInstance)
        {
            int direction = 4;

            if (hitInstance.Direction < 45)
            {
                direction = 1;
            }
            else if (hitInstance.Direction < 135)
            {
                direction = 3;
            }
            else if (hitInstance.Direction < 225)
            {
                direction = 2;
            }
            else if (hitInstance.Direction < 315)
            {
                direction = 0;
            }
            else
            {
                direction = 1;
            }
            float damage = hitInstance.DamageDealt;

            NetworkDataSender.SendTargetCocoonTakeDamageData(steamID.m_SteamID, (int)damage, direction, (int)HazardType.ENEMY, (int)hitInstance.AttackType);
        }

        public void HitEffect(CollisionSide direction,int damage, AttackTypes attackType)
        {
            int newDirection = 4;

            switch((int)direction)
            {
                case 1:
                    newDirection = 0;
                    break;
                case 2:
                    newDirection = 180;
                    break;
                case 3:
                    newDirection = 90;
                    break;
                case 4:
                    newDirection = 270;
                    break;
            }

            HitInstance hitInstance = new HitInstance { AttackType = attackType, Direction = newDirection };
            if(dummy.hitEffectsRegular != null)
                dummy.hitEffectsRegular.ReceiveHitEffect(hitInstance);
        }
    }
}
