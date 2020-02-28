using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using HugsLib;
using HugsLib.Settings;

namespace RunAndGun.Harmony
{
    [HarmonyPatch(typeof(Pawn), "TicksPerMove")]
    static class Pawn_TicksPerMove
    {
        static void Postfix(Pawn __instance, ref int __result)
        {
            if (__instance == null || __instance.stances == null)
            {
                return;
            }
            if (__instance.stances.curStance is Stance_RunAndGun || __instance.stances.curStance is Stance_RunAndGun_Cooldown)
            {
                int penalty = 0;
                if (hasLightWeapon(__instance))
                {
                    penalty = Base.movementPenaltyLight.Value;
                }
                else
                {
                    penalty = Base.movementPenaltyHeavy.Value;
                }
                float factor = ((float)(100 + penalty) / 100);
                __result = (int)Math.Floor((float)__result * factor);
            }
        }
        static bool hasLightWeapon(Pawn pawn)
        {
            if( pawn.equipment != null && pawn.equipment.Primary != null)
            {

                bool found = Base.weaponSelecter.Value.InnerList.TryGetValue(pawn.equipment.Primary.def.defName, out WeaponRecord value);
                if (found && !value.isSelected)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
