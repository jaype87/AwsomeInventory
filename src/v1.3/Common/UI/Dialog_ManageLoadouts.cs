// <copyright file="Dialog_ManageLoadouts.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

namespace AwesomeInventory.UI
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Loadout;

    using RimWorld;

    using UnityEngine;

    using Verse;

    /// <summary>
    ///     A dialog window for managing loadouts.
    /// </summary>
    [RegisterType(typeof(Dialog_ManageLoadouts), typeof(Dialog_ManageLoadouts))]
    public class Dialog_ManageLoadouts : Window, IReset
    {
        /// <summary>
        ///     Source categories for loadout dialog.
        /// </summary>
        public enum CategorySelection
        {
            /// <summary>
            ///     Ranged weapons.
            /// </summary>
            Ranged,

            /// <summary>
            ///     Melee weapons.
            /// </summary>
            Melee,

            /// <summary>
            ///     Apparels.
            /// </summary>
            Apparel,

            /// <summary>
            ///     Things that are minified.
            /// </summary>
            Minified,

            /// <summary>
            ///     Generic thing def.
            /// </summary>
            Generic,

            /// <summary>
            ///     All things, won't include generics, can include minified/able now.
            /// </summary>
            All
        }

        private const float _paneDivider = 5 / 9f;
        private const int _loadoutNameMaxLength = 50;

        /// <summary>
        ///     Controls the window size and position.
        /// </summary>
        private static Vector2 _initialSize;

        private static List<ItemContext> _itemContexts;
        private static readonly List<IReset> _resettables = new List<IReset>();
        private List<ItemContext> _categorySource;

        /// <summary>
        ///     The current loadout the dialog window shows.
        /// </summary>
        protected AwesomeInventoryLoadout _currentLoadout;

        private string _filter = string.Empty;
        private readonly bool _fixPawn;
        private Vector2 _loadoutListScrollPosition = Vector2.zero;

        /// <summary>
        ///     The selected pawn for this dialog window.
        /// </summary>
        protected Pawn _pawn;

        private float _scrollViewHeight;
        private List<ItemContext> _source;

        private Vector2 _sourceListScrollPosition = Vector2.zero;

    #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Dialog_ManageLoadouts" /> class.
        /// </summary>
        /// <param name="loadout"> Selected loadout. </param>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="fixPawn"> Wheter the loadout window should display the same pawn even user selects another. </param>
        [Obsolete(ErrorText.NoDirectCall, true)]
        public Dialog_ManageLoadouts(AwesomeInventoryLoadout loadout, Pawn pawn, bool fixPawn = false)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));
            _pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));

            var width = UIText.TenCharsString.Times(11).GetWidthCached();
            _initialSize = new Vector2(width, UI.screenHeight / 2f);

            _currentLoadout = loadout;
            _fixPawn        = fixPawn;
            _resettables.Add(this);
            _resettables.Add(new WhiteBlacklistView());

            doCloseX                = true;
            forcePause              = true;
            absorbInputAroundWindow = false;
            closeOnClickedOutside   = false;
            closeOnAccept           = false;
        }

    #endregion Constructors

        /// <summary>
        ///     Gets initial window size for this dialog.
        /// </summary>
        public override Vector2 InitialSize => _initialSize;

        private static HashSet<ThingDef> AllSuitableDefs => DefManager.SuitableDefs;

        /// <summary>
        ///     Store states on if the loadout window is displaying wish list or blacklist.
        /// </summary>
        protected class WhiteBlacklistView : IReset
        {
            /// <summary>
            ///     Display name for wish list.
            /// </summary>
            public static readonly string WishlistDisplayName = UIText.Wishlist.TranslateSimple();

            /// <summary>
            ///     Display name for blacklist.
            /// </summary>
            public static readonly string BlacklistDisplayName = UIText.Blacklist.TranslateSimple();

            /// <summary>
            ///     Gets or sets a value indicating whether the loadout window is displaying wish list.
            /// </summary>
            public static bool IsWishlist { get; set; } = true;

            /// <summary>
            ///     Reset state.
            /// </summary>
            public void Reset()
            {
                IsWishlist = true;
            }
        }

        private class ItemContext
        {
            public bool IsVisible;
            public ThingDef ThingDef;
        }

    #region Methods

        /// <summary>
        ///     Draw contents in window.
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

            if (Event.current.type == EventType.Layout)
                return;

            if (!_fixPawn)
            {
                if (Find.Selector.SingleSelectedThing is Pawn pawn && pawn.IsColonist && !pawn.Dead)
                {
                    if (pawn != _pawn)
                    {
                        _pawn = pawn;
                        var loadout = _pawn.GetLoadout();

                        if (loadout == null)
                        {
                            Close();

                            return;
                        }

                        _currentLoadout = loadout;
                        _resettables.ForEach(r => r.Reset());
                    }
                    else
                    {
                        _currentLoadout = _pawn.GetLoadout();
                    }
                }
                else
                {
                    Close();
                }
            }

            GUI.BeginGroup(canvas);
            Text.Font = GameFont.Small;

            var useableSize = new Vector2(canvas.width - DrawUtility.CurrentPadding * 2,
                canvas.height - DrawUtility.CurrentPadding * 2);

            // Draw top buttons
            var buttonRow = new WidgetRow(useableSize.x, 0, UIDirection.LeftThenDown, useableSize.x);
            DrawTopButtons(buttonRow);

            // Set up rects for the rest parts.
            var nameFieldRect = new Rect(0,
                0,
                buttonRow.FinalX - WidgetRow.DefaultGap,
                GenUI.SmallIconSize);

            var whiteBlackListRect = new Rect(0,
                nameFieldRect.yMax + GenUI.GapTiny,
                useableSize.x * _paneDivider,
                GenUI.SmallIconSize);

            var weightBarRect = GetWeightRect(whiteBlackListRect.ReplaceyMax(canvas.yMax));

            var loadoutItemsRect = new Rect(0,
                whiteBlackListRect.yMax + GenUI.GapTiny,
                whiteBlackListRect.width,
                useableSize.y - whiteBlackListRect.yMax - GenUI.GapTiny - weightBarRect.height);

            var sourceButtonRect = new Rect(loadoutItemsRect.xMax + Listing.ColumnSpacing,
                buttonRow.FinalY + GenUI.ListSpacing,
                useableSize.x - loadoutItemsRect.xMax - Listing.ColumnSpacing,
                GenUI.SmallIconSize);

            var sourceItemsRect = new Rect(sourceButtonRect.x,
                sourceButtonRect.yMax + GenUI.GapTiny,
                sourceButtonRect.width,
                useableSize.y - sourceButtonRect.yMax);

            DrawWishBlackListOptions(whiteBlackListRect);
            DrawLoadoutNameField(nameFieldRect);

            if (WhiteBlacklistView.IsWishlist)
                DrawItemsInLoadout(loadoutItemsRect, _currentLoadout);
            else
                DrawItemsInLoadout(loadoutItemsRect, _currentLoadout.BlackList);

            GUI.DrawTexture(new Rect(loadoutItemsRect.x, loadoutItemsRect.yMax, loadoutItemsRect.width, 1f), BaseContent.GreyTex);
            DrawWeightBar(weightBarRect);

            DrawCategoryIcon(sourceButtonRect);
            DrawItemsInSourceCategory(sourceItemsRect);

            GUI.EndGroup();
        }

        /// <summary>
        ///     Called by game root code before the window is opened.
        /// </summary>
        /// <remarks>
        ///     It is only called once for the entire time period when this dialog is open including a change in selected
        ///     pawn.
        /// </remarks>
        public override void PreOpen()
        {
            base.PreOpen();
            var visibleDefs = new HashSet<ThingDef>(AllSuitableDefs);
            visibleDefs.IntersectWith(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEverOrMinifiable).Select(t => t.def).Distinct());

            var itemContexts = new ConcurrentBag<ItemContext>();

            Parallel.ForEach(Partitioner.Create(AllSuitableDefs)
                , thingDef =>
                {
                    var itemContext = new ItemContext
                    {
                        ThingDef = thingDef
                    };

                    if (thingDef is AIGenericDef genericDef)
                        itemContext.IsVisible = visibleDefs.Any(def => genericDef.Includes(def));
                    else
                        itemContext.IsVisible = visibleDefs.Contains(thingDef);

                    itemContexts.Add(itemContext);
                });

            _itemContexts = itemContexts.OrderBy(td => td.ThingDef.label).ToList();
            SetCategory(CategorySelection.Ranged);
        }

        /// <summary>
        ///     Draw icon for source category.
        /// </summary>
        /// <param name="canvas"> <see cref="Rect" /> for drawing. </param>
        public void DrawCategoryIcon(Rect canvas)
        {
            var row = new WidgetRow(canvas.x, canvas.y);
            DrawCategoryIcon(CategorySelection.Ranged, TexResource.IconRanged, ref row, UIText.SourceRangedTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Melee, TexResource.IconMelee, ref row, UIText.SourceMeleeTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Apparel, TexResource.Apparel, ref row, UIText.SourceApparelTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Minified, TexResource.IconMinified, ref row, UIText.SourceMinifiedTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.Generic, TexResource.IconGeneric, ref row, UIText.SourceGenericTip.TranslateSimple());
            DrawCategoryIcon(CategorySelection.All, TexResource.IconAll, ref row, UIText.SourceAllTip.TranslateSimple());

            var nameFieldLen = UIText.TenCharsString.GetWidthCached();
            var incrementX   = canvas.xMax - row.FinalX - nameFieldLen - WidgetRow.IconSize - WidgetRow.ButtonExtraSpace;
            row.Gap(incrementX);
            row.Icon(TexResource.IconSearch, UIText.SourceFilterTip.TranslateSimple());

            var textFilterRect = new Rect(row.FinalX, canvas.y, nameFieldLen, canvas.height);
            DrawTextFilter(textFilterRect);
            TooltipHandler.TipRegion(textFilterRect, UIText.SourceFilterTip.TranslateSimple());
        }

        /// <summary>
        ///     Set category for drawing available items in selection.
        /// </summary>
        /// <param name="category"> Category for selection. </param>
        public void SetCategory(CategorySelection category)
        {
            switch (category)
            {
                case CategorySelection.Ranged:
                    _source = _categorySource = _itemContexts.Where(context => context.ThingDef.IsRangedWeapon).ToList();

                    break;

                case CategorySelection.Melee:
                    _source = _categorySource = _itemContexts.Where(context => context.ThingDef.IsMeleeWeapon).ToList();

                    break;

                case CategorySelection.Apparel:
                    _source = _categorySource = _itemContexts.Where(context => context.ThingDef.IsApparel).ToList();

                    break;

                case CategorySelection.Minified:
                    _source = _categorySource = _itemContexts.Where(context => context.ThingDef.Minifiable).ToList();

                    break;

                case CategorySelection.Generic:
                    _source = _categorySource = _itemContexts.Where(context => context.ThingDef is AIGenericDef).ToList();

                    break;

                case CategorySelection.All:
                default:
                    _source = _categorySource = _itemContexts;

                    break;
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            _sourceListScrollPosition  = Vector2.zero;
            _loadoutListScrollPosition = Vector2.zero;
        }

        /// <summary>
        ///     Draw wishlist/blacklist choice on screen.
        /// </summary>
        /// <param name="canvas"> Rect for drawing. </param>
        protected virtual void DrawWishBlackListOptions(Rect canvas)
        {
            var globalSettingIconRect = canvas.ReplaceWidth(GenUI.SmallIconSize);
            TooltipHandler.TipRegion(globalSettingIconRect, UIText.GlobalApparelSetting.TranslateSimple());

            if (Widgets.ButtonImage(globalSettingIconRect, TexResource.Gear))
                Find.WindowStack.Add(new Dialog_ManageOutfitSettings(_currentLoadout.filter));

            var costumeIconRect = globalSettingIconRect.ReplaceX(globalSettingIconRect.xMax);
            TooltipHandler.TipRegion(costumeIconRect, UIText.CostumeSettings.TranslateSimple());

            if (Widgets.ButtonImage(costumeIconRect, TexResource.Costume))
                Find.WindowStack.Add(new Dialog_Costume(_currentLoadout, _pawn));

            var centerCanvas = canvas.ReplacexMin(canvas.x + GenUI.SmallIconSize * 2).ReplacexMax(canvas.xMax - 3 * GenUI.SmallIconSize);
            var drawingRect  = new Rect(0, centerCanvas.y, GenUI.SmallIconSize * 2 + DrawUtility.TwentyCharsWidth + GenUI.GapTiny * 2, GenUI.ListSpacing);
            var centeredRect = drawingRect.CenteredOnXIn(centerCanvas);
            var widgetRow    = new WidgetRow(centeredRect.x, centeredRect.y, UIDirection.RightThenDown);

            if (widgetRow.ButtonIcon(TexResource.TriangleLeft))
                WhiteBlacklistView.IsWishlist ^= true;

        #pragma warning disable SA1118 // Parameter should not span multiple lines
            Text.Anchor = TextAnchor.MiddleCenter;

            widgetRow.LabelWithHighlight(WhiteBlacklistView.IsWishlist
                                             ? WhiteBlacklistView.WishlistDisplayName
                                             : WhiteBlacklistView.BlacklistDisplayName
                , WhiteBlacklistView.IsWishlist
                      ? UIText.WishlistTooltip.TranslateSimple()
                      : UIText.BlacklistTooltip.TranslateSimple()
                , DrawUtility.TwentyCharsWidth);
            Text.Anchor = TextAnchor.UpperLeft;
        #pragma warning restore SA1118 // Parameter should not span multiple lines

            if (widgetRow.ButtonIcon(TexResource.TriangleRight))
                WhiteBlacklistView.IsWishlist ^= true;

            // Import items from other loadout
            var importRect = new Rect(centerCanvas.xMax, canvas.y, GenUI.SmallIconSize, canvas.height);
            DrawImportLoadout(importRect);

            // Sort list alphabetically.
            var sortRect = importRect.ReplaceX(importRect.xMax);
            TooltipHandler.TipRegion(sortRect, UIText.SortListAlphabetically.TranslateSimple());

            if (Widgets.ButtonImage(sortRect, TexResource.SortLetterA))
                _currentLoadout.ThingGroupSelectors.SortBy(g => g.LabelCapNoCount.StripTags());

            // Use generic stuff for all apparels in list.
            var genericAssignRect = sortRect.ReplaceX(sortRect.xMax);

            if (Widgets.ButtonImage(genericAssignRect, TexResource.GenericTransform))
                Find.WindowStack.Add(new Dialog_InstantMessage(UIText.UseGenericStuff.TranslateSimple()
                    , new Vector2(400, 150)
                    , buttonAAction: () =>
                    {
                        foreach (var groupSelector in _currentLoadout)
                            if (groupSelector.AllowedThing.IsApparel)
                                foreach (var selector in groupSelector)
                                    (selector as SingleThingSelector).SetStuff(null);
                    }
                    , buttonBText: UIText.CancelButton.TranslateSimple()));
        }

        /// <summary>
        ///     Import items from other loadouts.
        /// </summary>
        /// <param name="importRect"> Rect for drawing. </param>
        protected virtual void DrawImportLoadout(Rect importRect)
        {
            TooltipHandler.TipRegion(importRect, UIText.ImportLoadout.TranslateSimple());

            if (!Widgets.ButtonImage(importRect, TexResource.ImportLoadout))
                return;

            var options = LoadoutManager.Loadouts.Where(l => l.GetType() == typeof(AwesomeInventoryLoadout))
                                        .OrderBy(l => l.label)
                                        .Select(loadout => new FloatMenuOption(loadout.label, () =>
                                        {
                                            foreach (var selector in loadout)
                                                _currentLoadout.Add(new ThingGroupSelector(selector));

                                            _currentLoadout.CopyCostumeFrom(loadout);
                                        }))
                                        .ToList();

            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>
        ///     Draw top buttons in <see cref="Dialog_ManageLoadouts" />.
        /// </summary>
        /// <param name="row"> Helper for drawing. </param>
        protected virtual void DrawTopButtons(WidgetRow row)
        {
            ValidateArg.NotNull(row, nameof(row));

            var loadouts = LoadoutManager.Loadouts.Where(l => l.GetType() != typeof(AwesomeInventoryCostume)).ToList();

            List<FloatMenuOption> options;

            if (row.ButtonText(UIText.DeleteLoadout.TranslateSimple()))
            {
                options = loadouts.Select((t, j) => j)
                                  .Select(i => new FloatMenuOption(loadouts[i].label, () =>
                                  {
                                      if (loadouts.Count > 1)
                                      {
                                          foreach (var costume in loadouts[i].Costumes)
                                              LoadoutManager.TryRemoveLoadout(costume);

                                          LoadoutManager.TryRemoveLoadout(loadouts[i]);
                                      }
                                      else
                                      {
                                          var msgRect = new Rect(Vector2.zero, Text.CalcSize(UIText.TryToDeleteLastLoadout.TranslateSimple())).ExpandedBy(50);

                                          Find.WindowStack.Add(new Dialog_InstantMessage(UIText.TryToDeleteLastLoadout.TranslateSimple(), msgRect.size, UIText.OK.TranslateSimple())
                                          {
                                              windowRect = msgRect
                                          });
                                      }
                                  }))
                                  .ToList();

                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (row.ButtonText(UIText.CopyLoadout.TranslateSimple()))
            {
                var loadout = _currentLoadout is AwesomeInventoryCostume costume ? costume.Base : _currentLoadout;
                _currentLoadout = new AwesomeInventoryLoadout(loadout);
                LoadoutManager.AddLoadout(_currentLoadout);
                _pawn.SetLoadout(_currentLoadout);
            }

            if (row.ButtonText(UIText.NewLoadout.TranslateSimple()))
            {
                var loadout = new AwesomeInventoryLoadout(_pawn)
                {
                    label = LoadoutManager.GetIncrementalLabel(_currentLoadout.label)
                };
                LoadoutManager.AddLoadout(loadout);
                _currentLoadout = loadout;
                _pawn.SetLoadout(loadout);
            }

            if (!row.ButtonText(UIText.SelectLoadout.TranslateSimple()))
                return;

            options = new List<FloatMenuOption>();

            if (loadouts.Count == 0)
                options.Add(new FloatMenuOption(UIText.NoLoadout.Translate(), null));
            else
                options.AddRange(loadouts.Select((t, j) => j)
                                         .Select(i => new FloatMenuOption(loadouts[i].label, () =>
                                         {
                                             _currentLoadout            = loadouts[i];
                                             _loadoutListScrollPosition = Vector2.zero;
                                             _pawn.SetLoadout(_currentLoadout);

                                             if (BetterPawnControlUtility.IsActive)
                                                 BetterPawnControlUtility.SaveState(new List<Pawn>
                                                 {
                                                     _pawn
                                                 });
                                         })));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>
        ///     Draw item information in a row.
        /// </summary>
        /// <param name="row"> Rect used for drawing. </param>
        /// <param name="index"> Position of a <see cref="ThingGroupSelector" /> in <paramref name="groupSelectors" />. </param>
        /// <param name="groupSelectors"> Thing to draw. </param>
        /// <param name="reorderableGroup"> The group this <paramref name="row" /> belongs to. </param>
        /// <param name="drawShadow"> If true, it draws a shadow copy of the row. It is used for drawing a row when it is dragged. </param>
        protected virtual void DrawItemRow(Rect row, int index, IList<ThingGroupSelector> groupSelectors, int reorderableGroup, bool drawShadow = false)
        {
            ValidateArg.NotNull(row, nameof(row));
            ValidateArg.NotNull(groupSelectors, nameof(groupSelectors));

            /* Label (fill) | Weight | Gear Icon | Count Field | Delete Icon */

            var widgetRow     = new WidgetRow(row.width, row.y, UIDirection.LeftThenDown, row.width);
            var groupSelector = groupSelectors[index];

            // Draw delete icon.
            DrawDeleteIconInThingRow(widgetRow, groupSelectors, groupSelector);

            Text.Anchor = TextAnchor.MiddleLeft;

            // Draw count field.
            if (WhiteBlacklistView.IsWishlist)
            {
                DrawCountFieldInThingRow(new Rect(widgetRow.FinalX - WidgetRow.IconSize * 2 - WidgetRow.DefaultGap, widgetRow.FinalY, WidgetRow.IconSize * 2, GenUI.ListSpacing),
                    groupSelector);
                widgetRow.Gap(WidgetRow.IconSize * 2 + 4f);
            }

            // Draw gear icon.
            DrawGearIconInThingRow(widgetRow, groupSelector);

            // Draw threshold.
            if (widgetRow.ButtonIcon(TexResource.Threshold, UIText.StockMode.TranslateSimple()))
                Find.WindowStack.Add(new Dialog_RestockTrigger(groupSelector));

            Text.WordWrap = false;
            Text.Anchor   = TextAnchor.MiddleLeft;

            // Draw weight.
            widgetRow.Label(groupSelector.Weight.ToStringMass());

            var textSize = drawShadow
                               ? Text.CalcSize(groupSelector.LabelCapNoCount.StripTags()).x
                               : Text.CalcSize(groupSelector.LabelCapNoCount).x;

            // 24f for the defIcon, 130f for the fixed size UI elements
            widgetRow.Gap(row.width - (24f + widgetRow.CellGap * 2 + textSize + 130f + Text.CalcSize(groupSelector.Weight.ToStringMass()).x));

            // Draw label.
            var labelRect = widgetRow.Label(drawShadow
                                                ? groupSelector.LabelCapNoCount.StripTags().Colorize(Theme.MilkySlicky.ForeGround)
                                                : groupSelector.LabelCapNoCount);

            Text.Anchor = TextAnchor.UpperLeft;

            if (!drawShadow)
            {
                ReorderableWidget.Reorderable(reorderableGroup, labelRect);

                // Tooltips && Highlights
                Widgets.DrawHighlightIfMouseover(row);

                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(row))
                {
                    TooltipHandler.ClearTooltipsFrom(labelRect);

                    if (Event.current.button == 1)
                    {
                        var floatMenu = new FloatMenu(new List<FloatMenuOption>
                        {
                            new FloatMenuOption(UIText.AddToAllLoadout.TranslateSimple()
                                , () =>
                                {
                                    var loadout = _currentLoadout;

                                    if (loadout is AwesomeInventoryCostume costume)
                                        loadout = costume.Base;

                                    groupSelector.AddToLoadouts(LoadoutManager.PlainLoadouts.Except(loadout));
                                })
                        });
                        Find.WindowStack.Add(floatMenu);
                    }
                }
                else
                {
                    TooltipHandler.TipRegion(labelRect
                        , UIText.DragToReorder.TranslateSimple() + Environment.NewLine + UIText.RigthClickForMoreOptions.TranslateSimple());
                }
            }

            Text.WordWrap = true;

            // Draw icon.
            if (groupSelector.AllowedThing.DrawMatSingle != null && groupSelector.AllowedThing.DrawMatSingle.mainTexture != null)
                widgetRow.DefIcon(groupSelector.AllowedThing);
        }

        /// <summary>
        ///     Draw a gear icon in thing row.
        /// </summary>
        /// <param name="widgetRow"> A drawing helper. </param>
        /// <param name="groupSelector"> The target group selector.</param>
        protected virtual void DrawGearIconInThingRow(WidgetRow widgetRow, ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(widgetRow, nameof(widgetRow));
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            var allowedThing = groupSelector.AllowedThing;

            if ((allowedThing.MadeFromStuff || allowedThing.HasComp(typeof(CompQuality)) || allowedThing.useHitPoints)
                && widgetRow.ButtonIcon(TexResource.Gear))
                Find.WindowStack.Add(new Dialog_StuffAndQuality(groupSelector));
        }

        /// <summary>
        ///     Draw a delete icon in thing row.
        /// </summary>
        /// <param name="widgetRow"> Helper for drawing. </param>
        /// <param name="groupSelectors"> A list of <see cref="ThingGroupSelector" /> displayed in loadout window. </param>
        /// <param name="groupSelector"> The <see cref="ThingGroupSelector" /> the delete icon attached to. </param>
        protected virtual void DrawDeleteIconInThingRow(WidgetRow widgetRow, IList<ThingGroupSelector> groupSelectors, ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(widgetRow, nameof(widgetRow));
            ValidateArg.NotNull(groupSelectors, nameof(groupSelectors));

            if (widgetRow.ButtonIcon(TexResource.CloseXSmall, UIText.Delete.TranslateSimple()))
                groupSelectors.Remove(groupSelector);
        }

        /// <summary>
        ///     Check if <paramref name="loadout" /> is too heavy for <paramref name="pawn" />.
        /// </summary>
        /// <param name="pawn"> The pawn used for baseline. </param>
        /// <param name="loadout"> Loadout to check. </param>
        /// <returns> Returns true if <paramref name="loadout" /> is too heavy for <paramref name="pawn" />. </returns>
        protected virtual bool IsOverEncumbered(Pawn pawn, AwesomeInventoryLoadout loadout)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));

            return loadout.Weight / MassUtility.Capacity(pawn) > 1f;
        }

        /// <summary>
        ///     Get a weight rect for drawing weight bar.
        /// </summary>
        /// <param name="canvas"> The canvas at which bottom a weight bar is drawn. </param>
        /// <returns> Return a rect for drawing weight bar. </returns>
        protected virtual Rect GetWeightRect(Rect canvas)
        {
            canvas.Set(canvas.x, canvas.yMax - DrawUtility.CurrentPadding - GenUI.ListSpacing, canvas.width, GenUI.ListSpacing);

            return canvas;
        }

        /// <summary>
        ///     Draw weight bar at the bottom of the loadout window.
        /// </summary>
        /// <param name="canvas"> Rect for drawing. </param>
        protected virtual void DrawWeightBar(Rect canvas)
        {
            var fillPercent = Mathf.Clamp01(_currentLoadout.Weight / MassUtility.Capacity(_pawn));

            GenBar.BarWithOverlay(canvas,
                fillPercent,
                IsOverEncumbered(_pawn, _currentLoadout) ? AwesomeInventoryTex.ValvetTex as Texture2D : AwesomeInventoryTex.RWPrimaryTex as Texture2D,
                UIText.Weight.Translate(),
                _currentLoadout.Weight.ToString("0.##") + "/" + MassUtility.Capacity(_pawn).ToStringMass(),
                string.Empty);
        }

        /// <summary>
        ///     Draw count field in a thing row.
        /// </summary>
        /// <param name="canvas"> Rect for drawing. </param>
        /// <param name="groupSelector"> The selected group selector. </param>
        protected virtual void DrawCountFieldInThingRow(Rect canvas, ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            var countInt = groupSelector.AllowedStackCount;
            var buffer   = countInt.ToString();
            Widgets.TextFieldNumeric(canvas, ref countInt, ref buffer);
            TooltipHandler.TipRegion(canvas, UIText.CountFieldTip.Translate(groupSelector.AllowedStackCount));

            if (countInt != groupSelector.AllowedStackCount)
                groupSelector.SetStackCount(countInt);
        }

        private static void ReorderItems(int oldIndex, int newIndex, IList<ThingGroupSelector> groupSelectors)
        {
            if (oldIndex == newIndex)
                return;
            
            var index    = newIndex > oldIndex ? newIndex - 1 : newIndex;
            var selector = groupSelectors[oldIndex];
            groupSelectors.RemoveAt(oldIndex);
            groupSelectors.Insert(index, selector);
        }

        private void DrawTextFilter(Rect canvas)
        {
            var filter = GUI.TextField(canvas, _filter);

            if (filter == _filter)
                return;
            
            _filter = filter;
            _source = _categorySource.Where(td => td.ThingDef.label.ToUpperInvariant().Contains(_filter.ToUpperInvariant())).ToList();
        }

        private void DrawLoadoutNameField(Rect canvas)
        {
            _currentLoadout.label = Widgets.TextField(canvas, _currentLoadout.label, _loadoutNameMaxLength, Outfit.ValidNameRegex);
        }

        private void DrawItemsInLoadout(Rect canvas, IList<ThingGroupSelector> groupSelectors)
        {
            var listRect = new Rect(0, 0, canvas.width - GenUI.ScrollBarWidth, _scrollViewHeight);

            // darken whole area
            GUI.DrawTexture(canvas, TexResource.DarkBackground);
            Widgets.BeginScrollView(canvas, ref _loadoutListScrollPosition, listRect);

            // Set up reorder functionality
            var reorderableGroup = ReorderableWidget.NewGroup_NewTemp((from, to) =>
                {
                    ReorderItems(from, to, groupSelectors);
                    DrawUtility.ResetDrag();
                }
                , ReorderableDirection.Vertical
                , -1
                , (index, pos) =>
                {
                    var position = DrawUtility.GetPostionForDrag(windowRect.ContractedBy(Margin), new Vector2(canvas.x, canvas.y), index, GenUI.ListSpacing);
                    var dragRect = new Rect(position, new Vector2(listRect.width, GenUI.ListSpacing));

                    Find.WindowStack.ImmediateWindow(Rand.Int,
                        dragRect,
                        WindowLayer.Super,
                        () =>
                        {
                            GUI.DrawTexture(dragRect.AtZero(), Theme.MilkySlicky.BGTex);
                            DrawItemRow(dragRect.AtZero(), index, groupSelectors, 0, true);
                        }, false);
                });

            var curY = 0f;

            for (var i = 0; i < groupSelectors.Count; i++)
            {
                // create row rect
                var row = new Rect(0f, curY, listRect.width, GenUI.ListSpacing);
                curY += GenUI.ListSpacing;

                // alternate row background
                if (i % 2 == 0)
                    GUI.DrawTexture(row, TexResource.DarkBackground);

                DrawItemRow(row, i, groupSelectors, reorderableGroup);
                GUI.color = Color.white;
            }

            _scrollViewHeight = curY + GenUI.ListSpacing;

            Widgets.EndScrollView();
        }

        private void DrawItemsInSourceCategory(Rect canvas)
        {
            GUI.DrawTexture(canvas, TexResource.DarkBackground);

            var viewRect = new Rect(canvas)
            {
                height = _source.Count * GenUI.ListSpacing
            };
            viewRect.width  -= GenUI.GapWide;

            Widgets.BeginScrollView(canvas, ref _sourceListScrollPosition, viewRect.AtZero());
            DrawUtility.GetIndexRangeFromScrollPosition(canvas.height, _sourceListScrollPosition.y, out var from, out var to, GenUI.ListSpacing);

            for (var i = from; i < to && i < _source.Count; i++)
            {
                var baseColor = GUI.color;
                var thingDef  = _source[i].ThingDef;

                if (!_source[i].IsVisible)
                    GUI.color = Color.gray;

                var row      = new Rect(0f, i * GenUI.ListSpacing, canvas.width, GenUI.ListSpacing);
                var iconRect = new Rect(row).ReplaceWidth(24f);
                iconRect.xMin += GenUI.GapTiny;
                Widgets.DefIcon(iconRect, thingDef);

                var labelRect = new Rect(row).ReplaceWidth(row.width - GenUI.SmallIconSize - GenUI.ScrollBarWidth);
                TooltipHandler.TipRegion(row, thingDef.GetDetailedTooltip());

                labelRect.xMin += GenUI.GapTiny;

                if (i % 2 == 0)
                    GUI.DrawTexture(row, TexResource.DarkBackground);

                var j = i;

                DrawUtility.DrawLabelButton(labelRect
                    , thingDef.LabelCap
                    , () =>
                    {
                        var groupSelector = new ThingGroupSelector(thingDef);

                        ThingSelector thingSelector;

                        if (thingDef is AIGenericDef genericDef)
                            thingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<GenericThingSelector>(thingDef);
                        else
                            thingSelector = AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(thingDef, null);

                        groupSelector.SetStackCount(1);
                        groupSelector.Add(thingSelector);

                        if (WhiteBlacklistView.IsWishlist)
                            _currentLoadout.Add(groupSelector);
                        else
                            _currentLoadout.AddToBlacklist(groupSelector);
                    },
                    TextAnchor.MiddleCenter);

                GUI.color = baseColor;

                var infoRect = new Rect(labelRect.xMax, labelRect.y, GenUI.SmallIconSize, GenUI.ListSpacing);

                if (!(thingDef is AIGenericDef) && Widgets.ButtonImage(infoRect, TexResource.Info))
                {
                    var pair = new ThingStuffPair(thingDef, GenStuff.DefaultStuffFor(thingDef));
                    Find.WindowStack.Add(new Dialog_InfoCard(pair.MakeThingWithoutID()));
                }
            }

            Widgets.EndScrollView();
        }

        private void DrawCategoryIcon(CategorySelection sourceSelected, Texture2D texButton, ref WidgetRow row, string tip)
        {
            if (!row.ButtonIcon(texButton, tip))
                return;
            
            SetCategory(sourceSelected);
            _filter                   = string.Empty;
            _sourceListScrollPosition = Vector2.zero;
        }

    #endregion Methods
    }
}
