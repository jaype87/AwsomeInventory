using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Harmony;


namespace RPG_Inventory_Remake
{
    public static class Utility_Draw
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
    }
}
