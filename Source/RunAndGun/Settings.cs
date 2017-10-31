using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Settings;
using RunAndGun.Utilities;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

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
        private SettingHandle<int> enableForFleeChance;
        private SettingHandle<bool> enableForAI;
        internal static SettingHandle<StringHashSetHandler> weaponSelecter;
        internal static SettingHandle<float> weightLimitFilter;
        internal static SettingHandle<bool> applyFilter;



        private const int minPercentage = 0;
        private const int maxPercentage = 100;
        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);


        public override void DefsLoaded()
        {
            float maxWeightMelee;
            float maxWeightRanged;
            WeaponUtility.getHeaviestWeapons(WeaponUtility.getAllWeapons(), out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            float maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);

            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "RG_AccuracyPenalty_Title".Translate(), "RG_AccuracyPenalty_Description".Translate(), 10, Validators.IntRangeValidator(minPercentage, maxPercentage));
            movementPenalty = Settings.GetHandle<int>("movementPenalty", "RG_MovementPenalty_Title".Translate(), "RG_MovementPenalty_Description".Translate(), 35, Validators.IntRangeValidator(minPercentage, maxPercentage));
            enableForFleeChance = Settings.GetHandle<int>("enableRGForFleeChance", "RG_EnableRGForFleeChance_Title".Translate(), "RG_EnableRGForFleeChance_Description".Translate(), 100, Validators.IntRangeValidator(minPercentage, maxPercentage));

            enableForAI = Settings.GetHandle<bool>("enableRGForAI", "RG_EnableRGForAI_Title".Translate(), "RG_EnableRGForAI_Description".Translate(), true);

            weightLimitFilter = Settings.GetHandle<float>("LimitModeSingle_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 3.4f);
            weightLimitFilter.CustomDrawer = rect => { return DrawUtility.CustomDrawer_Filter(rect, weightLimitFilter, applyFilter, false, 0, maxWeightTotal, highlight1); };

            applyFilter = Settings.GetHandle<bool>("applyFilter", "RG_ApplyFilter_Title".Translate(), "RG_ApplyFilter_Description".Translate(), false);
            applyFilter.VisibilityPredicate = delegate { return false; };

            weaponSelecter = Settings.GetHandle<StringHashSetHandler>("LimitModeSingle_Selection", "WeaponSelection_title".Translate(), "WeaponSelection_desc".Translate(), null);
            weaponSelecter.CustomDrawer = rect => { return DrawUtility.CustomDrawer_MatchingWeapons_active(rect, weaponSelecter, highlight1, "ConsideredWeapons".Translate(), "NotConsideredWeapons".Translate()); };

        }

        

    }
}
