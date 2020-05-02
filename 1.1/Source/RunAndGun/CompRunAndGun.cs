using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using HugsLib.Settings;
using HugsLib;

namespace RunAndGun
{
    public class CompRunAndGun : ThingComp
    {

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

        //This can be misused to read isEnabled from other mods without using (expensive) reflection. 
        public override string GetDescriptionPart()
        {   
            return isEnabled.ToString();
        }


        public override void CompTickRare()
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                bool found = Base.weaponForbidder.Value.InnerList.TryGetValue(pawn.equipment.Primary.def.defName, out WeaponRecord value);
                if (found && value.isSelected)
                {
                    isEnabled = false;
                }
            }
        }


        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Pawn pawn = (Pawn)(parent as Pawn);
            bool enableRGForAI = Base.enableForAI.Value;
            if (!pawn.IsColonist && enableRGForAI)
            {
                isEnabled = true;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isEnabled, "isEnabled", false);
        }

    }
}

