using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using RimWorld;
using Verse.Sound;
using System.Reflection.Emit;
using System.Reflection;

namespace RunAndGun.Harmony
{
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class Verb_TryCastNextBurstShot
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction instruction in instructionsList)
            {
                if (instruction.operand as MethodInfo == typeof(Pawn_StanceTracker).GetMethod("SetStance"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(Verb_TryCastNextBurstShot).GetMethod("SetStanceRunAndGun"));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        public static void SetStanceRunAndGun(Pawn_StanceTracker stanceTracker, Stance_Cooldown stance)
        {
            if(stanceTracker.pawn.equipment == null)
            {
                stanceTracker.SetStance(stance);
                return;
            }
            if (stanceTracker.pawn.equipment.Primary == stance.verb.EquipmentSource || stance.verb.EquipmentSource == null)
            {
                if ((((stanceTracker.curStance is Stance_RunAndGun) || (stanceTracker.curStance is Stance_RunAndGun_Cooldown))) && stanceTracker.pawn.pather.Moving)
                {
                    stanceTracker.SetStance(new Stance_RunAndGun_Cooldown(stance.ticksLeft, stance.focusTarg, stance.verb));
                }
                else
                {
                    stanceTracker.SetStance(stance);
                }
            }
        }

        /*
        static bool Prefix(Verb __instance)
        {
            if (!__instance.CasterIsPawn || (!(__instance.CasterPawn.stances.curStance is Stance_RunAndGun) && !(__instance.CasterPawn.stances.curStance is Stance_RunAndGun_Cooldown)))
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
                    if(__instance.CasterPawn.jobs.curJob.def.Equals(JobDefOf.Goto))
                    {
                        __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.ticksBetweenBurstShots + 1, currentTarget, __instance));
                    }
                    else
                    {
                        __instance.CasterPawn.stances.SetStance(new Stance_Cooldown(__instance.verbProps.ticksBetweenBurstShots + 1, currentTarget, __instance));
                    }
                }
            }
            else
            {
                __instance.state = VerbState.Idle;
                if (__instance.CasterPawn.jobs.curJob.def.Equals(JobDefOf.Goto))
                {
                    __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.AdjustedCooldownTicks(__instance, __instance.CasterPawn), currentTarget, __instance));
                }
                else
                {
                    __instance.CasterPawn.stances.SetStance(new Stance_Cooldown(__instance.verbProps.AdjustedCooldownTicks(__instance, __instance.CasterPawn), currentTarget, __instance));
                }
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
    */
    }
}
