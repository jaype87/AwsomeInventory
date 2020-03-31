// <copyright file="Dialog_ManageLoadouts.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
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
        private const int _loadoutNameMaxLength = 50;
        private static readonly HashSet<ThingDef> _allSuitableDefs = GameComponent_DefManager.GetSuitableDefs();

        /// <summary>
        /// Controls the window size and position.
        /// </summary>
        private static Vector2 _initialSize;
        private static Pawn _pawn;
        private static List<ItemContext> _itemContexts;

        private Vector2 _availableScrollPosition = Vector2.zero;
        private AwesomeInventoryLoadout _currentLoadout;
        private string _filter = string.Empty;
        private float _scrollViewHeight;
        private Vector2 _slotScrollPosition = Vector2.zero;
        private List<ItemContext> _categorySource;
        private List<ItemContext> _source;
        private CategorySelection _sourceType = CategorySelection.Ranged;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_ManageLoadouts"/> class.
        /// </summary>
        /// <param name="loadout"> Selected loadout. </param>
        /// <param name="pawn"> Selected pawn. </param>
        public Dialog_ManageLoadouts(AwesomeInventoryLoadout loadout, Pawn pawn)
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
            closeOnClickedOutside = false;
            closeOnAccept = false;
            draggable = true;
        }

        #endregion Constructors

        /// <summary>
        /// Source categories for loadout dialog.
        /// </summary>
        public enum CategorySelection
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

            if (Find.Selector.SingleSelectedThing is Pawn pawn && pawn.IsColonist && !pawn.Dead)
            {
                _pawn = pawn;
                AwesomeInventoryLoadout loadout = _pawn.GetLoadout();
                if (loadout == null)
                {
                    loadout = new AwesomeInventoryLoadout(pawn);
                    LoadoutManager.AddLoadout(loadout);
                    pawn.SetLoadout(loadout);
                }

                _currentLoadout = loadout;
            }
            else
            {
                this.Close();
            }

            GUI.BeginGroup(canvas);
            Text.Font = GameFont.Small;
            Vector2 useableSize = new Vector2(
                canvas.width - DrawUtility.CurrentPadding * 2,
                canvas.height - DrawUtility.CurrentPadding * 2);

            // Draw top buttons
            WidgetRow buttonRow = new WidgetRow(useableSize.x, 0, UIDirection.LeftThenDown, useableSize.x);
            this.DrawTopButtons(buttonRow);

            // Set up rects for the rest parts.
            Rect nameFieldRect = new Rect(
                0,
                0,
                useableSize.x - buttonRow.FinalX - GenUI.GapWide,
                GenUI.SmallIconSize);

            Rect whiteBlackListRect = new Rect(
                0,
                nameFieldRect.yMax + GenUI.GapTiny,
                useableSize.x * _paneDivider,
                GenUI.SmallIconSize);

            Rect loadoutItemsRect = new Rect(
                0,
                whiteBlackListRect.yMax + GenUI.GapTiny,
                whiteBlackListRect.width,
                useableSize.y - whiteBlackListRect.yMax - GenUI.ListSpacing);

            Rect sourceButtonRect = new Rect(
                loadoutItemsRect.xMax + Listing.ColumnSpacing,
                buttonRow.FinalY + GenUI.ListSpacing,
                useableSize.x - loadoutItemsRect.xMax - Listing.ColumnSpacing,
                GenUI.SmallIconSize);

            Rect sourceItemsRect = new Rect(
                sourceButtonRect.x,
                sourceButtonRect.yMax + GenUI.GapTiny,
                sourceButtonRect.width,
                useableSize.y - sourceButtonRect.yMax);

            Rect weightBarRect = new Rect(
                0,
                canvas.yMax - DrawUtility.CurrentPadding - GenUI.SmallIconSize,
                loadoutItemsRect.width,
                GenUI.SmallIconSize);

            this.DrawWhiteBlackListOptions(whiteBlackListRect);
            this.DrawLoadoutNameField(nameFieldRect);
            this.DrawItemsInLoadout(loadoutItemsRect);
            GUI.DrawTexture(new Rect(loadoutItemsRect.x, loadoutItemsRect.yMax, loadoutItemsRect.width, 1f), BaseContent.GreyTex);
            this.DrawWeightBar(weightBarRect);

            this.DrawCategoryIcon(sourceButtonRect);
            this.DrawItemsInSourceCategory(sourceItemsRect);

            GUI.EndGroup();
        }

        /// <summary>
        /// Called by game root code before the window is opened.
        /// </summary>
        /// <remarks> It is only called once for the entire time period when this dialog is open including a change in selected pawn. </remarks>
        public override void PreOpen()
        {
            base.PreOpen();
            HashSet<ThingDef> visibleDefs = GameComponent_DefManager.GetSuitableDefs();
            visibleDefs.IntersectWith(
                Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEverOrMinifiable).Select(t => t.def));

            _itemContexts = new List<ItemContext>();
            foreach (ThingDef td in _allSuitableDefs)
            {
                ItemContext itemContext = new ItemContext() { thingDef = td };
                if (td is AIGenericDef genericDef)
                {
                    itemContext.IsVisible = !visibleDefs.Any(def => genericDef.Includes(def));
                }
                else
                {
                    itemContext.IsVisible = !visibleDefs.Contains(td);
                }

                _itemContexts.Add(itemContext);
            }

            _itemContexts = _itemContexts.OrderBy(td => td.thingDef.label).ToList();
            SetCategory(CategorySelection.Ranged);
        }

        protected virtual void DrawWhiteBlackListOptions(Rect canvas)
        {
            Rect drawingRect = new Rect(0, canvas.y, GenUI.SmallIconSize * 2 + DrawUtility.TwentyCharsWidth + GenUI.GapTiny * 2, GenUI.ListSpacing);
            Rect centeredRect = drawingRect.CenteredOnXIn(canvas);
            WidgetRow widgetRow = new WidgetRow(centeredRect.x, centeredRect.y, UIDirection.RightThenDown);
            widgetRow.ButtonIcon(TexResource.TriangleLeft);
            Text.Anchor = TextAnchor.MiddleCenter;
            widgetRow.LabelWithHighlight("White List", DrawUtility.TwentyCharsWidth);
            Text.Anchor = TextAnchor.UpperLeft;
            widgetRow.ButtonIcon(TexResource.TriangleRight);
        }

        /// <summary>
        /// Draw top buttons in <see cref="Dialog_ManageLoadouts"/>.
        /// </summary>
        /// <param name="row"> Helper for drawing. </param>
        protected virtual void DrawTopButtons(WidgetRow row)
        {
            ValidateArg.NotNull(row, nameof(row));

            List<AwesomeInventoryLoadout> loadouts = LoadoutManager.Loadouts;

            if (row.ButtonText(UIText.DeleteLoadout.TranslateSimple()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int j = 0; j < loadouts.Count; j++)
                {
                    int i = j;
                    options.Add(new FloatMenuOption(
                        loadouts[i].label,
                        () =>
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
                                    new Dialog_InstantMessage(
                                        ErrorMessage.TryToDeleteLastLoadout.Translate(), msgRect.size, UIText.OK.Translate())
                                    {
                                        windowRect = msgRect,
                                    });
                            }
                        }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (row.ButtonText(UIText.CopyLoadout.TranslateSimple()))
            {
                _currentLoadout = new AwesomeInventoryLoadout(_currentLoadout);
                LoadoutManager.AddLoadout(_currentLoadout);
                _pawn.SetLoadout(_currentLoadout);
            }

            if (row.ButtonText(UIText.NewLoadout.TranslateSimple()))
            {
                AwesomeInventoryLoadout loadout = new AwesomeInventoryLoadout(_pawn)
                {
                    label = LoadoutManager.GetIncrementalLabel(_currentLoadout.label),
                };
                LoadoutManager.AddLoadout(loadout);
                _currentLoadout = loadout;
                _pawn.SetLoadout(loadout);
            }

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
        }

        /// <summary>
        /// Draw icon for source category.
        /// </summary>
        /// <param name="canvas"> <see cref="Rect"/> for drawing. </param>
        public void DrawCategoryIcon(Rect canvas)
        {
            WidgetRow row = new WidgetRow(canvas.x, canvas.y);
            DrawCategoryIcon(CategorySelection.Ranged, TexResource.IconRanged, ref row, UIText.SourceRangedTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Melee, TexResource.IconMelee, ref row, UIText.SourceMeleeTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Apparel, TexResource.Apparel, ref row, UIText.SourceApparelTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Minified, TexResource.IconMinified, ref row, UIText.SourceMinifiedTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Generic, TexResource.IconGeneric, ref row, UIText.SourceGenericTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.All, TexResource.IconAll, ref row, UIText.SourceAllTip.TranslateSimple());

            float nameFieldLen = GenUI.GetWidthCached(UIText.TenCharsString);
            float incrementX = canvas.xMax - row.FinalX - nameFieldLen - WidgetRow.IconSize - WidgetRow.ButtonExtraSpace;
            row.Gap(incrementX);
            row.Icon(TexResource.IconSearch, UIText.SourceFilterTip.TranslateSimple());

            Rect textFilterRect = new Rect(row.FinalX, canvas.y, nameFieldLen, canvas.height);
            this.DrawTextFilter(textFilterRect);
            TooltipHandler.TipRegion(textFilterRect, UIText.SourceFilterTip.TranslateSimple());
        }

        /// <summary>
        /// Set category for drawing available items in selection.
        /// </summary>
        /// <param name="category"> Category for selection. </param>
        public void SetCategory(CategorySelection category)
        {
            switch (category)
            {
                case CategorySelection.Ranged:
                    _source = _categorySource = _itemContexts.Where(context => context.thingDef.IsRangedWeapon).ToList();
                    _sourceType = CategorySelection.Ranged;
                    break;

                case CategorySelection.Melee:
                    _source = _categorySource = _itemContexts.Where(context => context.thingDef.IsMeleeWeapon).ToList();
                    _sourceType = CategorySelection.Melee;
                    break;

                case CategorySelection.Apparel:
                    _source = _categorySource = _itemContexts.Where(context => context.thingDef.IsApparel).ToList();
                    _sourceType = CategorySelection.Apparel;
                    break;

                case CategorySelection.Minified:
                    _source = _categorySource = _itemContexts.Where(context => context.thingDef.Minifiable).ToList();
                    _sourceType = CategorySelection.Minified;
                    break;

                case CategorySelection.Generic:
                    _source = _categorySource = _itemContexts.Where(context => context.thingDef is AIGenericDef).ToList();
                    _sourceType = CategorySelection.Generic;
                    break;

                case CategorySelection.All:
                default:
                    _source = _categorySource = _itemContexts;
                    _sourceType = CategorySelection.All;
                    break;
            }
        }

        /// <summary>
        /// Draw item information in a row.
        /// </summary>
        /// <param name="row"> Rect used for drawing. </param>
        /// <param name="groupSelector"> Thing to draw. </param>
        /// <param name="reorderableGroup"> The group this <paramref name="row"/> belongs to. </param>
        /// <param name="drawShadow"> If true, it draws a shadow copy of the row. It is used for drawing a row when it is dragged. </param>
        protected virtual void DrawItemRow(Rect row, ThingGroupSelector groupSelector, int reorderableGroup, bool drawShadow = false)
        {
            ValidateArg.NotNull(row, nameof(row));
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            /* Label (fill) | Weight | Gear Icon | Count Field | Delete Icon */

            WidgetRow widgetRow = new WidgetRow(row.width, row.y, UIDirection.LeftThenDown, row.width);

            // Draw delete icon.
            if (widgetRow.ButtonIcon(TexResource.CloseXSmall, UIText.Delete.TranslateSimple()))
            {
                _currentLoadout.Remove(groupSelector);
            }

            Text.Anchor = TextAnchor.MiddleLeft;

            // Draw count field.
            this.DrawCountField(
                new Rect(widgetRow.FinalX - WidgetRow.IconSize * 2 - WidgetRow.DefaultGap, widgetRow.FinalY, WidgetRow.IconSize * 2, GenUI.ListSpacing),
                groupSelector);
            widgetRow.GapButtonIcon();
            widgetRow.GapButtonIcon();

            // Draw gear icon.
            ThingDef allowedThing = groupSelector.AllowedThing;
            if ((allowedThing.MadeFromStuff || allowedThing.HasComp(typeof(CompQuality)) || allowedThing.useHitPoints)
                && widgetRow.ButtonIcon(TexResource.Gear))
            {
                Find.WindowStack.Add(new Dialog_StuffAndQuality(groupSelector, _currentLoadout));
            }

            Text.WordWrap = false;

            // Draw weight.
            widgetRow.Label(groupSelector.Weight.ToStringMass());

            // Draw label.
            Rect labelRect = widgetRow.Label(
                drawShadow
                    ? groupSelector.LabelCapNoCount.StripTags().Colorize(Theme.MilkySlicky.ForeGround)
                    : groupSelector.LabelCapNoCount
                , widgetRow.FinalX);

            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            if (!drawShadow)
            {
                ReorderableWidget.Reorderable(reorderableGroup, labelRect);

                // Tooltips && Highlights
                Widgets.DrawHighlightIfMouseover(row);
                if (Event.current.type == EventType.MouseDown)
                {
                    TooltipHandler.ClearTooltipsFrom(labelRect);
                }
                else
                {
                    TooltipHandler.TipRegion(labelRect, UIText.DragToReorder.Translate());
                }
            }
        }

        private void DrawCountField(Rect canvas, ThingGroupSelector groupSelector)
        {
            int countInt = groupSelector.AllowedStackCount;
            string buffer = countInt.ToString();
            Widgets.TextFieldNumeric(canvas, ref countInt, ref buffer);
            TooltipHandler.TipRegion(canvas, UIText.CountFieldTip.Translate(groupSelector.AllowedStackCount));

            if (countInt != groupSelector.AllowedStackCount)
            {
                int delta = countInt - groupSelector.AllowedStackCount;
                _pawn.GetComp<CompAwesomeInventoryLoadout>().InventoryMargins[groupSelector] -= delta;
            }

            groupSelector.SetStackCount(countInt);
        }

        private void DrawTextFilter(Rect canvas)
        {
            string filter = GUI.TextField(canvas, _filter);
            if (filter != _filter)
            {
                _filter = filter;
                _source = _categorySource.Where(td => td.thingDef.label.ToUpperInvariant().Contains(_filter.ToUpperInvariant())).ToList();
            }
        }

        private void DrawLoadoutNameField(Rect canvas)
        {
            _currentLoadout.label = Widgets.TextField(canvas, _currentLoadout.label, _loadoutNameMaxLength, Outfit.ValidNameRegex);
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
                            this.DrawItemRow(dragRect.AtZero(), _currentLoadout[index], 0, true);
                        }, false);
                });

            float curY = 0f;
            for (int i = 0; i < _currentLoadout.Count; i++)
            {
                // create row rect
                Rect row = new Rect(0f, curY, listRect.width, GenUI.ListSpacing);
                curY += GenUI.ListSpacing;

                // alternate row background
                if (i % 2 == 0)
                    GUI.DrawTexture(row, TexResource.DarkBackground);

                this.DrawItemRow(row, _currentLoadout[i], reorderableGroup);
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
                if (_source[i].IsVisible)
                    GUI.color = Color.gray;

                Rect row = new Rect(0f, i * GenUI.ListSpacing, canvas.width, GenUI.ListSpacing);
                Rect labelRect = new Rect(row);
                TooltipHandler.TipRegion(row, _source[i].thingDef.GetWeightAndBulkTip());

                labelRect.xMin += GenUI.GapTiny;
                if (i % 2 == 0)
                    GUI.DrawTexture(row, TexResource.DarkBackground);

                int j = i;
                DrawUtility.DrawLabelButton(
                    labelRect
                    , _source[j].thingDef.LabelCap
                    , () =>
                    {
                        ThingDef thingDef = _source[j].thingDef;
                        ThingGroupSelector groupSelector = new ThingGroupSelector(thingDef, _currentLoadout.NextGroupID);

                        ThingSelector thingSelector;
                        if (thingDef is AIGenericDef genericDef)
                        {
                            thingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<GenericThingSelector>(thingDef);
                        }
                        else
                        {
                            thingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(thingDef, null);
                        }

                        groupSelector.SetStackCount(1);

                        groupSelector.Add(thingSelector);
                        _currentLoadout.Add(groupSelector);
                    });

                GUI.color = baseColor;
            }

            Widgets.EndScrollView();
        }

        private void ReorderItems(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                _currentLoadout.Insert(newIndex, _currentLoadout[oldIndex]);
                _currentLoadout.RemoveAt((oldIndex >= newIndex) ? (oldIndex + 1) : oldIndex);
            }
        }

        private void DrawCategoryIcon(CategorySelection sourceSelected, Texture2D texButton, ref WidgetRow row, string tip)
        {
            if (row.ButtonIcon(texButton, tip))
            {
                SetCategory(sourceSelected);
                _filter = string.Empty;
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

        protected virtual bool IsOverEncumbered(Pawn pawn, AwesomeInventoryLoadout loadout)
        {
            return loadout.Weight / MassUtility.Capacity(pawn) > 1f;
        }

        #endregion Methods

        private class ItemContext
        {
            public ThingDef thingDef;
            public bool IsVisible;
        }
    }
}
