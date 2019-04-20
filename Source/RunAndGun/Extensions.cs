using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RunAndGun
{
    public static class Extensions
    {
        public static bool HasRangedWeapon(this Pawn instance)
        {
            if(instance.equipment != null && instance.equipment.Primary != null && instance.equipment.Primary.def.IsWeaponUsingProjectiles)
            {
                return true;
            }
            return false;
        }
        public static bool Contains(this Rect rect, Rect otherRect)
        {
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMin)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMin)))
                return false;
            return true;
        }
    }
}
