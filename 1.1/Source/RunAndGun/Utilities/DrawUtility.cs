using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RunAndGun.Utilities
{
    class DrawUtility
    {
        private const float ContentPadding = 5f;
        private const float IconSize = 32f;
        private const float IconGap = 1f;
        private const float TextMargin = 20f;
        private const float BottomMargin = 2f;

        private static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);
        private static Color background = new Color(0.5f, 0, 0, 0.1f);

        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static Color exceptionBackground = new Color(0f, 0.5f, 0, 0.1f);


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




        private static bool DrawIconForWeapon(ThingDef weapon, KeyValuePair<String, WeaponRecord> item, Rect contentRect, Vector2 iconOffset, int buttonID)
        {

            var iconTex = weapon.uiIcon;
            Color color = getColor(weapon);
            Color colorTwo = getColor(weapon);
            Graphic g2 = null;

            if (weapon.graphicData != null && weapon.graphicData.Graphic != null)
            {
                Graphic g = weapon.graphicData.Graphic;
                g2 = weapon.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);
            }

            var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

            if (!contentRect.Contains(iconRect))
                return false;

            string label = weapon.label;

            TooltipHandler.TipRegion(iconRect, label);

            MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("square", true));
            }
            else if(item.Value.isException == true){
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("square", true));
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("square", true));
            }

            Texture resolvedIcon;
            if (!weapon.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = weapon.uiIcon;
            }
            else if (g2 != null)
            {
                resolvedIcon = g2.MatSingle.mainTexture;
            }
            else
            {
                resolvedIcon = new Texture2D(0,0);
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



        public static bool CustomDrawer_Filter(Rect rect, SettingHandle<float> slider, bool def_isPercentage, float def_min, float def_max, Color background)
        {
            drawBackground(rect, background);
            int labelWidth = 50;

            Rect sliderPortion = new Rect(rect);
            sliderPortion.width = sliderPortion.width - labelWidth;

            Rect labelPortion = new Rect(rect);
            labelPortion.width = labelWidth;
            labelPortion.position = new Vector2(sliderPortion.position.x + sliderPortion.width + 5f, sliderPortion.position.y + 4f);

            sliderPortion = sliderPortion.ContractedBy(2f);

            if (def_isPercentage)
                Widgets.Label(labelPortion, (Mathf.Round(slider.Value * 100f)).ToString("F0") + "%");
            else
                Widgets.Label(labelPortion, slider.Value.ToString("F2"));

            float val = Widgets.HorizontalSlider(sliderPortion, slider.Value, def_min, def_max, true);
            bool change = false;

            if (slider.Value != val)
                change = true;

            slider.Value = val;
            return change;
        }

        public static bool CustomDrawer_Tabs(Rect rect, SettingHandle<String> selected, String[] defaultValues)
        {
            int labelWidth = 140;
            int offset = 0;
            bool change = false;

            foreach (String tab in defaultValues)
            {
                Rect buttonRect = new Rect(rect);
                buttonRect.width = labelWidth;
                buttonRect.position = new Vector2(buttonRect.position.x + offset, buttonRect.position.y);
                Color activeColor = GUI.color;
                bool isSelected = tab == selected.Value;
                if (isSelected)
                    GUI.color = SelectedOptionColor;
                bool clicked = Widgets.ButtonText(buttonRect, tab);
                if (isSelected)
                    GUI.color = activeColor;

                if (clicked)
                {
                    if (selected.Value != tab)
                    {
                        selected.Value = tab;
                    }
                    else
                    {
                        selected.Value = "none";
                    }
                    change = true;
                }

                offset += labelWidth;

            }
            return change;
        }


        //This needs to be refactored, I don't like the mixing of the forbid weapons setting and the matching weapons setting with a filter. 
        internal static void filterWeapons(ref SettingHandle<DictWeaponRecordHandler> setting, List<ThingDef> allWeapons, SettingHandle<float> filter = null)
        {
            if (setting.Value == null)
            {
                setting.Value = new DictWeaponRecordHandler();
            }

            Dictionary<String, WeaponRecord> selection = new Dictionary<string, WeaponRecord>();
            foreach (ThingDef weapon in allWeapons)
            {
                bool shouldSelect = false;
                if (filter != null)
                {
                    float mass = weapon.GetStatValueAbstract(StatDefOf.Mass);
                    shouldSelect = mass >= filter.Value;
                }
                WeaponRecord value = null;
                bool found = setting.Value.InnerList.TryGetValue(weapon.defName, out value);
                if (found && value.isException)
                {
                    selection.Add(weapon.defName, value);
                }
                else
                {
                    bool weaponDefaultForbidden = weapon.GetModExtension<DefModExtension_SettingDefaults>() is DefModExtension_SettingDefaults modExt && modExt.weaponForbidden;

                    shouldSelect = filter == null ? weaponDefaultForbidden : shouldSelect;
                    selection.Add(weapon.defName, new WeaponRecord(shouldSelect, false, weapon.label));
                }

            }
            selection = selection.OrderBy(d => d.Value.label).ToDictionary(d => d.Key, d => d.Value);
            setting.Value.InnerList = selection;
        }


        internal static bool CustomDrawer_MatchingWeapons_active(Rect wholeRect, SettingHandle<DictWeaponRecordHandler> setting, List<ThingDef> allWeapons, SettingHandle<float> filter = null, string yesText = "Light weapon", string noText = "Heavy weapon")
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
            bool change = false;
            int numSelected = 0;

            filterWeapons(ref setting, allWeapons, filter);
            Dictionary<string, WeaponRecord> selection = setting.Value.InnerList;

            foreach (KeyValuePair<String, WeaponRecord> item in selection)
            {
                if (item.Value.isSelected)
                {
                    numSelected++;
                }
            }


            int biggerRows = Math.Max(numSelected / iconsPerRow, (selection.Count - numSelected) / iconsPerRow) + 1;
            setting.CustomDrawerHeight = (biggerRows * IconSize) + (biggerRows * IconGap) + TextMargin;
            Dictionary<String, ThingDef> allWeaponsDict = allWeapons.ToDictionary(o => o.defName, o => o);
            int indexLeft = 0;
            int indexRight = 0;
            foreach (KeyValuePair<String, WeaponRecord> item in selection)
            {
                Rect rect = item.Value.isSelected ? rightRect : leftRect;
                int index = item.Value.isSelected ? indexRight: indexLeft;
                if (item.Value.isSelected)
                {
                    indexRight++;
                }
                else
                {
                    indexLeft++;
                }

                int collum = (index % iconsPerRow);
                int row = (index / iconsPerRow);
                ThingDef weapon = null;
                bool found = allWeaponsDict.TryGetValue(item.Key, out weapon);
                bool interacted = false;
                if (found)
                {
                    interacted = DrawIconForWeapon(weapon, item, rect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), index);
                }
                if (interacted)
                {
                    change = true;
                    item.Value.isSelected = !item.Value.isSelected;
                    item.Value.isException = !item.Value.isException;
                }
            }

            /*
            if (setting.Value == null || (Settings.applyFilter.Value == true))
            {
                setting.Value = new DictWeaponRecordHandler();
                for (int i = 0; i < allWeapons.Count; i++)
                {

                    float mass = allWeapons[i].thing.GetStatValueAbstract(StatDefOf.Mass);
                    if (mass <= Settings.weightLimitFilter.Value || !shouldFilter)
                    {
                        setting.Value.InnerList.Add(allWeapons[i].thing.defName);
                    }
                }
                Settings.applyFilter.Value = false;

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

            List<ThingStuffPair> unselectedWeapons = new List<ThingStuffPair>();
            for (int i = 0; i < allWeapons.Count; i++)
            {
                if (!selection.Contains(allWeapons[i].thing.defName))
                    unselectedWeapons.Add(allWeapons[i]);
            }

            bool change = false;

            int biggerRows = Math.Max((selection.Count - 1) / iconsPerRow, (unselectedWeapons.Count - 1) / iconsPerRow) + 1;
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
            for (int i = 0; i < unselectedWeapons.Count; i++)
            {
                int collum = (i % iconsPerRow);
                int row = (i / iconsPerRow);
                bool interacted = DrawIconForWeapon(unselectedWeapons[i].thing, rightRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap), i);

                if (interacted)
                {
                    change = true;
                    selection.Add(unselectedWeapons[i].thing.defName);
                }
            }
            */
            if (change)
            {
                setting.Value.InnerList = selection;
            }
            return change;
        }
    }
}
