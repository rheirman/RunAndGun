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
            Log.Message("stance: " + __instance.stances.curStance.ToString());
            if (__instance.stances.curStance is Stance_RunAndGun || __instance.stances.curStance is Stance_RunAndGun_Cooldown)
            {
                ModSettingsPack settings = HugsLibController.SettingsManager.GetModSettings("RunAndGun");
                int value = settings.GetHandle<int>("movementPenalty").Value;
                float factor = ((float)(100 + value) / 100);
                __result = (int)Math.Floor((float)__result * factor);
            }

        }
    }
}
