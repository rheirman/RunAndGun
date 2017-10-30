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
        internal static SettingHandle<StringHashSetHandler> LimitModeSingle_Selection;


        private const float weightLimit = 3.4f;
        private const float ContentPadding = 5f;
        private const float IconSize = 32f;
        private const float IconGap = 1f;
        private const float TextMargin = 20f;
        private const float BottomMargin = 2f;

        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);

        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);


        private const int minPercentage = 0;
        private const int maxPercentage = 100;
        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);


        public override void DefsLoaded()
        {
            float maxWeightMelee;
            float maxWeightRanged;
            getHeaviestWeapons(getAllWeapons(), out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            float maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);


            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "RG_AccuracyPenalty_Title".Translate(), "RG_AccuracyPenalty_Description".Translate(), 10, Validators.IntRangeValidator(minPercentage, maxPercentage));
            movementPenalty = Settings.GetHandle<int>("movementPenalty", "RG_MovementPenalty_Title".Translate(), "RG_MovementPenalty_Description".Translate(), 35, Validators.IntRangeValidator(minPercentage, maxPercentage));
            enableForFleeChance = Settings.GetHandle<int>("enableRGForFleeChance", "RG_EnableRGForFleeChance_Title".Translate(), "RG_EnableRGForFleeChance_Description".Translate(), 100, Validators.IntRangeValidator(minPercentage, maxPercentage));
            enableForAI = Settings.GetHandle<bool>("enableRGForAI", "RG_EnableRGForAI_Title".Translate(), "RG_EnableRGForAI_Description".Translate(), true);
            LimitModeSingle_Selection = Settings.GetHandle<StringHashSetHandler>("LimitModeSingle_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingle_Selection.CustomDrawer = rect => { return CustomDrawer_MatchingWeapons_active(rect, LimitModeSingle_Selection, highlight1, "ConsideredSidearms".Translate(), "NotConsideredSidearms".Translate()); };
            //LimitModeSingle_Absolute = Settings.GetHandle<float>("LimitModeSingle_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            //LimitModeSingle_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingle_Absolute, false, 0, maxWeightTotal, highlight1); };

        }


        private static void drawBackground(Rect rect, Color background)
        {
            Color save = GUI.color;
            GUI.color = background;
            GUI.DrawTexture(rect, TexUI.FastFillTex);
            GUI.color = save;
        }
        private static void DrawLabel(string labelText, Rect textRect, float offset)
        {
            var labelHeight = Text.CalcHeight(labelText, textRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(textRect.x, textRect.yMin - labelHeight + offset, textRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
        private static Color getColor(ThingDef weapon)
        {
            if (weapon.graphicData != null)
            {
                return weapon.graphicData.color;
            }
            return Color.white;
        }

        private static bool DrawIconForWeapon(ThingDef weapon, Rect contentRect, Vector2 iconOffset, int buttonID)
        {
            var iconTex = weapon.uiIcon;
            Graphic g = weapon.graphicData.Graphic;
            Color color = getColor(weapon);
            Color colorTwo = getColor(weapon);
            Graphic g2 = weapon.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

            //if (!contentRect.Contains(iconRect))
              //  return false;

            string label = weapon.label;

            TooltipHandler.TipRegion(iconRect, label);
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.MouseoverCommand);
            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("drawPocket", true));
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconMouseOverColor);
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("drawPocket", true));
                //Graphics.DrawTexture(iconRect, TextureResources.drawPocket, new Rect(0, 0, 1f, 1f), 0, 0, 0, 0, iconBaseColor);
            }

            Texture resolvedIcon;
            if (!weapon.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = weapon.uiIcon;
            }
            else
            {
                resolvedIcon = g2.MatSingle.mainTexture;
            }
            GUI.color = color;
            GUI.DrawTexture(iconRect, resolvedIcon);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(iconRect, true))
            {
                Event.current.button = buttonID;
                return true;
            }
            else
                return false;
        }

        private static List<ThingStuffPair> getAllWeapons()
        {
            List<ThingStuffPair> allWeaponPairs;

            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && td.canBeSpawningInventory && !td.weaponTags.NullOrEmpty<string>();

            allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
                float num2 = thingDef.generateCommonality / num;
                if (num2 != 1f)
                {
                    for (int i = 0; i < allWeaponPairs.Count; i++)
                    {
                        ThingStuffPair thingStuffPair = allWeaponPairs[i];
                        if (thingStuffPair.thing == thingDef)
                        {
                            allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
                        }
                    }
                }
            }
            return filterForType(allWeaponPairs, false);
        }
        internal static List<ThingStuffPair> filterForType(List<ThingStuffPair> list, bool allowThingDuplicates)
        {
            List<ThingStuffPair> returnList = new List<ThingStuffPair>();
            List<ThingDef> things = new List<ThingDef>();
            foreach (ThingStuffPair weapon in list)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                if ((allowThingDuplicates | !things.Contains(weapon.thing)))
                {
                    returnList.Add(weapon);
                    things.Add(weapon.thing);
                }
                
            }
            return returnList;
        }

        internal static void getHeaviestWeapons(List<ThingStuffPair> list, out float weightMelee, out float weightRanged)
        {
            weightMelee = float.MinValue;
            weightRanged = float.MinValue;
            foreach (ThingStuffPair weapon in list)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                float mass = weapon.thing.GetStatValueAbstract(StatDefOf.Mass);
                if (weapon.thing.IsRangedWeapon)
                {
                    if (mass > weightRanged)
                        weightRanged = mass;
                }
                else if (weapon.thing.IsMeleeWeapon)
                {
                    if (mass > weightMelee)
                        weightMelee = mass;
                }
            }
        }


        public static bool CustomDrawer_FloatSlider(Rect rect, SettingHandle<float> setting, bool def_isPercentage, float def_min, float def_max, Color background)
        {
            drawBackground(rect, background);
            /*if (setting.Value == null)
                setting.Value = new IntervalSliderHandler(def_isPercentage, def_value, def_min, def_max);
            if (setting.Value.setup == false)
                setting.Value = new IntervalSliderHandler(def_isPercentage, setting.Value, def_min, def_max);*/

            Rect sliderPortion = new Rect(rect);
            sliderPortion.width = sliderPortion.width - 50;
            Rect labelPortion = new Rect(rect);
            labelPortion.width = 50;
            labelPortion.position = new Vector2(sliderPortion.position.x + sliderPortion.width + 5f, sliderPortion.position.y + 4f);

            sliderPortion = sliderPortion.ContractedBy(2f);

            if (def_isPercentage)
                Widgets.Label(labelPortion, (Mathf.Round(setting.Value * 100f)).ToString("F0") + "%");
            else
                Widgets.Label(labelPortion, setting.Value.ToString("F2"));

            float val = Widgets.HorizontalSlider(sliderPortion, setting.Value, def_min, def_max, true);
            bool change = false;

            if (setting.Value != val)
                change = true;

            setting.Value = val;

            return change;
        }

        internal static bool CustomDrawer_MatchingWeapons_active(Rect wholeRect, SettingHandle<StringHashSetHandler> setting, Color background, string yesText = "Sidearms", string noText = "Not sidearms", bool excludeNeolithic = false)
        {
            drawBackground(wholeRect, background);


            GUI.color = Color.white;

            Rect leftRect = new Rect(wholeRect);
            leftRect.width = leftRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y);
            Rect rightRect = new Rect(wholeRect);
            rightRect.width = rightRect.width / 2;
            leftRect.height = wholeRect.height - TextMargin + BottomMargin;
            rightRect.position = new Vector2(rightRect.position.x + leftRect.width, rightRect.position.y);

            DrawLabel(yesText, leftRect, TextMargin);
            DrawLabel(noText, rightRect, TextMargin);

            leftRect.position = new Vector2(leftRect.position.x, leftRect.position.y + TextMargin);
            rightRect.position = new Vector2(rightRect.position.x, rightRect.position.y + TextMargin);

            int iconsPerRow = (int)(leftRect.width / (IconGap + IconSize));


            List<ThingStuffPair> allWeapons = getAllWeapons();
            allWeapons.Sort(new MassComparer());


            if (setting.Value == null)
            {
                setting.Value = new StringHashSetHandler();
                for (int i = 0; i < allWeapons.Count; i++)
                {

                        float mass = allWeapons[i].thing.GetStatValueAbstract(StatDefOf.Mass);
                        if (mass <= weightLimit)
                        {
                            setting.Value.InnerList.Add(allWeapons[i].thing.defName);
                        }
                }

            }
            HashSet<string> selection = setting.Value.InnerList;


            List<string> selectionAsList = selection.ToList();
            ThingDef[] selectionThingDefs = new ThingDef[selectionAsList.Count];

            for (int i = 0; i < allWeapons.Count; i++)
            {
                for (int j = 0; j < selectionThingDefs.Length; j++)
                {
                    if (selectionAsList[j].Equals(allWeapons[i].thing.defName))
                        selectionThingDefs[j] = allWeapons[i].thing;
                }
            }

            List<ThingStuffPair> unselectedSidearms = new List<ThingStuffPair>();
            for (int i = 0; i < allWeapons.Count; i++)
            {
                if (!selection.Contains(allWeapons[i].thing.defName))
                    unselectedSidearms.Add(allWeapons[i]);
            }

            bool change = false;

            int biggerRows = Math.Max((selection.Count - 1) / iconsPerRow, (unselectedSidearms.Count - 1) / iconsPerRow) + 1;
            setting.CustomDrawerHeight = (biggerRows * IconSize) + ((biggerRows) * IconGap) + TextMargin;

            for (int i = 0; i < selectionAsList.Count; i++)
            {
                if (selectionThingDefs[i] == null)
                {
                    continue;
                }
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForWeapon(selectionThingDefs[i], leftRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);
                if (interacted)
                {
                    change = true;
                    selection.Remove(selectionAsList[i]);
                }
            }
            for (int i = 0; i < unselectedSidearms.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForWeapon(unselectedSidearms[i].thing, rightRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);
                if (interacted)
                {
                    change = true;
                    selection.Add(unselectedSidearms[i].thing.defName);
                }
            }
            if (change)
            {
                setting.Value.InnerList = selection;
            }
            return change;
        }

    }
}
