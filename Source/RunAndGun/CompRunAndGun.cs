using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;


namespace RunAndGun
{
    public class CompRunAndGun : ThingComp
    {
        // Fire mode variables


        private Pawn pawn
        {
            get
            {

                Pawn pawn = parent as Pawn;
                if (pawn == null)
                    Log.Error("pawn is null");
                return pawn;
            }
        }



        public override void Initialize(CompProperties props)
        {
            Log.Message("init called");
            base.Initialize(props);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Log.Message("get extra gizmos called");
            if (pawn != null && pawn.Drafted && pawn.Faction.Equals(Faction.OfPlayer))
            {
                foreach (Command com in GenerateGizmos())
                {
                    yield return com;
                }
            }
        }

        public IEnumerable<Command> GenerateGizmos()
        {
            Command_Action testActionGizmo = new Command_Action
            {
                action = testAction,
                defaultLabel = "Boom!",
                defaultDesc = "There goes the neighbourhood!".Translate(),
                icon = ContentFinder<Texture2D>.Get(("UI/Buttons/test"), true),
                tutorTag = null
            };

            yield return testActionGizmo;
        }

        public void testAction()
        {
            Log.Message("testAction called");
            // Copy-paste from GenExplosion


            Explosion explosion = new Explosion();
            explosion.position = pawn.Position;
            explosion.radius = 15;
            explosion.damType = DamageDefOf.Bomb;
            explosion.instigator = pawn;
            explosion.damAmount = GenMath.RoundRandom(1000);
            explosion.weaponGear = null;
            //explosion.preExplosionSpawnThingDef = null;
            //explosion.preExplosionSpawnChance = 100;
            //explosion.preExplosionSpawnThingCount = 1;
            //explosion.postExplosionSpawnThingDef = Props.postExplosionSpawnThingDef;
            //explosion.postExplosionSpawnChance = Props.postExplosionSpawnChance;
            //explosion.postExplosionSpawnThingCount = Props.postExplosionSpawnThingCount;
            //explosion.applyDamageToExplosionCellsNeighbors = Props.applyDamageToExplosionCellsNeighbors;
            pawn.Map.GetComponent<ExplosionManager>().StartExplosion(explosion, DamageDefOf.Bomb.soundExplosion);
        }

    }
}

