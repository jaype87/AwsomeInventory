// <copyright file="CEDrawGearTabWorker.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Loadout;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Draws window content for CE gear tab.
    /// </summary>
    internal class CEDrawGearTabWorker : DrawGearTabWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CEDrawGearTabWorker"/> class.
        /// </summary>
        /// <param name="awesomeInventoryTab"> Gear tab to draw. </param>
        public CEDrawGearTabWorker(AwesomeInventoryTab awesomeInventoryTab)
            : base(awesomeInventoryTab)
        {
        }

        /// <inheritdoc/>
        protected override Rect SetOutRectForJealousTab(Rect canvas)
        {
            // Substract the height of two lines for a weight bar and a bulk bar.
            return canvas.ReplaceHeight(canvas.height - GenUI.ListSpacing * 2);
        }

        /// <inheritdoc/>
        protected override void DrawWeightBar(Rect rect, Pawn selPawn)
        {
            base.DrawWeightBar(rect.ReplaceHeight(GenUI.SmallIconSize), selPawn);

            List<Thing> things = new List<Thing>();
            if (selPawn.equipment?.AllEquipmentListForReading != null)
            {
                things.AddRange(selPawn.equipment.AllEquipmentListForReading);
            }

            if (selPawn.inventory?.innerContainer != null)
            {
                things.AddRange(selPawn.inventory.innerContainer);
            }

            // Draw bulk bar.
            float currentBulk = things.Sum(thing => thing.GetStatValue(CE_StatDefOf.Bulk) * thing.stackCount)
                + selPawn.apparel?.WornApparel.Sum(apparel => apparel.GetStatValue(CE_StatDefOf.WornBulk)) ?? 0;
            float carryBulk = selPawn.GetStatValue(CE_StatDefOf.CarryBulk);
            float fillPercent = Mathf.Clamp01(currentBulk / carryBulk);
            GenBar.BarWithOverlay(
            rect.ReplaceY(rect.yMax + GenUI.GapSmall)
            , fillPercent
            , SolidColorMaterials.NewSolidColorTexture(Color.Lerp(AwesomeInventoryTex.RWPrimaryColor, AwesomeInventoryTex.Valvet, fillPercent))
            , UIText.Bulk.TranslateSimple()
            , currentBulk.ToString("0.#") + "/" + carryBulk.ToString()
            , (this.DrawHelper as DrawHelperCE).BulkTextFor(selPawn));
        }

        /// <inheritdoc/>
        protected override void DrawArmorStats(WidgetRow row, Pawn pawn, bool apparelChanged)
        {
            string format = "0.#";

            this.DrawArmorStatsWorker(
                row
                , pawn
                , StatDefOf.ArmorRating_Blunt
                , (value) => value.ToString(format) + CEStrings.MPa.TranslateSimple()
                , TexResource.ArmorBlunt
                , UIText.ArmorBlunt.TranslateSimple()
                , apparelChanged
                , false);

            this.DrawArmorStatsWorker(
                row
                , pawn
                , StatDefOf.ArmorRating_Sharp
                , (value) => value.ToString(format) + CEStrings.mmRHA.TranslateSimple()
                , TexResource.ArmorSharp
                , UIText.ArmorSharp.TranslateSimple()
                , apparelChanged
                , false);

            this.DrawArmorStatsWorker(
                row
                , pawn
                , StatDefOf.ArmorRating_Heat
                , (value) => value.ToString(format) + UIText.ArmorHeat.TranslateSimple()
                , TexResource.ArmorHeat
                , UIText.ArmorHeat.TranslateSimple()
                , apparelChanged
                , true);
        }

        /// <inheritdoc/>
        protected override Tuple<float, string> GetArmorStat(Pawn pawn, StatDef stat, bool apparelChanged)
        {
            Tuple<float, string> tuple;
            if (apparelChanged)
            {
                string unit = string.Empty;
                if (stat == StatDefOf.ArmorRating_Blunt)
                    unit = CEStrings.MPa.TranslateSimple();
                else if (stat == StatDefOf.ArmorRating_Sharp)
                    unit = CEStrings.mmRHA.TranslateSimple();
                else if (stat == StatDefOf.ArmorRating_Heat)
                    unit = UIText.ArmorHeat.TranslateSimple();

                string tooltip = string.Empty;
                float value = Utility.CalculateArmorByPartsCE(pawn, stat, ref tooltip, unit);
                _statCache[stat] = tuple = Tuple.Create(value, tooltip);
            }
            else
            {
                if (!_statCache.TryGetValue(stat, out tuple))
                {
                    Log.Error("Armor stat is not initiated.");
                }
            }

            return tuple;
        }

        /// <inheritdoc/>
        protected override void DrawArmorStatsRow(WidgetRow row, Pawn pawn, StatDef stat, string label, bool apparelChanged)
        {
            ValidateArg.NotNull(row, nameof(row));

            Tuple<float, string> tuple = this.GetArmorStat(pawn, stat, apparelChanged);
            row.Label(label);
            row.Gap((WidgetRow.LabelGap * 120) - row.FinalX);

            if (stat == StatDefOf.ArmorRating_Blunt)
                row.Label(Utility.FormatArmorValue(tuple.Item1, CEStrings.MPa.TranslateSimple()));
            else if (stat == StatDefOf.ArmorRating_Sharp)
                row.Label(Utility.FormatArmorValue(tuple.Item1, CEStrings.mmRHA.TranslateSimple()));
            else if (stat == StatDefOf.ArmorRating_Heat)
                row.Label(Utility.FormatArmorValue(tuple.Item1, UIText.ArmorHeat.TranslateSimple()));

            Rect tipRegion = new Rect(0, row.FinalY, row.FinalX, WidgetRow.IconSize);
            row.Gap(int.MaxValue);

            TooltipHandler.TipRegion(tipRegion, tuple.Item2);
            Widgets.DrawHighlightIfMouseover(tipRegion);
        }
    }
}