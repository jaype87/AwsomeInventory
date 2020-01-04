using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG_Inventory_for_CE
{
    /// <summary>
    /// Used for storing backup functions
    /// </summary>
    internal class Freezer
    {
        /// <summary>
        ///     Draw overall armor in a format as "Sharp   100 Rha", wihout the double quotes
        /// </summary>
        /// <param name="selPawn"></param>
        /// <param name="curY"></param>
        /// <param name="width"></param>
        /// <param name="stat"></param>
        /// <param name="label"></param>
        /// <param name="unit"></param>
        public static void TryDrawOverallArmor(Pawn selPawn, ref float curY, float width, StatDef stat, string label, string unit)
        {
            if (selPawn.RaceProps.body != BodyDefOf.Human)
            {
                return;
            }
            float num = 0f;
            List<Apparel> wornApparel = selPawn.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                num += wornApparel[i].GetStatValue(stat, true) * wornApparel[i].def.apparel.HumanBodyCoverage;
            }
            if (num > 0.005f)
            {
                Rect rect = new Rect(0f, curY, width, _standardLineHeight);
                List<BodyPartRecord> bpList = selPawn.RaceProps.body.AllParts;
                string text = "";
                for (int i = 0; i < bpList.Count; i++)
                {
                    float armorValue = 0f;
                    BodyPartRecord part = bpList[i];
                    if (part.depth == BodyPartDepth.Outside && (part.coverage >= 0.1 || part.def == BodyPartDefOf.Eye || part.def == BodyPartDefOf.Neck))
                    {
                        text += part.LabelCap + ": ";
                        for (int j = wornApparel.Count - 1; j >= 0; j--)
                        {
                            Apparel apparel = wornApparel[j];
                            if (apparel.def.apparel.CoversBodyPart(part))
                            {
                                armorValue += apparel.GetStatValue(stat, true);
                            }
                        }
                        text += FormatArmorValue(armorValue, unit) + "\n";
                    }
                }
                TooltipHandler.TipRegion(rect, text);

                // Draw Label
                Widgets.Label(rect, label.Truncate(200f, null));
                // Draw armor value
                rect.xMin += 200;
                Widgets.Label(rect, FormatArmorValue(num, unit));
                curY += _standardLineHeight;
            }
        }
    }
}
