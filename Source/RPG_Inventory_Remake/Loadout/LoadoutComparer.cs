using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake.RPGILoadout
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
            return x.GetHashCode() == y.GetHashCode();
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
}
