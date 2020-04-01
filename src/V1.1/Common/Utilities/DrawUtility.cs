// <copyright file="DrawUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    public static class DrawUtility
    {
        public const float TinyIconSize = 20f;

        public static readonly Color HighlightGreen = new Color(134 / 255f, 206 / 255f, 0, 1);
        public static readonly Color HighlightBrown = new Color(212 / 255f, 141 / 255f, 0, 1);
        public static readonly float TwentyCharsWidth = UIText.TenCharsString.Times(2f).GetWidthCached();

        public static Vector2 MouseDownPos;
        public static bool isDrag;
        private static Vector2 _dragStartPos;

        /// <summary>
        /// Gets padding between the border of window and its content.
        /// </summary>
        public static float CurrentPadding
        {
            get
            {
                return Text.CurFontStyle.margin.top + Text.CurFontStyle.padding.top;
            }
        }

        /// <summary>
        /// Draw a lable which doubles as a button.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="label"> Label to draw in <paramref name="rect"/>. </param>
        /// <param name="action"> Action to take when it is clicked. </param>
        public static void DrawLabelButton(Rect rect, string label, Action action)
        {
            DrawLabelButton(rect, label, action, false);
        }

        /// <summary>
        /// Draw a lable which doubles as a button.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="label"> Label to draw in <paramref name="rect"/>. </param>
        /// <param name="action"> Action to take when it is clicked. </param>
        /// <param name="toggleable"> Indicates if button can be toggled and uses a selected texture if true. </param>
        public static void DrawLabelButton(Rect rect, string label, Action action, bool toggleable)
        {
            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            if (toggleable)
            {
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlightSelected(rect);
            }
            else
            {
                Widgets.DrawHighlightIfMouseover(rect);
            }

            if (Widgets.ButtonInvisible(rect))
            {
                action?.Invoke();
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
        }

        /// <summary>
        /// Return button size based on the provided rect
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="scale"> size of the button relative to rect </param>
        /// <param name="ratio"> ratio of the button's length to width. Default is golden ratio. </param>
        /// <param name="vertical">direction for the length of the button </param>
        /// <returns></returns>
        public static Vector2 GetButtonSize(Rect rect, float scale, float ratio = 1.62f, bool vertical = false)
        {
            if (rect == null)
            {
                throw new ArgumentNullException(nameof(rect));
            }
            if (scale <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }
            double size = rect.width * rect.height * scale;
            float width = (float)Math.Sqrt(size / ratio);
            float length = width * ratio;

            if (vertical)
            {
                return new Vector2(width, length);
            }
            return new Vector2(length, width);
        }

        public static Vector2 GetStartPositionForDrag(Rect realRect, Vector2 listMin, float index, float rowHeight)
        {
            float x = realRect.xMin + listMin.x;
            float y = realRect.yMin + listMin.y + index * rowHeight;
            return new Vector2(x, y);
        }

        public static Vector2 GetPostionForDrag(Rect realRect, Vector2 listMin, float index, float rowHeight)
        {
            if (!isDrag)
            {
                isDrag = true;
                MouseDownPos = Event.current.mousePosition;
                _dragStartPos = GetStartPositionForDrag(realRect, listMin, index, rowHeight);
                return _dragStartPos;
            }
            else
            {
                return _dragStartPos + (Event.current.mousePosition - MouseDownPos);
            }
        }

        public static void ResetDrag()
        {
            isDrag = false;
            MouseDownPos = Vector2.zero;
            _dragStartPos = Vector2.zero;
        }

        public static void DrawBoxWithColor(Rect rect, Texture texture, int thickness = 1)
        {
            Vector2 b = new Vector2(rect.x, rect.y);
            Vector2 a = new Vector2(rect.x + rect.width, rect.y + rect.height);
            if (b.x > a.x)
            {
                float x = b.x;
                b.x = a.x;
                a.x = x;
            }

            if (b.y > a.y)
            {
                float y = b.y;
                b.y = a.y;
                a.y = y;
            }

            Vector3 vector = a - b;
            GUI.DrawTexture(new Rect(b.x, b.y, thickness, vector.y), texture);
            GUI.DrawTexture(new Rect(a.x - (float)thickness, b.y, thickness, vector.y), texture);
            GUI.DrawTexture(new Rect(b.x + (float)thickness, b.y, vector.x - (float)(thickness * 2), thickness), texture);
            GUI.DrawTexture(new Rect(b.x + (float)thickness, a.y - (float)thickness, vector.x - (float)(thickness * 2), thickness), texture);
        }

        /// <summary>
        /// Color <paramref name="s"/> based on <paramref name="thing"/> quality.
        /// </summary>
        /// <param name="s"> String to color. </param>
        /// <param name="thing"> Thing with quality. </param>
        /// <returns> Colored string. </returns>
        public static string ColorizeByQuality(this string s, Thing thing)
        {
            if (thing.TryGetQuality(out QualityCategory qualityCategory))
            {
                s = s.ColorizeByQuality(qualityCategory);
            }

            return s;
        }

        /// <summary>
        /// Color <paramref name="s"/> based on <paramref name="qualityCategory"/>.
        /// </summary>
        /// <param name="s"> String to color. </param>
        /// <param name="qualityCategory"> Quality value. </param>
        /// <returns> Colored string. </returns>
        public static string ColorizeByQuality(this string s, QualityCategory qualityCategory)
        {
            switch (qualityCategory)
            {
                case QualityCategory.Awful:
                    return s.Colorize(GenColor.FromHex("cc1a00"));
                case QualityCategory.Poor:
                    return s.Colorize(GenColor.FromHex("b0b3af"));
                case QualityCategory.Normal:
                    return s.Colorize(Color.white);
                case QualityCategory.Good:
                    return s.Colorize(ColorLibrary.LightGreen);
                case QualityCategory.Excellent:
                    return s.Colorize(ColorLibrary.Cyan);
                case QualityCategory.Masterwork:
                    return s.Colorize(ColorLibrary.Sand);
                case QualityCategory.Legendary:
                    return s.Colorize(ColorLibrary.Orange);
            }

            return s;
        }

        /// <summary>
        /// Get the index range for a list whose content will be rendered on screen.
        /// </summary>
        /// <param name="totalLength"> The length of a scrollable list. </param>
        /// <param name="scrollPosition"> Scroll position for the list view. </param>
        /// <param name="from"> Start index of a list where drawing begins. </param>
        /// <param name="to"> <paramref name="to"/> is positioned at one element behind the index where drawing should stop. </param>
        /// <param name="unitLength"> The length of a unit elemnt in the list. </param>
        public static void GetIndexRangeFromScrollPosition(float totalLength, float scrollPosition, out int from, out int to, float unitLength)
        {
            from = Mathf.FloorToInt(scrollPosition / unitLength);
            to = from + (int)Math.Ceiling(totalLength / unitLength);
        }
    }
}
