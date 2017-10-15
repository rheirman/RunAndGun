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
            get { return "RunAndGun"; }
        }
        private SettingHandle<int> accuracyPenalty;
        private SettingHandle<int> movementPenalty;
        private SettingHandle<bool> enableForAI;
        public override void DefsLoaded()
        {
            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "RG_AccuracyPenalty_Title".Translate(), "RG_AccuracyPenalty_Description".Translate(), 10, Validators.IntRangeValidator(0, 100));
            movementPenalty = Settings.GetHandle<int>("movementPenalty", "RG_MovementPenalty_Title".Translate(), "RG_MovementPenalty_Description".Translate(), 35, Validators.IntRangeValidator(0, 100));
            enableForAI = Settings.GetHandle<bool>("enableRGForAI", "RG_EnableRGForAI_Title".Translate(), "RG_EnableRGForAI_Description".Translate(), true);
        }
    }
}
