// <copyright file="Dialog_ManageLoadouts.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AwesomeInventory.Loadout;
using AwesomeInventory.Resources;
using AwesomeInventory.Utilities;
using RimWorld;
using RPG_Inventory_Remake_Common;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A dialog window for managing loadouts.
    /// </summary>
    public class Dialog_ManageLoadouts : Window
    {
        private const float _paneDivider = 5 / 9f;
        protected static readonly float _overburdenThresold;
        private static readonly HashSet<ThingDef> _allSuitableDefs = GameComponent_DefManager.GetSuitableDefs();

        /// <summary>
        /// Controls the window size and position.
        /// </summary>
        private static Vector2 _initialSize;
        private static Pawn _pawn;
        private static List<SelectableItem> _selectableItems;

        private Vector2 _availableScrollPosition = Vector2.zero;
        private readonly int _loadoutNameMaxLength = 50;
        private const float _barHeight = 24f;
        private Vector2 _countFieldSize = new Vector2(40f, 24f);
        private AILoadout _currentLoadout;
        private string _filter = string.Empty;
        private const float _iconSize = 16f;
        private const float _margin = 6f;
        private const float _topAreaHeight = 30f;
        private float _scrollViewHeight;
        private Vector2 _slotScrollPosition = Vector2.zero;
        private List<SelectableItem> _source;
        private SourceSelection _sourceType = SourceSelection.Ranged;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_ManageLoadouts"/> class.
        /// </summary>
        /// <param name="loadout"> Selected loadout. </param>
        /// <param name="pawn"> Selected pawn. </param>
        public Dialog_ManageLoadouts(AILoadout loadout, Pawn pawn)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));
            ValidateArg.NotNull(pawn, nameof(pawn));

            float width = GenUI.GetWidthCached(UIText.TenCharsString.Times(10));
            _initialSize = new Vector2(width, Verse.UI.screenHeight / 2f);

            _pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
            _currentLoadout = loadout;

            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = false;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        #endregion Constructors

        /// <summary>
        /// Source categories for loadout dialog.
        /// </summary>
        public enum SourceSelection
        {
            /// <summary>
            /// Ranged weapons.
            /// </summary>
            Ranged,

            /// <summary>
            /// Melee weapons.
            /// </summary>
            Melee,

            /// <summary>
            /// Apparels.
            /// </summary>
            Apparel,

            /// <summary>
            /// Things that are minified.
            /// </summary>
            Minified,

            /// <summary>
            /// Generic thing def.
            /// </summary>
            Generic,

            /// <summary>
            /// All things, won't include generics, can include minified/able now.
            /// </summary>
            All,
        }

        /// <summary>
        /// Gets initial window size for this dialog.
        /// </summary>
        public override Vector2 InitialSize => _initialSize;

        #region Methods

        /// <summary>
        /// Draw contents in window.
        /// </summary>
        /// <param name="canvas"> Canvas to draw. </param>
        public override void DoWindowContents(Rect canvas)
        {
            /*
             * ||        Buttons          ||
             * || Loadout Name Text Field ||    Category Button Image   ||
             * ||     Items in Loadout    || Avaialable Items to Choose ||
             * ||        Weight Bar       ||
             *
             */

            GUI.BeginGroup(canvas);
            Text.Font = GameFont.Small;
            Vector2 useableSize = new Vector2(
                canvas.width - DrawUtility.CurrentPadding * 2,
                canvas.height - DrawUtility.CurrentPadding * 2);

            // Top buttons
            WidgetRow row = new WidgetRow(0, 0, UIDirection.RightThenDown, useableSize.y);

            List<AILoadout> loadouts = LoadoutManager.Loadouts;

            if (row.ButtonText(UIText.SelectLoadout.TranslateSimple()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                if (loadouts.Count == 0)
                {
                    options.Add(new FloatMenuOption(UIText.NoLoadout.Translate(), null));
                }
                else
                {
                    for (int j = 0; j < loadouts.Count; j++)
                    {
                        int i = j;
                        options.Add(new FloatMenuOption(
                            loadouts[i].label,
                            () =>
                            {
                                _currentLoadout = loadouts[i];
                                _slotScrollPosition = Vector2.zero;
                                _pawn.SetLoadout(_currentLoadout);
                            }));
                    }
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (row.ButtonText(UIText.NewLoadout.TranslateSimple()))
            {
                AILoadout loadout = new AILoadout()
                {
                    label = LoadoutManager.GetIncrementalLabel(_currentLoadout.label),
                };
                LoadoutManager.AddLoadout(loadout);
                _currentLoadout = loadout;
                _pawn.SetLoadout(loadout);
            }

            if (row.ButtonText(UIText.CopyLoadout.TranslateSimple()))
            {
                _currentLoadout = new AILoadout(_currentLoadout);
                LoadoutManager.AddLoadout(_currentLoadout);
                _pawn.SetLoadout(_currentLoadout);
            }

            if (row.ButtonText(UIText.DeleteLoadout.TranslateSimple()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int j = 0; j < loadouts.Count; j++)
                {
                    int i = j;
                    options.Add(new FloatMenuOption(loadouts[i].label,
                        delegate
                        {
                            if (loadouts.Count > 1)
                            {
                                LoadoutManager.TryRemoveLoadout(loadouts[i], false);
                            }
                            else
                            {
                                Rect msgRect = new Rect(Vector2.zero, Text.CalcSize(ErrorMessage.TryToDeleteLastLoadout.Translate()))
                                                .ExpandedBy(50);
                                Find.WindowStack.Add(
                                    new Dialog_InstantMessage
                                        (ErrorMessage.TryToDeleteLastLoadout.Translate(), msgRect.size, UIText.OK.Translate())
                                    {
                                        windowRect = msgRect
                                    });
                            }
                        }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            Rect nameFieldRect = new Rect(
                0,
                row.FinalY + GenUI.ListSpacing,
                useableSize.x * _paneDivider,
                GenUI.SmallIconSize);

            Rect loadoutItemsRect = new Rect(
                0,
                nameFieldRect.yMax + GenUI.GapTiny,
                useableSize.x * _paneDivider,
                canvas.height - nameFieldRect.yMax - GenUI.GapTiny - GenUI.ListSpacing * 2 - DrawUtility.CurrentPadding);

            Rect sourceButtonRect = new Rect(
                nameFieldRect.xMax + Listing.ColumnSpacing, 
                nameFieldRect.y,
                useableSize.x - nameFieldRect.xMax - Listing.ColumnSpacing,
                GenUI.SmallIconSize);

            Rect sourceItemsRect = new Rect(
                sourceButtonRect.x,
                sourceButtonRect.yMax + GenUI.GapTiny,
                sourceButtonRect.width,
                useableSize.y - sourceButtonRect.yMax - GenUI.GapTiny);

            Rect weightBarRect = new Rect(
                0,
                loadoutItemsRect.yMax + GenUI.GapTiny,
                nameFieldRect.width,
                GenUI.SmallIconSize);

            this.DrawLoadoutNameField(nameFieldRect);
            this.DrawItemsInLoadout(loadoutItemsRect);
            this.DrawCategoryIcon(sourceButtonRect);
            this.DrawItemsInSourceCategory(sourceItemsRect);
            this.DrawWeightBar(weightBarRect);

            GUI.EndGroup();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            HashSet<ThingDef> visibleDefs = GameComponent_DefManager.GetSuitableDefs();
            visibleDefs.IntersectWith(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEverOrMinifiable).Select(t => t.def));
            _selectableItems = new List<SelectableItem>();
            foreach (ThingDef td in _allSuitableDefs)
            {
                SelectableItem selectableItem = new SelectableItem() { thingDef = td };
                if (td is AIGenericDef genericDef)
                {
                    selectableItem.isGreyedOut = visibleDefs.Any(def => genericDef.Includes(def));
                }
                else
                {
                    selectableItem.isGreyedOut = !visibleDefs.Contains(td);
                }

                _selectableItems.Add(selectableItem);
            }

            _selectableItems = _selectableItems.OrderBy(td => td.thingDef.label).ToList();
            SetSource(SourceSelection.Ranged);
        }

        public void DrawCategoryIcon(Rect canvas)
        {
            WidgetRow row = new WidgetRow(canvas.x, canvas.y);
            DrawSourceIcon(SourceSelection.Ranged, TexResource.IconRanged, ref row, UIText.SourceRangedTip.Translate());
            DrawSourceIcon(SourceSelection.Melee, TexResource.IconMelee, ref row, UIText.SourceMeleeTip.Translate());
            DrawSourceIcon(SourceSelection.Apparel, TexResource.Apparel, ref row, UIText.SourceApparelTip.Translate());
            DrawSourceIcon(SourceSelection.Minified, TexResource.IconMinified, ref row, UIText.SourceMinifiedTip.Translate());
            DrawSourceIcon(SourceSelection.Generic, TexResource.IconGeneric, ref row, UIText.SourceGenericTip.Translate());
            DrawSourceIcon(SourceSelection.All, TexResource.IconAll, ref row, UIText.SourceAllTip.Translate());

            float nameFieldLen = GenUI.GetWidthCached(UIText.TenCharsString);
            float incrementX = canvas.xMax - row.FinalX - nameFieldLen - WidgetRow.IconSize - WidgetRow.ButtonExtraSpace;
            row.Gap(incrementX);
            row.Icon(TexResource.IconSearch, UIText.SourceFilterTip.Translate());

            Rect filterRect = new Rect(row.FinalX, canvas.y, nameFieldLen, canvas.height);
            DrawFilterField(filterRect);
            TooltipHandler.TipRegion(filterRect, UIText.SourceFilterTip.Translate());
        }

        public void FilterSource(string filter)
        {
            // reset source
            SetSource(_sourceType, true);

            // filter
            _source = _source.Where(td => td.thingDef.label.ToUpperInvariant().Contains(_filter.ToUpperInvariant())).ToList();
        }

        public void SetSource(SourceSelection source, bool preserveFilter = false)
        {
            if (!preserveFilter)
                _filter = "";

            switch (source)
            {
                case SourceSelection.Ranged:
                    _source = _selectableItems.Where(row => row.thingDef.IsRangedWeapon).ToList();
                    _sourceType = SourceSelection.Ranged;
                    break;

                case SourceSelection.Melee:
                    _source = _selectableItems.Where(row => row.thingDef.IsMeleeWeapon).ToList();
                    _sourceType = SourceSelection.Melee;
                    break;

                case SourceSelection.Apparel:
                    _source = _selectableItems.Where(row => row.thingDef.IsApparel).ToList();
                    _sourceType = SourceSelection.Apparel;
                    break;

                case SourceSelection.Minified:
                    _source = _selectableItems.Where(row => row.thingDef.Minifiable).ToList();
                    _sourceType = SourceSelection.Minified;
                    break;

                case SourceSelection.Generic:
                    _sourceType = SourceSelection.Generic;
                    _source = _selectableItems.Where(row => row.thingDef is AIGenericDef).ToList();
                    break;

                case SourceSelection.All:
                default:
                    _source = _selectableItems;
                    _sourceType = SourceSelection.All;
                    break;
            }
        }

        private void DrawCountField(Rect canvas, Thing thing)
        {
            if (thing == null)
                return;

            int countInt = thing.stackCount;
            string buffer = countInt.ToString();
            Widgets.TextFieldNumeric(canvas, ref countInt, ref buffer);
            TooltipHandler.TipRegion(canvas, UIText.CountFieldTip.Translate(thing.stackCount));
            if (countInt != thing.stackCount)
            {
                int delta = countInt - thing.stackCount;
                _pawn.GetComp<CompAwesomeInventoryLoadout>().InventoryTracker[thing] -= delta;
                _currentLoadout.SetDirtyAll();
            }
            thing.stackCount = countInt;
        }

        private void DrawFilterField(Rect canvas)
        {
            string filter = GUI.TextField(canvas, _filter);
            if (filter != _filter)
            {
                _filter = filter;
                FilterSource(_filter);
            }
        }

        private void DrawLoadoutNameField(Rect canvas)
        {
            _currentLoadout.label = Widgets.TextField(canvas, _currentLoadout.label, _loadoutNameMaxLength, Outfit.ValidNameRegex);
        }

        protected virtual void DrawItemRow(Rect row, Thing thing, int reorderableGroup, bool drawShadow = false)
        {
            /* label (fill) | gear icon | count | delete (iconSize) */

            WidgetRow widgetRow = new WidgetRow(row.width, row.y, UIDirection.LeftThenDown, row.width);

            if (widgetRow.ButtonIcon(TexResource.CloseXSmall, UIText.Delete.TranslateSimple()))
            {
                _currentLoadout.Remove(thing);
            }

            DrawCountField(new Rect(widgetRow.FinalX - WidgetRow.IconSize * 2 - WidgetRow.DefaultGap, widgetRow.FinalY, WidgetRow.IconSize * 2, GenUI.ListSpacing), thing);
            widgetRow.GapButtonIcon();
            widgetRow.GapButtonIcon();

            if ((thing.def.MadeFromStuff || thing.TryGetQuality(out _))
                && !thing.def.IsArt
                && widgetRow.ButtonIcon(TexResource.Gear))
            {
                Find.WindowStack.Add(new Dialog_StuffAndQuality(thing, _currentLoadout));
            }

            Text.WordWrap = false;
            Rect labelRect = widgetRow.Label(drawShadow ? thing.LabelCapNoCount.Colorize(Theme.MilkySlicky.ForeGround) : thing.LabelCapNoCount, widgetRow.FinalX);
            Text.WordWrap = true;
            if (!drawShadow)
            {
                ReorderableWidget.Reorderable(reorderableGroup, labelRect);
            }

            if (!drawShadow)
            {
                // Tooltips && Highlights
                Widgets.DrawHighlightIfMouseover(row);
                if (Event.current.type == EventType.MouseDown)
                {
                    TooltipHandler.ClearTooltipsFrom(labelRect);
                }
                else
                {
                    TooltipHandler.TipRegion(labelRect, string.Concat(thing.GetWeightTip(), '\n', UIText.DragToReorder.Translate()));
                }
            }

            //// Set up rects
            //Rect labelRect = new Rect(row);
            //labelRect.xMax -= row.height + _countFieldSize.x + _iconSize + GenUI.GapSmall;
            //if (!drawShadow)
            //{
            //    ReorderableWidget.Reorderable(reorderableGroup, labelRect);
            //}

            //Rect gearIconRect = new Rect(labelRect.xMax, row.y, row.height, row.height);

            //Rect countRect = new Rect(
            //    gearIconRect.xMax,
            //    row.yMin + (row.height - _countFieldSize.y) / 2f,
            //    _countFieldSize.x,
            //    _countFieldSize.y);

            //Rect deleteRect = new Rect(countRect.xMax + GenUI.GapSmall, row.yMin + (row.height - _iconSize) / 2f, _iconSize, _iconSize);

            //// label
            //Text.Anchor = TextAnchor.MiddleLeft;
            //Text.WordWrap = false;
            //Widgets.Label(labelRect, thing.LabelCapNoCount);
            //Text.WordWrap = true;
            //Text.Anchor = TextAnchor.UpperLeft;

            //// gear icon
            //if ((thing.def.MadeFromStuff || thing.TryGetQuality(out _))
            //    && !thing.def.IsArt
            //    && Widgets.ButtonImage(gearIconRect, TexResource.Gear))
            //{
            //    Find.WindowStack.Add(new Dialog_StuffAndQuality(thing, _currentLoadout));
            //}

            //// count
            //DrawCountField(countRect, thing);

            //// delete
            //if (Mouse.IsOver(deleteRect))
            //    GUI.DrawTexture(row, TexUI.HighlightTex);
            //if (Widgets.ButtonImage(deleteRect, TexResource.IconClear))
            //{
            //    _currentLoadout.Remove(thing);
            //}

            //if (!drawShadow)
            //{
            //    // Tooltips && Highlights
            //    Widgets.DrawHighlightIfMouseover(row);
            //    if (Event.current.type == EventType.MouseDown)
            //    {
            //        TooltipHandler.ClearTooltipsFrom(labelRect);
            //    }
            //    else
            //    {
            //        TooltipHandler.TipRegion(labelRect, string.Concat(thing.GetWeightTip(), '\n', UIText.DragToReorder.Translate()));
            //    }

            //    TooltipHandler.TipRegion(deleteRect, UIText.Delete.Translate());
            //}
        }

        private void DrawItemsInLoadout(Rect canvas)
        {
            Rect listRect = new Rect(0, 0, canvas.width - GenUI.ScrollBarWidth, _scrollViewHeight);

            // darken whole area
            GUI.DrawTexture(canvas, TexResource.DarkBackground);
            Widgets.BeginScrollView(canvas, ref _slotScrollPosition, listRect);

            // Set up reorder functionality
            int reorderableGroup = ReorderableWidget.NewGroup(
                (int from, int to) =>
                {
                    this.ReorderItems(from, to);
                    DrawUtility.ResetDrag();
                }
                , ReorderableDirection.Vertical
                , -1
                , (index, pos) =>
                {
                    Vector2 position = DrawUtility.GetPostionForDrag(windowRect.ContractedBy(Margin), new Vector2(canvas.x, canvas.y), index, GenUI.ListSpacing);
                    Rect dragRect = new Rect(position, new Vector2(listRect.width, GenUI.ListSpacing));
                    Find.WindowStack.ImmediateWindow(
                        Rand.Int,
                        dragRect,
                        WindowLayer.Super,
                        () =>
                        {
                            GUI.DrawTexture(dragRect.AtZero(), SolidColorMaterials.NewSolidColorTexture(Theme.MilkySlicky.BackGround));
                            this.DrawItemRow(dragRect.AtZero(), _currentLoadout.CachedList[index], 0, true);
                            //GUI.color = Color.white;
                        }, false);
                });

            float curY = 0f;
            for (int i = 0; i < _currentLoadout.CachedList.Count; i++)
            {
                // create row rect
                Rect row = new Rect(0f, curY, listRect.width, GenUI.ListSpacing);
                curY += GenUI.ListSpacing;

                // alternate row background
                if (i % 2 == 0)
                    GUI.DrawTexture(row, TexResource.DarkBackground);

                this.DrawItemRow(row, _currentLoadout.CachedList[i], reorderableGroup);
                GUI.color = Color.white;
            }

            _scrollViewHeight = curY + GenUI.ListSpacing;

            Widgets.EndScrollView();
        }

        private void DrawItemsInSourceCategory(Rect canvas)
        {
            GUI.DrawTexture(canvas, TexResource.DarkBackground);

            Rect viewRect = new Rect(canvas);
            viewRect.height = _source.Count * GenUI.ListSpacing;
            viewRect.width -= GenUI.GapWide;

            Widgets.BeginScrollView(canvas, ref _availableScrollPosition, viewRect.AtZero());
            for (int i = 0; i < _source.Count; i++)
            {
                Color baseColor = GUI.color;

                // gray out weapons not in stock
                if (_source[i].isGreyedOut)
                    GUI.color = Color.gray;

                Rect row = new Rect(0f, i * GenUI.ListSpacing, canvas.width, GenUI.ListSpacing);
                Rect labelRect = new Rect(row);
                TooltipHandler.TipRegion(row, _source[i].thingDef.GetWeightAndBulkTip());

                labelRect.xMin += GenUI.GapTiny;
                if (i % 2 == 0)
                    GUI.DrawTexture(row, TexResource.DarkBackground);

                int j = i;
                DrawUtility.DrawLineButton
                    (labelRect
                    , _source[j].thingDef.LabelCap
                    , _source[j].thingDef
                    , (target) =>
                    {
                        _currentLoadout.Add(target);
                    });

                GUI.color = baseColor;
            }

            Widgets.EndScrollView();
        }

        private void ReorderItems(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                _currentLoadout.CachedList.Insert(newIndex, _currentLoadout.CachedList[oldIndex]);
                _currentLoadout.CachedList.RemoveAt((oldIndex >= newIndex) ? (oldIndex + 1) : oldIndex);
            }
        }

        private void DrawSourceIcon(SourceSelection sourceSelected, Texture2D texButton, ref WidgetRow row, string tip)
        {
            if (row.ButtonIcon(texButton, tip, GenUI.MouseoverColor))
            {
                SetSource(sourceSelected);
                _availableScrollPosition = Vector2.zero;
            }
        }

        protected virtual void DrawWeightBar(Rect canvas)
        {
            float fillPercent = Mathf.Clamp01(_currentLoadout.Weight / MassUtility.Capacity(_pawn));
            GenBar.BarWithOverlay(
                canvas,
                fillPercent,
                this.IsOverEncumbered(_pawn, _currentLoadout) ? AwesomeInventoryTex.ValvetTex as Texture2D : AwesomeInventoryTex.RMPrimaryTex as Texture2D,
                UIText.Weight.Translate(),
                _currentLoadout.Weight.ToString("0.#") + "/" + MassUtility.Capacity(_pawn).ToStringMass(),
                string.Empty);
        }

        protected virtual bool IsOverEncumbered(Pawn pawn, AILoadout loadout)
        {
            return loadout.Weight / MassUtility.Capacity(pawn) > 1f;
        }

        #endregion Methods

        private class SelectableItem
        {
            public ThingDef thingDef;
            public bool isGreyedOut;
        }
    }
}
