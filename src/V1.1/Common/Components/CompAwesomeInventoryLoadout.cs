// <copyright file="CompAwesomeInventoryLoadout.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class CompAwesomeInventoryLoadout : ThingComp
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
                        int expected = curInventory.Sum(t => groupSelector.Allows(t, out _) ? t.stackCount : 0) - countToFetch;
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

        /// <summary>
        /// Called by game code when the game starts.
        /// </summary>
        /// <param name="props"> Properties used for initializing this comp. </param>
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
                    this.UpdateForNewLoadout(loadout);
                    this.Loadout = loadout;
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

            this.Restock(thing);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> is added and merged.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has been added and merged. </param>
        /// <param name="mergedAmount"> Number of <paramref name="thing"/> that is added and merged. </param>
        public void NotifiedAddedAndMergedWith(Thing thing, int mergedAmount)
        {
            this.Restock(thing, mergedAmount);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> has been removed.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has been removed. </param>
        public void NotifiedRemoved(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            this.DeleteStock(thing);
        }

        /// <summary>
        /// Update internal tracking info when notified <paramref name="thing"/> has splitted off.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> that has splitted off. </param>
        /// <param name="count"> Number of splitted <paramref name="thing"/>. </param>
        public void NotifiedSplitOff(Thing thing, int count)
        {
            this.DeleteStock(thing, count);
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

            this.Loadout?.RemoveAddNewThingGroupSelectorCallback(this.AddNewThingGroupSelectorCallback);
            this.Loadout?.RemoveRemoveThingGroupSelectorCallback(this.RemoveThingGroupSelectorCallback);
            this.Loadout?.RemoveStackCountChangedCallback(this.StackCountChangedCallback);

            this.UpdateInventoryMargin(newLoadout);

            newLoadout.AddAddNewThingGroupSelectorCallback(this.AddNewThingGroupSelectorCallback);
            newLoadout.AddRemoveThingGroupSelectorCallback(this.RemoveThingGroupSelectorCallback);
            newLoadout.AddStackCountChangedCallback(this.StackCountChangedCallback);

            Loadout = newLoadout;
        }

        protected virtual void AddNewThingGroupSelectorCallback(ThingGroupSelector groupSelector)
        {
            this.UpdateInventoryMargin(
                this.InventoryMargins
                .Where(pair => ThingDefComparer.Instance.Equals(pair.Key.AllowedThing, groupSelector.AllowedThing))
                .Select(pair => pair.Key)
                .Append(groupSelector));
        }

        protected virtual void StackCountChangedCallback(ThingGroupSelector groupSelector, int oldStackCount)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            this.InventoryMargins[groupSelector] += oldStackCount - groupSelector.AllowedStackCount;
        }

        protected virtual void RemoveThingGroupSelectorCallback(ThingGroupSelector groupSelector) => this.InventoryMargins.Remove(groupSelector);

        protected virtual ThingGroupSelectorPool FindPotentialThingGroupSelectors(Thing thing, IEnumerable<ThingGroupSelector> groupSelectors)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            return this.FindPotentialThingGroupSelectors(thing, thing.stackCount, groupSelectors);
        }

        protected virtual ThingGroupSelectorPool FindPotentialThingGroupSelectors(Thing thing, int stackCount, IEnumerable<ThingGroupSelector> groupSelectors)
        {
            ThingGroupSelectorPool pool = new ThingGroupSelectorPool()
            {
                Thing = thing,
                StackCount = stackCount,
                OrderedSelectorTuples = new List<Tuple<ThingSelector, ThingGroupSelector>>(),
            };
            foreach (ThingGroupSelector groupSelector in groupSelectors)
            {
                if (groupSelector.Allows(thing, out ThingSelector thingSelector))
                    pool.OrderedSelectorTuples.Add(Tuple.Create(thingSelector, groupSelector));
            }

            if (pool.OrderedSelectorTuples.Count > 1)
                pool.OrderedSelectorTuples = pool.OrderedSelectorTuples.OrderBy(t => t.Item1, ThingSelectorComparer.Instance).ToList();

            return pool;
        }

        protected virtual void Restock(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            this.Restock(thing, thing.stackCount);
        }

        protected virtual void Restock(Thing thing, int reStockCount)
        {
            this.Restock(this.FindPotentialThingGroupSelectors(thing, reStockCount, this.InventoryMargins.Keys));
        }

        protected virtual void Restock(ThingGroupSelectorPool pool)
        {
            int restockCount = pool.StackCount;
            foreach (var tuple in pool.OrderedSelectorTuples)
            {
                if (this.InventoryMargins[tuple.Item2] + restockCount <= 0)
                {
                    this.InventoryMargins[tuple.Item2] += restockCount;
                    restockCount = 0;
                    break;
                }
                else
                {
                    restockCount += this.InventoryMargins[tuple.Item2];
                    this.InventoryMargins[tuple.Item2] = 0;
                }
            }

            if (restockCount != 0)
            {
                this.InventoryMargins[pool.OrderedSelectorTuples.First().Item2] += restockCount;
            }
        }

        protected virtual void DeleteStock(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            this.DeleteStock(thing, thing.stackCount);
        }

        private void DeleteStock(Thing thing, int stackCountToDelete)
        {
            ThingGroupSelectorPool pool = this.FindPotentialThingGroupSelectors(thing, stackCountToDelete, this.InventoryMargins.Keys);
            foreach (var tuple in pool.OrderedSelectorTuples)
            {
                int maxNegativeMargin = tuple.Item2.AllowedStackCount * -1;
                if (this.InventoryMargins[tuple.Item2] - pool.StackCount < maxNegativeMargin)
                {
                    pool.StackCount -= this.InventoryMargins[tuple.Item2] - maxNegativeMargin;
                    this.InventoryMargins[tuple.Item2] = maxNegativeMargin;
                }
                else
                {
                    this.InventoryMargins[tuple.Item2] -= pool.StackCount;
                    break;
                }
            }
        }

        private static List<Thing> MakeListForPawnGearAndInventory(Pawn pawn)
        {
            List<Thing> things = new List<Thing>();
            things.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
            things.AddRange(pawn.apparel.WornApparel.Cast<Thing>());
            things.AddRange(pawn.inventory.innerContainer);

            return things;
        }

        private void UpdateInventoryMargin(IEnumerable<ThingGroupSelector> groupSelectors)
        {
            ValidateArg.NotNull(groupSelectors, nameof(groupSelectors));

            this.InventoryMargins.Clear();
            foreach (ThingGroupSelector groupSelector in groupSelectors)
            {
                this.InventoryMargins[groupSelector] = groupSelector.AllowedStackCount * -1;
            }

            ConcurrentBag<ThingGroupSelectorPool> pools = new ConcurrentBag<ThingGroupSelectorPool>();
            Parallel.ForEach(
                Partitioner.Create(MakeListForPawnGearAndInventory(_pawn)),
                (Thing thing) =>
                {
                    ThingGroupSelectorPool pool = this.FindPotentialThingGroupSelectors(thing, groupSelectors);

                    if (pool.OrderedSelectorTuples.Any())
                        pools.Add(pool);
                });

            foreach (ThingGroupSelectorPool pool in pools)
            {
                this.Restock(pool);
            }
        }

        protected struct ThingGroupSelectorPool
        {
            public Thing Thing;
            public int StackCount;
            public List<Tuple<ThingSelector, ThingGroupSelector>> OrderedSelectorTuples;
        }
    }
}
