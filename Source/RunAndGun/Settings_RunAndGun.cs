using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace RunAndGun
{
    public class Settings_RunAndGun : ModBase
    {
        public override string ModIdentifier
        {
            get { return "SettingTest"; }
        }
        private SettingHandle<int> accuracyPenalty;
        private SettingHandle<int> movementPenalty;
        public override void DefsLoaded()
        {
            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "Accuracy penalty(%)".Translate(), "The accuracy penalty when run and gun is enabled".Translate(), 10);
            movementPenalty = Settings.GetHandle<int>("movementPenalty", "Movement penalty(%)".Translate(), "The movement penalty when while shooting".Translate(), 15);
        }
    }
}
