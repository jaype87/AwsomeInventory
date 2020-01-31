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
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return LoadoutComparer.GetHashCodeFromComparer(x) == LoadoutComparer.GetHashCodeFromComparer(y);
        }

        public override int GetHashCode(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(string.Empty);
            }
            return string.Concat
                (obj.def.defName
                , obj.Stuff?.defName ?? string.Empty
                , obj.TryGetQuality(out QualityCategory qc) ? qc.ToString() : string.Empty
                ).GetHashCode();
        }
    }

    public static class LoadoutComparer
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public static bool isEqual(Thing x, Thing y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return GetHashCodeFromComparer(x) == GetHashCodeFromComparer(y);
        }

        public static int GetHashCodeFromComparer(Thing obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(string.Empty);
            }
            return string.Concat
                (obj.def.defName
                , obj.Stuff?.defName ?? string.Empty
                , obj.TryGetQuality(out QualityCategory qc) ? qc.ToString() : string.Empty
                ).GetHashCode();
        }
    }
}
