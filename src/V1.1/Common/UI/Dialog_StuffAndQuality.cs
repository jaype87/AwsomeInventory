// <copyright file="Dialog_StuffAndQuality.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeInventory.Loadout;
using AwesomeInventory.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using TSPWQuality = RimWorld.ThingStuffPairWithQuality;

namespace AwesomeInventory.UI
{
    /* Variables state tables.
     * \\ x indicates that combination is possible.
     * \\ N\A means that combination is faulty.
     *
     * --------------*--------*-------------------------
     *              True    False | _useseparateButton
     *     True      x       N\A  |
     *     False     x        x   |
     * -------------------------------------------------
     * _isSeparated
     *
     * --------------*--------*-------------------------
     *              True    False | _isSeparated
     *     Null      N\A      x   |
     *   Not Null    x       N\A  |
     * -------------------------------------------------
     * _selectedSingleThingSelector
     */

    /// <summary>
    /// Dialog window for setting stuff, quality and hit points of things in loadout.
    /// </summary>
    public class Dialog_StuffAndQuality : Window
    {
        protected static readonly List<StatDef> _armorStats;
        protected static readonly List<StatDef> _baseWeaponStats;
        protected static readonly List<StatDef> _meleeWeaponStats;
        protected static readonly List<StatDef> _rangedWeaponStats;
        protected static readonly List<StatDef> _generalItemStats;

        private static readonly Regex _regex = new Regex(@"\d*", RegexOptions.Compiled);

        private static Vector2 _initialSize;
        private static Vector2 _initialSizeForWeapon;
        private static Vector2 _initialSizeForNoStuff;

        private static float _lineHeaderWidth = UIText.TenCharsString.Times(1.6f).GetWidthCached();
        private static float _statColumnWidth = UIText.TenCharsString.Times(1.6f).GetWidthCached();
        private static float _lineHeaderWidthForWeapon = UIText.TenCharsString.Times(2.5f).GetWidthCached();

        private bool _isSeparated;
        private bool _useSeparateButton;
        private ThingDef _stuffPreview;
        private SingleThingSelector _selectedSingleThingSelector;
        private QualityCategory _qualityPreview = QualityCategory.Normal;

        private float _availableStuffScrollViewHeight;
        private float _selectedStuffScrollViewHeight;
        private Vector2 _availableStuffscrollPosition = Vector2.zero;
        private Vector2 _selectedStuffscrollPosition = Vector2.zero;
        private float _statScrollWidth;
        private Vector2 _statScrollPosition = Vector2.zero;

        private ThingGroupSelector _groupSelector;
        private AwesomeInventoryLoadout _loadout;

        /// <summary>
        /// Initializes static members of the <see cref="Dialog_StuffAndQuality"/> class.
        /// </summary>
        static Dialog_StuffAndQuality()
        {
            // Set size.
            float width = GenUI.GetWidthCached(UIText.TenCharsString.Times(12));
            _initialSize = new Vector2(width, Verse.UI.screenHeight / 3f + GenUI.ListSpacing);
            _initialSizeForWeapon = new Vector2(_initialSize.x * 1.1f, _initialSize.y);
            _initialSizeForNoStuff = new Vector2(_initialSize.x * 0.5f, _initialSize.y);

            _armorStats = new List<StatDef>()
            {
                StatDefOf.MaxHitPoints,
                StatDefOf.ArmorRating_Sharp,
                StatDefOf.ArmorRating_Blunt,
                StatDefOf.ArmorRating_Heat,
                StatDefOf.Insulation_Cold,
                StatDefOf.Insulation_Heat,
                StatDefOf.Mass,
            };

            _baseWeaponStats = new List<StatDef>()
            {
                StatDefOf.MaxHitPoints,
                StatDefOf.MeleeWeapon_AverageDPS,
                StatDefOf.MeleeWeapon_AverageArmorPenetration,
                StatDefOf.Mass,
            };

            _meleeWeaponStats = new List<StatDef>()
            {
                StatDefOf.MeleeWeapon_CooldownMultiplier,
            };

            _rangedWeaponStats = new List<string>()
            {
                RangedWeaponStatsString.ArmorPenetration,
                RangedWeaponStatsString.Damage,
            }.Select(s => ToStatDef(s)).ToList();

            _generalItemStats = new List<StatDef>()
            {
                StatDefOf.MaxHitPoints,
                StatDefOf.MarketValue,
                StatDefOf.Mass,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_StuffAndQuality"/> class.
        /// </summary>
        /// <param name="groupSelector"> Draw dialog from this selector. </param>
        /// <param name="loadout"> Current selected loadout. </param>
        public Dialog_StuffAndQuality(ThingGroupSelector groupSelector, AwesomeInventoryLoadout loadout)
        {
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _groupSelector = groupSelector ?? throw new ArgumentNullException(nameof(groupSelector));

            IEnumerable<SingleThingSelector> singleThingSelectors = _groupSelector.OfType<SingleThingSelector>();
            _useSeparateButton = false;
            _isSeparated = false;
            _selectedSingleThingSelector = null;
            if (singleThingSelectors.Count() > 1)
            {
                _useSeparateButton = true;
                _isSeparated = singleThingSelectors.Select(t => t.AllowedQualityLevel).ToHashSet().Count > 1
                            || singleThingSelectors.Select(t => t.AllowedHitPointsPercent).ToHashSet().Count > 1;
                if (_isSeparated)
                    _selectedSingleThingSelector = singleThingSelectors.First();
            }

            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            doCloseX = true;
        }

        /// <summary>
        /// Gets size for the dialog window. In fact, it is more like setting size for the window.
        /// </summary>
        public override Vector2 InitialSize
        {
            get
            {
                if (!_groupSelector.AllowedThing.MadeFromStuff)
                {
                    return _initialSizeForNoStuff;
                }
                else if (_groupSelector.AllowedThing.IsWeapon)
                {
                    return _initialSizeForWeapon;
                }

                return _initialSize;
            }
        }

        /// <summary>
        /// Draw window content in <paramref name="canvas"/>.
        /// </summary>
        /// <param name="canvas"> Rect for drawing. </param>
        public override void DoWindowContents(Rect canvas)
        {
            /* <Layout>
             * Title
             * Stuff source     | Selected source  | Stats
             *                  |                  | Hitpoint slider
             *                  |                  | Quality slider
             * Preview quality  |     Separate     |
            */

            if (_groupSelector.AllowedThing.MadeFromStuff)
                this.DoWindowContentsGeneral(canvas);
            else
                this.DoWindowContentsForNoStuffItem(canvas);
        }

        public override void PreClose()
        {
            _availableStuffscrollPosition = Vector2.zero;
        }

        protected virtual void DoWindowContentsForNoStuffItem(Rect canvas)
        {
            float rollingY = 0;

            // Draw Title
            Rect titleRec = DrawUtility.DrawTitle(
                canvas.position,
                string.Concat(UIText.Customize.TranslateSimple(), " ", _groupSelector.AllowedThing.LabelCap),
                ref rollingY);

            GUI.color = Color.grey;
            Text.Anchor = TextAnchor.LowerLeft;
            Text.WordWrap = false;
            Widgets.Label(new Rect(titleRec.x, titleRec.yMax, canvas.width, GenUI.ListSpacing), string.Concat("(", UIText.MouseOverNumbersForDetails.TranslateSimple(), ")"));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = Color.white;

            // Draw stats
            Rect statRect = new Rect(canvas.x, rollingY + GenUI.ListSpacing, canvas.width, canvas.height - rollingY - GenUI.ListSpacing * 3);
            this.DrawStats(statRect, _groupSelector);

            // Draw sliders
            this.DrawSliders(new Rect(statRect.x, statRect.yMax, statRect.width / 2, GenUI.SmallIconSize).CenteredOnXIn(canvas));
        }

        protected virtual void DoWindowContentsGeneral(Rect canvas)
        {
            float rollingY = 0;

            // Draw Title
            Rect titleRec = DrawUtility.DrawTitle(
                canvas.position,
                string.Concat(UIText.Customize.TranslateSimple(), " ", _groupSelector.AllowedThing.LabelCap),
                ref rollingY);

            GUI.color = Color.grey;
            Text.Anchor = TextAnchor.LowerLeft;
            Text.WordWrap = false;
            Widgets.Label(new Rect(titleRec.xMax + GenUI.Gap, titleRec.y, canvas.width - titleRec.width, titleRec.height), string.Concat("(", UIText.MouseOverNumbersForDetails.TranslateSimple(), ")"));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = Color.white;

            // Draw scrollable list
            canvas.yMin = rollingY + GenUI.GapSmall;
            Rect listRect = new Rect(canvas.x, canvas.y, canvas.width * 0.35f + GenUI.GapSmall, canvas.height - GenUI.ListSpacing);
            this.DrawStuffScrollableList(listRect);

            // Draw quality choice for preview
            WidgetRow previewQualityRow = new WidgetRow(listRect.x, listRect.yMax, UIDirection.RightThenDown, listRect.width);
            this.DrawPreviewQuality(previewQualityRow);

            // Draw radio button for "Separate" option
            if (_useSeparateButton)
            {
                this.DrawSeparateRadioButton(
                new Rect(
                    previewQualityRow.FinalX + GenUI.GapSmall,
                    previewQualityRow.FinalY,
                    UIText.Separate.TranslateSimple().GetWidthCached() + GenUI.SmallIconSize + GenUI.GapTiny,
                    GenUI.ListSpacing));
            }

            // Draw stats
            Rect rect = canvas.RightPart(0.65f);
            rect.yMax -= GenUI.ListSpacing * 2 + DrawUtility.CurrentPadding;
            rect.x += GenUI.GapSmall;
            this.DrawStats(rect, _groupSelector);

            // Draw sliders
            this.DrawSliders(new Rect(0, rect.yMax + GenUI.GapTiny, rect.width * 0.6f, GenUI.SmallIconSize).CenteredOnXIn(rect));
        }

        protected virtual void DrawSliders(Rect rect)
        {
            // Draw hitpoint slider
            int dragID = Rand.Int;
            Rect hitpointsRect = rect;
            Rect qualityRect;
            if (_groupSelector.AllowedThing.useHitPoints)
            {
                this.DrawHitPointsSlider(hitpointsRect, dragID);
                qualityRect = hitpointsRect.ReplaceY(hitpointsRect.yMax + GenUI.GapTiny);
            }
            else
            {
                qualityRect = hitpointsRect;
            }

            // Draw quality slider
            if (_groupSelector.AllowedThing.HasComp(typeof(CompQuality)))
                this.DrawQualitySlider(qualityRect, ++dragID);
        }

        protected virtual void DrawQualitySlider(Rect qualityRect, int dragID)
        {
            QualityRange qualityRange = _isSeparated ? _selectedSingleThingSelector.AllowedQualityLevel : _groupSelector.SingleThingSelectors.First().AllowedQualityLevel;
            Widgets.QualityRange(qualityRect, dragID, ref qualityRange);
            if (_isSeparated)
                _selectedSingleThingSelector.SetQualityRange(qualityRange);
            else
                _groupSelector.SingleThingSelectors.ForEach(s => s.SetQualityRange(qualityRange));
        }

        protected virtual void DrawHitPointsSlider(Rect hitpointRect, int dragID)
        {
            FloatRange hitpointRange = _groupSelector.SingleThingSelectors.First().AllowedHitPointsPercent;
            Widgets.FloatRange(hitpointRect, dragID, ref hitpointRange, 0f, 1f, UIText.HitPoints, ToStringStyle.PercentZero);
            _groupSelector.SingleThingSelectors.ForEach(s => s.SetHitPoints(hitpointRange));
        }

        /// <summary>
        /// Draw separate radio button for managing loadout.
        /// </summary>
        /// <param name="rect"> Rect for drawing the radio button. </param>
        protected virtual void DrawSeparateRadioButton(Rect rect)
        {
            if (Widgets.RadioButtonLabeled(rect, UIText.Separate.TranslateSimple(), _isSeparated))
            {
                _isSeparated ^= true;
                if (_isSeparated)
                    _selectedSingleThingSelector = _groupSelector.OfType<SingleThingSelector>().First();
                else
                    _selectedSingleThingSelector = null;
            }

            TooltipHandler.TipRegion(rect, UIText.SeparateTooltip.TranslateSimple());
            Widgets.DrawHighlightIfMouseover(rect);
        }

        /// <summary>
        /// Draw choices for preview quality.
        /// </summary>
        /// <param name="widgetRow"> Helper for drawing. </param>
        protected virtual void DrawPreviewQuality(WidgetRow widgetRow)
        {
            ValidateArg.NotNull(widgetRow, nameof(widgetRow));

            GUI.color = Color.grey;
            Text.Anchor = TextAnchor.MiddleLeft;
            widgetRow.LabelWithHighlight(UIText.PreviewQuality.TranslateSimple(), UIText.PreviewQualityTooltip.TranslateSimple());
            GUI.color = Color.white;

            if (widgetRow.ButtonIcon(TexResource.TriangleLeft))
                _qualityPreview = _qualityPreview.Previous();

            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.Label(_qualityPreview.GetLabel(), GenUI.GetWidthCached(UIText.TenCharsString));

            if (widgetRow.ButtonIcon(TexResource.TriangleRight))
                _qualityPreview = _qualityPreview.Next();
        }

        /// <summary>
        /// Draw stuff source and selected stuff scrollable lists.
        /// </summary>
        /// <param name="canvas"> Rect for drawing two lists. </param>
        protected virtual void DrawStuffScrollableList(Rect canvas)
        {
            if (_groupSelector.AllowedThing.MadeFromStuff)
            {
                canvas.width -= GenUI.GapSmall;

                // Draw stuff source.
                List<ThingDef> stuffSource = GenStuff.AllowedStuffsFor(_groupSelector.AllowedThing).ToList();
                Rect candidateStuffScrollableList = canvas.LeftPart(0.5f);
                Widgets.NoneLabelCenteredVertically(candidateStuffScrollableList.ReplaceHeight(GenUI.ListSpacing), UIText.StuffSource.TranslateSimple());
                candidateStuffScrollableList.yMin += GenUI.ListSpacing;
                this.DrawStuffSourceScrollableList(candidateStuffScrollableList, stuffSource, ref _availableStuffscrollPosition, ref _availableStuffScrollViewHeight);

                // Draw selected stuff.
                Rect selectedStuffScrollableList = canvas.RightPart(0.5f).ReplaceX(candidateStuffScrollableList.xMax + GenUI.GapSmall);
                Widgets.NoneLabelCenteredVertically(selectedStuffScrollableList.ReplaceHeight(GenUI.ListSpacing), UIText.SelectedStuff.TranslateSimple());
                selectedStuffScrollableList.yMin += GenUI.ListSpacing;
                this.DrawSelectedStuffScrollableList(selectedStuffScrollableList, ref _selectedStuffscrollPosition, ref _selectedStuffScrollViewHeight);
            }
            else
            {
                this.DrawEmptyStuffList(canvas);
            }
        }

        /// <summary>
        /// Draw list of stuffs that can be used to make thing.
        /// </summary>
        /// <param name="outRect"> Rect for drawing. </param>
        protected virtual void DrawStuffSourceScrollableList(Rect outRect, IList<ThingDef> stuffList, ref Vector2 scrollPosition, ref float scrollViewHeight)
        {
            ValidateArg.NotNull(stuffList, nameof(stuffList));

            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width, scrollViewHeight);
            Text.Font = GameFont.Small;
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Rect row = new Rect(viewRect.x, viewRect.y, viewRect.width, GenUI.ListSpacing);
            if (!stuffList.Any())
            {
                Rect rect = outRect.TopPart(0.6f);
                Text.Font = GameFont.Medium;
                Widgets.NoneLabelCenteredVertically(rect, UIText.NoMaterial.TranslateSimple());
                Text.Font = GameFont.Small;
            }

            stuffList.OrderBy(t => t.defName);
            for (int i = 0; i < stuffList.Count; ++i)
            {
                Texture2D texture2D = i % 2 == 0 ? TexUI.TextBGBlack : TexUI.GrayTextBG;
                GUI.DrawTexture(row, texture2D);

                ThingDef stuff = stuffList[i];
                DrawUtility.DrawLabelButton(
                    row
                    , stuff.LabelAsStuff.CapitalizeFirst()
                    , () =>
                        {
                            // Remove generic choice from the thing group selector once a specific stuff source is chosen.
                            if (_groupSelector.OfType<SingleThingSelector>().First().AllowedStuff == null)
                                _groupSelector.Clear();

                            SingleThingSelector singleThingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(_groupSelector.AllowedThing, stuff);
                            singleThingSelector.SetQualityRange(new QualityRange(_qualityPreview, QualityCategory.Legendary));
                            _groupSelector.Add(singleThingSelector);

                            if (_useSeparateButton == false && _groupSelector.Count > 1)
                            {
                                _useSeparateButton = true;
                                _isSeparated = false;
                            }

                            if (_isSeparated == true)
                                _selectedSingleThingSelector = singleThingSelector;
                            else
                                _selectedSingleThingSelector = null;
                        });

                // Set stuff for preview.
                if (Mouse.IsOver(row))
                {
                    _stuffPreview = stuff;
                    Widgets.DrawHighlight(row);
                }

                row.y = row.yMax;
            }

            scrollViewHeight = row.yMax;
            Widgets.EndScrollView();
        }

        /// <summary>
        /// Draw a list of stuff that is selected by a player for their loadout.
        /// </summary>
        /// <param name="outRect"> A rect for drawing. </param>
        /// <param name="scrollPosition"> Position of the scroll bar. </param>
        /// <param name="scrollViewHeight"> The height of this list. </param>
        protected virtual void DrawSelectedStuffScrollableList(Rect outRect, ref Vector2 scrollPosition, ref float scrollViewHeight)
        {
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width, scrollViewHeight);
            Text.Font = GameFont.Small;

            List<SingleThingSelector> singleThingSelectors = _groupSelector.OfType<SingleThingSelector>().ToList();

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Rect row = new Rect(viewRect.x, viewRect.y, viewRect.width, GenUI.ListSpacing);

            // Generic choice has AllowedStuff = null. And it should not coexist with more specific stuff choice
            // in the ThingGroupSelector.
            if (singleThingSelectors.Any(s => s.AllowedStuff == null))
            {
                if (singleThingSelectors.Count > 1)
                {
                    Log.Error(ErrorText.NonExclusiveGenericStuffSource);
                }
                else
                {
                    Rect rect = outRect.TopPart(0.6f);
                    Text.Font = GameFont.Medium;
                    Widgets.NoneLabelCenteredVertically(rect, UIText.NoMaterial.TranslateSimple());
                    Text.Font = GameFont.Small;
                }
            }
            else
            {
                for (int i = 0; i < singleThingSelectors.Count; ++i)
                {
                    Texture2D texture2D = i % 2 == 0 ? TexUI.TextBGBlack : TexUI.GrayTextBG;
                    texture2D = singleThingSelectors[i] == _selectedSingleThingSelector ? TexUI.HighlightSelectedTex : texture2D;
                    GUI.DrawTexture(row, texture2D);
                    if (Mouse.IsOver(row))
                        Widgets.DrawHighlightSelected(row);

                    Rect closeRect = new Rect(row.x, row.y, GenUI.SmallIconSize, GenUI.SmallIconSize);

                    if (Widgets.ButtonImage(closeRect.ContractedBy(GenUI.GapTiny), TexResource.CloseXSmall))
                    {
                        _groupSelector.Remove(singleThingSelectors[i]);
                        if (!_groupSelector.Any())
                        {
                            // If there is no SinglethingSelctor left in the group selector, add a generic SingleThingSelector.
                            SingleThingSelector singleThingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(_groupSelector.AllowedThing, null);
                            _groupSelector.Add(singleThingSelector);
                        }

                        if (singleThingSelectors[i] == _selectedSingleThingSelector)
                        {
                            _selectedSingleThingSelector = (SingleThingSelector)_groupSelector.First();
                        }

                        if (_useSeparateButton == true && _groupSelector.Count < 2)
                        {
                            _useSeparateButton = false;
                            _isSeparated = false;
                            _selectedSingleThingSelector = null;
                        }
                    }

                    ThingDef stuff = singleThingSelectors[i].AllowedStuff;
                    DrawUtility.DrawLabelButton(
                        row.ReplaceX(row.x + GenUI.SmallIconSize)
                        , stuff.LabelAsStuff.CapitalizeFirst()
                        , () =>
                        {
                            if (_isSeparated)
                                _selectedSingleThingSelector = singleThingSelectors[i];
                        });

                    row.y = row.yMax;
                }
            }

            scrollViewHeight = row.yMax;
            Widgets.EndScrollView();
        }

        private static void DrawStatTableHeader(WidgetRow row, float columnWidth, List<TSPWQuality> pairListToDraw)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            foreach (TSPWQuality pair in pairListToDraw)
            {
                if (row.ButtonIcon(TexResource.Info))
                {
                    Thing thing = pair.MakeThingWithoutID();
                    Find.WindowStack.Add(new Dialog_InfoCard(thing));
                }

                row.Label(pair.stuff != null ? pair.stuff.LabelAsStuff.CapitalizeFirst().ColorizeByQuality(pair.Quality) : pair.thing.LabelCap.ToString(), columnWidth - WidgetRow.IconSize - 2 * WidgetRow.DefaultGap);
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
        }

        private static void DrawStatNameColumn(Rect cell, List<StatDef> stats, out float rollingY)
        {
            foreach (StatDef statDef in stats)
            {
                Widgets.NoneLabelCenteredVertically(cell, statDef.LabelCap);
                Widgets.DrawHighlightIfMouseover(cell);
                TooltipHandler.TipRegion(cell, statDef.description);
                cell.y += GenUI.ListSpacing;
            }

            rollingY = cell.y;
        }

        private void DrawStats(Rect rect, ThingGroupSelector groupSelector)
        {
            List<TSPWQuality> pairListToDraw = new List<TSPWQuality>();

            if (groupSelector.AllowedThing.MadeFromStuff)
            {
                foreach (SingleThingSelector singleThingSelector in groupSelector.OfType<SingleThingSelector>())
                {
                    if (singleThingSelector.AllowedStuff != null)
                    {
                        pairListToDraw.Add(
                            new TSPWQuality(
                                groupSelector.AllowedThing, singleThingSelector.AllowedStuff, singleThingSelector.AllowedQualityLevel.min));
                    }
                }

                if (_stuffPreview != null)
                {
                    pairListToDraw.Add(new TSPWQuality(groupSelector.AllowedThing, _stuffPreview, _qualityPreview));
                }
            }
            else
            {
                pairListToDraw.Add(new TSPWQuality(groupSelector.AllowedThing, null, QualityCategory.Normal));
            }

            _statScrollWidth = pairListToDraw.Count * _statColumnWidth;
            WidgetRow row = new WidgetRow(
                rect.x + (_groupSelector.AllowedThing.IsWeapon ? _lineHeaderWidthForWeapon : _lineHeaderWidth),
                rect.y,
                UIDirection.RightThenDown,
                _statScrollWidth);

            Rect outRect = new Rect(row.FinalX, row.FinalY, _statColumnWidth * 3, rect.height);
            Rect viewRect = new Rect(row.FinalX, row.FinalY, _statScrollWidth, rect.height);

            // Draw stats
            if (groupSelector.AllowedThing.IsApparel)
            {
                DrawStatNameColumn(new Rect(rect.x, row.FinalY + GenUI.ListSpacing, _lineHeaderWidth, GenUI.ListSpacing), _armorStats, out _);
                Widgets.ScrollHorizontal(outRect, ref _statScrollPosition, viewRect);
                Widgets.BeginScrollView(outRect, ref _statScrollPosition, viewRect);

                DrawStatTableHeader(row, _statColumnWidth, pairListToDraw);
                this.DrawStatRows(_armorStats, pairListToDraw, row, _statColumnWidth);

                Widgets.EndScrollView();
            }
            else if (groupSelector.AllowedThing.IsMeleeWeapon)
            {
                DrawStatNameColumn(new Rect(rect.x, row.FinalY + GenUI.ListSpacing, _lineHeaderWidthForWeapon, GenUI.ListSpacing), _baseWeaponStats, out float nameColumnY);
                DrawStatNameColumn(new Rect(rect.x, nameColumnY, _lineHeaderWidthForWeapon, GenUI.ListSpacing), _meleeWeaponStats, out _);
                Widgets.ScrollHorizontal(outRect, ref _statScrollPosition, viewRect);
                Widgets.BeginScrollView(outRect, ref _statScrollPosition, viewRect);

                DrawStatTableHeader(row, _statColumnWidth, pairListToDraw);
                this.DrawStatRows(_baseWeaponStats, pairListToDraw, row, _statColumnWidth);
                this.DrawStatRows(_meleeWeaponStats, pairListToDraw, row, _statColumnWidth);

                Widgets.EndScrollView();
            }
            else if (groupSelector.AllowedThing.IsRangedWeapon)
            {
                DrawStatNameColumn(new Rect(rect.x, row.FinalY + GenUI.ListSpacing, _lineHeaderWidthForWeapon, GenUI.ListSpacing), _baseWeaponStats, out float nameColumnY);
                DrawStatNameColumn(new Rect(rect.x, nameColumnY, _lineHeaderWidthForWeapon, GenUI.ListSpacing), _rangedWeaponStats, out _);
                Widgets.ScrollHorizontal(outRect, ref _statScrollPosition, viewRect);
                Widgets.BeginScrollView(outRect, ref _statScrollPosition, viewRect);

                DrawStatTableHeader(row, _statColumnWidth, pairListToDraw);
                this.DrawStatRows(_baseWeaponStats, pairListToDraw, row, _statColumnWidth);
                this.DrawRangedStatRows(pairListToDraw, row, _statColumnWidth);

                Widgets.EndScrollView();
            }
            else
            {
                DrawStatNameColumn(new Rect(rect.x, row.FinalY + GenUI.ListSpacing, _lineHeaderWidth, GenUI.ListSpacing), _generalItemStats, out _);
                Widgets.ScrollHorizontal(outRect, ref _statScrollPosition, viewRect);
                Widgets.BeginScrollView(outRect, ref _statScrollPosition, viewRect);

                DrawStatTableHeader(row, _statColumnWidth, pairListToDraw);
                this.DrawStatRows(_generalItemStats, pairListToDraw, row, _statColumnWidth);

                Widgets.EndScrollView();
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawEmptyStuffList(Rect rect)
        {
            Widgets.NoneLabelCenteredVertically(rect, UIText.NoMaterial.TranslateSimple());
        }

        private void DrawRangedStatRows(List<TSPWQuality> pairList, WidgetRow row, float numColumnWidth)
        {
            List<List<StatDrawInfo>> listHolder = new List<List<StatDrawInfo>>();
            List<List<StatDrawInfo>> transposedLists = new List<List<StatDrawInfo>>();

            // listHolder[0]  listHolder[1]
            // | stuff1 |     | stuff2 |
            // | Damage |     | Damage |
            // | AP     |     | AP     |

            /*                       stuff1   stuff2
             * transposedLists[0]    Damage   Damage
             * transposedLists[1]    AP       AP
            */

            foreach (TSPWQuality pair in pairList)
            {
                List<StatDrawInfo> infoList = new List<StatDrawInfo>();

                infoList.AddRange(pair.thing.SpecialDisplayStats(StatRequest.For(pair.thing, pair.stuff, pair.Quality))
                                .Where(r => _rangedWeaponStats.Any(s => s.label.TranslateSimple().CapitalizeFirst() == r.LabelCap))
                                .Select(r => (StatDrawInfo)r)
                                .ToList());
                listHolder.Add(infoList);
            }

            // Transpose lists
            for (int i = 0; i < _rangedWeaponStats.Count; i++)
            {
                List<StatDrawInfo> newList = new List<StatDrawInfo>();
                foreach (List<StatDrawInfo> list in listHolder)
                {
                    newList.Add(
                        list.Find(s => s.LabelCap == _rangedWeaponStats[i].label.TranslateSimple().CapitalizeFirst())
                        ?? new StatDrawInfo() { ValueString = "-", Tip = string.Empty, });
                }

                if (newList.Count > 1)
                {
                    List<StatDrawInfo> orderedList = newList.OrderByDescending(t => t.Value).ToList();
                    foreach (StatDrawInfo statDrawInfo in orderedList)
                    {
                        if (statDrawInfo.Value == orderedList[0].Value)
                            statDrawInfo.Color = Color.green;
                    }
                }

                transposedLists.Add(newList);
            }

            DrawStatRows(_rangedWeaponStats, null, row, numColumnWidth, transposedLists, true);
        }

        private void DrawStatRows(List<StatDef> stats, List<TSPWQuality> pairs, WidgetRow row, float numColumnWidth,
                                  List<List<StatDrawInfo>> listHolder = null, bool specialStats = false)
        {
            for (int i = 0; i < stats.Count; i++)
            {
                List<StatDrawInfo> statInfoList = new List<StatDrawInfo>();
                if (!specialStats)
                {
                    statInfoList = new List<StatDrawInfo>();

                    // Retrieve stat value and create a view model, StatDrawInfo, for drawing.
                    foreach (TSPWQuality pair in pairs)
                    {
                        StatDrawInfo drawInfo = new StatDrawInfo();
                        Thing tempThing = pair.MakeThingWithoutID();
                        StatRequest statRequest = StatRequest.For(tempThing);
                        if ((stats[i].Worker.ShouldShowFor(statRequest) && !stats[i].Worker.IsDisabledFor(tempThing)) || stats[i] == StatDefOf.MaxHitPoints)
                        {
                            drawInfo.StatRequest = StatRequest.For(pair.MakeThingWithoutID());
                            drawInfo.Value = stats[i].Worker.GetValue(drawInfo.StatRequest);
                            drawInfo.Tip = stats[i].Worker.GetExplanationFull(drawInfo.StatRequest, stats[i].toStringNumberSense, drawInfo.Value);
                        }
                        else
                        {
                            drawInfo.Value = -1;
                            drawInfo.Tip = string.Empty;
                        }

                        statInfoList.Add(drawInfo);
                    }

                    if (statInfoList.Count > 1)
                    {
                        // Highlight highest stat value.
                        List<StatDrawInfo> orderedList = statInfoList.OrderByDescending(t => t.Value).ToList();
                        foreach (StatDrawInfo statDrawInfo in orderedList)
                        {
                            if (statDrawInfo.Value == orderedList[0].Value)
                                statDrawInfo.Color = Color.green;
                        }
                    }
                }
                else
                {
                    statInfoList = listHolder[i];
                }

                // Draw stat for each stuff choice.
                Text.Anchor = TextAnchor.MiddleCenter;
                foreach (StatDrawInfo info in statInfoList)
                {
                    GUI.color = info.Color;
                    info.DrawRect = row.Label(
                        specialStats
                        ? info.ValueString
                        : info.Value != -1
                            ? stats[i].Worker.ValueToString(info.Value, true, stats[i].toStringNumberSense)
                            : "-"
                        , numColumnWidth - WidgetRow.DefaultGap);
                    Widgets.DrawHighlightIfMouseover(info.DrawRect);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    TooltipHandler.TipRegion(info.DrawRect, info.Tip);
                    Text.Anchor = TextAnchor.MiddleCenter;
                }

                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private class StatDrawInfo
        {
            public Color Color = Color.white;
            public float Value = -1;
            public string ValueString;
            public StatRequest StatRequest = default;
            public string Tip = string.Empty;
            public Rect DrawRect;
            public string LabelCap;

            /// <summary>
            /// Adapter for transforming type StatDrawEntry to StatDrawInfo.
            /// </summary>
            /// <param name="entry"> The <see cref="StatDrawEntry"/> for conversion. </param>
            public static explicit operator StatDrawInfo(StatDrawEntry entry)
            {
                ValidateArg.NotNull(entry, nameof(entry));

                StatDrawInfo result = new StatDrawInfo
                {
                    ValueString = entry.ValueString,
                    LabelCap = entry.LabelCap,
                };
                _ = float.TryParse(_regex.Match(entry.ValueString).Value, out result.Value);
                result.Tip = entry.GetExplanationText(StatRequest.ForEmpty());
                return result;
            }
        }

        private static StatDef ToStatDef(string str)
        {
            return new StatDef() { label = str };
        }
    }
}
