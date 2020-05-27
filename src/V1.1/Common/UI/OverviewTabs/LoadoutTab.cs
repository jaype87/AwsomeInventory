// <copyright file="LoadoutTab.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A tab for summary on loadouts.
    /// </summary>
    public class LoadoutTab : OverviewTab
    {
        private static DrawHelper _drawHelper;
        private static bool _dragging;
        private static float _pawnNameWidth = UIText.TenCharsString.Times(2).GetWidthCached();
        private static float _tabHeight = 0;
        private static float _rowHeight = GenUI.ListSpacing * 3;

        private List<Pawn> _colonist = new List<Pawn>();
        private List<PawnRowViewModel> _pawnRowScrollPos = new List<PawnRowViewModel>();
        private Vector2 _tabScrollPos = Vector2.zero;
        private ViewMode _mode = ViewMode.Loadout;

        private AwesomeInventoryLoadout _copy;
        private ThingGroupSelector _groupSelectCopy;

        static LoadoutTab()
        {
            if (!AwesomeInventoryServiceProvider.TryGetImplementation(out _drawHelper))
                Log.Error("No implementation for DrawHelper");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadoutTab"/> class.
        /// </summary>
        /// <param name="containerState"> State of the container. </param>
        public LoadoutTab(ContainerState containerState)
            : base(containerState)
        {
        }

        private enum ViewMode
        {
            Loadout,
            Inventory,
        }

        /// <inheritdoc/>
        public override string Label { get; } = UIText.Loadouts.TranslateSimple();

        /// <inheritdoc/>
        public override void DoTabContent(Rect rect)
        {
            this.DrawModeButton(rect, out float rollingY);

            Rect scrollRect = rect.ReplaceyMin(rollingY);
            Widgets.BeginScrollView(scrollRect, ref _tabScrollPos, new Rect(0, 0, rect.width, _tabHeight));

            this.DrawLoadoutView(scrollRect);

            Widgets.EndScrollView();
        }

        /// <inheritdoc/>
        public override void PreOpen()
        {
            _colonist = Find.Maps.SelectMany(map => map.mapPawns.FreeColonists).ToList();
            _pawnRowScrollPos = _colonist.Select(c => new PawnRowViewModel()).ToList();
            _tabScrollPos = Vector2.zero;
            _tabHeight = _colonist.Count * GenUI.ListSpacing * 3;
        }

        /// <inheritdoc/>
        public override void PreSwitch()
        {
            // no op.
        }

        private void DrawModeButton(Rect rowRect, out float rollingY)
        {
            WidgetRow widgetRow = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown);
            switch (_mode)
            {
                case ViewMode.Loadout:
                    if (widgetRow.ButtonText(UIText.LoadoutView.TranslateSimple()))
                        _mode = ViewMode.Inventory;
                    break;

                case ViewMode.Inventory:
                    if (widgetRow.ButtonText(UIText.InventoryView.TranslateSimple()))
                        _mode = ViewMode.Loadout;
                    break;
            }

            rollingY = widgetRow.FinalY + GenUI.ListSpacing;
        }

        private void DrawLoadoutView(Rect rect)
        {
            Rect rowRect = rect.ReplaceHeight(_rowHeight);
            DrawUtility.GetIndexRangeFromScrollPosition(_tabHeight > rect.height ? rect.height : _tabHeight, _tabScrollPos.y, out int from, out int to, _rowHeight);
            for (int i = from; i < to; i++)
            {
                this.DrawTabRow(rowRect.ReplaceY(i * _rowHeight), _colonist[i], _pawnRowScrollPos[i], _mode);
            }
        }

        private void DrawMakeLoadout(Rect rect, Pawn pawn)
        {
            string label = UIText.MakeLoadout.TranslateSimple();
            WidgetRow widgetRow = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown);
            if (widgetRow.ButtonText(label))
            {
                Find.WindowStack.Add(
                    new FloatMenu(pawn.MakeActionableLoadoutOption()));
            }
            else if (_copy != null && widgetRow.ButtonIcon(TexResource.Paste))
            {
                pawn.SetLoadout(_copy);
            }
        }

        private void DrawTabRow(Rect rect, Pawn pawn, PawnRowViewModel viewModel, ViewMode viewMode)
        {
            Rect nameRect = rect.ReplaceWidth(_pawnNameWidth).ReplaceHeight(GenUI.ListSpacing);
            Rect selectRect = nameRect.ReplaceHeight(GenUI.ListSpacing * 2);
            if (Widgets.ButtonInvisible(selectRect))
            {
                Selector selector = Find.Selector;
                selector.ClearSelection();
                selector.Select(pawn);
            }

            Widgets.DrawHighlightIfMouseover(selectRect);
            Widgets.Label(nameRect, pawn.NameFullColored);
            Widgets.Label(nameRect.ReplaceY(nameRect.yMax), pawn.LabelNoCountColored);

            rect = rect.ReplaceX(nameRect.xMax).ReplaceWidth((rect.width - _pawnNameWidth) / 2);
            if (viewMode == ViewMode.Loadout)
                this.DrawLoadoutRow(rect, pawn, viewModel);
            else if (viewMode == ViewMode.Inventory)
                this.DrawInventoryRow(rect, pawn, viewModel);
        }

        private void DrawInventoryRow(Rect rect, Pawn pawn, PawnRowViewModel viewModel)
        {
            IEnumerable<Thing> equipped = pawn.equipment.AllEquipmentListForReading.Concat(pawn.apparel.WornApparel);
            this.DrawScrollableThings(rect, equipped, UIText.EquippedItems.TranslateSimple(), ref viewModel.EquippedViewWidth, ref viewModel.EquippedScrollPos);

            rect = rect.ReplaceX(rect.xMax + GenUI.Gap).ReplaceWidth(rect.width - GenUI.Gap);
            IEnumerable<Thing> items = pawn.inventory.innerContainer;
            this.DrawScrollableThings(rect, items, UIText.InventoryItems.TranslateSimple(), ref viewModel.InventoryViewWidth, ref viewModel.InventoryScrollPos);
        }

        private void DrawLoadoutRow(Rect rect, Pawn pawn, PawnRowViewModel viewModel)
        {
            if (pawn.UseLoadout(out CompAwesomeInventoryLoadout comp))
            {
                this.DrawScrollableLoadout(rect, pawn, comp, viewModel);

                rect = rect.ReplaceX(rect.xMax + GenUI.Gap).ReplaceWidth(rect.width - GenUI.Gap);
                this.DrawScrollableMissing(rect, pawn, comp, viewModel);
            }
            else
            {
                DrawMakeLoadout(rect, pawn);
            }
        }

        private void DrawScrollableMissing(Rect rect, Pawn pawn, CompAwesomeInventoryLoadout comp, PawnRowViewModel viewModel)
        {
            Rect labelRect = rect.ReplaceHeight(GenUI.ListSpacing);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, UIText.MissingItems.TranslateSimple());
            Text.Anchor = TextAnchor.UpperLeft;

            Rect missingItemsRect = new Rect(rect.x, rect.y + GenUI.ListSpacing, rect.width, GenUI.ListSpacing + GenUI.ScrollBarWidth);
            Rect viewRect = missingItemsRect.ReplaceHeight(GenUI.ListSpacing).ReplaceWidth(viewModel.MissingViewWidth);

            Widgets.ScrollHorizontal(missingItemsRect, ref viewModel.MissingScrollPos, viewRect);
            Widgets.BeginScrollView(missingItemsRect, ref viewModel.MissingScrollPos, viewRect);
            Rect iconRect = viewRect.ReplaceWidth(GenUI.SmallIconSize);

            foreach (Thing thing in comp.InventoryMargins
                .Where(pair => pair.Value < 0)
                .Select(pair => pair.Key.SingleThingSelectors.FirstOrDefault()?.ThingSample)
                .MakeThingGroup().OrderedList)
            {
                this.DrawThingIcon(iconRect, pawn, thing, comp);
                iconRect = iconRect.ReplaceX(iconRect.xMax);
            }

            viewModel.MissingViewWidth = iconRect.x - rect.x;

            Widgets.EndScrollView();
        }

        private void DrawScrollableLoadout(Rect rect, Pawn pawn, CompAwesomeInventoryLoadout comp, PawnRowViewModel viewModel)
        {
            if (pawn.outfits.CurrentOutfit is AwesomeInventoryLoadout loadout)
            {
                // Loadout name button
                string loadoutName = loadout.label;
                WidgetRow widgetRow = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown);
                if (widgetRow.ButtonText(loadoutName))
                {
                    Dialog_ManageLoadouts dialog = AwesomeInventoryServiceProvider.MakeInstanceOf<Dialog_ManageLoadouts>(loadout, pawn, true);
                    dialog.closeOnClickedOutside = true;
                    Find.WindowStack.Add(dialog);
                }
                else if (widgetRow.ButtonIcon(TexResource.Copy))
                {
                    _copy = loadout;
                    Messages.Message(string.Concat(UIText.CopyLoadout.TranslateSimple(), $" {_copy.label}"), MessageTypeDefOf.NeutralEvent);
                }
                else if (_copy != null && widgetRow.ButtonIcon(TexResource.Paste))
                {
                    pawn.SetLoadout(_copy);
                }

                // Loadout scroll view
                Rect loadoutItemRect = new Rect(rect.x, widgetRow.FinalY + GenUI.ListSpacing, rect.width, GenUI.ListSpacing + GenUI.ScrollBarWidth);
                Rect viewRect = loadoutItemRect.ReplaceHeight(GenUI.ListSpacing).ReplaceWidth(viewModel.LoadoutViewWidth);

                Widgets.ScrollHorizontal(loadoutItemRect, ref viewModel.LoadoutScrollPos, viewRect);
                Widgets.BeginScrollView(loadoutItemRect, ref viewModel.LoadoutScrollPos, viewRect);
                Rect iconRect = viewRect.ReplaceWidth(GenUI.SmallIconSize);
                foreach (ThingGroupSelector groupSelector in comp.Loadout
                    .OrderByDescending(a => a.SingleThingSelectors.FirstOrDefault()?.ThingSample, new LoadoutUtility.ThingTypeComparer()))
                {
                    if (groupSelector.SingleThingSelectors.FirstOrDefault() is SingleThingSelector selector)
                    {
                        this.DrawThingIcon(iconRect, pawn, selector.ThingSample, comp);
                        this.DragItemToLoadout(iconRect, groupSelector);
                        iconRect = iconRect.ReplaceX(iconRect.xMax);
                    }
                }

                viewModel.LoadoutViewWidth = iconRect.x - rect.x;

                Widgets.EndScrollView();
            }
        }

        private void DrawThingIcon(Rect rect, Pawn pawn, Thing thing, CompAwesomeInventoryLoadout comp)
        {
            this.DrawThingIcon(rect, thing);
            if (Event.current.button == 1 && Widgets.ButtonInvisible(rect))
            {
                Find.WindowStack.Add(
                    new FloatMenu(
                        new List<FloatMenuOption>()
                        {
                            ContextMenuUtility.OptionForThingOnPawn(pawn, thing, comp),
                        }));
            }
        }

        private void DrawThingIcon(Rect rect, Thing thing)
        {
            Widgets.ThingIcon(rect, thing);
            Widgets.DrawHighlightIfMouseover(rect);
            TooltipHandler.TipRegion(rect, _drawHelper.TooltipTextFor(thing, false));
        }

        private void DragItemToLoadout(Rect iconRect, ThingGroupSelector groupSelector)
        {
            if (InputUtility.IsLeftMouseClick && iconRect.Contains(Event.current.mousePosition))
            {
                if (InputUtility.IsControl && InputUtility.IsLeftMouseClick)
                {
                    _groupSelectCopy = groupSelector;
                    _dragging = true;
                    Event.current.Use();
                }
            }

            if (InputUtility.IsLeftMouseUp && _dragging)
            {
                int index = Mathf.FloorToInt(Event.current.mousePosition.y / _rowHeight);
                if (index > -1 && index < _colonist.Count && _colonist[index].UseLoadout(out CompAwesomeInventoryLoadout comp))
                    comp.Loadout.Add(new ThingGroupSelector(_groupSelectCopy));

                _dragging = false;
                Event.current.Use();
            }

            if (_dragging)
                DrawUtility.DrawMouseAttachmentWithThing(_groupSelectCopy.AllowedThing, _groupSelectCopy.SingleThingSelectors.FirstOrDefault()?.AllowedStuff);
        }

        private void DrawScrollableThings(Rect rect, IEnumerable<Thing> things, string label, ref float width, ref Vector2 scrollPos)
        {
            Rect labelRect = rect.ReplaceHeight(GenUI.ListSpacing);
            Widgets.Label(labelRect, label);

            Rect outRect = new Rect(rect.x, labelRect.yMax, rect.width, GenUI.ListSpacing + GenUI.ScrollBarWidth);
            Rect viewRect = new Rect(rect.x, labelRect.yMax, width, GenUI.ListSpacing);
            Widgets.ScrollHorizontal(outRect, ref scrollPos, viewRect);
            Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);

            Rect iconRect = outRect.ReplaceWidth(GenUI.SmallIconSize);
            foreach (Thing thing in things)
            {
                this.DrawThingIcon(iconRect.ReplaceHeight(GenUI.SmallIconSize), thing);
                iconRect = iconRect.ReplaceX(iconRect.xMax);
            }

            Widgets.EndScrollView();

            width = things.Count() * GenUI.SmallIconSize;
        }

        private class PawnRowViewModel
        {
            public Vector2 LoadoutScrollPos = Vector2.zero;

            public float LoadoutViewWidth = 0;

            public Vector2 MissingScrollPos = Vector2.zero;

            public float MissingViewWidth = 0;

            public Vector2 EquippedScrollPos = Vector2.zero;

            public float EquippedViewWidth = 0;

            public Vector2 InventoryScrollPos = Vector2.zero;

            public float InventoryViewWidth = 0;
        }
    }
}
