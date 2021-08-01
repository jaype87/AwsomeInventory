// <copyright file="InventoryTab.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private static ConcurrentDictionary<Thing, ThingModel> _thingSlotGroup
            = new ConcurrentDictionary<Thing, ThingModel>(ThingComparer.Instance);

        private ThingGroupModel _thingGroupModel;

        private InventoryViewModel _viewModel = new InventoryViewModel();

        private List<Thing> _missingItems = new List<Thing>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryTab"/> class.
        /// </summary>
        /// <param name="containerState"> State of the container. </param>
        public InventoryTab(ContainerState containerState)
            : base(containerState)
        {
        }

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
                , _thingGroupModel.Weapons
                , ref _viewModel.EquipmentViewScrollPos
                , _thingGroupModel.Weapons.Count * GenUI.ListSpacing
                , ref _viewModel.EquipmentSearchText
                , true);

            DrawInventoryList(
                UIText.Apparel.TranslateSimple()
                , new Rect(columnWidth, rect.y, listWidth, rect.height)
                , _thingGroupModel.Apparels
                , ref _viewModel.ApparelViewScrollPos
                , _thingGroupModel.Apparels.Count * GenUI.ListSpacing
                , ref _viewModel.ApparelSearchText
                , true);

            DrawInventoryList(
                UIText.Inventory.TranslateSimple()
                , new Rect(columnWidth * 2, rect.y, listWidth, rect.height)
                , _thingGroupModel.Miscellaneous
                , ref _viewModel.MiscellanousScrollPos
                , _thingGroupModel.Miscellaneous.Count * GenUI.ListSpacing
                , ref _viewModel.MiscellanousSearchText
                , true);

            DrawInventoryList(
                UIText.MissingItems.TranslateSimple()
                , new Rect(columnWidth * 3, rect.y, listWidth, rect.height)
                , _missingItems
                , ref _viewModel.MissingItemsScrollPos
                , _missingItems.Count * GenUI.ListSpacing
                , ref _viewModel.MissingItemsSearchText
                , false);
        }

        /// <inheritdoc/>
        public override void PreOpen()
        {
            _thingSlotGroup.Clear();
            var groups =
                Find.Maps.SelectMany(map => map.haulDestinationManager.AllGroupsListForReading)
                         .Select(
                              (slotGroup) =>
                                  new
                                  {
                                      SlotGroup = slotGroup,
                                      Things = slotGroup.HeldThings.Where(t => !t.LabelCapNoCount.NullOrEmpty())
                                          .GetMergedList(
                                             ThingComparer.Instance
                                             , (thing) =>
                                             {
                                                 if (thing is Corpse corpse)
                                                 {
                                                     if (corpse.Bugged)
                                                         return corpse.ThingID;
                                                     else
                                                         return corpse.LabelCapNoCount;
                                                 }
                                                 else
                                                 {
                                                     return thing.LabelCapNoCount;
                                                 }
                                             }),
                                  });
            Parallel.ForEach(
                Partitioner.Create(groups)
                , (group) =>
                {
                    foreach (Thing thing in group.Things)
                    {
                        _thingSlotGroup.AddOrUpdate(
                            thing
                            , new ThingModel()
                            {
                                StackCount = thing.stackCount,
                                SlotGroups = new ConcurrentBag<SlotGroup>() { group.SlotGroup },
                            }
                            , (t, oldvalue) =>
                            {
                                Interlocked.Add(ref oldvalue.StackCount, thing.stackCount);
                                oldvalue.SlotGroups.Add(group.SlotGroup);
                                return oldvalue;
                            });
                    }
                });

            _thingGroupModel = _thingSlotGroup.Keys.MakeThingGroup();
        }

        /// <inheritdoc/>
        public override void PreSwitch()
        {
            _missingItems = this.GetMissingItems();
        }

        private void DrawInventoryList(string label, Rect rect, List<Thing> items, ref Vector2 scrollPos, float listLength, ref string searchText, bool inSlot)
        {
            Widgets.NoneLabelCenteredVertically(rect.ReplaceHeight(GenUI.ListSpacing), label);
            Text.Anchor = TextAnchor.MiddleLeft;
            SearchableList.Draw(
                rect.ReplaceyMin(rect.yMin + GenUI.ListSpacing)
                , GenUI.ListSpacing
                , (canvas, thing, index) =>
                {
                    if (inSlot)
                        thing.stackCount = _thingSlotGroup[thing].StackCount;

                    Rect labelRect = canvas.ReplaceWidth(canvas.width - GenUI.SmallIconSize * 2 - GenUI.ScrollBarWidth);

                    string thingLabel = string.Empty;
                    if (thing is Corpse corpse)
                    {
                        if (corpse.Bugged)
                            thingLabel = corpse.ThingID;
                        else
                            thingLabel = corpse.InnerPawn.NameFullColored;
                    }
                    else
                    {
                        thingLabel = thing.LabelCap.ColorizeByQuality(thing);
                    }

                    Widgets.Label(labelRect, thingLabel);
                    if (inSlot && Widgets.ButtonImage(new Rect(labelRect.xMax, labelRect.y, GenUI.SmallIconSize, GenUI.ListSpacing), TexResource.ArrowBottom))
                    {
                        FloatMenuUtility.MakeMenu(
                            _thingSlotGroup[thing].SlotGroups.Select(g => g.parent)
                            , g =>
                            {
                                switch (g)
                                {
                                    case Thing t:
                                        return $"{t.LabelCap} - {t.thingIDNumber}";

                                    case Zone zone:
                                        return zone.label;

                                    default:
                                        return g.ToString();
                                }
                            }
                            , t => () =>
                            {
                                Find.Selector.ClearSelection();
                                Find.Selector.Select(t);
                                Find.CameraDriver.JumpToCurrentMapLoc(t.Position);
                                _containerState.Minimizing = true;
                            });
                    }

                    Widgets.InfoCardButton(labelRect.xMax + GenUI.SmallIconSize, labelRect.y, thing.def);
                }
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

        private class ThingModel
        {
            public ConcurrentBag<SlotGroup> SlotGroups;

            public int StackCount;

            public string HolderName;

            public IntVec3 Position;
        }
    }
}
