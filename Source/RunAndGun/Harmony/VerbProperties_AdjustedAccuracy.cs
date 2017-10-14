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
            Log.Message("HoldingOwner type: " + equipment.holdingOwner.Owner.GetType().ToString());
            if (!(equipment.holdingOwner.Owner is Pawn_EquipmentTracker))
            {
                return;
            }
            Pawn_EquipmentTracker eqt = (Pawn_EquipmentTracker)equipment.holdingOwner.Owner;
            Pawn pawn = Traverse.Create(eqt).Field("pawn").GetValue<Pawn>();

            if (pawn.stances.curStance is Stance_RunAndGun || pawn.stances.curStance is Stance_RunAndGun_Cooldown)
            {
                Log.Message("Accuracy was: " + __result.ToString());
                ModSettingsPack settings = HugsLibController.SettingsManager.GetModSettings("RunAndGun");
                int value = settings.GetHandle<int>("accuracyPenalty").Value;
                Log.Message("Value: " + value);

                float factor = ((float)(100 - value) / 100);
                Log.Message("Factor:" + factor);

                __result *= factor;
                Log.Message("Accuracy now is: " + __result.ToString());
            }
        }
    }
}
