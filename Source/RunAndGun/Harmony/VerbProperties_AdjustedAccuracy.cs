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


    [HarmonyPatch(typeof(VerbProperties), "AdjustedAccuracy")]
    static class VerbProperties_AdjustedAccuracy
    {
        static void Postfix(VerbProperties __instance, ref Thing equipment, ref float __result)
        {

            if (!(equipment.holdingOwner.Owner is Pawn_EquipmentTracker))
            {
                return;
            }
            Pawn_EquipmentTracker eqt = (Pawn_EquipmentTracker)equipment.holdingOwner.Owner;
            Pawn pawn = Traverse.Create(eqt).Field("pawn").GetValue<Pawn>();

            if (pawn.stances.curStance is Stance_RunAndGun || pawn.stances.curStance is Stance_RunAndGun_Cooldown)
            {
                ModSettingsPack settings = HugsLibController.SettingsManager.GetModSettings("RunAndGun");
                int value = settings.GetHandle<int>("accuracyPenalty").Value;
                float factor = ((float)(100 - value) / 100);
                __result *= factor;
            }
        }
    }
}
