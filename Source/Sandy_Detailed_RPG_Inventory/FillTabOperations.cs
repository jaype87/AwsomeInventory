using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CombatExtended;
using UnityEngine;
using RimWorld;
using Verse;

namespace RPG_Inventory_for_CE
{
    public static class FillTabOperations
    {
        private const float _barHeight = 20f;
        private const float _margin = 15f;
        private const float _thingIconSize = 28f;
        private const float _thingLeftX = 36f;
        private const float _thingRowHeight = 28f;
        private const float _topPadding = 30f;
        private const float _standardLineHeight = 22f;

        private static readonly Color _highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color _thingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static Vector2 _scrollPosition = Vector2.zero;

        private static List<Thing> workingInvList = new List<Thing>();

        private static float _scrollViewHeight;
        public static void DrawAscetic() { }
        public static void DrawGreedy() { }
        public static void DrawJealous(RPG_Pawn selPawn, Vector2 size)
        {
            float _barHeight = 20f;
            float _margin = 15f;
            // get the inventory comp
            CompInventory comp = selPawn.Pawn.TryGetComp<CompInventory>();

            // set up rects
            Rect listRect = new Rect(
                _margin,
                _topPadding,
                size.x - 2 * _margin,
                size.y - _topPadding - _margin);

            if (comp != null)
            {
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.FrameDisplayed);

                // adjust rects if comp found
                listRect.height -= (_margin / 2 + _barHeight) * 2;
                Rect weightRect = new Rect(_margin, listRect.yMax + _margin / 2, listRect.width, _barHeight);
                Rect bulkRect = new Rect(_margin, weightRect.yMax + _margin / 2, listRect.width, _barHeight);

                // draw bars
                Utility_Loadouts.DrawBar(bulkRect, comp.currentBulk, comp.capacityBulk, "CE_Bulk".Translate(), selPawn.Pawn.GetBulkTip());
                Utility_Loadouts.DrawBar(weightRect, comp.currentWeight, comp.capacityWeight, "CE_Weight".Translate(), selPawn.Pawn.GetWeightTip());

                // draw text overlays on bars
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;

                string currentBulk = CE_StatDefOf.CarryBulk.ValueToString(comp.currentBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
                string capacityBulk = CE_StatDefOf.CarryBulk.ValueToString(comp.capacityBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
                Widgets.Label(bulkRect, currentBulk + "/" + capacityBulk);

                string currentWeight = comp.currentWeight.ToString("0.#");
                string capacityWeight = CE_StatDefOf.CarryWeight.ValueToString(comp.capacityWeight, CE_StatDefOf.CarryWeight.toStringNumberSense);
                Widgets.Label(weightRect, currentWeight + "/" + capacityWeight);

                Text.Anchor = TextAnchor.UpperLeft;
            }

            // start drawing list (rip from ITab_Pawn_Gear)

            GUI.BeginGroup(listRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, listRect.width, listRect.height);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, _scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            float num = 0f;
            Utility.TryDrawComfyTemperatureRange(selPawn.Pawn, ref num, viewRect.width);
            if (Utility.ShouldShowOverallArmor(selPawn.Pawn))
            {
                Widgets.ListSeparator(ref num, viewRect.width, "OverallArmor".Translate());
                Utility.TryDrawOverallArmor(selPawn.Pawn, ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), " " + "CE_MPa".Translate());
                Utility.TryDrawOverallArmor(selPawn.Pawn, ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "CE_mmRHA".Translate());
                Utility.TryDrawOverallArmor(selPawn.Pawn, ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
            }
            if (Utility.ShouldShowEquipment(selPawn.Pawn))
            {
                Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
                foreach (ThingWithComps current in selPawn.Pawn.equipment.AllEquipmentListForReading)
                {
                    Utility.DrawThingRow(selPawn, ref num, viewRect.width, current);
                }
            }
            if (Utility.ShouldShowApparel(selPawn.Pawn))
            {
                Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
                foreach (Apparel current2 in from ap in selPawn.Pawn.apparel.WornApparel
                                             orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                             select ap)
                {
                    Utility.DrawThingRow(selPawn, ref num, viewRect.width, current2);
                }
            }
            if (Utility.ShouldShowInventory(selPawn.Pawn))
            {
                // get the loadout so we can make a decision to show a button.
                bool showMakeLoadout = false;
                Loadout curLoadout = selPawn.Pawn.GetLoadout();
                if (selPawn.Pawn.IsColonist && (curLoadout == null || curLoadout.Slots.NullOrEmpty()) && (selPawn.Pawn.inventory.innerContainer.Any() || selPawn.Pawn.equipment?.Primary != null))
                    showMakeLoadout = true;

                if (showMakeLoadout) num += 3; // Make a little room for the button.

                float buttonY = num; // Could be accomplished with seperator being after the button...

                Widgets.ListSeparator(ref num, viewRect.width, "Inventory".Translate());

                // only offer this button if the pawn has no loadout or has the default loadout and there are things/equipment...
                if (showMakeLoadout)
                {
                    Rect loadoutButtonRect = new Rect(viewRect.width / 2, buttonY, viewRect.width / 2, 26f); // button is half the available width...
                    if (Widgets.ButtonText(loadoutButtonRect, "CE_MakeLoadout".Translate()))
                    {
                        Loadout loadout = selPawn.Pawn.GenerateLoadoutFromPawn();
                        LoadoutManager.AddLoadout(loadout);
                        selPawn.Pawn.SetLoadout(loadout);

                        // UNDONE ideally we'd open the assign (MainTabWindow_OutfitsAndLoadouts) tab as if the user clicked on it here.
                        // (ProfoundDarkness) But I have no idea how to do that just yet.  The attempts I made seem to put the RimWorld UI into a bit of a bad state.
                        //                     ie opening the tab like the dialog below.
                        //                    Need to understand how RimWorld switches tabs and see if something similar can be done here
                        //                     (or just remove the unfinished marker).

                        // Opening this window is the same way as if from the assign tab so should be correct.
                        Find.WindowStack.Add(new Dialog_ManageLoadouts(selPawn.Pawn.GetLoadout()));

                    }
                }

                workingInvList.Clear();
                workingInvList.AddRange(selPawn.Pawn.inventory.innerContainer);
                for (int i = 0; i < workingInvList.Count; i++)
                {
                    Utility.DrawThingRow(selPawn, ref num, viewRect.width, workingInvList[i].GetInnerIfMinified(), true);
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

        }
    }
}
