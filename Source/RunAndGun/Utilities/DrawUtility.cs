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

        private static readonly Color SelectedOptionColor = new Color(0.5f, 1f, 0.5f, 1f);
        private static readonly Color constGrey = new Color(0.8f, 0.8f, 0.8f, 1f);


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

            string label = weapon.label;

            TooltipHandler.TipRegion(iconRect, label);
            MouseoverSounds.DoRegion(iconRect, SoundDefOf.MouseoverCommand);
            if (Mouse.IsOver(iconRect))
            {
                GUI.color = iconMouseOverColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("drawPocket", true));
            }
            else
            {
                GUI.color = iconBaseColor;
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("drawPocket", true));
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



        public static bool CustomDrawer_Filter(Rect rect, SettingHandle<float> slider, SettingHandle<bool> button, bool def_isPercentage, float def_min, float def_max, Color background)
        {
            drawBackground(rect, background);

            Rect sliderPortion = new Rect(rect);
            sliderPortion.width = sliderPortion.width - 120;
            Rect labelPortion = new Rect(rect);
            labelPortion.width = 50;
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

            Rect buttonRect = new Rect(rect);
            buttonRect.width = 70;
            buttonRect.position = new Vector2(labelPortion.position.x + labelPortion.width - 10f, sliderPortion.position.y);

            bool clicked = Widgets.ButtonText(buttonRect, "RG_Button_Apply".Translate());
            if (clicked)
            {
                button.Value = true;
                return true;
            }

            return change;
        }

        internal static bool CustomDrawer_MatchingWeapons_active(Rect wholeRect, SettingHandle<StringHashSetHandler> setting, Color background, string yesText = "Weapons", string noText = "Not Weapons", bool excludeNeolithic = false)
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


            List<ThingStuffPair> allWeapons = WeaponUtility.getAllWeapons();
            allWeapons.Sort(new MassComparer());


            if (setting.Value == null || Settings.applyFilter.Value == true)
            {
                setting.Value = new StringHashSetHandler();
                for (int i = 0; i < allWeapons.Count; i++)
                {

                    float mass = allWeapons[i].thing.GetStatValueAbstract(StatDefOf.Mass);
                    if (mass <= Settings.weightLimitFilter.Value)
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
            if (change)
            {
                setting.Value.InnerList = selection;
            }
            return change;
        }
    }
}
