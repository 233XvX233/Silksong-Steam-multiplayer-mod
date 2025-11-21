using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SilksongMultiplayer
{
    public class InviteLobbyButton : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (SilksongMultiplayerAPI.RoomManager.enterRoom)
            {
                // 打开 Steam 邀请好友界面
                SilksongMultiplayerAPI.RoomManager.Invite();
            }
        }

        public void Update()
        {
            transform.GetChild(0).GetComponent<Text>().text = "邀请好友";

            if (this.GetComponent<EventTrigger>())
                this.GetComponent<EventTrigger>().enabled = false;
        }
    }
}
