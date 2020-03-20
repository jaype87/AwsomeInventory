// <copyright file="DrawGearTabWorker.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using AwesomeInventory.Utilities;
using RimWorld;
using RimWorld.Planet;
using RPG_Inventory_Remake_Common;
using RPGIResource;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AwesomeInventory.UI
{
    using AIBPGDef = AwesomeInventory.AwesomeInventoryBodyPartGroupDefOf;

    /// <summary>
    /// Draw contents for <see cref="ITab_Pawn_Gear"/>.
    /// </summary>
    public abstract class DrawGearTabWorker : IDrawGearTab
    {
        /// <summary>
        /// Scroll position of the gear tab. Should get reset whenever changing the selected pawn.
        /// </summary>
        protected Vector2 _scrollPosition = Vector2.zero;

        /// <summary>
        /// Divides gear tab into left, for displaying apparels, and right, for stats and paper doll.
        /// </summary>
        protected float _divider = 0.35f;

        private const float _margin = 15f;
        private const float _topPadding = 50f;
        private const float _apparelRectWidth = 56f;
        private const float _apparelRectHeight = 56f;
        private const float _startingXforRect = 150f;

        private static float _scrollViewHeight;
        private SmartRectList<Apparel> _smartRectList;

        private Dictionary<StatDef, Tuple<float, string>> _armorStatCache = new Dictionary<StatDef, Tuple<float, string>>();
        private AwesomeInventoryTabBase _gearTab;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawGearTabWorker"/> class.
        /// </summary>
        /// <param name="gearTab"> The gear tab it draws on. </param>
        public DrawGearTabWorker(AwesomeInventoryTabBase gearTab)
        {
            _gearTab = gearTab;

        }

        /// <inheritdoc/>
        public virtual void RestScrollPosition()
        {
            _scrollPosition = Vector2.zero;
        }

        /// <inheritdoc/>
        public virtual void DrawAscetic() { }

        /// <inheritdoc/>
        public virtual void DrawJealous(Pawn selPawn, Rect canvas, bool apparelChanged)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            Rect innerCanvas = canvas.ContractedBy(GenUI.GapSmall);
            GUI.BeginGroup(innerCanvas);
            Rect outRect = innerCanvas.AtZero();
            Rect viewRect = outRect;
            viewRect.height = _scrollViewHeight;
            viewRect.width -= GenUI.Gap;

            // start drawing the view
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            // draw all stats on the right
            Rect statRect = outRect.RightPart(_divider);
            this.DrawStatPanel(statRect, selPawn, out float statY, apparelChanged);

            // Draw paper doll.
            statY -= WidgetRow.IconSize;
            Rect pawnRect = new Rect(new Vector2(statRect.x + GenUI.GapSmall, statY), UtilityConstant.PaperDollSize);
            Utility.DrawColonist(pawnRect, selPawn);

            #region Weapon

            // TODO take a look at how shield is euqipped.
            SmartRect<ThingWithComps> rectForEquipment =
                new SmartRect<ThingWithComps>(
                    template: new Rect(statRect.x, pawnRect.yMax, _apparelRectWidth, _apparelRectHeight),
                    selector: (thing) => { return true; },
                    xLeftCurPosition: pawnRect.x,
                    xRightCurPosition: pawnRect.x,
                    list: null,
                    xLeftEdge: pawnRect.x,
                    xRightEdge: canvas.x - DrawUtility.SmallIconSize);

            SmartRectList<ThingWithComps> equipementRectList = new SmartRectList<ThingWithComps>();
            equipementRectList.Init(rectForEquipment);

            if (Utility.ShouldShowEquipment(selPawn))
            {
                Rect primaryRect = rectForEquipment.NextAvailableRect();
                GUI.DrawTexture(primaryRect, Command.BGTex);
                TooltipHandler.TipRegion(primaryRect, UIText.PrimaryWeapon.Translate());

                foreach (ThingWithComps equipment in selPawn.equipment.AllEquipmentListForReading)
                {
                    if (equipment == selPawn.equipment.Primary)
                    {
                        this.DrawThingIcon(selPawn, primaryRect, equipment);
                    }
                    else
                    {
                        Rect emptyRect = equipementRectList.GetRectFor(equipment);
                        if (emptyRect == default)
                        {
                            emptyRect = equipementRectList.GetWorkingSmartRect(
                                (euipment) => { return true; },
                                pawnRect.x,
                                pawnRect.x).GetRectFor(equipment);
                        }

                        if (emptyRect != default)
                        {
                            this.DrawThingIcon(selPawn, emptyRect, equipment);
                        }
                    }
                }
            }
            #endregion

            #region Apparels

            // List order: Head:200-181, Neck:180-101, Torso:100-51, Waist:50-11, Legs:10-0
            // Check \steamapps\common\RimWorld\Mods\Core\Defs\Bodies\BodyPartGroups.xml
            this.DrawDefaultThingIconRects(selPawn.apparel.WornApparel, outRect.LeftPart(1 - _divider), apparelChanged);
            IEnumerable<Apparel> extraApparels = this.DrawApparels(selPawn, selPawn.apparel.WornApparel, _smartRectList);

            #endregion

            #region Draw Traits

            SmartRect<Apparel> lastSmartRect = _smartRectList.SmartRects.Last();
            float traitY = lastSmartRect.yMax + lastSmartRect.HeightGap;
            WidgetRow traitRow = new WidgetRow(viewRect.x, traitY, UIDirection.RightThenDown, statRect.x - viewRect.x);
            List<Trait> traits = selPawn.story.traits.allTraits;

            traitRow.Label(UIText.Traits.Translate() + ": ");
            for (int i = 0; i < traits.Count; i++)
            {
                TooltipHandler.TipRegion(
                    traitRow.Label(traits[i].LabelCap + (i != traits.Count ? ", " : string.Empty)),
                    traits[i].TipString(selPawn));
            }

            float rollingY = traitRow.FinalY + WidgetRow.IconSize;
            #endregion

            #region Extra Apparels

            // If there is any more remains, put them into their own category
            if (extraApparels.Any())
            {
                rollingY += Utility.StandardLineHeight;
                float x = viewRect.x;
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Corgi_ExtraApparels".Translate());

                foreach (Apparel extraApparel in extraApparels)
                {
                    Rect rect = new Rect(x, rollingY, _apparelRectWidth, _apparelRectHeight);
                    this.DrawThingIcon(selPawn, rect, extraApparel);

                    x += _apparelRectWidth + GenUI.GapSmall;
                    if (x + _apparelRectWidth > viewRect.xMax)
                    {
                        rollingY += _apparelRectHeight + GenUI.GapSmall;
                        x = viewRect.x;
                    }
                }

                rollingY += _apparelRectHeight + GenUI.GapSmall;
            }

            #endregion Extra Apparels

            #region Draw Inventory

            // Balance the y coordinate of the left and right panels.
            if (Utility.ShouldShowInventory(selPawn))
            {
                if (rollingY < equipementRectList.SmartRects.Last().yMax)
                {
                    rollingY = equipementRectList.SmartRects.Last().yMax;
                }

                rollingY += Utility.StandardLineHeight;

                if (Dialog_Mod.Settings.UseLoadout)
                {
                    WidgetRow row = new WidgetRow(viewRect.xMax, rollingY, UIDirection.LeftThenDown, viewRect.width);
                    if (row.ButtonText(UIText.OpenLoadout.Translate()))
                    {
                        if (selPawn.IsColonist && selPawn.GetLoadout() == null)
                        {
                            AILoadout loadout = new AILoadout(selPawn);
                            LoadoutManager.AddLoadout(loadout);
                            selPawn.SetLoadout(loadout);
                        }

                        Find.WindowStack.Add(new Dialog_ManageLoadouts(selPawn.GetLoadout(), selPawn));
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
                                list.Add(new FloatMenuOption(
                                    loadouts[i].label,
                                    () =>
                                    {
                                        selPawn.SetLoadout(loadouts[local_i]);
                                    }));
                            }
                        }

                        Find.WindowStack.Add(new FloatMenu(list));
                    }

                    Text.Anchor = TextAnchor.MiddleRight;
                    Text.WordWrap = false;

                    row.Label(selPawn.GetLoadout()?.label, GenUI.GetWidthCached(UIText.TenCharsString.Times(2.5f)));

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.WordWrap = true;
                    rollingY = row.FinalY + WidgetRow.IconSize - Text.LineHeight;
                }

                Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.Inventory.Translate());

                ThingOwner<Thing> things = selPawn.inventory.innerContainer;
                for (int i = 0; i < things.Count; i++)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, things[i].GetInnerIfMinified());
                }
            }
            #endregion Draw Inventory

            // TODO Add support for smart medicine
            /*
            //if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            //{
            //    smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
            //    .Invoke(null, new object[] { selPawn, rollingY, viewRect.width });
            //}
            */

            if (Event.current.type == EventType.Layout)
            {
                _scrollViewHeight = rollingY + 30f;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        /// <inheritdoc/>
        public virtual void DrawGreedy(Pawn selPawn, Rect canvas, bool apparelChanged)
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
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            float rollingY = 0f;
            // draw mass info and temperature
            Utility.TryDrawMassInfo(selPawn, ref rollingY, viewRect.width);
            Utility.TryDrawComfyTemperatureRange(selPawn, ref rollingY, viewRect.width);

            // draw overall armor
            if (Utility.ShouldShowOverallArmor(selPawn))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, "OverallArmor".Translate());
                Utility.TryDrawOverallArmor(selPawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "%");
                Utility.TryDrawOverallArmor(selPawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), "%");
                Utility.TryDrawOverallArmor(selPawn, ref rollingY, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
            }
            if (Utility.ShouldShowEquipment(selPawn))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Equipment".Translate());
                foreach (ThingWithComps current in selPawn.equipment.AllEquipmentListForReading)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, current);
                }
            }
            if (Utility.ShouldShowApparel(selPawn))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Apparel".Translate());
                foreach (Apparel current2 in from ap in selPawn.apparel.WornApparel
                                             orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                             select ap)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, current2);
                }
            }
            if (Utility.ShouldShowInventory(selPawn))
            {
                if (Dialog_Mod.Settings.UseLoadout)
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
                            , 26f), selPawn.GetLoadout()?.label ?? "Corgi_NoLoadout".Translate());
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
                                    selPawn.SetLoadout(loadouts[local_i]);
                                }));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }

                    Rect loadoutButtonRect = new Rect(viewRect.width / 4 * 3, buttonY, viewRect.width / 4, 26f); // button is half the available width...
                    if (Widgets.ButtonText(loadoutButtonRect, "Corgi_OpenLoadout".Translate()))
                    {
                        if (selPawn.IsColonist && (selPawn.GetLoadout() == null))
                        {
                            AILoadout loadout = new AILoadout(selPawn);
                            LoadoutManager.AddLoadout(loadout);
                            selPawn.SetLoadout(loadout);
                        }

                        Find.WindowStack.Add(new Dialog_ManageLoadouts(selPawn.GetLoadout(), selPawn));
                    }
                }
                Widgets.ListSeparator(ref rollingY, viewRect.width, "Inventory".Translate());

                ThingOwner<Thing> things = selPawn.inventory.innerContainer;
                for (int i = 0; i < things.Count; i++)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, things[i].GetInnerIfMinified());
                }
            }

            //// Add support for smart medicine
            //if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            //{
            //    smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
            //    .Invoke(null, new object[] { selPawn, rollingY, viewRect.width });
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

        /// <summary>
        /// Draw frames, which indicates quality, around <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Target item. </param>
        /// <param name="rect"> Position on screen. </param>
        protected static void DrawQualityFrame(ThingWithComps thing, Rect rect)
        {
            if (thing.TryGetQuality(out QualityCategory c))
            {
                switch (c)
                {
                    case QualityCategory.Legendary:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Lengendary);
                        break;

                    case QualityCategory.Masterwork:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Masterwork);
                        break;

                    case QualityCategory.Excellent:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Excellent);
                        break;

                    case QualityCategory.Good:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Good);
                        break;

                    case QualityCategory.Normal:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Normal);
                        break;

                    case QualityCategory.Poor:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Poor);
                        break;

                    case QualityCategory.Awful:
                        DrawUtility.DrawBoxWithColor(rect, RPGITex.Awful);
                        break;
                }
            }
        }

        /// <summary>
        /// Draw hitpoint background for <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Target item. </param>
        /// <param name="rect"> Position of screen. </param>
        protected static void DrawHitpointBackground(Thing thing, Rect rect)
        {
            ValidateArg.NotNull(thing, nameof(thing));
            Rect hitpointsBG = rect.ContractedBy(2f);
            float hitpointPercentage = hitpointsBG.height * (thing.HitPoints / (float)thing.MaxHitPoints);
            hitpointsBG.yMin = hitpointsBG.yMax - hitpointPercentage;

            // draw background indicator for hitpoints
            GUI.DrawTexture(hitpointsBG, SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.47f, 0.53f, 0.44f)));
            if (thing.HitPoints <= ((float)thing.MaxHitPoints / 2))
            {
                GUI.DrawTexture(hitpointsBG, SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.5f, 0.31f, 0.44f)));
            }
        }

        /// <summary>
        /// Draw thing icon, description and function buttons in a row.
        /// </summary>
        /// <param name="selPawn"> Selected Pawn.</param>
        /// <param name="y"> The yMax coordinate after the row is drawn. </param>
        /// <param name="width"> Width of this row. </param>
        /// <param name="thing"> Thing to draw. </param>
        protected virtual void DrawThingRow(Pawn selPawn, ref float y, float width, Thing thing)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));
            ValidateArg.NotNull(thing, nameof(thing));

            Rect rect = new Rect(0f, y, width, 28f);
            rect.width -= GenUI.SmallIconSize;
            Widgets.InfoCardButton(rect.width, y, thing);
            rect.width -= GenUI.SmallIconSize;
            Rect rect2 = new Rect(rect.width, y, DrawUtility.IconSize, DrawUtility.IconSize);

            // Draw drop button.
            TooltipHandler.TipRegion(rect2, UIText.DropThing.Translate());
            if (Widgets.ButtonImage(rect2, TexResource.Drop))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                AwesomeInventoryTabBase.InterfaceDrop.Invoke(_gearTab, new object[] { thing });
            }

            if (thing is ThingWithComps thingWithComps)
            {
                // Draw unload now button
                rect.width -= 24f;
                Rect rect3 = new Rect(rect.width, y, DrawUtility.IconSize, DrawUtility.IconSize);
                TooltipHandler.TipRegion(rect3, "Corgi_UnloadNow".Translate());
                Texture2D image = ContentFinder<Texture2D>.Get("UI/Icons/DoubleDownArrow", true);
                if (thingWithComps.GetComp<CompRPGIUnload>()?.Unload ?? false)
                {
                    if (Widgets.ButtonImage(rect3, image, DrawUtility.HighlightBrown, DrawUtility.HighlightGreen))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        InterfaceUnloadNow(thingWithComps, selPawn);
                    }
                }
                else
                {
                    if (Widgets.ButtonImage(rect3, image, Color.white, DrawUtility.HighlightGreen))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        InterfaceUnloadNow(thingWithComps, selPawn);
                    }
                }
            }

            // Draw ingest button.
            if ((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && selPawn.WillEat(thing))
            {
                Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(thing.LabelNoCount, thing));
                if (Widgets.ButtonImage(rect3, TexResource.Ingest))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    AwesomeInventoryTabBase.InterfaceIngest.Invoke(_gearTab, new object[] { thing });
                }
            }

            rect.width -= 24f;
            Rect rect4 = rect;
            rect4.xMin = rect4.xMax - 60f;
            CaravanThingsTabUtility.DrawMass(thing, rect4);
            rect.width -= 60f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }

            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);

            // Draw label for thing in a row
            string text = thing.LabelCap;
            bool isForced = thing is Apparel apparel
                         && selPawn.outfits != null
                         && selPawn.outfits.forcedHandler.IsForced(apparel);
            Text.WordWrap = false;
            Widgets.Label(rect5, text.Truncate(rect5.width));
            Text.WordWrap = true;

            string tooltipText = Utility.TooltipTextForThing(thing, false, isForced);

            this.MouseContextMenu(selPawn, thing, rect);
            TooltipHandler.TipRegion(rect, tooltipText);

            y += 28f;
        }

        /// <summary>
        /// Draw carried weight, comfortable temperature and stats of armor.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="rollingY"> The yMax coordinate when stat panel is drawn. </param>
        /// <param name="apparelChanged"> Indicates whether apparels on pawn have changed. </param>
        protected virtual void DrawStatPanel(Rect rect, Pawn pawn, out float rollingY, bool apparelChanged)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

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
                row.Gap(int.MaxValue);
            }

            // Draw minimum comfy temperature
            row.Icon(ContentFinder<Texture2D>.Get(RPGIIcons.MinTemperature, true), UIText.ComfyTemperatureRange.Translate());
            row.Label(pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true).ToStringTemperature());
            row.Gap(GenUI.Gap);

            // Draw maximum comfy temperature
            row.Icon(TexResource.IconFlame, UIText.ComfyTemperatureRange.Translate());
            row.Label(pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true).ToStringTemperature());
            row.Gap(int.MaxValue);

            // Draw armor stats
            DrawArmorStats(row, pawn, StatDefOf.ArmorRating_Blunt, TexResource.ArmorBlunt, apparelChanged);
            DrawArmorStats(row, pawn, StatDefOf.ArmorRating_Sharp, TexResource.ArmorSharp, apparelChanged);
            DrawArmorStats(row, pawn, StatDefOf.ArmorRating_Heat, TexResource.ArmorHeat, apparelChanged);

            rollingY = row.FinalY;
        }

        /// <summary>
        /// Draw sharp, blunt, heat stats for armor.
        /// </summary>
        /// <param name="row"> A drawing helper for drawing in a row. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="stat"> Stat to draw. </param>
        /// <param name="icon"> Icon for <paramref name="stat"/>. </param>
        /// <param name="apparelChanged"> Indicates whether apparels on pawn have changed. </param>
        /// <remarks> It costs 1ms to calculate one stat for a pawn with 16 apparels, therefore the cache. </remarks>
        protected virtual void DrawArmorStats(WidgetRow row, Pawn pawn, StatDef stat, Texture2D icon, bool apparelChanged)
        {
            ValidateArg.NotNull(row, nameof(row));

            Tuple<float, string> tuple;
            if (apparelChanged)
            {
                float value = Utility.CalculateArmorByParts(pawn, stat, out string tip);
                _armorStatCache[stat] = tuple = Tuple.Create(value, tip);
            }
            else
            {
                if (!_armorStatCache.TryGetValue(stat, out tuple))
                {
                    Log.Error("Armor stat is not initiated.");
                }
            }

            row.Icon(icon, UIText.ArmorBlunt.Translate());
            Rect tipRect = row.Label(tuple.Item1.ToStringPercent());

            // Move row to next level.
            row.Gap(int.MaxValue);

            TooltipHandler.TipRegion(tipRect, tuple.Item2);
        }

        /// <summary>
        /// Draw traits on gear tab.
        /// </summary>
        /// <param name="row"> A drawing helper for drawing in a row. </param>
        /// <param name="traits"> Traits to draw. </param>
        /// <param name="pawn"> Selected pawn. </param>
        protected virtual void DrawTraits(WidgetRow row, IEnumerator<Trait> traits, Pawn pawn)
        {
            ValidateArg.NotNull(row, nameof(row));
            ValidateArg.NotNull(traits, nameof(traits));

            if (traits.MoveNext())
            {
                Trait trait = traits.Current;
                TooltipHandler.TipRegion(row.Label(trait.LabelCap), trait.TipString(pawn));
                row.Gap(int.MaxValue);
            }
        }

        /// <summary>
        /// Draw thing icon on <paramref name="rect"/>.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="rect"> Automatically find next available rect to draw on. </param>
        /// <param name="thing"> Thing to draw. </param>
        protected virtual void DrawThingIcon(Pawn selPawn, Rect rect, ThingWithComps thing)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            DrawQualityFrame(thing, rect);
            GUI.DrawTexture(rect, Command.BGTex);
            DrawHitpointBackground(thing, rect);

            // Draw thing icon.
            Rect rect1 = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
            Widgets.ThingIcon(rect1, thing);

            if (Mouse.IsOver(rect))
            {
                GUI.color = Color.grey;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                Widgets.InfoCardButton(rect.x, rect.y, thing);

                Rect buttonRect;

                // Draw Unload Now button
                buttonRect = new Rect(rect.xMax - DrawUtility.SmallIconSize, rect.yMax - DrawUtility.SmallIconSize, DrawUtility.SmallIconSize, DrawUtility.SmallIconSize);
                TooltipHandler.TipRegion(buttonRect, UIText.UnloadNow.Translate());
                if (thing.GetComp<CompRPGIUnload>()?.Unload ?? false)
                {
                    if (Widgets.ButtonImage(buttonRect, TexResource.DoubleDownArrow, DrawUtility.HighlightBrown, DrawUtility.HighlightGreen))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        AwesomeInventoryTabBase.InterfaceUnloadNow(thing, selPawn);
                    }
                }
                else
                {
                    if (Widgets.ButtonImage(buttonRect, TexResource.DoubleDownArrow, Color.white, DrawUtility.HighlightGreen))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        AwesomeInventoryTabBase.InterfaceUnloadNow(thing, selPawn);
                    }
                }

                GUI.color = Color.white;
            }

            // Draw tainted and forced icon.
            bool isForced = false;
            if (thing is Apparel apparel)
            {
                isForced = selPawn.outfits.forcedHandler.IsForced(apparel);
                if (apparel.WornByCorpse)
                {
                    Rect rect3 = new Rect(rect.xMax - DrawUtility.SmallIconSize, rect.y, DrawUtility.SmallIconSize, DrawUtility.SmallIconSize);
                    GUI.DrawTexture(rect3, TexResource.IconTainted);
                    TooltipHandler.TipRegion(rect3, "WasWornByCorpse".Translate());
                }

                if (isForced)
                {
                    Rect rect4 = new Rect(rect.x, rect.yMax - 20f, 20f, 20f);
                    GUI.DrawTexture(rect4, TexResource.IconForced);
                    TooltipHandler.TipRegion(rect4, UIText.ForcedApparel.Translate());
                }
            }

            Text.WordWrap = true;
            if (AwesomeInventoryServiceProvider.TryGetImplementation(out IDrawHelper drawHelper))
            {
                TooltipHandler.TipRegion(rect, drawHelper.TooltipTextFor(thing, true, isForced));
            }

            MouseContextMenu(selPawn, thing, rect);
        }

        /// <summary>
        /// Add equipment option to <paramref name="menuOptions"/>.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="equipment"> Equipment to act on. </param>
        /// <param name="menuOptions"> List to add option to. </param>
        protected virtual void AddEquipmentOption(Pawn selPawn, ThingWithComps equipment, List<FloatMenuOption> menuOptions)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));
            ValidateArg.NotNull(equipment, nameof(equipment));
            ValidateArg.NotNull(menuOptions, nameof(menuOptions));

            if (equipment.def.IsWeapon)
            {
                string labelShort = equipment.LabelShort;
                FloatMenuOption equipOption;

                // Add put away option
                if (selPawn.equipment.AllEquipmentListForReading.Contains(equipment) && selPawn.inventory != null)
                {
                    equipOption = new FloatMenuOption(
                        UIText.PutAway.Translate(labelShort),
                        () =>
                        {
                            selPawn.equipment.TryTransferEquipmentToContainer(selPawn.equipment.Primary, selPawn.inventory.innerContainer);
                        });
                }
                else if (selPawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                {
                    equipOption = new FloatMenuOption(UIText.CannotEquip.Translate(labelShort) + " (" + UIText.IsIncapableOfViolenceLower.Translate(selPawn.LabelShort, selPawn) + ")", null);
                }
                else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    equipOption = new FloatMenuOption(UIText.CannotEquip.Translate(labelShort) + " (" + UIText.Incapable.Translate() + ")", null);
                }
                else
                {
                    // Add equip option
                    string text5 = UIText.Equip.Translate(labelShort);
                    if (equipment.def.IsRangedWeapon && selPawn.story != null && selPawn.story.traits.HasTrait(TraitDefOf.Brawler))
                    {
                        text5 = text5 + " " + UIText.EquipWarningBrawler.Translate();
                    }

                    equipOption = new FloatMenuOption(
                        text5,
                        () =>
                        {
                            if (selPawn.CurJob != null)
                            {
                                selPawn.jobs.StopAll();
                            }

                            // put away equiped weapon first
                            if (selPawn.equipment.Primary != null)
                            {
                                if (!selPawn.equipment.TryTransferEquipmentToContainer(selPawn.equipment.Primary, selPawn.inventory.innerContainer))
                                {
                                    // if failed, drop the weapon
                                    selPawn.equipment.MakeRoomFor(equipment);
                                }
                            }

                            if (selPawn.equipment.Primary == null)
                            {
                                // unregister new weapon in the inventory list and register it in equipment list.
                                selPawn.equipment.GetDirectlyHeldThings().TryAddOrTransfer(equipment);
                            }
                            else
                            {
                                Messages.Message("CannotEquip".Translate(labelShort), MessageTypeDefOf.NeutralEvent);
                            }
                        });
                }

                menuOptions.Add(equipOption);
            }
        }

        /// <summary>
        /// Add equipment option to <paramref name="floatOptionList"/>.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="apparel"> Equipment to act on. </param>
        /// <param name="floatOptionList"> List to add option to. </param>
        protected virtual void AddApparelOption(Pawn selPawn, Apparel apparel, List<FloatMenuOption> floatOptionList)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));
            ValidateArg.NotNull(apparel, nameof(apparel));
            ValidateArg.NotNull(floatOptionList, nameof(floatOptionList));

            Pawn pawn = selPawn;
            string labelShort = apparel.LabelShort;
            FloatMenuOption option = null;

            // Equip option
            if (pawn.inventory.Contains(apparel))
            {
                option = new FloatMenuOption(
                    UIText.VanillaWear.Translate(labelShort),
                    () =>
                    {
                        DressJob dressJob = SimplePool<DressJob>.Get();
                        dressJob.def = AwesomeInventory_JobDefOf.AwesomeInventory_Dress;
                        dressJob.targetA = apparel;
                        dressJob.ForceWear = false;
                        pawn.jobs.TryTakeOrderedJob(dressJob, JobTag.ChangingApparel);
                    });
                floatOptionList.Add(option);

                option = new FloatMenuOption(
                    UIText.AIForceWear.Translate(labelShort),
                    () =>
                    {
                        DressJob dressJob = SimplePool<DressJob>.Get();
                        dressJob.def = AwesomeInventory_JobDefOf.AwesomeInventory_Dress;
                        dressJob.targetA = apparel;
                        dressJob.ForceWear = true;
                        pawn.jobs.TryTakeOrderedJob(dressJob, JobTag.ChangingApparel);
                    });
                floatOptionList.Add(option);
            }

            // Put away option
            if (pawn.apparel.Contains(apparel) && pawn.inventory != null)
            {
                option = new FloatMenuOption(
                    UIText.PutAway.Translate(labelShort),
                    () =>
                    {
                        pawn.jobs.TryTakeOrderedJob(
                            JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_Undress, apparel),
                            JobTag.ChangingApparel);
                    });
                floatOptionList.Add(option);
            }

            // Drop option
            if (pawn.apparel.Contains(apparel) || pawn.inventory.Contains(apparel))
            {
                option = new FloatMenuOption(
                    UIText.DropThing.Translate(),
                    () =>
                    {
                        AwesomeInventoryTabBase.InterfaceDrop.Invoke(_gearTab, new object[] { apparel });
                    });
                floatOptionList.Add(option);
            }
        }

        /// <summary>
        /// Context menu when right click on items.
        /// </summary>
        /// <param name="selPawn"> Pawn who holds <paramref name="thing"/>. </param>
        /// <param name="thing"> Thing that is being right-clicked on. </param>
        /// <param name="rect"> Positino on screen where <paramref name="thing"/> is drawn. </param>
        protected virtual void MouseContextMenu(Pawn selPawn, Thing thing, Rect rect)
        {
            if (Widgets.ButtonInvisible(rect) && Event.current.button == 1)
            {
                // Check if pawn is under control
                if ((bool)AwesomeInventoryTabBase.CanControlColonist.GetValue((ITab_Pawn_Gear)_gearTab))
                {
                    List<FloatMenuOption> floatOptionList = new List<FloatMenuOption>();

                    // Equipment option
                    if (thing is ThingWithComps equipment)
                        this.AddEquipmentOption(selPawn, equipment, floatOptionList);

                    // Apparel option
                    if (thing is Apparel apparel)
                        this.AddApparelOption(selPawn, apparel, floatOptionList);

                    if (floatOptionList.Count > 0)
                    {
                        FloatMenu window = new FloatMenu(floatOptionList);
                        Find.WindowStack.Add(window);
                    }
                }
            }
        }

        /// <summary>
        /// Draw default rects for apparels.
        /// </summary>
        /// <param name="apparels"> An IEnumerable of <see cref="Apparel"/>. </param>
        protected virtual void DrawDefaultThingIconRects(IEnumerable<Apparel> apparels, Rect canvas, bool apparelChanged)
        {
            ValidateArg.NotNull(apparels, nameof(apparels));

            // Hats. BodyGroup: Fullhead; Layer: Overhead
            SmartRect<Apparel> smartRect =
                new SmartRect<Apparel>(
                    template: new Rect(canvas.x, canvas.y, _apparelRectWidth, _apparelRectHeight),
                    (Apparel apparel) =>
                    {
                        return apparel.def.apparel.bodyPartGroups[0].listOrder > AIBPGDef.Neck.listOrder;
                    },
                    xLeftCurPosition: _startingXforRect,
                    xRightCurPosition: _startingXforRect,
                    null,
                    xLeftEdge: 10,
                    xRightEdge: canvas.xMax);

            _smartRectList = new SmartRectList<Apparel>();
            _smartRectList.Init(smartRect);

            float xLeftCurrentPosition = smartRect.XLeftCurrentPosition;
            float xRightCurrentPosition = smartRect.XRightCurrentPosition;

            IEnumerable<Apparel> sortedApparels = from ap in apparels
                                                  orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                                  select ap;

            // Add a default rect for head level.
            smartRect.AddDefaultRect(
                (Apparel apparel) =>
                {
                    return apparel.def.apparel.bodyPartGroups.Contains(AIBPGDef.FullHead)
                        || apparel.def.apparel.bodyPartGroups.Contains(AIBPGDef.UpperHead);
                },
                UIText.Head);

            // Add a smart rect for Neck level if any of apparels is found for the level.
            foreach (Apparel apparel in sortedApparels)
            {
                // If apparel worn below neck, break loop.
                if (apparel.def.apparel.bodyPartGroups[0].listOrder <= AIBPGDef.Torso.listOrder)
                    break;

                if (apparel.def.apparel.bodyPartGroups[0].listOrder <= AIBPGDef.Neck.listOrder)
                {
                    smartRect = smartRect.List.GetWorkingSmartRect(
                        (Apparel app) =>
                        {
                            return app.def.apparel.bodyPartGroups[0].listOrder <= AIBPGDef.Neck.listOrder
                                && app.def.apparel.bodyPartGroups[0].listOrder > AIBPGDef.Torso.listOrder;
                        },
                        xLeftCurrentPosition,
                        xRightCurrentPosition);
                    break;
                }
            }

            // Add a smart rect for torso level.
            smartRect = smartRect.List.GetWorkingSmartRect(
                        (Apparel app) =>
                        {
                            return app.def.apparel.bodyPartGroups[0].listOrder <= AIBPGDef.Torso.listOrder
                                && app.def.apparel.bodyPartGroups[0].listOrder > AIBPGDef.Waist.listOrder;
                        },
                        xLeftCurrentPosition,
                        xRightCurrentPosition);

            // Add three default rects for torso level.
            smartRect.AddDefaultRect(
                (Apparel apparel) =>
                {
                    return apparel.def.apparel.bodyPartGroups[0] == AIBPGDef.Torso
                        && apparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell;
                },
                UIText.TorsoShellLayer);

            smartRect.AddDefaultRect(
                (Apparel apparel) =>
                {
                    return apparel.def.apparel.bodyPartGroups[0] == AIBPGDef.Torso
                        && apparel.def.apparel.LastLayer == ApparelLayerDefOf.Middle;
                },
                UIText.TorsoMiddleLayer);

            smartRect.AddDefaultRect(
                (Apparel apparel) =>
                {
                    return apparel.def.apparel.bodyPartGroups[0] == AIBPGDef.Torso
                        && apparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin;
                },
                UIText.TorsoOnSkinLayer);

            // Add a smart rect for waist level.
            smartRect = smartRect.List.GetWorkingSmartRect(
                        (Apparel app) =>
                        {
                            return app.def.apparel.bodyPartGroups[0].listOrder <= AIBPGDef.Waist.listOrder
                                && app.def.apparel.bodyPartGroups[0].listOrder > AIBPGDef.Legs.listOrder;
                        },
                        xLeftCurrentPosition,
                        xRightCurrentPosition);

            // Add one default rect for waist level.
            smartRect.AddDefaultRect(
                (Apparel apparel) =>
                {
                    return apparel.def.apparel.bodyPartGroups[0] == AIBPGDef.Waist
                        && apparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt;
                },
                UIText.Belt);

            // Add a smart rect for leg level.
            smartRect = smartRect.List.GetWorkingSmartRect(
                        (Apparel app) =>
                        {
                            return app.def.apparel.bodyPartGroups[0].listOrder <= AIBPGDef.Legs.listOrder;
                        },
                        xLeftCurrentPosition,
                        xRightCurrentPosition);

            // Add a default rect for leg level.
            smartRect.AddDefaultRect(
                (Apparel apparel) =>
                {
                    return apparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin;
                },
                UIText.Pant);

            this.DrawDefaultThingIconRectsWorker(_smartRectList);
        }

        /// <summary>
        /// Where it actually starts drawing default rects.
        /// </summary>
        /// <param name="rectList"> Contains smartrects ready for drawing. </param>
        protected virtual void DrawDefaultThingIconRectsWorker(SmartRectList<Apparel> rectList)
        {
            ValidateArg.NotNull(rectList, nameof(rectList));

            foreach (SmartRect<Apparel> smartRect in rectList.SmartRects)
            {
                foreach (IconRect<Apparel> iconRect in smartRect.DefaultRects)
                {
                    GUI.DrawTexture(iconRect.Rect, Command.BGTex);
                    TooltipHandler.TipRegionByKey(iconRect.Rect.ContractedBy(GenUI.GapSmall), iconRect.Tooltip);
                }
            }
        }

        /// <summary>
        /// Draw thing icons for apparels.
        /// </summary>
        /// <param name="selPawn"> Selcted pawn. </param>
        /// <param name="apparels"> Apparels to draw. </param>
        /// <param name="rectList"> A list that holds all <see cref="SmartRect{T}"/> needed for drawing. </param>
        /// <returns> Apparels that cannot fit in the panel. </returns>
        protected virtual IEnumerable<Apparel> DrawApparels(Pawn selPawn, IEnumerable<Apparel> apparels, SmartRectList<Apparel> rectList)
        {
            Queue<Apparel> queue = new Queue<Apparel>(apparels);
            Queue<Apparel> backupQueue = new Queue<Apparel>();

            while (queue.Any())
            {
                Apparel apparel = queue.Dequeue();
                Rect emptyRect = rectList.GetRectFor(apparel);
                if (emptyRect == default)
                {
                    emptyRect = rectList.GetNextBestRectFor(apparel);
                }

                if (emptyRect != default)
                {
                    this.DrawThingIcon(selPawn, emptyRect, apparel);
                }
                else
                {
                    backupQueue.Enqueue(apparel);
                }
            }

            return backupQueue.AsEnumerable();
        }

        /// <summary>
        /// Unload items on pawn.
        /// </summary>
        /// <param name="t"> Thing to unload. </param>
        /// <param name="pawn"> Selected pawn. </param>
        protected virtual void InterfaceUnloadNow(ThingWithComps t, Pawn pawn)
        {
            ValidateArg.NotNull(t, nameof(t));
            ValidateArg.NotNull(pawn, nameof(pawn));

            // TODO examine HaulToContainer code path
            // If there is no comps in def, AllComps will always return an empty list
            // Can't add new comp if the parent class has no comp to begin with
            // The .Any() is not a fool proof test if some mods use it as a dirty way to
            // comps to things that should not have comps
            // Check ThingWithComps for more information
            if (t.AllComps.Any())
            {
                CompRPGIUnload comp = t.GetComp<CompRPGIUnload>();
                if (comp == null)
                {
                    t.AllComps.Add(new CompRPGIUnload(true));
                    JobGiver_AwesomeInventory_Unload.QueueJob(pawn, JobGiver_AwesomeInventory_Unload.TryGiveJobStatic(pawn, t));
                }
                else if (comp.Unload == true)
                {
                    // Check JobGiver_AwesomeInventory_Unload for more information
                    comp.Unload = false;
                    if (pawn.CurJob?.targetA.Thing == t && pawn.CurJobDef == AwesomeInventory_JobDefOf.AwesomeInventory_Unload)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    QueuedJob queuedJob = pawn.jobs.jobQueue.FirstOrDefault(
                            j => j.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Fake &&
                            j.job.targetA.Thing == t);
                    if (queuedJob != null)
                    {
                        pawn.jobs.jobQueue.Extract(queuedJob.job);
                    }
                }
                else if (comp.Unload == false)
                {
                    comp.Unload = true;
                    JobGiver_AwesomeInventory_Unload.QueueJob(pawn, JobGiver_AwesomeInventory_Unload.TryGiveJobStatic(pawn, t));
                }
            }
        }
    }
}
