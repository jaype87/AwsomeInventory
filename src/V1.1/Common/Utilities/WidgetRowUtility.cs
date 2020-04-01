// <copyright file="WidgetRowUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Utilities for <see cref="WidgetRow"/>.
    /// </summary>
    public static class WidgetRowUtility
    {
        /// <summary>
        /// Gets maxWidth field in <see cref="WidgetRow"/>.
        /// </summary>
        public static FieldInfo MaxWidthField { get; }
            = typeof(WidgetRow).GetField("maxWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Gets startX field in <see cref="WidgetRow"/>.
        /// </summary>
        public static FieldInfo StartXField { get; }
            = typeof(WidgetRow).GetField("startX", BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo CurXField { get; }
            = typeof(WidgetRow).GetField("curX", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo IncrementPosition { get; }
            = typeof(WidgetRow).GetMethod("IncrementPosition", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Available width left in <see cref="WidgetRow"/>.
        /// </summary>
        /// <param name="row"> Instance of <see cref="WidgetRow"/>. </param>
        /// <returns> Available width left in <paramref name="row"/>. </returns>
        public static float AvailableWidth(this WidgetRow row)
        {
            return (float)MaxWidthField.GetValue(row) - (row.FinalX - (float)StartXField.GetValue(row));
        }

        /// <summary>
        /// Draw label which will be highlighted when mouse is over.
        /// </summary>
        /// <param name="widgetRow"> Helper for drawing. </param>
        /// <param name="text"> Text for label. </param>
        /// <param name="width"> Width of the label. </param>
        /// <returns> Rect in which the label is drawn. </returns>
        public static Rect LabelWithHighlight(this WidgetRow widgetRow, string text, float width = -1)
        {
            return LabelWithHighlight(widgetRow, text, null, width);
        }

        /// <summary>
        /// Draw label which will be highlighted when mouse is over.
        /// </summary>
        /// <param name="widgetRow"> Helper for drawing. </param>
        /// <param name="text"> Text for label. </param>
        /// <param name="tooltip"> Tooltip for this label when mouse is over. </param>
        /// <param name="width"> Width of the label. </param>
        /// <returns> Rect in which the label is drawn. </returns>
        public static Rect LabelWithHighlight(this WidgetRow widgetRow, string text, string tooltip, float width = -1)
        {
            Rect labelRect = widgetRow.Label(text, width);
            TooltipHandler.TipRegion(labelRect, tooltip);
            Widgets.DrawHighlightIfMouseover(labelRect);
            return labelRect;
        }

        public static void LabelScrollableHorizontal(this WidgetRow widgetRow, string text, ref Vector2 scrollPosition, float width)
        {
            Rect outRect = new Rect(widgetRow.FinalX, widgetRow.FinalY, width, GenUI.ListSpacing);
            Rect viewRect = outRect.ReplaceWidth(text.StripTags().GetWidthCached());
            float curX = widgetRow.FinalX;
            Widgets.ScrollHorizontal(outRect, ref scrollPosition, viewRect);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            widgetRow.Label(text);
            Widgets.EndScrollView();
            CurXField.SetValue(widgetRow, curX);
            IncrementPosition.Invoke(widgetRow, new object[] { width });
        }
    }
}
