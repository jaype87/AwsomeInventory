// <copyright file="CompAwesomeInventoryLoadout.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
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
        #region Fields

        private Pawn _pawn;

        /// <summary>
        /// Value in this dictionary acts as a margin. If the amount set in loadout is met, the margin is 0.
        /// Excessive amount has a positive margin, and vice versa.
        /// </summary>
        public Dictionary<Thing, int> InventoryTracker = null;
        public AILoadout Loadout = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CompAwesomeInventoryLoadout"/> class.
        /// </summary>
        public CompAwesomeInventoryLoadout()
        {
        }

        #endregion

        #region Properties

        public bool NeedRestock
        {
            get
            {
                if (InventoryTracker == null)
                {
                    return false;
                }
#if DEBUG
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(((Pawn)parent).Name + "'s inventory");
                stringBuilder.AppendLine();
                foreach (var item in InventoryTracker)
                {
                    stringBuilder.AppendFormat(ErrorMessage.ReportString, nameof(NeedRestock), item.Key.LabelCap, item.Value);
                    stringBuilder.AppendLine();
                }
                Log.Warning(stringBuilder.ToString(), true);

                var curInventory = MakeLookupForPawnGearAndInventory(_pawn);
                foreach (Thing thing in Loadout)
                {
                    if (!InventoryTracker.ContainsKey(thing))
                    {
                        string message
                            = string.Concat(ErrorMessage.InvTrackerAndLoadoutOutOfSync
                                           , "\n"
                                           , string.Format(ErrorMessage.ExpectedString, thing.LabelCap, thing.stackCount, 0));
                        Log.ErrorOnce(message, Rand.Int, true);
                    }
                    else
                    {
                        int expected = (curInventory.TryGetValue(thing, out int count) ? count : 0) - Loadout[thing].Thing.stackCount;
                        if (InventoryTracker[thing] != expected)
                        {
                            string message
                            = string.Concat(ErrorMessage.InvTrackerAndLoadoutOutOfSync
                                            , "\n"
                                            , string.Format(ErrorMessage.ExpectedString, thing.LabelCap, expected, InventoryTracker[thing]));
                            Log.ErrorOnce(message, Rand.Int, true);
                        }
                    }
                }
#endif
                return InventoryTracker.Values.Any(m => m < 0);
            }
        }

        public IEnumerable<Thing> ItemsToRestock
        {
            get
            {
                if (!NeedRestock)
                {
                    yield break;
                }
                foreach (var item in InventoryTracker)
                {
                    if (item.Value < 0)
                    {
                        item.Key.stackCount = item.Value;
                        yield return item.Key;
                    }
                }
                yield break;
            }
        }

        #endregion

        #region Methods

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _pawn = (Pawn)parent;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (parent is Pawn pawn)
            {
                if (pawn.outfits?.CurrentOutfit is AILoadout loadout)
                {
                    Loadout = loadout;
                }
            }
        }

        private void NotifiedThingChanged(Thing thing, int count, bool isAdd)
        {
            if (thing == null || Loadout == null || !InventoryTracker.ContainsKey(thing))
            {
                return;
            }

            if (InventoryTracker.ContainsKey(thing))
            {
                InventoryTracker[thing] = InventoryTracker[thing] + count * (isAdd ? 1 : -1);
            }

            foreach (var pair in InventoryTracker)
            {
                if (pair.Key.def is AIGenericDef genericDef)
                {
                    if (genericDef.Includes(thing.def))
                    {
                        InventoryTracker[pair.Key] = InventoryTracker[pair.Key] + count * (isAdd ? 1 : -1);
                    }
                }
            }
        }

        public void NotifiedAdded(Thing thing)
        {
            this.NotifiedThingChanged(thing, thing.stackCount, true);
        }

        public void NotifiedAddedAndMergedWith(Thing thing, int mergedAmount)
        {
            this.NotifiedThingChanged(thing, mergedAmount, true);
        }

        public void NotifiedRemoved(Thing thing)
        {
            this.NotifiedThingChanged(thing, thing.stackCount, false);
        }

        public void NotifiedSplitOff(Thing thing, int count)
        {
            this.NotifiedThingChanged(thing, count, false);
        }

        public void UpdateForNewLoadout(AILoadout newLoadout)
        {
            if (newLoadout == null)
            {
                return;
            }

            if (Loadout == null)
            {
                InventoryTracker = new Dictionary<Thing, int>(new LoadoutComparer<Thing>());
                Dictionary<Thing, int> curInventory = MakeLookupForPawnGearAndInventory(_pawn);

                foreach (Thing thing in newLoadout)
                {
                    if (curInventory.TryGetValue(thing, out int curStack))
                    {
                        InventoryTracker[thing] = curStack - thing.stackCount;
                    }
                    else
                    {
                        InventoryTracker[thing] = -thing.stackCount;
                    }
                }
            }
            else if (Loadout == newLoadout)
            {
                return;
            }
            else
            {
                // Remove deleted items
                foreach (Thing thing in InventoryTracker.Keys.ToList())
                {
                    if (!newLoadout.Contains(thing))
                    {
                        InventoryTracker.Remove(thing);
                    }
                }

                // Add new items or updated the old ones
                Dictionary<Thing, int> curInventory = MakeLookupForPawnGearAndInventory(_pawn);
                foreach (Thing thing in newLoadout)
                {
                    if (Loadout.TryGetThing(thing.MakeThingStuffPairWithQuality(), out Thing oldThing))
                    {
                        InventoryTracker[thing] += (oldThing.stackCount - thing.stackCount);
                    }
                    else
                    {
                        if (curInventory.TryGetValue(thing, out int curStack))
                        {
                            InventoryTracker[thing] = curStack - thing.stackCount;
                        }

                        InventoryTracker[thing] = -thing.stackCount;
                    }
                }

                Loadout.CallbacksOnAddOrRemove.Remove(UpdateInventoryTracker);
            }

            newLoadout.CallbacksOnAddOrRemove.Add(UpdateInventoryTracker);
            Loadout = newLoadout;
        }

        private void UpdateInventoryTracker(Thing thing, bool removed)
        {
            if (removed)
            {
                InventoryTracker.Remove(thing);
            }
            else
            {
                if (InventoryTracker.ContainsKey(thing))
                {
                    InventoryTracker[thing] -= thing.stackCount;
                }
                else
                {
                    InventoryTracker[thing] = -thing.stackCount;
                }
            }
        }

        private static Dictionary<Thing, int> MakeLookupForPawnGearAndInventory(Pawn pawn)
        {
            List<Thing> things = new List<Thing>();
            things.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
            things.AddRange(pawn.apparel.WornApparel.Cast<Thing>());
            things.AddRange(pawn.inventory.innerContainer);
            return things.ToDictionary(
                (Thing thing) => thing
                , (Thing thing) => thing.stackCount
                , new LoadoutComparer<Thing>());
        }

        /// <summary>
        /// Determines if and how many of an item currently fit into the inventory with regards to weight constraints.
        /// </summary>
        /// <param name="thing">Thing to check</param>
        /// <param name="count">Maximum amount of the thing, first param, that can fit into the inventory</param>
        /// <param name="ignoreEquipment">Whether to include currently equipped weapons when calculating current weight/bulk</param>
        /// <returns>True if one or more items fit into the inventory</returns>
        public bool CanFitInInventory(Thing thing, out int count, bool ignoreEquipment = false)
        {
            if (!(parent is Pawn pawn))
            {
                count = 0;
                return false;
            }
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }

            float thingWeight;

            thingWeight = thing.GetStatValue(StatDefOf.Mass);

            // Subtract weight of currently equipped weapon
            float eqWeight = 0f;
            if (ignoreEquipment && pawn?.equipment?.Primary != null)
            {
                eqWeight = pawn.equipment.Primary.GetStatValue(StatDefOf.Mass);
            }

            // Calculate how many items we can fit into our inventory
            float amountByWeight = thingWeight <= 0 ? thing.stackCount : (MassUtility.FreeSpace(pawn) + eqWeight) / thingWeight;
            count = Mathf.FloorToInt(Mathf.Min(amountByWeight, thing.stackCount));
            return count > 0;
        }

        public void ExposeData()
        {
            List<Thing> things = new List<Thing>();
            List<int> margins = new List<int>();
            Scribe_References.Look(ref Loadout, nameof(Loadout));
            Scribe_Collections.Look(ref InventoryTracker, nameof(InventoryTracker), LookMode.Reference, LookMode.Value, ref things, ref margins);
        }

        #endregion Methods
    }
}
