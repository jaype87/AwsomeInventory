// <copyright file="CEDrawGearTabWorker.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

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
            Log.Warning("DrawWeightBarBase: " + rect.width);
            base.DrawWeightBar(rect, selPawn);

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
            Log.Warning("DrawWeightBar: " + rect.width);
            GenBar.BarWithOverlay(
            rect.ReplaceY(rect.yMax + GenUI.GapSmall)
            , fillPercent
            , SolidColorMaterials.NewSolidColorTexture(Color.Lerp(AwesomeInventoryTex.RWPrimaryColor, AwesomeInventoryTex.Valvet, fillPercent))
            , UIText.Bulk.TranslateSimple()
            , currentBulk.ToString("0.#") + "/" + carryBulk.ToString()
            , (this.DrawHelper as DrawHelperCE).BulkTextFor(selPawn));
        }
    }
}