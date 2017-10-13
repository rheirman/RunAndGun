using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace RunAndGun
{
    public class Settings : ModBase
    {
        public override string ModIdentifier
        {
            get { return "SettingTest"; }
        }
        private SettingHandle<int> accuracyPenalty;
        private SettingHandle<int> movementPenalty;
        public override void DefsLoaded()
        {
            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "RG_AccuracyPenalty_Title".Translate(), "RG_AccuracyPenalty_Description".Translate(), 10);
            movementPenalty = Settings.GetHandle<int>("movementPenalty", "RG_MovementPenalty_Title".Translate(), "RG_MovementPenalty_Description".Translate(), 15);
        }
    }
}
