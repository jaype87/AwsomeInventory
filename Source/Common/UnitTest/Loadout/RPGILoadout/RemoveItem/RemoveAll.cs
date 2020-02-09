#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RPG_Inventory_Remake.Loadout;

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class RemoveAll : Test_RemoveItem
    {
        public override void Setup()
        {
            loadoutInstance.Clear();
        }

        public override void Run(out bool result)
        {
            result = true;

            result &=
                    // Check count
                    AssertUtility.Expect(loadoutInstance.Count, 0, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)))
                    &&
                    AssertUtility.Expect(loadoutInstance.CachedList.Count, 0, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)))
                    ;
        }

        public override void Cleanup()
        {
            foreach(Thing thing in things)
            {
                loadoutInstance.Add(thing);
            }
        }
    }
}
#endif