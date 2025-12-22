using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilksongMultiplayer
{
    public enum NetworkMessageType : byte
    {
        PlayerJoin = 0,
        PlayerPosition = 1,
        PlayerAnimation = 2,
        MapChange = 3,
        PlayerTakeDamage = 4,
        MapPosition = 5,
        HeroAttackAnimation = 6,
        EnemyFsmState = 7,
        EnemyTakeDamage = 8,
        PlayerKnockDown = 9,
        CocoonTakeDamage = 10,
        EnemyPosition = 11,
        EnemyTarget = 12,
        EnemyHp = 13,
        SceneOwner = 14,
        AllKnockDown = 15,
        Skin = 16,

    }
}
