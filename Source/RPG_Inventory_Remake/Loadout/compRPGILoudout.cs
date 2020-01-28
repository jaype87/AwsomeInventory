using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace RPG_Inventory_Remake.Loadout
{
    /// <summary>
    /// Save loudout information and its current state
    /// </summary>
    /// <remarks>
    /// Another way to monitor things added or removed is to add a thingComp to every qualified thingDef
    /// and take advantage of the PreAbsorbStack(), PostSplitoff() function, etc..
    /// </remarks>
    public class compRPGILoudout : ThingComp
    {
        #region Fields

        public RPGILoadout Loadout = null;
        /// <summary>
        /// Value in this dictionary acts as a margin. If the amount set in loadout is met, the margin is 0.
        /// Excessive amount has a positive margin, vice versa.
        /// </summary>
        public Dictionary<Thing, int> InventoryTracker = new Dictionary<Thing, int>();

        #endregion

        #region Properties

        public bool NeedRefill
        {
            get
            {
                return InventoryTracker.Values.Any(m => m < 0);
            }
        }

        public IEnumerable<Thing> ItemsToRefill
        {
            get
            {
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

        public compRPGILoudout(RPGILoadout loadout)
        {
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            InventoryTracker = new Dictionary<Thing, int>(new LoadoutComparer<Thing>());
            foreach (Thing thing in loadout)
            {
                InventoryTracker.Add(thing, thing.stackCount);
            }
        }

        #endregion

        #region Methods

        public void NotifiedAdded(Thing thing)
        {
            if (thing == null || !InventoryTracker.ContainsKey(thing))
            {
                return;
            }
            InventoryTracker[thing] += thing.stackCount;
        }

        public void NotifiedAddedAndMergedWith(Thing thing, int mergedAmount)
        {
            if (thing == null || !InventoryTracker.ContainsKey(thing))
            {
                return;
            }
            InventoryTracker[thing] += mergedAmount;
        }

        public void NotifiedRemoved(Thing thing)
        {
            if (thing == null || !InventoryTracker.ContainsKey(thing))
            {
                return;
            }
            InventoryTracker.Remove(thing);
        }

        public void UpdateForNewLoadout(RPGILoadout newLoadout)
        {
            if (newLoadout == null)
            {
                return;
            }

            if (Loadout == null)
            {
                Init();
                foreach (Thing thing in newLoadout)
                {
                    InventoryTracker[thing] = thing.stackCount;
                }
            }
            else
            {
                // Remove deleted items
                foreach (Thing thing in InventoryTracker.Keys)
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

        public void Init()
        {
            InventoryTracker = new Dictionary<Thing, int>(new LoadoutComparer<Thing>());
        }

        #endregion Methods
    }
}
