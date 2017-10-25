using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;
using Verse.Sound;

namespace RunAndGun.Harmony
{


    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    static class Verb_TryCastNextBurstShot
    {
        static bool Prefix(Verb __instance)
        {
            Log.Message("TryCastNextBurstShot");
            if (!__instance.CasterIsPawn || (!(__instance.CasterPawn.stances.curStance is Stance_RunAndGun) && !(__instance.CasterPawn.stances.curStance is Stance_RunAndGun_Cooldown)) || __instance.verbProps.MeleeRange)
            {
                return true;
            }
            int burstShotsLeft = Traverse.Create(__instance).Field("burstShotsLeft").GetValue<int>();
            LocalTargetInfo currentTarget = Traverse.Create(__instance).Field("currentTarget").GetValue<LocalTargetInfo>();
            if (Traverse.Create(__instance).Method("TryCastShot").GetValue<bool>())
            {
                if (__instance.verbProps.muzzleFlashScale > 0.01f)
                {
                    MoteMaker.MakeStaticMote(__instance.caster.Position, __instance.caster.Map, ThingDefOf.Mote_ShotFlash, __instance.verbProps.muzzleFlashScale);
                }
                if (__instance.verbProps.soundCast != null)
                {
                    __instance.verbProps.soundCast.PlayOneShot(new TargetInfo(__instance.caster.Position, __instance.caster.Map, false));
                }
                if (__instance.verbProps.soundCastTail != null)
                {
                    __instance.verbProps.soundCastTail.PlayOneShotOnCamera(__instance.caster.Map);
                }

                if (__instance.CasterPawn.thinker != null)
                {
                    Traverse.Create(__instance.CasterPawn.mindState).Method("Notify_EngagedTarget");
                }
                if (__instance.CasterPawn.mindState != null)
                {
                    Traverse.Create(__instance.CasterPawn.mindState).Method("Notify_AttackedTarget", currentTarget);
                }
                if (!__instance.CasterPawn.Spawned)
                {
                    return false;
                }
                Traverse.Create(__instance).Field("burstShotsLeft").SetValue(burstShotsLeft - 1);
            }
            else
            {

                Traverse.Create(__instance).Field("burstShotsLeft").SetValue(0);
            }
            if (Traverse.Create(__instance).Field("burstShotsLeft").GetValue<int>() > 0)
            {
                int ticksBetweenBurstShots = Traverse.Create(__instance.verbProps).Field("ticksBetweenBurstShots").GetValue<int>();
                Traverse.Create(__instance).Field("ticksToNextBurstShot").SetValue(ticksBetweenBurstShots);

                if (__instance.CasterIsPawn)
                {
                    __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.ticksBetweenBurstShots + 1, currentTarget, __instance));
                }
            }
            else
            {
                __instance.state = VerbState.Idle;
                __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.AdjustedCooldownTicks(__instance, __instance.CasterPawn, __instance.ownerEquipment), currentTarget, __instance));
                if (__instance.castCompleteCallback != null)
                {
                    __instance.castCompleteCallback();
                }
            }
            
            if(!(__instance.CasterPawn.stances.curStance is Stance_RunAndGun_Cooldown)){
                return true;
            }

            return false;
        }
    }
}
