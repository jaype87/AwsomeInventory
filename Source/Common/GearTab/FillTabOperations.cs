// <copyright file="FillTabOperations.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AwesomeInventory.Common.Loadout;
using RimWorld;
using RPG_Inventory_Remake_Common;
using RPGIResource;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Common
{
    public class FillTabOperations
    {
        private const float _margin = 15f;
        private const float _topPadding = 50f;
        private const float _apparelRectWidth = 56f;
        private const float _apparelRectHeight = 56f;
        private const float _startingXforRect = 150f;

        public static Vector2 ScrollPosition = Vector2.zero;

        private static SmartRect _smartRectHead;
        private static SmartRect _smartRectCurrent;
        private static List<Thing> workingInvList = new List<Thing>();
        private static List<ThingWithComps> _apparelOverflow = new List<ThingWithComps>();
        private static List<SmartRect> _smartRects = new List<SmartRect>();

        private static float _scrollViewHeight;
        public static void DrawAscetic() { }
        public static void DrawJealous(PawnModal selPawn, Rect canvas)
        {
            // Races that don't have a humanoid body will fall back to Greedy tab
            if (selPawn.Pawn.RaceProps.body != BodyDefOf.Human)
            {
                DrawGreedy(selPawn, canvas);
                return;
            }

            Rect innerCanvas = canvas.ContractedBy(GenUI.GapSmall);
            GUI.BeginGroup(innerCanvas);
            Rect outRect = innerCanvas.AtZero();
            Rect viewRect = outRect;
            viewRect.height = _scrollViewHeight;
            viewRect.width -= GenUI.Gap;

            // start drawing the view
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref ScrollPosition, viewRect);

            // draw all stats on the right
            Rect rectStat = outRect.RightPart(0.35f);

            DrawStatPanel(rectStat, selPawn.Pawn, out float statY);

            //Pawn
            //float statY = 120;
            statY -= WidgetRow.IconSize;
            Rect pawnRect = new Rect(new Vector2(rectStat.x + GenUI.GapSmall, statY), UtilityConstant.PaperDollSize);

            Utility.DrawColonist(pawnRect, selPawn.Pawn);

            #region Weapon
            // draw equipment (equipment means barrel that can shoot bullets, plank that can slice flesh in half)
            // It is weapon.
            SmartRect rectForEquipment =
                new SmartRect(new Rect(), CorgiBodyPartGroupDefOf.Arse,
                              pawnRect.x, pawnRect.x, null, pawnRect.x, canvas.x - 20f)
                {
                    y = pawnRect.yMax - GenUI.GapSmall - 3f,
                    width = _apparelRectWidth,
                    height = _apparelRectHeight
                };

            if (Utility.ShouldShowEquipment(selPawn.Pawn) && (selPawn.Pawn.RaceProps.body == BodyDefOf.Human))
            {
                Rect primaryRect = rectForEquipment.NextAvailableRect();

                GUI.DrawTexture(primaryRect, Command.BGTex);
                TooltipHandler.TipRegion(primaryRect, "Corgi_PrimaryWeapon".Translate());

                foreach (ThingWithComps fireShootingBarrel in selPawn.Pawn.equipment.AllEquipmentListForReading)
                {
                    if (fireShootingBarrel == selPawn.Pawn.equipment.Primary)
                    {
                        Utility.DrawThingRowWithImage(selPawn, primaryRect, fireShootingBarrel);
                    }
                    else
                    {
                        UtilityDraw.DrawThumbnails(selPawn, rectForEquipment, fireShootingBarrel, _apparelOverflow);
                    }
                }
            }
            #endregion

            // List order: Head:200-181, Neck:180-101, Torso:100-51, Waist:50-11, Legs:10-0
            // Check \steamapps\common\RimWorld\Mods\Core\Defs\Bodies\BodyPartGroups.xml
            IEnumerable<Apparel> apparels = from ap in selPawn.Pawn.apparel.WornApparel
                                            orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                            select ap;
            IEnumerator<Apparel> apparelCounter = apparels.GetEnumerator();

            #region Headwear
            //Hats. BodyGroup: Fullhead; Layer: Overhead
            _smartRectHead = _smartRectCurrent =
                new SmartRect(new Rect(outRect.x, outRect.y, _apparelRectWidth, _apparelRectHeight),
                              BodyPartGroupDefOf.UpperHead,
                              _startingXforRect, _startingXforRect,
                              _smartRects, 10, rectStat.x);
            Rect HeadOverRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(HeadOverRect, Command.BGTex);
            Rect tipRect1 = HeadOverRect;
            TooltipHandler.TipRegion(tipRect1, "Sandy_Head".Translate());

            // cont is used to Check if there is more apparel in the list
            bool cont = false;
            bool hatDrawn = false;
            // Draw apparels on the head
            while (apparelCounter.MoveNext())
            {
                Apparel apparel = apparelCounter.Current;
                if (apparel.def.apparel.bodyPartGroups[0].listOrder > CorgiBodyPartGroupDefOf.Neck.listOrder)
                {
                    // Check RimWorld.Pawn_ApparelTracker.Wear() for more information on wearing rules
                    if ((apparel.def.apparel.bodyPartGroups.Contains(CorgiBodyPartGroupDefOf.FullHead) ||
                         apparel.def.apparel.bodyPartGroups.Contains(CorgiBodyPartGroupDefOf.UpperHead)) &&
                         hatDrawn == false)
                    {
                        Utility.DrawThingRowWithImage(selPawn, HeadOverRect, apparel);
                        hatDrawn = true;
                    }
                    else
                    {
                        _smartRectCurrent = _smartRectCurrent
                                            .GetWorkingRect(CorgiBodyPartGroupDefOf.UpperHead,
                                                            _startingXforRect,
                                                            _startingXforRect);
                        UtilityDraw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                    }
                }
                else
                {
                    cont = true;
                    break;
                }
            }
            #endregion

            #region Neck
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
                                                                _startingXforRect, _startingXforRect);
                        UtilityDraw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }
            #endregion

            #region Draw Torso
            // Draw apparels on Torso
            _smartRectCurrent = _smartRectCurrent
                                    .GetWorkingRect(CorgiBodyPartGroupDefOf.Torso,
                                                    _startingXforRect, _startingXforRect);

            //Shirts. BodyGroup: Torso; Layer: OnSkin
            Rect TorsoSkinRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(TorsoSkinRect, Command.BGTex);
            Rect tipRect3 = TorsoSkinRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect3, "Sandy_TorsoOnSkin".Translate());

            //Vests. BodyGroup: Torso; Layer: Middle
            Rect TorsoMidRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(TorsoMidRect, Command.BGTex);
            Rect tipRect2 = TorsoMidRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect2, "Sandy_TorsoMiddle".Translate());

            //Dusters. BodyGroup: Torso; Layer: Shell
            Rect TorsoShellRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(TorsoShellRect, Command.BGTex);
            Rect tipRect4 = TorsoShellRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect4, "Sandy_TorsoShell".Translate());

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
                        }
                        else
                        {
                            _smartRectCurrent = _smartRectCurrent
                                            .GetWorkingRect(CorgiBodyPartGroupDefOf.Torso,
                                                            _startingXforRect,
                                                            _startingXforRect);
                            UtilityDraw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                        }
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }
            #endregion Draw Torso

            #region Draw Waist
            // Draw apparels on waist
            _smartRectCurrent = _smartRectCurrent
                                    .GetWorkingRect(CorgiBodyPartGroupDefOf.Waist,
                                                    _startingXforRect,
                                                    _startingXforRect);
            //Belts. BodyGroup: Waist; Layer: Belt
            Rect WaistBeltRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(WaistBeltRect, Command.BGTex);
            Rect tipRect5 = WaistBeltRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect5, "Sandy_Belt".Translate());

            if (cont)
            {
                bool waistBeltDrawn = false;
                cont = false;

                do
                {
                    Apparel apparel = apparelCounter.Current;
                    if (apparel.def.apparel.bodyPartGroups[0].listOrder <= CorgiBodyPartGroupDefOf.Waist.listOrder &&
                        apparel.def.apparel.bodyPartGroups[0].listOrder > CorgiBodyPartGroupDefOf.Legs.listOrder)
                    {
                        if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt && waistBeltDrawn == false)
                        {
                            Utility.DrawThingRowWithImage(selPawn, WaistBeltRect, apparel);
                            waistBeltDrawn = true;
                        }
                        else
                        {
                            _smartRectCurrent = _smartRectCurrent
                                                .GetWorkingRect(CorgiBodyPartGroupDefOf.Waist,
                                                                _startingXforRect,
                                                                _startingXforRect);
                            UtilityDraw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                        }
                    }
                    else
                    {
                        cont = true;
                        break;
                    }

                } while (apparelCounter.MoveNext());
            }
            #endregion Draw Waist

            #region Draw Legs
            // Draw apparels on lesg
            _smartRectCurrent = _smartRectCurrent
                                    .GetWorkingRect(CorgiBodyPartGroupDefOf.Legs,
                                                    _startingXforRect,
                                                    _startingXforRect);
            //Pants. BodyGroup: Legs; Layer: OnSkin
            Rect LegSkinRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(LegSkinRect, Command.BGTex);
            Rect tipRect6 = LegSkinRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect6, "Sandy_Pants".Translate());
            if (cont)
            {
                bool pantWore = false;
                cont = false;
                do
                {
                    Apparel apparel = apparelCounter.Current;
                    if (apparel.def.apparel.bodyPartGroups[0].listOrder <= CorgiBodyPartGroupDefOf.Legs.listOrder &&
                        apparel.def.apparel.bodyPartGroups[0].listOrder > 0)
                    {
                        if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin && pantWore == false)
                        {
                            Utility.DrawThingRowWithImage(selPawn, LegSkinRect, apparel);
                            pantWore = true;
                        }
                        else
                        {
                            _smartRectCurrent = _smartRectCurrent
                                                .GetWorkingRect(CorgiBodyPartGroupDefOf.Legs,
                                                                _startingXforRect,
                                                                _startingXforRect);
                            UtilityDraw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                        }
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }
            #endregion Draw Legs

            #region Draw Traits

            float traitY = _smartRectCurrent.yMax + _smartRectCurrent.HeightGap;
            WidgetRow traitRow = new WidgetRow(viewRect.x, traitY, UIDirection.RightThenDown, viewRect.width);
            List<Trait> traits = selPawn.Pawn.story.traits.allTraits;

            traitRow.Label(UIText.Traits.Translate() + ": ");
            for (int i = 0; i < traits.Count; i++)
            {
                TooltipHandler.TipRegion(traitRow.Label(traits[i].LabelCap + (i != traits.Count ? ", " : ""))
                    , traits[i].TipString(selPawn.Pawn));
            }

            float rollingY = traitRow.FinalY + WidgetRow.IconSize;
            #endregion

            #region Extra Apparels
            // Put undrawn into overflow list
            if (cont)
            {
                do
                {
                    _apparelOverflow.Add(apparelCounter.Current);
                } while (apparelCounter.MoveNext());
            }
            // Try to draw the extras
            UtilityDraw.DrawApparelToNextRow(selPawn, _smartRectHead, _apparelOverflow);

            // If there is any more remains, put them into their own category
            if (_apparelOverflow.Count > 0)
            {
                float rollingY1 = rollingY + Utility.StandardLineHeight;
                Widgets.ListSeparator(ref rollingY1, viewRect.width, "Corgi_ExtraApparels".Translate());
                _smartRectCurrent = _smartRectCurrent.GetWorkingRect(CorgiBodyPartGroupDefOf.Arse, _margin, _margin);
                _smartRectCurrent.y = rollingY1 + Utility.StandardLineHeight;

                do
                {
                    List<ThingWithComps> tempList = new List<ThingWithComps>(_apparelOverflow);
                    _apparelOverflow.Clear();
                    foreach (Apparel apparel in tempList)
                    {
                        UtilityDraw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                    }
                    if (_apparelOverflow.Count > 0)
                    {
                        SmartRect temp = _smartRectCurrent.CreateSibling(CorgiBodyPartGroupDefOf.Arse);
                        temp.PreviousSibling = _smartRectCurrent;
                        _smartRectCurrent.NextSibling = temp;
                        _smartRectCurrent = temp;
                        continue;
                    }
                } while (_apparelOverflow.Count > 0);
                rollingY = rollingY1;
            }
            #endregion Extra Apparels

            #region Draw Inventory
            // Draw inventory
            if (Utility.ShouldShowInventory(selPawn.Pawn))
            {

                if (rollingY < rectForEquipment.Rect.yMax)
                {
                    rollingY = rectForEquipment.Rect.yMax;
                }
                rollingY += Utility.StandardLineHeight;

                if (ModStarter.Settings.UseLoadout)
                {
                    WidgetRow row = new WidgetRow(viewRect.xMax, rollingY, UIDirection.LeftThenDown, viewRect.width);
                    if (row.ButtonText(UIText.OpenLoadout.Translate()))
                    {
                        if (selPawn.Pawn.IsColonist && selPawn.Pawn.GetLoadout() == null)
                        {
                            AILoadout loadout = new AILoadout(selPawn.Pawn);
                            LoadoutManager.AddLoadout(loadout);
                            selPawn.Pawn.SetLoadout(loadout);
                        }
                        Find.WindowStack.Add(new Dialog_ManageLoadouts(selPawn.Pawn.GetLoadout(), selPawn.Pawn));
                    }
                    if (row.ButtonText(UIText.SelectLoadout.Translate()))
                    {
                        List<AILoadout> loadouts = LoadoutManager.Loadouts;
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        if (loadouts.Count == 0)
                        {
                            list.Add(new FloatMenuOption(UIText.NoLoadout.Translate(), null));
                        }
                        else
                        {
                            for (int i = 0; i < loadouts.Count; i++)
                            {
                                int local_i = i;
                                list.Add(new FloatMenuOption(loadouts[i].label, delegate
                                {
                                    selPawn.Pawn.SetLoadout(loadouts[local_i]);
                                }));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                    Text.Anchor = TextAnchor.MiddleRight;
                    Text.WordWrap = false;

                    row.Label(selPawn.Pawn.GetLoadout()?.label, GenUI.GetWidthCached(UIText.TenCharsString.Times(2.5f)));

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.WordWrap = true;
                    rollingY = row.FinalY + WidgetRow.IconSize - Text.LineHeight;
                }
                Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.Inventory.Translate());



                workingInvList.Clear();
                workingInvList.AddRange(selPawn.Pawn.inventory.innerContainer);
                for (int i = 0; i < workingInvList.Count; i++)
                {
                    Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, workingInvList[i].GetInnerIfMinified(), true);
                }
            }
            #endregion Draw Inventory

            //// Add support for smart medicine
            //if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            //{
            //    smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
            //    .Invoke(null, new object[] { selPawn.Pawn, rollingY, viewRect.width });
            //}

            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = rollingY + 30f;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Reset();
        }
        public static void DrawGreedy(PawnModal selPawn, Rect canvas)
        {
            // set up rects
            Rect listRect = new Rect(
                _margin,
                _topPadding,
                canvas.width - 2 * _margin,
                canvas.height - _topPadding - _margin);

            // start drawing list
            GUI.BeginGroup(listRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, listRect.width, listRect.height);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, _scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref ScrollPosition, viewRect);

            float rollingY = 0f;
            // draw mass info and temperature
            Utility.TryDrawMassInfo(selPawn.Pawn, ref rollingY, viewRect.width);
            Utility.TryDrawComfyTemperatureRange(selPawn.Pawn, ref rollingY, viewRect.width);

            // draw overall armor
            if (Utility.ShouldShowOverallArmor(selPawn.Pawn))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, "OverallArmor".Translate());
                Utility.TryDrawOverallArmor(selPawn.Pawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "%");
                Utility.TryDrawOverallArmor(selPawn.Pawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), "%");
                Utility.TryDrawOverallArmor(selPawn.Pawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
            }
            if (Utility.ShouldShowEquipment(selPawn.Pawn))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Equipment".Translate());
                foreach (ThingWithComps current in selPawn.Pawn.equipment.AllEquipmentListForReading)
                {
                    Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, current);
                }
            }
            if (Utility.ShouldShowApparel(selPawn.Pawn))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Apparel".Translate());
                foreach (Apparel current2 in from ap in selPawn.Pawn.apparel.WornApparel
                                             orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                             select ap)
                {
                    Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, current2);
                }
            }
            if (Utility.ShouldShowInventory(selPawn.Pawn))
            {
                if (ModStarter.Settings.UseLoadout)
                {
                    rollingY += 3;
                    float buttonY = rollingY;

                    Text.WordWrap = false;
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(
                        new Rect
                            (GenUI.GetWidthCached(UIText.TenCharsString)
                            , buttonY
                            , viewRect.width / 2 - GenUI.GetWidthCached(UIText.TenCharsString)
                            , 26f), selPawn.Pawn.GetLoadout()?.label ?? "Corgi_NoLoadout".Translate());
                    Text.WordWrap = true;
                    Text.Anchor = TextAnchor.UpperLeft;

                    // Select loadout button
                    if (Widgets.ButtonText(new Rect(viewRect.width / 2, buttonY, viewRect.width / 4, 26f), Translator.Translate("Corgi_SelectLoadout"), true, false, true))
                    {
                        List<AILoadout> loadouts = LoadoutManager.Loadouts;
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        if (loadouts.Count == 0)
                        {
                            list.Add(new FloatMenuOption(UIText.NoLoadout.Translate(), null));
                        }
                        else
                        {
                            for (int i = 0; i < loadouts.Count; i++)
                            {
                                int local_i = i;
                                list.Add(new FloatMenuOption(loadouts[i].label, delegate
                                {
                                    selPawn.Pawn.SetLoadout(loadouts[local_i]);
                                }));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }

                    Rect loadoutButtonRect = new Rect(viewRect.width / 4 * 3, buttonY, viewRect.width / 4, 26f); // button is half the available width...
                    if (Widgets.ButtonText(loadoutButtonRect, "Corgi_OpenLoadout".Translate()))
                    {
                        if (selPawn.Pawn.IsColonist && (selPawn.Pawn.GetLoadout() == null))
                        {
                            AILoadout loadout = new AILoadout(selPawn.Pawn);
                            LoadoutManager.AddLoadout(loadout);
                            selPawn.Pawn.SetLoadout(loadout);
                        }

                        Find.WindowStack.Add(new Dialog_ManageLoadouts(selPawn.Pawn.GetLoadout(), selPawn.Pawn));
                    }
                }
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Inventory".Translate());

                workingInvList.Clear();
                workingInvList.AddRange(selPawn.Pawn.inventory.innerContainer);
                for (int i = 0; i < workingInvList.Count; i++)
                {
                    Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, workingInvList[i].GetInnerIfMinified(), true);
                }
            }

            //// Add support for smart medicine
            //if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            //{
            //    smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
            //    .Invoke(null, new object[] { selPawn.Pawn, rollingY, viewRect.width });
            //}

            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = rollingY + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DrawStatPanel(Rect rect, Pawn pawn, out float rollingY)
        {
            if (pawn.Dead)
            {
                rollingY = rect.yMax;
                return;
            }

            WidgetRow row = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown, rect.width);

            // Draw Mass
            if (Utility.ShouldShowInventory(pawn))
            {
                float carried = MassUtility.GearAndInventoryMass(pawn);
                float capacity = MassUtility.Capacity(pawn);

                row.Icon(ContentFinder<Texture2D>.Get(RPGIIcons.Mass, true), UIText.MassCarried.Translate());
                row.Label(string.Concat(carried, "/", capacity));
                row.Gap(Int32.MaxValue);
            }

            // Draw minimum comfy temperature
            row.Icon(ContentFinder<Texture2D>.Get(RPGIIcons.MinTemperature, true), UIText.ComfyTemperatureRange.Translate());
            row.Label(pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true).ToStringTemperature());
            row.Gap(Int32.MaxValue);

            // Draw maximum comfy temperature
            row.Icon(ContentFinder<Texture2D>.Get(RPGIIcons.MaxTemperature, true), UIText.ComfyTemperatureRange.Translate());
            row.Label(pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true).ToStringTemperature());
            row.Gap(Int32.MaxValue);

            // Draw armor stats
            DrawArmorStats(row, pawn, StatDefOf.ArmorRating_Blunt, RPGIIcons.ArmorBlunt);

            DrawArmorStats(row, pawn, StatDefOf.ArmorRating_Sharp, RPGIIcons.ArmorSharp);

            DrawArmorStats(row, pawn, StatDefOf.ArmorRating_Heat, RPGIIcons.ArmorHeat);
            rollingY = row.FinalY;
        }

        private static void DrawArmorStats(WidgetRow row, Pawn pawn, StatDef stat, string iconPath)
        {
            float value = Utility.CalculateArmorByParts(pawn, stat, out string tip);

            row.Icon(ContentFinder<Texture2D>.Get(iconPath, true), UIText.ArmorBlunt.Translate());
            Rect tipRect = row.Label(value.ToStringPercent());
            row.Gap(Int32.MaxValue);

            TooltipHandler.TipRegion(tipRect, tip);
        }

        private static void DrawTraits(WidgetRow rowRTL, IEnumerator<Trait> traits, Pawn pawn)
        {
            if (traits.MoveNext())
            {
                Trait trait = traits.Current;
                TooltipHandler.TipRegion(rowRTL.Label(trait.LabelCap), trait.TipString(pawn));
                rowRTL.Gap(Int32.MaxValue);
            }
        }

        private static void Reset()
        {
            workingInvList.Clear();
            _apparelOverflow.Clear();
            _smartRects.Clear();
        }
    }
}
