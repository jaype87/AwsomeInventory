#if UnitTest
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Diagnostics;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake;
#endif

namespace RPG_Inventory_Remake_Common.UnitTest
{
    public class AddUnmergeableItemTwice : Test_AddItem
    {
        public override void Setup()
        {
            loadoutInstance.AddItem(things[0]);
            loadoutInstance.AddItem(things[0]);
        }

        public override void Run(out bool result)
        {
            result = true;
            // Test if items are merged. Even it has a stack limit of 1, it doesn't matter in loadout
            result &=
                AssertUtility.Expect(loadoutInstance.Count(), 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance)));
            // Test if CachedList is in sync
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList.Count, 1, string.Format(StringResource.ObjectCount, nameof(loadoutInstance.CachedList)));
            // Test if the instance of the item is shared between source and loadout
            result &=
                AssertUtility.AreEqual(things[0], loadoutInstance[things[0]].Thing, nameof(things), nameof(loadoutInstance));
            // Ditto
            result &=
                AssertUtility.AreEqual(things[0], loadoutInstance.CachedList[0], nameof(things), nameof(loadoutInstance.CachedList));
            // Test whether the stackcount is correct
            result &=
                AssertUtility.Expect(loadoutInstance.CachedList[0].stackCount, 2, string.Format(StringResource.ObjectCount, things[0].LabelCapNoCount));
        }

        public override void Cleanup()
        {
            loadoutInstance.Clear();
        }
    }
}
#endif