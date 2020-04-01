// <copyright file="CompAwesomeInventoryLoadout.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.Resources;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Save loudout information and its current state.
    /// </summary>
    /// <remarks>
    ///     Another way to monitor things added or removed is to add a thingComp to every qualified thingDef
    /// and take advantage of the PreAbsorbStack(), PostSplitoff() function, etc..
    ///     The correct initiate state for this clas is both Loadout and InventoryTracker are null. After moving
    /// to other states, none of them can be null.
    /// </remarks>
    public class CompAwesomeInventoryLoadout : ThingComp, IExposable
    {
        private Pawn _pawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompAwesomeInventoryLoadout"/> class.
        /// </summary>
        public CompAwesomeInventoryLoadout()
        {
        }

        /// <summary>
        /// Gets a dictionary acts as a inventory margin tracker. If the amount set in loadout is met, the margin is 0.
        /// Excessive amount has a positive margin, and vice versa.
        /// </summary>
        public Dictionary<ThingGroupSelector, int> InventoryMargins { get; private set; }

        /// <summary>
        /// Gets a <see cref="AwesomeInventoryLoadout"/> this comp holds.
        /// </summary>
        public AwesomeInventoryLoadout Loadout { get; private set; }

        /// <summary>
        /// Gets a value indicating whether loadout on this <see cref="Pawn"/> needs to restock.
        /// </summary>
        public bool NeedRestock
        {
            get
            {
                if (InventoryMargins == null)
                {
                    return false;
                }
#if DEBUG
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(((Pawn)parent).Name + "'s inventory");
                stringBuilder.AppendLine();
                foreach (var item in InventoryMargins)
                {
                    stringBuilder.AppendFormat(ErrorMessage.ReportString, nameof(NeedRestock), item.Key.LabelCapNoCount, item.Value);
                    stringBuilder.AppendLine();
                }

                Log.Warning(stringBuilder.ToString(), true);

                List<Thing> curInventory = MakeListForPawnGearAndInventory(_pawn);
                foreach (ThingGroupSelector groupSelector in Loadout)
                {
                    if (!InventoryMargins.ContainsKey(groupSelector))
                    {
                        string message
                            = string.Concat(
                                ErrorMessage.InvTrackerAndLoadoutOutOfSync
                                , "\n"
                                , string.Format(ErrorMessage.ExpectedString, groupSelector.LabelCapNoCount, groupSelector.AllowedStackCount, 0));
                        Log.ErrorOnce(message, Rand.Int, true);
                    }
                    else
                    {
                        int countToFetch = groupSelector.AllowedStackCount;
                        int expected = curInventory.Sum(t => groupSelector.Allows(t) ? t.stackCount : 0) - countToFetch;
                        if (InventoryMargins[groupSelector] != expected)
                        {
                            string message
                            = string.Concat(
                                ErrorMessage.InvTrackerAndLoadoutOutOfSync
                                , "\n"
                                , string.Format(ErrorMessage.ExpectedString, groupSelector.LabelCapNoCount, expected, InventoryMargins[groupSelector]));
                            Log.ErrorOnce(message, Rand.Int, true);
                        }
                    }
                }
#endif
                return InventoryMargins.Values.Any(m => m < 0);
            }
        }

        /// <summary>
        /// Gets a list of items that are needed to restock.
        /// </summary>
        public IEnumerable<ThingGroupSelector> ItemsToRestock
        {
            get
            {
                if (!NeedRestock)
                {
                    yield break;
                }

                foreach (var item in InventoryMargins)
                {
                    if (item.Value < 0)
                    {
                        yield return item.Key;
                    }
                }

                yield break;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _pawn = (Pawn)parent;
        }

        /// <summary>
        /// Called by the game root code post spawn setup.
        /// </summary>
        /// <param name="respawningAfterLoad"> True if the <see cref="ThingComp.parent"/> is respawned after load. </param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (parent is Pawn pawn)
            {
                if (pawn.outfits?.CurrentOutfit is AwesomeInventoryLoadout loadout)
                {
                    Loadout = loadout;
                }
            }
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> are added.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that is added. </param>
        public void NotifiedAdded(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            this.NotifiedThingChanged(thing, thing.stackCount, true);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> is added and merged.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has been added and merged. </param>
        /// <param name="mergedAmount"> Number of <paramref name="thing"/> that is added and merged. </param>
        public void NotifiedAddedAndMergedWith(Thing thing, int mergedAmount)
        {
            this.NotifiedThingChanged(thing, mergedAmount, true);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> has been removed.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has been removed. </param>
        public void NotifiedRemoved(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            this.NotifiedThingChanged(thing, thing.stackCount, false);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> has splitted off.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has splitted off. </param>
        /// <param name="count"> Number of splitted <paramref name="thing"/>. </param>
        public void NotifiedSplitOffHandler(Thing thing, int count)
        {
            this.NotifiedThingChanged(thing, count, false);
        }

        /// <summary>
        /// Update <see cref="CompAwesomeInventoryLoadout.InventoryMargins"/> whenever a new loadout is assigned to pawn.
        /// </summary>
        /// <param name="newLoadout"> The new loadout assigned to pawn. </param>
        public void UpdateForNewLoadout(AwesomeInventoryLoadout newLoadout)
        {
            if (newLoadout == null || this.Loadout == newLoadout)
                return;

            if (this.InventoryMargins == null)
                this.InventoryMargins = new Dictionary<ThingGroupSelector, int>();
            else
                this.InventoryMargins.Clear();

            List<Thing> curInventory = MakeListForPawnGearAndInventory(_pawn);

            foreach (ThingGroupSelector groupSelector in newLoadout)
            {
                this.UpdateInventoryMargin(groupSelector, curInventory);
            }

            newLoadout.AddAddNewThingGroupSelectorCallback(this.UpdateInventoryMargin);
            newLoadout.AddRemoveThingGroupSelectorCallback(this.RemoveThingGroupSelector);
            newLoadout.AddStackCountChangedCallback(this.UpdateInventoryMargin);

            Loadout = newLoadout;
        }

        /// <summary>
        /// Save state.
        /// </summary>
        public void ExposeData()
        {
            AwesomeInventoryLoadout loadout = this.Loadout;
            Scribe_References.Look(ref loadout, nameof(this.Loadout));

            List<ThingGroupSelector> things = new List<ThingGroupSelector>();
            List<int> margins = new List<int>();
            Dictionary<ThingGroupSelector, int> inventoryMargins = this.InventoryMargins;
            Scribe_Collections.Look(ref inventoryMargins, nameof(InventoryMargins), LookMode.Reference, LookMode.Value, ref things, ref margins);

            this.Loadout = loadout;
            this.InventoryMargins = inventoryMargins;
        }

        private static List<Thing> MakeListForPawnGearAndInventory(Pawn pawn)
        {
            List<Thing> things = new List<Thing>();
            things.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
            things.AddRange(pawn.apparel.WornApparel.Cast<Thing>());
            things.AddRange(pawn.inventory.innerContainer);

            return things;
        }

        private void NotifiedThingChanged(Thing thing, int count, bool isAdd)
        {
            if (thing == null || Loadout == null)
                return;

            foreach (KeyValuePair<ThingGroupSelector, int> pair in InventoryMargins)
            {
                if (pair.Key.Allows(thing))
                {
                    InventoryMargins[pair.Key] = pair.Value + count * (isAdd ? 1 : -1);
                }
            }
        }

        private void RemoveThingGroupSelector(ThingGroupSelector groupSelector) => this.InventoryMargins.Remove(groupSelector);

        private void UpdateInventoryMargin(ThingGroupSelector groupSelector, List<Thing> curInventory)
        {
            this.InventoryMargins[groupSelector] =
                    curInventory.Sum(t => groupSelector.Allows(t) ? t.stackCount : 0)
                    - groupSelector.AllowedStackCount;
        }

        private void UpdateInventoryMargin(ThingGroupSelector groupSelector)
        {
            this.UpdateInventoryMargin(groupSelector, MakeListForPawnGearAndInventory(_pawn));
        }
    }
}
