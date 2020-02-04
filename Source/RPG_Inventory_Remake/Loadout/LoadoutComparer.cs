using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Diagnostics.CodeAnalysis;

namespace RPG_Inventory_Remake.Loadout
{
    public class LoadoutComparer<T> : EqualityComparer<T> where T : Thing
    {
        public override bool Equals(T x, T y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            ThingStuffPairWithQuality pairX = x.MakeThingStuffPairWithQuality();
            ThingStuffPairWithQuality pairY = y.MakeThingStuffPairWithQuality();
            return pairX == pairY;
        }

        public override int GetHashCode(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(string.Empty);
            }
            return obj.MakeThingStuffPairWithQuality().GetHashCode();
        }
    }
}
