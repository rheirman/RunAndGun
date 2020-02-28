using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;
using Verse.AI;
using HugsLib.Settings;
using HugsLib;

namespace RunAndGun.Harmony
{
    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public class MentalStateHandler_TryStartMentalState
    {
        static void Postfix(MentalStateHandler __instance, MentalStateDef stateDef)
        {
            if (stateDef != MentalStateDefOf.PanicFlee)
            {
                return;
            }
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            CompRunAndGun comp = pawn.TryGetComp<CompRunAndGun>();

            if (comp != null && Base.enableForAI.Value)
            {
                comp.isEnabled = shouldRunAndGun();
            }
        }
        static bool shouldRunAndGun()
        {
            var rndInt = new Random(DateTime.Now.Millisecond).Next(1, 100);
            int chance = Base.enableForFleeChance.Value;
            if (rndInt <= chance)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
