using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using Verse;
using Verse.Sound;
using Verse.AI;
using RimWorld;



namespace RunAndGun.Harmony
{

    [StaticConstructorOnStartup]
    class HarmonyBase
    {
        static HarmonyBase()
        {
            var harmony = HarmonyInstance.Create("RunAndGun.Harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("HarmonyBase initialisation executed");
        }
    }
    /*
    [HarmonyPatch(typeof(JobDriver_Goto),"Notify_StanceChanged")]
    static class JobDriver_Wait_Notify_StanceChanged
    {
        static bool Prefix(JobDriver_Goto __instance)
        {
            Log.Message("notify stancechanged is called");
            return true;
        }
    }

    [HarmonyPatch(typeof(JobDriver_Goto), "CheckForAutoAttack")]
    static class JobDriver_Wait_CheckForAutoAttack
    {
        static bool Prefix(JobDriver_Goto __instance)
        {
            Log.Message("checkforautoattack is called");
            return true;
        }
    }
    */
    [HarmonyPatch(typeof(JobDriver_Goto), "SetupToils")]
    static class JobDriver_SetupToils
    {
        static void Postfix(JobDriver_Goto __instance)
        {
            if(__instance is JobDriver_Goto)
            {
                List<Toil> toils = Traverse.Create(__instance).Field("toils").GetValue<List<Toil>>();
                if(toils.Count() > 0)
                {

                    Toil toil = toils.ElementAt(0);
                    toil.AddPreTickAction(delegate
                    {
                        if(__instance.pawn != null && __instance.pawn.IsColonist && __instance.pawn.Drafted && !__instance.pawn.Downed)
                        {
                            checkForAutoAttack(__instance);
                        }
                    });


                }

            }
        }
        static void checkForAutoAttack(JobDriver_Goto __instance)
        {
            if ((__instance.pawn.story == null || !__instance.pawn.story.WorkTagIsDisabled(WorkTags.Violent)) 
                && __instance.pawn.Faction != null 
                && !(__instance.pawn.stances.curStance is Stance_RunAndGun) 
                && __instance.pawn.jobs.curJob.def == JobDefOf.Goto 
                && (__instance.pawn.drafter == null || __instance.pawn.drafter.FireAtWill))
            {
                CompRunAndGun comp = __instance.pawn.TryGetComp<CompRunAndGun>();
                if(comp.isEnabled == false)
                {
                    return;
                }

                Verb verb = __instance.pawn.TryGetAttackVerb(true);
                if (verb != null && !verb.verbProps.MeleeRange)
                {
                    TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat;
                    if (verb.verbProps.ai_IsIncendiary)
                    {
                        targetScanFlags |= TargetScanFlags.NeedNonBurning;
                    }
                    Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, null, verb.verbProps.range, verb.verbProps.minRange, targetScanFlags);
                    if (thing != null)
                    {
                        __instance.pawn.equipment.TryStartAttack(thing);
                        return;
                    }
                }
            }
        }

    }




    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryStartAttack")]
    static class Verb_TryStartAttack
    {
        static bool Prefix(Pawn_EquipmentTracker __instance, ref LocalTargetInfo targ)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            CompRunAndGun comp = pawn.TryGetComp<CompRunAndGun>();
            Log.Message("TryStartAttack");
            Log.Message("current stance: " + pawn.stances.curStance.ToString());

            if (comp == null || !comp.isEnabled  || pawn.jobs.curJob.def != JobDefOf.Goto)
            {
                Log.Message("Executing normal TryStartCastOn");
                return true;
            }
            //if (pawn.stances.FullBodyBusy)
            //{
            //   return false;
            //}
            if(!(pawn.stances.curStance is Stance_Mobile))
            {
                return false;
            }

            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                return false;
            }
            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(allowManualCastWeapons);
            if(verb != null)
            {
                Log.Message("TryStartCast!");
                verb.TryStartCastOn(targ, false, true);
            }
            return false;
        }
    }


    [HarmonyPatch(typeof(Verb), "TryStartCastOn")]
    static class Verb_TryStartCastOn
    {
        static bool Prefix(Verb __instance, ref LocalTargetInfo castTarg, ref bool surpriseAttack, ref bool canFreeIntercept)
        {
            Log.Message("Prefix method is called");
            Pawn pawn = __instance.CasterPawn;

            if (__instance.caster == null)
            {
                Log.Error("Verb " + __instance.GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
                return false;
            }
            if (!__instance.caster.Spawned)
            {
                return false;
            }
            if (__instance.state == VerbState.Bursting || !__instance.CanHitTarget(castTarg))
            {
                return false;
            }
            if (__instance.verbProps.CausesTimeSlowdown && castTarg.HasThing && (castTarg.Thing.def.category == ThingCategory.Pawn || (castTarg.Thing.def.building != null && castTarg.Thing.def.building.IsTurret)) && castTarg.Thing.Faction == Faction.OfPlayer && __instance.caster.HostileTo(Faction.OfPlayer))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }

            if (__instance.CasterIsPawn)
            {
                CompRunAndGun comp = pawn.TryGetComp<CompRunAndGun>();
                if (comp == null || !comp.isEnabled || pawn.jobs.curJob.def != JobDefOf.Goto)
                {
                    Log.Message("Executing normal Stance_busy");
                    return true;
                }
            }


            Traverse.Create(__instance).Field("surpriseAttack").SetValue(surpriseAttack);
            Traverse.Create(__instance).Field("canFreeInterceptNow").SetValue(canFreeIntercept);
            Traverse.Create(__instance).Field("currentTarget").SetValue(castTarg);


            if (__instance.CasterIsPawn && __instance.verbProps.warmupTime > 0f )
            {

                ShootLine newShootLine;
                if (!__instance.TryFindShootLineFromTo(__instance.caster.Position, castTarg, out newShootLine))
                {
                    return false;
                }
                __instance.CasterPawn.Drawer.Notify_WarmingCastAlongLine(newShootLine, __instance.caster.Position);
                float statValue = __instance.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
                int ticks = (__instance.verbProps.warmupTime * statValue).SecondsToTicks();
                Log.Message("creating Stance_RunAndGun");
                __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun(ticks, castTarg, __instance));
            }
            else
            {
                Log.Message("Calling warmupcomplete through TryStartCastOn");
                __instance.WarmupComplete();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    static class Verb_TryCastNextBurstShot
    {
        static bool Prefix(Verb __instance)
        {

            if(!__instance.CasterIsPawn || (!(__instance.CasterPawn.stances.curStance is Stance_RunAndGun) && !(__instance.CasterPawn.stances.curStance is Stance_RunAndGun_Cooldown))){
                Log.Message("executing normal TryCastNextBurstShot");
                return true;
            }
            Log.Message("executing normal TryCastNextBurstShot for RunAndGun");
            Log.Message("1");
            int burstShotsLeft = Traverse.Create(__instance).Field("burstShotsLeft").GetValue<int>();
            Log.Message("2");
            LocalTargetInfo currentTarget = Traverse.Create(__instance).Field("currentTarget").GetValue<LocalTargetInfo>();
            Log.Message("3");
            if (Traverse.Create(__instance).Method("TryCastShot").GetValue<bool>())
            {
                Log.Message("5");
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
                    Log.Message("6");
                }
                if (__instance.CasterPawn.mindState != null)
                {
                    Log.Message("pre-7");
                    Traverse.Create(__instance.CasterPawn.mindState).Method("Notify_AttackedTarget", currentTarget);
                    Log.Message("7");
                }
                if (!__instance.CasterPawn.Spawned)
                {
                    return false;
                }
                Traverse.Create(__instance).Field("burstShotsLeft").SetValue(burstShotsLeft - 1);
                Log.Message("8");
            }
            else
            {
                Log.Message("pre-9");
                Traverse.Create(__instance).Field("burstShotsLeft").SetValue(0);
                Log.Message("9");
            }
            if (Traverse.Create(__instance).Field("burstShotsLeft").GetValue<int>() > 0)
            {
                Log.Message("pre-10");
                Log.Message("10");
                int ticksBetweenBurstShots = Traverse.Create(__instance.verbProps).Field("ticksBetweenBurstShots").GetValue<int>();
                Log.Message("11");
                Traverse.Create(__instance).Field("ticksToNextBurstShot").SetValue(ticksBetweenBurstShots);
                Log.Message("12");
                if (__instance.CasterIsPawn)
                {
                    __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.ticksBetweenBurstShots + 1, currentTarget, __instance));
                }
            }
            else
            {
                __instance.state = VerbState.Idle;
                __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.AdjustedCooldownTicks(__instance.ownerEquipment), currentTarget, __instance));
                if (__instance.castCompleteCallback != null)
                {
                    __instance.castCompleteCallback();
                }
            }
            return false;
        }
    }





}
