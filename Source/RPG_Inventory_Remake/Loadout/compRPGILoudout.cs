using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace RPG_Inventory_Remake.RPGILoadout
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

        public LoadoutManager LoadoutManager;
        public RPGILoadout<Thing> Loadout;
        /// <summary>
        /// Value in this dictionary acts as a margin. If the amount set in loadout is met, the margin is 0.
        /// Excessive amount has a positive margin, vice versa.
        /// </summary>
        public Dictionary<Thing, int> InventoryTracker;

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

        public compRPGILoudout(RPGILoadout<Thing> loadout)
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

        public void UpdateForNewLoadout(RPGILoadout<Thing> newloadout)
        {
            if (newloadout == null)
            {
                return;
            }

            foreach (Thing thing in newloadout)
            {
                if (Loadout.TryGetValue(thing, out Thing oldLoadout))
                {
                    InventoryTracker[thing] += (oldLoadout.stackCount - thing.stackCount);
                }
                else
                {
                    InventoryTracker[thing] = thing.stackCount;
                }
            }
            Loadout = newloadout;
        }

        #endregion Methods
    }
}
