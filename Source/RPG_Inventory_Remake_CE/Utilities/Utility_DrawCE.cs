using CombatExtended;
using Verse;
using UnityEngine;
using RimWorld;

namespace RPG_Inventory_Remake_CE
{
    public class Utility_DrawCE
    {
        public static void DrawBulkBreakdown(Pawn pawn, Rect rect)
        {
            string text = "";
            float value;
            foreach (ThingWithComps eq in pawn.equipment.AllEquipmentListForReading)
            {
                value = eq.GetStatValue(CE_StatDefOf.Bulk) * eq.stackCount;
                if (value > 0.1)
                {
                    text += string.Concat(eq.LabelShortCap, ": ", value, "\n");
                }
            }
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                value = apparel.GetStatValue(CE_StatDefOf.Bulk) * apparel.stackCount;
                if (value > 0.1)
                {
                    text += string.Concat(apparel.LabelShortCap, ": ", value, "\n");
                }
            }
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                value = thing.GetStatValue(CE_StatDefOf.Bulk) * thing.stackCount;
                if (value > 0.1)
                {
                    text += string.Concat(thing.LabelShortCap, ": ", value, "\n");
                }
            }
            TooltipHandler.TipRegion(rect, text);
        }
    }
}
