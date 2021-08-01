// <copyright file="DrawGearTabWorker.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using RimWorld;
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

        /// <summary>
        /// Cache for stat values.
        /// </summary>
        protected Dictionary<StatDef, Tuple<float, string>> _statCache = new Dictionary<StatDef, Tuple<float, string>>();

        private const float _apparelRectWidth = 56f;
        private const float _apparelRectHeight = 56f;
        private const float _startingXforRect = 150f;
        private static readonly Vector2 PaperDollSize = new Vector2(128f, 128f);

        private static float _scrollViewHeight;
        private SmartRectList<Apparel> _smartRectList;

        private Dictionary<Thing, Tuple<string, string>> _thingTooltipCache = new Dictionary<Thing, Tuple<string, string>>();
        private Dictionary<Pawn, List<Tuple<Trait, string>>> _traitCache = new Dictionary<Pawn, List<Tuple<Trait, string>>>();
        private AwesomeInventoryTabBase _gearTab;
        private DrawHelper _drawHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawGearTabWorker"/> class.
        /// </summary>
        /// <param name="gearTab"> The gear tab it draws on. </param>
        public DrawGearTabWorker(AwesomeInventoryTabBase gearTab)
        {
            _gearTab = gearTab;
        }

        /// <summary>
        /// Gets draw helper provided either by vanilla or CE implementation of this mod.
        /// </summary>
        protected DrawHelper DrawHelper
        {
            get
            {
                if (_drawHelper == null)
                {
                    if (AwesomeInventoryServiceProvider.TryGetImplementation(out DrawHelper drawHelper))
                        _drawHelper = drawHelper;
                    else
                        Log.Error(ErrorText.DrawHelperIsMissing);
                }

                return _drawHelper;
            }
        }

        /// <inheritdoc/>
        public virtual void Reset()
        {
            _scrollPosition = Vector2.zero;
            _thingTooltipCache.Clear();
        }

        /// <inheritdoc/>
        public virtual void DrawAscetic()
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Bug in style cop.")]
        public virtual void DrawJealous(Pawn selPawn, Rect canvas, bool apparelChanged)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            Rect outRect = this.SetOutRectForJealousTab(canvas);
            Rect viewRect = outRect;
            viewRect.height = _scrollViewHeight;
            viewRect.width -= GenUI.ScrollBarWidth;

            // start drawing the view
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            // draw all stats on the right
            Rect statRect = viewRect.RightPart(_divider);
            this.DrawStatPanel(statRect, selPawn, out float statY, apparelChanged);

            // Draw paper doll.
            Rect pawnRect = new Rect(new Vector2(statRect.x + GenUI.GapSmall, statY), PaperDollSize);
            Utility.DrawColonist(pawnRect, selPawn);

        #region Weapon

            // TODO take a look at how shield is equipped.
            SmartRect<ThingWithComps> rectForEquipment =
                new SmartRect<ThingWithComps>(
                    template: new Rect(statRect.x, pawnRect.yMax, _apparelRectWidth, _apparelRectHeight),
                    selector: (thing) => { return true; },
                    xLeftCurPosition: pawnRect.x,
                    xRightCurPosition: pawnRect.x,
                    list: null,
                    xLeftEdge: pawnRect.x,
                    xRightEdge: outRect.xMax - GenUI.ScrollBarWidth);

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
            this.DrawDefaultThingIconRects(selPawn.apparel.WornApparel, viewRect.LeftPart(1 - _divider), apparelChanged);
            IEnumerable<Apparel> extraApparels = this.DrawApparels(selPawn, selPawn.apparel.WornApparel, _smartRectList);

            #endregion

            #region Draw Traits

            SmartRect<Apparel> lastSmartRect = _smartRectList.SmartRects.Last();
            float traitY = lastSmartRect.yMax + lastSmartRect.HeightGap;
            WidgetRow traitRow = new WidgetRow(viewRect.x, traitY, UIDirection.RightThenDown, statRect.x - viewRect.x);

            this.DrawTraits(traitRow, selPawn);

            float rollingY = traitRow.FinalY + WidgetRow.IconSize;
            #endregion

            #region Extra Apparels

            // If there is any more remains, put them into their own category
            if (extraApparels.Any())
            {
                rollingY += Utility.StandardLineHeight;
                float x = viewRect.x;
                Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.ExtraApparels.TranslateSimple());

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

                if (AwesomeInventoryMod.Settings.UseLoadout && selPawn.IsColonist && !selPawn.IsQuestLodger())
                    this.DrawLoadoutButtons(selPawn, viewRect.xMax, ref rollingY, viewRect.width);

                ThingOwner<Thing> things = selPawn.inventory.innerContainer;
                if (!things.Any())
                    Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.Inventory.Translate());

                this.DrawInventory(things, selPawn, ref rollingY, viewRect.width);
            }
        #endregion Draw Inventory

            _scrollViewHeight = rollingY + InspectPaneUtility.TabHeight;

            Widgets.EndScrollView();

            this.DrawWeightBar(new Rect(outRect.x, outRect.yMax, viewRect.width, GenUI.SmallIconSize), selPawn);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        /// <inheritdoc/>
        public virtual void DrawGreedy(Pawn selPawn, Rect canvas, bool apparelChanged)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            // start drawing list
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            Rect outRect = new Rect(canvas.x, canvas.y, canvas.width, canvas.height - GenUI.ListSpacing);

            Rect viewRect = outRect;
            viewRect.height = _scrollViewHeight;
            viewRect.width -= GenUI.ScrollBarWidth;

            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            float rollingY;
            WidgetRow row = new WidgetRow(viewRect.x, viewRect.y, UIDirection.RightThenDown, viewRect.width);

            // draw mass info and temperature
            // this.DrawMassInfoRow(row, selPawn, apparelChanged);
            this.DrawComfyTemperatureRow(row, selPawn, apparelChanged);
            rollingY = row.FinalY;

            // draw overall armor
            Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.OverallArmor.TranslateSimple());
            row.Init(viewRect.x, rollingY, UIDirection.RightThenDown, viewRect.width);

            this.DrawArmorStatsRow(row, selPawn, StatDefOf.ArmorRating_Sharp, UIText.ArmorSharp.TranslateSimple(), apparelChanged);
            this.DrawArmorStatsRow(row, selPawn, StatDefOf.ArmorRating_Blunt, UIText.ArmorBlunt.TranslateSimple(), apparelChanged);
            this.DrawArmorStatsRow(row, selPawn, StatDefOf.ArmorRating_Heat, UIText.ArmorHeat.TranslateSimple(), apparelChanged);
            rollingY = row.FinalY;

            if ((bool)AwesomeInventoryTabBase.ShouldShowEquipment.Invoke(_gearTab, new object[] { selPawn }))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.Equipment.TranslateSimple());
                foreach (ThingWithComps equipment in selPawn.equipment.AllEquipmentListForReading)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, equipment, false);
                }
            }

            if ((bool)AwesomeInventoryTabBase.ShouldShowApparel.Invoke(_gearTab, new object[] { selPawn }))
            {
                Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.Apparel.TranslateSimple());
                foreach (Apparel apparel in from ap in selPawn.apparel.WornApparel
                                            orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                            select ap)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, apparel, false);
                }
            }

            if ((bool)AwesomeInventoryTabBase.ShouldShowInventory.Invoke(_gearTab, new object[] { selPawn }))
            {
                if (AwesomeInventoryMod.Settings.UseLoadout && selPawn.IsColonist && !selPawn.IsQuestLodger())
                {
                    this.DrawLoadoutButtons(selPawn, viewRect.xMax, ref rollingY, viewRect.width);
                }

                Widgets.ListSeparator(ref rollingY, viewRect.width, UIText.Inventory.TranslateSimple());

                ThingOwner<Thing> things = selPawn.inventory.innerContainer;
                for (int i = 0; i < things.Count; i++)
                {
                    this.DrawThingRow(selPawn, ref rollingY, viewRect.width, things[i], true);
                }
            }

            //// Add support for smart medicine
            /*
            //if (AccessTools.TypeByName("SmartMedicine.FillTab_Patch") is Type smartMedicine)
            //{
            //    smartMedicine.GetMethod("DrawStockUpButton", BindingFlags.Public | BindingFlags.Static)
            //    .Invoke(null, new object[] { selPawn, rollingY, viewRect.width });
            //}
            */

            _scrollViewHeight = rollingY + InspectPaneUtility.TabHeight;

            Widgets.EndScrollView();

            this.DrawWeightBar(new Rect(outRect.x, outRect.yMax, viewRect.width, GenUI.SmallIconSize), selPawn);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Draw frames, which indicates quality, around <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Target item. </param>
        /// <param name="rect"> Position on screen. </param>
        public static void DrawQualityFrame(ThingWithComps thing, Rect rect)
        {
            if (thing == null)
                return;

            if (thing.TryGetQuality(out QualityCategory c))
            {
                switch (c)
                {
                    case QualityCategory.Legendary:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.LegendaryTex, 2);
                        break;

                    case QualityCategory.Masterwork:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.MasterworkTex, 2);
                        break;

                    case QualityCategory.Excellent:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.ExcellentTex, 2);
                        break;

                    case QualityCategory.Good:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.GoodTex, 2);
                        break;

                    case QualityCategory.Normal:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.NormalTex, 2);
                        break;

                    case QualityCategory.Poor:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.PoorTex, 2);
                        break;

                    case QualityCategory.Awful:
                        DrawUtility.DrawBoxWithColor(rect, QualityColor.Instance.AwfulTex, 2);
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
        /// Draw weight bar for current mass <paramref name="selPawn"/> carries.
        /// </summary>
        /// <param name="rect"> Rect for drawing. </param>
        /// <param name="selPawn"> Selected pawn. </param>
        protected virtual void DrawWeightBar(Rect rect, Pawn selPawn)
        {
            Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);

            Rect bar1 = rect.ReplaceY(rect.y + GenUI.GapTiny);
            GenBar.BarWithOverlay(
                bar1,
                MassUtility.EncumbrancePercent(selPawn),
                MassUtility.IsOverEncumbered(selPawn) ? AwesomeInventoryTex.ValvetTex as Texture2D : AwesomeInventoryTex.RWPrimaryTex as Texture2D,
                UIText.Weight.TranslateSimple(),
                MassUtility.GearAndInventoryMass(selPawn).ToString("0.#") + "/" + MassUtility.Capacity(selPawn).ToStringMass(),
                this.DrawHelper.WeightTextFor(selPawn));
        }

        /// <summary>
        /// Draw loadout buttons from the right on gear tab.
        /// </summary>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="x"> Start position for drawing buttons. </param>
        /// <param name="rollingY"> Y position. </param>
        /// <param name="width"> Width of available space for drawing. </param>
        protected virtual void DrawLoadoutButtons(Pawn selPawn, float x, ref float rollingY, float width)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            CompAwesomeInventoryLoadout comp = selPawn.TryGetComp<CompAwesomeInventoryLoadout>();

            bool openLoadout = false;
            if (AwesomeInventoryMod.Settings.UseLoadout && comp != null)
            {
                List<FloatMenuOption> loadoutOptions = BuildMenuOptions(LoadoutManager.Loadouts.Where(l => l.GetType() == typeof(AwesomeInventoryLoadout)).OfType<Outfit>().ToList());
                List<FloatMenuOption> outfitOptions = BuildMenuOptions(Current.Game.outfitDatabase.AllOutfits.Where(o => o.GetType() != typeof(AwesomeInventoryCostume)).ToList() ?? Enumerable.Empty<Outfit>().ToList());

                WidgetRow row = new WidgetRow(x, rollingY, UIDirection.LeftThenDown, width);
                if (row.ButtonText(UIText.OpenLoadout.TranslateSimple()))
                {
                    if (selPawn.outfits?.CurrentOutfit is AwesomeInventoryLoadout)
                    {
                        Find.WindowStack.Add(
                            AwesomeInventoryServiceProvider.MakeInstanceOf<Dialog_ManageLoadouts>(selPawn.outfits.CurrentOutfit, selPawn, false));
                    }
                    else
                    {
                        openLoadout = true;
                        Find.WindowStack.Add(new FloatMenu(selPawn.MakeActionableLoadoutOption().Concat(loadoutOptions).ToList(), UIText.ChooseLoadut.TranslateSimple()));
                    }
                }

                string costumeButtonText;
                if (selPawn.outfits?.CurrentOutfit is AwesomeInventoryLoadout loadout)
                {
                    IList<AwesomeInventoryCostume> costumes;
                    if (loadout is AwesomeInventoryCostume costume)
                    {
                        costumeButtonText = costume.label;
                        costumes = costume.Base.Costumes;
                    }
                    else if (loadout.Costumes.Any())
                    {
                        costumeButtonText = UIText.SelectCostume.TranslateSimple();
                        costumes = loadout.Costumes;
                    }
                    else
                    {
                        costumeButtonText = UIText.NoCostumeAvailable.TranslateSimple();
                        costumes = Enumerable.Empty<AwesomeInventoryCostume>().ToList();
                    }

                    if (row.ButtonText(costumeButtonText))
                    {
                        openLoadout = false;
                        if (costumes.Any())
                        {
                            Find.WindowStack.Add(
                                new FloatMenu(
                                    BuildMenuOptions(costumes.OfType<Outfit>().ToList())
                                    .Concat(new FloatMenuOption(
                                        UIText.DontUseCostume.TranslateSimple()
                                        , () =>
                                        {
                                            selPawn.outfits.CurrentOutfit = (loadout as AwesomeInventoryCostume)?.Base ?? loadout;

                                            if (BetterPawnControlUtility.IsActive)
                                                BetterPawnControlUtility.SaveState(new List<Pawn> { selPawn });
                                        }))
                                    .ToList()));
                        }
                    }
                }
                else
                {
                    row.ButtonText(UIText.NoCostumeAvailable.TranslateSimple());
                }

                string selectButtonText = selPawn.outfits?.CurrentOutfit != null
                                          ? (selPawn.outfits.CurrentOutfit.GetType() != typeof(AwesomeInventoryCostume))
                                            ? selPawn.outfits.CurrentOutfit.label
                                            : (selPawn.outfits.CurrentOutfit as AwesomeInventoryCostume).Base.label
                                          : UIText.NoLoadout.TranslateSimple();
                if (row.ButtonText(selectButtonText))
                {
                    openLoadout = false;
                    Find.WindowStack.Add(new FloatMenu(outfitOptions.ToList()));
                }

                rollingY = row.FinalY + WidgetRow.IconSize;
            }

            List<FloatMenuOption> BuildMenuOptions(IList<Outfit> outfits)
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                if (outfits == null)
                    return options;

                for (int i = 0; i < outfits.Count; i++)
                {
                    int local_i = i;
                    options.Add(new FloatMenuOption(
                        outfits[local_i].label,
                        () =>
                        {
                            selPawn.outfits.CurrentOutfit = outfits[local_i];
                            if (openLoadout && outfits[local_i] is AwesomeInventoryLoadout loadout)
                            {
                                Find.WindowStack.Add(
                                    AwesomeInventoryServiceProvider.MakeInstanceOf<Dialog_ManageLoadouts>(loadout, selPawn, false));
                            }

                            if (BetterPawnControlUtility.IsActive)
                                BetterPawnControlUtility.SaveState(new List<Pawn> { selPawn });
                        }));
                }

                return options;
            }
        }

        /// <summary>
        /// Get armor stats for <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="stat"> Stat for armor rating. </param>
        /// <param name="apparelChanged"> Indicates if apparels have changed since last call. </param>
        /// <returns> A tuple contains value and tooltip for <paramref name="stat"/>. </returns>
        protected virtual Tuple<float, string> GetArmorStat(Pawn pawn, StatDef stat, bool apparelChanged)
        {
            Tuple<float, string> tuple;
            if (apparelChanged)
            {
                float value = Utility.CalculateArmorByParts(pawn, stat, out string tip);
                _statCache[stat] = tuple = Tuple.Create(value, tip);
            }
            else
            {
                if (!_statCache.TryGetValue(stat, out tuple))
                {
                    Log.Error("Armor stat is not initiated.");
                }
            }

            return tuple;
        }

        /// <summary>
        /// Draw armor stats row for greedy tab.
        /// </summary>
        /// <param name="row"> A <see cref="WidgetRow"/> initialized to a certain size of canvas. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="stat"> Stat to draw. </param>
        /// <param name="label"> Label for <paramref name="stat"/>. </param>
        /// <param name="apparelChanged"> Indicates if apparels have changed since last call. </param>
        protected virtual void DrawArmorStatsRow(WidgetRow row, Pawn pawn, StatDef stat, string label, bool apparelChanged)
        {
            ValidateArg.NotNull(row, nameof(row));

            Tuple<float, string> tuple = this.GetArmorStat(pawn, stat, apparelChanged);
            row.Label(label);
            row.Gap((WidgetRow.LabelGap * 120) - row.FinalX);
            row.Label(Utility.FormatArmorValue(tuple.Item1, "%"));
            Rect tipRegion = new Rect(0, row.FinalY, row.FinalX, WidgetRow.IconSize);
            row.Gap(int.MaxValue);

            TooltipHandler.TipRegion(tipRegion, tuple.Item2);
            Widgets.DrawHighlightIfMouseover(tipRegion);
        }

        /// <summary>
        /// Draw mass info row for greedy tab.
        /// </summary>
        /// <param name="row"> A <see cref="WidgetRow"/> initialized to a certain size of canvas. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="apparelChanged"> Indicates if apparels have changed since last call. </param>
        protected virtual void DrawMassInfoRow(WidgetRow row, Pawn pawn, bool apparelChanged)
        {
            ValidateArg.NotNull(row, nameof(row));

            float carriedMass = MassUtility.GearAndInventoryMass(pawn);
            float capacity = MassUtility.Capacity(pawn);
            row.Label(UIText.MassCarried.Translate(carriedMass.ToString("0.##"), capacity.ToString("0.##")));
            row.Gap(int.MaxValue);
        }

        /// <summary>
        /// Draw comfy temperature for greedy tab.
        /// </summary>
        /// <param name="row"> A <see cref="WidgetRow"/> initialized to a certain size of canvas. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="apparelChanged"> Indicates if apparels have changed since last call. </param>
        protected virtual void DrawComfyTemperatureRow(WidgetRow row, Pawn pawn, bool apparelChanged)
        {
            ValidateArg.NotNull(row, nameof(row));
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (pawn.Dead)
                return;

            row.Label(
                string.Concat(
                    new string[]
                    {
                        UIText.ComfyTemperatureRange.Translate(),
                        ": ",
                        this.GetTemperatureStats(pawn, StatDefOf.ComfyTemperatureMin, apparelChanged).ToStringTemperature("F0"),
                        "~",
                        this.GetTemperatureStats(pawn, StatDefOf.ComfyTemperatureMax, apparelChanged).ToStringTemperature("F0"),
                    }));
            row.Gap(int.MaxValue);
        }

        /// <summary>
        /// Draw thing icon, description and function buttons in a row.
        /// </summary>
        /// <param name="selPawn"> Selected Pawn.</param>
        /// <param name="y"> The yMax coordinate after the row is drawn. </param>
        /// <param name="width"> Width of this row. </param>
        /// <param name="thing"> Thing to draw. </param>
        /// <param name="inventory"> Whether <paramref name="thing"/> is in inventory section. </param>
        protected virtual void DrawThingRow(Pawn selPawn, ref float y, float width, Thing thing, bool inventory)
        {
            ValidateArg.NotNull(selPawn, nameof(selPawn));
            ValidateArg.NotNull(thing, nameof(thing));

            float xInfoButton = width - GenUI.SmallIconSize;
            Widgets.InfoCardButton(xInfoButton, y, thing);

            WidgetRow row = new WidgetRow(xInfoButton, y, UIDirection.LeftThenDown, xInfoButton);

            if (this.ShowDropButton(selPawn, thing, inventory, out bool canDrop, out string tooltip))
            {
                if (canDrop)
                {
                    if (row.ButtonIcon(TexResource.Drop, tooltip))
                    {
                        AwesomeInventoryTabBase.InterfaceDrop.Invoke(_gearTab, new object[] { thing });
                    }

                    Rect unloadButtonRect = new Rect(row.FinalX - GenUI.SmallIconSize, row.FinalY, GenUI.SmallIconSize, GenUI.ListSpacing);

                    if (_gearTab.IsColonistPlayerControlled())
                    {
                        // Draw unload now button
                        TooltipHandler.TipRegion(unloadButtonRect, UIText.UnloadNow.TranslateSimple());
                        if (UnloadNowUtility.ThingInQueue(selPawn, thing))
                        {
                            if (Widgets.ButtonImage(unloadButtonRect, TexResource.DoubleDownArrow, AwesomeInventoryTex.HighlightBrown, AwesomeInventoryTex.HighlightGreen))
                            {
                                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                                InterfaceUnloadNow(thing, selPawn);
                            }
                        }
                        else
                        {
                            if (Widgets.ButtonImage(unloadButtonRect, TexResource.DoubleDownArrow, Color.white, AwesomeInventoryTex.HighlightGreen))
                            {
                                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                                InterfaceUnloadNow(thing, selPawn);
                            }
                        }
                        
                        row.Gap(GenUI.SmallIconSize + 4f);
                    }
                }
                else
                {
                    Rect iconRect = new Rect(row.FinalX - GenUI.SmallIconSize, row.FinalY, GenUI.SmallIconSize, GenUI.SmallIconSize);
                    Widgets.ButtonImage(iconRect, TexResource.Drop, Color.grey, Color.grey, false);
                    TooltipHandler.TipRegion(iconRect, tooltip);
                    row.Gap(GenUI.SmallIconSize + 4f);
                }
            }

            GUI.color = Color.white;

            // Draw ingest button.
            if ((bool)AwesomeInventoryTabBase.CanControlColonist.GetValue(_gearTab) && (thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow && selPawn.WillEat(thing))
            {
                if (row.ButtonIcon(TexResource.Ingest, UIText.ConsumeThing.Translate(thing.LabelNoCount, thing)))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    AwesomeInventoryTabBase.InterfaceIngest.Invoke(_gearTab, new object[] { thing });
                }
            }
            else
            {
                row.Gap(4f);
            }

            // Draw mass.
            row.Label((thing.GetStatValue(StatDefOf.Mass) * thing.stackCount).ToStringMass());

            Rect labelRect = new Rect(0, row.FinalY, row.FinalX, GenUI.ListSpacing);
            if (Mouse.IsOver(labelRect))
            {
                // Get tooltip.
                if (!_thingTooltipCache.TryGetValue(thing, out Tuple<string, string> tuple))
                {
                    _thingTooltipCache[thing] = Tuple.Create(this.DrawHelper.TooltipTextFor(thing, true), this.DrawHelper.TooltipTextFor(thing, false));
                    tuple = _thingTooltipCache[thing];
                }

                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(labelRect, TexUI.HighlightTex);
                this.MouseContextMenu(selPawn, thing, labelRect);
                TooltipHandler.TipRegion(labelRect, tuple.Item2);
            }

            // Draw icon.
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(labelRect.position, new Vector2(GenUI.ListSpacing, GenUI.ListSpacing)), thing);
                labelRect.x += GenUI.ListSpacing;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;

            // Draw label.
            string text = thing.LabelCap.ColorizeByQuality(thing);
            bool isForced = thing is Apparel apparel
                         && selPawn.outfits != null
                         && selPawn.outfits.forcedHandler.IsForced(apparel);
            Text.WordWrap = false;
            string trimmedText = text.Truncate(labelRect.width - GenUI.SmallIconSize);
            Widgets.Label(labelRect, trimmedText);
            if (isForced)
            {
                Widgets.ButtonImage(new Rect(labelRect.x + Text.CalcSize(trimmedText).x + WidgetRow.LabelGap, labelRect.y, GenUI.SmallIconSize, GenUI.SmallIconSize), TexResource.IconForced);
            }

            Text.WordWrap = true;

            y += GenUI.ListSpacing;
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

            Text.Anchor = TextAnchor.MiddleLeft;
            WidgetRow row = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown, rect.width);

            float minTemperature = this.GetTemperatureStats(pawn, StatDefOf.ComfyTemperatureMin, apparelChanged);
            float maxTemperature = this.GetTemperatureStats(pawn, StatDefOf.ComfyTemperatureMax, apparelChanged);
            float ambientTemperature = pawn.AmbientTemperature;

            row.Icon(TexResource.ThermometerGen, UIText.CurrentTemperature.TranslateSimple());
            row.LabelWithHighlight(
                ambientTemperature < minTemperature
                    ? ambientTemperature.ToStringTemperature().Colorize(ColorLibrary.SkyBlue)
                    : ambientTemperature > maxTemperature
                    ? ambientTemperature.ToStringTemperature().Colorize(ColorLibrary.Red)
                    : ambientTemperature.ToStringTemperature()
                , UIText.CurrentTemperature.TranslateSimple());
            row.Gap(int.MaxValue);

            // Draw minimum comfy temperature
            row.Icon(TexResource.ThermometerCold, UIText.ComfyTemperatureRange.TranslateSimple());
            row.LabelWithHighlight(minTemperature.ToStringTemperature(), UIText.ComfyTemperatureRange.TranslateSimple());
            row.Gap(GenUI.Gap);

            // Draw maximum comfy temperature
            row.Icon(TexResource.ThermometerHot, UIText.ComfyTemperatureRange.TranslateSimple());
            row.LabelWithHighlight(maxTemperature.ToStringTemperature(), UIText.ComfyTemperatureRange.TranslateSimple());
            row.Gap(int.MaxValue);

            // Draw armor stats
            this.DrawArmorStats(row, pawn, apparelChanged);

            Text.Anchor = TextAnchor.UpperLeft;
            rollingY = row.FinalY;
        }

        /// <summary>
        /// Draw armor stats on gear tab.
        /// </summary>
        /// <param name="row"> Drawing helper. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="apparelChanged"> If true, worn apparels have changed since last read. </param>
        protected virtual void DrawArmorStats(WidgetRow row, Pawn pawn, bool apparelChanged)
        {
            Func<float, string> numToString = (float value) => value.ToStringPercent("0.#");
            this.DrawArmorStatsWorker(row, pawn, StatDefOf.ArmorRating_Blunt, numToString, TexResource.ArmorBlunt, UIText.ArmorBlunt.TranslateSimple(), apparelChanged, false);
            this.DrawArmorStatsWorker(row, pawn, StatDefOf.ArmorRating_Sharp, numToString, TexResource.ArmorSharp, UIText.ArmorSharp.TranslateSimple(), apparelChanged, false);
            this.DrawArmorStatsWorker(row, pawn, StatDefOf.ArmorRating_Heat, numToString, TexResource.ArmorHeat, UIText.ArmorHeat.TranslateSimple(), apparelChanged, true);
        }

        /// <summary>
        /// Draw mass info in Jealous tab.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="row"> Helper to draw contents in a row. </param>
        protected virtual void DrawMassInfo(Pawn pawn, WidgetRow row)
        {
            ValidateArg.NotNull(row, nameof(row));

            if (Utility.ShouldShowInventory(pawn))
            {
                float carried = MassUtility.GearAndInventoryMass(pawn);
                float capacity = MassUtility.Capacity(pawn);

                row.Icon(TexResource.Mass, UIText.AIMassCarried.TranslateSimple());
                row.Label(string.Concat(carried, "/", capacity));
                row.Gap(int.MaxValue);
            }
        }

        /// <summary>
        /// Get comfortable temperature stats for <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="stat"> Temperature stat. </param>
        /// <param name="apparelChanged"> Indicates if apparels have changed since last call. </param>
        /// <returns> Value for comfortable temperature. </returns>
        protected virtual float GetTemperatureStats(Pawn pawn, StatDef stat, bool apparelChanged)
        {
            float value;
            if (apparelChanged)
            {
                value = pawn.GetStatValue(stat);
                _statCache[stat] = Tuple.Create(value, string.Empty);
            }
            else
            {
                value = _statCache[stat].Item1;
            }

            return value;
        }

        /// <summary>
        /// Draw sharp, blunt, heat stats for armor in jealous tab.
        /// </summary>
        /// <param name="row"> A drawing helper for drawing in a row. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="stat"> Stat to draw. </param>
        /// <param name="numToString"> Transform float value to string presentation. </param>
        /// <param name="icon"> Icon for <paramref name="stat"/>. </param>
        /// <param name="altIconText"> Description for <paramref name="icon"/>. </param>
        /// <param name="apparelChanged"> Indicates whether apparels on pawn have changed. </param>
        /// <param name="changeLine"> If true, move <paramref name="row"/> to next line. </param>
        /// <remarks> It costs 1ms to calculate one stat for a pawn with 16 apparels, therefore the cache. </remarks>
        protected virtual void DrawArmorStatsWorker(WidgetRow row, Pawn pawn, StatDef stat, Func<float, string> numToString, Texture2D icon, string altIconText, bool apparelChanged, bool changeLine)
        {
            ValidateArg.NotNull(row, nameof(row));
            ValidateArg.NotNull(numToString, nameof(numToString));

            Tuple<float, string> tuple = this.GetArmorStat(pawn, stat, apparelChanged);

            if (row.AvailableWidth() < WidgetRow.IconSize + WidgetRow.DefaultGap + Text.CalcSize(numToString(tuple.Item1)).x)
                row.Gap(int.MaxValue);

            Rect iconRect = row.Icon(icon, string.Empty);
            Rect numberRect = row.Label(numToString(tuple.Item1));
            Rect tipRect = new Rect(iconRect) { xMax = numberRect.xMax };

            if (changeLine)
                row.Gap(int.MaxValue);

            TooltipHandler.TipRegion(tipRect, string.Concat(altIconText, Environment.NewLine, tuple.Item2));
            Widgets.DrawHighlightIfMouseover(tipRect);
        }

        /// <summary>
        /// Draw traits on gear tab.
        /// </summary>
        /// <param name="traitRow"> A drawing helper for drawing in a row. </param>
        /// <param name="selPawn"> Selected pawn. </param>
        protected virtual void DrawTraits(WidgetRow traitRow, Pawn selPawn)
        {
            ValidateArg.NotNull(traitRow, nameof(traitRow));
            ValidateArg.NotNull(selPawn, nameof(selPawn));

            List<Trait> traits = selPawn.story?.traits?.allTraits;

            if (traits == null)
                return;

            if (!_traitCache.TryGetValue(selPawn, out List<Tuple<Trait, string>> cache))
            {
                List<Tuple<Trait, string>> tuples = new List<Tuple<Trait, string>>();
                foreach (Trait trait in traits)
                {
                    tuples.Add(Tuple.Create(trait, trait.TipString(selPawn)));
                }

                _traitCache.Add(selPawn, tuples);
            }
            else
            {
                IEnumerable<Trait> newTraits = traits.Except(cache.Select(t => t.Item1));
                if (newTraits.Any())
                {
                    foreach (Trait trait in newTraits)
                    {
                        cache.Add(Tuple.Create(trait, trait.TipString(selPawn)));
                    }
                }
            }

            traitRow.Label(UIText.Traits.Translate() + ": ");
            for (int i = 0; i < traits.Count; i++)
            {
                Rect tipRegion = traitRow.Label(traits[i].LabelCap + (i != traits.Count ? ", " : string.Empty));

                TooltipHandler.TipRegion(
                    tipRegion,
                    _traitCache[selPawn].Find(t => t.Item1 == traits[i]).Item2);
                Widgets.DrawHighlightIfMouseover(tipRegion);
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

            GUI.DrawTexture(rect, Command.BGTex);
            DrawHitpointBackground(thing, rect);
            DrawQualityFrame(thing, rect);

            // Draw thing icon.
            Rect rect1 = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
            Widgets.ThingIcon(rect1, thing);

            Rect buttonRect = new Rect(rect.xMax - DrawUtility.TinyIconSize, rect.yMax - DrawUtility.TinyIconSize, DrawUtility.TinyIconSize, DrawUtility.TinyIconSize);
            if (Mouse.IsOver(rect))
            {
                GUI.color = Color.grey;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                Widgets.InfoCardButton(rect.x, rect.y, thing);

                // Draw Unload Now button
                if (this.ShowDropButton(selPawn, thing, false, out bool canDrop, out _) && canDrop && _gearTab.IsColonistPlayerControlled())
                {
                    TooltipHandler.TipRegion(buttonRect, UIText.UnloadNow.Translate());
                    if (UnloadNowUtility.ThingInQueue(selPawn, thing))
                    {
                        if (Widgets.ButtonImage(buttonRect, TexResource.DoubleDownArrow, AwesomeInventoryTex.HighlightBrown, AwesomeInventoryTex.HighlightGreen))
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            InterfaceUnloadNow(thing, selPawn);
                        }
                    }
                    else
                    {
                        if (Widgets.ButtonImage(buttonRect, TexResource.DoubleDownArrow, Color.white, AwesomeInventoryTex.HighlightGreen))
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            InterfaceUnloadNow(thing, selPawn);
                        }
                    }
                }

                GUI.color = Color.white;
            }
            else if (UnloadNowUtility.ThingInQueue(selPawn, thing))
            {
                if (Widgets.ButtonImage(buttonRect, TexResource.DoubleDownArrow, AwesomeInventoryTex.HighlightBrown, AwesomeInventoryTex.HighlightGreen))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    InterfaceUnloadNow(thing, selPawn);
                }

                GUI.color = Color.white;
            }

            // Draw tainted, locked or forced icon.
            bool isForced = false;
            if (thing is Apparel apparel)
            {
                isForced = selPawn.outfits?.forcedHandler?.IsForced(apparel) ?? false;
                if (apparel.WornByCorpse)
                {
                    Rect rect3 = new Rect(rect.xMax - DrawUtility.TinyIconSize, rect.y, DrawUtility.TinyIconSize, DrawUtility.TinyIconSize);
                    GUI.DrawTexture(rect3, TexResource.IconTainted);
                    TooltipHandler.TipRegion(rect3, "WasWornByCorpse".Translate());
                }

                if (isForced)
                {
                    Rect rect4 = new Rect(rect.x, rect.yMax - 20f, 20f, 20f);
                    GUI.DrawTexture(rect4, TexResource.IconForced);
                    TooltipHandler.TipRegion(rect4, UIText.ForcedApparel.Translate());
                }

                if (selPawn.apparel.IsLocked(apparel))
                {
                    GUI.DrawTexture(buttonRect, TexResource.IconLock);
                    TooltipHandler.TipRegion(buttonRect, UIText.DropThingLocked.TranslateSimple());
                }
            }

            Text.WordWrap = true;
            string tooltip;
            if (!_thingTooltipCache.TryGetValue(thing, out Tuple<string, string> tuple))
            {
                _thingTooltipCache[thing] = Tuple.Create(this.DrawHelper.TooltipTextFor(thing, true), this.DrawHelper.TooltipTextFor(thing, false));
                tuple = _thingTooltipCache[thing];
            }

            tooltip = isForced ? tuple.Item1 : tuple.Item2;
            TooltipHandler.TipRegion(rect, tooltip);

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
                FloatMenuOption equipOption = null;

                // Add put away option
                if (selPawn.equipment.AllEquipmentListForReading.Contains(equipment)
                    && selPawn.inventory != null
                    && this.ShowDropButton(selPawn, equipment, false, out bool canDrop, out _)
                    && canDrop)
                {
                    equipOption = new FloatMenuOption(
                        UIText.PutAway.Translate(labelShort),
                        () =>
                        {
                            selPawn.equipment.TryTransferEquipmentToContainer(selPawn.equipment.Primary, selPawn.inventory.innerContainer);
                        });

                    menuOptions.Add(
                        new FloatMenuOption(
                            UIText.DropThing.TranslateSimple()
                            , () => AwesomeInventoryTabBase.InterfaceDrop.Invoke(_gearTab, new object[] { equipment })));
                }
                else if (selPawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                {
                    equipOption = new FloatMenuOption(UIText.CannotEquip.Translate(labelShort) + " (" + UIText.IsIncapableOfViolenceLower.Translate(selPawn.LabelShort, selPawn) + ")", null);
                }
                else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    equipOption = new FloatMenuOption(UIText.CannotEquip.Translate(labelShort) + " (" + UIText.Incapable.Translate() + ")", null);
                }
                else if (selPawn.inventory.Contains(equipment) && EquipmentUtility.CanEquip(equipment, selPawn))
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
                                Thing thingToEquip;
                                if (equipment.stackCount > 1)
                                    thingToEquip = equipment.SplitOff(1);
                                else
                                    thingToEquip = equipment;

                                // unregister new weapon in the inventory list and register it in equipment list.
                                selPawn.equipment.GetDirectlyHeldThings().TryAddOrTransfer(thingToEquip);
                            }
                            else
                            {
                                Messages.Message("CannotEquip".Translate(labelShort), MessageTypeDefOf.NeutralEvent);
                            }
                        });
                }

                if (equipOption != null)
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
            if (pawn.inventory.Contains(apparel) && ApparelOptionUtility.CanWear(pawn, apparel))
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

            if (this.ShowDropButton(selPawn, apparel, false, out bool canDrop, out _) && canDrop)
            {
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

                    // Loadout option
                    if (selPawn.UseLoadout(out CompAwesomeInventoryLoadout comp))
                        floatOptionList.Add(ContextMenuUtility.OptionForThingOnPawn(selPawn, thing, comp));

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
        /// <param name="canvas"> Space available for drawing. </param>
        /// <param name="apparelChanged"> Indicates if apparels have changed since last call. </param>
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
                        && apparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin;
                },
                UIText.TorsoOnSkinLayer);

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
        /// <param name="thing"> Thing to unload. </param>
        /// <param name="pawn"> Pawn who carries <paramref name="thing"/>. </param>
        protected virtual void InterfaceUnloadNow(Thing thing, Pawn pawn)
        {
            ValidateArg.NotNull(thing, nameof(thing));
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (UnloadNowUtility.ThingInQueue(pawn, thing))
            {
                UnloadNowUtility.StopJob(pawn, thing);
            }
            else
            {
                UnloadNowUtility.QueueJob(pawn, thing);
            }
        }

        /// <summary>
        /// Set out rect size excluding bottom stat bars for the jealous tab.
        /// </summary>
        /// <param name="canvas"> Rect for the gear tab. </param>
        /// <returns> A rect for drawing the jealous tab. </returns>
        protected virtual Rect SetOutRectForJealousTab(Rect canvas)
        {
            // Substract the height of one line for a weight bar and a bulk bar.
            return canvas.ReplaceHeight(canvas.height - GenUI.ListSpacing);
        }

        /// <summary>
        /// Check if AI should show drop button for <paramref name="thing"/>.
        /// </summary>
        /// <param name="pawn"> Pawn who carries <paramref name="thing"/>. </param>
        /// <param name="thing"> Thing to check. </param>
        /// <param name="inventory"> If <paramref name="thing"/> is in the inventory section. </param>
        /// <param name="canDrop"> Whether the thing can be dropped. </param>
        /// <param name="tooltip"> Tooltip for the drop button. </param>
        /// <returns> Returns true if AI should show the drop button. </returns>
        protected virtual bool ShowDropButton(Pawn pawn, Thing thing, bool inventory, out bool canDrop, out string tooltip)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));
            ValidateArg.NotNull(thing, nameof(thing));

            bool showDropButton = false;
            canDrop = false;
            tooltip = string.Empty;
            if ((bool)AwesomeInventoryTabBase.CanControl.GetValue(_gearTab)
                && (inventory
                    || (bool)AwesomeInventoryTabBase.CanControlColonist.GetValue((ITab_Pawn_Gear)_gearTab)
                    || (pawn.Spawned && !pawn.Map.IsPlayerHome)))
            {
                showDropButton = true;

                // Check for bio-coded weapon and inventory
                bool flag2;
                if (pawn.IsQuestLodger())
                {
                    if (inventory)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        CompBiocodable compBiocodable = thing.TryGetComp<CompBiocodable>();
                        if (compBiocodable != null && compBiocodable.Biocoded)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
                            flag2 = compBladelinkWeapon != null && compBladelinkWeapon.CodedPawn == pawn;
                        }
                    }
                }
                else
                {
                    flag2 = false;
                }

                Apparel apparel;
                bool flag3 = (apparel = thing as Apparel) != null && pawn.apparel != null && pawn.apparel.IsLocked(apparel);

                canDrop = !(flag2 || flag3);

                if (flag3)
                {
                    tooltip = "DropThingLocked".TranslateSimple();
                }
                else if (flag2)
                {
                    tooltip = "DropThingLodger".TranslateSimple();
                }
                else
                {
                    tooltip = UIText.DropThing.TranslateSimple();
                }

                return showDropButton;
            }

            return showDropButton;
        }

        /// <summary>
        /// Draw items in pawn's inventory in a list style.
        /// </summary>
        /// <param name="things"> Things to draw. </param>
        /// <param name="selPawn"> Selected pawn. </param>
        /// <param name="rollingY"> Y position. </param>
        /// <param name="width"> Width of the drawing rect. </param>
        protected virtual void DrawInventory(ThingOwner<Thing> things, Pawn selPawn, ref float rollingY, float width)
        {
            ValidateArg.NotNull(things, nameof(things));

            ThingGroupModel thingGroup = things.MakeThingGroup();

            if (thingGroup.Weapons.Any())
                Widgets.ListSeparator(ref rollingY, width, UIText.StatWeaponName.TranslateSimple());
            foreach (Thing thing in thingGroup.Weapons)
                this.DrawThingRow(selPawn, ref rollingY, width, thing, true);

            if (thingGroup.Apparels.Any())
                Widgets.ListSeparator(ref rollingY, width, UIText.Apparel.TranslateSimple());
            foreach (Thing thing in thingGroup.Apparels)
                this.DrawThingRow(selPawn, ref rollingY, width, thing, true);

            if (thingGroup.Miscellaneous.Any())
                Widgets.ListSeparator(ref rollingY, width, UIText.Items.TranslateSimple());
            foreach (Thing thing in thingGroup.Miscellaneous)
                this.DrawThingRow(selPawn, ref rollingY, width, thing, true);
        }
    }
}
