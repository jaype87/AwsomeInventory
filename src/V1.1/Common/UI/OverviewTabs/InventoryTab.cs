// <copyright file="InventoryTab.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// An overview tab for inventory.
    /// </summary>
    public class InventoryTab : OverviewTab
    {
        private List<Thing> _storedThing = new List<Thing>();

        private ThingGroupModel _thingGroupModel;

        private InventoryViewModel _viewModel = new InventoryViewModel();

        private List<Thing> _missingItems = new List<Thing>();

        /// <inheritdoc/>
        public override string Label => UIText.Inventory.TranslateSimple();

        /// <inheritdoc/>
        public override void DoTabContent(Rect rect)
        {
            float columnWidth = rect.width / 4;
            float listWidth = columnWidth - GenUI.GapSmall;
            rect.y += GenUI.GapTiny;

            DrawInventoryList(
                UIText.Equipment.TranslateSimple()
                , rect.ReplaceWidth(listWidth)
                , _thingGroupModel.Weapons.GetMergedList(ThingComparer.Instance, (thing) => thing.LabelCap)
                , ref _viewModel.EquipmentViewScrollPos
                , _thingGroupModel.Weapons.Count * GenUI.ListSpacing
                , ref _viewModel.EquipmentSearchText);

            DrawInventoryList(
                UIText.Apparel.TranslateSimple()
                , new Rect(columnWidth, rect.y, listWidth, rect.height)
                , _thingGroupModel.Apparels.GetMergedList(ThingComparer.Instance, (thing) => thing.LabelCap)
                , ref _viewModel.ApparelViewScrollPos
                , _thingGroupModel.Apparels.Count * GenUI.ListSpacing
                , ref _viewModel.ApparelSearchText);

            DrawInventoryList(
                UIText.Inventory.TranslateSimple()
                , new Rect(columnWidth * 2, rect.y, listWidth, rect.height)
                , _thingGroupModel.Miscellaneous.GetMergedList(ThingComparer.Instance, (thing) => thing.LabelCap)
                , ref _viewModel.MiscellanousScrollPos
                , _thingGroupModel.Miscellaneous.Count * GenUI.ListSpacing
                , ref _viewModel.MiscellanousSearchText);

            DrawInventoryList(
                UIText.MissingItems.TranslateSimple()
                , new Rect(columnWidth * 3, rect.y, listWidth, rect.height)
                , _missingItems
                , ref _viewModel.MissingItemsScrollPos
                , _missingItems.Count * GenUI.ListSpacing
                , ref _viewModel.MissingItemsSearchText);
        }

        /// <inheritdoc/>
        public override void PreOpen()
        {
            _storedThing = Find.Maps.SelectMany(
                map =>
                    map.haulDestinationManager
                       .AllGroupsListForReading
                       .SelectMany(
                       (slotGroup) => slotGroup.HeldThings)).ToList();

            _thingGroupModel = _storedThing.MakeThingGroup();
        }

        /// <inheritdoc/>
        public override void PreSwitch()
        {
            _missingItems = this.GetMissingItems();
        }

        private static void DrawInventoryList(string label, Rect rect, List<Thing> items, ref Vector2 scrollPos, float listLength, ref string searchText)
        {
            Widgets.NoneLabelCenteredVertically(rect.ReplaceHeight(GenUI.ListSpacing), label);
            Text.Anchor = TextAnchor.MiddleLeft;
            SearchableList.Draw(
                rect.ReplaceyMin(rect.yMin + GenUI.ListSpacing)
                , GenUI.ListSpacing
                , (canvas, thing, index) => Widgets.Label(canvas, thing.LabelCap.ColorizeByQuality(thing))
                , ref scrollPos
                , listLength
                , items
                , ref searchText);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private List<Thing> GetMissingItems()
        {
            return PawnUtility.AllPlayerPawns
                    .SelectMany(
                        p => p.TryGetComp<CompAwesomeInventoryLoadout>()?.InventoryMargins
                              .Where(
                                pair => pair.Value < 0)
                              .Select(
                                pair =>
                                {
                                    Thing thing = pair.Key.SingleThingSelectors.FirstOrDefault()?.ThingSample;
                                    if (thing != null)
                                        thing.stackCount = -pair.Value;
                                    return thing;
                                })
                              .Where(
                                thing => thing != null))
                    .GetMergedList(ThingComparer.Instance, (thing) => thing.LabelCap);
        }

        private class InventoryViewModel
        {
            public Vector2 EquipmentViewScrollPos;

            public string EquipmentSearchText;

            public Vector2 ApparelViewScrollPos;

            public string ApparelSearchText;

            public Vector2 MiscellanousScrollPos;

            public string MiscellanousSearchText;

            public Vector2 MissingItemsScrollPos;

            public string MissingItemsSearchText;
        }
    }
}
