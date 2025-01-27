using System;

namespace OpenKh.Godot.Helpers
{
    public static class CollisionHelpers
    {
        [Flags]
        public enum CoctFlags : uint
        {
            PartyStand = 1 << 0,
            EntityFallOverride = 1 << 1,
            UnknownFallFlag1 = 1 << 2,
            UnknownFallFlag2 = 1 << 3,
            HitPlayer = 1 << 4,
            HitEnemy = 1 << 5,
            HitFlyEnemy = 1 << 6,
            HitAttack = 1 << 7,
            HitSafety = 1 << 8,
            IK = 1 << 9,
            Dangle = 1 << 10,
            Barrier = 1 << 11,
            MsgWall = 1 << 12,
            Callback = 1 << 13,
            CaribDisp0 = 1 << 14,
            CaribDisp1 = 1 << 15,
            Belt = 1 << 16,
            PolygonSe0 = 1 << 17,
            PolygonSe1 = 1 << 18,
            HitRtn = 1 << 19,
            NoHitFloor = 1 << 20,
        }
    }
}
