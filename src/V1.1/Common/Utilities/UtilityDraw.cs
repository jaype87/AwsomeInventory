// <copyright file="UtilityDraw.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Common
{
    public static class UtilityDraw
    {
        public static Vector2 MouseDownPos;
        private static Vector2 _dragStartPos;
        public static bool isDrag;

        public static bool DrawThumbnails(PawnModal pawn, SmartRect smartRect, ThingWithComps thing, List<ThingWithComps> apparelOverflow)
        {
            // find next available rect, if not found in current row, check the row above
            Rect newRect = smartRect.NextAvailableRect();
            if (newRect == default)
            {
                if (smartRect.PreviousSibling != null)
                {
                    newRect = smartRect.PreviousSibling.NextAvailableRect();
                }
            }

            if (newRect == default)
            {
                apparelOverflow.Add(thing);
                return false;
            }
            else
            {
                GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                Utility.DrawThingRowWithImage(pawn, newRect, thing);
            }
            return true;
        }

        public static void DrawApparelToNextRow(PawnModal pawn, SmartRect smartRectHead, List<ThingWithComps> apparelOverflow)
        {
            if (apparelOverflow.Count > 0)
            {
                List<ThingWithComps> tempList = new List<ThingWithComps>(apparelOverflow);
                apparelOverflow.Clear();
                foreach (ThingWithComps apparel in tempList)
                {
                    SmartRect smartRect = smartRectHead.List.Find(r => r.BodyPartGroup.listOrder < apparel.def.apparel.bodyPartGroups[0].listOrder);
                    if (smartRect != default)
                    {
                        Rect rect = smartRect.NextAvailableRect();
                        if (rect != default)
                        {
                            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                            Utility.DrawThingRowWithImage(pawn, rect, apparel);
                        }
                        else
                        {
                            apparelOverflow.Add(apparel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw title at "position" and return next available Y as rollingY
        /// </summary>
        /// <param name="position"></param>
        /// <param name="title"></param>
        /// <param name="rollingY"></param>
        public static Rect DrawTitle(Vector2 position, string title, ref float rollingY)
        {
            Text.Font = GameFont.Medium;
            Vector2 titleSize = Text.CalcSize(title);
            Rect rectToDraw = new Rect(position, titleSize);
            Widgets.Label(rectToDraw, title);
            Text.Font = GameFont.Small;
            rollingY = rectToDraw.yMax;
            return rectToDraw;
        }

        public static void DrawLineButton<Target>(Rect rect, string label, Target target, Action<Target> action)
        {
            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            Widgets.DrawHighlightIfMouseover(rect);
            if (Widgets.ButtonInvisible(rect))
            {
                action?.Invoke(target);
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
    }
}
