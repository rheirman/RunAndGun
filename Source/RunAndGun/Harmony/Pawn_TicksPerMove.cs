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
                ModSettingsPack settings = HugsLibController.SettingsManager.GetModSettings("RunAndGun");
                int penalty = 0;
                if (hasLightWeapon(__instance))
                {
                    Log.Message("pawn has light weapon");
                    penalty = Settings.movementPenaltyLight;
                }
                else
                {
                    Log.Message("pawn has heavy weapon");
                    penalty = Settings.movementPenaltyHeavy;
                }
                float factor = ((float)(100 + penalty) / 100);
                __result = (int)Math.Floor((float)__result * factor);
            }

        }

        static bool hasLightWeapon(Pawn pawn)
        {
            if( pawn.equipment.Primary != null)
            {
                HashSet<string> lightWeapons = Settings.weaponSelecter.Value.InnerList;
                if (lightWeapons.Contains(pawn.equipment.Primary.def.defName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
