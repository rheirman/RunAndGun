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
using HugsLib;
using HugsLib.Settings;



namespace RunAndGun.Harmony
{

    [StaticConstructorOnStartup]
    class HarmonyBase
    {
        static HarmonyBase()
        {
            var harmony = HarmonyInstance.Create("RunAndGun.Harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

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

            if (comp == null || !comp.isEnabled  || pawn.jobs.curJob.def != JobDefOf.Goto)
            {
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
                __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun(ticks, castTarg, __instance));
            }
            else
            {
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
                __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun_Cooldown(__instance.verbProps.AdjustedCooldownTicks(__instance.ownerEquipment), currentTarget, __instance));
                if (__instance.castCompleteCallback != null)
                {
                    __instance.castCompleteCallback();
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(Pawn), "TicksPerMove")]
    static class Pawn_TicksPerMove
    {
        static void Postfix(Pawn __instance, ref int __result)
        {
            if (__instance.stances.curStance is Stance_RunAndGun || __instance.stances.curStance is Stance_RunAndGun_Cooldown)
            {
             
                ModSettingsPack settings = HugsLibController.SettingsManager.GetModSettings("RunAndGun");
                int value = settings.GetHandle<int>("movementPenalty").Value;
                float factor = ((float)(100 + value) / 100);
                __result = (int)Math.Floor((float)__result * factor);
            }

        }
    }

    //TODO: find a way to get the pawn from this method. 
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
            Pawn_EquipmentTracker eqt = (Pawn_EquipmentTracker) equipment.holdingOwner.Owner;
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
