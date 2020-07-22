// <copyright file="AwesomeInventoryMod.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using AwesomeInventory.UI;
using UnityEngine;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// A dialog window for configuring mod settings.
    /// </summary>
    public class AwesomeInventoryMod : Mod
    {
        private static Vector2 _qualityColorScrollListHeight;
        private static Rect _qualityColorViewRect;
        private static List<QualityColor> _themes;

        private static AwesomeInventorySetting _settings;

        public static string BugReportUrl { get; } = "https://steamcommunity.com/workshop/filedetails/discussion/2050289408/2145343824305183281/";

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryMod"/> class.
        /// </summary>
        /// <param name="content"> Includes metadata of a mod. </param>
        public AwesomeInventoryMod(ModContentPack content)
            : base(content)
        {
            _settings = this.GetSettings<AwesomeInventorySetting>();
        }

        /// <summary>
        /// Gets setting for Awesome inventory.
        /// </summary>
        public static AwesomeInventorySetting Settings
        {
            get => _settings;
        }

        /// <summary>
        /// Draw mod settings.
        /// </summary>
        /// <param name="inRect"> Rect for drawing. </param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.ColumnWidth = inRect.width / 3 - 20f;
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled(UIText.UseLoadout.TranslateSimple(), ref _settings.UseLoadout, UIText.UseLoadoutTooltip.TranslateSimple());
            listingStandard.CheckboxLabeled(UIText.OpenLoadoutFromContextMenu.TranslateSimple(), ref _settings.OpenLoadoutInContextMenu, UIText.OpenLoadoutFromContextMenuTooltip.TranslateSimple());
            listingStandard.CheckboxLabeled(UIText.PatchAllRaces.TranslateSimple(), ref _settings.PatchAllRaces, UIText.PatchAllRacesTooltip.TranslateSimple());

            if (CombatExtendedUtility.IsActive)
                _settings.AutoEquipWeapon = false;
            else
                listingStandard.CheckboxLabeled(UIText.AutoEquipWeapon.TranslateSimple(), ref _settings.AutoEquipWeapon, UIText.AutoEquipWeaponTooltip.TranslateSimple());

            listingStandard.CheckboxLabeled(UIText.UseGearTabToggle.TranslateSimple(), ref _settings.UseToggleGizmo, UIText.UseGearTabToggleTooltip.TranslateSimple());

            listingStandard.CheckboxLabeled(UIText.UseHotSwap.TranslateSimple(), ref _settings.UseHotSwap, string.Empty);

            listingStandard.CheckboxLabeled(UIText.UseTakeDrug.TranslateSimple(), ref _settings.UseTakeDrugs, UIText.UseTakeDrugTooltip.TranslateSimple());

            listingStandard.CheckboxLabeled(UIText.ShowRestartButton.TranslateSimple(), ref _settings.ShowRestartButton, UIText.ShowRestartButtonTooltip.TranslateSimple());

            listingStandard.NewColumn();

            this.DrawQualityColorScrollableList(listingStandard);
            listingStandard.Gap();

            DrawGearTabSizeSetting(listingStandard);
            listingStandard.Gap();

            listingStandard.NewColumn();
            DrawHelpUrl(listingStandard);

            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Return the name for display in the game's mod setting section.
        /// </summary>
        /// <returns> Display name for Awesome Inventory. </returns>
        public override string SettingsCategory()
        {
            return UIText.AwesomeInventoryDisplayName.TranslateSimple();
        }

        private static void DrawHelpUrl(Listing_Standard listingStandard)
        {
            Rect labelRect = listingStandard.GetRect(GenUI.ListSpacing);
            DrawUrl(
                labelRect
                , UIText.ExplainFeatures.TranslateSimple()
                , @"https://github.com/Mhburg/AwsomeInventory/wiki/Explanations-on-Loadout-Costume-feature");

            labelRect = listingStandard.GetRect(GenUI.ListSpacing);
            DrawUrl(
                labelRect
                , UIText.TipsOnCostume.TranslateSimple()
                , @"https://github.com/Mhburg/AwsomeInventory/wiki/How-to-use-costume");

            labelRect = listingStandard.GetRect(GenUI.ListSpacing);
            DrawUrl(
                labelRect
                , UIText.TipsForHotSwap.TranslateSimple()
                , @"https://github.com/Mhburg/AwsomeInventory/wiki/Hot-Swap-Costume");
        }

        private static void DrawUrl(Rect labelRect, string text, string url)
        {
            Widgets.Label(labelRect, text.Colorize(ColorLibrary.SkyBlue));
            if (Mouse.IsOver(labelRect))
            {
                Vector2 size = Text.CalcSize(text);
                Widgets.DrawLine(
                    new Vector2(labelRect.x, labelRect.y + size.y)
                    , new Vector2(labelRect.x + size.x, labelRect.y + size.y)
                    , ColorLibrary.SkyBlue, 1);
            }

            if (Widgets.ButtonInvisible(labelRect))
            {
                Application.OpenURL(url);
            }
        }

        private static string TooltipForQualityColor(QualityColor qualityColor)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Legendary".Colorize(qualityColor.Legendary));
            stringBuilder.AppendLine("Masterwork".Colorize(qualityColor.Masterwork));
            stringBuilder.AppendLine("Excellent".Colorize(qualityColor.Excellent));
            stringBuilder.AppendLine("Good".Colorize(qualityColor.Good));
            stringBuilder.AppendLine("Normal".Colorize(qualityColor.Normal));
            stringBuilder.AppendLine("Poor".Colorize(qualityColor.Poor));
            stringBuilder.AppendLine("Awful".Colorize(qualityColor.Awful));
            stringBuilder.AppendLine("Generic".Colorize(qualityColor.Generic));

            return stringBuilder.ToString();
        }

        private static void DrawGearTabSizeSetting(Listing_Standard listingStandard)
        {
            Text.Font = GameFont.Medium;
            listingStandard.Label(UIText.GoNuts.TranslateSimple());
            Text.Font = GameFont.Small;
            listingStandard.GapLine();
            Rect rect = listingStandard.GetRect(GenUI.ListSpacing);
            string widthBuffer = _settings.GearTabWidth.ToString(), heightBuffer = _settings.GearTabHeight.ToString();

            Widgets.Label(rect, UIText.GearTabWidth.TranslateSimple());
            rect.Set(rect.x + UIText.GearTabWidth.TranslateSimple().GetWidthCached() + GenUI.GapTiny, rect.y, GenUI.SmallIconSize * 2, GenUI.SmallIconSize);
            Widgets.TextFieldNumeric(rect, ref _settings.GearTabWidth, ref widthBuffer);

            rect = listingStandard.GetRect(GenUI.ListSpacing);

            Widgets.Label(rect, UIText.GearTabHeight.TranslateSimple());
            rect.Set(rect.x + UIText.GearTabHeight.TranslateSimple().GetWidthCached() + GenUI.GapTiny, rect.y, GenUI.SmallIconSize * 2, GenUI.SmallIconSize);
            Widgets.TextFieldNumeric(rect, ref _settings.GearTabHeight, ref heightBuffer);
        }

        private void DrawQualityColorScrollableList(Listing_Standard listingStandard)
        {
            Rect labelRect = listingStandard.Label(UIText.ChooseThemeColorForQuality.TranslateSimple());
            Widgets.DrawLineHorizontal(labelRect.x, listingStandard.CurHeight, labelRect.width - GenUI.ScrollBarWidth);

            Rect outRect = listingStandard.GetRect(GenUI.ListSpacing * 4);

            if (_themes == null)
            {
                _themes = AwesomeInventoryServiceProvider.Plugins.Values.OfType<QualityColor>().ToList();
                float height = _themes.Count * GenUI.ListSpacing;
                _qualityColorViewRect = outRect.ReplaceHeight(Mathf.Max(outRect.height + GenUI.ListSpacing, height));
            }

            Widgets.BeginScrollView(outRect, ref _qualityColorScrollListHeight, _qualityColorViewRect.AtZero());

            float rollingY = 0;
            Rect optionRect = new Rect(0, 0, _qualityColorViewRect.width - GenUI.ScrollBarWidth, GenUI.ListSpacing);
            Text.Anchor = TextAnchor.MiddleCenter;
            foreach (QualityColor qualityColor in _themes)
            {
                optionRect = optionRect.ReplaceY(rollingY);
                if (Widgets.RadioButtonLabeled(optionRect, qualityColor.DisplayName, _settings.QualityColorPluginID == qualityColor.ID))
                {
                    _settings.QualityColorPluginID = qualityColor.ID;
                    QualityColor.ChangeTheme(qualityColor.ID);
                }

                Widgets.DrawHighlightIfMouseover(optionRect);
                TooltipHandler.TipRegion(optionRect, TooltipForQualityColor(qualityColor));

                rollingY += GenUI.ListSpacing;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndScrollView();
        }
    }
}
