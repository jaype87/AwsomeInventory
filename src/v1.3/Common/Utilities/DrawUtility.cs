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
        public static readonly float TwentyCharsWidth = UIText.TenCharsString.Times(2f).GetWidthCached();

        public static Vector2 MouseDownPos;
        public static bool isDrag;
        private static Vector2 _dragStartPos;
        private static QualityColor _qualityColor;

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
        /// Gets window padding used by vanilla.
        /// </summary>
        public static float WindowPadding => 18f;

        /// <summary>
        /// Draw a label which doubles as a button.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="label"> Label to draw in <paramref name="rect"/>. </param>
        /// <param name="action"> Action to take when it is clicked. </param>
        public static void DrawLabelButton(Rect rect, string label, Action action, TextAnchor textAnchor = TextAnchor.MiddleLeft)
        {
            DrawLabelButton(rect, label, action, false, textAnchor: textAnchor);
        }

        /// <summary>
        /// Draw a label which doubles as a button.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="label"> Label to draw in <paramref name="rect"/>. </param>
        /// <param name="action"> Action to take when it is clicked. </param>
        /// <param name="toggleable"> Indicates if button can be toggled and uses a selected texture if true. </param>
        public static void DrawLabelButton(Rect rect, string label, Action action, bool toggleable, bool drawHighlight = true, TextAnchor textAnchor = TextAnchor.MiddleLeft)
        {
            Text.WordWrap = false;
            Text.Anchor   = textAnchor;

            Widgets.Label(rect, label);
            if (toggleable && drawHighlight)
            {
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlightSelected(rect);
            }
            else if (drawHighlight)
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
                    return s.Colorize(QualityColor.Instance.Awful);
                case QualityCategory.Poor:
                    return s.Colorize(QualityColor.Instance.Poor);
                case QualityCategory.Normal:
                    return s.Colorize(QualityColor.Instance.Normal);
                case QualityCategory.Good:
                    return s.Colorize(QualityColor.Instance.Good);
                case QualityCategory.Excellent:
                    return s.Colorize(QualityColor.Instance.Excellent);
                case QualityCategory.Masterwork:
                    return s.Colorize(QualityColor.Instance.Masterwork);
                case QualityCategory.Legendary:
                    return s.Colorize(QualityColor.Instance.Legendary);
            }

            return s;
        }

        /// <summary>
        /// Get the index range for a list whose content will be rendered on screen.
        /// </summary>
        /// <param name="viewRectLength"> The length of view rect. </param>
        /// <param name="scrollPosition"> Scroll position for the list view. </param>
        /// <param name="from"> Start index of a list where drawing begins. </param>
        /// <param name="to"> <paramref name="to"/> is positioned at one element behind the index where drawing should stop. </param>
        /// <param name="unitLength"> The length of a unit elemnt in the list. </param>
        public static void GetIndexRangeFromScrollPosition(float viewRectLength, float scrollPosition, out int from, out int to, float unitLength)
        {
            from = Mathf.FloorToInt(scrollPosition / unitLength);
            to = from + (int)Math.Ceiling(viewRectLength / unitLength);
        }

        public static void DrawMouseAttachmentWithThing(this ThingDef thingDef, ThingDef stuffDef)
        {
            if (!(thingDef.uiIcon == null) && !(thingDef.uiIcon == BaseContent.BadTex))
            {
                Color color;

                if (stuffDef != null)
                    color = thingDef.GetColorForStuff(stuffDef);
                else
                    color = thingDef.MadeFromStuff ? thingDef.GetColorForStuff(GenStuff.DefaultStuffFor(thingDef)) : thingDef.uiIconColor;

                Rect dragRect = new Rect(Verse.UI.GUIToScreenPoint(Event.current.mousePosition) + new Vector2(GenUI.SmallIconSize / 2, 0), new Vector2(GenUI.SmallIconSize, GenUI.SmallIconSize));
                Find.WindowStack.ImmediateWindow(
                    Rand.Int,
                    dragRect,
                    WindowLayer.Super,
                    () =>
                    {
                        GUI.color = color;
                        GUI.DrawTexture(dragRect.AtZero(), thingDef.uiIcon);
                        GUI.color = Color.white;
                    }, false);
            }
        }

        public static void TextPosition(TextAnchor anchor, Action action)
        {
            TextAnchor old = Text.Anchor;
            Text.Anchor = anchor;
            action?.Invoke();
            Text.Anchor = old;
        }

        public static T TextPosition<T>(TextAnchor anchor, Func<T> func)
        {
            ValidateArg.NotNull(func, nameof(func));

            TextAnchor old = Text.Anchor;
            Text.Anchor = anchor;
            T result = func.Invoke();
            Text.Anchor = old;

            return result;
        }
    }
}
