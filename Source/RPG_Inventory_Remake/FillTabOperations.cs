using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Harmony;
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    public class FillTabOperations
    {
        private const float _barHeight = 20f;
        private const float _margin = 15f;
        private const float _topPadding = 50f;
        private const float _apparelRectWidth = 56f;
        private const float _apparelRectHeight = 56f;
        private const float _startingXforRect = 150f;

        private static readonly Color _highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color _thingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static Vector2 _scrollPosition = Vector2.zero;

        private static SmartRect _smartRectHead;
        private static SmartRect _smartRectCurrent;
        private static List<Thing> workingInvList = new List<Thing>();
        private static List<ThingWithComps> _apparelOverflow = new List<ThingWithComps>();
        private static List<SmartRect> _smartRects = new List<SmartRect>();


        private static float _scrollViewHeight;
        public static void DrawAscetic() { }
        public static void DrawJealous(RPG_Pawn selPawn, Vector2 size)
        {
            // Races that don't have a humanoid body will fall back to Greedy tab
            if (selPawn.Pawn.RaceProps.body != BodyDefOf.Human)
            {
                DrawGreedy(selPawn, size);
                return;
            }
            // set up rects
            Rect listRect = new Rect(
                _margin,
                _topPadding,
                size.x - 2 * _margin,
                size.y - _topPadding - _margin);

            // start drawing the view
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

            // draw equipment (equipment means barrel that can shoot bullets, plank that can slice flesh in half)
            // It is weapon.
            SmartRect rectForEquipment = new SmartRect(new Rect(), CorgiBodyPartGroupDefOf.Arse,
                                                       PawnRect.x, PawnRect.x, null, PawnRect.x, size.x - 20f);
            rectForEquipment.y = PawnRect.yMax;
            rectForEquipment.width = _apparelRectWidth;
            rectForEquipment.height = _apparelRectHeight;
            if (Utility.ShouldShowEquipment(selPawn.Pawn) && (selPawn.Pawn.RaceProps.body == BodyDefOf.Human))
            {
                Rect primaryRect = rectForEquipment.NextAvailableRect();
                GUI.DrawTexture(primaryRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
                TooltipHandler.TipRegion(primaryRect, "Corgi_PrimaryWeapon".Translate());
                foreach (ThingWithComps fireShootingBarrel in selPawn.Pawn.equipment.AllEquipmentListForReading)
                {
                    if (fireShootingBarrel == selPawn.Pawn.equipment.Primary)
                    {
                        Utility.DrawThingRowWithImage(selPawn, primaryRect, fireShootingBarrel);
                    }
                    else
                    {
                        Utility_Draw.DrawThumbnails(selPawn, rectForEquipment, fireShootingBarrel, _apparelOverflow);
                    }
                }
            }

            // List order: Head:200-181, Neck:180-101, Torso:100-51, Waist:50-11, Legs:10-0
            // Check \steamapps\common\RimWorld\Mods\Core\Defs\Bodies\BodyPartGroups.xml
            IEnumerable<Apparel> apparels = from ap in selPawn.Pawn.apparel.WornApparel
                                            orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                            select ap;

            IEnumerator<Apparel> apparelCounter = apparels.GetEnumerator();

            //Hats. BodyGroup: Fullhead; Layer: Overhead
            _smartRectHead = _smartRectCurrent =
                new SmartRect(new Rect(0, 0, _apparelRectWidth, _apparelRectHeight),
                              BodyPartGroupDefOf.UpperHead,
                              _startingXforRect, _startingXforRect,
                              _smartRects, 10, rectStat.x);
            Rect HeadOverRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(HeadOverRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
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
                        Utility_Draw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
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
                                                                _startingXforRect, _startingXforRect);
                        Utility_Draw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
                    }
                    else
                    {
                        cont = true;
                        break;
                    }
                } while (apparelCounter.MoveNext());
            }

            #region Draw Torso
            // Draw apparels on Torso
            _smartRectCurrent = _smartRectCurrent
                                    .GetWorkingRect(CorgiBodyPartGroupDefOf.Torso,
                                                    _startingXforRect, _startingXforRect);

            //Shirts. BodyGroup: Torso; Layer: OnSkin
            Rect TorsoSkinRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(TorsoSkinRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect3 = TorsoSkinRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect3, "Sandy_TorsoOnSkin".Translate());

            //Vests. BodyGroup: Torso; Layer: Middle
            Rect TorsoMidRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(TorsoMidRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect2 = TorsoMidRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect2, "Sandy_TorsoMiddle".Translate());

            //Dusters. BodyGroup: Torso; Layer: Shell
            Rect TorsoShellRect = _smartRectCurrent.NextAvailableRect();
            GUI.DrawTexture(TorsoShellRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
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
                            Utility_Draw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
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
            GUI.DrawTexture(WaistBeltRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
            Rect tipRect5 = WaistBeltRect.ContractedBy(12f);
            TooltipHandler.TipRegion(tipRect5, "Sandy_Belt".Translate());

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
                                                                _startingXforRect,
                                                                _startingXforRect);
                            Utility_Draw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
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
            GUI.DrawTexture(LegSkinRect, ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true));
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
                            Utility_Draw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
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
            Utility_Draw.DrawApparelToNextRow(selPawn, _smartRectHead, _apparelOverflow);

            // If there is any more remains, put them into their own category
            if (_apparelOverflow.Count > 0)
            {
                float rollingY1 = _smartRectCurrent.y + _smartRectCurrent.height + Utility.StandardLineHeight;
                Widgets.ListSeparator(ref rollingY1, viewRect.width, "Corgi_ExtraApparels".Translate());
                _smartRectCurrent = _smartRectCurrent.GetWorkingRect(CorgiBodyPartGroupDefOf.Arse, _margin, _margin);
                _smartRectCurrent.y = rollingY1 + Utility.StandardLineHeight;

                do
                {
                    List<ThingWithComps> tempList = new List<ThingWithComps>(_apparelOverflow);
                    _apparelOverflow.Clear();
                    foreach (Apparel apparel in tempList)
                    {
                        Utility_Draw.DrawThumbnails(selPawn, _smartRectCurrent, apparel, _apparelOverflow);
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
            }
            #endregion Extra Apparels

            #region Draw Inventory
            // Draw inventory
            float rollingY = 0;
            if (Utility.ShouldShowInventory(selPawn.Pawn))
            {

                if (_smartRectCurrent.Rect.yMax > rectForEquipment.Rect.yMax)
                {
                    rollingY = _smartRectCurrent.Rect.yMax;
                }
                else
                {
                    rollingY = rectForEquipment.Rect.yMax;
                }
                rollingY += Utility.StandardLineHeight;

                Widgets.ListSeparator(ref rollingY, viewRect.width, "Inventory".Translate());


                workingInvList.Clear();
                workingInvList.AddRange(selPawn.Pawn.inventory.innerContainer);
                for (int i = 0; i < workingInvList.Count; i++)
                {
                    Utility.DrawThingRow(selPawn, ref rollingY, viewRect.width, workingInvList[i].GetInnerIfMinified(), true);
                }
            }
            #endregion Draw Inventory

            // Add support for smart medicine
            if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            {
                smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { selPawn.Pawn, rollingY, viewRect.width });
            }

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
        public static void DrawGreedy(RPG_Pawn selPawn, Vector2 size)
        {
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

                    Widgets.Label(new Rect(viewRect.width / 4, buttonY, viewRect.width / 4, 26f), selPawn.Pawn.GetLoadout()?.Label ?? "Corgi_NoLoadout".Translate());

                    // Select loadout button
                    if (Widgets.ButtonText(new Rect(viewRect.width / 2, buttonY, viewRect.width / 4, 26f), Translator.Translate("Corgi_SelectLoadout"), true, false, true))
                    {
                        List<RPGILoadout<Thing>> loadouts = LoadoutManager.Loadouts;
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        if (loadouts.Count == 0)
                        {
                            list.Add(new FloatMenuOption(Translator.Translate("Corgi_NoLoadout"), null));
                        }
                        else
                        {
                            for (int i = 0; i < loadouts.Count; i++)
                            {
                                int local_i = i;
                                list.Add(new FloatMenuOption(loadouts[i].Label, delegate
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
                            RPGILoadout<Thing> loadout = new RPGILoadout<Thing>(selPawn.Pawn);
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

            // Add support for smart medicine
            if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            {
                smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { selPawn.Pawn, rollingY, viewRect.width });
            }

            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = rollingY + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }





        private static void Reset()
        {
            workingInvList.Clear();
            _apparelOverflow.Clear();
            _smartRects.Clear();
        }
    }
}
