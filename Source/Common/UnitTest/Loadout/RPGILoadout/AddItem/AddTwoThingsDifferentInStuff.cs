using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPG_Inventory_Remake_Common.UnitTest;
using Verse;
using RimWorld;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class AddTwoThingsDifferentInStuff : Test_AddItem
    {
        public override void Setup()
        {
            loadoutInstance.AddItem(things[0]);
            loadoutInstance.AddItem(things[2]);
        }
        public override void Run(out bool result)
        {
            result = true;
            // Test if items are seperated
            result &=
                AssertUtility.Expect(loadoutInstance.Count(), 2, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)));
            // Test if CachedList is in sync
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList.Count, 2, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)));
            // Test if the instance of the item is shared between source and loadout
            result &=
                AssertUtility.AreEqual(things[0], loadoutInstance[things[0]].Thing, nameof(things), nameof(loadoutInstance));
            result &=
                AssertUtility.AreEqual(things[2], loadoutInstance[things[2]].Thing, nameof(things), nameof(loadoutInstance));
            // Ditto
            result &=
                AssertUtility.AreEqual(things[0], loadoutInstance.CachedList[0], nameof(things), nameof(loadoutInstance.CachedList));
            result &=
                AssertUtility.AreEqual(things[2], loadoutInstance.CachedList[1], nameof(things), nameof(loadoutInstance.CachedList));
            // Test whether the stackcount is correct
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList[0].stackCount, 1, string.Format(StringResource.ObjectCount, things[0].LabelCapNoCount));
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList[1].stackCount, 1, string.Format(StringResource.ObjectCount, things[2].LabelCapNoCount));
        }

        public override void Cleanup()
        {
            loadoutInstance.Clear();
        }
    }
}
