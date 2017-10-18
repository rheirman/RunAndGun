using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace RunAndGun
{
    class Stance_RunAndGun_Cooldown : Stance_Cooldown
    {
        private const float MaxRadius = 0.5f;
        public override bool StanceBusy
        {
            get
            {
                return false;
            }
        }
        public Stance_RunAndGun_Cooldown()
        {
        }
        public Stance_RunAndGun_Cooldown(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
        {
        }

    }

}
