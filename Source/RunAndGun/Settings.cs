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
        internal static SettingHandle<int> accuracyPenalty;
        internal static SettingHandle<int> movementPenaltyHeavy;
        internal static SettingHandle<int> movementPenaltyLight;
        internal static SettingHandle<int> enableForFleeChance;
        internal static SettingHandle<bool> enableForAI;
        internal static SettingHandle<StringHashSetHandler> weaponSelecter;
        internal static SettingHandle<StringHashSetHandler> weaponForbidder;
        internal static SettingHandle<String> tabsHandler;

        internal static SettingHandle<float> weightLimitFilter;
        internal static SettingHandle<bool> applyFilter;



        private const int minPercentage = 0;
        private const int maxPercentage = 100;
        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);
        String[] tabNames = { "RG_tab1".Translate(), "RG_tab2".Translate()};

        public override void DefsLoaded()
        {
            float maxWeightMelee;
            float maxWeightRanged;
            WeaponUtility.getHeaviestWeapons(WeaponUtility.getAllWeapons(), out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            float maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);



            enableForAI = Settings.GetHandle<bool>("enableRGForAI", "RG_EnableRGForAI_Title".Translate(), "RG_EnableRGForAI_Description".Translate(), true);
            enableForFleeChance = Settings.GetHandle<int>("enableRGForFleeChance", "RG_EnableRGForFleeChance_Title".Translate(), "RG_EnableRGForFleeChance_Description".Translate(), 100, Validators.IntRangeValidator(minPercentage, maxPercentage));
            enableForFleeChance.VisibilityPredicate = delegate { return enableForAI.Value; };

            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "RG_AccuracyPenalty_Title".Translate(), "RG_AccuracyPenalty_Description".Translate(), 10, Validators.IntRangeValidator(minPercentage, maxPercentage));

            movementPenaltyHeavy = Settings.GetHandle<int>("movementPenaltyHeavy", "RG_MovementPenaltyHeavy_Title".Translate(), "RG_MovementPenaltyHeavy_Description".Translate(), 100, Validators.IntRangeValidator(minPercentage, maxPercentage));
            movementPenaltyLight = Settings.GetHandle<int>("movementPenaltyLight", "RG_MovementPenaltyLight_Title".Translate(), "RG_MovementPenaltyLight_Description".Translate(), 10, Validators.IntRangeValidator(minPercentage, maxPercentage));

            tabsHandler = Settings.GetHandle<String>("tabs", "RG_Tabs_Title".Translate(), "", "none");
            tabsHandler.CustomDrawer = rect => { return DrawUtility.CustomDrawer_Tabs(rect, tabsHandler, tabNames); };

            weightLimitFilter = Settings.GetHandle<float>("weightLimitFilter", "RG_WeightLimitFilter_Title".Translate(), "RG_WeightLimitFilter_Description".Translate(), 3.4f);
            weightLimitFilter.CustomDrawer = rect => { return DrawUtility.CustomDrawer_Filter(rect, weightLimitFilter, applyFilter, false, 0, maxWeightTotal, highlight1); };
            weightLimitFilter.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[0]; };

            applyFilter = Settings.GetHandle<bool>("applyFilter", "", "", false);
            applyFilter.VisibilityPredicate = delegate { return false; };

            weaponSelecter = Settings.GetHandle<StringHashSetHandler>("weaponSelecter", "RG_WeaponSelection_Title".Translate(), "RG_WeaponSelection_Description".Translate(), null);
            weaponSelecter.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[0]; };

            weaponForbidder = Settings.GetHandle<StringHashSetHandler>("weaponForbidder", "RG_WeaponForbidder_Title".Translate(), "RG_WeaponForbidder_Description".Translate(), null);
            weaponForbidder.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[1]; };

            weaponSelecter.CustomDrawer = rect => { return DrawUtility.CustomDrawer_MatchingWeapons_active(rect, weaponSelecter, highlight1, true, "RG_ConsideredLight".Translate(), "RG_ConsideredHeavy".Translate()); };
            weaponForbidder.CustomDrawer = rect => { return DrawUtility.CustomDrawer_MatchingWeapons_active(rect, weaponForbidder, highlight1, false, "RG_Allow".Translate(), "RG_Forbid".Translate() ); };

            if(weaponSelecter.Value == null)
            {
                weaponSelecter.Value = getDefaultForWeaponSelecter();
            }
            if (weaponForbidder.Value == null)
            {
                weaponForbidder.Value = getDefaultForWeaponForbidder();
            }
        }


        private StringHashSetHandler getDefaultForWeaponSelecter()
        {
            StringHashSetHandler result = new StringHashSetHandler();
            List<ThingStuffPair> allWeapons = WeaponUtility.getAllWeapons();
            allWeapons.Sort(new MassComparer());
            result.InnerList = new HashSet<string>();
            for (int i = 0; i < allWeapons.Count; i++)
            {

                float mass = allWeapons[i].thing.GetStatValueAbstract(StatDefOf.Mass);
                if (mass <= weightLimitFilter.Value)
                {
                    result.InnerList.Add(allWeapons[i].thing.defName);
                }
            }
            return result;
        }

        private StringHashSetHandler getDefaultForWeaponForbidder()
        {
            StringHashSetHandler result = new StringHashSetHandler();
            List<ThingStuffPair> allWeapons = WeaponUtility.getAllWeapons();
            allWeapons.Sort(new MassComparer());
            result.InnerList = new HashSet<string>();
            for (int i = 0; i < allWeapons.Count; i++)
            {
                    result.InnerList.Add(allWeapons[i].thing.defName);
            }
            return result;
        }



    }
}
