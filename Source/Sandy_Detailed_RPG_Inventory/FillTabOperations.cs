using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CombatExtended;
using UnityEngine;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake
{
    public class FillTabOperations
    {
        private const float _barHeight = 20f;
        private const float _margin = 15f;
        private const float _thingIconSize = 28f;
        private const float _thingLeftX = 36f;
        private const float _thingRowHeight = 28f;
        private const float _topPadding = 50f;
        private const float _apparelRectWidth = 56f;
        private const float _apparelRectHeight = 56f;


        private static readonly Color _highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color _thingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static Vector2 _scrollPosition = Vector2.zero;
        private static SmartRect _smartRectHead;
        private static SmartRect _smartRectCurrent;

        private static List<Thing> workingInvList = new List<Thing>();
        private static List<Apparel> _apparelOverflow = new List<Apparel>();
        private static List<SmartRect> _smartRects = new List<SmartRect>();


        private static float _scrollViewHeight;
        public static void DrawAscetic() { }
        public static void DrawJealous(RPG_Pawn selPawn, Vector2 size)
        {
            // Races that don't have a humanoid body will fall back to Greedy tab
            if (selPawn.Pawn.RaceProps.body != BodyDefOf.Human)
            {
                //DrawGreedy(selPawn, size);
                return;
            }
            // set up rects
            Rect listRect = new Rect(
                _margin,
                _topPadding,
                size.x - 2 * _margin,
                size.y - _topPadding - _margin);

            // start drawing list
            GUI.BeginGroup(listRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, listRect.width, listRect.height);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, _scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            // draw Mass info and Temperature on the side
            Vector2 rectStat = new Vector2(374, 0);
            Utility.TryDrawMassInfoWithImage(selPawn.Pawn, rectStat);
            Utility.TryDrawComfyTemperatureRangeWithImage(selPawn.Pawn, rectStat);

            // draw armor rating for humanoid on the side
            Rect rectarmor = new Rect(374f, 84f, 128f, 85f);
            TooltipHandler.TipRegion(rectarmor, "OverallArmor".Translate());
            Rect rectsharp = new Rect(rectarmor.x, rectarmor.y, rectarmor.width, 27f);
            Utility.TryDrawOverallArmorWithImage(selPawn.Pawn, rectsharp, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(),
                                     ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorSharp_Icon", true));
            Rect rectblunt = new Rect(rectarmor.x, rectarmor.y + 30f, rectarmor.width, 27f);
            Utility.TryDrawOverallArmorWithImage(selPawn.Pawn, rectblunt, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(),
                                     ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorBlunt_Icon", true));
            Rect rectheat = new Rect(rectarmor.x, rectarmor.y + 60f, rectarmor.width, 27f);
            Utility.TryDrawOverallArmorWithImage(selPawn.Pawn, rectheat, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(),
                                     ContentFinder<Texture2D>.Get("UI/Icons/Sandy_ArmorHeat_Icon", true));

            //Pawn
            Color color = new Color(1f, 1f, 1f, 1f);
            GUI.color = color;
            Rect PawnRect = new Rect(374f, 172f, 128f, 128f);
            Utility.DrawColonist(PawnRect, selPawn.Pawn);

            // Head:200-181, Neck:180-101, Torso:100-51, Waist:50-11, Legs:10-0

            IEnumerable<Apparel> apparels = from ap in selPawn.Pawn.apparel.WornApparel
                                            orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                            select ap;

            IEnumerator<Apparel> apparelCounter = apparels.GetEnumerator();

            //Hats. BodyGroup: Fullhead; Layer: Overhead
            Rect HeadOverRect = new Rect(150f, 0f, _apparelRectWidth, _apparelRectHeight);
            GUI.DrawTexture(HeadOverRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect1 = HeadOverRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect1, "Sandy_Head".Translate());
            _smartRectHead = _smartRectCurrent =
                new SmartRect(HeadOverRect, BodyPartGroupDefOf.FullHead,
                              HeadOverRect.x, HeadOverRect.x + HeadOverRect.width,
                              _smartRects, 10, rectStat.x);

            bool cont = false;
            // Draw apparels on the head
            while (apparelCounter.MoveNext())
            {
                Apparel apparel = apparelCounter.Current;
                if (apparel.def.apparel.bodyPartGroups[0].listOrder > CorgiBodyPartGroupDefOf.Neck.listOrder)
                {
                    if (apparel.def.apparel.bodyPartGroups[0] == CorgiBodyPartGroupDefOf.FullHead)
                    {
                        Utility.DrawThingRowWithImage(selPawn, HeadOverRect, apparel);
                    }
                    else
                    {
                        _smartRectCurrent = _smartRectCurrent
                                            .GetWorkingRect(CorgiBodyPartGroupDefOf.FullHead,
                                                            HeadOverRect.x,
                                                            HeadOverRect.x);
                        DrawThumbnails(selPawn, _smartRectCurrent, apparel);
                    }
                }
                else
                {
                    cont = true;
                    break;
                }
            }

            // Draw apparels on the neck
            if (cont)
            {
                cont = false;
                do
                {
                    Apparel apparel = apparelCounter.Current;
                    if (apparel.def.apparel.bodyPartGroups[0].listOrder <= CorgiBodyPartGroupDefOf.Neck.listOrder &&
                        apparel.def.apparel.bodyPartGroups[0].listOrder > BodyPartGroupDefOf.Torso.listOrder)
                    {
                        _smartRectCurrent = _smartRectCurrent
                                                .GetWorkingRect(CorgiBodyPartGroupDefOf.Neck,
                                                                HeadOverRect.x,
                                                                HeadOverRect.x);
                        DrawThumbnails(selPawn, _smartRectCurrent, apparel);
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }

            //Vests. BodyGroup: Torso; Layer: Middle
            Rect TorsoMidRect = new Rect(76f, 148f, _apparelRectWidth, _apparelRectHeight);
            GUI.DrawTexture(TorsoMidRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect2 = TorsoMidRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect2, "Sandy_TorsoMiddle".Translate());

            //Shirts. BodyGroup: Torso; Layer: OnSkin
            Rect TorsoSkinRect = new Rect(150f, 148f, _apparelRectWidth, _apparelRectHeight);
            GUI.DrawTexture(TorsoSkinRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect3 = TorsoSkinRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect3, "Sandy_TorsoOnSkin".Translate());

            //Dusters. BodyGroup: Torso; Layer: Shell
            Rect TorsoShellRect = new Rect(224f, 148f, _apparelRectWidth, _apparelRectHeight);
            GUI.DrawTexture(TorsoShellRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect4 = TorsoShellRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect4, "Sandy_TorsoShell".Translate());

            // Draw apparels on Torso
            if (cont)
            {
                cont = false;
                do
                {
                    Apparel apparel = apparelCounter.Current;
                    if (apparel.def.apparel.bodyPartGroups[0].listOrder <= CorgiBodyPartGroupDefOf.Torso.listOrder &&
                        apparel.def.apparel.bodyPartGroups[0].listOrder > CorgiBodyPartGroupDefOf.Waist.listOrder)
                    {
                        if (apparel.def.apparel.bodyPartGroups[0] == BodyPartGroupDefOf.Torso)
                        {
                            if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
                            {
                                Utility.DrawThingRowWithImage(selPawn, TorsoShellRect, apparel);
                            }
                            else if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Middle)
                            {
                                Utility.DrawThingRowWithImage(selPawn, TorsoMidRect, apparel);
                            }
                            else if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin)
                            {
                                Utility.DrawThingRowWithImage(selPawn, TorsoSkinRect, apparel);
                            }
                            else
                            {
                                _smartRectCurrent = _smartRectCurrent
                                                .GetWorkingRect(CorgiBodyPartGroupDefOf.Torso,
                                                                HeadOverRect.x,
                                                                HeadOverRect.x);
                                DrawThumbnails(selPawn, _smartRectCurrent, apparel);
                            }
                        }
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }

            //Belts. BodyGroup: Waist; Layer: Belt
            Rect WaistBeltRect = new Rect(150f, 222f, _apparelRectWidth, _apparelRectHeight);
            GUI.DrawTexture(WaistBeltRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect5 = WaistBeltRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect5, "Sandy_Belt".Translate());

            // Draw apparels on waist
            if (cont)
            {
                cont = false;
                do
                {
                    Apparel apparel = apparelCounter.Current;
                    if (apparel.def.apparel.bodyPartGroups[0].listOrder <= CorgiBodyPartGroupDefOf.Waist.listOrder &&
                        apparel.def.apparel.bodyPartGroups[0].listOrder > CorgiBodyPartGroupDefOf.Legs.listOrder)
                    {
                        if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt)
                        {
                            Utility.DrawThingRowWithImage(selPawn, WaistBeltRect, apparel);
                        }
                        else
                        {
                            _smartRectCurrent = _smartRectCurrent
                                                .GetWorkingRect(CorgiBodyPartGroupDefOf.Waist,
                                                                HeadOverRect.x,
                                                                HeadOverRect.x);
                            DrawThumbnails(selPawn, _smartRectCurrent, apparel);
                        }
                    }
                    else
                    {
                        cont = true;
                        break;
                    }

                } while (apparelCounter.MoveNext());
            }

            //Pants. BodyGroup: Legs; Layer: OnSkin
            Rect LegSkinRect = new Rect(150f, 296f, _apparelRectWidth, _apparelRectHeight);
            GUI.DrawTexture(LegSkinRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect6 = LegSkinRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect6, "Sandy_Pants".Translate());

            if (cont)
            {
                cont = false;
                do
                {
                    Apparel apparel = apparelCounter.Current;
                    if (apparel.def.apparel.bodyPartGroups[0].listOrder <= CorgiBodyPartGroupDefOf.Legs.listOrder &&
                        apparel.def.apparel.bodyPartGroups[0].listOrder > 0)
                    {
                        if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin)
                        {
                            Utility.DrawThingRowWithImage(selPawn, LegSkinRect, apparel);
                        }
                        else
                        {
                            _smartRectCurrent = _smartRectCurrent
                                                .GetWorkingRect(CorgiBodyPartGroupDefOf.Legs,
                                                                HeadOverRect.x,
                                                                HeadOverRect.x);
                            DrawThumbnails(selPawn, _smartRectCurrent, apparel);
                        }
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }

            if (cont)
            {
                do
                {
                    _apparelOverflow.Add(apparelCounter.Current);
                } while (apparelCounter.MoveNext());
            }

            if (_apparelOverflow.Count > 0)
            {
                float rollingY = _smartRectCurrent.y + _smartRectCurrent.height + Utility.StandardLineHeight;
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Extra Apparels".Translate());
                _smartRectCurrent = _smartRectCurrent.GetWorkingRect(CorgiBodyPartGroupDefOf.Arse, 0, 0);
                _smartRectCurrent.y = rollingY;

                do
                {
                    List<Apparel> tempList = _apparelOverflow;
                    _apparelOverflow.Clear();
                    foreach (Apparel apparel in tempList)
                    {
                        DrawThumbnails(selPawn, _smartRectCurrent, apparel);
                    }
                    if (_apparelOverflow.Count > 0)
                    {
                        SmartRect temp = _smartRectCurrent.CreateSibling(CorgiBodyPartGroupDefOf.Arse);
                        temp.PreviousSibling = _smartRectCurrent;
                        _smartRectCurrent.NextSibling = temp;
                        _smartRectCurrent = temp;
                    }
                } while (_apparelOverflow.Count > 0);
            }

            float rollingY1 = _smartRectCurrent.y + _smartRectCurrent.height;
            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = rollingY1 + 30f;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Reset();
        }
        //public static void DrawGreedy(RPG_Pawn selPawn, Vector2 size)
        //{
        //    // set up rects
        //    Rect listRect = new Rect(
        //        _margin,
        //        _topPadding,
        //        size.x - 2 * _margin,
        //        size.y - _topPadding - _margin);

        //    // start drawing list
        //    GUI.BeginGroup(listRect);
        //    Text.Font = GameFont.Small;
        //    GUI.color = Color.white;
        //    Rect outRect = new Rect(0f, 0f, listRect.width, listRect.height);
        //    Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, _scrollViewHeight);
        //    Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

        //    float rollingY = 0f;
        //    // draw mass info and temperature
        //    Utility.TryDrawMassInfo(selPawn.Pawn, ref rollingY, viewRect.width);
        //    Utility.TryDrawComfyTemperatureRange(selPawn.Pawn, ref rollingY, viewRect.width);

        //    // draw overall armor
        //    if (Utility.ShouldShowOverallArmor(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref rollingY, viewRect.width, "OverallArmor".Translate());
        //        Utility.TryDrawOverallArmor(selPawn.Pawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), " " + "CE_MPa".Translate());
        //        Utility.TryDrawOverallArmor(selPawn.Pawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "CE_mmRHA".Translate());
        //        Utility.TryDrawOverallArmor(selPawn.Pawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
        //    }
        //    if (Utility.ShouldShowEquipment(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref rollingY, viewRect.width, "Equipment".Translate());
        //        foreach (ThingWithComps current in selPawn.Pawn.equipment.AllEquipmentListForReading)
        //        {
        //            Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, current);
        //        }
        //    }
        //    if (Utility.ShouldShowApparel(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref rollingY, viewRect.width, "Apparel".Translate());
        //        foreach (Apparel current2 in from ap in selPawn.Pawn.apparel.WornApparel
        //                                     orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
        //                                     select ap)
        //        {
        //            Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, current2);
        //        }
        //    }
        //    if (Utility.ShouldShowInventory(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref rollingY, viewRect.width, "Inventory".Translate());

        //        workingInvList.Clear();
        //        workingInvList.AddRange(selPawn.Pawn.inventory.innerContainer);
        //        for (int i = 0; i < workingInvList.Count; i++)
        //        {
        //            Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, workingInvList[i].GetInnerIfMinified(), true);
        //        }
        //    }
        //    if (Event.current.type == EventType.Layout)
        //    {
        //        _scrollViewHeight = rollingY + 30f;
        //    }
        //    Widgets.EndScrollView();
        //    GUI.EndGroup();
        //    GUI.color = Color.white;
        //    Text.Anchor = TextAnchor.UpperLeft;
        //}
        //public static void DrawGreedy_CE(RPG_Pawn selPawn, Vector2 size)
        //{
        //    // get the inventory comp
        //    CompInventory comp = selPawn.Pawn.TryGetComp<CompInventory>();

        //    // set up rects
        //    Rect listRect = new Rect(
        //        _margin,
        //        _topPadding,
        //        size.x - 2 * _margin,
        //        size.y - _topPadding - _margin);

        //    // draw bars at the bottom
        //    if (comp != null)
        //    {
        //        PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.FrameDisplayed);

        //        // adjust rects if comp found
        //        listRect.height -= (_margin / 2 + _barHeight) * 2;
        //        Rect weightRect = new Rect(_margin, listRect.yMax + _margin / 2, listRect.width, _barHeight);
        //        Rect bulkRect = new Rect(_margin, weightRect.yMax + _margin / 2, listRect.width, _barHeight);

        //        // draw bars
        //        Utility_Loadouts.DrawBar(bulkRect, comp.currentBulk, comp.capacityBulk, "CE_Bulk".Translate(), selPawn.Pawn.GetBulkTip());
        //        Utility_Loadouts.DrawBar(weightRect, comp.currentWeight, comp.capacityWeight, "CE_Weight".Translate(), selPawn.Pawn.GetWeightTip());

        //        // draw text overlays on bars
        //        Text.Font = GameFont.Small;
        //        Text.Anchor = TextAnchor.MiddleCenter;

        //        string currentBulk = CE_StatDefOf.CarryBulk.ValueToString(comp.currentBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
        //        string capacityBulk = CE_StatDefOf.CarryBulk.ValueToString(comp.capacityBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
        //        Widgets.Label(bulkRect, currentBulk + "/" + capacityBulk);

        //        string currentWeight = comp.currentWeight.ToString("0.#");
        //        string capacityWeight = CE_StatDefOf.CarryWeight.ValueToString(comp.capacityWeight, CE_StatDefOf.CarryWeight.toStringNumberSense);
        //        Widgets.Label(weightRect, currentWeight + "/" + capacityWeight);

        //        Text.Anchor = TextAnchor.UpperLeft;
        //    }

        //    // start drawing list (rip from ITab_Pawn_Gear)

        //    GUI.BeginGroup(listRect);
        //    Text.Font = GameFont.Small;
        //    GUI.color = Color.white;
        //    Rect outRect = new Rect(0f, 0f, listRect.width, listRect.height);
        //    Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, _scrollViewHeight);
        //    Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
        //    float num = 0f;
        //    Utility.TryDrawComfyTemperatureRange(selPawn.Pawn, ref num, viewRect.width);
        //    if (Utility.ShouldShowOverallArmor(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref num, viewRect.width, "OverallArmor".Translate());
        //        Utility.TryDrawOverallArmor(selPawn.Pawn, ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), " " + "CE_MPa".Translate());
        //        Utility.TryDrawOverallArmor(selPawn.Pawn, ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "CE_mmRHA".Translate());
        //        Utility.TryDrawOverallArmor(selPawn.Pawn, ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
        //    }
        //    if (Utility.ShouldShowEquipment(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
        //        foreach (ThingWithComps current in selPawn.Pawn.equipment.AllEquipmentListForReading)
        //        {
        //            Utility.DrawThingRow(selPawn, ref num, viewRect.width, current);
        //        }
        //    }
        //    if (Utility.ShouldShowApparel(selPawn.Pawn))
        //    {
        //        Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
        //        foreach (Apparel current2 in from ap in selPawn.Pawn.apparel.WornApparel
        //                                     orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
        //                                     select ap)
        //        {
        //            Utility.DrawThingRow(selPawn, ref num, viewRect.width, current2);
        //        }
        //    }
        //    if (Utility.ShouldShowInventory(selPawn.Pawn))
        //    {
        //        // get the loadout so we can make a decision to show a button.
        //        bool showMakeLoadout = false;
        //        Loadout curLoadout = selPawn.Pawn.GetLoadout();
        //        if (selPawn.Pawn.IsColonist && (curLoadout == null || curLoadout.Slots.NullOrEmpty()) && (selPawn.Pawn.inventory.innerContainer.Any() || selPawn.Pawn.equipment?.Primary != null))
        //            showMakeLoadout = true;

        //        if (showMakeLoadout) num += 3; // Make a little room for the button.

        //        float buttonY = num; // Could be accomplished with seperator being after the button...

        //        Widgets.ListSeparator(ref num, viewRect.width, "Inventory".Translate());

        //        // only offer this button if the pawn has no loadout or has the default loadout and there are things/equipment...
        //        if (showMakeLoadout)
        //        {
        //            Rect loadoutButtonRect = new Rect(viewRect.width / 2, buttonY, viewRect.width / 2, 26f); // button is half the available width...
        //            if (Widgets.ButtonText(loadoutButtonRect, "CE_MakeLoadout".Translate()))
        //            {
        //                Loadout loadout = selPawn.Pawn.GenerateLoadoutFromPawn();
        //                LoadoutManager.AddLoadout(loadout);
        //                selPawn.Pawn.SetLoadout(loadout);

        //                // UNDONE ideally we'd open the assign (MainTabWindow_OutfitsAndLoadouts) tab as if the user clicked on it here.
        //                // (ProfoundDarkness) But I have no idea how to do that just yet.  The attempts I made seem to put the RimWorld UI into a bit of a bad state.
        //                //                     ie opening the tab like the dialog below.
        //                //                    Need to understand how RimWorld switches tabs and see if something similar can be done here
        //                //                     (or just remove the unfinished marker).

        //                // Opening this window is the same way as if from the assign tab so should be correct.
        //                Find.WindowStack.Add(new Dialog_ManageLoadouts(selPawn.Pawn.GetLoadout()));

        //            }
        //        }

        //        workingInvList.Clear();
        //        workingInvList.AddRange(selPawn.Pawn.inventory.innerContainer);
        //        for (int i = 0; i < workingInvList.Count; i++)
        //        {
        //            Utility.DrawThingRow(selPawn, ref num, viewRect.width, workingInvList[i].GetInnerIfMinified(), true);
        //        }
        //    }
        //    if (Event.current.type == EventType.Layout)
        //    {
        //        _scrollViewHeight = num + 30f;
        //    }
        //    Widgets.EndScrollView();
        //    GUI.EndGroup();
        //    GUI.color = Color.white;
        //    Text.Anchor = TextAnchor.UpperLeft;

        //}
        public static bool DrawThumbnails(RPG_Pawn pawn, SmartRect smartRect, Apparel apparel)
        {
            Rect newRect = smartRect.NextAvailableRect();
            if (newRect == default)
            {
                _apparelOverflow.Add(apparel);
                return false;
            }
            else
            {
                GUI.DrawTexture(newRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                Utility.DrawThingRowWithImage(pawn, newRect, apparel);
            }
            return true;
        }
        private static void Reset()
        {
            workingInvList.Clear();
            _apparelOverflow.Clear();
            _smartRects.Clear();
        }
    }
}
