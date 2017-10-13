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

                Pawn pawn = (Pawn) (parent as Pawn);
                if (pawn == null)
                    Log.Error("pawn is null");
                return pawn;
            }
        }
        public bool isEnabled = false; 





        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isEnabled, "currentFireMode", false);
        }



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
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
            String uiElement = isEnabled ? "disable_RG" : "enable_RG";
            String label = isEnabled ? "RG_Action_Disable_Label".Translate() : "RG_Action_Enable_Label".Translate();
            String description = isEnabled ? "RG_Action_Disable_Description".Translate() : "RG_Action_Enable_Description".Translate();
            Command_Action testActionGizmo = new Command_Action
            {
                action = runAndGunAction,
                defaultLabel = label,
                defaultDesc = description,
                icon = ContentFinder<Texture2D>.Get(("UI/Buttons/" + uiElement), true),
                tutorTag = null
            };

            yield return testActionGizmo;
        }

        public void runAndGunAction()
        {
            isEnabled = !isEnabled;
        }
    }
}

