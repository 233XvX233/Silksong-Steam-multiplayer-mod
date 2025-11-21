using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SilksongMultiplayer
{
    internal class AttackAnimTimeCounter : MonoBehaviour
    {
        public float remainDuration = 0;
        void Update()
        {
            if(remainDuration > 0)
            {
                remainDuration -= Time.deltaTime;
            }
            else if(remainDuration > -100)
            {
                if(this.GetComponent<tk2dSprite>())
                    this.GetComponent<tk2dSprite>().color = new Color(1, 1, 1, 0);

                remainDuration = -100;
            }
        }

        public void SetRemainDuration(float duration)
        {
            remainDuration = duration;
            if(this.GetComponent<tk2dSprite>())
                this.GetComponent<tk2dSprite>().color = new Color(1, 1, 1, 1);
        }

    }
}
