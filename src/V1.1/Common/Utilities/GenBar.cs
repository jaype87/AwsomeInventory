// <copyright file="GenBar.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Provides generic bar utilities.
    /// </summary>
    public static class GenBar
    {
        /// <summary>
        /// Draw bar with label and overlay text.
        /// </summary>
        /// <param name="rect"> Position to draw. </param>
        /// <param name="fillPercent"> How much the bar is filled. </param>
        /// <param name="fillTex"> Texture for filler. </param>
        /// <param name="label"> Label to prepend to the bar. </param>
        /// <param name="overlayText"> Text to draw over the bar. </param>
        /// <param name="tooltip"> A breakdown of weight carried by pawn. </param>
        /// <returns> Rect used for drawing bar. </returns>
        public static Rect BarWithOverlay(Rect rect, float fillPercent, Texture2D fillTex, string label, string overlayText, string tooltip)
        {
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect labelRect = new Rect(rect)
            {
                width = Text.CalcSize(label).x,
            };
            Widgets.Label(labelRect, label);

            rect.xMin += labelRect.width + WidgetRow.DefaultGap;
            Widgets.FillableBar(rect, fillPercent, fillTex, BaseContent.BlackTex, false);

            if (Mouse.IsOver(rect) && !tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(rect, tooltip);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, overlayText);
            Text.Anchor = TextAnchor.UpperLeft;

            return rect;
        }
    }
}
