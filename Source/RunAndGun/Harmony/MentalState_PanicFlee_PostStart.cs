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
    public class MentalState_PanicFlee_PostStart
    {
        static void Postfix(MentalStateHandler __instance, MentalStateDef stateDef)
        {
            Log.Message("1");
            if(stateDef != MentalStateDefOf.PanicFlee)
            {
                return;
            }
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            CompRunAndGun comp = pawn.TryGetComp<CompRunAndGun>();
            Log.Message("2");

            if (comp != null && RunAndGun.Base.enableRGForAI.Value)
            {
                Log.Message("3");

                comp.isEnabled = shouldRunAndGun();
                Log.Message("9");

            }
        }
        static bool shouldRunAndGun()
        {
            Log.Message("4");
            var rndInt = new Random(DateTime.Now.Millisecond).Next(1, 100);
            Log.Message("5");
            int chance = RunAndGun.Base.enableRGForFleeChance.Value;
            Log.Message("6");
            if (rndInt <= chance)
            {
                Log.Message("7");
                return true;
            }
            else
            {
                Log.Message("8");
                return false;
            }

        }
    }
}
