using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake.Loadout
{
    public class GameComponent_DefManager : GameComponent
    {
        private static HashSet<ThingDef> _allSuitableDefs;

        public GameComponent_DefManager(Game game)
        {

        }

        public override void FinalizeInit()
        {
            List<ThingDef> allSuitableDefs;
            allSuitableDefs = DefDatabase<ThingDef>
                                .AllDefsListForReading
                                .Where(thingDef => (thingDef.EverHaulable || thingDef.Minifiable)
                                                && !thingDef.menuHidden
                                                && IsSuitableThingDef(thingDef))
                                .ToList();
            _allSuitableDefs = new HashSet<ThingDef>(allSuitableDefs, new CompareThingDef());
            _allSuitableDefs.AddRange(LoadoutGenericDef.GenericDefsToThingDefs);
        }

        /// <summary>
        /// Return a copy of a cached AllSuitableDefs
        /// </summary>
        /// <returns></returns>
        public static HashSet<ThingDef> GetSuitableDefs()
        {
            return new HashSet<ThingDef>(_allSuitableDefs.AsEnumerable(), _allSuitableDefs.Comparer);
        }

        private bool IsSuitableThingDef(ThingDef td)
        {
            return td.Minifiable
                    || (td.thingClass != typeof(Corpse)
                        && !td.IsFrame
                        && !td.destroyOnDrop);
        }

        private class CompareThingDef : EqualityComparer<ThingDef>
        {
            public override bool Equals(ThingDef x, ThingDef y)
            {
                return x.defName == y.defName;
            }

            public override int GetHashCode(ThingDef obj)
            {
                return obj.defName.GetHashCode();
            }
        }
    }
}
