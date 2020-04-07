// <copyright file="CEGearTabWorker.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Draws window content for CE gear tab.
    /// </summary>
    internal class CEGearTabWorker : DrawGearTabWorker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CEGearTabWorker"/> class.
        /// </summary>
        /// <param name="awesomeInventoryTab"> Gear tab to draw. </param>
        public CEGearTabWorker(AwesomeInventoryTab awesomeInventoryTab)
            : base(awesomeInventoryTab)
        {
        }

        /// <inheritdoc/>
        protected override Rect SetOutRectForJealousTab(Rect canvas)
        {
            // Substract the height of two lines for a weight bar and a bulk bar.
            return canvas.ReplaceHeight(canvas.height - GenUI.ListSpacing * 2);
        }

        protected override void DrawWeightBar(Rect rect, Pawn selPawn)
        {
            base.DrawWeightBar(rect, selPawn);
            GenBar.BarWithOverlay(
                rect.ReplaceHeight(rect.yMax),
                
                )
        }
    }
}