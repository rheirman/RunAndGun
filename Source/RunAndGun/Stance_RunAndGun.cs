using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RunAndGun
{
    class Stance_RunAndGun : Stance_Warmup
    {
        public override bool StanceBusy
        {
            get
            {
                return false;
            }
        }
        public Stance_RunAndGun()
        {
        }
        public Stance_RunAndGun(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
        {
        }



    }

}
