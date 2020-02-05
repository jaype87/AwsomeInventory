using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;

namespace RPG_Inventory_Remake.Loadout
{
    /// <summary>
    /// Save loudout information and its current state
    /// </summary>
    /// <remarks>
    ///     Another way to monitor things added or removed is to add a thingComp to every qualified thingDef
    /// and take advantage of the PreAbsorbStack(), PostSplitoff() function, etc..
    ///     The correct initiate state for this clas is both Loadout and InventoryTracker are null. After moving
    /// to other states, none of them can be null.
    /// </remarks>
    public partial class compRPGILoudout : ThingComp, IExposable
    {
        #region Fields

        public RPGILoadout Loadout = null;

        /// <summary>
        /// Value in this dictionary acts as a margin. If the amount set in loadout is met, the margin is 0.
        /// Excessive amount has a positive margin, and vice versa.
        /// </summary>
        public Dictionary<Thing, int> InventoryTracker = null;

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

        #region Constructor

        public compRPGILoudout()
        {
        }

        #endregion

        #region Methods

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (parent is Pawn pawn)
            {
                if (pawn.outfits?.CurrentOutfit is RPGILoadout loadout)
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
                if (pair.Key.def is LoadoutGenericDef genericDef)
                {
                    if (genericDef.Includes(thing.def))
                    {
                        InventoryTracker[pair.Key] = InventoryTracker[pair.Key] + count * (isAdd ? 1 : -1);
                    }
                }
            }
#if DEBUG
            RPGIDebug.DebugNotifiedMethod(parent as Pawn, thing, Loadout, InventoryTracker);
#endif
        }

        public void NotifiedAdded(Thing thing)
        {
            NotifiedThingChanged(thing, thing.stackCount, true);
        }

        public void NotifiedAddedAndMergedWith(Thing thing, int mergedAmount)
        {
            NotifiedThingChanged(thing, mergedAmount, true);
        }

        public void NotifiedRemoved(Thing thing)
        {
            NotifiedThingChanged(thing, thing.stackCount, false);
        }
        
        public void NotifiedSplitOff(Thing thing, int count)
        {
            NotifiedThingChanged(thing, count, false);
        }

        public void UpdateForNewLoadout(RPGILoadout newLoadout)
        {
            if (newLoadout == null)
            {
                return;
            }
            if (Loadout == null)
            {
                InventoryTracker = new Dictionary<Thing, int>(new LoadoutComparer<Thing>());
                foreach (Thing thing in newLoadout)
                {
                    InventoryTracker[thing] = thing.stackCount;
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
                foreach (Thing thing in newLoadout)
                {
                    if (Loadout.TryGetThing(thing.MakeThingStuffPairWithQuality(), out Thing oldThing))
                    {
                        InventoryTracker[thing] += (oldThing.stackCount - thing.stackCount);
                    }
                    else
                    {
                        InventoryTracker[thing] = thing.stackCount;
                    }
                }
            }
            Loadout = newLoadout;
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

    //public class CompProperties_RPGILoadout : CompProperties
    //{
    //    public CompProperties_RPGILoadout()
    //    {
    //        compClass = typeof(compRPGILoudout);
    //    }
    //}
}
