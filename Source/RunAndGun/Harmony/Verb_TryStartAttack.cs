using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;

namespace RunAndGun.Harmony
{

    //TODO: reconsider if this patch is really necessary
    /*
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryStartAttack")]
    static class Verb_TryStartAttack
    {
        static bool Prefix(Pawn_EquipmentTracker __instance, ref LocalTargetInfo targ)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            CompRunAndGun comp = pawn.TryGetComp<CompRunAndGun>();

            if (comp == null || !comp.isEnabled || pawn.jobs.curJob.def != JobDefOf.Goto)
            {
                return true;
            }
            //if (pawn.stances.FullBodyBusy)
            //{
            //   return false;
            //}
            if (!(pawn.stances.curStance is Stance_Mobile))
            {
                return false;
            }

            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return false;
            }
            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(allowManualCastWeapons);
            if (verb != null)
            {
                verb.TryStartCastOn(targ, false, true);
            }
            return false;
        }
    }
    */


}
