// <copyright file="Dialog_ManageLoadoutCE.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <inheritdoc/>
    public class Dialog_ManageLoadoutCE : Dialog_ManageLoadouts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_ManageLoadoutCE"/> class.
        /// </summary>
        /// <param name="loadout"> Loadout to display. </param>
        /// <param name="pawn"> Pawn who wears the <paramref name="loadout"/>. </param>
        public Dialog_ManageLoadoutCE(AwesomeInventoryLoadout loadout, Pawn pawn)
            : base(loadout, pawn)
        {
        }

        /// <summary>
        /// Returns a rect for drawing weight bar and bulk bar.
        /// </summary>
        /// <param name="canvas"> Canvas at which bottom two bars will be drawn. </param>
        /// <returns> A rect where two bars are drawn. </returns>
        protected override Rect GetWeightRect(Rect canvas)
        {
            canvas.Set(canvas.x, canvas.yMax - DrawUtility.CurrentPadding - GenUI.ListSpacing * 2, canvas.width, GenUI.ListSpacing * 2);
            return canvas;
        }

        /// <summary>
        /// Draw weight bar and bulk bar.
        /// </summary>
        /// <param name="canvas"> Rect for drawing weight bar and bulk bar. </param>
        protected override void DrawWeightBar(Rect canvas)
        {
            base.DrawWeightBar(canvas.ReplaceHeight(GenUI.SmallIconSize));

            float currentBulk = _currentLoadout.GroupBy(
                groupSelector => groupSelector.AllowedThing
                , groupSelector => groupSelector.AllowedStackCount)
                .Sum(
                    group => group.Key.IsApparel
                                     ? BulkUtility.BulkFor(group.Key, null, true)
                                       + BulkUtility.BulkFor(group.Key, null, false) * (group.Sum() - 1)
                                     : BulkUtility.BulkFor(group.Key, null, false) * group.Sum());
            float capaticyBulk = MassBulkUtility.BaseCarryBulk(_pawn)
                                 +
                                 _currentLoadout.Select(selector => selector.AllowedThing).Distinct().Sum(
                                     thingDef => thingDef.equippedStatOffsets.GetStatOffsetFromList(CE_StatDefOf.CarryBulk));

            float fillPercent = Mathf.Clamp01(currentBulk / capaticyBulk);

            canvas.Set(canvas.x, canvas.y + GenUI.ListSpacing + GenUI.GapTiny, canvas.width, GenUI.ListSpacing);
            GenBar.BarWithOverlay(
                canvas
                , fillPercent
                , SolidColorMaterials.NewSolidColorTexture(Color.Lerp(AwesomeInventoryTex.RWPrimaryColor, AwesomeInventoryTex.Valvet, fillPercent))
                , UIText.Bulk.TranslateSimple()
                , currentBulk.ToString("0.#") + "/" + capaticyBulk.ToString("0.#")
                , string.Empty);
        }
    }
}
