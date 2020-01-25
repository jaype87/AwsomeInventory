using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Harmony;


namespace RPG_Inventory_Remake_Common
{
    public static class UtilityDraw
    {
        public static bool DrawThumbnails(RPG_Pawn pawn, SmartRect smartRect, ThingWithComps thing, List<ThingWithComps> apparelOverflow)
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

        public static void DrawApparelToNextRow(RPG_Pawn pawn, SmartRect smartRectHead, List<ThingWithComps> apparelOverflow)
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
        public static void DrawTitle(Vector2 position, string title, ref float rollingY)
        {
            Text.Font = GameFont.Medium;
            Vector2 titleSize = Text.CalcSize(title);
            //position.x += GenUI.GapSmall;
            //position.y += GenUI.GapSmall;
            Rect rectToDraw = new Rect(position, titleSize);
            Widgets.Label(rectToDraw, title);
            Text.Font = GameFont.Small;
            rollingY = rectToDraw.yMax;
        }

        public static void DrawLineButton<Target>(Rect rect, string label, Target target, Action<Target> action)
        {
            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(rect, label);
            Widgets.DrawHighlightIfMouseover(rect);
            if (Widgets.ButtonInvisible(rect))
            {
                action(target);
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
    }
}
